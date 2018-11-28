using UnityEngine;
using System.Collections;
[AddComponentMenu("azure[Sky]/v1.5/Sky Control Simple Cloud")]
[ExecuteInEditMode]
public class skyControl_v1_5_SimpleCloud : MonoBehaviour
{
	//  DECLARATION OF VARIABLEs
//-------------------------------------------------------------------------------------------------------
	//Used in the Editor Script only
	public bool showObjMats     = false;
	public bool showScattering  = false;
	public bool showTextures    = false;
	public bool showSkyCustom   = false;
	public bool showFog         = false;
	public bool showTime_of_Day = false;
	public bool showOptions     = false;
//-------------------------------------------------------------------------------------------------------
	// GameObjects
	public GameObject SUN;
	public GameObject MOON;
	public Material   skyMat;
	public Material   fogMat;
//-------------------------------------------------------------------------------------------------------
	// For scattering equations
	public float RayCoeff      =  1.0f;  // Rayleigh coefficient
	public float MieCoeff      =  1.0f;  // Mie coefficient
	public float Turbidity     =  1.0f;
	public float g             =  0.75f; // Directionality factor
	
																	   // Original value of lambda"(wavelength)" is Vector3(680e-9f, 550e-9f, 450e-9f).
	public Vector3 lambda      =  new Vector3(650.0f, 570.0f, 475.0f); // Used integers for customization in the Inspector, will need...
																	   //convert to original scale before using in the equations, multiplying by 1.0e-9f.
	
	public Vector3 K           		=  new Vector3(686.0f, 678.0f, 666.0f);
	private const float n      		=  1.0003f;   // Refractive index of air
	private const float N      		=  2.545E25f; // Molecular density
	private const float pn     		=  0.035f;    // depolatization factor for standard air
	private const float pi     		=  Mathf.PI;  // 3.141592
	private const float altitude    =  2000.0f;
//-------------------------------------------------------------------------------------------------------
	// Textures
	public  Texture2D emptySkyTexture;
	public  Texture2D cloudTexture;
	public  Texture2D moonTexture;
	public  Cubemap   starField;
	public  Cubemap   starNoise;
//-------------------------------------------------------------------------------------------------------
	// For the sky customization
	public  Color sunsetColor    =  Color.white;
	public  Color moonLightColor =  new Color(0.0f,0.5f,1.0f,1.0f);
	public  float sunIntensity   =  75.0f;   // Intensity of the sun
	public  float moonIntensity  =  0.25f;   // Intensity of the moonlight

	public  float sunDiskSize    =  0.998f;
	public  float sunDiskIntensity= 1.0f;

	public  float dayLuminance   =  1.0f;    //
	public  float nightLuminance =  1.35f;   //
	public  float nightIntensity =  0.2f;    //
	public  float starsExtinction=  1.5f;    // The stars will shown on the horizon ?
	public  float starsIntensity =  2.0f;
	public  float cloudSpeed     =  0.05f;
	public  float moonSize       =  5.0f;
	public  float shadowX        =  0.0f;
	public  float shadowY        =  0.24f;
	public  float shadowSize     =  1.5f;
	public  float shadowIntensity=  0.015f;
	public  float blueObscurance =  0.5f;
	
	public   float scintSpeed    =  5.5f;   // Stars scintillation speed
	private  float scintRot      =  0.0f;   // For noise texture rotation, to get star scintillation
	private  Matrix4x4 noiseRot;
	
	public  float exposure       =  1.5f;    // For internal sky tonemapping in the shader
	public  float colorCorrection=  1.0f;
	public  float horizonAltitude=  0.1f;
//-------------------------------------------------------------------------------------------------------
	// For the fog settings
	public Color  normalFog_sunsetColor = new Color(0.8f,0.33f,0.12f,1.0f);
	public Color  normalFog_noonColor   = new Color(0.83f,0.89f,1.0f,1.0f);
	public Color  normalFog_nightColor  = new Color(0.18f,0.20f,0.21f,1.0f);
	public float  normalFogDistance = 2.0f;
	public float  inScatterFogDistance = 3.0f;
	public float  mixFogDistance = 7.6f;
	public bool   linearFog      =  false;
//-------------------------------------------------------------------------------------------------------
	// For the Time of Day
	public  int   UTC                  =  0;
	public  float DAY_CYCLE_IN_MINUTES =  3.0f;
	public  float CURRENT_TIME         =  6.0f;
	public  float Longitude            =  0;
	private Vector3 v3         =  Vector3.zero; // For Sun Rotation
	private float DEGREES_PER_MINUTE =  0.25f;  //  360°/1440 Minutes per Day = 0.25° per Minutes
	private float ROTATION     =  0;
//-------------------------------------------------------------------------------------------------------
	// Options
	public bool skyUpdate      =  true;
	public bool hdr            =  false;
	public bool useLensFlare   =  true;
	public int  spaceColorIndex    = 0;
	public int  ambientSourceIndex = 0;
	
	public Color sunsetAmbientColor = new Color(1.0f,1.0f,1.0f,1.0f);
	public Color sunsetAmbientSkyColor = new Color(1.0f,1.0f,1.0f,1.0f);
	public Color sunsetAmbientHorizonColor = new Color(1.0f,1.0f,1.0f,1.0f);
	public Color sunsetAmbientGroundColor = new Color(1.0f,1.0f,1.0f,1.0f);
	public float sunsetAmbientIntensity = 0.5f;
	
	public Color noonAmbientColor = new Color(1.0f,1.0f,1.0f,1.0f);
	public Color noonAmbientSkyColor = new Color(1.0f,1.0f,1.0f,1.0f);
	public Color noonAmbientHorizonColor = new Color(1.0f,1.0f,1.0f,1.0f);
	public Color noonAmbientGroundColor = new Color(1.0f,1.0f,1.0f,1.0f);
	public float noonAmbientIntensity = 0.5f;
	
	public Color nightAmbientColor = new Color(1.0f,1.0f,1.0f,1.0f);
	public Color nightAmbientSkyColor = new Color(1.0f,1.0f,1.0f,1.0f);
	public Color nightAmbientHorizonColor = new Color(1.0f,1.0f,1.0f,1.0f);
	public Color nightAmbientGroundColor = new Color(1.0f,1.0f,1.0f,1.0f);
	public float nightAmbientIntensity = 0.5f;
	
	public float ambientTransitionSpeed = 1.0f;

	// To save lights components
	private Light     sunLight;
	private Light     moonLight;
	private LensFlare sunFlare;
//=======================================================================================================
//-------------------------------------------------------------------------------------------------------
	// Use this for initialization
	void Start()
	{
		// Get lights components
		if (useLensFlare){
			sunFlare = SUN.GetComponent<LensFlare> ();
		}
		sunLight = SUN.GetComponent<Light> ();
		moonLight = MOON.GetComponent<Light> ();

		SkyUpdate();
		SetTime (CURRENT_TIME, DAY_CYCLE_IN_MINUTES);
		v3.y += Longitude;
		MOON.transform.localScale = new Vector3(-1,-1,1);
	}
	
	// Update is called once per frame
	void Update()
	{
		///////////////////////////
		// Update the sky SHADER //

		// Update the sky equations every frame ?
		if (skyUpdate)
		{
			SkyUpdate();
		}
		// Needs constant update
		skyMat.SetVector ("sunDir" ,   -SUN.transform.forward  );
		skyMat.SetVector ("moonDir",   -MOON.transform.forward );
		skyMat.SetMatrix ("moonMatrix", MOON.transform.worldToLocalMatrix);
		fogMat.SetVector ("sunDir" ,   -SUN.transform.forward  );
		fogMat.SetVector ("moonDir",   -MOON.transform.forward );
		fogMat.SetMatrix ("moonMatrix", MOON.transform.worldToLocalMatrix);
		if (scintSpeed > 0.0f)// For rotation of the noise texture in shader to apply star scintillation
		{
			scintRot += scintSpeed * Time.deltaTime;
			Quaternion rot = Quaternion.Euler (scintRot, scintRot, scintRot);
			noiseRot = Matrix4x4.TRS (Vector3.zero, rot, new Vector3 (1, 1, 1));
			skyMat.SetMatrix ("noiseMatrix", noiseRot);
		}

		/////////////////
		// Time of Day //

		v3.x -= ROTATION * Time.deltaTime;
		if (v3.x <= -360)v3.x+=360;
		// Only in gameplay
		if (Application.isPlaying)
		{
			SUN.transform.eulerAngles = v3; //Rotation of the sun on the x axis
		}

		// Only in Editor
		#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				// Get lights components to works in edit mode
				if (useLensFlare){
					sunFlare = SUN.GetComponent<LensFlare> ();
				}
				sunLight = SUN.GetComponent<Light> ();
				moonLight = MOON.GetComponent<Light> ();

				v3.x = (CURRENT_TIME + UTC) * 360.0f / (24) - 90; // Select the "Current Time",this will be used for sun initial rotation
				v3.y = Longitude + 180; 				          // Sets the longitude of the sun, this will be used for sun initial position
				SUN.transform.localEulerAngles = v3;		      // Apply Rotation
				MOON.transform.localScale = new Vector3(1,1,1);
			}
		#endif
		SunLightIntensity ();
		MoonLightIntensity ();
		SetAmbient ();
		
	}




//=======================================================================================================
//-------------------------------------------------------------------------------------------------------
	// Get Beta Rayleight
	private Vector3 BetaRay()
	{
		Vector3 converted_lambda = lambda * 1.0e-9f; // Converting the wavelength values given in Inpector for real scale used in formula
		Vector3 Br;
		
		// (6.0f - 7.0f * pn) and (6.0f + 3.0f * pn), they are not included in this equation because there is no significant visual changes in the sky
		////////////////
		// Without pn //
		//Br.x = ((8.0f * Mathf.Pow(pi, 3.0f) * (Mathf.Pow(Mathf.Pow(n, 2.0f) - 1.0f, 2.0f)) ) / (3.0f * N * Mathf.Pow(converted_lambda.x, 4.0f)))*altitude;
		//Br.y = ((8.0f * Mathf.Pow(pi, 3.0f) * (Mathf.Pow(Mathf.Pow(n, 2.0f) - 1.0f, 2.0f)) ) / (3.0f * N * Mathf.Pow(converted_lambda.y, 4.0f)))*altitude;
		//Br.z = ((8.0f * Mathf.Pow(pi, 3.0f) * (Mathf.Pow(Mathf.Pow(n, 2.0f) - 1.0f, 2.0f)) ) / (3.0f * N * Mathf.Pow(converted_lambda.z, 4.0f)))*altitude;
		
		///////////////////////
		// Original equation //
		Br.x = (((8.0f * Mathf.Pow(pi, 3.0f) * (Mathf.Pow(Mathf.Pow(n, 2.0f) - 1.0f, 2.0f)))*(6.0f+3.0f*pn) ) / ((3.0f * N * Mathf.Pow(converted_lambda.x, 4.0f))*(6.0f-7.0f*pn) ))*altitude;
		Br.y = (((8.0f * Mathf.Pow(pi, 3.0f) * (Mathf.Pow(Mathf.Pow(n, 2.0f) - 1.0f, 2.0f)))*(6.0f+3.0f*pn) ) / ((3.0f * N * Mathf.Pow(converted_lambda.y, 4.0f))*(6.0f-7.0f*pn) ))*altitude;
		Br.z = (((8.0f * Mathf.Pow(pi, 3.0f) * (Mathf.Pow(Mathf.Pow(n, 2.0f) - 1.0f, 2.0f)))*(6.0f+3.0f*pn) ) / ((3.0f * N * Mathf.Pow(converted_lambda.z, 4.0f))*(6.0f-7.0f*pn) ))*altitude;
		
		return Br;
	}
//-------------------------------------------------------------------------------------------------------
	// Get Beta Mie
	private Vector3 BetaMie()
	{
		float c = (0.2f * Turbidity ) * 10.0f;
		Vector3 Bm;
		Bm.x = (434.0f * c * pi * Mathf.Pow((2.0f * pi) / lambda.x, 2.0f) * K.x);
		Bm.y = (434.0f * c * pi * Mathf.Pow((2.0f * pi) / lambda.y, 2.0f) * K.y);
		Bm.z = (434.0f * c * pi * Mathf.Pow((2.0f * pi) / lambda.z, 2.0f) * K.z);

		Bm.x=Mathf.Pow(Bm.x,-1.0f);
		Bm.y=Mathf.Pow(Bm.y,-1.0f);
		Bm.z=Mathf.Pow(Bm.z,-1.0f);

		return Bm;
	}
//-------------------------------------------------------------------------------------------------------
	// Get (3.0/(16.0*pi)) for rayleight phase function
	private float pi316()
	{
		return 3.0f/(16.0f*pi);
	}
//-------------------------------------------------------------------------------------------------------
	// Get (1.0/(4.0*pi)) for mie phase function
	private float pi14()
	{
		return 1.0f/(4.0f*pi);
	}
//-------------------------------------------------------------------------------------------------------
	// Get mie g constants
	private Vector3 mieG()
	{
		return new Vector3(1.0f-g*g,1.0f+g*g,2.0f*g);
	}






//=======================================================================================================
//-------------------------------------------------------------------------------------------------------
	// Set "Time of Day" and "Day Duration"
	public void SetTime(float time,float dayDuration)
	{
		v3.x = -((time) + UTC) * (60*DEGREES_PER_MINUTE)-90;
		if (dayDuration>0.0)
		{
			ROTATION = (DEGREES_PER_MINUTE * 1440) / (dayDuration * 60); //Gets the degree to rotate the sun according to the time selected
		}
		else { ROTATION=0; }
	}
//-------------------------------------------------------------------------------------------------------
	private void SunLightIntensity()
	{
		//		Light     sunLight = SUN.GetComponent<Light> ();
		//		LensFlare sunFlare = SUN.GetComponent<LensFlare> ();
		if (sunLight != null)
		{
			sunLight.intensity = Vector3.Dot (SUN.transform.forward, Vector3.down);
			if(sunLight.intensity <= 0)
			{
				sunLight.enabled = false;
			}else { sunLight.enabled = true; }
			
			if (sunFlare != null)
			{
				if (useLensFlare)
				{
					sunFlare.enabled = true;
					sunFlare.brightness = Vector3.Dot (SUN.transform.forward, new Vector3(0,-0.25f,0));
				}else { sunFlare.enabled =false; }
			}
		}
	}
//-------------------------------------------------------------------------------------------------------
	private void MoonLightIntensity()
	{
		//		Light moonLight = MOON.GetComponent<Light> ();
		if (moonLight != null)
		{
			moonLight.intensity = Vector3.Dot (SUN.transform.forward, new Vector3(0,0.1f,0));
			if(moonLight.intensity <= 0)
			{
				moonLight.enabled = false;
			}else { moonLight.enabled = true; }
		}
	}
	//-------------------------------------------------------------------------------------------------------
	private void SetAmbient()
	{
		float lerp1;
		float lerp2;
		float sunset_to_noon_Intensity;
		switch (ambientSourceIndex)
		{
		case 0:
			lerp1 =  Vector3.Dot(SUN.transform.forward, new Vector3(0,-ambientTransitionSpeed,0));
			lerp2 =  Vector3.Dot(SUN.transform.forward, new Vector3(0,ambientTransitionSpeed,0));
			sunset_to_noon_Intensity  = Mathf.Lerp(sunsetAmbientIntensity, noonAmbientIntensity, lerp1);
			sunset_to_noon_Intensity  = Mathf.Lerp(sunset_to_noon_Intensity, nightAmbientIntensity, lerp2);
			RenderSettings.ambientIntensity = sunset_to_noon_Intensity;
			
			break;

		case 1:
			lerp1 =  Vector3.Dot(SUN.transform.forward, new Vector3(0,-ambientTransitionSpeed,0));
			lerp2 =  Vector3.Dot(SUN.transform.forward, new Vector3(0,ambientTransitionSpeed,0));
			sunset_to_noon_Intensity  = Mathf.Lerp(sunsetAmbientIntensity, noonAmbientIntensity, lerp1);
			sunset_to_noon_Intensity  = Mathf.Lerp(sunset_to_noon_Intensity, nightAmbientIntensity, lerp2);
			RenderSettings.ambientIntensity = sunset_to_noon_Intensity;
			
			Color sunset_to_noon_SkyColor =  Color.Lerp(sunsetAmbientSkyColor, noonAmbientSkyColor, lerp1);
			sunset_to_noon_SkyColor =  Color.Lerp(sunset_to_noon_SkyColor, nightAmbientSkyColor, lerp2);
			RenderSettings.ambientSkyColor = sunset_to_noon_SkyColor;
			
			Color sunset_to_noon_EquatorColor =  Color.Lerp(sunsetAmbientHorizonColor, noonAmbientHorizonColor, lerp1);
			sunset_to_noon_EquatorColor =  Color.Lerp(sunset_to_noon_EquatorColor, nightAmbientHorizonColor, lerp2);
			RenderSettings.ambientEquatorColor = sunset_to_noon_EquatorColor;
			
			Color sunset_to_noon_GroundColor =  Color.Lerp(sunsetAmbientGroundColor, noonAmbientGroundColor, lerp1);
			sunset_to_noon_GroundColor =  Color.Lerp(sunset_to_noon_GroundColor, nightAmbientGroundColor, lerp2);
			RenderSettings.ambientGroundColor = sunset_to_noon_GroundColor;
			
			break;
			
		case 2:
			lerp1 =  Vector3.Dot(SUN.transform.forward, new Vector3(0,-ambientTransitionSpeed,0));
			lerp2 =  Vector3.Dot(SUN.transform.forward, new Vector3(0,ambientTransitionSpeed,0));
			sunset_to_noon_Intensity  = Mathf.Lerp(sunsetAmbientIntensity, noonAmbientIntensity, lerp1);
			sunset_to_noon_Intensity  = Mathf.Lerp(sunset_to_noon_Intensity, nightAmbientIntensity, lerp2);
			RenderSettings.ambientIntensity = sunset_to_noon_Intensity;
			
			Color sunset_to_noon_Color =  Color.Lerp(sunsetAmbientColor, noonAmbientColor, lerp1);
			sunset_to_noon_Color =  Color.Lerp(sunset_to_noon_Color, nightAmbientColor, lerp2);
			RenderSettings.ambientSkyColor = sunset_to_noon_Color;
			break;
		}
	}





//=======================================================================================================
//-------------------------------------------------------------------------------------------------------
	// Update the sky properties
	void SkyUpdate()
	{
		// SKY MATERIAL //
		skyMat.SetVector ("Br", BetaRay () * RayCoeff);
		skyMat.SetVector ("Br2", BetaRay () * 3.0f);
		skyMat.SetVector ("Bm", BetaMie () * MieCoeff);
		skyMat.SetVector ("Brm", BetaRay () * RayCoeff + BetaMie () * MieCoeff);
		skyMat.SetVector ("mieG", mieG ());
		skyMat.SetVector ("mieG", mieG ());
		
		skyMat.SetFloat ("pi316", pi316 ());
		skyMat.SetFloat ("pi14", pi14 ());
		skyMat.SetFloat ("pi", pi);
		
		skyMat.SetTexture ("Cloud1", cloudTexture);
		skyMat.SetTexture ("moonSampler", moonTexture);
		skyMat.SetTexture ("starField", starField);
		skyMat.SetTexture ("starNoise", starNoise);
		
		skyMat.SetColor ("sunsetColor", sunsetColor);
		skyMat.SetColor ("moonLightColor", moonLightColor);
		
		skyMat.SetFloat ("sunIntensity", sunIntensity);
		skyMat.SetFloat ("moonLightIntensity", moonIntensity);

		skyMat.SetFloat ("sunDiskSize", sunDiskSize);
		skyMat.SetFloat ("sunDiskIntensity", sunDiskIntensity);

		skyMat.SetFloat ("dayLuminance", dayLuminance);
		skyMat.SetFloat ("blueObscurance", blueObscurance);
		skyMat.SetFloat ("nightLuminance", nightLuminance);
		skyMat.SetFloat ("nightIntensity", nightIntensity);
		skyMat.SetFloat ("starsExtinction", starsExtinction);
		skyMat.SetFloat ("starsIntensity", starsIntensity);
		skyMat.SetFloat ("cloudSpeed", cloudSpeed);
		skyMat.SetFloat ("moonSize", moonSize);
		skyMat.SetFloat ("shadowX", shadowX);
		skyMat.SetFloat ("shadowY", shadowY);
		skyMat.SetFloat ("shadowSize", shadowSize);
		skyMat.SetFloat ("shadowIntensity", shadowIntensity);
		skyMat.SetFloat ("horizonAltitude", horizonAltitude);
		
		skyMat.SetFloat ("Exposure", exposure);
		skyMat.SetFloat ("colorCorrection", colorCorrection);
		
		// FOG MATERIAL //
		fogMat.SetVector ("Br", BetaRay () * RayCoeff);
		fogMat.SetVector ("Br2", BetaRay () * 3.0f);
		fogMat.SetVector ("Bm", BetaMie () * MieCoeff);
		fogMat.SetVector ("Brm", BetaRay () * RayCoeff + BetaMie () * MieCoeff);
		fogMat.SetVector ("mieG", mieG ());
		fogMat.SetVector ("mieG", mieG ());
		
		fogMat.SetFloat ("pi316", pi316 ());
		fogMat.SetFloat ("pi14", pi14 ());
		fogMat.SetFloat ("pi", pi);
		
		fogMat.SetTexture ("emptySky", emptySkyTexture);
		
		fogMat.SetColor ("sunsetColor", sunsetColor);
		fogMat.SetColor ("moonLightColor", moonLightColor);
		
		fogMat.SetFloat ("sunIntensity", sunIntensity);
		fogMat.SetFloat ("moonLightIntensity", moonIntensity);
		fogMat.SetFloat ("dayLuminance", dayLuminance);
		fogMat.SetFloat ("blueObscurance", blueObscurance);
		fogMat.SetFloat ("nightLuminance", nightLuminance);
		fogMat.SetFloat ("nightIntensity", nightIntensity);
		fogMat.SetFloat ("horizonAltitude", horizonAltitude);
		
		fogMat.SetColor ("normalFog_sunsetColor", normalFog_sunsetColor);
		fogMat.SetColor ("normalFog_noonColor", normalFog_noonColor);
		fogMat.SetColor ("normalFog_nightColor", normalFog_nightColor);
		fogMat.SetFloat ("normalFogDistance", normalFogDistance);
		fogMat.SetFloat ("inScatterFogDistance", inScatterFogDistance);
		fogMat.SetFloat ("mixFogDistance", mixFogDistance);
		
		fogMat.SetFloat ("Exposure", exposure);
		fogMat.SetFloat ("colorCorrection", colorCorrection);
		if (linearFog) {
			fogMat.SetFloat ("linearFog", 0.45f);
		} else {
			fogMat.SetFloat ("linearFog", 1.0f);
		}
		
		// General Settings //
		if (hdr)
		{
			skyMat.DisableKeyword ("HDR_OFF");
			skyMat.EnableKeyword ("HDR_ON");
			fogMat.DisableKeyword ("HDR_OFF");
			fogMat.EnableKeyword ("HDR_ON");
		}
		else
		{
			skyMat.EnableKeyword ("HDR_OFF");
			skyMat.DisableKeyword ("HDR_ON");
			fogMat.EnableKeyword ("HDR_OFF");
			fogMat.DisableKeyword ("HDR_ON");
		}
	}




//=======================================================================================================
//-------------------------------------------------------------------------------------------------------
	void OnEnable()
	{
		#if UNITY_EDITOR
		if (SUN == null)
			Debug.Log("Warning. Apply the <b>Sun</b> to azure[Sky] control in Inspector");
		if (MOON == null)
			Debug.Log("Warning. Apply the <b>Moon</b> to azure[Sky] control in Inspector");
		if (skyMat == null)
			Debug.Log("Warning. Apply the <b>Sky Material</b> to azure[Sky] control in Inspector");
		if (fogMat == null)
			Debug.Log("Warning. Apply the <b>Fog Material</b> to azure[Sky] control in Inspector");
		if (emptySkyTexture == null)
			Debug.Log("Warning. Azure Sky Control script does not have a texture assigned to <b>Empty Sky Texture</b>");
		if (cloudTexture == null)
			Debug.Log("Warning. Azure Sky Control script does not have a texture assigned to <b>Cloud Texture</b>");
		if (starField == null)
			Debug.Log("Warning. Azure Sky Control script does not have a texture assigned to <b>Star Field (Cube Map)</b>");
		if (starNoise == null)
			Debug.Log("Warning. Azure Sky Control script does not have a texture assigned to <b>Star Noise (Cube Map)</b>");
		if (moonTexture == null)
			Debug.Log("Warning. Azure Sky Control script does not have a texture assigned to <b>Moon Texture</b>");
		if(useLensFlare && SUN.GetComponent<LensFlare> () == null)
			Debug.Log("Warning. azure[Sky]. <b>Use Lens Flare</b> is selected, but the sunlight does not have the Lens Flare component");
		#endif
	}
}