using UnityEngine;
using System.Collections;

namespace CurvedUI
{
    /// <summary>
    /// A simple script to make the pointer follow mouse movement and pass the control ray to canvsa
    /// </summary>
    public class CUI_GunMovement : MonoBehaviour
    {

#pragma warning disable 0649
        [SerializeField]
        CurvedUISettings mySettings;
        [SerializeField]
        Transform pivot;
        [SerializeField]
        float sensitivity = 0.1f;
        Vector3 lastMouse;
#pragma warning restore 0649

        // Use this for initialization
        void Start()
        {
            lastMouse = Input.mousePosition;
        }

        // Update is called once per frame
        void Update()
        {

            Vector3 mouseDelta = Input.mousePosition - lastMouse;
            lastMouse = Input.mousePosition;
            pivot.localEulerAngles += new Vector3(-mouseDelta.y, mouseDelta.x, 0) * sensitivity;


            //pass ray to canvas
            Ray myRay = new Ray(this.transform.position, this.transform.forward);

            CurvedUIInputModule.CustomControllerRay = myRay;
            CurvedUIInputModule.CustomControllerButtonState = Input.GetButton("Fire1");
        }
    }
}
