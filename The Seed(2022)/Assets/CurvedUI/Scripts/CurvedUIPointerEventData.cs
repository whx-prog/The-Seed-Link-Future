using System;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CurvedUI
{
    /// <summary>
    /// This class stores additional information that CurvedUI uses for its Pointer Events. 
    /// Right now its only used to store the controller used to interact with canvas.
    /// </summary>
    public class CurvedUIPointerEventData : PointerEventData
    {
        public CurvedUIPointerEventData(EventSystem eventSystem)
            : base(eventSystem)
        {

        }

        public enum ControllerType
        {
            NONE = -1,
            VIVE = 0,
        }

        public GameObject Controller;

        /// <summary>
        /// Basically the position of user's finger on a touchpad. Goes from -1,-1 to 1,1
        /// </summary>
        public Vector2 TouchPadAxis = Vector2.zero;
    }
}