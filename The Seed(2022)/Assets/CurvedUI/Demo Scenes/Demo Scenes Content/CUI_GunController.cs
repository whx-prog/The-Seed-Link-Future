using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CurvedUI
{
    public class CUI_GunController : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField]
        CurvedUISettings ControlledCanvas;
        [SerializeField]
        Transform LaserBeamTransform;
#pragma warning restore 0649

        // Update is called once per frame
        void Update()
        {

            //tell canvas to use the direction of the gun as a ray controller
            Ray myRay = new Ray(this.transform.position, this.transform.forward);

            if (ControlledCanvas)
                CurvedUIInputModule.CustomControllerRay = myRay;


            //change the laser's length depending on where it hits
            float length = 10000;

            RaycastHit hit;
            if (Physics.Raycast(myRay, out hit, length))
            {

                //check for graphic under pointer if we hit curved canvas. We only want transforms with graphics that are drawn by canvas (depth not -1) to block the pointer.
                int SelectablesUnderPointer = 0;
                if (hit.transform.GetComponent<CurvedUIRaycaster>() != null)
                {
                    SelectablesUnderPointer = hit.transform.GetComponent<CurvedUIRaycaster>().GetObjectsUnderPointer().FindAll(x => x.GetComponent<Graphic>() != null && x.GetComponent<Graphic>().depth != -1).Count;
                }

                //Debug.Log("found graphics: " + SelectablesUnderPointer);
                length = SelectablesUnderPointer == 0 ? 10000 : Vector3.Distance(hit.point, this.transform.position);

            }

            LaserBeamTransform.localScale = LaserBeamTransform.localScale.ModifyZ(length);


            //make laser beam thicker if mose is pressed
            if (Input.GetMouseButton(0))
            {

                LaserBeamTransform.localScale = LaserBeamTransform.localScale.ModifyX(0.75f).ModifyY(0.75f);

            }
            else {
                LaserBeamTransform.localScale = LaserBeamTransform.localScale.ModifyX(0.2f).ModifyY(0.2f);

            }
        }
    }
}
