using UnityEngine;
using System.Collections;
[AddComponentMenu("azure[Sky]/v1.5/Cloud Animation")]
[RequireComponent (typeof (skyControl_v1_5_AnimatedCloud))]
[ExecuteInEditMode]
public class CloudAnimation : MonoBehaviour
{
	public  Texture2D[] clouds;
	private Texture2D   c1;
	private Texture2D   c2;
	private int 		currentCloud;
	public  float 	    animationSpeed;
	private float       lerp;

	private skyControl_v1_5_AnimatedCloud skyController;


//=======================================================================================================
//-------------------------------------------------------------------------------------------------------
	// Use this for initialization
	void Start()
	{
		skyController = GetComponent<skyControl_v1_5_AnimatedCloud> ();
		currentCloud = 0;
		if (clouds.Length > 1)
		{
			skyController.skyMat.SetTexture ("Cloud1", clouds [currentCloud]);
			skyController.skyMat.SetTexture ("Cloud2", clouds [currentCloud + 1]);
		}
	}

//-------------------------------------------------------------------------------------------------------
	// Update is called once per frame
	void Update()
	{
		if (clouds.Length == 60)
		{
			lerp += animationSpeed * Time.deltaTime;
			if (lerp >= 1.0f) {
				if (currentCloud < 59) {
					currentCloud += 1;
				} else {
					currentCloud = 0;
				}


				if (currentCloud <= 59) {
					skyController.skyMat.SetTexture ("Cloud1", clouds [currentCloud]);
				} else {
					skyController.skyMat.SetTexture ("Cloud1", clouds [0]);
				}
			
				if (currentCloud <= 58) {
					skyController.skyMat.SetTexture ("Cloud2", clouds [currentCloud + 1]);
				} else {
					skyController.skyMat.SetTexture ("Cloud2", clouds [0]);
				}

				lerp = 0.0f;
			}
			skyController.skyMat.SetFloat ("cloudLerp", lerp);
		}

//-------------------------------------------------------------------------------------------------------
		if (clouds.Length > 0)
		{
			// No animation in the editor
			#if UNITY_EDITOR
			if (!Application.isPlaying) {
				skyController.skyMat.SetTexture ("Cloud1", clouds [0]);
				skyController.skyMat.SetTexture ("Cloud2", clouds [0]);
			}
			#endif
		}
	}

//=======================================================================================================
//-------------------------------------------------------------------------------------------------------
	void OnEnable()
	{
		// Warning messages
		#if UNITY_EDITOR
		if (clouds.Length < 60)
		{
			Debug.Log("Missing select all the animation clip textures in the <b>Clouds Animation</b> script. Remaining " + (60 - clouds.Length) + " of 60 textures");
		}
		#endif
	}
}
