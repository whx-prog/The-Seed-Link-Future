using UnityEngine;

namespace TrailsFX.Demos {

    public class RotateObject : MonoBehaviour
	{

		public float speed = 100f;

		Vector3 eulerAngles;

		void Start ()
		{
			SetAngles ();
		}

		void Update ()
		{
			transform.Rotate (eulerAngles * (Time.deltaTime * speed));
			if (Random.value > 0.995f) {
				SetAngles ();
			}
		}

		void SetAngles ()
		{
			eulerAngles = new Vector3 (Random.value - 0.5f, Random.value - 0.5f, Random.value - 0.5f);
		}
	}

}