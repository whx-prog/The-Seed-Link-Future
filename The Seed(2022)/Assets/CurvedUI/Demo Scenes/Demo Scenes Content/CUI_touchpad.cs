using UnityEngine;
using System.Collections;
using CurvedUI;

public class CUI_touchpad : MonoBehaviour {

#pragma warning disable 0649
    RectTransform container;
    [SerializeField] RectTransform dot;
#pragma warning restore 0649

#if CURVEDUI_STEAMVR_LEGACY
	CurvedUIViveController controller;

    void Start () {
        controller = CurvedUIInputModule.Right;
       

        //subscribe to event that will be fired every time a finger moves across the touchpad.
        controller.TouchpadAxisChanged += MoveDotOnTouchpadAxisChanged;
	} 

    void Update()
    {
        //show / hide dot when touchpad is touched or not
        dot.gameObject.SetActive(controller.IsTouchpadTouched);

    }
#endif

    void Awake()
    {
        container = this.transform as RectTransform;
    }

    void MoveDotOnTouchpadAxisChanged(object o, ViveInputArgs args)
    {
        //update dot position when touch position changes.
        dot.anchoredPosition = new Vector2(args.touchpadAxis.x * container.rect.width * 0.5f, args.touchpadAxis.y * container.rect.width * 0.5f);

    }
}
