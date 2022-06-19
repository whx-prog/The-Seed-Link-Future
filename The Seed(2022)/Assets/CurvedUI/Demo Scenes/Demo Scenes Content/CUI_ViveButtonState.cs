using UnityEngine;
using System.Collections;
using UnityEngine.UI;
namespace CurvedUI {

    /// <summary>
    /// This script show you how to access the state of any button on Vive Controller via CurvedUI scripts. We use right controller as an example
    /// </summary>
    public class CUI_ViveButtonState : MonoBehaviour
    {

        enum ViveButton
        {
            Trigger,
            TouchpadTouch,
            TouchpadPress,
            Grip,
            Menu,

        }

#pragma warning disable 414 // this is just so we wont get "unused variable" code warnings when compiling without Vive.
        [SerializeField]
        Color ActiveColor = Color.green;
        [SerializeField]
        Color InActiveColor = Color.gray;
        [SerializeField] ViveButton ShowStateFor = ViveButton.Trigger;
#pragma warning restore 414


#if CURVEDUI_STEAMVR_LEGACY
        // Update is called once per frame
        void Update()
        {
           
            if(CurvedUIInputModule.Right == null)
            {
                Debug.LogError("Right controller not found - it may be off");
                return;
            }

            bool pressed = false;

            switch (ShowStateFor)
            {
                case ViveButton.Trigger:
                {
                    pressed = GetUsedController().IsTriggerPressed;
                    break;
                }
                case ViveButton.TouchpadPress:
                {
                    pressed = GetUsedController().IsTouchpadPressed;
                    break;
                }
                case ViveButton.TouchpadTouch:
                {
                    pressed = GetUsedController().IsTouchpadTouched;
                    break;
                }
                case ViveButton.Grip:
                {
                    pressed = GetUsedController().IsGripPressed;
                    break;
                }
                case ViveButton.Menu:
                {
                    pressed = GetUsedController().IsApplicationMenuPressed;
                    break;
                }
            }

            this.GetComponentInChildren<Image>().color = pressed ? ActiveColor : InActiveColor;
        }

        CurvedUIViveController GetUsedController()
        {
            return CurvedUIInputModule.Instance.UsedHand == CurvedUIInputModule.Hand.Right ? CurvedUIInputModule.Right : CurvedUIInputModule.Left;
        }
#endif
    }
}


