using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace CurvedUI
{
    public class CUI_AnimateCurvedFillOnStart : MonoBehaviour
    {

        // Update is called once per frame
        void Update()
        {

            CurvedUISettings set = this.GetComponent<CurvedUISettings>();
            Text textie = this.GetComponentInChildren<Text>();

            if (Time.time < 1.5f)
            {

                set.RingFill = Mathf.PerlinNoise(Time.time * 30.23234f, Time.time * 30.2313f) * 0.15f;
                textie.text = "Accesing Mainframe...";

            }
            else if (Time.time < 2.5f)
            {

                set.RingFill = Mathf.Clamp(set.RingFill + Time.deltaTime * 3, 0, 1);
                textie.text = "Mainframe Active";

            }


        }
    }
}
