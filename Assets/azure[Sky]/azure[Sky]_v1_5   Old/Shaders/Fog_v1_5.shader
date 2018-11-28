// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "azure[Sky]/v1_5/Fog Scattering"
{
	SubShader 
	{
	    Cull Front    // Render side
		Fog{Mode Off} // Don't use fog
    	ZWrite Off    // Don't draw to bepth buffer
    	ZTest Always
    	Pass 
    	{
			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			#pragma multi_compile HDR_ON HDR_OFF
			
			uniform float4 sunsetColor;
			uniform float  nightIntensity;
			uniform float  nightLuminance;
			uniform float  dayLuminance;
			uniform float  horizonAltitude;
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
			uniform float  linearFog;
			
			uniform float  sunIntensity;
			uniform float  moonLightIntensity;
			uniform float4 moonLightColor;
			
			uniform sampler2D     emptySky;
			uniform sampler2D 	 _MainTex;
			uniform sampler2D    _CameraDepthTexture;
			uniform float4x4     _FrustumCorners;
			uniform float4       _MainTex_TexelSize;
			
			uniform float4  normalFog_sunsetColor;
			uniform float4  normalFog_noonColor;
			uniform float4  normalFog_nightColor;
			uniform float   normalFogDistance;
			uniform float   inScatterFogDistance;
			uniform float   mixFogDistance;
			
			struct appdata{
			    float4 vertex   : POSITION;
			    float4 texcoord : TEXCOORD0;
			};

			struct v2f 
			{
    			float4 Position        : SV_POSITION;
    			float2 Fade            : TEXCOORD0; // sunFade,  mix.
    			float3 moonPos         : TEXCOORD1;
    			float2 uv 	           : TEXCOORD2;
				float4 interpolatedRay : TEXCOORD3;
				float2 uv_depth        : TEXCOORD4;
			};

			v2f vert(appdata v)
			{
    			v2f o;
    			UNITY_INITIALIZE_OUTPUT(v2f, o);
    			
    			half index = v.vertex.z;
				v.vertex.z = 0.1;
				o.uv       = v.texcoord.xy;
				o.uv_depth = v.texcoord.xy;
				#if UNITY_UV_STARTS_AT_TOP
				if (_MainTex_TexelSize.y < 0)
					o.uv.y = 1-o.uv.y;
				#endif
				o.interpolatedRay   = _FrustumCorners[(int)index];
				o.interpolatedRay.w = index;
    			
    			o.Position = UnityObjectToClipPos(v.vertex);
    			
    			o.Fade.x = saturate( sunDir.y+0.25 );                             		 	     			// Fade the sun ("daysky") when cross the horizon.
			    o.Fade.y = saturate(clamp(1.0 - sunDir.y,0.0,0.5));                          	 			// Mix sunset"(fex)" with daysky"(1-fex)".

    			return o;
			}
			
			float4 frag(v2f IN) : SV_Target
			{
               ////////////////
			   // Directions //
			   float  depth       = Linear01Depth(UNITY_SAMPLE_DEPTH(tex2D(_CameraDepthTexture,IN.uv_depth)));
               float3 viewDir     = normalize(depth * IN.interpolatedRay);
			   float sunCosTheta  = dot( viewDir, sunDir );
			   viewDir            = normalize(depth * IN.interpolatedRay) + horizonAltitude; // Change the horizon altitude. "(0.1=HorAlt)"
			   
			   ////////////////
			   // Extinction //
			   float  zenith = acos(saturate(viewDir.y));
//			   zenith = (acos(max(0, dot(float3(0,1,0), sunDir))));
			   float  z      = (cos(zenith) + 0.15 * pow(93.885 - ((zenith * 180.0) / pi), -1.253));
			   float  SR     = 8.4  / z;
			   float  SM     = 1.25 / z;
			   float3 fex    = exp(-(Br*SR + Bm*SM));  // Original fex calculation.
			   float3 fex2   = exp(-(Br2*SR + Bm*SM)); // Fex calculation with rayleigh coefficient == 3. For the sunset.
			   
			   ////////////
			   // Clouds //
			   sunsetColor  = lerp(sunsetColor,1.0,saturate(dot(sunDir-0.5, float3(0.0,1,0.0))));
			   float2 uv    = float2(atan2(viewDir.z, viewDir.x), -acos(viewDir.y)) / float2(2.0*pi, pi);
			   float4 empty = tex2D(emptySky, uv);
			   
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
			    
			   float3 inScatter = BrmTheta * sunIntensity * (1.0 - fex);
			   inScatter *= saturate((lerp( 3.5 , pow(2000.0 * BrmTheta * fex2,0.5),IN.Fade.y) * 0.05));
			   inScatter *= (empty.rgb * dayLuminance) * sunsetColor.rgb;
			   inScatter *= pow((1-fex),blueObscurance);
			   inScatter *= IN.Fade.x; // Sun fade in the horizon.
			   
			   
			   ///////////////
			   // Night Sky //
			   ///////////////
			   float3 nightSky   = (pow( 1.0-fex2, 3.0) * nightIntensity) * (1-IN.Fade.x);	// Defaut night sky color
			          nightSky  *= empty.rgb * nightLuminance;
			   

			   //////////////////////
			   // Sky finalization //
			   float3 finalSky = inScatter + nightSky;
			   
		   	   ////////////////
			   // tonemaping //
			   #ifndef HDR_ON
			   finalSky = saturate( 1.0 - exp( -Exposure * finalSky ) );
			   #endif
			   
			   
			   //////////////////////
			   // Color Correction //
			   finalSky = pow(finalSky,colorCorrection);
			   
			   ///////////////
			   // Apply Fog //
			   																				    // Mask depth Buffer
			   float  Mask       =    saturate( lerp(1.0, 0.0, depth) * _ProjectionParams.z );	// White for the pixels that draw to the depth buffer
																							    // Blak for pixels that do not draw for the depth buffer
																							    
																							    
			   float3 screen     =    tex2D(_MainTex, IN.uv);                       						// Original scene
			   
			   
			   float3 normalFogCol =  lerp(normalFog_sunsetColor,normalFog_noonColor,saturate(sunDir.y));
			          normalFogCol =  lerp(normalFog_nightColor,normalFogCol, saturate(sunDir.y+0.75));
			   float3 normalFog    =  lerp(screen,normalFogCol, Mask);
			          normalFog    =  lerp(screen,normalFog, saturate(pow(depth * normalFogDistance,linearFog)));
			          normalFog    =  pow(normalFog,colorCorrection);
			   
			   
			   float3 inScatteringFog =    lerp(screen,finalSky, Mask);                  						                 // Creating the fog color.
			          inScatteringFog =    lerp(screen, inScatteringFog, saturate(pow(depth * inScatterFogDistance,linearFog))); // Mixing the fog with the scene, according to the depth.
			   
			   float3 finalFog = lerp(normalFog, inScatteringFog, saturate(pow(depth * mixFogDistance,linearFog)));

			   
			   return float4(finalFog,1.0);
//			   return float4(Mask,Mask,Mask,1.0);             // To see the mask
//			   return float4(depth*10,depth*10,depth*10,1.0); // To see the depth
			}
			ENDCG
    	}
	}
	Fallback Off
}