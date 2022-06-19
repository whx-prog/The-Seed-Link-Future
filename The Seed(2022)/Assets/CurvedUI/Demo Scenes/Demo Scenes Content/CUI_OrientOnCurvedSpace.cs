using UnityEngine;
using System.Collections;

namespace CurvedUI
{
    public class CUI_OrientOnCurvedSpace : MonoBehaviour
    {

        public CurvedUISettings mySettings;


        // Use this for initialization
        void Awake()
        {

            mySettings = GetComponentInParent<CurvedUISettings>();


        }

        // Update is called once per frame
        void Update()
        {
            Vector3 positionInCanvasSpace = mySettings.transform.worldToLocalMatrix.MultiplyPoint3x4(this.transform.parent.position);
            transform.position = mySettings.CanvasToCurvedCanvas(positionInCanvasSpace);
            transform.rotation = Quaternion.LookRotation(mySettings.CanvasToCurvedCanvasNormal(transform.parent.localPosition), transform.parent.up);
        }
    }
}
