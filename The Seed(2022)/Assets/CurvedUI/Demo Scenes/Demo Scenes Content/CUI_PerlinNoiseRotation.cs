using UnityEngine;
using System.Collections;

namespace CurvedUI
{
    public class CUI_PerlinNoiseRotation : MonoBehaviour
    {

        public float samplingSpeed = 1;

        public float maxrotation = 60;

        RectTransform rectie;

        // Use this for initialization
        void Start()
        {
            rectie = transform as RectTransform;
        }

        // Update is called once per frame
        void Update()
        {
            rectie.localEulerAngles = new Vector3(0, 0, Mathf.PerlinNoise(Time.time * samplingSpeed, Time.time * samplingSpeed).Remap(0, 1, -maxrotation, maxrotation));

        }
    }
}
