using UnityEngine;
using System.Collections;

/*
 * This script manages the animations and rotation of the agent.
 * */

namespace ParagonAI
{
    [RequireComponent(typeof(UnityEngine.AI.NavMeshAgent))]
    public class AnimationScript : MonoBehaviour
    {
        //Stuff
        public ParagonAI.BaseScript myBaseScript;
        public Transform myAIBodyTransform;
        public ParagonAI.GunScript gunScript;
        public Animator animator;
        Transform myTransform;
        public ParagonAI.RotateToAimGunScript rotateGunScript;

        //Speed
        float currentVelocityRatio = 0;
        float animationDampTime = 0.15f;
        UnityEngine.AI.NavMeshAgent agent;

        //Offset
        //This is required because we de-parent the body from the navmesh agent for 
        //rotation purposes.  We need to make it keep up.
        public Vector3 bodyOffset;

        //Cover
        public float minDistToCrouch = 1;

        //Speeds
        public float maxMovementSpeed = -1f;
        public float animatorSpeed = 1f;
        public float meleeAnimationSpeed = 1f;

        //Animation Hashes
        private int currentAngleHash;
        private int engagingHash;
        private int crouchingHash;
        private int reloadingHash;
        private int meleeHash;
        private int fireHash;
        private int forwardsMoveHash;
        private int sidewaysMoveHash;
        private int sprintingHash;

        //Dynamic objects
        public float maxAngleDeviation = 10;
        Quaternion currRotRequired;
        public bool useCustomRotation = false;

        Vector3 directionToFace;

        //Rotation
        float myAngle;
        [Range(0.0f, 90.0f)]
        public float minAngleToRotateBase = 65;
        Quaternion newRotation;
        public float turnSpeed = 4.0f;

        public float meleeAnimationLength = 3;
        
        private bool sprinting = false;


        // Use this for initialization
        void Awake()
        {
            SetHashes();
        }

        void Start()
        {
            //Set offset of mesh	
            if (myAIBodyTransform)
            {
                bodyOffset = myAIBodyTransform.localPosition;
                bodyOffset.x *= transform.localScale.x;
                bodyOffset.y *= transform.localScale.y;
                bodyOffset.z *= transform.localScale.z;
                myAIBodyTransform.parent = null;
            }
            else
            {
                Debug.LogWarning("No transform set for 'myAIBodyTransform'.  Please assign a transform in the inspector!");
                this.enabled = false;
            }

            //Inititate Hashes and stuff	
            agent = gameObject.GetComponent<UnityEngine.AI.NavMeshAgent>();
            minDistToCrouch = minDistToCrouch * minDistToCrouch;
            myTransform = transform;


            //Check to make sure we have all of our scripts assigned	
            if (!myBaseScript)
            {
                Debug.LogWarning("No Base Script found!  Please add one in the inspector!");
                this.enabled = false;
            }
            else if (maxMovementSpeed < 0)
            {
                maxMovementSpeed = myBaseScript.runSpeed;
            }

            if (!animator)
            {
                Debug.LogWarning("No animator component found!  Please add one in the inspector!");
                this.enabled = false;
            }
            else
            {
                animator.speed = animatorSpeed;
            }
        }

        // Update is called once per frame
        void LateUpdate()
        {
            //Set body to it's current position
            myAIBodyTransform.position = myTransform.position + bodyOffset;
            AnimateAI();
            //This has to be in late update or we get nasty non-normalized quaternions.
            RotateAI();
        }


        //Animations
        void AnimateAI()
        {
            //Correctly blend strafing and forwards/backwards movement
            animator.SetFloat(forwardsMoveHash, Vector3.Dot(myAIBodyTransform.forward, agent.desiredVelocity) / maxMovementSpeed, animationDampTime, Time.deltaTime);
            animator.SetFloat(sidewaysMoveHash, Vector3.Dot(myAIBodyTransform.right, agent.desiredVelocity) / maxMovementSpeed, animationDampTime, Time.deltaTime);

            //Check to see if we should crouch, and if so, crouch.  We only crouch if we are in cover and not firing or being suppressed.
            if (myBaseScript.inCover && (!gunScript || !gunScript.IsFiring() || !myBaseScript.shouldFireFromCover) && Vector3.SqrMagnitude(myTransform.position - myBaseScript.GetCurrentCoverNodePos()) < minDistToCrouch && currentVelocityRatio < 0.3)
            {
                animator.SetBool(crouchingHash, true);
            }
            else
            {
                animator.SetBool(crouchingHash, false);
            }
        }

        
        public void StartSprinting()
        {
        	sprinting = true;
        	
        	//Make sure the animation in question exists.
            //If the trigger is not found, no animation is played, but no error is thrown.
            for (int i = 0; i < animator.parameters.Length; i++)
            {
                if (animator.parameters[i].name == "Sprinting")
                {
                    animator.SetBool(sprintingHash, true);
                }
            }
        }   
        
        public void StopSprinting()
        {
        	sprinting = false;
        	
        	//Make sure the animation in question exists.
            //If the trigger is not found, no animation is played, but no error is thrown.
            for (int i = 0; i < animator.parameters.Length; i++)
            {
                if (animator.parameters[i].name == "Sprinting")
                {
                    animator.SetBool(sprintingHash, false);
                }
            }
        }                   
		
		
		public bool isSprinting()
		{
			return sprinting;
		}
	
        public void PlayReloadAnimation()
        {
            animator.SetTrigger(reloadingHash);
        }

        public void PlayFiringAnimation()
        {
            animator.SetTrigger(fireHash);
        }

        public IEnumerator StartMelee()
        {
            //Stop aiming our weapon at the enemy
            rotateGunScript.Deactivate();

            //Rotate to face the target
            directionToFace = -(myAIBodyTransform.position - myBaseScript.targetTransform.position);
            useCustomRotation = true;
            directionToFace.y = 0;

            //Make sure we're rotating		
            while (isPlaying && myAIBodyTransform && myBaseScript.targetTransform && Vector3.Angle(directionToFace, myAIBodyTransform.forward) > maxAngleDeviation)
            {
                directionToFace = -(myAIBodyTransform.position - myBaseScript.targetTransform.position);
                directionToFace.y = 0;

                //Debug stuff
                Debug.DrawRay(myTransform.position, myTransform.forward * 100, Color.magenta);
                Debug.DrawRay(myTransform.position, directionToFace * 100, Color.blue);
                yield return null;
            }

            //Play teh animation
            if (isPlaying && myAIBodyTransform)
            {
                animator.SetTrigger(meleeHash);
                yield return new WaitForSeconds(meleeAnimationLength);
            }
            useCustomRotation = false;
            rotateGunScript.Activate();
            myBaseScript.StopMelee();
        }


        //Stop errors from spamming the console when the game is stopped in the editor.
        bool isPlaying = true;
        void OnApplicationQuit()
        {
            isPlaying = false;
        }

        public IEnumerator WaitForAnimationToFinish()
        {
            //Wait for transition to finish
            while (animator.IsInTransition(1))
            {
                yield return null;
            }
            //Wait for animation to finish
            while (!animator.IsInTransition(1))
            {
                yield return null;
            }
            //wat for second for second transition to finish
            while (animator.IsInTransition(1))
            {
                yield return null;
            }
        }

        //Dynamic Objects
        public IEnumerator DynamicObjectAnimation(string transitionName, Vector3 dir, DynamicObject dynamicObjectScript)
        {
            directionToFace = dir;
            useCustomRotation = true;

            directionToFace.y = 0;

            //make sure we're rotating to face the proper direction		
            while (Vector3.Angle(directionToFace, myAIBodyTransform.forward) > maxAngleDeviation)
            {
                Debug.DrawRay(myTransform.position, myTransform.forward * 100, Color.magenta);
                Debug.DrawRay(myTransform.position, directionToFace * 100, Color.blue);
                yield return null;
            }

            //Stop before triggering action to make things smoother
            yield return new WaitForSeconds(0.25f);

            //Play the animation and affect the object	
            bool shouldReactivate = false;
            if (rotateGunScript.isEnabled)
            {
                rotateGunScript.Deactivate();
                shouldReactivate = true;
            }

            dynamicObjectScript.AffectDynamicObject();

            //Make sure the animation in question exists.
            //If the trigger is not found, no animation is played, but no error is thrown.
            for (int i = 0; i < animator.parameters.Length; i++)
            {
                if (animator.parameters[i].name == transitionName)
                {
                    animator.SetTrigger(Animator.StringToHash(transitionName));
                    //Wait until the animation finishes
                    //Wait for transition to finish
                    yield return StartCoroutine(WaitForAnimationToFinish());
                    break;
                }
            }

            //Star aiming our weapon again.
            if (shouldReactivate)
            {
                rotateGunScript.Activate();
            }

            //Finish up the dynamic object sequence
            dynamicObjectScript.EndDynamicObjectUsage();
            useCustomRotation = false;
        }


        //Rotating
        void RotateAI()
        {
            //Rotate to look in the given direction, if one is given.
            if (useCustomRotation)
            {
                newRotation = Quaternion.LookRotation(directionToFace);
                newRotation.eulerAngles = new Vector3(0.0f, newRotation.eulerAngles.y, 0.0f);
                myAIBodyTransform.rotation = Quaternion.Lerp(myAIBodyTransform.rotation, newRotation, turnSpeed * Time.deltaTime);
            }
            else if ((myBaseScript.IsEnaging() && myBaseScript.targetTransform && !sprinting))
            {
                //Get angle between vector of movement and the actual direction enemyBody is facing				
                myAngle = Vector3.Angle(myTransform.forward, myAIBodyTransform.forward);

                if (Vector3.Angle(-myAIBodyTransform.right, myTransform.forward) > 90)
                {
                    myAngle = -myAngle;
                }

                //Get angle between vector of movement and the direction we want to be facing
                float angleBetweenFor = Vector3.Angle(myTransform.forward, myBaseScript.targetTransform.position - myAIBodyTransform.position);

                //The following if statement is to even out clipping and crossfading problems with ~45 degree angle strafing.
                //If the angle between the direction we are moving in and the vector to the target will commonly result in clipping, 
                //then we face the legs in the direction of movement, play either the forwards or backwards animations and
                // rely on the chest movement to aim at the target.

                //We will also always rotate to fact the target if the speed is too low, 
                //because while standing still, the vector of movement becomes unreliable.	
                if (angleBetweenFor > minAngleToRotateBase && angleBetweenFor < 180 - minAngleToRotateBase)
                {
                    newRotation = Quaternion.LookRotation(myBaseScript.targetTransform.position - myAIBodyTransform.position);
                }
                else
                {
                    //Play correct animation			    				
                    if (angleBetweenFor < 90)
                    {
                        newRotation = Quaternion.LookRotation(myTransform.forward);
                        animator.SetFloat(forwardsMoveHash, Vector3.Magnitude(agent.desiredVelocity) / maxMovementSpeed, animationDampTime, Time.deltaTime);
                        animator.SetFloat(sidewaysMoveHash, 0, animationDampTime, Time.deltaTime);
                    }
                    else
                    {
                        newRotation = Quaternion.LookRotation(-myTransform.forward);
                        animator.SetFloat(forwardsMoveHash, -Vector3.Magnitude(agent.desiredVelocity) / maxMovementSpeed, animationDampTime, Time.deltaTime);
                        animator.SetFloat(sidewaysMoveHash, 0, animationDampTime, Time.deltaTime);
                    }
                }

                //Make sure we only rotate around the y axis
                newRotation.eulerAngles = new Vector3(0.0f, newRotation.eulerAngles.y, 0.0f);

                //Smoothly rotate to face target						                                						                                
                myAIBodyTransform.rotation = Quaternion.Slerp(myAIBodyTransform.rotation, newRotation, Time.deltaTime * turnSpeed);
            }
            //Look in the direction we are moving.
            else
            {
                myAngle = 0;

                newRotation = Quaternion.LookRotation(myTransform.forward);
                newRotation.eulerAngles = new Vector3(0.0f, newRotation.eulerAngles.y, 0.0f);

                myAIBodyTransform.rotation = Quaternion.Lerp(myAIBodyTransform.rotation, newRotation, turnSpeed * Time.deltaTime);
            }
        }

        //Setters
        void SetHashes()
        {
            crouchingHash = Animator.StringToHash("Crouching");
            engagingHash = Animator.StringToHash("Engaging");
            reloadingHash = Animator.StringToHash("Reloading");
            meleeHash = Animator.StringToHash("Melee");
            fireHash = Animator.StringToHash("Fire");
            sidewaysMoveHash = Animator.StringToHash("Horizontal");
            forwardsMoveHash = Animator.StringToHash("Forwards");
            sprintingHash = Animator.StringToHash("Sprinting");
            setHashes = true;
        }


        bool setHashes = false;

        //Called when the agent enters direct combat
        //The weapon is raised
        public void SetEngaging()
        {
            //yield return null;
            if (!setHashes)
                SetHashes();
            animator.SetBool(engagingHash, true);
        }

        //Called when the agent loses track of the target, or the target is eliminated.
        //The weapon is lowered
        public void SetDisengage()
        {
            //yield return null;
            if (animator)
            {
                if (!setHashes)
                    SetHashes();
                animator.SetBool(engagingHash, false);
            }
        }
    }
}
