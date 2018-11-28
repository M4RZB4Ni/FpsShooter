using UnityEditor;
using UnityEngine;
using System.Collections;

[CustomEditor(typeof(SkyControl_v2_0_SimpleCloud))]
public class SkyControl_v2_0_SimpleCloud_Editor : Editor
{
	private string[] spaceColor = new string[]{"Default", "Linear To Gamma"};
	private string[] ambientSource = new string[]{"Skybox", "Gradient", "Color"};
	
	
	//=======================================================================================================
	//-------------------------------------------------------------------------------------------------------
	public override void OnInspectorGUI()
	{
		//Get target
		SkyControl_v2_0_SimpleCloud Target = (SkyControl_v2_0_SimpleCloud)target;
		Undo.RecordObject(target, "Undo Sky Values");
		
		//-------------------------------------------------------------------------------------------------------
		/////////////////////////////////////
		/////////OBJs & MATs Tab/////////////
		/// /////////////////////////////////
		EditorGUILayout.BeginVertical ("Box");
		EditorGUILayout.BeginHorizontal ();
		EditorGUILayout.Space();
		Target.showObjMats = EditorGUILayout.Foldout(Target.showObjMats, "Obj & Mat");
		EditorGUILayout.EndHorizontal ();
		EditorGUILayout.Space();
		if (Target.showObjMats)
		{
			///Sun ObjectField///
			EditorGUILayout.BeginHorizontal ();
			GUILayout.Label ("Sun GameObject");
			Target.SUN = (GameObject)EditorGUILayout.ObjectField (Target.SUN, typeof(GameObject), true, GUILayout.Width (125), GUILayout.Height (15));
			EditorGUILayout.EndHorizontal ();
			///Moon ObjectField///
			EditorGUILayout.BeginHorizontal ();
			GUILayout.Label ("Moon GameObject");
			Target.MOON = (GameObject)EditorGUILayout.ObjectField (Target.MOON, typeof(GameObject), true, GUILayout.Width (125), GUILayout.Height (15));
			EditorGUILayout.EndHorizontal ();
			///Sky Material Field///
			EditorGUILayout.BeginHorizontal ();
			GUILayout.Label ("Sky Material");
			Target.skyMat = (Material)EditorGUILayout.ObjectField (Target.skyMat, typeof(Material), true, GUILayout.Width (125), GUILayout.Height (15));
			EditorGUILayout.EndHorizontal ();
			///Fog Material Field///
			EditorGUILayout.BeginHorizontal ();
			GUILayout.Label ("Fog Material");
			Target.fogMat = (Material)EditorGUILayout.ObjectField (Target.fogMat, typeof(Material), true, GUILayout.Width (125), GUILayout.Height (15));
			EditorGUILayout.EndHorizontal ();
			EditorGUILayout.Space();
		}
		EditorGUILayout.EndVertical ();
		
		
		//-------------------------------------------------------------------------------------------------------
		//////////////////////////////////
		/////////Scattering Tab///////////
		/// //////////////////////////////
		EditorGUILayout.BeginVertical ("Box");
		EditorGUILayout.BeginHorizontal ();
		EditorGUILayout.Space();
		Target.showScattering = EditorGUILayout.Foldout(Target.showScattering, "Scattering");
		EditorGUILayout.EndHorizontal ();
		EditorGUILayout.Space();
		if (Target.showScattering)
		{
			///Wave-Length///
			EditorGUILayout.BeginVertical ("Box");
			
			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.Space();
			GUILayout.Label ("Wave-Length");
			EditorGUILayout.EndHorizontal ();
			// R
			EditorGUILayout.BeginHorizontal ();
			Target.lambda.x = EditorGUILayout.Slider("R",Target.lambda.x,0.0f, 1000.0f);
			EditorGUILayout.EndHorizontal ();
			// G
			EditorGUILayout.BeginHorizontal ();
			Target.lambda.y = EditorGUILayout.Slider("G",Target.lambda.y,0.0f, 1000.0f);
			EditorGUILayout.EndHorizontal ();
			// B
			EditorGUILayout.BeginHorizontal ();
			Target.lambda.z = EditorGUILayout.Slider("B",Target.lambda.z,0.0f, 1000.0f);
			EditorGUILayout.EndHorizontal ();
			
			EditorGUILayout.EndVertical ();
			
			EditorGUILayout.Space();
			
			
			Target.RayCoeff      =  EditorGUILayout.Slider("Rayleigh",Target.RayCoeff,1.0f, 5.0f);
			Target.MieCoeff      =  EditorGUILayout.Slider("Mie",Target.MieCoeff,1.0f, 5.0f);
			Target.Turbidity     =  EditorGUILayout.Slider("Turbidity",Target.Turbidity,1.0f, 5.0f);
			Target.g             =  EditorGUILayout.Slider("G",Target.g,0.0f, 0.99f);
			Target.sunIntensity  =  EditorGUILayout.Slider("Sun Intensity",Target.sunIntensity,25.0f, 100.0f);
			Target.moonIntensity =  EditorGUILayout.Slider("Moon intensity",Target.moonIntensity,0.0f, 1.0f);
			EditorGUILayout.Space();
			///Reset variables///
			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.Space();
			if(GUILayout.Button("Standard Settings"))
			{
				Target.lambda        = new Vector3(650.0f, 570.0f, 475.0f);
				Target.RayCoeff      = 1.5f;
				Target.MieCoeff      = 1.0f;
				Target.Turbidity     = 1.0f;
				Target.g             = 0.75f;
				Target.sunIntensity  = 100.0f;
				Target.moonIntensity = 0.25f;
			}
			EditorGUILayout.Space();
			EditorGUILayout.EndHorizontal ();
			EditorGUILayout.Space();
		}
		EditorGUILayout.EndVertical ();
		
		
		//-------------------------------------------------------------------------------------------------------
		//////////////////////////////////
		/////////Textures Tab/////////////
		//////////////////////////////////
		EditorGUILayout.BeginVertical ("Box");
		EditorGUILayout.BeginHorizontal ();
		EditorGUILayout.Space();
		Target.showTextures = EditorGUILayout.Foldout(Target.showTextures, "Textures");
		EditorGUILayout.EndHorizontal ();
		EditorGUILayout.Space();
		if(Target.showTextures)
		{
			///Moon Texture///
			EditorGUILayout.BeginHorizontal ();
			GUILayout.Label("Cloud Texture");
			Target.cloudTexture = (Texture2D)EditorGUILayout.ObjectField(Target.cloudTexture,typeof(Texture2D),true,GUILayout.Width(60),GUILayout.Height(60));
			EditorGUILayout.EndHorizontal ();

			///Moon Texture///
			EditorGUILayout.BeginHorizontal ();
			GUILayout.Label("Moon Texture");
			Target.moonTexture = (Texture2D)EditorGUILayout.ObjectField(Target.moonTexture,typeof(Texture2D),true,GUILayout.Width(60),GUILayout.Height(60));
			EditorGUILayout.EndHorizontal ();
			
			///Star Field Cubemap///
			EditorGUILayout.BeginHorizontal ();
			GUILayout.Label("Star Field (Cubemap)");
			Target.starField = (Cubemap)EditorGUILayout.ObjectField(Target.starField,typeof(Cubemap),true,GUILayout.Width(60),GUILayout.Height(60));
			EditorGUILayout.EndHorizontal ();
			
			///Star Noise Cubemap///
			EditorGUILayout.BeginHorizontal ();
			GUILayout.Label("Star Noise (Cubemap)");
			Target.starNoise = (Cubemap)EditorGUILayout.ObjectField(Target.starNoise,typeof(Cubemap),true,GUILayout.Width(60),GUILayout.Height(60));
			EditorGUILayout.EndHorizontal ();
			
			///Milky Way Cubemap///
			EditorGUILayout.BeginHorizontal ();
			GUILayout.Label("Milky Way (Cubemap)");
			Target.milkyWay = (Cubemap)EditorGUILayout.ObjectField(Target.milkyWay,typeof(Cubemap),true,GUILayout.Width(60),GUILayout.Height(60));
			EditorGUILayout.EndHorizontal ();
			
			EditorGUILayout.Space();
		}
		EditorGUILayout.EndVertical ();
		
		
		
		//-------------------------------------------------------------------------------------------------------
		///////////////////////////////////////
		/////////Sky Customization Tab/////////
		///////////////////////////////////////
		EditorGUILayout.BeginVertical ("Box");
		EditorGUILayout.BeginHorizontal ();
		EditorGUILayout.Space();
		Target.showSkyCustom = EditorGUILayout.Foldout(Target.showSkyCustom, "Sky Settings");
		EditorGUILayout.EndHorizontal ();
		EditorGUILayout.Space();
		if (Target.showSkyCustom)
		{
			///Sunset Color///
			EditorGUILayout.BeginHorizontal ();
			GUILayout.Label("Sunset Color");
			Target.sunsetColor = EditorGUILayout.ColorField(Target.sunsetColor,GUILayout.Width(50));
			EditorGUILayout.EndHorizontal ();
			///Moonset Color///
			EditorGUILayout.BeginHorizontal ();
			GUILayout.Label("Moon Light Color");
			Target.moonLightColor = EditorGUILayout.ColorField(Target.moonLightColor,GUILayout.Width(50));
			EditorGUILayout.EndHorizontal ();
			
			EditorGUILayout.Space();
			Target.sunDiskSize      = EditorGUILayout.Slider("Sun Disk Size",Target.sunDiskSize,1.0f,0.996f);
			Target.sunDiskIntensity = EditorGUILayout.Slider("Sun Disk Intensity",Target.sunDiskIntensity,0.0f, 5.0f);
			
			EditorGUILayout.Space();
			Target.dayLuminance    = EditorGUILayout.Slider("Day Luminance",Target.dayLuminance,1.0f, 3.0f);
			Target.dayBlueObscurance    = EditorGUILayout.Slider("Day Blue Darkness",Target.dayBlueObscurance,0.0f, 1.0f);
			EditorGUILayout.Space();
			Target.nightBlueObscurance  = EditorGUILayout.Slider("Night Blue Darkness",Target.nightBlueObscurance,0.0f, 1.0f);
			Target.nightLuminance  = EditorGUILayout.Slider("Night Luminance",Target.nightLuminance,1.0f, 3.0f);
			Target.nightIntensity  = EditorGUILayout.Slider("Night Intensity",Target.nightIntensity,0.0f, 1.0f);
			//			Target.nightTopCloud   = EditorGUILayout.Slider("Night Top Cloud",Target.nightTopCloud,0.0f, 0.25f);
			
			EditorGUILayout.Space();
			Target.starsExtinction = EditorGUILayout.Slider("Stars Extinction",Target.starsExtinction,0.5f, 3.0f);
			Target.starsIntensity  = EditorGUILayout.Slider("Stars Intensity",Target.starsIntensity,0.0f, 5.0f);
			Target.scintSpeed      = EditorGUILayout.Slider("Stars Scintillation",Target.scintSpeed,1.0f, 10.0f);
			EditorGUILayout.Space();
			
			
			///Wave-Length///
			EditorGUILayout.BeginVertical ("Box");
			
			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.Space();
			GUILayout.Label ("Milky Way");
			EditorGUILayout.EndHorizontal ();
			
			Target.milkyWayIntensity = EditorGUILayout.Slider("Milky Way Intensity",Target.milkyWayIntensity,0.0f, 1.5f);
			Target.milkyWayPower     = EditorGUILayout.Slider("Milky Way Power",Target.milkyWayPower,1.0f, 4.0f);
			
			EditorGUILayout.Space();
			GUILayout.Label ("Position:");
			Target.vMilkWayPos.x     = EditorGUILayout.Slider("X",Target.vMilkWayPos.x,0.0f, 360.0f);
			Target.vMilkWayPos.y     = EditorGUILayout.Slider("Y",Target.vMilkWayPos.y,0.0f, 360.0f);
			Target.vMilkWayPos.z     = EditorGUILayout.Slider("Z",Target.vMilkWayPos.z,0.0f, 360.0f);
			
			EditorGUILayout.EndVertical ();
			
			
			
			
			EditorGUILayout.Space();
			Target.moonSize        = EditorGUILayout.Slider("Moon Size",Target.moonSize,2.5f, 7.0f);
			Target.shadowX         = EditorGUILayout.Slider("Moon Shadow X-axis",Target.shadowX,-1.0f, 1.0f);
			Target.shadowY         = EditorGUILayout.Slider("Moon Shadow Y-axis",Target.shadowY,-1.0f, 1.0f);
			Target.shadowSize      = EditorGUILayout.Slider("Moon Shadow Size",Target.shadowSize,5.0f, 1.0f);
			Target.shadowIntensity = EditorGUILayout.Slider("Moon Shadow Intensity",Target.shadowIntensity,0.0f, 1.0f);
			EditorGUILayout.Space();
			Target.exposure        = EditorGUILayout.Slider("Expossure",Target.exposure,0.0f, 5.0f);
			Target.horizonAltitude = EditorGUILayout.Slider("Horizon Altitude",Target.horizonAltitude,0.0f, 0.25f);
			EditorGUILayout.Space();
			///Reset variables///
			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.Space();
			if(GUILayout.Button("Standard Settings"))
			{
				Target.sunsetColor     = Color.white;
				Target.moonLightColor  = new Color(0.0f,0.5f,1.0f,1.0f);
				Target.vMilkWayPos     = Vector3.zero;
				
				Target.sunDiskSize      = 0.997f;
				Target.sunDiskIntensity = 1.0f;
				
				Target.dayLuminance    = 1.0f;
				Target.dayBlueObscurance   = 1.0f;
				Target.nightBlueObscurance = 1.0f;
				Target.nightLuminance  = 1.0f;
				Target.nightIntensity  = 0.142f;
				Target.starsExtinction = 1.5f;
				Target.starsIntensity  = 5.0f;
				Target.scintSpeed      = 5.5f;
				Target.moonSize        = 5.0f;
				Target.shadowX         = -1.0f;
				Target.shadowY         = 0.24f;
				Target.shadowSize      = 1.5f;
				Target.shadowIntensity = 0.0f;
				Target.exposure        = 1.5f;
				Target.horizonAltitude = 0.25f;
			}
			EditorGUILayout.Space();
			EditorGUILayout.EndHorizontal ();
			EditorGUILayout.Space();
		}
		EditorGUILayout.EndVertical ();
		
		
		
		//-------------------------------------------------------------------------------------------------------
		///////////////////////////////////////
		////////Cloud Customization Tab////////
		///////////////////////////////////////
		EditorGUILayout.BeginVertical ("Box");
		EditorGUILayout.BeginHorizontal ();
		EditorGUILayout.Space();
		Target.showCloudCustom = EditorGUILayout.Foldout(Target.showCloudCustom, "Cloud Settings");
		EditorGUILayout.EndHorizontal ();
		EditorGUILayout.Space();
		if (Target.showCloudCustom)
		{
			////SUNSET///
			EditorGUILayout.BeginVertical ("Box");
			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.Space();
			GUILayout.Label ("Sunset Cloud");
			EditorGUILayout.EndHorizontal ();
			// Edge Color
			EditorGUILayout.BeginHorizontal ();
			GUILayout.Label("Edge Color");
			Target.sunsetEdgeColor = EditorGUILayout.ColorField(Target.sunsetEdgeColor,GUILayout.Width(50));
			EditorGUILayout.EndHorizontal ();
			// Dark Color
			EditorGUILayout.BeginHorizontal ();
			GUILayout.Label("Dark Color");
			Target.sunsetDarkColor = EditorGUILayout.ColorField(Target.sunsetDarkColor,GUILayout.Width(50));
			EditorGUILayout.EndHorizontal ();
			// Cloud Density
			EditorGUILayout.BeginHorizontal ();
			Target.sunsetCloudDensity = EditorGUILayout.Slider("Density",Target.sunsetCloudDensity,1.0f, 5.0f);
			EditorGUILayout.EndHorizontal ();
			///Reset variables///
			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.Space();
			if(GUILayout.Button("Reset"))
			{
				Target.sunsetEdgeColor = new Color(0.99f, 0.79f, 0.64f, 1.0f);
				Target.sunsetDarkColor = new Color(0.25f, 0.34f, 0.44f, 1.0f);
				Target.sunsetCloudDensity = 1.75f;
			}
			EditorGUILayout.Space();
			EditorGUILayout.EndHorizontal ();
			EditorGUILayout.Space();
			EditorGUILayout.EndVertical ();
			
			////NOON///
			EditorGUILayout.BeginVertical ("Box");
			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.Space();
			GUILayout.Label ("Noon Cloud");
			EditorGUILayout.EndHorizontal ();
			// Edge Color
			EditorGUILayout.BeginHorizontal ();
			GUILayout.Label("Edge Color");
			Target.noonEdgeColor = EditorGUILayout.ColorField(Target.noonEdgeColor,GUILayout.Width(50));
			EditorGUILayout.EndHorizontal ();
			// Dark Color
			EditorGUILayout.BeginHorizontal ();
			GUILayout.Label("Dark Color");
			Target.noonDarkColor = EditorGUILayout.ColorField(Target.noonDarkColor,GUILayout.Width(50));
			EditorGUILayout.EndHorizontal ();
			// Cloud Density
			EditorGUILayout.BeginHorizontal ();
			Target.noonCloudDensity = EditorGUILayout.Slider("Density",Target.noonCloudDensity,1.0f, 5.0f);
			EditorGUILayout.EndHorizontal ();
			///Reset variables///
			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.Space();
			if(GUILayout.Button("Reset"))
			{
				Target.noonEdgeColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
				Target.noonDarkColor = new Color(0.25f, 0.34f, 0.44f, 1.0f);
				Target.noonCloudDensity = 1.0f;
			}
			EditorGUILayout.Space();
			EditorGUILayout.EndHorizontal ();
			EditorGUILayout.Space();
			EditorGUILayout.EndVertical ();
			
			////NIGHT///
			EditorGUILayout.BeginVertical ("Box");
			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.Space();
			GUILayout.Label ("Night Cloud");
			EditorGUILayout.EndHorizontal ();
			// Edge Color
			EditorGUILayout.BeginHorizontal ();
			GUILayout.Label("Edge Color");
			Target.nightEdgeColor = EditorGUILayout.ColorField(Target.nightEdgeColor,GUILayout.Width(50));
			EditorGUILayout.EndHorizontal ();
			// Dark Color
			EditorGUILayout.BeginHorizontal ();
			GUILayout.Label("Dark Color");
			Target.nightDarkColor = EditorGUILayout.ColorField(Target.nightDarkColor,GUILayout.Width(50));
			EditorGUILayout.EndHorizontal ();
			// Cloud Density
			EditorGUILayout.BeginHorizontal ();
			Target.nightCloudDensity = EditorGUILayout.Slider("Density",Target.nightCloudDensity,1.0f, 5.0f);
			EditorGUILayout.EndHorizontal ();
			// Moon Bright Intensity
			EditorGUILayout.BeginHorizontal ();
			Target.moonBrightIntensity = EditorGUILayout.Slider("Moon Bright Intensity",Target.moonBrightIntensity,1.0f, 7.5f);
			EditorGUILayout.EndHorizontal ();
			// Moon Bright Range
			EditorGUILayout.BeginHorizontal ();
			Target.moonBrightRange = EditorGUILayout.Slider("Moon Bright Range",Target.moonBrightRange,5.0f, 1.0f);
			EditorGUILayout.EndHorizontal ();
			
			///Reset variables///
			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.Space();
			if(GUILayout.Button("Reset"))
			{
				Target.nightEdgeColor = new Color(0.10f, 0.20f, 0.32f, 1.0f);
				Target.nightDarkColor = new Color(0.01f, 0.03f, 0.09f, 1.0f);
				Target.nightCloudDensity = 4.4f;
				Target.moonBrightIntensity = 3.0f;
				Target.moonBrightRange     = 1.0f;
			}
			EditorGUILayout.Space();
			EditorGUILayout.EndHorizontal ();
			EditorGUILayout.Space();
			EditorGUILayout.EndVertical ();
			
			////General///
			EditorGUILayout.BeginVertical ("Box");
			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.Space();
			GUILayout.Label ("General Settings");
			EditorGUILayout.EndHorizontal ();
			// Cloud Extinction
			EditorGUILayout.BeginHorizontal ();
			Target.cloudExtinction = EditorGUILayout.Slider("Extinction",Target.cloudExtinction,1.0f, 5.0f);
			EditorGUILayout.EndHorizontal ();
			// Alpha Saturation
			EditorGUILayout.BeginHorizontal ();
			Target.alphaSaturation = EditorGUILayout.Slider("Alpha Saturation",Target.alphaSaturation,1.0f, 5.0f);
			EditorGUILayout.EndHorizontal ();
			
			///Reset variables///
			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.Space();
			if(GUILayout.Button("Reset"))
			{
				Target.cloudExtinction = 3.0f;
				Target.alphaSaturation = 2.0f;
			}
			EditorGUILayout.Space();
			EditorGUILayout.EndHorizontal ();
			EditorGUILayout.Space();
			EditorGUILayout.EndVertical ();
		}
		EditorGUILayout.EndVertical ();
		
		
		
		//-------------------------------------------------------------------------------------------------------
		//////////////////////////////////
		/////////Fog Settings Tab/////////
		//////////////////////////////////
		EditorGUILayout.BeginVertical ("Box");
		EditorGUILayout.BeginHorizontal ();
		EditorGUILayout.Space();
		Target.showFog = EditorGUILayout.Foldout(Target.showFog, "Fog Settings");
		EditorGUILayout.EndHorizontal ();
		EditorGUILayout.Space();
		if(Target.showFog)
		{
			EditorGUILayout.Space();
			
			///Linear Fog Toggle///
			Target.linearFog = EditorGUILayout.Toggle("Linear Fog Color ?",Target.linearFog);
			Target.inScatterFogDistance = EditorGUILayout.Slider("Scattering Fog Distance",Target.inScatterFogDistance,0.0f, 20.0f);
			//GUILayout.Label ("Blend point between Normal Fog and Scattering Fog");
			Target.mixFogDistance = EditorGUILayout.Slider("Blend Point between ''Normal Fog'' and ''Scattering Fog''",Target.mixFogDistance,0.0f, 50.0f);
			
			////NORMAL fOG///
			EditorGUILayout.BeginVertical ("Box");
			//Title
			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.Space();
			GUILayout.Label ("Normal Fog");
			EditorGUILayout.EndHorizontal ();
			// Sunset Color
			EditorGUILayout.BeginHorizontal ();
			GUILayout.Label("Sunset Color");
			Target.normalFog_sunsetColor = EditorGUILayout.ColorField(Target.normalFog_sunsetColor,GUILayout.Width(50));
			EditorGUILayout.EndHorizontal ();
			// Noon Color
			EditorGUILayout.BeginHorizontal ();
			GUILayout.Label("Noon Color");
			Target.normalFog_noonColor = EditorGUILayout.ColorField(Target.normalFog_noonColor,GUILayout.Width(50));
			EditorGUILayout.EndHorizontal ();
			// Night Color
			EditorGUILayout.BeginHorizontal ();
			GUILayout.Label("Night Color");
			Target.normalFog_nightColor = EditorGUILayout.ColorField(Target.normalFog_nightColor,GUILayout.Width(50));
			EditorGUILayout.EndHorizontal ();
			
			Target.normalFogDistance = EditorGUILayout.Slider("Normal Fog Distance",Target.normalFogDistance,0.0f, 50.0f);
			
			EditorGUILayout.EndVertical ();
			
			////GLOBAL COLOR///
			EditorGUILayout.BeginVertical ("Box");
			//Title
			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.Space();
			GUILayout.Label ("Global Color");
			EditorGUILayout.EndHorizontal ();
			// Sunset Color
			EditorGUILayout.BeginHorizontal ();
			GUILayout.Label("Sunset Color");
			Target.sunsetGlobalColor = EditorGUILayout.ColorField(Target.sunsetGlobalColor,GUILayout.Width(50));
			EditorGUILayout.EndHorizontal ();
			// Noon Color
			EditorGUILayout.BeginHorizontal ();
			GUILayout.Label("Noon Color");
			Target.noonGlobalColor = EditorGUILayout.ColorField(Target.noonGlobalColor,GUILayout.Width(50));
			EditorGUILayout.EndHorizontal ();
			// Night Color
			EditorGUILayout.BeginHorizontal ();
			GUILayout.Label("Night Color");
			Target.nightGlobalColor = EditorGUILayout.ColorField(Target.nightGlobalColor,GUILayout.Width(50));
			EditorGUILayout.EndHorizontal ();
			
			EditorGUILayout.EndVertical ();
			
			///Reset variables///
			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.Space();
			if(GUILayout.Button("Standard Settings"))
			{
				Target.inScatterFogDistance = 3.0f;
				Target.linearFog = true;
				Target.mixFogDistance = 50.0f;
				
				Target.normalFog_sunsetColor = new Color(0.8f,0.33f,0.12f,1.0f);
				Target.normalFog_noonColor   = new Color(0.83f,0.89f,1.0f,1.0f);
				Target.normalFog_nightColor  = new Color(0.18f,0.20f,0.21f,1.0f);
				
				Target.sunsetGlobalColor = new Color(1.0f,1.0f,1.0f,1.0f);
				Target.noonGlobalColor   = new Color(1.0f,1.0f,1.0f,1.0f);
				Target.nightGlobalColor  = new Color(1.0f,1.0f,1.0f,1.0f);
				
				Target.normalFogDistance = 0.0f;
			}
			EditorGUILayout.Space();
			EditorGUILayout.EndHorizontal ();
			EditorGUILayout.Space();
		}
		EditorGUILayout.EndVertical ();
		
		
		//-------------------------------------------------------------------------------------------------------
		//////////////////////////////////
		/////////Time of Day Tab//////////
		EditorGUILayout.BeginVertical ("Box");
		EditorGUILayout.BeginHorizontal ();
		EditorGUILayout.Space();
		Target.showTime_of_Day = EditorGUILayout.Foldout(Target.showTime_of_Day, "Time of Day");
		EditorGUILayout.EndHorizontal ();
		EditorGUILayout.Space();
		
		if (Target.showTime_of_Day)
		{
			Target.azure_TIME_of_DAY=EditorGUILayout.Slider("Current Time",Target.azure_TIME_of_DAY,0.0f, 24.0f);
			Target.azure_UTC = EditorGUILayout.IntSlider("UTC",Target.azure_UTC,-12, 12);
			Target.azure_Longitude=EditorGUILayout.Slider("Longitude",Target.azure_Longitude,-180.0f, 180.0f);
			
			EditorGUILayout.BeginHorizontal ();
			GUILayout.Label("Day Duration in Minutes");
			Target.azure_DAY_CYCLE = EditorGUILayout.FloatField(Target.azure_DAY_CYCLE,GUILayout.Width(50));
			EditorGUILayout.EndHorizontal ();
			///Invert Poles///
			Target.invertEastWest = EditorGUILayout.Toggle("Invert East-West",Target.invertEastWest);
			if(Target.invertEastWest){ Target.azure_East = 180.0f; } else { Target.azure_East = 0.0f; }
			EditorGUILayout.Space();
		}
		EditorGUILayout.EndVertical ();
		
		
		
		//-------------------------------------------------------------------------------------------------------
		//////////////////////////////////
		/////////Options Tab//////////////
		//////////////////////////////////
		EditorGUILayout.BeginVertical ("Box");
		EditorGUILayout.BeginHorizontal ();
		EditorGUILayout.Space();
		Target.showOptions = EditorGUILayout.Foldout(Target.showOptions, "Options");
		EditorGUILayout.EndHorizontal ();
		EditorGUILayout.Space();
		if(Target.showOptions)
		{
			EditorGUILayout.Space();
			
			///Sky Update Toggle///
			Target.skyUpdate = EditorGUILayout.Toggle("Sky Update",Target.skyUpdate);
			
			///HDR Toggle///
			Target.hdr = EditorGUILayout.Toggle("HDR",Target.hdr);
			
			///LensFlare Toggle///
			Target.useLensFlare = EditorGUILayout.Toggle("Use Lens Flare ?",Target.useLensFlare);
			
			///Space Color///
			Target.spaceColorIndex = EditorGUILayout.Popup("Space Color",Target.spaceColorIndex, spaceColor);
			switch(Target.spaceColorIndex)
			{
			case 0:
				Target.colorCorrection = 1.0f;
				break;
			case 1:
				Target.colorCorrection = 2.2f;
				break;
			}

			///////////////////
			///Ambient Color///
			EditorGUILayout.BeginVertical ("Box");
			
			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.Space();
			GUILayout.Label ("Ambient");
			EditorGUILayout.EndHorizontal ();
			EditorGUILayout.Space();
			
			Target.ambientSourceIndex = EditorGUILayout.Popup("Ambient Source",Target.ambientSourceIndex, ambientSource);
			switch(Target.ambientSourceIndex)
			{
			case 0:
				RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Skybox;
				Target.sunsetAmbientIntensity = EditorGUILayout.Slider("Sunset Ambient Intensity",Target.sunsetAmbientIntensity,0.0f, 1.0f);
				Target.noonAmbientIntensity = EditorGUILayout.Slider("Noon Ambient Intensity",Target.noonAmbientIntensity,0.0f, 1.0f);
				Target.nightAmbientIntensity = EditorGUILayout.Slider("Night Ambient Intensity",Target.nightAmbientIntensity,0.0f, 1.0f);

				EditorGUILayout.Space();
				Target.ambientTransitionSpeed = EditorGUILayout.Slider("Tansition Speed",Target.ambientTransitionSpeed,1.0f, 7.5f);
				break;
			case 1:
				RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
				//SUNSET
				EditorGUILayout.BeginVertical ("Box");
				EditorGUILayout.BeginHorizontal ();
				EditorGUILayout.Space();
				GUILayout.Label ("Sunset");
				EditorGUILayout.EndHorizontal ();
				EditorGUILayout.Space();
				
				EditorGUILayout.BeginHorizontal ();
				GUILayout.Label("Sky Color");
				Target.sunsetAmbientSkyColor = EditorGUILayout.ColorField(Target.sunsetAmbientSkyColor,GUILayout.Width(50));
				EditorGUILayout.EndHorizontal ();
				
				EditorGUILayout.BeginHorizontal ();
				GUILayout.Label("Equator Color");
				Target.sunsetAmbientHorizonColor = EditorGUILayout.ColorField(Target.sunsetAmbientHorizonColor,GUILayout.Width(50));
				EditorGUILayout.EndHorizontal ();
				
				EditorGUILayout.BeginHorizontal ();
				GUILayout.Label("Ground Color");
				Target.sunsetAmbientGroundColor = EditorGUILayout.ColorField(Target.sunsetAmbientGroundColor,GUILayout.Width(50));
				EditorGUILayout.EndHorizontal ();
				
				Target.sunsetAmbientIntensity = EditorGUILayout.Slider("Ambient Intensity",Target.sunsetAmbientIntensity,0.0f, 1.0f);
				EditorGUILayout.Space();
				EditorGUILayout.EndVertical ();
				
				//NOON
				EditorGUILayout.BeginVertical ("Box");
				EditorGUILayout.BeginHorizontal ();
				EditorGUILayout.Space();
				GUILayout.Label ("Noon");
				EditorGUILayout.EndHorizontal ();
				EditorGUILayout.Space();
				
				EditorGUILayout.BeginHorizontal ();
				GUILayout.Label("Sky Color");
				Target.noonAmbientSkyColor = EditorGUILayout.ColorField(Target.noonAmbientSkyColor,GUILayout.Width(50));
				EditorGUILayout.EndHorizontal ();
				
				EditorGUILayout.BeginHorizontal ();
				GUILayout.Label("Equator Color");
				Target.noonAmbientHorizonColor = EditorGUILayout.ColorField(Target.noonAmbientHorizonColor,GUILayout.Width(50));
				EditorGUILayout.EndHorizontal ();
				
				EditorGUILayout.BeginHorizontal ();
				GUILayout.Label("Ground Color");
				Target.noonAmbientGroundColor = EditorGUILayout.ColorField(Target.noonAmbientGroundColor,GUILayout.Width(50));
				EditorGUILayout.EndHorizontal ();
				
				Target.noonAmbientIntensity = EditorGUILayout.Slider("Ambient Intensity",Target.noonAmbientIntensity,0.0f, 1.0f);
				EditorGUILayout.Space();
				EditorGUILayout.EndVertical ();
				
				//NIGHT
				EditorGUILayout.BeginVertical ("Box");
				EditorGUILayout.BeginHorizontal ();
				EditorGUILayout.Space();
				GUILayout.Label ("Night");
				EditorGUILayout.EndHorizontal ();
				EditorGUILayout.Space();
				
				EditorGUILayout.BeginHorizontal ();
				GUILayout.Label("Sky Color");
				Target.nightAmbientSkyColor = EditorGUILayout.ColorField(Target.nightAmbientSkyColor,GUILayout.Width(50));
				EditorGUILayout.EndHorizontal ();
				
				EditorGUILayout.BeginHorizontal ();
				GUILayout.Label("Equator Color");
				Target.nightAmbientHorizonColor = EditorGUILayout.ColorField(Target.nightAmbientHorizonColor,GUILayout.Width(50));
				EditorGUILayout.EndHorizontal ();
				
				EditorGUILayout.BeginHorizontal ();
				GUILayout.Label("Ground Color");
				Target.nightAmbientGroundColor = EditorGUILayout.ColorField(Target.nightAmbientGroundColor,GUILayout.Width(50));
				EditorGUILayout.EndHorizontal ();
				
				Target.nightAmbientIntensity = EditorGUILayout.Slider("Ambient Intensity",Target.nightAmbientIntensity,0.0f, 1.0f);
				EditorGUILayout.Space();
				EditorGUILayout.EndVertical ();

				EditorGUILayout.Space();
				Target.ambientTransitionSpeed = EditorGUILayout.Slider("Tansition Speed",Target.ambientTransitionSpeed,1.0f, 7.5f);
				
				break;
			case 2:
				RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
				//SUNSET
				EditorGUILayout.BeginVertical ("Box");
				EditorGUILayout.BeginHorizontal ();
				EditorGUILayout.Space();
				GUILayout.Label ("Sunset");
				EditorGUILayout.EndHorizontal ();
				EditorGUILayout.Space();
				EditorGUILayout.BeginHorizontal ();
				GUILayout.Label("Ambient Color");
				Target.sunsetAmbientColor = EditorGUILayout.ColorField(Target.sunsetAmbientColor,GUILayout.Width(50));
				EditorGUILayout.EndHorizontal ();
				Target.sunsetAmbientIntensity = EditorGUILayout.Slider("Ambient Intensity",Target.sunsetAmbientIntensity,0.0f, 1.0f);
				EditorGUILayout.Space();
				EditorGUILayout.EndVertical ();
				
				//NOON
				EditorGUILayout.BeginVertical ("Box");
				EditorGUILayout.BeginHorizontal ();
				EditorGUILayout.Space();
				GUILayout.Label ("Noon");
				EditorGUILayout.EndHorizontal ();
				EditorGUILayout.Space();
				EditorGUILayout.BeginHorizontal ();
				GUILayout.Label("Ambient Color");
				Target.noonAmbientColor = EditorGUILayout.ColorField(Target.noonAmbientColor,GUILayout.Width(50));
				EditorGUILayout.EndHorizontal ();
				Target.noonAmbientIntensity = EditorGUILayout.Slider("Ambient Intensity",Target.noonAmbientIntensity,0.0f, 1.0f);
				EditorGUILayout.Space();
				EditorGUILayout.EndVertical ();
				
				//NIGHT
				EditorGUILayout.BeginVertical ("Box");
				EditorGUILayout.BeginHorizontal ();
				EditorGUILayout.Space();
				GUILayout.Label ("Night");
				EditorGUILayout.EndHorizontal ();
				EditorGUILayout.Space();
				EditorGUILayout.BeginHorizontal ();
				GUILayout.Label("Ambient Color");
				Target.nightAmbientColor = EditorGUILayout.ColorField(Target.nightAmbientColor,GUILayout.Width(50));
				EditorGUILayout.EndHorizontal ();
				Target.nightAmbientIntensity = EditorGUILayout.Slider("Ambient Intensity",Target.nightAmbientIntensity,0.0f, 1.0f);
				EditorGUILayout.Space();
				EditorGUILayout.EndVertical ();
				
				EditorGUILayout.Space();
				Target.ambientTransitionSpeed = EditorGUILayout.Slider("Tansition Speed",Target.ambientTransitionSpeed,1.0f, 7.5f);
				
				break;
			}
			EditorGUILayout.Space();
			EditorGUILayout.EndVertical ();
			
			/// Button ///
			EditorGUILayout.Space();
			if(GUILayout.Button("Send Sky Material to Skybox"))
			{
				RenderSettings.skybox = Target.skyMat;
			}
			
		}
		EditorGUILayout.EndVertical ();
		
		
		
		//-------------------------------------------------------------------------------------------------------
		// Refresh the Inspector
		if (GUI.changed)
		{
			EditorUtility.SetDirty(target);
		}
	}
}
