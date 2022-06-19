using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CurvedUI
{
    public class CUI_ChangeValueOnHold : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {

        bool pressed = false;
        bool selected = false;

#pragma warning disable 0649
        [SerializeField]
        Image bg;
        [SerializeField]
        Color SelectedColor;
        [SerializeField]
        Color NormalColor;

        [SerializeField]
        CanvasGroup IntroCG;
        [SerializeField]
        CanvasGroup MenuCG;
#pragma warning restore 0649


        // Update is called once per frame
        void Update()
        {

            pressed = Input.GetKey(KeyCode.Space) || Input.GetButton("Fire1");

            ChangeVal();       
        }


        void ChangeVal()
        {

            if (this.GetComponent<Slider>().normalizedValue == 1)
            {
                //fade intro screen if we reached max slider value
                IntroCG.alpha -= Time.deltaTime;
                MenuCG.alpha += Time.deltaTime;
            }
            else {
                //change slider value - increase if its selected and button is pressed
                this.GetComponent<Slider>().normalizedValue += (pressed && selected) ? Time.deltaTime : -Time.deltaTime;
            }

            //change if intro screen can block interactions based on its opacity
            IntroCG.blocksRaycasts = IntroCG.alpha > 0;
        }


        public void OnPointerEnter(PointerEventData data)
        {
            bg.color = SelectedColor;
            bg.GetComponent<CurvedUIVertexEffect>().TesselationRequired = true;
            selected = true;
        }

        public void OnPointerExit(PointerEventData data)
        {
            bg.color = NormalColor;
            bg.GetComponent<CurvedUIVertexEffect>().TesselationRequired = true;
            selected = false;
        }

    }
}
