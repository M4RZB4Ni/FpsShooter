using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenDoorAct : MonoBehaviour {
    [SerializeField]
    GameObject Drone;
    Animator DoorAnim;
    bool check = true;
    [SerializeField]
    AudioSource Drone_Source;
   
    // Use this for initialization
    void Start () {
        Drone_Source.Stop();
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (check == true)
            {
                DoorAnim.SetBool("Open", true);
                DoorAnim.SetBool("Close", false);
                check = false;
                Debug.Log("DOOR Opened");
            }
            else if (check == false)
            {
                DoorAnim.SetBool("Open", false);
                DoorAnim.SetBool("Close", true);
                check = true;
                Debug.Log("Door Closed");
            }

        }
    }
   
}
