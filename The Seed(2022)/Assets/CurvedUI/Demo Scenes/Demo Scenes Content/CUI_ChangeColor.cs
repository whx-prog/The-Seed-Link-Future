using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

namespace CurvedUI
{
	public class CUI_ChangeColor : MonoBehaviour
	{
	
		public void ChangeColorToBlue()
		{
			this.GetComponent<Renderer>().material.color = Color.blue;
		}
	
		public void ChangeColorToCyan()
		{
			this.GetComponent<Renderer>().material.color = Color.cyan;
		}
	
		public void ChangeColorToWhite()
		{
			this.GetComponent<Renderer>().material.color = Color.white;
		}
	

	}
}

