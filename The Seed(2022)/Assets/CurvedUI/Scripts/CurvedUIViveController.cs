using UnityEngine;
using System.Collections;

namespace CurvedUI
{
	/// <summary>
	/// Contains a bunch of events you can subscribe to and bools to ask for current state of the controller. Can also trigger haptic feedback if you ask nicely.
	/// </summary>
	public class CurvedUIViveController : MonoBehaviour
	{
		#if CURVEDUI_STEAMVR_LEGACY
	

		#region settings
		public int axisFidelity = 1;
		#endregion



		#region internal variables
		private uint controllerIndex;
        private Type controllerType = Type.UNDEFINED;
        private SteamVR_TrackedObject trackedController;
		private SteamVR_Controller.Device device;

		private Vector2 touchpadAxis = Vector2.zero;
		private Vector2 triggerAxis = Vector2.zero;

		private bool controllerVisible = true;
		private ushort hapticPulseStrength;
		private int hapticPulseCountdown;
		private ushort maxHapticVibration = 3999;

        private Vector3 oculusTouchPoitingOriginOffset = new Vector3(0, -0.0258f, -0.033f);
        private Vector3 oculusTouchPoitingRotationOffset = new Vector3(38, 0, 0);

        public enum Type
        {
            UNDEFINED = 0,
            VIVE = 1,
            OCULUS_TOUCH = 2,
        }
		#endregion



		#region EVENTS
		public event ViveInputEvent TriggerClicked;
		public event ViveInputEvent TriggerUnclicked;
		public event ViveInputEvent TriggerAxisChanged;
		public event ViveInputEvent ApplicationMenuClicked;
		public event ViveInputEvent ApplicationMenuUnclicked;
		public event ViveInputEvent GripClicked;
		public event ViveInputEvent GripUnclicked;
		public event ViveInputEvent TouchpadClicked;
		public event ViveInputEvent TouchpadUnclicked;
		public event ViveInputEvent TouchpadTouched;
		public event ViveInputEvent TouchpadUntouched;
		public event ViveInputEvent TouchpadAxisChanged;
        public event ViveEvent ModelLoaded;



        public virtual void OnTriggerClicked(ViveInputArgs e)
		{
			if (TriggerClicked != null)
				TriggerClicked(this, e);
		}

		public virtual void OnTriggerUnclicked(ViveInputArgs e)
		{
			if (TriggerUnclicked != null)
				TriggerUnclicked(this, e);
		}

		public virtual void OnTriggerAxisChanged(ViveInputArgs e)
		{
			if (TriggerAxisChanged != null)
				TriggerAxisChanged(this, e);
		}

		public virtual void OnApplicationMenuClicked(ViveInputArgs e)
		{
			if (ApplicationMenuClicked != null)
				ApplicationMenuClicked(this, e);
		}

		public virtual void OnApplicationMenuUnclicked(ViveInputArgs e)
		{
			if (ApplicationMenuUnclicked != null)
				ApplicationMenuUnclicked(this, e);
		}

		public virtual void OnGripClicked(ViveInputArgs e)
		{
			if (GripClicked != null)
				GripClicked(this, e);
		}

		public virtual void OnGripUnclicked(ViveInputArgs e)
		{
			if (GripUnclicked != null)
				GripUnclicked(this, e);
		}

		public virtual void OnTouchpadClicked(ViveInputArgs e)
		{
			if (TouchpadClicked != null)
				TouchpadClicked(this, e);
		}

		public virtual void OnTouchpadUnclicked(ViveInputArgs e)
		{
			if (TouchpadUnclicked != null)
				TouchpadUnclicked(this, e);
		}

		public virtual void OnTouchpadTouched(ViveInputArgs e)
		{
			if (TouchpadTouched != null)
				TouchpadTouched(this, e);
		}

		public virtual void OnTouchpadUntouched(ViveInputArgs e)
		{
			if (TouchpadUntouched != null)
				TouchpadUntouched(this, e);
		}

		public virtual void OnTouchpadAxisChanged(ViveInputArgs e)
		{
			if (TouchpadAxisChanged != null)
				TouchpadAxisChanged(this, e);
		}
		#endregion




		#region LIFECYCLE
		void Awake()
		{
			trackedController = GetComponent<SteamVR_TrackedObject>();
            SteamVR_Events.RenderModelLoaded.AddListener(SteamVRModelLoaded);
		}

		void Update()
		{
			controllerIndex = (uint)trackedController.index;

            //this device is not tracked right now - it has no device index - skip it.
            if (controllerIndex < 0 || controllerIndex >= Valve.VR.OpenVR.k_unMaxTrackedDeviceCount) return;

            device = SteamVR_Controller.Input((int)controllerIndex);

            //get axis inputfrom debice
			Vector2 currentTriggerAxis = device.GetAxis(Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger);
			Vector2 currentTouchpadAxis = device.GetAxis();

			//haptic feedback
			if (hapticPulseCountdown > 0)
			{
				device.TriggerHapticPulse(hapticPulseStrength);
				hapticPulseCountdown -= 1;
			}

			//check for changes in trigger press
			if (Vector2ShallowEquals(triggerAxis, currentTriggerAxis))
			{
				triggerAxisChanged = false;
			}
			else
			{
				OnTriggerAxisChanged(SetButtonEvent(ref triggerPressed, true, currentTriggerAxis.x));
				triggerAxisChanged = true;
			}

			//check for changes in finger pos on touchpad
			if (Vector2ShallowEquals(touchpadAxis, currentTouchpadAxis))
			{
				touchpadAxisChanged = false;
			}
			else
			{
				OnTouchpadAxisChanged(SetButtonEvent(ref touchpadTouched, true, 1f));
				touchpadAxisChanged = true;
			}

			touchpadAxis = new Vector2(currentTouchpadAxis.x, currentTouchpadAxis.y);
			triggerAxis = new Vector2(currentTriggerAxis.x, currentTriggerAxis.y);

			//Trigger
			if (device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
			{
				OnTriggerClicked(SetButtonEvent(ref triggerPressed, true, currentTriggerAxis.x));
				triggerDown = true;
			}
			else {
				triggerDown = false;
			}

			if (device.GetPressUp(SteamVR_Controller.ButtonMask.Trigger))
			{
				OnTriggerUnclicked(SetButtonEvent(ref triggerPressed, false, 0f));
				triggerUp = true;
			}
			else {
				triggerUp = false;
			}

			//ApplicationMenu
			if (device.GetPressDown(SteamVR_Controller.ButtonMask.ApplicationMenu))
			{
				OnApplicationMenuClicked(SetButtonEvent(ref applicationMenuPressed, true, 1f));

			}
			else if (device.GetPressUp(SteamVR_Controller.ButtonMask.ApplicationMenu))
			{

				OnApplicationMenuUnclicked(SetButtonEvent(ref applicationMenuPressed, false, 0f));

			}

			//Grip
			if (device.GetPressDown(SteamVR_Controller.ButtonMask.Grip))
			{
				OnGripClicked(SetButtonEvent(ref gripPressed, true, 1f));

			}
			else if (device.GetPressUp(SteamVR_Controller.ButtonMask.Grip))
			{
				OnGripUnclicked(SetButtonEvent(ref gripPressed, false, 0f));
			}

     

			//Touchpad Clicked
			if (device.GetPressDown(SteamVR_Controller.ButtonMask.Touchpad))
			{
				OnTouchpadClicked(SetButtonEvent(ref touchpadPressed, true, 1f));

			}
			else if (device.GetPressUp(SteamVR_Controller.ButtonMask.Touchpad))
			{
				OnTouchpadUnclicked(SetButtonEvent(ref touchpadPressed, false, 0f));

			}

			//Touchpad Touched
			if (device.GetTouchDown(SteamVR_Controller.ButtonMask.Touchpad))
			{
				OnTouchpadTouched(SetButtonEvent(ref touchpadTouched, true, 1f));

			}
			else if (device.GetTouchUp(SteamVR_Controller.ButtonMask.Touchpad))
			{
				OnTouchpadUntouched(SetButtonEvent(ref touchpadTouched, false, 0f));

			}
		}
		#endregion 



		#region PRIVATE
		/// <summary>
		/// Compare two vectors if there are about equal. 
		/// </summary>
		bool Vector2ShallowEquals(Vector2 vectorA, Vector2 vectorB)
		{
			return (vectorA.x.ToString("F" + axisFidelity) == vectorB.x.ToString("F" + axisFidelity) &&
				vectorA.y.ToString("F" + axisFidelity) == vectorB.y.ToString("F" + axisFidelity));
		}

        void SteamVRModelLoaded(SteamVR_RenderModel model, bool loaded)
        {
            //find if the controller is touch or vive by the type of its trackpad
            Valve.VR.ETrackedPropertyError pError = new Valve.VR.ETrackedPropertyError();
            int axisprop = Valve.VR.OpenVR.System.GetInt32TrackedDeviceProperty((uint)trackedController.index, Valve.VR.ETrackedDeviceProperty.Prop_Axis0Type_Int32, ref pError);
            controllerType = axisprop == (int)Valve.VR.EVRControllerAxisType.k_eControllerAxis_Joystick ? Type.OCULUS_TOUCH : Type.VIVE;

            //call evenets
            if (ModelLoaded != null) ModelLoaded(this);
        }
        #endregion



            #region PUBLIC
        public void ToggleControllerVisible(bool on)
		{
			foreach (MeshRenderer renderer in this.GetComponentsInChildren<MeshRenderer>())
			{
				renderer.enabled = on;
			}

			foreach (SkinnedMeshRenderer renderer in this.GetComponentsInChildren<SkinnedMeshRenderer>())
			{
				renderer.enabled = on;
			}
			controllerVisible = on;
		}

		/// <summary>
		/// Triggers the haptic pulse.
		/// </summary>
		/// <param name="duration">Duration in frames.</param>
		/// <param name="strength">Strength of the pulse. 100 is barely noticable, 300-500 seems about right for most uses. </param>
		public void TriggerHapticPulse(int duration, ushort strength)
		{
			hapticPulseCountdown = duration;
			hapticPulseStrength = (strength <= maxHapticVibration ? strength : maxHapticVibration);
		}
		#endregion



		#region SETTERS AND GETTERS
        /// <summary>
        /// Is this controller a Vive Controller, Oculush Touch, or other?
        /// </summary>
        public Type ControllerType {
            get { return controllerType; }
        }

        /// <summary>
        /// Returns the pointing direction, based on controller type. Oculus touch controllers point in a slighlty different way than Vive controllers.
        /// </summary>
        public Vector3 PointingDirection {
            get { return controllerType == Type.OCULUS_TOUCH ? transform.localToWorldMatrix.MultiplyVector(Quaternion.Euler(oculusTouchPoitingRotationOffset) * Vector3.forward) : transform.forward; }

        }

        /// <summary>
        /// Returns the pointing origin, based on controller type. Oculus touch controllers point in a slighlty different way than Vive controllers.
        /// </summary>
        public Vector3 PointingOrigin {
            get { return transform.TransformPoint(controllerType == Type.OCULUS_TOUCH ? oculusTouchPoitingOriginOffset : Vector3.zero); }

        }

        /// <summary>
        /// Are the render components of the Controller enabled?
        /// </summary>
        /// <returns><c>true</c> if this instance is controller visible; otherwise, <c>false</c>.</returns>
        public bool IsControllerVisible() { return controllerVisible; }

		/// <summary>
		/// Has trigger been pressed down this frame?
		/// </summary>
		public bool IsTriggerDown { get { return triggerDown; } }
		bool triggerDown = false;

		/// <summary>
		/// Has trigger been released this frame?
		/// </summary>
		public bool IsTriggerUp { get { return triggerUp; } }
		bool triggerUp = false;

		/// <summary>
		/// Is trigger pressed during this frame?
		/// </summary>
		public bool IsTriggerPressed { get { return triggerAxis.x > 0.5f; }  }
		bool triggerPressed = false;

		/// <summary>
		/// Has trigger axis (how hard trigger is pressed) changed this frame?
		/// </summary>
		public bool IsTriggerAxisChanged { get { return triggerAxisChanged; } }
		bool triggerAxisChanged = false;

		/// <summary>
		/// Has user's finger position changed on touchpad this grame?
		/// </summary>
		public bool IsTouchpadAxisChanged { get { return touchpadAxisChanged; } }
		bool touchpadAxisChanged = false;

		/// <summary>
		/// Is Application menu pressed right now?
		/// </summary>
		public bool IsApplicationMenuPressed { get { return applicationMenuPressed; } }
		bool applicationMenuPressed = false;

		/// <summary>
		/// Is touchpad pressed this frame?
		/// </summary>
		public bool IsTouchpadPressed { get { return touchpadPressed; } }
		bool touchpadPressed = false;

		/// <summary>
		/// Is user's finger resting on the touchpad?
		/// </summary>
		public bool IsTouchpadTouched { get { return touchpadTouched; } }
		bool touchpadTouched = false;

		/// <summary>
		/// Is user holding the grip button?
		/// </summary>
		public bool IsGripPressed { get { return gripPressed; } }
		bool gripPressed = false;

        /// <summary>
		/// FingerPosition on touchpad?
		/// </summary>
		public Vector2 TouchPadAxis { get { return touchpadAxis; } }
        #endregion

	

        ViveInputArgs SetButtonEvent(ref bool buttonBool, bool value, float buttonPressure)
		{
			buttonBool = value;
			ViveInputArgs e;
			e.controllerIndex = controllerIndex;
			e.buttonPressure = buttonPressure;
			e.touchpadAxis = device.GetAxis();
			return e;
		}

		#endif 

	}

	public struct ViveInputArgs
	{
		public uint controllerIndex;
		public float buttonPressure;
		public Vector2 touchpadAxis;
	}

	public delegate void ViveInputEvent(object sender, ViveInputArgs e);
    public delegate void ViveEvent(object sender);
}