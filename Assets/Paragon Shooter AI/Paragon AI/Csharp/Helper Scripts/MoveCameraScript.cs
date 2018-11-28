using UnityEngine;
using System.Collections;

/*
 * Moves the demo camera around.  Not part of the Paragon AI system.
 * */

namespace ParagonAI
{
    public class MoveCameraScript : MonoBehaviour
    {

        public float rotateSpeed = 3;
        public float moveSpeedSlow = 1;
        public float moveSpeedFast = 2;
        private float currentSpeed;

        private float yRotation;
        private float xRotation;

        private Vector3 origPos;
        private Quaternion origRot;

        void Awake()
        {
            origPos = transform.position;
            origRot = transform.rotation;
        }

        void Update()
        {
            if (Input.GetKey(KeyCode.R))
            {
                Application.LoadLevel(0);
            }


            xRotation = transform.eulerAngles.x;
            yRotation = transform.eulerAngles.y;

            if (Input.GetKey(KeyCode.LeftShift))
            {
                currentSpeed = moveSpeedFast;
            }
            else
            {
                currentSpeed = moveSpeedSlow;
            }

            //Camera Motion	
            if (Input.GetButton("Fire2") || Input.GetButton("Fire1"))
            {
                //Look 	
                yRotation += Input.GetAxis("Mouse X") * rotateSpeed;
                xRotation -= Input.GetAxis("Mouse Y") * rotateSpeed;
                //xRotation = Mathf.Clamp(xRotation, -90, 90);

                transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0);

                //Move Camera   	
                float inputX = Input.GetAxis("Horizontal");
                float inputY = Input.GetAxis("Vertical");
                //var inputZ : float = Input.GetAxis("ZVertical"); 
                float inputZ = 0;

                //Use raw keycodes just so that we can make sure it's compatable without needing weird input settings.
                if (Input.GetKey(KeyCode.E))
                {
                    inputZ = 0.75f;
                }
                else if (Input.GetKey(KeyCode.Q))
                {
                    inputZ = -0.75f;
                }


                transform.Translate(Vector3.forward * inputY * currentSpeed, transform);
                transform.Translate(Vector3.right * inputX * currentSpeed, transform);
                transform.Translate(Vector3.up * inputZ * currentSpeed, transform);
            }

            if (Input.GetKey(KeyCode.V))
            {
                transform.position = origPos;
                transform.rotation = origRot;
            }
        }

        public bool showControls = true;

        void OnGUI()
        {
            if (showControls)
            {
                Rect r = new Rect(10, 10, 1000, 1000);
                GUI.Label(r, "Use Right Mouse + WASDQE to move the camera.  R to restart");
            }
        }

    }
}
