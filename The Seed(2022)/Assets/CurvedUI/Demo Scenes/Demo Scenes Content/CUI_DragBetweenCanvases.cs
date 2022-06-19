using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace CurvedUI
{
    /// <summary>
    /// This component enables accurate object dragging over curved canvas. It also allows user to drop the object on another canvas.
    /// It supports both mouse and gaze controllers. Add it to your canvas object with image component.
    /// </summary>
    public class CUI_DragBetweenCanvases : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
    {

        //saved location where we grabbed the panel
        Vector2 dragPoint;


        public void OnBeginDrag(PointerEventData data)
        {
            Debug.Log("OnBeginDrag");
            Vector2 newPos = Vector2.zero;
            RaycastPosition(out newPos);

            //save distance from click point to object center to allow for precise dragging
            dragPoint = new Vector2((transform as RectTransform).localPosition.x, (transform as RectTransform).localPosition.y) - newPos;
        }

        /// <summary>
        /// drag the transform along the mouse. We use raycast to determine its position on curved canvas. 
        /// </summary>
        /// <param name="data"></param>
        public void OnDrag(PointerEventData data)
        {

            CurvedUISettings myCurvedCanvas = GetComponentInParent<CurvedUISettings>();
            Ray ray3d = Camera.main.ScreenPointToRay(new Vector2(Screen.width / 2.0f, Screen.height / 2.0f));
           

            if (CurvedUIInputModule.ControlMethod == CurvedUIInputModule.CUIControlMethod.MOUSE)
            {
                //position when using mouse
                ray3d = Camera.main.ScreenPointToRay(Input.mousePosition);

            }
            else if (CurvedUIInputModule.ControlMethod == CurvedUIInputModule.CUIControlMethod.GAZE)
            {
                //position when using gaze
                ray3d = Camera.main.ScreenPointToRay(new Vector2(Screen.width / 2.0f, Screen.height / 2.0f));
            }

            //find a canvas underneath the pointer.
            RaycastHit hit;
            if (Physics.Raycast(ray3d, out hit))
            {
                CurvedUISettings curvedCanvasUnderPointer = hit.collider.GetComponentInParent<CurvedUISettings>();
                Vector2 newPos = Vector2.zero;
                if (curvedCanvasUnderPointer != null)
                {
                    //change canvas if we moved it to the other one.
                    if(curvedCanvasUnderPointer != myCurvedCanvas)
                    {
                        //move this panel to the other canvas and resets its position.
                        this.transform.SetParent(curvedCanvasUnderPointer.transform);
                        this.transform.ResetTransform();

                        //force the panel and each of its children to update their parent CurvedUISettings.
                        //This will ensure they will rebuild with proper angle and stuff.
                        foreach(CurvedUIVertexEffect eff in GetComponentsInChildren<CurvedUIVertexEffect>())
                        {
                            eff.FindParentSettings(true);
                        }
                    }

                    //find the raycast position in flat canvas units.
                    curvedCanvasUnderPointer.RaycastToCanvasSpace(ray3d, out newPos);

                    //set the new position of the panel. Add the drag point so we're still holding the object in the same spot.
                    (transform as RectTransform).localPosition = newPos + dragPoint;
                }
            }

           
        }

        public void OnEndDrag(PointerEventData data)
        {
            Debug.Log("OnEndDrag");
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
                //position when using gaze
                GetComponentInParent<CurvedUISettings>().RaycastToCanvasSpace(Camera.main.ScreenPointToRay(new Vector2(Screen.width / 2.0f, Screen.height / 2.0f)), out newPos);
            }
            else newPos = Vector2.zero;
        }

    }
}
