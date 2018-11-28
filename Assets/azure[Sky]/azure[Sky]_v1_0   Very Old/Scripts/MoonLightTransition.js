// Attach as a moon directional light if it exists
// Change Moon light intensity between day and night
@script AddComponentMenu ("azure[Sky]/v1.0/Moon Light Transition")
@script ExecuteInEditMode

private var time:float;
function Update ()
{
   time=Transition(Vector3.Dot(transform.forward, Vector3(0.0,-1,0.0)));
   if (time>0)
   {
      if(GetComponent.<Light>().intensity<0.15)GetComponent.<Light>().intensity+=0.1*Time.deltaTime;
   }
   else{ GetComponent.<Light>().intensity -= 0.1*Time.deltaTime; }
}

function Transition(zenith:float)
{
	return Mathf.Max(0.0, 1.0 - Mathf.Exp(-(((Mathf.PI/(2.0-0.25)) - Mathf.Acos(zenith))/2)));
}