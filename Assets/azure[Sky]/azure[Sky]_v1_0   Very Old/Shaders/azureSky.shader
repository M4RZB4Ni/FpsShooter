// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "azure[Sky]/v1_0/Sky"
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
			#include "azureSkyScatter.cginc"
			
			sampler2D Layer1; // cloud layer 1
			sampler2D Layer2; // cloud layer 2
			sampler2D Layer3; // stars map
			sampler2D Layer4; // noise map for stars scintillation
			half Layer1Speed,Layer2Speed; // clouds move speed
			half StarRange;  // intensity of the stars
			half StarBlend;  // Night time is emerging stars
			half StarExtinction;// Extinction of stars in the horizon
			half StarRot; // Stars map initial "horizon rotation"
			half ScintillationSpeed; // Scintillation speed of the stars
			half Layer1_startRot; // Cloud Layer1 initial rotation
			half Layer2_startRot; // Cloud Layer2 initial rotation
			half colorCorrection;

			struct v2f 
			{
    			float4 Position : SV_POSITION;
    			float3 WorldPos : TEXCOORD0;
			};

			v2f vert(appdata v)
			{
    			v2f o;
    			o.Position = UnityObjectToClipPos(v.vertex);
    			o.WorldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
    			return o;
			}
			
			half4 frag(v2f IN) : COLOR
			{
               float3 viewDir = normalize(IN.WorldPos);
			   half sunCosTheta = dot( viewDir, sunDir );
			   half moonCosTheta = dot( viewDir, moonDir);
			   IN.WorldPos.y+=HorizonAltitude*_ProjectionParams.z;// HorizonAltitude * Camera's Far Clipping Planes
			   viewDir = normalize(IN.WorldPos);
			   
			   half zenith= acos(max(0.0, dot(half3(0.0,1.0,0.0), viewDir)));
			   half SR = 8.4E3 / (cos(zenith) + 0.15 * pow(93.885 - ((zenith * 180.0) / pi), -1.253));
			   half SM = 1.25E3 / (cos(zenith) + 0.15 * pow(93.885 - ((zenith * 180.0) / pi), -1.253));
			   half3 fex=Fex(SR,SM);
			   half3 Brm=Br+Bm;
			   
			   //Textures=>// Clouds,Stars,Noise
			   SunsetColor=lerp(SunsetColor,half4(1,1,1,1),saturate(dot(sunDir-SunsetAltitude, half3(0.0,1,0.0))));
			   MoonsetColor=lerp(MoonsetColor,half4(1.0,1.0,1.0,1.0),dot(moonDir-MoonsetAltitude, half3(0.0,1.0,0.0)));
			   half speed=_Time;
			   half2 uv = float2(atan2(viewDir.z, viewDir.x), -acos(viewDir.y)) / half2(2.0*pi, pi);
			   half4 Cloud = ((tex2D(Layer1, uv+half2(Layer1Speed*speed+Layer1_startRot,0))+tex2D(Layer2, uv+half2(Layer2Speed*speed+Layer2_startRot,0)))*0.5);
			   half FexToFloat=lerp(float(0),float(1),fex);
			   half starFade=saturate(dot(sunDir, half3(0.0,-StarBlend,0.0)));
			   half3 Stars = ((StarRange*tex2D(Layer3, uv+half2(StarRot,0)).rgb)*tex2D(Layer4, uv+half2(speed*ScintillationSpeed,0)).rgb)*starFade;
			   Stars*=FexToFloat*StarExtinction;
			   
			   //Sun Scatter//
			   half Esun = sunIntensity(dot(sunDir, half3(0.0,1.0,0.0)));
			   half3 SLin = pow(Esun * ((BrTheta(sunCosTheta) + BmTheta(sunCosTheta)) / Brm) * (1.0 - fex),1.5);
			   SLin *= lerp(half3(3.5,3.5,3.5),pow(2000 * ((BrTheta(sunCosTheta) + BmTheta(sunCosTheta)) / Brm) * fex,0.75),clamp(1-dot(half3(0.0,2.0,0.0), sunDir),0.0,0.25))*0.04;
			   SLin += half3(0.0,0.0003,0.0075);
			   half intensityCloud = lerp(half(0),pow(Cloud.g,10)*FexToFloat,saturate(dot(sunDir-0.5, half3(0.0,7.5,0.0))));
			   SLin =((log2(2.0/pow(DayLuminance,4.0)))*SLin)*(pow((Cloud.rgb+intensityCloud)*SunsetColor,3.0));
			   SLin =pow(((SLin*(0.15*SLin+0.05)+0.004)/(SLin*(0.15*SLin+0.50)+0.06))-0.0666,0.4);
			   
			   //Moon Scatter//
			   half Emoon = MoomIntensity(dot(sunDir, half3(0.0,1.0,0.0)));
			   half3 MLin = pow(Emoon * ((BrTheta(moonCosTheta) + BmTheta(moonCosTheta)) / Brm) * (1.0 - fex),1.5);
			   MLin *= lerp(half3(3.5,3.5,3.5),pow(2000 * ((BrTheta(moonCosTheta) + BmTheta(moonCosTheta)) / Brm) * (1-fex),half3(0.75,0.75,0.75)),clamp(1-dot(half3(0.0,5.0,0.0), sunDir),0.0,0.25))*0.04;
			   MLin += half3(0.0,0.0003,0.00075)+Stars;
			   MLin =((log2(2.0/pow(NightLuminance,4.0)))*MLin)*(pow(Cloud.rgb*MoonsetColor,3.0));
			   MLin =pow(((MLin*(0.15*MLin+0.05)+0.004)/(MLin*(0.15*MLin+0.50)+0.06))-0.0666,0.4);
			   
			   half4 c;
			   c.rgb=lerp(saturate(MLin),saturate(SLin),saturate(dot(sunDir+0.1, half3(0.0,Time_of_Transition,0.0))));
			   c.a=1;
			   c*=SkyExposure;
			   c=pow(c,colorCorrection);
			   return c;
			}
			ENDCG
    	}
	}
	Fallback Off
}