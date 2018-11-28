float3 sunDir;               // Direction of Sun Light
float3 moonDir;              // Direction of Moon Light
half3 Br;                   // Total Rayleigh Function
half3 Bm;                   // Total Mie Function
float  pi;
half  g;                    // Directionality Factor
half  DayLuminance;         // Luminance of the Day
half  NightLuminance;       // Luminance of the Night
half  Time_of_Transition;   // Time of Transition between Day and Night
half  HorizonAltitude;      // Sky Horizon Altitude
half  SunsetAltitude;       // Sunset Horizon Altitude
half  MoonsetAltitude;      // Moonset Horizon Altitude
half  SunIntensity;        
half  MoonIntensity;
half4 SunsetColor;
half4 MoonsetColor;
half  SkyExposure;           // Sky Layer Range

struct appdata{
    float4 vertex   : POSITION;
    float4 texcoord : TEXCOORD0;
};

//float3 Lerp(float3 a, float3 b, float w)
//{
//  return a + w*(b-a);
//}
half3 Fex(half SR,half SM){
   return exp(-(Br+Bm)*(SR+SM));
}
half3 BrTheta(half cosTheta){
   //float rayPhase = (3.0/(16.0*pi)) * (1.0+pow(cosTheta,2.0)); // Preetham Function
   half rayPhase = (3.0/(16.0*pi)) * (2.0+0.5*pow(cosTheta,2.0)); // Nielsen Function
   return rayPhase * Br;
}
half3 BmTheta(half cosTheta){
   //float miePhase = (1.0/(4.0*pi)) * ( (1.0-g*g) / pow(((1.0+g*g)-2*g*cosTheta),1.5) ); // Preetham Function
   half miePhase = 1.5 / (4.0 * pi) * (1.0 - g*g) * pow(abs(1.0 + (g*g) - 2.0*g*cosTheta), -1.5) * (1.0 + cosTheta * cosTheta) / (2.0 + g*g); // Eric Bruneton Function
   return miePhase * Bm;
}
half sunIntensity(half zenith){
	return SunIntensity * max(0.0, 1.0 - exp(-(((pi/(2.0-0.2)) - acos(zenith))/1.5)));
}
half MoomIntensity(half zenith){
	return MoonIntensity * max(0.2, 1.25 - exp(-(((pi/(2.0-0.85)) - acos(zenith))/1.5)));
}