//    This script is an example of how to change the time of day manually in gameplay. Place this script in any object in the...
//    scene and drag the GameObject that has azureSkyControl script for the 'target' property in the Inspector.

//    When you press the space key, the time of day will change for the selected in the Inspector.

//    * The first function argument is the time that will be the day, use values between 0-24.
//    * The second function argument is time in minutes that the day will take to pass. If the value is zero, the hours will not pass.

//    Obs: This is just one example, this script is not attached to any GameObject in the demo scene.


#pragma strict
var target:SkyControl_v1_0;   // Drag in ispector the GamObject that has the azureSkyControl script to this property
var TimeOfDay:float=6.0;      // 6:00 AM
var dayDuration : float =3.0; // New duration of day in minutes
 
function Update ()
{
   if (Input.GetKeyDown("enter"))
   {
      target.setTime(TimeOfDay,dayDuration); // Using the function that is in the control script to set the time of day.
   }
}