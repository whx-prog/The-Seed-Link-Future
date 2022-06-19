using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace CurvedUI{

	public static class CurvedUIExtensionMethods
	{


		/// <summary>
		///Direct Vector3 comparison can produce wrong results sometimes due to float inacuracies.
		///This is an aproximate comparison.
		/// <returns></returns>
		public static bool AlmostEqual(this Vector3 a, Vector3 b, double accuracy = 0.01)
		{
			return Vector3.SqrMagnitude(a - b) < accuracy; 
		}

		public static float Remap(this float value, float from1, float to1, float from2, float to2)
		{
			return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
		}

		public static float RemapAndClamp(this float value, float from1, float to1, float from2, float to2)
		{
			return value.Remap(from1, to1, from2, to2).Clamp(from2, to2);
		}


		public static float Remap(this int value, float from1, float to1, float from2, float to2)
		{
			return (value - from1) / (to1 - from1) * (to2 - from2) + from2;

		}

		public static double Remap(this double value, double from1, double to1, double from2, double to2)
		{
			return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
		}



		public static float Clamp(this float value, float min, float max)
		{
			return Mathf.Clamp(value, min, max);
		}

		public static float Clamp(this int value, int min, int max)
		{
			return Mathf.Clamp(value, min, max);
		}

		public static int Abs(this int value)
		{
			return Mathf.Abs(value);
		}

		public static float Abs(this float value)
		{
			return Mathf.Abs(value);
		}


		/// <summary>
		/// Returns value rounded to nearest integer (both up and down).
		/// </summary>
		/// <returns>The int.</returns>
		/// <param name="value">Value.</param>
		public static int ToInt(this float value)
		{
			return Mathf.RoundToInt(value);
		}

		public static int FloorToInt(this float value)
		{
			return Mathf.FloorToInt(value);
		}

		public static int CeilToInt(this float value)
		{
			return Mathf.FloorToInt(value);
		}

		public static Vector3 ModifyX(this Vector3 trans, float newVal)
		{
			trans = new Vector3(newVal, trans.y, trans.z);
			return trans;
		}

		public static Vector3 ModifyY(this Vector3 trans, float newVal)
		{
			trans = new Vector3(trans.x, newVal, trans.z);
			return trans;
		}

		public static Vector3 ModifyZ(this Vector3 trans, float newVal)
		{
			trans = new Vector3(trans.x, trans.y, newVal);
			return trans;
		}

		public static Vector2 ModifyVectorX(this Vector2 trans, float newVal)
		{
			trans = new Vector3(newVal, trans.y);
			return trans;
		}

		public static Vector2 ModifyVectorY(this Vector2 trans, float newVal)
		{
			trans = new Vector3(trans.x, newVal);
			return trans;
		}


		/// <summary>
		/// Resets transform's local position, rotation and scale
		/// </summary>
		/// <param name="trans">Trans.</param>
		public static void ResetTransform(this Transform trans)
		{
			trans.localPosition = Vector3.zero;
			trans.localRotation = Quaternion.identity;
			trans.localScale = Vector3.one;
		}

		public static T AddComponentIfMissing<T>(this GameObject go) where T : Component
		{
			if (go.GetComponent<T>() == null)
			{
				return go.AddComponent<T>();
			}
			else return go.GetComponent<T>();
		}



		/// <summary>
		/// Checks if given component is preset and if not, adds it and returns it.
		/// </summary>
		public static T AddComponentIfMissing<T>(this Component go) where T : Component
		{
			return go.gameObject.AddComponentIfMissing<T>();
		}

	}

}
