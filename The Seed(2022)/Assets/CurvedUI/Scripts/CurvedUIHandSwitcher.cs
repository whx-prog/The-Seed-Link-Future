using UnityEngine;
using System.Collections;
using System.Security.Permissions;

namespace CurvedUI {

    /// <summary>
    /// This script switches the hand controlling the UI when a click on the other controller's trigger is detected.
    /// This emulates the functionality seen in SteamVR overlay or Oculus Home.
    /// Works both for SteamVR and Oculus SDK.
    /// </summary>
    public class CurvedUIHandSwitcher : MonoBehaviour
    {
#pragma warning disable 0649
#pragma warning disable 414
        [SerializeField]
        GameObject LaserBeam;

        [SerializeField]
        [Tooltip("If true, when player clicks the trigger on the other hand, we'll instantly set it as UI controlling hand and move the pointer to it.")]
        bool autoSwitchHands = true;

        [Header("Optional")] 
        [SerializeField] [Tooltip("If set, pointer will be placed as a child of this transform, instead of the current VR SDKs Camera Rig.")]
        private Transform leftHandOverride;
        [SerializeField] [Tooltip("If set, pointer will be placed as a child of this transform, instead of the current VR SDKs Camera Rig.")]
        private Transform rightHandOverride;
        
#pragma warning restore 414
#pragma warning restore 0649




#if CURVEDUI_OCULUSVR
        //variables
        OVRInput.Controller activeCont;
        bool initialized = false;

        void Update()
        {
            if (CurvedUIInputModule.ControlMethod != CurvedUIInputModule.CUIControlMethod.OCULUSVR) return;

            activeCont = OVRInput.GetActiveController();

            if (!initialized && CurvedUIInputModule.Instance.OculusTouchUsedControllerTransform != null)
            {
                //Launch Hand Switch. This will place the laser pointer in the current hand.
                SwitchHandTo(CurvedUIInputModule.Instance.UsedHand);

                initialized = true;
            }

            //for Oculus Go and GearVR, switch automatically if a different controller is connected.
            //This covers the case where User changes hand setting in Oculus Go menu and gets back to our app.
            if ((activeCont == OVRInput.Controller.LTouch || activeCont == OVRInput.Controller.LHand)
                && CurvedUIInputModule.Instance.UsedHand != CurvedUIInputModule.Hand.Left)
                SwitchHandTo(CurvedUIInputModule.Hand.Left);
            else if ((activeCont == OVRInput.Controller.RTouch || activeCont == OVRInput.Controller.RHand) 
                && CurvedUIInputModule.Instance.UsedHand != CurvedUIInputModule.Hand.Right)
                SwitchHandTo(CurvedUIInputModule.Hand.Right);

            if(autoSwitchHands){
                //For Oculus Rift, we wait for the click before we change the pointer.
                if (IsButtonDownOnController(OVRInput.Controller.LTouch) && CurvedUIInputModule.Instance.UsedHand != CurvedUIInputModule.Hand.Left)
                {
                   SwitchHandTo(CurvedUIInputModule.Hand.Left);
                }
                else if (IsButtonDownOnController(OVRInput.Controller.RTouch) && CurvedUIInputModule.Instance.UsedHand != CurvedUIInputModule.Hand.Right)
                {
                   SwitchHandTo(CurvedUIInputModule.Hand.Right);
                }
            }  
        }

        bool IsButtonDownOnController(OVRInput.Controller cont, OVRInput.Controller cont2 = OVRInput.Controller.None)
        {
            return OVRInput.GetDown(CurvedUIInputModule.Instance.OculusTouchInteractionButton, cont) || (cont2 != OVRInput.Controller.None && OVRInput.GetDown(CurvedUIInputModule.Instance.OculusTouchInteractionButton, cont2));
        }





#elif CURVEDUI_STEAMVR_LEGACY
        void Start()
        {
            //connect to steamVR's OnModelLoaded events so we can update the pointer the moment controller is detected.
            CurvedUIInputModule.Right.ModelLoaded += OnModelLoaded;
            CurvedUIInputModule.Left.ModelLoaded += OnModelLoaded;
        }

        void OnModelLoaded(object sender)
        {
            SwitchHandTo(CurvedUIInputModule.Instance.UsedHand);
        }

        void Update()
        {       
            if (CurvedUIInputModule.ControlMethod != CurvedUIInputModule.CUIControlMethod.STEAMVR_LEGACY) return;

            if(autoSwitchHands){

                if (CurvedUIInputModule.Right != null && CurvedUIInputModule.Right.IsTriggerDown && CurvedUIInputModule.Instance.UsedHand != CurvedUIInputModule.Hand.Right)
                {
                    SwitchHandTo(CurvedUIInputModule.Hand.Right);

                }
                else if (CurvedUIInputModule.Left != null && CurvedUIInputModule.Left.IsTriggerDown && CurvedUIInputModule.Instance.UsedHand != CurvedUIInputModule.Hand.Left)
                {
                    SwitchHandTo(CurvedUIInputModule.Hand.Left);

                }
            }
        }


#elif CURVEDUI_STEAMVR_2
        void Start()
        {
            //initial setup in proper hand
            SwitchHandTo(CurvedUIInputModule.Instance.UsedHand);
        }
        void Update()
        {
           if (CurvedUIInputModule.ControlMethod != CurvedUIInputModule.CUIControlMethod.STEAMVR_2) return;

            //Switch hands during runtime when user clicks the action button on another controller
            if (autoSwitchHands && CurvedUIInputModule.Instance.SteamVRClickAction != null)
            {
                if (CurvedUIInputModule.Instance.SteamVRClickAction.GetState(Valve.VR.SteamVR_Input_Sources.RightHand) && CurvedUIInputModule.Instance.UsedHand != CurvedUIInputModule.Hand.Right){
                    SwitchHandTo(CurvedUIInputModule.Hand.Right);
                }
                else if (CurvedUIInputModule.Instance.SteamVRClickAction.GetState(Valve.VR.SteamVR_Input_Sources.LeftHand) && CurvedUIInputModule.Instance.UsedHand != CurvedUIInputModule.Hand.Left ){
                    SwitchHandTo(CurvedUIInputModule.Hand.Left);
                }
            }
        }
        
#elif CURVEDUI_UNITY_XR
        void Start()
        {
            //initial setup in proper hand
            SwitchHandTo(CurvedUIInputModule.Instance.UsedHand);
        }
        void Update()
        {
           if (!autoSwitchHands || CurvedUIInputModule.ControlMethod != CurvedUIInputModule.CUIControlMethod.UNITY_XR) return;

           bool pressed = false;
           if (CurvedUIInputModule.Instance.RightXRController != null && CurvedUIInputModule.Instance.UsedHand != CurvedUIInputModule.Hand.Right)
           {
               //get pressed ui button state on right controller.
               CurvedUIInputModule.Instance.RightXRController.inputDevice.IsPressed(CurvedUIInputModule.Instance.RightXRController.uiPressUsage,
                   out pressed, CurvedUIInputModule.Instance.RightXRController.axisToPressThreshold);

               if(pressed)
                   SwitchHandTo(CurvedUIInputModule.Hand.Right);
           } 
                
           if (CurvedUIInputModule.Instance.LeftXRController != null && CurvedUIInputModule.Instance.UsedHand != CurvedUIInputModule.Hand.Left)
           {
               //get pressed ui button state on left controller.
               CurvedUIInputModule.Instance.LeftXRController.inputDevice.IsPressed(CurvedUIInputModule.Instance.LeftXRController.uiPressUsage,
                   out pressed, CurvedUIInputModule.Instance.LeftXRController.axisToPressThreshold);

               if(pressed)
                   SwitchHandTo(CurvedUIInputModule.Hand.Left);
           }
        }
#endif




        #region HELPER FUNCTIONS
        void SwitchHandTo(CurvedUIInputModule.Hand newHand)
        {
            CurvedUIInputModule.Instance.UsedHand = newHand;

            if (CurvedUIInputModule.Instance.ControllerTransform)
            {
                //hand overrides
                if (newHand ==  CurvedUIInputModule.Hand.Left && leftHandOverride)
                {
                    CurvedUIInputModule.Instance.PointerTransformOverride = leftHandOverride;
                }
                if (newHand ==  CurvedUIInputModule.Hand.Right && rightHandOverride)
                {
                    CurvedUIInputModule.Instance.PointerTransformOverride = rightHandOverride;
                }

                LaserBeam.transform.SetParent(CurvedUIInputModule.Instance.ControllerTransform);
                LaserBeam.transform.ResetTransform();
                LaserBeam.transform.LookAt(LaserBeam.transform.position + CurvedUIInputModule.Instance.ControllerPointingDirection);

            }
            else Debug.LogError("CURVEDUI: No Active controller that can be used as a parent of the pointer. Is the controller gameobject present on the scene and active?");
        }
        #endregion
    }

}


