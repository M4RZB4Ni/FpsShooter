#pragma strict
@script AddComponentMenu ("azure[Sky]/v1.0/Sky Control")
@script ExecuteInEditMode()

//Used in the Editor Script only
var showObjMats : boolean = true;
var showScattering : boolean = true;
var showTextures : boolean = true;
var showSkySettings : boolean = true;
var showFogSettings : boolean = true;
var showOptions : boolean = true;
var showMoonSettings : boolean = true;
var showTime_of_Day : boolean = true;
var spaceColorIndex : int = 0;
var colorCorrection : float=  1.0f;

//OBJs & MATs Tab
var SUN:GameObject;
var MOON:GameObject;
var SKYMAT:Material;
var FOGMAT:Material;
var MOONMAT:Material;

//Scattering Tab
var RayCoeff:float= 3.0;
var	MieCoeff:float= 0.05;
var	Turbidity:float= 1.0;
var	G:float= 0.75;
var SunIntensity:float= 500.0;
var MoonIntensity:float= 10.0;

//Textures Tab
var EmptySkyTexture:Texture2D;
var Cloud1:Texture2D;
var Cloud2:Texture2D;
var StarField:Texture2D;
var StarNoise:Texture2D;
var MoonTexture:Texture2D;
var MoonShadow:Texture2D;

//Sky Settings Tab
var SunsetColor:Color=Color(1.0,0.713,0.515,1.0);
var MoonsetColor:Color=Color(1.0,0.865,0.706,1.0);
var SkyExposure:float=1.0;
var DayLuminance:float=1.143;
var NightLuminance:float=1.125;
var DayNightTrans:float=10.0;
var Cloud1Speed:float=0.075;
var Cloud2Speed:float=0.045;
var Layer1_startRot:float=0.0;
var Layer2_startRot:float=0.0;
var StarScintillation:float=2.0;
var StarIntensity:float=7.5;
var StarExtinction:float=0.5;
var StarBlend:float=2.0;
var StarRot:float=0.0;
var SunsetAlt:float=0.12;
var MoonsetAlt:float=0.0;
var HorizonAlt:float=1.0;

//Fog Settings Tab
var NormalFogColor:Color=Color.white;
var ScatterFogDis:float=0.00024;
var ScatterFogDen:float=1.0;
var NormalFogDis:float=0.1;
var NormalFogDen:float=0.75;
var FogMin:float=0.075;
var FogMax:float=0.75;

// Moon Settings
var MoonShadowX:float=0.45;
var MoonShadowY:float=0.65;
var MoonShadowSize:float=0.35;
var MoonShadowIntensity:float=0.95;
var MoonColorIntensity:float=0.5;
var MoonHorizonColor:Color=Color.white;
var MoonZenithColor:Color=Color.white;

// Time of Day Tab
var DAY_CYCLE_IN_MINUTES:float=3;
var CURRENT_TIME:float=6.0;
var UTC:int=0;
var Longitude:float=0;
private var v3:Vector3=Vector3.zero; // For Sun Rotation
private var DEGREES_PER_MINUTE:float=0.25;//  360°/1440 Minutes per Day = 0.25° per Minutes
private var ROTATION:float=0;

//Options Tab
var RenderSide:int=0;
var spaceColor:float=1.0;

//Used in the sky formula
private var lambda : Vector3 = Vector3(680E-9, 550E-9, 450E-9);
private var K      : Vector3 = Vector3(0.686, 0.678, 0.666);
private var pi : float = Mathf.PI;   // 3.141592
private var n  : float = 1.0003;
private var N  : float = 2.545E25;
private var v  : float = 4.0;
private var Br : Vector3; //Total Rayleigh
private var Bm : Vector3; //Total Mie

function Awake()
{
   SkyUpdate();
   
   SKYMAT.SetVector("sunDir",-SUN.transform.forward);
   SKYMAT.SetVector("moonDir",-MOON.transform.forward);
   FOGMAT.SetVector("sunDir",-SUN.transform.forward);
   FOGMAT.SetVector("moonDir",-MOON.transform.forward);
   
   setTime(CURRENT_TIME,DAY_CYCLE_IN_MINUTES); // Set the start time of the day
   
   v3.y+=Longitude;
   MOON.transform.localScale.x*=-1;
   MOON.transform.localScale.y*=-1;
}



function Update()
{
#if UNITY_EDITOR
    if (!Application.isPlaying)
    {
       v3.x=(CURRENT_TIME+UTC)*360.0/(24)-90; // Select the "Current Time" in Editor only,this will be used for sun initial rotation
       v3.y=Longitude+180; // Sets the longitude of the sun, this will be used for sun initial position
       SUN.transform.localEulerAngles=v3;
    }
   
#endif
   v3.x-=ROTATION*Time.deltaTime;
   if (v3.x<=-360)v3.x+=360;
   if (Application.isPlaying)// Only in gameplay
   {
      SUN.transform.localEulerAngles=v3; //Rotation of the sun on the x axis
   }

   SkyUpdate();//Update Sky to see changes in Real Time in the Editor, delete this line if you do not need change the sky in real time when in gameplay
   
   //Here we must constantly update
   // Sets the direction of the moon and sun to the materials
   SKYMAT.SetVector("sunDir",-SUN.transform.forward);
   SKYMAT.SetVector("moonDir",-MOON.transform.forward);
   FOGMAT.SetVector("sunDir",-SUN.transform.forward);
   FOGMAT.SetVector("moonDir",-MOON.transform.forward);
}

function OnEnable()
{
   #if UNITY_EDITOR
    if (SUN == null)
		Debug.Log("Warning. Apply the <b>Sun Directional Light</b> to azure[Sky] control in Inspector");
    if (MOON == null)
		Debug.Log("Warning. Apply the <b>Moon GameObject</b> to azure[Sky] control in Inspector");
	if (SKYMAT == null)
		Debug.Log("Warning. Apply the <b>Sky Material</b> to azure[Sky] control in Inspector");
	if (FOGMAT == null)
		Debug.Log("Warning. Apply the <b>Fog Material</b> to azure[Sky] control in Inspector");
	if (MOONMAT == null)
		Debug.Log("Warning. Apply the <b>Moon Material</b> to azure[Sky] control in Inspector");
	if (EmptySkyTexture == null)
		Debug.Log("Warning. Azure Sky Control script does not have a texture assigned to <b>Base (Empty Sky)</b>");
	if (Cloud1 == null)
		Debug.Log("Warning. Azure Sky Control script does not have a texture assigned to <b>Clouds (Layer 1)</b>");
	if (Cloud2 == null)
		Debug.Log("Warning. Azure Sky Control script does not have a texture assigned to <b>Clouds (Layer 2)</b>");
	if (StarField == null)
		Debug.Log("Warning. Azure Sky Control script does not have a texture assigned to <b>Star Field</b>");
	if (StarNoise == null)
		Debug.Log("Warning. Azure Sky Control script does not have a texture assigned to <b>Star Noise Scintillation</b>");
	if (MoonTexture == null)
		Debug.Log("Warning. Azure Sky Control script does not have a texture assigned to <b>Moon Texture</b>");
	if (MoonShadow == null)
		Debug.Log("Warning. Azure Sky Control script does not have a texture assigned to <b>Moon Shadow</b>");
   #endif
}


function GetRay()
{
     /////Total Rayleigh/////
     ////////////////////////
     Br.x = RayCoeff * ((8.0 * Mathf.Pow(pi, 3.0) * Mathf.Pow(Mathf.Pow(n, 2.0) - 1.0, 2.0) ) / (3.0 * N * Mathf.Pow(lambda.x, 4.0)));
     Br.y = RayCoeff * ((8.0 * Mathf.Pow(pi, 3.0) * Mathf.Pow(Mathf.Pow(n, 2.0) - 1.0, 2.0) ) / (3.0 * N * Mathf.Pow(lambda.y, 4.0)));
     Br.z = RayCoeff * ((8.0 * Mathf.Pow(pi, 3.0) * Mathf.Pow(Mathf.Pow(n, 2.0) - 1.0, 2.0) ) / (3.0 * N * Mathf.Pow(lambda.z, 4.0)));
}
function GetMie()
{
   /////Total Mie/////
   ///////////////////
   var c : float = (0.2 * Turbidity ) * 10E-18;
   Bm.x = MieCoeff * (0.434 * c * pi * Mathf.Pow((2.0 * pi) / lambda.x, v - 2.0) * K.x);
   Bm.y = MieCoeff * (0.434 * c * pi * Mathf.Pow((2.0 * pi) / lambda.y, v - 2.0) * K.y);
   Bm.z = MieCoeff * (0.434 * c * pi * Mathf.Pow((2.0 * pi) / lambda.z, v - 2.0) * K.z);
}


// Update Sky and Fog Materials
// When you change a property in the Inspector, you must send the new values to the materials. This function makes it !
function SkyUpdate()
{
   GetRay();
   GetMie();
   //Scattering variables update
   SKYMAT.SetVector("Br",Br);
   SKYMAT.SetVector("Bm",Bm);
   SKYMAT.SetFloat("pi",Mathf.PI);
   SKYMAT.SetFloat("g",G);
   SKYMAT.SetFloat("SunIntensity",SunIntensity);
   SKYMAT.SetFloat("MoonIntensity",MoonIntensity);
   FOGMAT.SetVector("Br",Br);
   FOGMAT.SetVector("Bm",Bm);
   FOGMAT.SetFloat("pi",Mathf.PI);
   FOGMAT.SetFloat("g",G);
   FOGMAT.SetFloat("SunIntensity",SunIntensity);
   FOGMAT.SetFloat("MoonIntensity",MoonIntensity);
   //Sky textures updates
   SKYMAT.SetTexture("_EmptySkyTexture", EmptySkyTexture);
   SKYMAT.SetTexture("Layer1", Cloud1);
   SKYMAT.SetTexture("Layer2", Cloud2);
   SKYMAT.SetTexture("Layer3", StarField);
   SKYMAT.SetTexture("Layer4", StarNoise);
   FOGMAT.SetTexture("_EmptySkyTexture", EmptySkyTexture);
   MOONMAT.SetTexture("_MainTex", MoonTexture);
   MOONMAT.SetTexture("_Shadow", MoonShadow);
   //Sky settings update
   SKYMAT.SetColor("SunsetColor", SunsetColor);
   SKYMAT.SetColor("MoonsetColor", MoonsetColor);
   SKYMAT.SetFloat("SkyExposure",SkyExposure);
   SKYMAT.SetFloat("DayLuminance",DayLuminance);
   SKYMAT.SetFloat("NightLuminance",NightLuminance);
   SKYMAT.SetFloat("Time_of_Transition",DayNightTrans);
   SKYMAT.SetFloat("Layer1Speed",Cloud1Speed);
   SKYMAT.SetFloat("Layer2Speed",Cloud2Speed);
   SKYMAT.SetFloat("Layer1_startRot",Layer1_startRot);
   SKYMAT.SetFloat("Layer2_startRot",Layer2_startRot);
   SKYMAT.SetFloat("ScintillationSpeed",StarScintillation);
   SKYMAT.SetFloat("StarRange",StarIntensity);
   SKYMAT.SetFloat("StarExtinction",StarExtinction);
   SKYMAT.SetFloat("StarBlend",StarBlend);
   SKYMAT.SetFloat("StarRot",StarRot);
   SKYMAT.SetFloat("SunsetAltitude",SunsetAlt);
   SKYMAT.SetFloat("MoonsetAltitude",MoonsetAlt);
   SKYMAT.SetFloat("HorizonAltitude",HorizonAlt);
   //Fog Sky settings update
   FOGMAT.SetColor("SunsetColor", SunsetColor);
   FOGMAT.SetColor("MoonsetColor", MoonsetColor);
   FOGMAT.SetFloat("SkyExposure",SkyExposure);
   FOGMAT.SetFloat("DayLuminance",DayLuminance);
   FOGMAT.SetFloat("NightLuminance",NightLuminance);
   FOGMAT.SetFloat("Time_of_Transition",DayNightTrans);
   FOGMAT.SetFloat("SunsetAltitude",SunsetAlt);
   FOGMAT.SetFloat("MoonsetAltitude",MoonsetAlt);
   FOGMAT.SetFloat("HorizonAltitude",HorizonAlt);
   //Fog settings update
   FOGMAT.SetColor("FogColor", NormalFogColor);
   FOGMAT.SetFloat("ScatterFogDistance",ScatterFogDis);
   FOGMAT.SetFloat("ScatterFogDensity",ScatterFogDen);
   FOGMAT.SetFloat("NormalFogDistance",NormalFogDis);
   FOGMAT.SetFloat("NornalFogDensity",NormalFogDen);
   FOGMAT.SetFloat("FogMin",FogMin);
   FOGMAT.SetFloat("FogMax",FogMax);
   //Moon Settings Update
   MOONMAT.SetFloat("shadowX",MoonShadowX);
   MOONMAT.SetFloat("shadowY",MoonShadowY);
   MOONMAT.SetFloat("shadowSize",MoonShadowSize);
   MOONMAT.SetFloat("shadowIntensity",MoonShadowIntensity);
   MOONMAT.SetFloat("moonIntensity",MoonColorIntensity);
   MOONMAT.SetColor("_horizonColor",MoonHorizonColor);
   MOONMAT.SetColor("_zenithColor",MoonZenithColor);

   var FogMin:float=0.075;//minimum white that the fog will reach in the night
   var FogMax:float=0.75;//maximum white that the fog will reach in the day

   SKYMAT.SetInt("_Cull",RenderSide);
   FOGMAT.SetInt("_Cull",RenderSide);
   
   SKYMAT.SetFloat("colorCorrection", colorCorrection);
   FOGMAT.SetFloat("colorCorrection", colorCorrection);
}

function setTime(time:float,dayDuration:float)
{
    v3.x = -((time) + UTC) * (60*DEGREES_PER_MINUTE)-90;
    if (dayDuration>0.0)
    {
       ROTATION = (DEGREES_PER_MINUTE * 1440) / (dayDuration * 60); //Gets the degree to rotate the sun according to the time selected
    }
    else { ROTATION=0; }
}