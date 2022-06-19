using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace CurvedUI
{
    /// <summary>
    /// This component enables accurate object dragging over curved canvas. It supports both mouse and gaze controllers. Add it to your canvas object with image component.
    /// </summary>
    public class CUI_draggable : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
    {

        Vector2 savedVector;
        bool isDragged = false;


        public void OnBeginDrag(PointerEventData data)
        {
            Debug.Log("OnBeginDrag");
            Vector2 newPos = Vector2.zero;
            RaycastPosition(out newPos);

            //save distance from click point to object center to allow for precise dragging
            savedVector = new Vector2((transform as RectTransform).localPosition.x, (transform as RectTransform).localPosition.y) - newPos;

            isDragged = true;
        }

        public void OnDrag(PointerEventData data)  {  }

        public void OnEndDrag(PointerEventData data)
        {
            Debug.Log("OnEndDrag");

            isDragged = false;
        }

        void LateUpdate()
        {
            if (!isDragged) return;

            Debug.Log("OnDrag");

            //drag the transform along the mouse. We use raycast to determine its position on curved canvas.
            Vector2 newPos = Vector2.zero;

            RaycastPosition(out newPos);

            //add our initial distance from objects center
            (transform as RectTransform).localPosition = newPos + savedVector;
        }


        void RaycastPosition(out Vector2 newPos)
        {
            if (CurvedUIInputModule.ControlMethod == CurvedUIInputModule.CUIControlMethod.MOUSE)
            {
                //position when using mouse
                GetComponentInParent<CurvedUISettings>().RaycastToCanvasSpace(Camera.main.ScreenPointToRay(Input.mousePosition), out newPos);

            }
            else if (CurvedUIInputModule.ControlMethod == CurvedUIInputModule.CUIControlMethod.GAZE)
            {
                //position when using gaze - uses the center of the screen as guiding point.
                GetComponentInParent<CurvedUISettings>().RaycastToCanvasSpace(Camera.main.ScreenPointToRay(new Vector2(Screen.width / 2.0f, Screen.height / 2.0f)), out newPos);
            }
            else newPos = Vector2.zero;
        }
    }
}
