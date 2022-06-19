using UnityEngine;
using System.Collections;

namespace CurvedUI {
    public class CUI_ViveHapticPulse : MonoBehaviour
    {

#pragma warning disable 414 // this is just so we wont get "unused variable" code warnings when compiling without Vive.
        float PulseStrength;
#pragma warning restore 414

        void Start()
        {
            PulseStrength = 1;
        }

        public void SetPulseStrength(float newStr)
        {
            PulseStrength = Mathf.Clamp(newStr, 0, 1);
        }

        public void TriggerPulse() 
        {
#if CURVEDUI_STEAMVR_LEGACY
            CurvedUIInputModule.Right.TriggerHapticPulse(1, (ushort)(PulseStrength * 3000));
			#endif 
        }
    }
}

