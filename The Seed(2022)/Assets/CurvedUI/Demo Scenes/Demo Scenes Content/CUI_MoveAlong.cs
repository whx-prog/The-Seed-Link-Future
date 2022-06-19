using UnityEngine;
using System.Collections;

public class CUI_MoveAlong : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		(transform as RectTransform).anchoredPosition = new Vector2((transform as RectTransform).anchoredPosition.x + (transform as RectTransform).anchoredPosition.x / 100.0f, (transform as RectTransform).anchoredPosition.y);

		if((transform as RectTransform).anchoredPosition.x > (transform.parent as RectTransform).rect.width)
			(transform as RectTransform).anchoredPosition = new Vector2(20, (transform as RectTransform).anchoredPosition.y);
			
	}
}
