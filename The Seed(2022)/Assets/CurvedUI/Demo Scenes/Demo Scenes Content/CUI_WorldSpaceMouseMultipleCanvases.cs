using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace CurvedUI
{
    public class CUI_WorldSpaceMouseMultipleCanvases : MonoBehaviour
    {

        #pragma warning disable 0649 
        [SerializeField]
        List<CurvedUISettings> ControlledCanvases;
        [SerializeField]
        Transform WorldSpaceMouse;
        [SerializeField]
        CurvedUISettings MouseCanvas;
        #pragma warning restore 0649



        // Update is called once per frame
        void Update()
        {

            Vector3 worldSpaceMousePosInWorldSpace = MouseCanvas.CanvasToCurvedCanvas(WorldSpaceMouse.localPosition);
            Ray ControllerRay = new Ray(Camera.main.transform.position, worldSpaceMousePosInWorldSpace - Camera.main.transform.position);

            CurvedUIInputModule.CustomControllerRay = ControllerRay;


            if (Input.GetButton("Fire2"))
            {
                Vector2 newPos = Vector2.zero;
                MouseCanvas.RaycastToCanvasSpace(new Ray(Camera.main.transform.position, Camera.main.transform.forward), out newPos);
                CurvedUIInputModule.Instance.WorldSpaceMouseInCanvasSpace = newPos;

            }

            Debug.DrawRay(ControllerRay.GetPoint(0), ControllerRay.direction * 1000, Color.cyan);
        }
    }
}
