using UnityEngine;
using System.Collections;
[AddComponentMenu("azure[Sky]/v2.0/Cloud Animation")]
[RequireComponent (typeof (SkyControl_v2_0_AnimatedCloud))]
[ExecuteInEditMode]
public class CloudAnimation_v2_0 : MonoBehaviour
{
	public  Texture2D[] clouds;
	private Texture2D   c1;
	private Texture2D   c2;
	public  int         iniCloud;
	private int 		currentCloud;
	public  float 	    animationSpeed;
	private float       lerp;
	
	private SkyControl_v2_0_AnimatedCloud skyController;
	
	
	//=======================================================================================================
	//-------------------------------------------------------------------------------------------------------
	// Use this for initialization
	void Start()
	{
		skyController = GetComponent<SkyControl_v2_0_AnimatedCloud> ();
		currentCloud = iniCloud;
		if (clouds.Length > 1)
		{
			skyController.skyMat.SetTexture ("_Cloud1", clouds [currentCloud]);
			skyController.skyMat.SetTexture ("_Cloud2", clouds [currentCloud + 1]);
		}
	}
	
	//-------------------------------------------------------------------------------------------------------
	// Update is called once per frame
	void Update()
	{
		if (clouds.Length == 120)
		{
			lerp += animationSpeed * Time.deltaTime;
			if (lerp >= 1.0f)
			{
				if (currentCloud < 119) {
					currentCloud += 1;
				} else {
					currentCloud = 0;
				}
				
				
				if (currentCloud <= 119) {
					skyController.skyMat.SetTexture ("_Cloud1", clouds [currentCloud]);
				} else {
					skyController.skyMat.SetTexture ("_Cloud1", clouds [0]);
				}
				
				if (currentCloud <= 118) {
					skyController.skyMat.SetTexture ("_Cloud2", clouds [currentCloud + 1]);
				} else {
					skyController.skyMat.SetTexture ("_Cloud2", clouds [0]);
				}
				
				lerp = 0.0f;
			}
			skyController.skyMat.SetFloat ("_cloudLerp", lerp);
		}
		
		//-------------------------------------------------------------------------------------------------------
		// No animation in the editor
		#if UNITY_EDITOR
		if (clouds.Length > 0)
		{
			if (!Application.isPlaying) {
				skyController.skyMat.SetTexture ("_Cloud1", clouds [iniCloud]);
				skyController.skyMat.SetTexture ("_Cloud2", clouds [iniCloud]);
			}
		}
		#endif
	}
	
	//=======================================================================================================
	//-------------------------------------------------------------------------------------------------------
	void OnEnable()
	{
		// Warning messages
		#if UNITY_EDITOR
		if (clouds.Length < 120)
		{
			Debug.Log("Missing select all the animation clip textures in the <b>Clouds Animation</b> script. Remaining " + (60 - clouds.Length) + " of 60 textures");
		}
		#endif
	}

	public void setCloudSpeed(float speed)
	{
		animationSpeed = speed;
	}
}
