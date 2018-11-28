using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityStandardAssets.ImageEffects;

public class UI_v2_0_SimpleCloud : MonoBehaviour
{
	public SkyControl_v2_0_SimpleCloud skyController;
	public Camera mainCamera;
	public Slider timeline;
	public Slider milkyWayVis;
	public Slider milkyWayPow;

	public Toggle sunFlare;
	public Toggle sunShafts;


	// Update is called once per frame
	void Update()
	{
		skyController.azure_TIME_of_DAY = timeline.value;
		skyController.milkyWayIntensity = milkyWayVis.value;
		skyController.milkyWayPower     = milkyWayPow.value;

		skyController.useLensFlare   = sunFlare.isOn;
		foreach( SunShafts shafts in mainCamera.GetComponents<SunShafts>())
		{
			shafts.enabled = sunShafts.isOn;
		}
	}
}