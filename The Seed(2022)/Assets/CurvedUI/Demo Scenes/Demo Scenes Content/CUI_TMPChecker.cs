using UnityEngine;
using System.Collections;

namespace CurvedUI
{
    public class CUI_TMPChecker : MonoBehaviour
    {

#pragma warning disable 0649
        [SerializeField]
        GameObject testMsg;

        [SerializeField]
        GameObject enabledMsg;

        [SerializeField]
        GameObject disabledMsg;
#pragma warning restore 0649

        // Use this for initialization
        void Start()
        {
            testMsg.gameObject.SetActive(false);

#if CURVEDUI_TMP || TMP_PRESENT
            enabledMsg.gameObject.SetActive(true);
            disabledMsg.gameObject.SetActive(false);
#else
            enabledMsg.gameObject.SetActive(false);
            disabledMsg.gameObject.SetActive(true);
#endif
        }


    }
}
