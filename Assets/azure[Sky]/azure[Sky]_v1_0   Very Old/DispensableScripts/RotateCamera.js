private var v3:Vector3;
var speed:float=10.0;
 
function Start()
{
    v3= transform.localEulerAngles;
}
 
function LateUpdate()
{
	   if(Input.GetMouseButton(1))
	   {
	   
	        v3.x -= (Input.GetAxis("Mouse Y") * speed)*Time.deltaTime;
	        v3.y += (Input.GetAxis("Mouse X") * speed)*Time.deltaTime;      
	   }
	   v3=clamp(v3);
	   transform.localEulerAngles=v3;
}

function clamp(v3:Vector3)
{
   if (v3.x>360)v3.x-=360;
   if (v3.x<-360)v3.x+=360;
   if (v3.y>360)v3.y-=360;
   if (v3.y<-360)v3.y+=360;
   return v3;
}