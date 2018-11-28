﻿// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "azure[Sky]/Extra/v1.5/EmptySky"
{
	SubShader 
	{
	    Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
	    Cull [_Cull]  // Render side
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
			
			uniform float  sunDiskSize;
		    uniform float  sunDiskIntensity;
			
			uniform float  nightLuminance;
			uniform float  nightTopCloud;
			uniform float  dayLuminance;
			uniform float  horizonAltitude;
			uniform float  starsExtinction;
			uniform float  starsIntensity;
			uniform float  blueObscurance;
			
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
			
			uniform sampler2D    moonSampler;
			uniform samplerCUBE  starField;
			uniform samplerCUBE  starNoise;
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
    			
    			o.nightCompute.x = pow(max(0.0,o.WorldPos.y),starsExtinction);  				 			// Stars extinction from zenith to the horizon.
    			o.nightCompute.y = saturate(dot(float3(0, sunDir.y + 0.1, 0) , float3(0.0,-3.0,0.0))) * starsIntensity ;  // To stars fade in the sunrise.
    			
    			o.noiseRot = mul((float3x3)noiseMatrix,v.vertex.xyz); 							 			// Rotate noise texture to apply star scintillation

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
			          nightSky  *= nightLuminance;
			   float  moonShadow = tex2D(moonSampler, IN.moonPos.xy * shadowSize + 0.5 + float2(shadowX,shadowY)).a + shadowIntensity;  // Moon Shadow
			   float4 moonColor  = saturate( tex2D(moonSampler, IN.moonPos.xy * moonSize + float2(0.5,0.5)) * IN.Fade.z );				// Moon Color texture.
			   float  moonMask   = saturate(1.0 - (moonColor.b * 100));// To hide the stars that are behind the moon
			          moonColor *= moonShadow;
			   float3 moonLight  = saturate( (moonLightColor.rgb * moonLightIntensity) * pow(dot(viewDir, moonDir),5.0)  * IN.Fade.z );// Moonlight.
				
			   float  fadeStar      = IN.nightCompute.x * IN.nightCompute.y * (1.0-moonLight * 10.0); // When the stars will emerge and fade.
			   float  scintillation = texCUBE(starNoise, IN.noiseRot.xyz) * 2.0;
			   float3 stars         = saturate(texCUBE(starField, IN.moonPos.xyz) * fadeStar * moonMask) * scintillation;
			   

			   //////////////////////
			   // Sky finalization //
			   float3 finalSky = inScatter + (nightSky + moonColor.rgb + stars + moonLight);
			   
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
	}
	Fallback Off
}