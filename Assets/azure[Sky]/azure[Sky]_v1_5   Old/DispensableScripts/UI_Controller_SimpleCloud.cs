using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UI_Controller_SimpleCloud : MonoBehaviour {

	public skyControl_v1_5_SimpleCloud skyController;
	
	public Slider dayLuminance;
	public Slider nightLuminance;
	public Slider nightIntensity;
	
	public Slider starsExtinction;
	public Slider starsIntensity;
	
	public Slider moonSize;
	public Slider moonX;
	public Slider moonY;
	public Slider moonShadowIntensity;
	
	public Slider exposure;
	public Slider horizonAltitude;
	
	public InputField time;
	public InputField duration;
	
	private float t;
	private float d;
	
	
	// Update is called once per frame
	void Update()
	{
		// Sky Settings
		skyController.dayLuminance = dayLuminance.value;
		skyController.nightLuminance = nightLuminance.value;
		skyController.nightIntensity = nightIntensity.value;
		
		skyController.starsExtinction = starsExtinction.value;
		skyController.starsIntensity = starsIntensity.value;
		
		skyController.moonSize = moonSize.value;
		skyController.shadowX = moonX.value;
		skyController.shadowY = moonY.value;
		skyController.shadowIntensity = moonShadowIntensity.value;
		
		skyController.exposure = exposure.value;
		skyController.horizonAltitude = horizonAltitude.value;
		
		if (time.text != "")
		{
			t = float.Parse (time.text);
			if (t < 0) {
				t = 0;
				time.text = "0";
			}
			if (t > 24) {
				t = 24;
				time.text = "24";
			}
		}
		
		if (duration.text != "")
		{
			d = float.Parse (duration.text);
			
			if (d < 0) {
				d = 0;
				duration.text = "0";
			}
			if (d > 100) {
				d = 100;
				duration.text = "100";
			}
		}
	}
	
	public void ResetSettings()
	{
		dayLuminance.value = 1.0f;
		nightLuminance.value = 1.35f;
		nightIntensity.value = 0.2f;
		
		starsExtinction.value = 1.5f;
		starsIntensity.value = 2.0f;
		
		moonSize.value = 5.0f;
		moonX.value = 0.0f;
		moonY.value = 0.24f;
		moonShadowIntensity.value = 0.015f;
		
		exposure.value = 1.5f;
		horizonAltitude.value = 0.1f;
	}
	
	public void Set_a_Time()
	{
		if (time.text != "" && duration.text != "")
		{
			t = float.Parse (time.text);
			d = float.Parse (duration.text);
			if (t < 0) {
				t = 0;
				time.text = "0";
			}
			if (t > 24) {
				t = 24;
				time.text = "24";
			}
			
			if (d < 0) {
				d = 0;
				duration.text = "0";
			}
			if (d > 100) {
				d = 100;
				duration.text = "100";
			}
			
			skyController.SetTime (t, d);
		}
	}
}
