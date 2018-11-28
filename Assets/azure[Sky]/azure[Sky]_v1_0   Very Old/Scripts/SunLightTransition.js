// Attach as a sun directional light if it exists
// Change Ambient Color and sun light intensity between day and night
@script AddComponentMenu ("azure[Sky]/v1.0/Sun Light Transition")
@script ExecuteInEditMode()

function Update ()
{
   GetComponent.<Light>().intensity=Transition(Vector3.Dot(transform.forward, Vector3(0.0,-1,0.0)));
   RenderSettings.ambientSkyColor = Color.Lerp (Color.black,Color.white, GetComponent.<Light>().intensity-0.2);;
}

function Transition(zenith:float)
{
	return Mathf.Max(0.0, 1.0 - Mathf.Exp(-(((Mathf.PI/(2.0-0.25)) - Mathf.Acos(zenith))/2)));
}