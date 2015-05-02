using UnityEngine;
using System.Collections;

public class SwitchMode : MonoBehaviour 
{
	public void loadClient()
	{
		Application.LoadLevel("client");
	}

	public void loadServer()
	{
		Application.LoadLevel("host");
	}
}
