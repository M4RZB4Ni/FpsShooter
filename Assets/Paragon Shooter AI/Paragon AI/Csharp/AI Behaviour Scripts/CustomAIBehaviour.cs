using UnityEngine;
using System.Collections;

/*
 * Script that contains several behaviour classes
 * */


//Base behaviour class
namespace ParagonAI{
public class CustomAIBehaviour : MonoBehaviour{
		
	[HideInInspector]
	public ParagonAI.BaseScript baseScript = null;
	[HideInInspector]
	public ParagonAI.GunScript gunScript;
	[HideInInspector]
	public ParagonAI.SoundScript soundScript;
	[HideInInspector]
	public ParagonAI.RotateToAimGunScript rotateToAimGunScript;
	[HideInInspector]
	public ParagonAI.AnimationScript animationScript;
	[HideInInspector]
	public ParagonAI.CoverFinderScript coverFinderScript;	
	[HideInInspector]
	public Transform myTransform;
	[HideInInspector]	
	public UnityEngine.AI.NavMeshAgent agent;
	[HideInInspector]
	public LayerMask layerMask;
	
	public Vector3 targetVector;		
	public BehaviourLevel behaveLevel;
	
	public enum BehaviourLevel {
		None = 0, Idle = 1, Combat = 2,
	}
		
	public void Start(){
            ApplyBehaviour();
		}	

			
	public virtual void Initiate()
		{
            if (gameObject.GetComponent<ParagonAI.BaseScript>())
            {
                baseScript = gameObject.GetComponent<ParagonAI.BaseScript>();

                gunScript = baseScript.gunScript;
                soundScript = baseScript.audioScript;
                rotateToAimGunScript = baseScript.headLookScript;
                animationScript = baseScript.animationScript;
                coverFinderScript = baseScript.coverFinderScript;
                myTransform = baseScript.GetTranform();

                layerMask = ParagonAI.ControllerScript.currentController.GetLayerMask();

                agent = baseScript.GetAgent();
            }
		}
		
	public virtual void AICycle()
		{
			
		}
			
	public virtual void EachFrame()
		{
		}		
				
	public void KillBehaviour()
		{
		 	OnEndBehaviour();
		 	Destroy(this);
		}
		
	public virtual void OnEndBehaviour()
		{
		 
		}	
	
    public void ApplyBehaviour()
        {
            if (behaveLevel == BehaviourLevel.Idle)
            {
                Initiate();
                if (baseScript)
                    baseScript.SetIdleBehaviour(this);
            }
            else if (behaveLevel == BehaviourLevel.Combat)
            {
                Initiate();
                if (baseScript)
                    baseScript.SetCombatBehaviour(this);
            }	       
        }
							
}
}


//Run right to the target position.
namespace ParagonAI{
public class ChargeTarget : ParagonAI.CustomAIBehaviour{

    public override void Initiate()
		{
			base.Initiate();
		}

	public override void AICycle() 
	{
        //Run to the key transform if we have one
        if (baseScript.keyTransform)
        {
            targetVector = baseScript.keyTransform.position;
        }
        //Otherwise, run at the target we are firing on
        else if (baseScript.targetTransform)
        {
            targetVector = baseScript.targetTransform.position;
        }
	}
}
}

//Move to a key transform
namespace ParagonAI
{
    public class MoveToTransform : ParagonAI.CustomAIBehaviour
    {
        public override void Initiate()
        {
            base.Initiate();
        }

        public override void AICycle()
        {
            if (baseScript.keyTransform)
                targetVector = baseScript.keyTransform.position;
        }
    }
}

//Upon being warned of a grenade, move in a random direction in an attempt to escape.
namespace ParagonAI
{
    public class RunAwayFromGrenade : ParagonAI.CustomAIBehaviour
    {
        //Completely random number.
        float timeToGiveUp = 3.0f;

        public override void Initiate()
        {
            base.Initiate();
            GetPositionToMoveTo();
        }

        public override void AICycle()
        {
            if (timeToGiveUp < 0  || baseScript.transformToRunFrom == null || (!agent.pathPending && agent.remainingDistance < 1))
            {
                KillBehaviour();
            }
            timeToGiveUp -= Time.deltaTime;
        }

        void GetPositionToMoveTo()
        {
            //Find a random direction to run in.
            //We don't want to move directly away from the genade because otherwise we may end up backing right into a corner.
            //Raycasts to check for corners are too unreliable because of variations in terrain height.
            Vector3 tempVec = baseScript.transformToRunFrom.position;
            tempVec.x += (Random.value - 0.5f) * baseScript.distToRunFromGrenades;
            tempVec.z = (Random.value - 0.5f) * baseScript.distToRunFromGrenades;
            UnityEngine.AI.NavMeshHit hit;
            UnityEngine.AI.NavMesh.SamplePosition(tempVec, out hit, baseScript.distToRunFromGrenades, -1);
        }
    }
}

//Quickly move to the side
namespace ParagonAI{
public class Dodge : ParagonAI.CustomAIBehaviour{

	Vector3 dodgingTarget;

	public virtual void Inititae()
		{
			base.Initiate();
		}

	IEnumerator SetDodge()
	{
		float myOrigStoppingDist = agent.stoppingDistance;
	
        //Prepare the navagent to rapidly change directions.
		agent.speed = baseScript.dodgingSpeed;
		agent.stoppingDistance = 0;
		agent.acceleration = 10000;
		baseScript.isDodging = true;
			
		agent.SetDestination(dodgingTarget);
			
        //Don't move until we have plotted a path.
		while(!agent.hasPath)
			{
				yield return null;
			}
			
		yield return new WaitForSeconds(baseScript.dodgingTime);
		
        //return the navagent to it's original state.
		agent.acceleration = baseScript.origAcceleration;
		agent.stoppingDistance = myOrigStoppingDist;
		agent.speed = baseScript.runSpeed;
		baseScript.isDodging = false;
		
        //End the behaviour
		KillBehaviour();
	}

	bool AquireDodgingTarget()
	{		
		var dodgePos = myTransform.position;
        //Choose whether to dodge left or right.
		if(Random.value < 0.5)
			{
                //Get a target position.
				dodgePos += myTransform.right * baseScript.dodgingSpeed * baseScript.dodgingTime; 
			}
		else
			{
                //Get a target position.
				dodgePos += -myTransform.right * baseScript.dodgingSpeed * baseScript.dodgingTime;
			}
		
        //Make sure there are no walls in the way.
		if(!Physics.Linecast(myTransform.position + baseScript.dodgeClearHeightCheckPos, dodgePos, layerMask))
			{
				Debug.DrawLine(myTransform.position + baseScript.dodgeClearHeightCheckPos, dodgePos, Color.green);
				dodgingTarget = dodgePos;
				return true;
			}
		else
				Debug.DrawLine(myTransform.position + baseScript.dodgeClearHeightCheckPos, dodgePos, Color.red);
	
		return false;
	}
			

	public override void AICycle() 
	{
        // if we're not dodging, dodge,
		if(!baseScript.isDodging)
			{
				if(AquireDodgingTarget())
					{													
						StartCoroutine(SetDodge());
					}
				else
					{
						KillBehaviour();
					}
			}
	}
}
}

/*
 * Effectivly a chase behaviour, but the position of the target is only updated once the agent reaches it's first destination
 * */
namespace ParagonAI{
public class Search : ParagonAI.CustomAIBehaviour{

	float radiusToCallOffSearch;

	public override void Initiate()
		{
			base.Initiate();
			radiusToCallOffSearch = baseScript.radiusToCallOffSearch;
		}

	public override void AICycle() 
	{
        //Every time we get close to our goal position, set a new position based on our target's current position.
        if (baseScript.targetTransform && agent.remainingDistance < radiusToCallOffSearch)
        {
            targetVector = baseScript.targetTransform.position;
        }
        else if(!baseScript.targetTransform)
        {
            targetVector = myTransform.position;
        }
	}
}
}

/*
 * If the agent is not engaged in combat, move to the position a sound was heard
 * */
namespace ParagonAI{
public class InvestigateSound : ParagonAI.CustomAIBehaviour{

	float radiusToCallOffSearch;

	public virtual void Inititae()
		{
			base.Initiate();
			radiusToCallOffSearch = baseScript.radiusToCallOffSearch;
		}

	public override void AICycle() 
		{
            //End the behaviour if we get close enough to the source, or we engage a target.
			if(agent.remainingDistance < radiusToCallOffSearch || baseScript.IsEnaging())
				{	
					KillBehaviour();
				}
			else
				{
					targetVector = baseScript.lastHeardNoisePos;
				}
		}
		
	public override void OnEndBehaviour() 
		{
			Destroy(this);
		}		
}
}

/*
 * Move along a path defined by the user
 * */
namespace ParagonAI{
public class Patrol : ParagonAI.CustomAIBehaviour{
		
	bool haveAPatrolTarget = false;
	int currentPatrolIndex = 0;
	float patrolNodeDistSquared;
	
	public virtual void Inititae()
		{
			base.Initiate();
		}

	public override void AICycle() 
	{
		if(baseScript.patrolNodes.Length >= 0)
			{
				//if we don't have a current goal, find one.
				if(!haveAPatrolTarget)
					{
                        SetPatrolNodeDistSquared();
						targetVector = baseScript.patrolNodes[currentPatrolIndex].position;
						haveAPatrolTarget = true;
						
                        //Move the current patrol node index up and loop it around to the beginning if necessary 
						currentPatrolIndex++;
						if(currentPatrolIndex >= baseScript.patrolNodes.Length)
							{
								currentPatrolIndex = 0;
							}
					}
                //if we have one, check if we're to close.  If so, cancel the current goal.
				else if(Vector3.SqrMagnitude(targetVector - myTransform.position) < patrolNodeDistSquared)
					{
						haveAPatrolTarget = false;
					}	
			}
		else
			{
				Debug.LogError("No patrol nodes set!  Please set the array in the inspector, via script, or change the AI's non-engaging behavior");
			}
	}
	
	void SetPatrolNodeDistSquared()
		{
            patrolNodeDistSquared = baseScript.closeEnoughToPatrolNodeDist * baseScript.closeEnoughToPatrolNodeDist;
		}
		
}
}

/*
 * Randomly move around
 * */
namespace ParagonAI{
public class Wander : ParagonAI.CustomAIBehaviour{

	bool haveCurrentWanderPoint = false;

	public virtual void Inititae()
		{
			base.Initiate();
		}
		
	public override void EachFrame()
		{

		}			

	public override void AICycle() 
	{
		if(!haveCurrentWanderPoint)
			{
                //If we have no key transform, randomly choose a goal location within a given radius of agent's current position.
                if (!baseScript.keyTransform)
					targetVector = FindDestinationWithinRadius(myTransform.position);
				else
                    //If we do have a key transform, randomly choose a goal location within a given radius of our key transform.
                    targetVector = FindDestinationWithinRadius(baseScript.keyTransform.position);
				
				haveCurrentWanderPoint = true;
			}
		else if(!agent.pathPending && agent.remainingDistance < baseScript.GetDistToChooseNewWanderPoint())
			{		
				haveCurrentWanderPoint = false;
			}
			
	}

    public Vector3 FindDestinationWithinRadius(Vector3 originPos)
    {
        //Actually returns destination within a square.
        return new Vector3(originPos.x + (Random.value - 0.5f) * baseScript.GetWanderDiameter(), originPos.y, originPos.z + (Random.value - 0.5f) * baseScript.GetWanderDiameter());
    }	
}
}

/*
 * A complex behaviour that involves the use of cover.  
 * If no cover can be found, the agent will charge directly towards their target
 * */
namespace ParagonAI{
public class Cover : ParagonAI.CustomAIBehaviour{

	//bool movingTowardsCrouchPos = false;	
	float maxTimeInCover = 10f;
	float minTimeInCover = 5f;
	bool foundDynamicCover = false;
		
	
	public override void Initiate()
		{
			base.Initiate();
			maxTimeInCover = baseScript.maxTimeInCover;
			minTimeInCover = baseScript.minTimeInCover;
		}

		
	public override void OnEndBehaviour()
		{
			LeaveCover();	
		}
		
	public override void EachFrame()
		{	
			if(coverFinderScript)
				{		
                    //Choose which part of the cpover node we should move to based on whether we are suppressed and firing.
					if(!gunScript.IsFiring() || !baseScript.shouldFireFromCover)
						{
							targetVector = baseScript.currentCoverNodePos;
						}
					else
						{
							targetVector = baseScript.currentCoverNodeFiringPos;
						}

                    
                    if (baseScript.currentCoverNodeScript || foundDynamicCover)
						{
                            //If we can't reach our cover, find a different piece of cover.
                            if (agent.pathStatus == UnityEngine.AI.NavMeshPathStatus.PathPartial)
                                {
                                    LeaveCover();
                                }
                            //Start the countdown to leave cover once we reach it.
							if(!baseScript.inCover && agent.remainingDistance <= 0)
								{
                                    baseScript.inCover = true;
									StartCoroutine(SetTimeToLeaveCover(Random.Range(minTimeInCover, maxTimeInCover)));
								}								
						}
					else 
						{
                            //If we don't have cover, find cover.
                            ParagonAI.CoverData coverData = coverFinderScript.FindCover(baseScript.targetTransform, baseScript.keyTransform);
							
							if(coverData.foundCover)
								{
									SetCover(coverData.hidingPosition, coverData.firingPosition, coverData.isDynamicCover, coverData.coverNodeScript);
									//Play vocalization
									if(soundScript)
										soundScript.PlayCoverAudio();
								}
                             //If we can't find cover, charge at our target.
							else if(baseScript.targetTransform)
								{						
									targetVector = baseScript.targetTransform.position;
								}
						}
				}
            //If we don't have cover, charge at our target.
			else if(baseScript.targetTransform)
				{						
					targetVector = baseScript.targetTransform.position;
				}						
		}

	
		IEnumerator SetTimeToLeaveCover(float timeToLeave)
			{
                //Count down to leave cover
                while (timeToLeave > 0 && (baseScript.currentCoverNodeScript || foundDynamicCover))
					{
						if(baseScript.inCover)						
							timeToLeave--;
						else 
							timeToLeave -= 0.25f;
							
                        
						if(baseScript.targetTransform)
							{
                                //Makes the agent leave cover if it is no longer safe.  Uses the cover node's built in methods to check.
                                if (!foundDynamicCover && !baseScript.currentCoverNodeScript.CheckForSafety(baseScript.targetTransform.position))
									{	
										LeaveCover();
									}
                                //Makes the agent leave cover if it is no longer safe. 
								else if(foundDynamicCover && !Physics.Linecast(baseScript.currentCoverNodePos, baseScript.targetTransform.position, layerMask.value))
									{
										LeaveCover();
									}
							}
						yield return new WaitForSeconds(1);
					}
                if (baseScript.currentCoverNodeScript || foundDynamicCover)
					{
						LeaveCover();
					}
			}
		
        //Called when the agent wants to leave cover.  Sets variables to values appropriate for an agent that is not in cover.
		public void LeaveCover()
		{
            if (baseScript.currentCoverNodeScript)
				{
                    baseScript.currentCoverNodeScript.setOccupied(false);
                    baseScript.currentCoverNodeScript = null;
				}
			else if(foundDynamicCover)
				{
					ParagonAI.ControllerScript.currentController.RemoveACoverSpot(baseScript.currentCoverNodeFiringPos);
				}
			
			baseScript.inCover = false;
			baseScript.SetOrigStoppingDistance();
			
			foundDynamicCover = false;

            if (!baseScript.shouldFireFromCover)
				{
					coverFinderScript.ResetLastCoverPos();
				}
		}	
		
		
        //Used to set variables once cover is found.
		void SetCover(Vector3 newCoverPos, Vector3 newCoverFiringSpot,bool isDynamicCover,ParagonAI.CoverNodeScript newCoverNodeScript)
		{
			baseScript.currentCoverNodePos = newCoverPos;
			baseScript.currentCoverNodeFiringPos = newCoverFiringSpot;
			
			agent.stoppingDistance = 0;
		
			if(isDynamicCover)
				{
					foundDynamicCover = true;
					ParagonAI.ControllerScript.currentController.AddACoverSpot(baseScript.currentCoverNodeFiringPos);			
				}
			else
				{
                    baseScript.currentCoverNodeScript = newCoverNodeScript;
                    baseScript.currentCoverNodeScript.setOccupied(true);			
				}
		}		
}
}

/*
 * This behaviour causes the agent to move to a given position and face in a given direction
 * Once there, the agent will attempt to play an animation and call a method on a given object
 * The behaviour then ends
 * */
namespace ParagonAI{
//You can change the class name from BehaviorChildTemplate to anything else.  
public class DynamicObject : CustomAIBehaviour{
			
	Transform movementTargetTransform;
	string methodToCall;
	string dynamicObjectAnimation;
	
	//Set up some default stuff
	public void StartDynamicObject(Transform newMovementObjectTransform,string newAnimationToUse,string newMethodToCall, bool requireEngaging)
		{
			movementTargetTransform = newMovementObjectTransform;
			dynamicObjectAnimation = newAnimationToUse;	
			agent.stoppingDistance = 0;
				
			baseScript.usingDynamicObject = false;
				
			methodToCall = newMethodToCall;
		}	
	
	//Start things in motion when we arrive at the object
	void UseDynamicObject()
	{
		agent.speed = 0;	
		if(gunScript)
			{
				gunScript.SetCanCurrentlyFire(false);
			}
		StartCoroutine(animationScript.DynamicObjectAnimation(dynamicObjectAnimation, movementTargetTransform.forward, this));	
		baseScript.usingDynamicObject = true;
	}
	
	//Actually do the thing
	public void AffectDynamicObject()
	{
		movementTargetTransform.gameObject.SendMessage(methodToCall);
	}
	
	//Go back to normal
	public void EndDynamicObjectUsage()
	{	
		if(baseScript.usingDynamicObject)
			{
				baseScript.SetProperSpeed();
				baseScript.SetOrigStoppingDistance();	
				baseScript.usingDynamicObject = false;
				if(gunScript)
					{
						gunScript.SetCanCurrentlyFire(true);
					}				
				movementTargetTransform = null;
			}
		KillBehaviour();
	}
	
	void MoveToDynamicObject()
	{	
		if(movementTargetTransform)	
			{	
				Debug.DrawLine(myTransform.position, movementTargetTransform.position, Color.green, baseScript.cycleTime);
                //Once we're sure we've reached the object, use it.
                if (agent.remainingDistance != Mathf.Infinity && agent.pathStatus == UnityEngine.AI.NavMeshPathStatus.PathComplete && agent.remainingDistance <= 0 && !agent.pathPending && !baseScript.usingDynamicObject)
					{	
						UseDynamicObject();
					}
				else if(!baseScript.usingDynamicObject)
					{
						targetVector = movementTargetTransform.position;
					}
					
				//Debug.Break();
			}
		else
			KillBehaviour();
				
	}																
				
	//Your behavior code goes here
	public override void AICycle() 
		{
			MoveToDynamicObject();
		}

    public override void EachFrame()
		{
		
		}						
}
}
