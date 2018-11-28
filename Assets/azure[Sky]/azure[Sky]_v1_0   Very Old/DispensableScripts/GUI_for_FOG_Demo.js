#pragma strict
var ObjSkyControl:Transform; // Drag in ispector the GamObject that has the azureSkyControl script to this property
private var target:SkyControl_v1_0;

function Awake ()
{
    target=ObjSkyControl.GetComponent(SkyControl_v1_0);//Get azureSkyControl script of sky control GameObject to change the values in the GUI
    target.SunIntensity=500.0;
    target.MoonIntensity=10.0;
    target.DayLuminance=1.143;
    target.NightLuminance=1.125;
    target.StarIntensity=3.0;
    target.StarScintillation=2.0;
    target.HorizonAlt=1.0;
    target.ScatterFogDis=0.00024;
    target.NormalFogDis=0.01;
    target.NormalFogDen=0.75;
    target.MoonShadowX=0.45;
    target.MoonShadowY=0.65;
    target.MoonShadowIntensity=0.9;
}

function OnGUI()
{
    GUI.Box (new Rect (5,5,265,510),"<b>Sky Settings</b>");
    // Sun Intensity
    GUI.Label (Rect (10, 30, 100, 20), "Sun Intensity");
    target.SunIntensity=GUI.HorizontalSlider(Rect (110, 35, 100, 10), target.SunIntensity, 250.0, 500.0);
    // Moon Intensity
    GUI.Label (Rect (10, 50, 100, 20), "Moon Intensity");
    target.MoonIntensity=GUI.HorizontalSlider(Rect (110, 55, 100, 10), target.MoonIntensity, 2.0, 25.0);
    // Day Luminance
    GUI.Label (Rect (10, 70, 100, 20), "Day Luminance");
    target.DayLuminance=GUI.HorizontalSlider(Rect (110, 75, 100, 10), target.DayLuminance, 1.16, 0.75);
    // Night Luminance
    GUI.Label (Rect (10, 90, 100, 20), "Night Luminance");
    target.NightLuminance=GUI.HorizontalSlider(Rect (110, 95, 100, 10), target.NightLuminance, 1.15, 0.75);
    // Star Intensity
    GUI.Label (Rect (10, 110, 100, 20), "Star Intensity");
    target.StarIntensity=GUI.HorizontalSlider(Rect (110, 115, 100, 10), target.StarIntensity, 0.0, 3.0);
    // Star Scintillation
    GUI.Label (Rect (10, 130, 100, 20), "Star Scintillation");
    target.StarScintillation=GUI.HorizontalSlider(Rect (110, 135, 100, 10), target.StarScintillation, 0.0, 5.0);
    // Star Scintillation
    GUI.Label (Rect (10, 150, 100, 20), "Horizon altitude");
    target.HorizonAlt=GUI.HorizontalSlider(Rect (110, 155, 100, 10), target.HorizonAlt, 1.0, 0.0);
    
    
     GUI.Label (Rect (95, 200, 100, 20), "<b>Fog Settings</b>");
     // Scattering Fog Distance
     GUI.Label (Rect (10, 220, 150, 20), "Scattering Fog Distance");
     target.ScatterFogDis=GUI.HorizontalSlider(Rect (165, 225, 100, 10), target.ScatterFogDis, 0.0, 0.001);
     // Normal Fog Distance
     GUI.Label (Rect (10, 245, 150, 20), "Normal Fog Distance");
     target.NormalFogDis=GUI.HorizontalSlider(Rect (165, 250, 100, 10), target.NormalFogDis, target.ScatterFogDis, 0.01);
     if (target.NormalFogDis<=target.ScatterFogDis){target.NormalFogDis=target.ScatterFogDis;}
     // Normal Fog Density
     GUI.Label (Rect (10, 270, 150, 20), "Normal Fog Density");
     target.NormalFogDen=GUI.HorizontalSlider(Rect (165, 275, 100, 10), target.NormalFogDen, 0.0, 1.0);
     
     
     GUI.Label (Rect (90, 315, 100, 20), "<b>Moon Settings</b>");
     // Moon Shadow X Axis
     GUI.Label (Rect (10, 335, 150, 20), "Moon Shadow X-Axis");
     target.MoonShadowX=GUI.HorizontalSlider(Rect (165, 340, 100, 10), target.MoonShadowX, 1.0, -1.0);
     // Moon Shadow Y Axis
     GUI.Label (Rect (10, 360, 150, 20), "Moon Shadow Y-Axis");
     target.MoonShadowY=GUI.HorizontalSlider(Rect (165, 365, 100, 10), target.MoonShadowY, 1.0, -1.0);
     // Moon Shadow Intensity
     GUI.Label (Rect (10, 385, 150, 20), "Moon Shadow Intensity");
     target.MoonShadowIntensity=GUI.HorizontalSlider(Rect (165, 390, 100, 10), target.MoonShadowIntensity, 0.0, 1.0);
     
     
     // comands
     GUI.Label (Rect (10, 465, 300, 20), "<b>Right Mouse:</b> Rotate the camera");
     GUI.Label (Rect (10, 480, 300, 20), "<b>Wheel Mouse:</b> Reset the camera rotation");
     GUI.Label (Rect (10, 495, 300, 20), "<b>Space:</b> Restart the scene");
    


    
    
    if (GUI.Button(Rect(75,425,120,25),"Standard Settings"))
    {
        target.SunIntensity=500.0;
        target.MoonIntensity=10.0;
        target.DayLuminance=1.143;
        target.NightLuminance=1.125;
        target.StarIntensity=3.0;
        target.StarScintillation=2.0;
        target.HorizonAlt=1.0;
        target.ScatterFogDis=0.00024;
        target.NormalFogDis=0.01;
        target.NormalFogDen=0.75;
        target.MoonShadowX=0.45;
        target.MoonShadowY=0.65;
        target.MoonShadowIntensity=0.9;
    }
}