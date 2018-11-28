// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "azure[Sky]/v1_0/Moon"
{
    SubShader
    {
        Tags{"Queue"="Transparent-250""RenderType"="Transparent""IgnoreProjector"="True"}
        Fog{Mode Off}
        Cull [_Cull]
        ZWrite Off
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            uniform sampler2D _MainTex,_Shadow;
            uniform fixed shadowX,shadowY,shadowSize,shadowIntensity,moonIntensity;
            fixed4 _horizonColor,_zenithColor;
            half spaceColor;
            
            struct appdata{
			    float4 vertex   : POSITION;
			    float4 texcoord : TEXCOORD0;
			};

            struct v2f{
                float4 pos        : POSITION;
                float2 uv         : TEXCOORD0;
                float3 WorldPos   : TEXCOORD1;
            };

            v2f vert(appdata v){
                v2f o;
                o.pos= UnityObjectToClipPos(v.vertex);
                o.uv =v.texcoord.xy;
                o.WorldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            fixed4 frag(v2f IN) : COLOR
            {
                fixed fade=lerp(fixed3(0,0,0),fixed3(1,1,1),saturate(dot(normalize(IN.WorldPos),fixed3(0,4,0))));
                fixed shadow=tex2D(_Shadow,(IN.uv*shadowSize)+fixed2(shadowX,shadowY)).b;
                fixed3 moonColor=lerp(_horizonColor,_zenithColor,saturate(dot(normalize(IN.WorldPos),fixed3(0,1,0))));
                fixed4 c=tex2D(_MainTex,IN.uv);
                c.rgb*=moonColor;
                shadow*=shadowIntensity;
                c.a*=fade*(1-shadow)*moonIntensity;
                return c;
            }

            ENDCG
        }
    }

    Fallback Off
}
