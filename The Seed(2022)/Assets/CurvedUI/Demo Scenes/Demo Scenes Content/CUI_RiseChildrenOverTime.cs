using UnityEngine;
using System.Collections;

namespace CurvedUI
{
    public class CUI_RiseChildrenOverTime : MonoBehaviour
    {

        float current = 0;

        public float Speed = 10;
        public float RiseBy = 50;

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            current += Speed * Time.deltaTime;
            if (Mathf.RoundToInt(current) >= this.transform.childCount)
                current = 0;
            if (Mathf.RoundToInt(current) < 0)
                current = this.transform.childCount - 1;

            for (int i = 0; i < this.transform.childCount; i++)
            {
                if (Mathf.RoundToInt(current) == i)
                    this.transform.GetChild(i).localPosition = this.transform.GetChild(i).localPosition.ModifyZ(-RiseBy);
                else
                    this.transform.GetChild(i).localPosition = this.transform.GetChild(i).localPosition.ModifyZ(0);

            }
        }
    }
}
