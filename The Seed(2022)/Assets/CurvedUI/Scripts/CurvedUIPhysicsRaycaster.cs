using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

namespace CurvedUI
{
    /// <summary>
    /// Raycaster used for interactions with 3D objects.
    /// </summary>
    public class CurvedUIPhysicsRaycaster : BaseRaycaster
    {
        #region VARIABLES AND SETTINGS
        [SerializeField]
        protected int sortOrder = 20;


        //variables
        RaycastHit hitInfo;
        RaycastResult result;
        #endregion


        #region CONSTRUCTOR
        protected CurvedUIPhysicsRaycaster() { }
        #endregion


        #region RAYCASTING
        public override void Raycast(PointerEventData eventData, List<RaycastResult> resultAppendList)
        {
            //check if we have camera from which to cast a ray
            if (CurvedUIInputModule.Instance == null || CurvedUIInputModule.Instance.EventCamera == null)
                return;

            if (Physics.Raycast(CurvedUIInputModule.Instance.GetEventRay(), out hitInfo, float.PositiveInfinity, CompoundEventMask))
            {
                if (hitInfo.collider.GetComponentInParent<CurvedUISettings>()) return; //a canvas is hit - these raycastsResults are handled by CurvedUIRaycasters

                result = new RaycastResult
                {
                    gameObject = hitInfo.collider.gameObject,
                    module = this,
                    distance = hitInfo.distance,
                    index = resultAppendList.Count,
                    worldPosition = hitInfo.point,
                    worldNormal = hitInfo.normal,
                };
                resultAppendList.Add(result);
            }

            //Debug.Log("CUIPhysRaycaster: " + resultAppendList.Count);
        }
        #endregion


        #region SETTERS AND GETTERS
        /// <summary>
        /// This Component's event mask + eventCamera's event mask.
        /// </summary>
        public int CompoundEventMask {
            get { return (eventCamera != null) ? eventCamera.cullingMask & CurvedUIInputModule.Instance.RaycastLayerMask : -1; }
        }
        

        /// <summary>
        /// Camera used to process events
        /// </summary>
        public override Camera eventCamera {
            get  { return CurvedUIInputModule.Instance? CurvedUIInputModule.Instance.EventCamera : null;  }
        }

        public virtual int Depth {
            get { return (eventCamera != null) ? (int)eventCamera.depth : 0xFFFFFF; }
        }

        public override int sortOrderPriority {
            get  {  return sortOrder; }
        }
        #endregion
    }
}
