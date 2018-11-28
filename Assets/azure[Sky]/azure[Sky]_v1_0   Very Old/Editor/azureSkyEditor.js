#pragma strict
@CustomEditor (SkyControl_v1_0)
class azureSkyEditor extends Editor 
{
    private var spaceColor : String[] = ["Default", "Linear To Gamma"];
	private var RenderSideNames : String[] = ["Double", "Front", "Back"];
	private var RenderSideValues : int[] = [0,1,2];
	
    function OnInspectorGUI()
    {
        var skyScript : SkyControl_v1_0 = target as SkyControl_v1_0;
        Undo.RecordObject(skyScript, "Undo Sky Values");
		
		/////////////////////////////////
		/////////OBJs & MATs Tab/////////////
        EditorGUILayout.BeginVertical ("Box");
        EditorGUILayout.BeginHorizontal ();
        EditorGUILayout.Space();
        skyScript.showObjMats = EditorGUILayout.Foldout(skyScript.showObjMats, "Obj & Mat");
        EditorGUILayout.EndHorizontal ();
        EditorGUILayout.Space();
        if(skyScript.showObjMats)
        {
            ///Sun ObjectField///
        	EditorGUILayout.BeginHorizontal ();
	        GUILayout.Label("Sun GameObject");
        	skyScript.SUN=EditorGUILayout.ObjectField(skyScript.SUN,GameObject,true,GUILayout.Width(125),GUILayout.Height(20)) as GameObject;
        	EditorGUILayout.EndHorizontal ();
        	///Moon ObjectField///
        	EditorGUILayout.BeginHorizontal ();
	        GUILayout.Label("Moon GameObject");
        	skyScript.MOON=EditorGUILayout.ObjectField(skyScript.MOON,GameObject,true,GUILayout.Width(125),GUILayout.Height(20)) as GameObject;
        	EditorGUILayout.EndHorizontal ();
        	///Sky MaterialField///
        	EditorGUILayout.BeginHorizontal ();
	        GUILayout.Label("Sky Material");
        	skyScript.SKYMAT=EditorGUILayout.ObjectField(skyScript.SKYMAT,Material,true,GUILayout.Width(125),GUILayout.Height(20)) as Material;
        	EditorGUILayout.EndHorizontal ();
        	///Fog MaterialField///
        	EditorGUILayout.BeginHorizontal ();
	        GUILayout.Label("Fog Material");
        	skyScript.FOGMAT=EditorGUILayout.ObjectField(skyScript.FOGMAT,Material,true,GUILayout.Width(125),GUILayout.Height(20)) as Material;
        	EditorGUILayout.EndHorizontal ();
        	///Moon MaterialField///
        	EditorGUILayout.BeginHorizontal ();
	        GUILayout.Label("Moon Material");
        	skyScript.MOONMAT=EditorGUILayout.ObjectField(skyScript.MOONMAT,Material,true,GUILayout.Width(125),GUILayout.Height(20)) as Material;
        	EditorGUILayout.EndHorizontal ();
        	EditorGUILayout.Space();
        }
        EditorGUILayout.EndVertical ();
        
        //////////////////////////////////
		/////////Scattering Tab///////////
        EditorGUILayout.BeginVertical ("Box");
        EditorGUILayout.BeginHorizontal ();
        EditorGUILayout.Space();
        skyScript.showScattering = EditorGUILayout.Foldout(skyScript.showScattering, "Scattering");
        EditorGUILayout.EndHorizontal ();
        EditorGUILayout.Space();
        
        if (skyScript.showScattering)
        {
	        skyScript.RayCoeff=EditorGUILayout.Slider("Rayleigh",skyScript.RayCoeff,0, 20);
	        skyScript.MieCoeff=EditorGUILayout.Slider("Mie",skyScript.MieCoeff,0, 0.1);
	        skyScript.Turbidity=EditorGUILayout.Slider("Turbidity",skyScript.Turbidity,0, 5);
	        skyScript.G=EditorGUILayout.Slider("G",skyScript.G,0, 0.99);
	        skyScript.SunIntensity=EditorGUILayout.Slider("Sun intensity",skyScript.SunIntensity,250, 1000);
	        skyScript.MoonIntensity=EditorGUILayout.Slider("Moon intensity",skyScript.MoonIntensity,2, 25);
	        EditorGUILayout.Space();
	        ///Standard Settings Button/// Reset variables
	        EditorGUILayout.BeginHorizontal ();
	        EditorGUILayout.Space();
	        if(GUILayout.Button("Standard Settings"))
	        {
	            skyScript.RayCoeff=3.0;
	            skyScript.MieCoeff=0.05;
	            skyScript.Turbidity=1.0;
	            skyScript.G=0.75;
	            skyScript.SunIntensity=500.0;
	            skyScript.MoonIntensity=10.0;
	        }
	        EditorGUILayout.Space();
	        EditorGUILayout.EndHorizontal ();
	        EditorGUILayout.Space();
		}
		EditorGUILayout.EndVertical ();
        
		//////////////////////////////////
		/////////Textures Tab/////////////
        EditorGUILayout.BeginVertical ("Box");
        EditorGUILayout.BeginHorizontal ();
        EditorGUILayout.Space();
        skyScript.showTextures = EditorGUILayout.Foldout(skyScript.showTextures, "Textures");
        EditorGUILayout.EndHorizontal ();
        EditorGUILayout.Space();
        
        if(skyScript.showTextures)
        {
	        ///BaseSky Texture///
	        EditorGUILayout.BeginHorizontal ();
	        GUILayout.Label("Base (Empty Sky)");
	        skyScript.EmptySkyTexture=EditorGUILayout.ObjectField(skyScript.EmptySkyTexture,Texture2D,true,GUILayout.Width(60),GUILayout.Height(60)) as Texture2D;
	        EditorGUILayout.EndHorizontal ();
	        
	        ///Cloud Layer1///
	        EditorGUILayout.BeginHorizontal ();
	        GUILayout.Label("Clouds (Layer 1)");
	        skyScript.Cloud1=EditorGUILayout.ObjectField(skyScript.Cloud1,Texture2D,true,GUILayout.Width(60),GUILayout.Height(60)) as Texture2D;
	        EditorGUILayout.EndHorizontal ();
	        
	        ///Cloud Layer2///
	        EditorGUILayout.BeginHorizontal ();
	        GUILayout.Label("Clouds (Layer 2)");
	        skyScript.Cloud2=EditorGUILayout.ObjectField(skyScript.Cloud2,Texture2D,true,GUILayout.Width(60),GUILayout.Height(60)) as Texture2D;
	        EditorGUILayout.EndHorizontal ();
	        
	        ///StarField///
	        EditorGUILayout.BeginHorizontal ();
	        GUILayout.Label("Star Field");
	        skyScript.StarField=EditorGUILayout.ObjectField(skyScript.StarField,Texture2D,true,GUILayout.Width(60),GUILayout.Height(60)) as Texture2D;
	        EditorGUILayout.EndHorizontal ();
	        
	        ///Star Noise/// ==> For Scintillation
	        EditorGUILayout.BeginHorizontal ();
	        GUILayout.Label("Star Noise Scintillation");
	        skyScript.StarNoise=EditorGUILayout.ObjectField(skyScript.StarNoise,Texture2D,true,GUILayout.Width(60),GUILayout.Height(60)) as Texture2D;
	        EditorGUILayout.EndHorizontal ();
	        
	        ///Moon Texture///
	        EditorGUILayout.BeginHorizontal ();
	        GUILayout.Label("Moon Texture");
	        skyScript.MoonTexture=EditorGUILayout.ObjectField(skyScript.MoonTexture,Texture2D,true,GUILayout.Width(60),GUILayout.Height(60)) as Texture2D;
	        EditorGUILayout.EndHorizontal ();
	        
	        ///Moon Shadow///
	        EditorGUILayout.BeginHorizontal ();
	        GUILayout.Label("Moon Shadow");
	        skyScript.MoonShadow=EditorGUILayout.ObjectField(skyScript.MoonShadow,Texture2D,true,GUILayout.Width(60),GUILayout.Height(60)) as Texture2D;
	        EditorGUILayout.EndHorizontal ();
	        
	        EditorGUILayout.Space();
	    }
	    EditorGUILayout.EndVertical ();
	    
	    //////////////////////////////////
		/////////Sky Settings Tab/////////
        EditorGUILayout.BeginVertical ("Box");
        EditorGUILayout.BeginHorizontal ();
        EditorGUILayout.Space();
        skyScript.showSkySettings = EditorGUILayout.Foldout(skyScript.showSkySettings, "Sky Settings");
        EditorGUILayout.EndHorizontal ();
        EditorGUILayout.Space();
        
        if (skyScript.showSkySettings)
        {
        	///Sunset Color///
	        EditorGUILayout.BeginHorizontal ();
	        GUILayout.Label("Sunset/Sunrise Color");
	        skyScript.SunsetColor = EditorGUILayout.ColorField(skyScript.SunsetColor,GUILayout.Width(50));
	        EditorGUILayout.EndHorizontal ();
	        ///Moonset Color///
	        EditorGUILayout.BeginHorizontal ();
	        GUILayout.Label("Moonset/Moonrise Color");
	        skyScript.MoonsetColor = EditorGUILayout.ColorField(skyScript.MoonsetColor,GUILayout.Width(50));
	        EditorGUILayout.EndHorizontal ();
	        
	        skyScript.SkyExposure=EditorGUILayout.Slider("Sky Exposure",skyScript.SkyExposure,0.0, 2.0);
	        skyScript.DayLuminance=EditorGUILayout.Slider("Day Luminance",skyScript.DayLuminance,1.16, 0.75);
	        skyScript.NightLuminance=EditorGUILayout.Slider("Night Luminance",skyScript.NightLuminance,1.15, 0.75);
	        skyScript.DayNightTrans=EditorGUILayout.Slider("Day/Night Speed Transition",skyScript.DayNightTrans,0.0, 50.0);
	        skyScript.Cloud1Speed=EditorGUILayout.Slider("Cloud Layer1 Speed",skyScript.Cloud1Speed,0.0, 0.25);
	        skyScript.Cloud2Speed=EditorGUILayout.Slider("Cloud Layer2 Speed",skyScript.Cloud2Speed,0.0, 0.25);
	        skyScript.Layer1_startRot=EditorGUILayout.Slider("Cloud Layer1 Initial Rotation",skyScript.Layer1_startRot,-1.0, 1.0);
	        skyScript.Layer2_startRot=EditorGUILayout.Slider("Cloud Layer2 Initial Rotation",skyScript.Layer2_startRot,-1.0, 1.0);
	        skyScript.StarScintillation=EditorGUILayout.Slider("Star Scintillation Speed",skyScript.StarScintillation,0.0, 5.0);
	        skyScript.StarIntensity=EditorGUILayout.Slider("Star Intensity",skyScript.StarIntensity,0.0, 10.0);
	        skyScript.StarExtinction=EditorGUILayout.Slider("Star Extinction",skyScript.StarExtinction,0.1, 1.5);
	        skyScript.StarBlend=EditorGUILayout.Slider("Star Blend",skyScript.StarBlend,0.5, 3.0);
	        skyScript.StarRot=EditorGUILayout.Slider("Star Starting Rotation",skyScript.StarRot,0.0, 1.0);
	        skyScript.SunsetAlt=EditorGUILayout.Slider("Sunset Initial Altitude",skyScript.SunsetAlt,0.0, 0.5);
	        skyScript.MoonsetAlt=EditorGUILayout.Slider("Moonset Initial Altitude",skyScript.MoonsetAlt,0.0, 0.25);
	        skyScript.HorizonAlt=EditorGUILayout.Slider("Horizon Altitude",skyScript.HorizonAlt,0.0, 1.0);

	        EditorGUILayout.Space();
	        ///Standard Settings Button/// Reset variables
	        EditorGUILayout.BeginHorizontal ();
	        EditorGUILayout.Space();
	        if(GUILayout.Button("Standard Settings"))
	        {
	        	skyScript.SunsetColor=Color(1.0,0.713,0.515,1.0);
	        	skyScript.MoonsetColor=Color(1.0,0.865,0.706,1.0);
	        	skyScript.SkyExposure=1.0;
	            skyScript.DayLuminance=1.143;
	            skyScript.NightLuminance=1.125;
	            skyScript.DayNightTrans=10;
	            skyScript.Cloud1Speed=0.075;
	            skyScript.Cloud2Speed=0.045;
	            skyScript.Layer1_startRot=0.0;
	            skyScript.Layer2_startRot=0.0;
	            skyScript.StarScintillation=1.0;
	            skyScript.StarIntensity=7.5;
	            skyScript.StarExtinction=0.5;
	            skyScript.StarBlend=2.0;
	            skyScript.StarRot=0;
	            skyScript.SunsetAlt=0.25;
	            skyScript.MoonsetAlt=0.0;
	            skyScript.HorizonAlt=1;
	        }
	        EditorGUILayout.Space();
	        EditorGUILayout.EndHorizontal ();
	        EditorGUILayout.Space();
		}
		EditorGUILayout.EndVertical ();
		
		//////////////////////////////////
		/////////Fog Settings Tab/////////
        EditorGUILayout.BeginVertical ("Box");
        EditorGUILayout.BeginHorizontal ();
        EditorGUILayout.Space();
        skyScript.showFogSettings = EditorGUILayout.Foldout(skyScript.showFogSettings, "Fog Settings");
        EditorGUILayout.EndHorizontal ();
        EditorGUILayout.Space();
        
        if (skyScript.showFogSettings)
        {
        	///Normal fog Color///
        	EditorGUILayout.BeginHorizontal ();
	        GUILayout.Label("Normal Fog Color");
	        skyScript.NormalFogColor = EditorGUILayout.ColorField(skyScript.NormalFogColor,GUILayout.Width(50));
	        EditorGUILayout.EndHorizontal ();
	        
	        skyScript.ScatterFogDis=EditorGUILayout.Slider("Scattering Fog Distance",skyScript.ScatterFogDis,0.0, 0.001);
	        skyScript.ScatterFogDen=EditorGUILayout.Slider("Scattering Fog Density",skyScript.ScatterFogDen,0.0, 1.0);
	        skyScript.NormalFogDis=EditorGUILayout.Slider("Normal Fog Distance",skyScript.NormalFogDis,skyScript.ScatterFogDis,0.1);
	        skyScript.NormalFogDen=EditorGUILayout.Slider("Normal Fog Density",skyScript.NormalFogDen,0.0,1.0);
	        
	        EditorGUILayout.Space();
	        EditorGUILayout.Space();
	        GUILayout.Label("Normal Fog Min/Max ==> Darkness/Brightness");
			EditorGUILayout.MinMaxSlider(skyScript.FogMin, skyScript.FogMax,0.0, 1.0);
	        
	        EditorGUILayout.Space();
	        ///Standard Settings Button/// Reset variables
	        EditorGUILayout.BeginHorizontal ();
	        EditorGUILayout.Space();
	        if(GUILayout.Button("Standard Settings"))
	        {
	            skyScript.NormalFogColor=Color.white;
	            skyScript.ScatterFogDis=0.00024;
	            skyScript.ScatterFogDen=1.0;
	            skyScript.NormalFogDis=0.1;
	            skyScript.NormalFogDen=0.9;
	            skyScript.FogMin=0.075;
	            skyScript.FogMax=0.75;
	        }
	        EditorGUILayout.Space();
	        EditorGUILayout.EndHorizontal ();
	        EditorGUILayout.Space();
		}
		EditorGUILayout.EndVertical ();
		
		//////////////////////////////////
		/////////Moon Settings Tab////////
        EditorGUILayout.BeginVertical ("Box");
        EditorGUILayout.BeginHorizontal ();
        EditorGUILayout.Space();
        skyScript.showMoonSettings = EditorGUILayout.Foldout(skyScript.showMoonSettings, "Moon Settings");
        EditorGUILayout.EndHorizontal ();
        EditorGUILayout.Space();
        
        if (skyScript.showMoonSettings)
        {
            ///Moon Horizon Color///
	        EditorGUILayout.BeginHorizontal();
	        GUILayout.Label("Horizon Color");
	        skyScript.MoonHorizonColor = EditorGUILayout.ColorField(skyScript.MoonHorizonColor,GUILayout.Width(50));
	        EditorGUILayout.EndHorizontal ();
	        ///Moon Zenith Color///
	        EditorGUILayout.BeginHorizontal();
	        GUILayout.Label("Zenith Color");
	        skyScript.MoonZenithColor = EditorGUILayout.ColorField(skyScript.MoonZenithColor,GUILayout.Width(50));
	        EditorGUILayout.EndHorizontal ();
            
			skyScript.MoonShadowX=EditorGUILayout.Slider("Shadow X Axis",skyScript.MoonShadowX,1.0, -1.0);
			skyScript.MoonShadowY=EditorGUILayout.Slider("Shadow Y Axis",skyScript.MoonShadowY,1.0, -1.0);
			skyScript.MoonShadowSize=EditorGUILayout.Slider("Shadow Size",skyScript.MoonShadowSize,1.0, 0.0);
			skyScript.MoonShadowIntensity=EditorGUILayout.Slider("Shadow Intensity",skyScript.MoonShadowIntensity,0.0, 1.0);
			skyScript.MoonColorIntensity=EditorGUILayout.Slider("Moon Color Intensity",skyScript.MoonColorIntensity,0.0, 1.0);
	        EditorGUILayout.Space();
		}
		EditorGUILayout.EndVertical ();
		
		//////////////////////////////////
		/////////Time of Day Tab//////////
        EditorGUILayout.BeginVertical ("Box");
        EditorGUILayout.BeginHorizontal ();
        EditorGUILayout.Space();
        skyScript.showTime_of_Day = EditorGUILayout.Foldout(skyScript.showTime_of_Day, "Time of Day");
        EditorGUILayout.EndHorizontal ();
        EditorGUILayout.Space();
        
        if (skyScript.showTime_of_Day)
        {
	        skyScript.CURRENT_TIME=EditorGUILayout.Slider("Current Time",skyScript.CURRENT_TIME,0.0, 24.0);
	        skyScript.UTC=EditorGUILayout.IntSlider("UTC",skyScript.UTC,-12.0, 12.0);
	        skyScript.Longitude=EditorGUILayout.Slider("Longitude",skyScript.Longitude,-180.0, 180.0);
	        
	        EditorGUILayout.BeginHorizontal ();
	        GUILayout.Label("Day Duration in Minutes");
		    skyScript.DAY_CYCLE_IN_MINUTES = EditorGUILayout.FloatField(skyScript.DAY_CYCLE_IN_MINUTES,GUILayout.Width(50));
		    EditorGUILayout.EndHorizontal ();	
	        EditorGUILayout.Space();
		}
		EditorGUILayout.EndVertical ();

		//////////////////////////////////
		/////////Options Tab//////////////
        EditorGUILayout.BeginVertical ("Box");
        EditorGUILayout.BeginHorizontal ();
        EditorGUILayout.Space();
        skyScript.showOptions = EditorGUILayout.Foldout(skyScript.showOptions, "Options");
        EditorGUILayout.EndHorizontal ();
        EditorGUILayout.Space();
        
        if (skyScript.showOptions)
        {
	        EditorGUILayout.BeginHorizontal ();
	        GUILayout.Label("Render Side ");
	        skyScript.RenderSide=EditorGUILayout.IntPopup(skyScript.RenderSide, RenderSideNames, RenderSideValues,GUILayout.Width(50));
	        EditorGUILayout.EndHorizontal ();
	        
	        ///Space Color///
			skyScript.spaceColorIndex = EditorGUILayout.Popup("Space Color",skyScript.spaceColorIndex, spaceColor);
			switch(skyScript.spaceColorIndex)
			{
			case 0:
				skyScript.colorCorrection = 1.0f;
				break;
			case 1:
				skyScript.colorCorrection = 2.2f;
				break;
			}
	        
	        
	        EditorGUILayout.BeginHorizontal ();
	        EditorGUILayout.Space();
	        if(GUILayout.Button("Send To Skybox"))
	        {
	         	RenderSettings.skybox=skyScript.SKYMAT;
	        }
	        EditorGUILayout.Space();
	        EditorGUILayout.EndHorizontal ();
	        EditorGUILayout.Space();
		}
		EditorGUILayout.EndVertical ();
	    
	    // Update the Inspector
	    if (GUI.changed)
	    {
	       EditorUtility.SetDirty(target);
	    }
    }
}