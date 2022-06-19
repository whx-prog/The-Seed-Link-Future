using UnityEngine;
using System.Collections;


namespace CurvedUI
{
    public class CUI_PerlinNoisePosition : MonoBehaviour
    {

        public float samplingSpeed = 1;
        public Vector2 Range;

        RectTransform rectie;

        // Use this for initialization
        void Start()
        {
            rectie = transform as RectTransform;
        }

        // Update is called once per frame
        void Update()
        {
            rectie.anchoredPosition = new Vector2(Mathf.PerlinNoise(Time.time * samplingSpeed, Time.time * samplingSpeed).Remap(0, 1, -Range.x, Range.x),
                Mathf.PerlinNoise(Time.time * samplingSpeed * 1.333f, Time.time * samplingSpeed * 0.888f).Remap(0, 1, -Range.y, Range.y));
        }
    }
}
