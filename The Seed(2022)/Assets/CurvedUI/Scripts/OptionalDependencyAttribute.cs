
using System;
using System.Diagnostics;

//Used in this project:
//[assembly: CurvedUI.OptionalDependency("TMPro.TextMeshProUGUI", "CURVEDUI_TMP")]
//[assembly: CurvedUI.OptionalDependency("Valve.VR.InteractionSystem.Player", "CURVEDUI_STEAMVR_INT")]

namespace CurvedUI
{
    [Conditional("UNITY_CCU")]
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class OptionalDependencyAttribute : Attribute
    {
        public string dependentClass;
        public string define;

        public OptionalDependencyAttribute(string dependentClass, string define)
        {
            this.dependentClass = dependentClass;
            this.define = define;
        }
    }
}