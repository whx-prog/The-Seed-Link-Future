using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

#if CURVEDUI_TMP || TMP_PRESENT
using TMPro;
#endif

//To use this class you have to add CURVEDUI_TMP to your define symbols. You can do it in project settings.
//To learn how to do it visit http://docs.unity3d.com/Manual/PlatformDependentCompilation.html and search for "Platform Custom Defines"
namespace CurvedUI
{
    [ExecuteInEditMode]
    public class CurvedUITMP : MonoBehaviour
    {

#if CURVEDUI_TMP || TMP_PRESENT

        //internal
        private CurvedUIVertexEffect crvdVE;
        private TextMeshProUGUI tmpText;
        private CurvedUISettings mySettings;

        private List<UIVertex> m_UIVerts = new List<UIVertex>();
        private UIVertex m_tempVertex;
        private CurvedUITMPSubmesh m_tempSubMsh;

        private Vector2 savedSize;
        private Vector3 savedUp;
        private Vector3 savedPos;
        private Vector3 savedLocalScale;
        private Vector3 savedGlobalScale;
        private List<CurvedUITMPSubmesh> subMeshes = new List<CurvedUITMPSubmesh>();

        //flags
        public bool Dirty = false; // set this to true to force mesh update.
        private bool curvingRequired = false;
        private bool tesselationRequired = false;
        private bool quitting = false;

        //mesh data
        private Vector3[] vertices;
        //These are commented here and throught the script,
        //cause CurvedUI operates only on vertex positions,
        //but left here for future-proofing against some TMP features.
        //private Color32[] colors32;
        //private Vector2[] uv;
        //private Vector2[] uv2;
        //private Vector2[] uv3;
        //private Vector2[] uv4;
        //private Vector3[] normals;
        //private Vector4[] tangents;
        //private int[] indices;

        #region LIFECYCLE
        void Start()
        {
            if (mySettings == null)
                mySettings = GetComponentInParent<CurvedUISettings>();
        }



        void OnEnable()
        {
            FindTMP();

            if (tmpText)
            {
                tmpText.RegisterDirtyMaterialCallback(TesselationRequiredCallback);
                TMPro_EventManager.TEXT_CHANGED_EVENT.Add(TMPTextChangedCallback);

                tmpText.SetText(tmpText.text);
            }

#if UNITY_EDITOR
            if (!Application.isPlaying)
                UnityEditor.EditorApplication.update += LateUpdate;
#endif
        }



        void OnDisable()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                UnityEditor.EditorApplication.update -= LateUpdate;
#endif
            if (tmpText)
            {
                tmpText.UnregisterDirtyMaterialCallback(TesselationRequiredCallback);
                TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(TMPTextChangedCallback);
            }
        }



        void OnDestroy()
        {
            quitting = true;
        }


        void LateUpdate()
        {
            //if we're missing stuff, find it
            if (!tmpText) FindTMP();

            if (mySettings == null) return;

            //Edit Mesh on TextMeshPro component
            if (tmpText && !quitting)
            {
                if (ShouldTesselate())
                    tesselationRequired = true;

                if (Dirty || tesselationRequired || (curvingRequired && !Application.isPlaying))
                {
                    if (mySettings == null)
                    {
                        enabled = false;
                        return;
                    }
                    
                    //Get the flat vertices from TMP object.
                    //store a copy of flat UIVertices for later so we dont have to retrieve the Mesh every framee.
                    tmpText.renderMode = TMPro.TextRenderFlags.Render;
                    tmpText.ForceMeshUpdate(true);
                    CreateUIVertexList(tmpText.mesh);

                    //Tesselate and Curve the flat UIVertices stored in Vertex Helper
                    crvdVE.ModifyTMPMesh(ref m_UIVerts);

                    //fill curved vertices back to TMP mesh
                    FillMeshWithUIVertexList(tmpText.mesh, m_UIVerts);

                    //cleanup
                    tmpText.renderMode = TMPro.TextRenderFlags.DontRender;

                    //save current data
                    savedLocalScale = mySettings.transform.localScale;
                    savedGlobalScale = mySettings.transform.lossyScale;
                    savedSize = (transform as RectTransform).rect.size;
                    savedUp = mySettings.transform.worldToLocalMatrix.MultiplyVector(transform.up);
                    savedPos = mySettings.transform.worldToLocalMatrix.MultiplyPoint3x4(transform.position);
                    
                    //reset flags
                    tesselationRequired = false;
                    curvingRequired = false;
                    Dirty = false;

                    //prompt submeshes to update
                    FindSubmeshes();
                    foreach (CurvedUITMPSubmesh mesh in subMeshes)
                        mesh.UpdateSubmesh(true, false);
                }

                //Upload mesh to TMP Object's renderer
                if(tmpText.text.Length > 0)
                    tmpText.canvasRenderer.SetMesh(tmpText.mesh);
                else 
                    tmpText.canvasRenderer.Clear();
            }
        }
        #endregion




        #region UIVERTEX MANAGEMENT
        void CreateUIVertexList(Mesh mesh)
        {
            //trim if too long list
            if (mesh.vertexCount < m_UIVerts.Count)
                m_UIVerts.RemoveRange(mesh.vertexCount, m_UIVerts.Count - mesh.vertexCount);

            //extract mesh data
            vertices = mesh.vertices;
            //colors32 = mesh.colors32;
            //uv = mesh.uv;
            //uv2 = mesh.uv2;
            //uv3 = mesh.uv3;
            //uv4 = mesh.uv4;
            //normals = mesh.normals;
            //tangents = mesh.tangents;

            for (int i = 0; i < mesh.vertexCount; i++)
            {
                //add if list too short
                if (m_UIVerts.Count <= i)
                {
                    m_tempVertex = new UIVertex();
                    GetUIVertexFromMesh(ref m_tempVertex, i);
                    m_UIVerts.Add(m_tempVertex);
                }
                else //modify
                {
                    m_tempVertex = m_UIVerts[i];
                    GetUIVertexFromMesh(ref m_tempVertex, i);
                    m_UIVerts[i] = m_tempVertex;
                }
            }
            //indices = mesh.GetIndices(0);
        }

        void GetUIVertexFromMesh(ref UIVertex vert, int i)
        {
            vert.position = vertices[i];
            //vert.color = colors32[i];
            //vert.uv0 = uv[i];
            //vert.uv1 = uv2.Length > i ? uv2[i] : Vector2.zero;
            //vert.uv2 = uv3.Length > i ? uv3[i] : Vector2.zero;
            //vert.uv3 = uv4.Length > i ? uv4[i] : Vector2.zero;
            //vert.normal = normals[i];
            //vert.tangent = tangents[i];
        }

        void FillMeshWithUIVertexList(Mesh mesh, List<UIVertex> list)
        {
            if (list.Count >= 65536)
            {
                Debug.LogError("CURVEDUI: Unity UI Mesh can not have more than 65536 vertices. Remove some UI elements or lower quality.");
                return;
            }

            for (int i = 0; i < list.Count; i++)
            {
                vertices[i] = list[i].position;
                //colors32[i] = list[i].color;
                //uv[i] = list[i].uv0;
                //if (uv2.Length < i) uv2[i] = list[i].uv1;
                ////if (uv3.Length < i) uv3[i] = list[i].uv2;
                ////if (uv4.Length < i) uv4[i] = list[i].uv3;
                //normals[i] = list[i].normal;
                //tangents[i] = list[i].tangent;
            }


            //Fill mesh with data
            mesh.vertices = vertices;
            //mesh.colors32 = colors32;
            //mesh.uv = uv;
            //mesh.uv2 = uv2;
            ////mesh.uv3 = uv3;
            ////mesh.uv4 = uv4;
            //mesh.normals = normals;
            //mesh.tangents = tangents;
            //mesh.SetTriangles(indices, 0);
            mesh.RecalculateBounds();
        }
        #endregion



        #region PRIVATE
        void FindTMP()
        {
            if (this.GetComponent<TextMeshProUGUI>() != null)
            {
                tmpText = this.gameObject.GetComponent<TextMeshProUGUI>();
                crvdVE = this.gameObject.GetComponent<CurvedUIVertexEffect>();
                mySettings = GetComponentInParent<CurvedUISettings>();
                transform.hasChanged = false;

                FindSubmeshes();
            }
        }


        void FindSubmeshes()
        {
            foreach (TMP_SubMeshUI sub in GetComponentsInChildren<TMP_SubMeshUI>())
            {
                m_tempSubMsh = sub.gameObject.AddComponentIfMissing<CurvedUITMPSubmesh>();
                if (!subMeshes.Contains(m_tempSubMsh))
                    subMeshes.Add(m_tempSubMsh);
            }
        }

        bool ShouldTesselate()
        {
            if (savedSize != (transform as RectTransform).rect.size)
            {
                //Debug.Log("size changed");
                return true;
            }
            else if (savedLocalScale != mySettings.transform.localScale)
            {
                //Debug.Log("local scale changed");
                return true;
            }
            else if (savedGlobalScale != mySettings.transform.lossyScale)
            {
                //Debug.Log("global scale changed");
                return true;
            }
            else if (!savedUp.AlmostEqual(mySettings.transform.worldToLocalMatrix.MultiplyVector(transform.up)))
            {
                // Debug.Log("up changed");
                return true;
            }

            Vector3 testedPos = mySettings.transform.worldToLocalMatrix.MultiplyPoint3x4(transform.position);
            if (!savedPos.AlmostEqual(testedPos))
            {
                //we dont have to curve vertices if we only moved the object vertically in a cylinder.
                if (mySettings.Shape != CurvedUISettings.CurvedUIShape.CYLINDER || Mathf.Pow(testedPos.x - savedPos.x, 2) > 0.00001 || Mathf.Pow(testedPos.z - savedPos.z, 2) > 0.00001)
                {
                    //Debug.Log("pos changed");
                    return true;
                }
            }

            return false;
        }
        #endregion




        #region EVENTS AND CALLBACKS
        void TMPTextChangedCallback(object obj)
        {
            if (obj != (object)tmpText) return;

            tesselationRequired = true;
            //Debug.Log("tmp prop changed on "+this.gameObject.name, this.gameObject);
        }

        void TesselationRequiredCallback()
        {
            tesselationRequired = true;
            curvingRequired = true;
        }
        #endregion

#endif
    }
}



