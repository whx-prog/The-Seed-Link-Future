using UnityEngine;
using System.Collections;

public class CUI_rotation_anim : MonoBehaviour {

	public Vector3 Rotation;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		this.transform.RotateAround(this.transform.position, this.transform.up, Rotation.y * Time.deltaTime);

		this.transform.RotateAround(this.transform.position, this.transform.right, Rotation.x * Time.deltaTime);

		this.transform.RotateAround(this.transform.position, this.transform.forward, Rotation.z * Time.deltaTime);

	}
}
