using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UI_EmptySky_v2_0 : MonoBehaviour {

	public EmptySky_v2_0 skyController;
	
	// Update is called once per frame
	void Update ()
	{
		skyController.azure_TIME_of_DAY = GetComponent<Slider> ().value;
	}
}
