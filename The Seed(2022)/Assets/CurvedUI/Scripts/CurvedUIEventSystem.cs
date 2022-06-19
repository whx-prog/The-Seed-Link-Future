using UnityEngine.EventSystems;

namespace CurvedUI
{
    /// <summary>
    /// Fixes the issue where UI input would stop working in VR if the game window looses focus.
    /// </summary>
    public class CurvedUIEventSystem : EventSystem
    {
        public static CurvedUIEventSystem instance;

        protected override void Awake()
        {
            base.Awake();

            instance = this;
        }

        protected override void OnApplicationFocus(bool hasFocus)
        {
            base.OnApplicationFocus(true);
        }
    }

}





