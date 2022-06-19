using UnityEngine;
using System.Collections;

namespace CurvedUI
{
    public class CUI_inventory_paralax : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField]
        Transform front;
        [SerializeField]
        Transform back;
#pragma warning restore 0649

        Vector3 initFG;
        Vector3 initBG;

        public float change = 50;

        // Use this for initialization
        void Start()
        {
            initFG = front.position;
            initBG = back.position;
        }

        // Update is called once per frame
        void Update()
        {

            front.position = front.position.ModifyX(initFG.x + Input.mousePosition.x.Remap(0, Screen.width, -change, change));
            back.position = back.position.ModifyX(initBG.x - Input.mousePosition.x.Remap(0, Screen.width, -change, change));

            front.position = front.position.ModifyY(initFG.y + Input.mousePosition.y.Remap(0, Screen.height, -change, change) * (Screen.height / Screen.width));
            back.position = back.position.ModifyY(initBG.y - Input.mousePosition.y.Remap(0, Screen.height, -change, change) * (Screen.height / Screen.width));

        }
    }
}
