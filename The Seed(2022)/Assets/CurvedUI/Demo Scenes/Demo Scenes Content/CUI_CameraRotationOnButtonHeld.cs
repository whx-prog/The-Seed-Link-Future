using UnityEngine;
using System.Collections;



public class CUI_CameraRotationOnButtonHeld : MonoBehaviour {

    [SerializeField]
    float Sensitivity = 0.5f;

    Vector3 oldMousePos;
    bool move = true;

	// Use this for initialization
	void Start () {
        oldMousePos = Input.mousePosition;

    }

#if UNITY_EDITOR
    // Update is called once per frame
    void Update() {

        if (Input.GetButton("Fire2"))
        {
            move = true;


        }
        else
            move = false;


        if (move)
        {
            Vector2 mouseDelta = Input.mousePosition - oldMousePos;
            this.transform.eulerAngles = this.transform.eulerAngles + new Vector3(mouseDelta.y, -mouseDelta.x, 0) * Sensitivity;
        }

        oldMousePos = Input.mousePosition;
    }
#endif
}
