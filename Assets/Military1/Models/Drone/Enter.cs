using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enter : MonoBehaviour {
    [SerializeField]
    GameObject Drone;
    [SerializeField]
    Animator Run;
    [SerializeField]
    AudioSource Drone_Source;
    public float targetTime = 7f;
    bool ColideS=false;
    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

 

    }

 
    void OnTriggerEnter(Collider col)
    {
  
        Drone_Source.Play();
        Debug.Log("Collision");
        Run.SetBool("Start", true);
        Destroy(Drone, 12);

    }
   // void startTimer()
  //  {
       // targetTime -= Time.deltaTime;
       // if (targetTime <= 0.0f)
      //  {  
           // Drone_Source.Stop();
    //    }
  //  }
}
