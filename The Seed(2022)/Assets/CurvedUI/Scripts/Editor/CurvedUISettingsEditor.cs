using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine.EventSystems;
#if CURVEDUI_TMP || TMP_PRESENT
using TMPro;
#endif 

#if CURVEDUI_STEAMVR_2
using Valve.VR;
using System.Reflection;
using System;
#endif 


namespace CurvedUI { 
 

[CustomEditor(typeof(CurvedUISettings))]
    [ExecuteInEditMode]
    public class CurvedUISettingsEditor : Editor {

#pragma warning disable 414
        bool ShowRemoveCurvedUI = false;
        static bool ShowAdvaced = false;
		bool loadingCustomDefine = false;
        static bool CUIeventSystemPresent = false;
        private bool inPrefabMode = false;

        [SerializeField][HideInInspector]
        Dictionary<CurvedUIInputModule.CUIControlMethod, string> m_controlMethodDefineDict;

#if CURVEDUI_STEAMVR_2
        SteamVR_Action_Boolean[] steamVRActions;
        string[] steamVRActionsPaths;
#endif

#pragma warning restore 414



        #region LIFECYCLE
        void Awake()
		{
			AddCurvedUIComponents();
        }

        void OnEnable()
		{
            //if we're firing OnEnable, this means any compilation has ended. We're good!
            loadingCustomDefine = false;

            //look for CurvedUI custom EventSystem -> we'll check later if it makes sense to have one.
            CUIeventSystemPresent = (FindObjectsOfType(typeof(CurvedUIEventSystem)).Length > 0);
            
            //hacky way to make sure event is connected only once, but it works!
#if UNITY_2018_1_OR_NEWER
            EditorApplication.hierarchyChanged -= AddCurvedUIComponents;
            EditorApplication.hierarchyChanged -= AddCurvedUIComponents;
            EditorApplication.hierarchyChanged += AddCurvedUIComponents;
#else
            //hacky way to make sure event is connected only once, but it works!
            EditorApplication.hierarchyWindowChanged -= AddCurvedUIComponents;
            EditorApplication.hierarchyWindowChanged -= AddCurvedUIComponents;
            EditorApplication.hierarchyWindowChanged += AddCurvedUIComponents;
#endif


            //Warnings-------------------------------------//
            if (Application.isPlaying)
            {
                CurvedUISettings myTarget = (CurvedUISettings)target;
                
                //Canvas' layer not included in RaycastLayerMask warning
                if (!IsInLayerMask(myTarget.gameObject.layer, CurvedUIInputModule.Instance.RaycastLayerMask) &&
                    myTarget.Interactable)
                {
                    Debug.LogError("CURVEDUI: " + WarningLayerNotIncluded, myTarget.gameObject);
                }
                
                //check if the currently selected control method is enabled in editor.
                //Otherwise, show error.
                string define = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
                foreach(var key in ControlMethodDefineDict.Keys)
                {
                    if(myTarget.ControlMethod == key && !define.Contains(ControlMethodDefineDict[key]))
                        Debug.LogError("CURVEDUI: Selected control method (" + key.ToString() + ") is not enabled. " +
                                       "Enable it on CurvedUISettings component", myTarget.gameObject);
                }
            }



#if CURVEDUI_STEAMVR_2
            //Get action and their paths to show in the popup.
            steamVRActions = SteamVR_Input.GetActions<SteamVR_Action_Boolean>();
            steamVRActionsPaths = new string[] { "None" };
            if (steamVRActions != null && steamVRActions.Length > 0)
            {
                List<string> enumList = new List<string>();

                //add all action paths to list.
                for (int i = 0; i < steamVRActions.Length; i++)
                    enumList.Add(steamVRActions[i].fullPath);

                enumList.Add("None"); //need a way to null that field, so add None as last pick.

                //replace forward slashes with backslack instead. Otherwise they will not show up.
                for (int index = 0; index < enumList.Count; index++)
                    enumList[index] = enumList[index].Replace('/', '\\');

                steamVRActionsPaths = enumList.ToArray();
            }
#endif
        }
#endregion



            
        public override void OnInspectorGUI()
        {
     
       


            //initial settings------------------------------------//
            CurvedUISettings myTarget = (CurvedUISettings)target;
            if (target == null) return;
            GUI.changed = false;
            EditorGUIUtility.labelWidth = 150;
            #if UNITY_2018_3_OR_NEWER
            inPrefabMode = (UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage() != null);
            #endif


            //Version----------------------------------------------//
            GUILayout.Label("Version " + CurvedUISettings.Version, EditorStyles.miniLabel);

            
            //Warnings------------------------------------------//
            //Canvas is on layer that is not part of RaycastLayerMask
            if(!IsInLayerMask(myTarget.gameObject.layer, CurvedUIInputModule.Instance.RaycastLayerMask) && myTarget.Interactable)
            {
                EditorGUILayout.HelpBox(WarningLayerNotIncluded, MessageType.Error);
                GUILayout.Space(30);
            }
            
            //Improper event system warning
            if (CUIeventSystemPresent == false)
            {
                EditorGUILayout.HelpBox("Unity UI may become unresponsive in VR if game window loses focus. " +
                                        "Use CurvedUIEventSystem instead of standard EventSystem component to solve this issue.",
                                        MessageType.Warning);
                GUILayout.BeginHorizontal();
                GUILayout.Space(146);
                if (GUILayout.Button("Use CurvedUI Event System")) SwapEventSystem();
                GUILayout.EndHorizontal();
                GUILayout.Space(30);
            }
            //-----------------------------------------------------//    
             
            
            //Control methods--------------------------------------//
            DrawControlMethods();


            //shape settings----------------------------------------//
            GUILayout.Label("Shape", EditorStyles.boldLabel);
            myTarget.Shape = (CurvedUISettings.CurvedUIShape)EditorGUILayout.EnumPopup("Canvas Shape", myTarget.Shape);
            switch (myTarget.Shape)
            {
                case CurvedUISettings.CurvedUIShape.CYLINDER:
                {
                    myTarget.Angle = EditorGUILayout.IntSlider("Angle", myTarget.Angle, -360, 360);
                    myTarget.PreserveAspect = EditorGUILayout.Toggle("Preserve Aspect", myTarget.PreserveAspect);

                    break;
                }
                case CurvedUISettings.CurvedUIShape.CYLINDER_VERTICAL:
                {
                    myTarget.Angle = EditorGUILayout.IntSlider("Angle", myTarget.Angle, -360, 360);
                    myTarget.PreserveAspect = EditorGUILayout.Toggle("Preserve Aspect", myTarget.PreserveAspect);

                    break;
                }
                case CurvedUISettings.CurvedUIShape.RING:
                {
                    myTarget.RingExternalDiameter = Mathf.Clamp(EditorGUILayout.IntField("External Diameter", myTarget.RingExternalDiameter), 1, 100000);
                    myTarget.Angle = EditorGUILayout.IntSlider("Angle", myTarget.Angle, 0, 360);
                    myTarget.RingFill = EditorGUILayout.Slider("Fill", myTarget.RingFill, 0.0f, 1.0f);
                    myTarget.RingFlipVertical = EditorGUILayout.Toggle("Flip Canvas Vertically", myTarget.RingFlipVertical);
                    break;
                }
                case CurvedUISettings.CurvedUIShape.SPHERE:
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(150);
                    EditorGUILayout.HelpBox("Sphere shape is more expensive than a Cylinder shape. Keep this in mind when working on mobile VR.", MessageType.Info);
                    GUILayout.EndHorizontal();
                    GUILayout.Space(10);

                    if (myTarget.PreserveAspect)
                    {
                        myTarget.Angle = EditorGUILayout.IntSlider("Angle", myTarget.Angle, -360, 360);
                    }
                    else {
                        myTarget.Angle = EditorGUILayout.IntSlider("Horizontal Angle", myTarget.Angle, 0, 360);
                        myTarget.VerticalAngle = EditorGUILayout.IntSlider("Vertical Angle", myTarget.VerticalAngle, 0, 180);
                    }

                    myTarget.PreserveAspect = EditorGUILayout.Toggle("Preserve Aspect", myTarget.PreserveAspect);
                    break;
                }
            }//end of shape settings-------------------------------//



            //180 degree warning ----------------------------------//
            if ((myTarget.Shape != CurvedUISettings.CurvedUIShape.RING && myTarget.Angle.Abs() > 180) ||
                (myTarget.Shape == CurvedUISettings.CurvedUIShape.SPHERE && myTarget.VerticalAngle > 180))
                Draw180DegreeWarning();



            //advanced settings------------------------------------//
            GUILayout.Space(30);
            if (!ShowAdvaced)
            {              
                if (GUILayout.Button("Show Advanced Settings"))
                {
                    ShowAdvaced = true;
                    loadingCustomDefine = false;
                }
            }
            else
            {            
                //hide advances settings button.
                if (GUILayout.Button("Hide Advanced Settings")) ShowAdvaced = false;
                GUILayout.Space(20);
                
                //InputModule Options - only if we're not in prefab mode.
                if (!inPrefabMode)
                {
                    CurvedUIInputModule.Instance.RaycastLayerMask = LayerMaskField("Raycast Layer Mask",
                        CurvedUIInputModule.Instance.RaycastLayerMask);
                
                    //pointer override
                    GUILayout.Space(20);
                    CurvedUIInputModule.Instance.PointerTransformOverride = (Transform)EditorGUILayout.ObjectField("Pointer Override", CurvedUIInputModule.Instance.PointerTransformOverride, typeof(Transform), true);
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(150);
                    GUILayout.Label("(Optional) If set, its position and forward (blue) direction will be used to point at canvas.", EditorStyles.helpBox);
                    GUILayout.EndHorizontal();
                }
                else
                {
                    EditorGUILayout.HelpBox("Some settings are hidden in Prefab Mode", MessageType.Warning);
                }
                
                //quality
                GUILayout.Space(20);
                myTarget.Quality = EditorGUILayout.Slider("Quality", myTarget.Quality, 0.1f, 3.0f);
                GUILayout.BeginHorizontal();
                GUILayout.Space(150);
                GUILayout.Label("Smoothness of the curve. Bigger values mean more subdivisions. Decrease for better performance. Default 1", EditorStyles.helpBox);
                GUILayout.EndHorizontal();
                
                //common options
                myTarget.Interactable = EditorGUILayout.Toggle("Interactable", myTarget.Interactable);
                myTarget.BlocksRaycasts = EditorGUILayout.Toggle("Blocks Raycasts", myTarget.BlocksRaycasts);
                if (myTarget.Shape != CurvedUISettings.CurvedUIShape.SPHERE) myTarget.ForceUseBoxCollider = 
                    EditorGUILayout.Toggle("Force Box Colliders Use", myTarget.ForceUseBoxCollider);
                
                //add components button
                GUILayout.Space(20);
                GUILayout.BeginHorizontal();
                GUILayout.Label("Components", GUILayout.Width(146));
                if (GUILayout.Button("Add Curved Effect To Children")) AddCurvedUIComponents();
                GUILayout.EndHorizontal();

                //remove components button
                GUILayout.BeginHorizontal();
                GUILayout.Label("", GUILayout.Width(146));
                if (!ShowRemoveCurvedUI)
                {
                    if (GUILayout.Button("Remove CurvedUI from Canvas")) ShowRemoveCurvedUI = true;
                }
                else {
                    if (GUILayout.Button("Remove CurvedUI"))   RemoveCurvedUIComponents();
                    if (GUILayout.Button("Cancel"))   ShowRemoveCurvedUI = false;
                }
                GUILayout.EndHorizontal();

                //documentation link
                GUILayout.Space(20);
                GUILayout.BeginHorizontal();
                GUILayout.Label("Documentation", GUILayout.Width(146));
                if (GUILayout.Button("Open in web browser")) Help.BrowseURL("https://docs.google.com/document/d/10hNcvOMissNbGgjyFyV1MS7HwkXXE6270A6Ul8h8pnQ/edit");
                GUILayout.EndHorizontal();

            }//end of Advanced settings---------------------------//

            GUILayout.Space(20);

            //final settings
            if (GUI.changed && myTarget != null)
                EditorUtility.SetDirty(myTarget);
        }




#region CUSTOM GUI ELEMENTS
        void DrawControlMethods()
        {
            GUILayout.Label("Global Settings", EditorStyles.boldLabel);

            //Do not allow to change Global Settings in Prefab Mode
            //These are stored in CurvedUInputModule
            if (inPrefabMode)
            {
                EditorGUILayout.HelpBox(
                    "Some global settings (including Control Method) are hidden in Prefab Mode. These are stored on CurvedUIInputModule component which cannot be accessed now.",
                    MessageType.Warning);
                return;
            }

            //Control Method dropdown--------------------------------//
            CurvedUIInputModule.ControlMethod = (CurvedUIInputModule.CUIControlMethod)EditorGUILayout.EnumPopup("Control Method", CurvedUIInputModule.ControlMethod);
            GUILayout.BeginHorizontal();
            GUILayout.Space(150);
			GUILayout.BeginVertical();


            //Custom Settings for each Control Method---------------//
            switch (CurvedUIInputModule.ControlMethod)
            {


                case CurvedUIInputModule.CUIControlMethod.MOUSE:
                {
#if CURVEDUI_GOOGLEVR
					EditorGUILayout.HelpBox("Enabling this control method will disable GoogleVR support.", MessageType.Warning);
					DrawCustomDefineSwitcher("");
#else
                    GUILayout.Label("Basic Controller. Mouse on screen", EditorStyles.helpBox);
#endif
                    break;
                }// end of MOUSE



                case CurvedUIInputModule.CUIControlMethod.GAZE:
                {
#if CURVEDUI_GOOGLEVR
					EditorGUILayout.HelpBox("Enabling this control method will disable GoogleVR support.", MessageType.Warning);
					DrawCustomDefineSwitcher("");
#else
                    GUILayout.Label("Center of Canvas's Event Camera acts as a pointer. Can be used with any headset. If you're on cardboard, you can use it instead of GOOGLEVR control method.", EditorStyles.helpBox);
                    CurvedUIInputModule.Instance.GazeUseTimedClick = EditorGUILayout.Toggle("Use Timed Click", CurvedUIInputModule.Instance.GazeUseTimedClick);
                    if (CurvedUIInputModule.Instance.GazeUseTimedClick)
                    {
                        GUILayout.Label("Clicks a button if player rests his gaze on it for a period of time. You can assign an image to be used as a progress bar.", EditorStyles.helpBox);
                        CurvedUIInputModule.Instance.GazeClickTimer = EditorGUILayout.FloatField("Click Timer (seconds)", CurvedUIInputModule.Instance.GazeClickTimer);
                        CurvedUIInputModule.Instance.GazeClickTimerDelay = EditorGUILayout.FloatField("Timer Start Delay", CurvedUIInputModule.Instance.GazeClickTimerDelay);
                        CurvedUIInputModule.Instance.GazeTimedClickProgressImage = (UnityEngine.UI.Image)EditorGUILayout.ObjectField("Progress Image To FIll", CurvedUIInputModule.Instance.GazeTimedClickProgressImage, typeof(UnityEngine.UI.Image), true);
                    }
#endif
                    break;
                }// end of GAZE



                case CurvedUIInputModule.CUIControlMethod.WORLD_MOUSE:
                {

#if CURVEDUI_GOOGLEVR
					EditorGUILayout.HelpBox("Enabling this control method will disable GoogleVR support.", MessageType.Warning);
					DrawCustomDefineSwitcher("");
#else
                    GUILayout.Label("Mouse controller that is independent of the camera view. Use WorldSpaceMouseOnCanvas function to get its position.", EditorStyles.helpBox);
                    CurvedUIInputModule.Instance.WorldSpaceMouseSensitivity = EditorGUILayout.FloatField("Mouse Sensitivity", CurvedUIInputModule.Instance.WorldSpaceMouseSensitivity);
#endif
					break;
                }// end of WORLD_MOUSE



                case CurvedUIInputModule.CUIControlMethod.CUSTOM_RAY:
                {
#if CURVEDUI_GOOGLEVR
					EditorGUILayout.HelpBox("Enabling this control method will disable GoogleVR support.", MessageType.Warning);
					DrawCustomDefineSwitcher("");
#else
                    GUILayout.Label("Set a ray used to interact with canvas using CustomControllerRay function. Use CustomControllerButtonState bool to set button pressed state. Find both in CurvedUIInputModule class", EditorStyles.helpBox);
                    GUILayout.BeginHorizontal();
                    //GUILayout.Space(20);
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("View code snippet")) Help.BrowseURL("https://docs.google.com/document/d/10hNcvOMissNbGgjyFyV1MS7HwkXXE6270A6Ul8h8pnQ/edit#heading=h.b164qm67xp15");
                    GUILayout.EndHorizontal();
#endif
                    break;
                }// end of CUSTOM_RAY



                case CurvedUIInputModule.CUIControlMethod.STEAMVR_LEGACY:
                {
#if CURVEDUI_STEAMVR_LEGACY
                    // vive enabled, we can show settings
                    GUILayout.Label("Use with SteamVR plugin 1.2 or below. Trigger acts a button", EditorStyles.helpBox);
                    CurvedUIInputModule.Instance.UsedHand = (CurvedUIInputModule.Hand)EditorGUILayout.EnumPopup("Used Controller", CurvedUIInputModule.Instance.UsedHand);

#else
                    GUILayout.Label("For SteamVR plugin 1.2 or below.  Make sure you imported that SDK before enabling.", EditorStyles.helpBox);
                    DrawCustomDefineSwitcher(ControlMethodDefineDict[CurvedUIInputModule.CUIControlMethod.STEAMVR_LEGACY]);
#endif
                    break;
                }// end of STEAMVR_LEGACY



                case CurvedUIInputModule.CUIControlMethod.STEAMVR_2:
                {
#if CURVEDUI_STEAMVR_2
					GUILayout.Label("Use SteamVR controllers to interact with canvas. Requires SteamVR Plugin 2.0 or later.", EditorStyles.helpBox);
                   

                    if(steamVRActions != null)
                    {
                        CurvedUIInputModule.Instance.UsedHand = (CurvedUIInputModule.Hand)EditorGUILayout.EnumPopup("Hand", CurvedUIInputModule.Instance.UsedHand);

                        //Find currently selected action in CurvedUIInputModule
                        int curSelected = steamVRActionsPaths.Length - 1;
                        for (int i = 0; i < steamVRActions.Length; i++)
                        {
                            //no action selected? select one that most likely deals with UI
                            if(CurvedUIInputModule.Instance.SteamVRClickAction == null && steamVRActions[i].GetShortName().Contains("UI"))
                                CurvedUIInputModule.Instance.SteamVRClickAction = steamVRActions[i];

                            //otherwise show currently selected
                            if (steamVRActions[i] == CurvedUIInputModule.Instance.SteamVRClickAction) //otherwise show selected
                                curSelected = i;
                        }

                        //Show popup
                        int newSelected = EditorGUILayout.Popup("Click With", curSelected, steamVRActionsPaths, EditorStyles.popup);

                        //assign selected SteamVR Action to CurvedUIInputMOdule
                        if (curSelected != newSelected)
                        {
                            //none has been selected
                            if (newSelected >= steamVRActions.Length)
                                CurvedUIInputModule.Instance.SteamVRClickAction = null;
                            else
                                CurvedUIInputModule.Instance.SteamVRClickAction = steamVRActions[newSelected];
                        }
                    }
                    else
                    {
                        //draw error
                        EditorGUILayout.HelpBox("No SteamVR Actions set up. Configure your SteamVR plugin first in Window > Steam VR Input", MessageType.Error);

                    }

#else
                    GUILayout.Label("For SteamVR plugin 2.0 or above. Make sure you imported that SDK before enabling.", EditorStyles.helpBox);
                    DrawCustomDefineSwitcher(ControlMethodDefineDict[CurvedUIInputModule.CUIControlMethod.STEAMVR_2]);
#endif
                    break;
                }// end of STEAMVR_2



                case CurvedUIInputModule.CUIControlMethod.OCULUSVR:
                {
#if CURVEDUI_OCULUSVR
                    // oculus enabled, we can show settings
                    GUILayout.Label("Use Rift, Quest, Go, or GearVR controller to interact with your canvas.", EditorStyles.helpBox);
                    //hand property
                    CurvedUIInputModule.Instance.UsedHand = (CurvedUIInputModule.Hand)EditorGUILayout.EnumPopup("Hand", CurvedUIInputModule.Instance.UsedHand);
                    //button property
                    CurvedUIInputModule.Instance.OculusTouchInteractionButton = (OVRInput.Button)EditorGUILayout.EnumPopup("Interaction Button", CurvedUIInputModule.Instance.OculusTouchInteractionButton);
#else
                    GUILayout.Label("Make sure you imported the SDK before enabling.", EditorStyles.helpBox);
                    DrawCustomDefineSwitcher(ControlMethodDefineDict[CurvedUIInputModule.CUIControlMethod.OCULUSVR]);
#endif
                    break;
                }// end of OCULUSVR
                    
                    
                case CurvedUIInputModule.CUIControlMethod.UNITY_XR:
                {
#if CURVEDUI_UNITY_XR
                    // oculus enabled, we can show settings
                    GUILayout.Label("Use Unity XR Toolkit to interact with the canvas. You can choose UI button on the XRController itself.", EditorStyles.helpBox);
                    //hand property
                    CurvedUIInputModule.Instance.UsedHand = (CurvedUIInputModule.Hand)EditorGUILayout.EnumPopup("Hand", CurvedUIInputModule.Instance.UsedHand);
                    //assign controllers
                    CurvedUIInputModule.Instance.RightXRController = (UnityEngine.XR.Interaction.Toolkit.XRController)EditorGUILayout.ObjectField("Right Controller",
                        CurvedUIInputModule.Instance.RightXRController, typeof(UnityEngine.XR.Interaction.Toolkit.XRController), true);
                    CurvedUIInputModule.Instance.LeftXRController = (UnityEngine.XR.Interaction.Toolkit.XRController)EditorGUILayout.ObjectField("Left Controller",
                        CurvedUIInputModule.Instance.LeftXRController, typeof(UnityEngine.XR.Interaction.Toolkit.XRController), true);
#else
                    GUILayout.Label("Make sure you imported Unity XR Toolkit before enabling.", EditorStyles.helpBox);
                    DrawCustomDefineSwitcher(ControlMethodDefineDict[CurvedUIInputModule.CUIControlMethod.UNITY_XR]);
#endif
                    break;
                }// end of UNITY_XR



                case CurvedUIInputModule.CUIControlMethod.GOOGLEVR:
				{
#if CURVEDUI_GOOGLEVR
					GUILayout.Label("Use GoogleVR Reticle to interact with canvas. Requires GoogleVR SDK 1.200.1 or later.  Make sure you imported the SDK before enabling.", EditorStyles.helpBox);
#else
                    GUILayout.Label("Make sure you imported the GoogleVR SDK before enabling.", EditorStyles.helpBox);
                    DrawCustomDefineSwitcher(ControlMethodDefineDict[CurvedUIInputModule.CUIControlMethod.GOOGLEVR]);
#endif
                    break;
                }// end of GOOGLEVR


            }//end of CUIControlMethod Switch

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            GUILayout.Space(20);
        }



		/// <summary>
		/// Draws the define switcher for different control methods. 
		/// Because different control methods use different API's that may not always be available, 
		/// CurvedUI needs to be recompile with different custom defines to fix this. This method 
		/// manages the defines.
		/// </summary>
		/// <param name="defineToSet">Switcho.</param>
		void DrawCustomDefineSwitcher(string defineToSet)
		{
			GUILayout.BeginVertical();
			GUILayout.Label("Press the [Enable] button to recompile scripts for this control method. Afterwards, you'll see more settings here.", EditorStyles.helpBox);

			GUILayout.BeginHorizontal();
			GUILayout.Space(50);
			if (GUILayout.Button(loadingCustomDefine ? "Please wait..." : "Enable."))
			{
				loadingCustomDefine = true;

                //retrieve current defines
                string str = "";
				str += PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);

                //remove unused curvedui defines - dictionary based.
                string define = "";
                foreach (var key in ControlMethodDefineDict.Keys)
                {
                    define = ControlMethodDefineDict[key];

                    if (str.Contains(define))
                    {
                        if (str.Contains((";" + define)))
                            str = str.Replace((";" + define), "");
                        else
                            str = str.Replace(define, "");
                    }
                }
        
                //add this one, if not present.
                if (defineToSet != "" && !str.Contains(defineToSet))
                    str += ";" + defineToSet;

                //Submit defines. This will cause recompilation
                PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, str);         
			}
			GUILayout.EndHorizontal();
			GUILayout.EndVertical();
        }

        void Draw180DegreeWarning()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(150);
            EditorGUILayout.HelpBox("Cavas with angle bigger than 180 degrees will not be interactable. \n" +
                "This is caused by Unity Event System requirements. Use two canvases facing each other for fully interactive 360 degree UI.", MessageType.Warning);
            GUILayout.EndHorizontal();
            GUILayout.Space(10);
        }
#endregion




#region HELPER FUNCTIONS
        static List<string> layers;
        static string[] layerNames;

        public static LayerMask LayerMaskField (string label, LayerMask selected)
        {
            if (layers == null) {
                layers = new List<string>();
                layerNames = new string[4];
            } else {
                layers.Clear ();
            }
                     
            int emptyLayers = 0;
            for (int i=0;i<32;i++) {
                string layerName = LayerMask.LayerToName (i);
                         
                if (layerName != "") {
                             
                    for (;emptyLayers>0;emptyLayers--) layers.Add ("Layer "+(i-emptyLayers));
                    layers.Add (layerName);
                } else {
                    emptyLayers++;
                }
            }
                     
            if (layerNames.Length != layers.Count) {
                layerNames = new string[layers.Count];
            }
            for (int i=0;i<layerNames.Length;i++) layerNames[i] = layers[i];
                 
            selected.value =  EditorGUILayout.MaskField (label,selected.value,layerNames);
                 
            return selected;
        }
        
        bool IsInLayerMask(int layer, LayerMask layermask)
        {
            return layermask == (layermask | (1 << layer));
        }

        Dictionary<CurvedUIInputModule.CUIControlMethod, string> ControlMethodDefineDict {
            get
            {
                if (m_controlMethodDefineDict == null)
                {
                    m_controlMethodDefineDict = new Dictionary<CurvedUIInputModule.CUIControlMethod, string>();
                    m_controlMethodDefineDict.Add(CurvedUIInputModule.CUIControlMethod.GOOGLEVR, "CURVEDUI_GOOGLEVR");
                    m_controlMethodDefineDict.Add(CurvedUIInputModule.CUIControlMethod.STEAMVR_LEGACY, "CURVEDUI_STEAMVR_LEGACY");
                    m_controlMethodDefineDict.Add(CurvedUIInputModule.CUIControlMethod.STEAMVR_2, "CURVEDUI_STEAMVR_2");
                    m_controlMethodDefineDict.Add(CurvedUIInputModule.CUIControlMethod.OCULUSVR, "CURVEDUI_OCULUSVR");
                    m_controlMethodDefineDict.Add(CurvedUIInputModule.CUIControlMethod.UNITY_XR, "CURVEDUI_UNITY_XR");
                }
                return m_controlMethodDefineDict;
            }
        }

             

    void SwapEventSystem()
        {
            if (Application.isPlaying)
            {
                Debug.LogError("Cant do this in Play mode!");
                return;
            }

            EventSystem system = FindObjectOfType<EventSystem>();
            if (!(system is CurvedUIEventSystem))
            {
                system.AddComponentIfMissing<CurvedUIEventSystem>();
                DestroyImmediate(system);
            }

            CUIeventSystemPresent = true;
        }

        /// <summary>
        ///Travel the hierarchy and add CurvedUIVertexEffect to every gameobject that can be bent.
        /// </summary>
        private void AddCurvedUIComponents()
        {
            if (target == null) return;
            (target as CurvedUISettings).AddEffectToChildren();
        }



		/// <summary>
		/// Removes all CurvedUI components from this canvas.
		/// </summary>
        private void RemoveCurvedUIComponents()
   		{
	        if (target == null) return;

            //destroy TMP objects
            List<CurvedUITMP> tmps = new List<CurvedUITMP>();
            tmps.AddRange((target as CurvedUISettings).GetComponentsInChildren<CurvedUITMP>(true));
            for (int i = 0; i < tmps.Count; i++)
            {
                DestroyImmediate(tmps[i]);
            }

            List<CurvedUITMPSubmesh> submeshes = new List<CurvedUITMPSubmesh>();
            submeshes.AddRange((target as CurvedUISettings).GetComponentsInChildren<CurvedUITMPSubmesh>(true));
            for (int i = 0; i < submeshes.Count; i++)
            {
                DestroyImmediate(submeshes[i]);
            }

            //destroy curving componenets
            List<CurvedUIVertexEffect> comps = new List<CurvedUIVertexEffect>();
	        comps.AddRange((target as CurvedUISettings).GetComponentsInChildren<CurvedUIVertexEffect>(true));
	        for (int i = 0; i < comps.Count; i++)
	        {
	            if (comps[i].GetComponent<UnityEngine.UI.Graphic>() != null) comps[i].GetComponent<UnityEngine.UI.Graphic>().SetAllDirty();
	            DestroyImmediate(comps[i]);            
	        }
  

            //destroy raycasters
            List<CurvedUIRaycaster> raycasters = new List<CurvedUIRaycaster>();
	        raycasters.AddRange((target as CurvedUISettings).GetComponents<CurvedUIRaycaster>());
	        for (int i = 0; i < raycasters.Count; i++)
	        {
	            DestroyImmediate(raycasters[i]);
	        }

	        DestroyImmediate(target);
   		}
#endregion

#region STRINGS
    private static string WarningLayerNotIncluded = "This Canvas' layer " +
                                                "is not included in the RaycastLayerMask. User will not be able to interact with it. " +
                                                "Add its layer to RaycastLayerMask below to fix it, or set the " +
                                                "Interactable property to False to dismiss this message.";
#endregion

    }
}

