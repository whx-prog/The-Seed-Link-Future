using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UI;


namespace CurvedUI { 

	[CustomEditor(typeof(CurvedUIInputModule))]
	public class CurvedUIInputModuleEditor : Editor {

        bool opened = false;


#if CURVEDUI_GOOGLEVR
        bool isGVR = true;
#else
        bool isGVR = false;
#endif


        void OnEnable()
        {
            CurvedUIInputModule myTarget = (CurvedUIInputModule)target;


#if CURVEDUI_OCULUSVR
            //automatically find Oculus Rig, if possible
            if (myTarget.OculusCameraRig == null)
                myTarget.OculusCameraRig = Object.FindObjectOfType<OVRCameraRig>();

#elif CURVEDUI_STEAMVR_LEGACY
            //automatically find SteamVR Rig, if possible
            if (myTarget.SteamVRControllerManager == null)
            myTarget.SteamVRControllerManager = Object.FindObjectOfType<SteamVR_ControllerManager>();          
#elif CURVEDUI_STEAMVR_2
            //automatically find SteamVR Rig, if possible
            if (myTarget.SteamVRPlayArea == null)
                myTarget.SteamVRPlayArea = FindObjectOfType<Valve.VR.SteamVR_PlayArea>();
#endif
        }


        public override void OnInspectorGUI()
		{
            EditorGUILayout.HelpBox("Use CurvedUISettings component on your Canvas to configure CurvedUI", MessageType.Info);


            if (isGVR)//on GVR we draw all the stuff.
            {
                DrawDefaultInspector();
            }
            else
            {
                if (opened)
                {
                    if (GUILayout.Button("Hide Fields"))
                        opened = !opened;

                    DrawDefaultInspector();
                }
                else
                {
                    if (GUILayout.Button("Show Fields"))
                        opened = !opened;
                }
            }
       
            GUILayout.Space(20);
        }
	}

}
