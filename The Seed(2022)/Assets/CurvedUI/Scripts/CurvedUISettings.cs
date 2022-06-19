

#define CURVEDUI_PRESENT //If you're an asset creator and want to see if CurvedUI is imported, just use "#if CURVEDUI_PRESENT [your code] #endif"
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

#if CURVEDUI_TMP || TMP_PRESENT
using TMPro;
#endif 


/// <summary>
/// This class stores settings for the entire canvas. It also stores useful methods for converting cooridinates to and from 2d canvas to curved canvas, or world space.
/// CurvedUIVertexEffect components (added to every canvas gameobject)ask this class for per-canvas settings when applying their curve effect.
/// </summary>

namespace CurvedUI
{
	[AddComponentMenu("CurvedUI/CurvedUISettings")]
    [RequireComponent(typeof(Canvas))]
    public class CurvedUISettings : MonoBehaviour
    {
        public const string Version = "3.2";

        #region SETTINGS
        //Global settings
        [SerializeField]
        CurvedUIShape shape;

        [SerializeField]
        float quality = 1f;
        [SerializeField]
        bool interactable = true;
		[SerializeField]
		bool blocksRaycasts = true;
        [SerializeField]
        bool forceUseBoxCollider = false;

        //Cyllinder settings
        [SerializeField]
        int angle = 90;
        [SerializeField]
        bool preserveAspect = true;

        //Sphere settings
        [SerializeField]
        int vertAngle = 90;

        //ring settings
        [SerializeField]
        float ringFill = 0.5f;
        [SerializeField]
        int ringExternalDiamater = 1000;
        [SerializeField]
        bool ringFlipVertical = false;


        //internal system settings
        int baseCircleSegments = 16;


        //stored variables
        Vector2 savedRectSize;
        float savedRadius;
        Canvas myCanvas;
        RectTransform m_rectTransform;

#endregion


#region LIFECYCLE

        void Awake()
        {
            // If this canvas is on Default layer, switch it to UI layer..
            // this is to make sure that when using raycasting to detect interactions, 
            // nothing will interfere with it.
            if (gameObject.layer == 0) this.gameObject.layer = 5;

            //save initial variables
            savedRectSize = RectTransform.rect.size;
        }

        void Start()
        {
            if (Application.isPlaying)
            {    
                // lets get rid of any raycasters and add our custom one
                // It will be responsible for handling interactions.
                BaseRaycaster[] raycasters = GetComponents<BaseRaycaster>();
                foreach(BaseRaycaster caster in raycasters)
                {
                    if (!(caster is CurvedUIRaycaster))
                        caster.enabled = false;
                }
                this.gameObject.AddComponentIfMissing<CurvedUIRaycaster>();

                //find if there are any child canvases that may break interactions
                Canvas[] canvases = GetComponentsInChildren<Canvas>();
                foreach(Canvas cnv in canvases)
                {
                    if (cnv.gameObject != this.gameObject)
                    {
                        Transform trans = cnv.transform;
                        string hierarchyName = trans.name;
                       
                        while(trans.parent != null)
                        {
                            hierarchyName = trans.parent.name + "/" + hierarchyName;
                            trans = trans.parent;
                        }
                        Debug.LogWarning("CURVEDUI: Interactions on nested canvases are not supported. You won't be able to interact with any child object of [" + hierarchyName + "]", cnv.gameObject);
                    }
                }
            }

            //find needed references
            if (myCanvas == null)
                myCanvas = GetComponent<Canvas>();

            savedRadius = GetCyllinderRadiusInCanvasSpace();
        }


        void OnEnable()
        {
            //Redraw canvas object on enable.
            foreach (UnityEngine.UI.Graphic graph in (this).GetComponentsInChildren<UnityEngine.UI.Graphic>())
            {
                graph.SetAllDirty();
            }
        }

        void OnDisable()
        {
            foreach (UnityEngine.UI.Graphic graph in (this).GetComponentsInChildren<UnityEngine.UI.Graphic>())
            {
                graph.SetAllDirty();
            }
        }

        void Update()
        {

            //recreate the geometry if entire canvas has been resized
            if (RectTransform.rect.size != savedRectSize)
            {
                savedRectSize = RectTransform.rect.size;
                SetUIAngle(angle);
            }

            //check for improper canvas size
            if (savedRectSize.x == 0 || savedRectSize.y == 0)
                Debug.LogError("CurvedUI: Your Canvas size must be bigger than 0!");
        }
        #endregion


#region PRIVATE

        /// <summary>
        /// Changes the horizontal angle of the canvas.
        /// </summary>
        /// <param name="newAngle"></param>
        void SetUIAngle(int newAngle)  {

            if (myCanvas == null)
                myCanvas = GetComponent<Canvas>();

            //temp fix to make interactions with angle 0 possible
            if (newAngle == 0) newAngle = 1;

            angle = newAngle;

            savedRadius = GetCyllinderRadiusInCanvasSpace();

            foreach (CurvedUIVertexEffect ve in GetComponentsInChildren<CurvedUIVertexEffect>())
                ve.SetDirty(); 

            foreach (Graphic graph in GetComponentsInChildren<Graphic>())
                graph.SetAllDirty();

            if (Application.isPlaying && GetComponent<CurvedUIRaycaster>() != null)
                //tell raycaster to update its collider now that angle has changed.
                GetComponent<CurvedUIRaycaster>().RebuildCollider();
         }

        Vector3 CanvasToCyllinder(Vector3 pos)
        {
            float theta = (pos.x / savedRectSize.x) * Angle * Mathf.Deg2Rad;
            pos.x = Mathf.Sin(theta) * (SavedRadius + pos.z);
            pos.z += Mathf.Cos(theta) * (SavedRadius + pos.z) - (SavedRadius + pos.z);

            return pos;
        }

        Vector3 CanvasToCyllinderVertical(Vector3 pos)
        {
            float theta = (pos.y / savedRectSize.y) * Angle * Mathf.Deg2Rad;
            pos.y = Mathf.Sin(theta) * (SavedRadius + pos.z);
            pos.z += Mathf.Cos(theta) * (SavedRadius + pos.z) - (SavedRadius + pos.z);

            return pos;
        }

        Vector3 CanvasToRing(Vector3 pos)
        {
            float r = pos.y.Remap(savedRectSize.y * 0.5f * (RingFlipVertical ? 1 : -1), -savedRectSize.y * 0.5f * (RingFlipVertical ? 1 : -1), RingExternalDiameter * (1 - RingFill) * 0.5f, RingExternalDiameter * 0.5f);
            float theta = (pos.x / savedRectSize.x).Remap(-0.5f, 0.5f, Mathf.PI / 2.0f, angle * Mathf.Deg2Rad + Mathf.PI / 2.0f);
            pos.x = r * Mathf.Cos(theta);
            pos.y = r * Mathf.Sin(theta);

            return pos;
        }

        Vector3 CanvasToSphere(Vector3 pos)
        {
            float radius = SavedRadius;
            float vAngle = VerticalAngle;

            if (PreserveAspect)
            {
                vAngle = angle * (savedRectSize.y / savedRectSize.x);
                radius += Angle > 0 ? -pos.z : pos.z;
            }
            else {
                radius = savedRectSize.x / 2.0f + pos.z;
                if (vAngle == 0) return Vector3.zero;
            }

            //convert planar coordinates to spherical coordinates
            float theta = (pos.x / savedRectSize.x).Remap(-0.5f, 0.5f, (180 - angle) / 2.0f - 90, 180 - (180 - angle) / 2.0f - 90);
            theta *= Mathf.Deg2Rad;
            float gamma = (pos.y / savedRectSize.y).Remap(-0.5f, 0.5f, (180 - vAngle) / 2.0f, 180 - (180 - vAngle) / 2.0f);
            gamma *= Mathf.Deg2Rad;

            pos.z = Mathf.Sin(gamma) * Mathf.Cos(theta) * radius;
            pos.y = -radius * Mathf.Cos(gamma);
            pos.x = Mathf.Sin(gamma) * Mathf.Sin(theta) * radius;

            if (PreserveAspect)
                pos.z -= radius;

            return pos;
        }
#endregion



#region PUBLIC

        RectTransform RectTransform {
            get
            {
                if (m_rectTransform == null) m_rectTransform = transform as RectTransform;
                return m_rectTransform;
            }
        }

        /// <summary>
        /// Adds the CurvedUIVertexEffect component to every child gameobject that requires it. 
        /// CurvedUIVertexEffect creates the curving effect.
        /// </summary>
        public void AddEffectToChildren()
        {
            foreach (UnityEngine.UI.Graphic graph in GetComponentsInChildren<UnityEngine.UI.Graphic>(true))
            {
                if (graph.GetComponent<CurvedUIVertexEffect>() == null)
                {
                    graph.gameObject.AddComponent<CurvedUIVertexEffect>();
                    graph.SetAllDirty();
                }
            }

            //additional script that will create a curved caret for input fields
            foreach(UnityEngine.UI.InputField iField in GetComponentsInChildren<UnityEngine.UI.InputField>(true))
            {
                if (iField.GetComponent<CurvedUIInputFieldCaret>() == null)
                {
                    iField.gameObject.AddComponent<CurvedUIInputFieldCaret>();
                }
            }

            //TextMeshPro experimental support. Go to CurvedUITMP.cs to learn how to enable it.
#if CURVEDUI_TMP || TMP_PRESENT
		    foreach(TextMeshProUGUI tmp in GetComponentsInChildren<TextMeshProUGUI>(true)){
			    if(tmp.GetComponent<CurvedUITMP>() == null){
				    tmp.gameObject.AddComponent<CurvedUITMP>();
				    tmp.SetAllDirty();
			    }
		    }

            foreach (TMP_InputField tmp in GetComponentsInChildren<TMP_InputField>(true))
            {
                tmp.AddComponentIfMissing<CurvedUITMPInputFieldCaret>();
            }
#endif
        }

        /// <summary>
        /// Maps a world space vector to a curved canvas. 
        /// Operates in Canvas's local space.
        /// </summary>
        /// <param name="pos">World space vector</param>
        /// <returns>
        /// A vector on curved canvas in canvas's local space
        /// </returns>
        public Vector3 VertexPositionToCurvedCanvas(Vector3 pos)
        {
            switch (Shape)
            {
                case CurvedUIShape.CYLINDER:
                {
                    return CanvasToCyllinder(pos);
                }
                case CurvedUIShape.CYLINDER_VERTICAL:
                {
                    return CanvasToCyllinderVertical(pos);
                }
                case CurvedUIShape.RING:
                {
                    return CanvasToRing(pos);
                }
                case CurvedUIShape.SPHERE:
                {
                    return CanvasToSphere(pos);
                }
                default:
                {
                    return Vector3.zero;
                }
            }

        }

        /// <summary>
        /// Converts a point in Canvas space to a point on Curved surface in world space units. 
        /// </summary>
        /// <param name="pos">Position on canvas in canvas space</param>
        /// <returns>
        /// Position on curved canvas in world space.
        /// </returns>
        public Vector3 CanvasToCurvedCanvas(Vector3 pos)
        {
            pos = VertexPositionToCurvedCanvas(pos);
            if (float.IsNaN(pos.x) || float.IsInfinity(pos.x)) return Vector3.zero;
            else return transform.localToWorldMatrix.MultiplyPoint3x4(pos);
        }

        /// <summary>
        /// Returns a normal direction on curved canvas for a given point on flat canvas. Works in canvas' local space.
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public Vector3 CanvasToCurvedCanvasNormal(Vector3 pos)
        {
            //find the position in canvas space
            pos = VertexPositionToCurvedCanvas(pos);

            switch (Shape)
            {
                case CurvedUIShape.CYLINDER:
                {
                    // find the direction to the center of cyllinder on flat XZ plane
                    return transform.localToWorldMatrix.MultiplyVector((pos - new Vector3(0, 0, -GetCyllinderRadiusInCanvasSpace())).ModifyY(0)).normalized;
                }
                case CurvedUIShape.CYLINDER_VERTICAL:
                {
                    // find the direction to the center of cyllinder on flat YZ plane
                    return transform.localToWorldMatrix.MultiplyVector((pos - new Vector3(0, 0, -GetCyllinderRadiusInCanvasSpace())).ModifyX(0)).normalized;
                }
                case CurvedUIShape.RING:
                {
                    // just return the back direction of the canvas
                    return -transform.forward;
                }
                case CurvedUIShape.SPHERE:
                {
                    //return the direction towards the sphere's center
                    Vector3 center = (PreserveAspect ? new Vector3(0, 0, -GetCyllinderRadiusInCanvasSpace()) : Vector3.zero);
                    return transform.localToWorldMatrix.MultiplyVector((pos - center)).normalized;
                }
                default:
                {
                    return Vector3.zero;
                }
            }
        }

        /// <summary>
        /// Raycasts along the given ray and returns the intersection with the flat canvas. 
        /// Use to convert from world space to flat canvas space. 
        /// </summary>
        /// <param name="ray"></param>
        /// <returns>
        /// Returns the true if ray hits the CurvedCanvas.
        /// Outputs intersection point in flat canvas space. 
        /// </returns>
        public bool RaycastToCanvasSpace(Ray ray, out Vector2 o_positionOnCanvas)
        {
            CurvedUIRaycaster caster = this.GetComponent<CurvedUIRaycaster>();
            o_positionOnCanvas = Vector2.zero;

            switch (Shape)
            {
                case CurvedUISettings.CurvedUIShape.CYLINDER:
                {
                    return caster.RaycastToCyllinderCanvas(ray, out o_positionOnCanvas, true);
                }
                case CurvedUISettings.CurvedUIShape.CYLINDER_VERTICAL:
                {
                    return caster.RaycastToCyllinderVerticalCanvas(ray, out o_positionOnCanvas, true);
                }
                case CurvedUISettings.CurvedUIShape.RING:
                {
                    return caster.RaycastToRingCanvas(ray, out o_positionOnCanvas, true);
                }
                case CurvedUISettings.CurvedUIShape.SPHERE:
                {
                    return caster.RaycastToSphereCanvas(ray, out o_positionOnCanvas, true);
                }
                default:
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Returns the radius of curved canvas cyllinder, expressed in Cavas's local space units.
        /// </summary>
        public float GetCyllinderRadiusInCanvasSpace()
        {
            float ret;
            if (PreserveAspect)
            {
                if(shape == CurvedUIShape.CYLINDER_VERTICAL)
                  ret = (RectTransform.rect.size.y / ((2 * Mathf.PI) * (angle / 360.0f)));
                else
                  ret = (RectTransform.rect.size.x / ((2 * Mathf.PI) * (angle / 360.0f)));

            }
            else
                ret = (RectTransform.rect.size.x * 0.5f) / Mathf.Sin(Mathf.Clamp(angle, -180.0f, 180.0f) * 0.5f * Mathf.Deg2Rad);

            return angle == 0 ? 0 : ret;
        }

        /// <summary>
        /// Tells you how big UI quads can get before they should be tesselate to look good on current canvas settings.
        /// Used by CurvedUIVertexEffect to determine how many quads need to be created for each graphic.
        /// </summary>
        public Vector2 GetTesslationSize(bool modifiedByQuality = true)
        {
            Vector2 ret = RectTransform.rect.size;
            if (Angle != 0 || (!PreserveAspect && vertAngle != 0))
            {
                switch (shape)
                {
                    case CurvedUIShape.CYLINDER: ret /= GetSegmentsByAngle(angle); break;
                    case CurvedUIShape.CYLINDER_VERTICAL: goto case CurvedUIShape.CYLINDER;
                    case CurvedUIShape.RING: goto case CurvedUIShape.CYLINDER; 
                    case CurvedUIShape.SPHERE: 
                    {
                        ret.x /= GetSegmentsByAngle(angle);

                        if (PreserveAspect)
                            ret.y = ret.x * RectTransform.rect.size.y / RectTransform.rect.size.x;
                        else
                            ret.y /= GetSegmentsByAngle(VerticalAngle);
                        break;
                    }
                }
            }
            //Debug.Log(this.gameObject.name + " returning size " + ret + " which is " + ret * this.transform.localScale.x + " in world space.", this.gameObject);
            return ret / (modifiedByQuality ? Mathf.Clamp(Quality, 0.01f, 10.0f) : 1);
        }

        float GetSegmentsByAngle(float angle)
        {
            if (angle.Abs() <= 1)
                return 1;
            else if (angle.Abs() < 90)//proportionaly twice as many segments for small angle canvases
                return baseCircleSegments * (Mathf.Abs(angle).Remap(0, 90, 0.01f, 0.5f));
            else
                return baseCircleSegments * (Mathf.Abs(angle).Remap(90, 360.0f, 0.5f, 1));

        }

        /// <summary>
        /// Tells you how many segmetens should the entire 360 deg. cyllinder or sphere consist of.
        /// Used by CurvedUIVertexEffect
        /// </summary>
        public int BaseCircleSegments {
            get  {  return baseCircleSegments; }
        }

        /// <summary>
        /// The measure of the arc of the Canvas.
        /// </summary>
        public int Angle {
            get { return angle; }
            set {
                if (angle != value)
                    SetUIAngle(value);
            }
        }

        /// <summary>
        /// Multiplier used to deremine how many segments a base curve of a shape has. 
        /// Default 1. Lower values greatly increase performance. Higher values give you sharper curve.
        /// </summary>
        public float Quality {
            get { return quality; }
            set {
                if (quality != value)
                {
                    quality = value;
                    SetUIAngle(angle);
                }
            }
        }

        /// <summary>
        /// Current Shape of the canvas
        /// </summary>
        public CurvedUIShape Shape {
            get { return shape; }
            set  {
                if (shape != value)
                {
                    shape = value;
                    SetUIAngle(angle);
                }
            }
        }

        /// <summary>
        /// Vertical angle of the canvas. Used in sphere shape and ring shape.
        /// </summary>
        public int VerticalAngle {
            get { return vertAngle; }
            set  {
                if (vertAngle != value)
                {
                    vertAngle = value;
                    SetUIAngle(angle);
                }
            }
        }

        /// <summary>
        /// Fill of a ring in ring shaped canvas. 0-1
        /// </summary>
        public float RingFill {
            get { return ringFill; }
            set {
                if (ringFill != value)
                {
                    ringFill = value;
                    SetUIAngle(angle);
                }
            }
        }

        /// <summary>
        /// Calculated radius of the curved canvas. 
        /// </summary>
        public float SavedRadius {
            get {
                if (savedRadius == 0)
                    savedRadius = GetCyllinderRadiusInCanvasSpace();

                return savedRadius;
            }
        }

        /// <summary>
        /// External diameter of the ring shaped canvas.
        /// </summary>
        public int RingExternalDiameter {
            get { return ringExternalDiamater; }
            set  {
                if (ringExternalDiamater != value)
                {
                    ringExternalDiamater = value;
                    SetUIAngle(angle);
                }
            }
        }

        /// <summary>
        /// Whether the center of the ring should be bottom or top of the canvas.
        /// </summary>
        public bool RingFlipVertical {
            get { return ringFlipVertical; }
            set  {
                if (ringFlipVertical != value)
                {
                    ringFlipVertical = value;
                    SetUIAngle(angle);
                }
            }
        }

        /// <summary>
        /// If enabled, CurvedUI will try to preserve aspect ratio of original canvas.
        /// </summary>
        public bool PreserveAspect {
            get { return preserveAspect; }
            set {
                if (preserveAspect != value)
                {
                    preserveAspect = value;
                    SetUIAngle(angle);
                }
            }
        }


        /// <summary>
        /// Can the canvas be interacted with?
        /// </summary>
        public bool Interactable {
            get { return interactable; }
            set  {  interactable = value; }
        }

        /// <summary>
        /// Should The collider for this canvas be created using more expensive box colliders?
        /// DEfault false.
        /// </summary>
        public bool ForceUseBoxCollider {
            get { return forceUseBoxCollider; }
            set {  forceUseBoxCollider = value; }
        }

        

        /// <summary>
        /// Will the canvas block raycasts
        /// Settings this to false will destroy the canvas' collider.
        /// </summary>
        public bool BlocksRaycasts {
			get { return blocksRaycasts; }
			set
			{
				if (blocksRaycasts != value) {
					blocksRaycasts = value;

                    //tell raycaster to update its collider now that angle has changed.
                    if (Application.isPlaying && GetComponent<CurvedUIRaycaster>() != null)
						GetComponent<CurvedUIRaycaster>().RebuildCollider();
				}
			}
		}

        /// <summary>
        /// Should the raycaster take other layers into account to determine if canvas has been interacted with.
        /// </summary>
        [Obsolete("Use RaycastLayerMask property instead.")]
        public bool RaycastMyLayerOnly {
            get { return true; }
            set { }
        }

        /// <summary>
        /// Forces all child CurvedUI objects to recalculate
        /// </summary>
        /// <param name="calculateCurvedOnly"> Forces children to recalculate only the curvature. Will not make them retesselate vertices. Much faster.</param>
        public void SetAllChildrenDirty(bool recalculateCurveOnly = false)
        {
            foreach (CurvedUIVertexEffect eff in this.GetComponentsInChildren<CurvedUIVertexEffect>())
            {
                if (recalculateCurveOnly)
                    eff.SetDirty();
                else
                    eff.CurvingRequired = true;
            }
        }
        #endregion




        #region SHORTCUTS
        /// <summary>
        /// Returns true if user's pointer is currently pointing inside this canvas.
        /// This is a shortcut to CurvedUIRaycaster's PointingAtCanvas bool.
        /// </summary>
        public bool PointingAtCanvas {
            get {
                if (GetComponent<CurvedUIRaycaster>() != null)
                    return GetComponent<CurvedUIRaycaster>().PointingAtCanvas;
                else {
                    Debug.LogWarning("CURVEDUI: Can't check if user is pointing at this canvas - No CurvedUIRaycaster is added to this canvas.");
                    return false;
                }
            }
        }

        /// <summary>
        /// Sends OnClick event to every Button under pointer.
        /// This is a shortcut to CurvedUIRaycaster's Click method.
        /// </summary>
        public void Click()
        {
            if (GetComponent<CurvedUIRaycaster>() != null)
                GetComponent<CurvedUIRaycaster>().Click();
        }

        /// <summary>
        /// Current controller mode. Decides how user can interact with the canvas. 
        /// This is a shortcut to CurvedUIInputModule's property.
        /// </summary>
        public CurvedUIInputModule.CUIControlMethod ControlMethod {
            get { return CurvedUIInputModule.ControlMethod; }
            set { CurvedUIInputModule.ControlMethod = value; }
        }

        /// <summary>
        /// Returns all objects currently under the pointer.
        /// This is a shortcut to CurvedUIInputModule's method.
        /// </summary>
        public List<GameObject> GetObjectsUnderPointer()
        {
            if (GetComponent<CurvedUIRaycaster>() != null)
                return GetComponent<CurvedUIRaycaster>().GetObjectsUnderPointer();
            else return new List<GameObject>();
        }

        /// <summary>
        /// Returns all the canvas objects that are visible under given Screen Position of EventCamera
        /// This is a shortcut to CurvedUIInputModule's method.
        /// </summary>
        public List<GameObject> GetObjectsUnderScreenPos(Vector2 pos, Camera eventCamera = null)
        {
            if (eventCamera == null)
                eventCamera = myCanvas.worldCamera;

            if (GetComponent<CurvedUIRaycaster>() != null)
                return GetComponent<CurvedUIRaycaster>().GetObjectsUnderScreenPos(pos, eventCamera);
            else return new List<GameObject>();
        }


        /// <summary>
		/// Returns all the canvas objects that are intersected by given ray.
		/// This is a shortcut to CurvedUIInputModule's method.
        /// </summary>
		public List<GameObject> GetObjectsHitByRay(Ray ray)
        {
            if (GetComponent<CurvedUIRaycaster>() != null)
                return GetComponent<CurvedUIRaycaster>().GetObjectsHitByRay(ray);
            else return new List<GameObject>();
        }

        /// <summary>
        /// Gaze Control Method. Should execute OnClick events on button after user points at them?
		/// This is a shortcut to CurvedUIInputModule's property.
        /// </summary>
        public bool GazeUseTimedClick {
            get { return CurvedUIInputModule.Instance.GazeUseTimedClick; }
            set { CurvedUIInputModule.Instance.GazeUseTimedClick = value; }
        }

        /// <summary>
        /// Gaze Control Method. How long after user points on a button should we click it?
		/// This is a shortcut to CurvedUIInputModule's property.
        /// </summary>
        public float GazeClickTimer {
            get { return CurvedUIInputModule.Instance.GazeClickTimer; }
            set { CurvedUIInputModule.Instance.GazeClickTimer = value; }
        }

        /// <summary>
        /// Gaze Control Method. How long after user looks at a button should we start the timer? Default 1 second.
        /// This is a shortcut to CurvedUIInputModule's property.
        /// </summary>
        public float GazeClickTimerDelay {
            get { return CurvedUIInputModule.Instance.GazeClickTimerDelay; }
            set { CurvedUIInputModule.Instance.GazeClickTimerDelay = value; }
        }

        /// <summary>
        /// Gaze Control Method. How long till Click method is executed on Buttons under gaze? Goes 0-1.
		/// This is a shortcut to CurvedUIInputModule's property.
        /// </summary>
        public float GazeTimerProgress {
            get { return CurvedUIInputModule.Instance.GazeTimerProgress; }
        }
        #endregion







        #region ENUMS
        public enum CurvedUIShape
		{
			CYLINDER = 0,
			RING = 1,
			SPHERE = 2,
			CYLINDER_VERTICAL = 3,
		}
		#endregion
    }
}
