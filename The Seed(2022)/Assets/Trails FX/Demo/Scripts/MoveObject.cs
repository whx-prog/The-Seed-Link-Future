using UnityEngine;

namespace TrailsFX.Demos {

    public class MoveObject : MonoBehaviour
	{


		void Update ()
		{
			Rigidbody rb = GetComponent<Rigidbody> ();
			if (rb == null)
				return;

			Vector3 direction = Vector3.zero;
			if (Input.GetKey (KeyCode.A)) {
				direction = Vector3.right;
			}
			if (Input.GetKey (KeyCode.D)) {
				direction = Vector3.left;
			}
			if (Input.GetKey (KeyCode.W)) {
				direction = Vector3.back;
			}
			if (Input.GetKey (KeyCode.S)) {
				direction = Vector3.forward;
			}
			rb.AddForce (direction * 10);

		}
	}

}