﻿// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "azure[Sky]/v2_0/Simple Cloud"
{
	SubShader 
	{
	    Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" "IgnoreProjector"="True" }
	    Cull Back     // Render side
		Fog{Mode Off} // Don't use fog
    	ZWrite Off    // Don't draw to bepth buffer
    	Pass 
    	{
			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile HDR_ON HDR_OFF
			
			uniform float4 sunsetColor;
			uniform float  nightIntensity;
			uniform float  nightLuminance;
			
			uniform float  sunDiskSize;
		    uniform float  sunDiskIntensity;
			
			uniform float  dayLuminance;
			uniform float  horizonAltitude;
			uniform float  starsExtinction;
			uniform float  starsIntensity;
			
			uniform float  blueObscurance;
			uniform float  dayBlueObscurance;
			uniform float  nightBlueObscurance;
			
			uniform float3 sunDir;
			uniform float3 moonDir;
			uniform float3 Br;
			uniform float3 Br2;
			uniform float3 Bm;
			uniform float3 Brm;    //Br + Bm
			uniform float3 mieG;
			
			uniform float  pi316;
			uniform float  pi14;
			uniform float  pi;
			
			uniform float  Exposure;
			uniform float  colorCorrection;
			
			uniform float  moonSize;
			uniform float  sunIntensity;
			uniform float  moonLightIntensity;
			uniform float4 moonLightColor;
			
			uniform sampler2D    _Cloud1;
			uniform float        Longitude;
			uniform float        _AlphaSaturation;
			
			uniform sampler2D    moonSampler;
			uniform samplerCUBE  starField;
			uniform samplerCUBE  starNoise;
			uniform samplerCUBE  _MilkyWay;
			
			uniform float        _milkyWayIntensity;
			uniform float        _milkyWayPow;
			uniform float4x4     _milkyWayMatrix;
			
			uniform float4x4     moonMatrix;
			uniform float4x4     noiseMatrix;
			uniform float        shadowX;
			uniform float        shadowY;
			uniform float        shadowSize;
			uniform float        shadowIntensity;
			
			struct appdata{
			    float4 vertex   : POSITION;
			};

			struct v2f 
			{
    			float4 Position     : SV_POSITION;
    			float3 WorldPos     : TEXCOORD0;
    			float3 Fade         : TEXCOORD1; // sunFade,  mix,  fadeOtherSideMoon.
    			float3 moonPos      : TEXCOORD2;
    			float2 nightCompute : TEXCOORD3;
    			float3 noiseRot     : TEXCOORD4;
//    			float3 milkyWayPos  : TEXCOORD5;
			};

			v2f vert(appdata v)
			{
    			v2f o;
    			UNITY_INITIALIZE_OUTPUT(v2f, o);
    			
    			o.Position = UnityObjectToClipPos(v.vertex);
    			o.WorldPos = normalize(mul(unity_ObjectToWorld, v.vertex).xyz);
    			
    			o.Fade.x = saturate( sunDir.y+0.25 );                             		 	     			// Fade the sun ("daysky") when cross the horizon.
			    o.Fade.y = saturate(clamp(1.0 - sunDir.y,0.0,0.5));                          	 			// Mix sunset"(fex)" with daysky"(1-fex)".
			    o.Fade.z = saturate(dot(-moonMatrix[2].xyz,o.WorldPos)) * (o.WorldPos.y + horizonAltitude); // Fade  the other side moon generated by the moonMatrix...
			    																		 		 			//and fade the moon when cross the horizon.
    			
    			// For the rotation of the moon
    			o.moonPos.x = dot(moonMatrix[0].xyz,v.vertex.xyz);
    			o.moonPos.y = dot(moonMatrix[1].xyz,v.vertex.xyz);
    			o.moonPos.z = dot(moonMatrix[2].xyz,v.vertex.xyz);
    			
    			o.nightCompute.x = pow(max(0.0,o.WorldPos.y),starsExtinction);  				 			             // Stars extinction from zenith to the horizon.
    			o.nightCompute.y = saturate(dot(float3(0, sunDir.y + 0.1, 0), float3(0.0,-3.0,0.0))) * starsIntensity ;  // To stars fade in the sunrise.
    			
    			o.noiseRot = mul((float3x3)noiseMatrix,v.vertex.xyz); 							 			             // Rotate noise texture to apply star scintillation
//    			o.milkyWayPos = mul((float3x3)_milkyWayMatrix, v.vertex.xyz);
    			return o;
			}
			
			float4 frag(v2f IN) : SV_Target
			{
               ////////////////
			   // Directions //
               float3 viewDir     = normalize(IN.WorldPos);
			   float sunCosTheta  = dot( viewDir, sunDir );
			   viewDir            = normalize(IN.WorldPos+float3(0.0,horizonAltitude,0.0)); // Change the horizon altitude. "(0.1=HorAlt)"
			   
			   ////////////////
			   // Extinction //
			   float  zenith = acos(saturate(viewDir.y));
			   float  z      = (cos(zenith) + 0.15 * pow(93.885 - ((zenith * 180.0) / pi), -1.253));
			   float  SR     = 8.4  / z;
			   float  SM     = 1.25 / z;
			   float3 fex    = exp(-(Br*SR + Bm*SM));  // Original fex calculation.
			   float3 fex2   = exp(-(Br2*SR + Bm*SM)); // Fex calculation with rayleigh coefficient == 3. For the sunset.
			   
			   ////////////////
			   // Cloud Mask //
			   float2 uv = float2(-atan2(viewDir.z, viewDir.x), -acos(viewDir.y)) / float2(2.0*pi, pi);
				      uv = (uv - float2(Longitude, 0)) + float2(0.35, 0);
			   float4 tex       = tex2D(_Cloud1, uv);
			   float  cloudMask = (1-pow( tex.a, _AlphaSaturation)) * fex.b;
			   
			   ///////////////////
			   // Sun inScatter //
			   //  for  daysky  //
			   ///////////////////
			   //float  rayPhase = 1.0 + pow(cosTheta,2.0);                       // Preetham rayleigh phase function.
			   float  rayPhase = 2.0 + 0.5 * pow(sunCosTheta,2.0);                // Rayleigh phase function based on the Nielsen's paper.
			   float  miePhase = mieG.x / pow(mieG.y - mieG.z * sunCosTheta,1.5); // The Henyey-Greenstein phase function.
			    
			   float3 BrTheta  = pi316 * Br * rayPhase;
			   float3 BmTheta  = pi14  * Bm * miePhase;
			   float3 BrmTheta = (BrTheta + BmTheta * 2.0) / (Brm * 0.75);        // Brm is "Br+Bm", and the sum is already made in the Control Script.
			   
			   blueObscurance = lerp(nightBlueObscurance, dayBlueObscurance, saturate( sunDir.y+0.25 ));
			   sunsetColor  = lerp(sunsetColor,1.0,sunDir.y);
			   float3 inScatter = BrmTheta * sunIntensity * (1.0 - fex);
			   inScatter *= saturate((lerp( 3.5 , pow(2000.0 * BrmTheta * fex2,0.5),IN.Fade.y) * 0.05));
			   inScatter *= dayLuminance * sunsetColor.rgb;
			   inScatter *= pow((1-fex),blueObscurance);
			   inScatter *= IN.Fade.x; // Sun fade in the horizon.
			   
			   ////////////////
			   // Solar Disk //
			   float sunDisk  = smoothstep(sunDiskSize, sunDiskSize + 0.001, sunCosTheta) * sunDiskIntensity;
			   //sunDisk       *= pow(fex2,0.5);
			   
			   
			   ///////////////
			   // Night Sky //
			   ///////////////
			   float3 nightSky   = (pow( 1.0-fex2, 3.0) * nightIntensity) * (1-IN.Fade.x);												// Defaut night sky color
			   		  nightSky  *= pow((1-fex),blueObscurance);
			          nightSky  *= nightLuminance;
			   float  moonShadow = tex2D(moonSampler, IN.moonPos.xy * shadowSize + 0.5 + float2(shadowX,shadowY)).a + shadowIntensity;  // Moon Shadow
			   float4 moonColor  = saturate( tex2D(moonSampler, IN.moonPos.xy * moonSize + float2(0.5,0.5)) * IN.Fade.z );				// Moon Color texture.
			   float  moonMask   = saturate(1.0 - (moonColor.b * 100));// To hide the stars that are behind the moon
			          moonColor *= moonShadow;
			   float3 moonLight  = saturate( (moonLightColor.rgb * moonLightIntensity) * pow(dot(viewDir, moonDir),5.0)  * IN.Fade.z );// Moonlight.
				
			   float  fadeStar      = IN.nightCompute.x * IN.nightCompute.y * (1.0-moonLight * 10.0); // When the stars will emerge and fade.
			   float  scintillation = texCUBE(starNoise, IN.noiseRot.xyz) * 2.0;
			   float3 stars         = saturate(texCUBE(starField, IN.moonPos.xyz) * fadeStar * moonMask) * scintillation;
			   float3 milkyWay      = pow(texCUBE(_MilkyWay, mul((float3x3)_milkyWayMatrix, IN.moonPos.xyz)), _milkyWayPow) * _milkyWayIntensity * fadeStar;
			   

			   //////////////////////
			   // Sky finalization //
			   float3 finalSky = inScatter + (nightSky + ((moonColor.rgb + stars + milkyWay) * cloudMask) + moonLight);
			   
		   	   ////////////////
			   // tonemaping //
			   #ifndef HDR_ON
			   finalSky = saturate( 1.0 - exp( -Exposure * finalSky )) + (sunDisk * pow(fex2,0.5) * 0.25);
			   #endif
			   #ifdef HDR_ON
			   finalSky += (sunDisk * pow(fex2,0.5) * 0.25);
			   #endif
			   
			   
			   //////////////////////
			   // Color Correction //
			   finalSky = pow(finalSky,colorCorrection);
			   
			   return float4(finalSky,1.0);
			}
			ENDCG
    	}
    	
    	
    	/////////////////////////
    	/////////CLOUD///////////
    	Pass 
    	{
    		Blend SrcAlpha OneMinusSrcAlpha
    		
			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile HDR_ON HDR_OFF
			
			uniform sampler2D    _Cloud1;
			
			uniform float3    sunDir;
			uniform float3    moonDir;
			uniform float     horizonAltitude;
			
			uniform float     dayLuminance;
			uniform float     nightIntensity;

			uniform float     pi;
			
			uniform float     _CloudExtinction;
			uniform float     _AlphaSaturation;
			
			uniform float     _SunsetCloudDensity;
			uniform float     _NoonCloudDensity;
			uniform float     _NightCloudDensity;
			
			
			uniform float4    _sunsetEdgeColor;
			uniform float4    _sunsetDarkColor;
			uniform float4    _noonEdgeColor;
			uniform float4    _noonDarkColor;
			uniform float4    _nightEdgeColor;
			uniform float4    _nightDarkColor;
			
			uniform float     moonLightIntensity;
			uniform float4    moonLightColor;
			uniform float     _MoonBright;
			uniform float     _MoonBrightRange;
			uniform float4x4  moonMatrix;
			
			uniform float     Longitude;
			uniform int       rise_or_down;
			
			uniform float     colorCorrection;
			
			
			struct appdata{
			    float4 vertex   : POSITION;
			};

			struct v2f 
			{
    			float4 Position     : SV_POSITION;
    			float3 WorldPos     : TEXCOORD0;
    			float3 Fade         : TEXCOORD1; // sunFade,  mix.
			};

			v2f vert(appdata v)
			{
    			v2f o;
    			UNITY_INITIALIZE_OUTPUT(v2f, o);
    			
    			o.Position = UnityObjectToClipPos(v.vertex);
    			o.WorldPos = normalize(mul(unity_ObjectToWorld, v.vertex).xyz);
    			
    			o.Fade.x = saturate( sunDir.y+0.25 );                  // Fade the sun ("daysky") when cross the horizon.
			    o.Fade.y = saturate(clamp(1.0 - sunDir.y,0.0,0.5));    // Mix sunset"(fex)" with daysky"(1-fex)".
			    o.Fade.z = saturate(dot(-moonMatrix[2].xyz,o.WorldPos)) * (o.WorldPos.y + horizonAltitude);

    			return o;
			}
			
			float4 frag(v2f IN) : SV_Target
			{
			    ////////////////
			    // Directions //
				float3 viewDir    = normalize(IN.WorldPos+float3(0.0,horizonAltitude,0.0));
				//Longitude = dot(sunDir,float3(1,0,0));
				
				//////////////
				// Textures //
				float2 uv    = float2(-atan2(viewDir.z, viewDir.x), -acos(viewDir.y)) / float2(2.0*pi, pi);
				       uv = (uv - float2(Longitude, 0)) + float2(0.35, 0);
				float4 tex   = tex2D(_Cloud1, uv);
				float  alpha = pow( tex.a, _AlphaSaturation) * saturate(IN.WorldPos.y * _CloudExtinction);
				
				///////////////
				// Day Cloud //
				float3 _EdgeColor = lerp(_sunsetEdgeColor,_noonEdgeColor,sunDir.y);
				float  rise = tex.r;
				float  up   = tex.g;
				float  down = tex.b;
				
				float3 cloudLuminance = lerp(rise,down,rise_or_down);
				       cloudLuminance = lerp(cloudLuminance,up,saturate(dot(sunDir,float3(0,1,0))));
			           
			    float3 sunsetCloud = lerp(_sunsetDarkColor,_EdgeColor * clamp(1,1.25,dayLuminance),saturate(pow(cloudLuminance,_SunsetCloudDensity)));
			    float3 noonCloud   = lerp(_noonDarkColor,  _EdgeColor * clamp(1,1.25,dayLuminance),saturate(pow(cloudLuminance,_NoonCloudDensity)));
				float3 dayCloud    = lerp(sunsetCloud,noonCloud,sunDir.y + 0.25);
				
				/////////////////
				// Night Cloud //
				cloudLuminance = lerp(rise,down,1-rise_or_down);
			    cloudLuminance = lerp(cloudLuminance,up,saturate(dot(moonDir,float3(0,1,0))));
				float3 moonLight   = saturate( (moonLightColor.rgb * (moonLightIntensity * _MoonBright)) * pow(dot(viewDir, moonDir),_MoonBrightRange)  * IN.Fade.z );// Moonlight.
				float3 nightCloud  = lerp(_nightDarkColor, _nightEdgeColor * lerp(1,1.25,nightIntensity) * 0.25 + moonLight,saturate(pow(cloudLuminance,_NightCloudDensity)));
				
				//////////////////
				// Finalization //
				float3 finalCloud = lerp(nightCloud, dayCloud, saturate(dot(float3(0, sunDir.y + 0.3, 0), float3(0,2.25,0))));
				
				//////////////////////
			    // Color Correction //
			    finalCloud = pow(finalCloud,colorCorrection);
				
            	return float4(finalCloud,alpha);
			}
			ENDCG
    	}
	}
	Fallback Off
}