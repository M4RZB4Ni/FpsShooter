// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "azure[Sky]/v1_0/Fog" 
{
	SubShader 
	{ 
	    ZTest Always Cull Front ZWrite Off Fog { Mode Off }
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#include "azureSkyScatter.cginc"
			#include "UnityCG.cginc"

			uniform sampler2D   _MainTex;//Texture of Screen
			uniform sampler2D   _EmptySkyTexture; // Sky Base "(Empty Sky)"
			uniform sampler2D   _CameraDepthTexture;
			uniform float4      _MainTex_TexelSize;
			uniform float4x4    _FrustumCornersWS;
			
			uniform half ScatterFogDistance,NormalFogDistance,ScatterFogDensity,NornalFogDensity;
			uniform half FogMin,FogMax;
			uniform half4 FogColor;
			half colorCorrection;

			struct v2f {
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				float4 interpolatedRay : TEXCOORD1;
				float4 projPos : TEXCOORD2;
				float2 uv_depth : TEXCOORD3;
				float3 WorldPos : TEXCOORD4;
			};
			
			v2f vert( appdata v )
			{
				v2f o;
				UNITY_INITIALIZE_OUTPUT(v2f, o);
				half index = v.vertex.z;
				v.vertex.z = 0.1;
				o.pos = UnityObjectToClipPos (v.vertex);
				o.uv = v.texcoord.xy;
				o.uv_depth = v.texcoord.xy;
				o.projPos = ComputeScreenPos(o.pos);
				#if UNITY_UV_STARTS_AT_TOP
				if (_MainTex_TexelSize.y < 0)
					o.uv.y = 1-o.uv.y;
				#endif
				o.interpolatedRay = _FrustumCornersWS[(int)index];
				//o.interpolatedRay.w = index;
				o.WorldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				return o;
			}
			
			half4 frag(v2f IN) : COLOR
			{
			    float depth = LinearEyeDepth (tex2Dproj(_CameraDepthTexture,UNITY_PROJ_COORD(IN.projPos)).r);
			    half maskDepth  = Linear01Depth(UNITY_SAMPLE_DEPTH(tex2D(_CameraDepthTexture,IN.uv_depth)));
			    float3 viewDir = normalize(IN.WorldPos+depth* IN.interpolatedRay);
			    half sunCosTheta = dot( viewDir, sunDir );
			    half moonCosTheta = dot( viewDir, moonDir);
			    viewDir = normalize(IN.WorldPos+depth*(IN.interpolatedRay.xyz+((HorizonAltitude/10)*_ProjectionParams.z)));
			    
			    half zenith= acos(max(0.0, dot(half3(0.0,1.0,0.0), viewDir)));
			    half SR = 8.4E3 / (cos(zenith) + 0.15 * pow(93.885 - ((zenith * 180.0) / pi), -1.253));
			    half SM = 1.25E3 / (cos(zenith) + 0.15 * pow(93.885 - ((zenith * 180.0) / pi), -1.253));
			    half3 fex=Fex(SR,SM);
			    half3 Brm=Br+Bm;
			    
			    //Textures=>// Only Base SkyTexture here, no need of clouds and stars
			    SunsetColor=lerp(SunsetColor,half4(1,1,1,1),saturate(dot(sunDir-SunsetAltitude, half3(0.0,1,0.0))));
			    MoonsetColor=lerp(MoonsetColor,half4(1.0,1.0,1.0,1.0),dot(moonDir-MoonsetAltitude, half3(0.0,1.0,0.0)));
			    half2 uv = half2(atan2(viewDir.z, viewDir.x), -acos(viewDir.y)) / half2(2.0*pi, pi);
			    half4 Base =tex2D(_EmptySkyTexture, uv);
			   
			     //Sun Scatter//
			   half Esun = sunIntensity(dot(sunDir, half3(0.0,1.0,0.0)));
			   half3 SLin = pow(Esun * ((BrTheta(sunCosTheta) + BmTheta(sunCosTheta)) / Brm) * (1.0 - fex),1.5);
			   SLin *= lerp(half3(3.5,3.5,3.5),pow(2000 * ((BrTheta(sunCosTheta) + BmTheta(sunCosTheta)) / Brm) * fex,0.75),clamp(1-dot(half3(0.0,2.0,0.0), sunDir),0.0,0.25))*0.04;
			   SLin += half3(0.0,0.0003,0.0075);
			   half fade=lerp(float(0),float(1),fex);
			   half intensityCloud = lerp(half(0),pow(Base.g,10)*fade,saturate(dot(sunDir-0.5, half3(0.0,7.5,0.0))));
			   SLin =((log2(2.0/pow(DayLuminance,4.0)))*SLin)*(pow((Base.rgb+intensityCloud)*SunsetColor,3.0));
			   SLin =pow(((SLin*(0.15*SLin+0.05)+0.004)/(SLin*(0.15*SLin+0.50)+0.06))-0.0666,0.4);
			   
			    //Moon Scatter//
			   half Emoon = MoomIntensity(dot(sunDir, half3(0.0,1.0,0.0)));
			   half3 MLin = pow(Emoon * ((BrTheta(moonCosTheta) + BmTheta(moonCosTheta)) / Brm) * (1.0 - fex),1.5);
			   MLin *= lerp(half3(3.5,3.5,3.5),pow(2000 * ((BrTheta(moonCosTheta) + BmTheta(moonCosTheta)) / Brm) * (1-fex),half3(0.75,0.75,0.75)),clamp(1-dot(half3(0.0,5.0,0.0), sunDir),0.0,0.25))*0.04;
			   MLin += half3(0.0,0.0003,0.00075);
			   MLin =((log2(2.0/pow(NightLuminance,4.0)))*MLin)*(pow(Base.rgb*MoonsetColor,3.0));
			   MLin =pow(((MLin*(0.15*MLin+0.05)+0.004)/(MLin*(0.15*MLin+0.50)+0.06))-0.0666,0.4);
			    
			    //Fog//
			    half3 Sky=lerp(saturate(MLin),saturate(SLin),saturate(dot(sunDir+0.1, half3(0.0,Time_of_Transition,0.0))));
			    Sky*=SkyExposure;
			    Sky=pow(Sky,colorCorrection);
			    half3 Screen = tex2D(_MainTex, IN.uv).rgb;
			    half3 FogCol=lerp(FogColor*FogMin,FogColor*FogMax,saturate(sunDir.y));//Fog color bright blend between Day/Night
			    half3 ScatterFog = lerp(Screen,lerp(Screen,Sky,ScatterFogDensity),saturate(depth*NormalFogDistance));
				half3 NormalFog    = pow(lerp(Screen,lerp(Screen,FogCol,NornalFogDensity),saturate(depth*NormalFogDistance)),colorCorrection);
			    half3 FinalFog= lerp(NormalFog,ScatterFog,saturate(depth*ScatterFogDistance));// Blend "Normal fog" and "Scattering fog"
			    
			    half Mask= saturate(lerp(half(1),half(0),maskDepth)*1000);//Mask objects drawing on depth buffer
			    half3 FinalColor=lerp(Screen,FinalFog,Mask);//Applies the fog only the objects that draw for depth buffer
			    
			    return half4(FinalColor,1);
			}
			ENDCG
		} 
	}
}