using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Reflection;


namespace CurvedUI
{
    [CustomEditor(typeof(CurvedUIHandSwitcher))]
    public class CurvedUIHandSwitcherEditor : Editor
    {
        override public void OnInspectorGUI()
        {

            EditorGUILayout.HelpBox("This script moves the Laser Beam to the proper hand of OculusVR or SteamVR rig. Keep it active on the scene.", MessageType.Info);
            EditorGUILayout.HelpBox("The Laser Beam is just a visual guide - it does not handle interactions.", MessageType.Info);

            CurvedUIHandSwitcher manager = (CurvedUIHandSwitcher)target;

            DrawDefaultInspector();
        }
    }

}
