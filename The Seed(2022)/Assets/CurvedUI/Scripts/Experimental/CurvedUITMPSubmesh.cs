using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if CURVEDUI_TMP || TMP_PRESENT
using TMPro;
#endif

namespace CurvedUI
{
    [ExecuteInEditMode]
    public class CurvedUITMPSubmesh : MonoBehaviour
    {
#if CURVEDUI_TMP || TMP_PRESENT

        //saved references
        private VertexHelper vh;
        private Mesh straightMesh;
        private Mesh curvedMesh;
        private CurvedUIVertexEffect crvdVE;
        private TMP_SubMeshUI TMPsub;
        private TextMeshProUGUI TMPtext;

        public void UpdateSubmesh(bool tesselate, bool curve)
        {
            //find required components
            if (TMPsub == null) TMPsub = gameObject.GetComponent<TMP_SubMeshUI>();

            if (TMPsub == null) return;

            if (TMPtext == null)TMPtext = GetComponentInParent<TextMeshProUGUI>();

            if (crvdVE == null)crvdVE = gameObject.AddComponentIfMissing<CurvedUIVertexEffect>();


            //perform tesselatio and curving
            if (tesselate || straightMesh == null || vh == null || (!Application.isPlaying))
            {
                vh = new VertexHelper(TMPsub.mesh);

                //save straight mesh - it will be curved then every time the object moves on the canvas.
                straightMesh = new Mesh();
                vh.FillMesh(straightMesh);

                curve = true;
            }


            if (curve)
            {
                //Debug.Log("Submesh: Curve", this.gameObject);
                vh = new VertexHelper(straightMesh);
                crvdVE.ModifyMesh(vh);
                curvedMesh = new Mesh();
                vh.FillMesh(curvedMesh);
                crvdVE.CurvingRequired = true;
            }

            //upload mesh to TMP object's renderer
            TMPsub.canvasRenderer.SetMesh(curvedMesh);


            //cleanup for not needed submeshes.
            if (TMPtext != null && TMPtext.textInfo.materialCount < 2)
            {
                //Each submesh uses 1 additional material.
                //If materialCount is 1, this means Submesh is not needed. Bounce it to toggle cleanup.
                TMPsub.enabled = false;
                TMPsub.enabled = true;
            }
        }

#endif
    }

}


