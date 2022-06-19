using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CurvedUI
{
    public class CUI_PickImageFromSet : MonoBehaviour
    {

        static CUI_PickImageFromSet picked = null;



        public void PickThis()
        {
            if (picked != null)
                picked.GetComponent<Button>().targetGraphic.color = Color.white;

            Debug.Log("Clicked this!", this.gameObject);


            picked = this;
            picked.GetComponent<Button>().targetGraphic.color = Color.red;
        }
    }
}


