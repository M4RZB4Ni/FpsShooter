using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
//Selects what behaviours to use, and defines the parameters of the default behaviours
//Also handles melee
*/
namespace ParagonAI{
[RequireComponent (typeof (UnityEngine.AI.NavMeshAgent))]
public class BaseScript : MonoBehaviour 
{
	//Base Stuff
	//Scripts
	public ParagonAI.GunScript gunScript;
	public ParagonAI.SoundScript audioScript;
	public ParagonAI.RotateToAimGunScript headLookScript;
	public ParagonAI.AnimationScript animationScript;
	public ParagonAI.CoverFinderScript coverFinderScript;	
	
	public Transform targetTransform;
	
	private LayerMask layerMask;
	
	UnityEngine.AI.NavMeshAgent agent;
	Transform myTransform;
	
	ParagonAI.TargetScript myTargetScript;
	
	//Speed and navigation stuff
	float origAgentStoppingDist;
	
	public float sprintSpeed = 6.0f;
	public float runSpeed = 5.0f;
	public float alertSpeed = 4.0f;
	public float idleSpeed = 3.0f;
		
	//AI Behavior Stuff
	public AIType myAIType = AIType.Tactical;
	public IdleBehaviour myIdleBehaviour = IdleBehaviour.Wander;
	
	//Status Stuff
	bool engaging;	
	public float cycleTime = 0.2f;
	
	//Searching Stuff
	public float radiusToCallOffSearch = 5.0f;
	public Vector3 lastHeardNoisePos;
	
	//Cover
	public bool inCover = false;
    [HideInInspector]
    public CoverNodeScript currentCoverNodeScript;
    public float timeBetweenSafetyChecks = 1.0f;
	public Vector3 currentCoverNodePos;
	public Vector3 currentCoverNodeFiringPos;
	//bool movingTowardsCrouchPos = false;	
	public float maxTimeInCover = 10f;
	public float minTimeInCover = 5f;
    public bool shouldFireFromCover = true;
	bool foundDynamicCover = false;
	
	//Dodging
	//[Range (0.0f, 100.0f)]
	//public float timeBetweenDodges = 3.0f;
	public float dodgingSpeed = 6.0f;
	//public bool canDodge = true;
	public float dodgingTime = 0.5f;
	Vector3 dodgingTarget;
	public float dodgingClearHeight = 1.0f;
	[HideInInspector]
	public Vector3 dodgeClearHeightCheckPos;
	[HideInInspector]
	public float origAcceleration;
	float timeUntilNextDodge;
	public float timeBetweenLoSDodges = 4.0f;
	public bool shouldTryAndDodge = true;
	[HideInInspector]
	public bool isDodging = false;
    public float minDistToDodge = 5;
	
	
	//Patrolling
	public float closeEnoughToPatrolNodeDist = 5.0f;
	public Transform[] patrolNodes;
	public bool shouldShowPatrolPath = false;
	
	//Wandering
    public float wanderDiameter = 50;
	//float wanderDiameterSquared;
	public float distToChooseNewWanderPoint = 4;
	
	//Defending
	public Transform keyTransform;
	
	//Melee
	public bool canMelee = false;
	public float meleeDamage = 100;
	public float timeBetweenMelees = 2.0f;
	public float meleeRange = 2.0f;
	public float timeUntilMeleeDamageIsDealt = 0.2f;
    [HideInInspector]
	public bool isMeleeing = false;
	
	public bool canSprint = false;
	public float distFromTargetToSprint = 25f;
				
	public enum AIType {
		Berserker = 0, Tactical = 1, Custom = 2,
	}
	
	public enum IdleBehaviour{
		Search = 1, Patrol = 2, Wander = 3, MoveToTransform = 4, Custom = 5,
	}
	
	public enum AIBehaviour{
        GoToMoveTarget = 0, Cover = 1, Search = 2, Dodging = 3, Patrolling = 4, Wander = 5, UseDynamicObject = 6, InvestigateSound = 7, Custom = 8, MoveToTransform = 9, RunFromGrenade = 10,
	}
		
	// Use this for initialization
	void Awake () 
	{
		//initialize various values.  
		//Mainly taking inputs from the user and putting them into the formats we use later, 
		//eg: squared values for faster distance comparison.	
		myTransform = transform;
		
		
		timeUntilNextDodge = timeBetweenLoSDodges*Random.value;			
		dodgeClearHeightCheckPos = Vector3.zero;
		dodgeClearHeightCheckPos.y = dodgingClearHeight;
		
		distFromTargetToSprint = distFromTargetToSprint*distFromTargetToSprint;
		meleeRange = meleeRange*meleeRange;
		
		if(gameObject.GetComponent<UnityEngine.AI.NavMeshAgent>() != null){
			agent = gameObject.GetComponent<UnityEngine.AI.NavMeshAgent>();
			origAcceleration = agent.acceleration;
			agent.speed = idleSpeed;
			origAgentStoppingDist = agent.stoppingDistance;
		}
		else{
			Debug.LogWarning("No navmesh agent on the same object as the BaseScript!  Please add one!");
			this.enabled = false;
			return;
		}
				
		if(idleSpeed > runSpeed){
			idleSpeed = runSpeed;
		}	
		if(headLookScript){
			headLookScript.Deactivate();
		}		
	}
	
	//Start the cycle after everything else is initialized	
	void Start()
	{		
		GetDefaultBehaviours();	
		if(ParagonAI.ControllerScript.currentController != null)
			{
				layerMask = ParagonAI.ControllerScript.currentController.GetLayerMask();
			}
		else 
			{
				this.enabled = false;
			}
		StartCoroutine("AICycle");
	}
	

	IEnumerator AICycle()
	{
        //Will stop if the script is disabled at any point.
		while(this.enabled)
			{
				if(currentBehaviour != null)
					{
						currentBehaviour.AICycle();
					}				
				
                //Check to see if we should dodge.
                //Dodges are automatically triggered after spending time in their target's line of sight
				if(!isDodging)
					{
						if(myTransform && targetTransform && canMelee && !isMeleeing && Vector3.SqrMagnitude(myTransform.position - targetTransform.position) < meleeRange)
							{
								animationScript.StopSprinting();	
								SetProperSpeed();
								headLookScript.Activate();
								StartCoroutine(AttackInMelee());
							}
						else
							{
								if(!isMeleeing && Vector3.SqrMagnitude(myTransform.position - currentBehaviour.targetVector) > distFromTargetToSprint && canSprint && engaging)
									{
										animationScript.StartSprinting();	
										SetSprintSpeed();		
										headLookScript.Deactivate();		
									}							
								MoveAI();	
							}
					}
				
				if(Vector3.SqrMagnitude(myTransform.position - currentBehaviour.targetVector) < distFromTargetToSprint && engaging)
				{
					animationScript.StopSprinting();
					SetProperSpeed();
					headLookScript.Activate();	
				}			
				yield return new WaitForSeconds(cycleTime);	
			}
	}
	
	CustomAIBehaviour currentBehaviour;
	
	bool canOverrideBehaviour = true;
	
	public CustomAIBehaviour idleBehaviour;
	public CustomAIBehaviour combatBehaviour;
	
    //Set our idle behaviour.  If we are not using an override behaviour, start or restart the appropriate behaviour.
	public void SetIdleBehaviour(CustomAIBehaviour c){
		idleBehaviour = c;
	
		if(canOverrideBehaviour)
			SetBehaviour();
	}

    //Set our combat behaviour.  If we are not using an override behaviour, start or restart the appropriate behaviour.
	public void SetCombatBehaviour(CustomAIBehaviour c){
		combatBehaviour = c;
	
		if(canOverrideBehaviour)
			SetBehaviour();
	}	
	
	public bool hasOverrideBehaviour()
	{
		return !canOverrideBehaviour;
	}

	
	void SetBehaviour()
	{
        //set our top speed, depending on whether or not we are currently engaging
        SetProperSpeed();
		if(currentBehaviour)
			currentBehaviour.OnEndBehaviour();
		
	    //If we're engaging, perform our combat behaviour
		if(engaging && combatBehaviour) {
				SetCurrentBehaviour(combatBehaviour);
		}
        //If we are not engaging, perform our idle behaviour
		else if(idleBehaviour) {
				SetCurrentBehaviour(idleBehaviour);
		}
	}
	
	void SetCurrentBehaviour(CustomAIBehaviour b){
        //Call OnEndBehaviour on the behaviour we are replacing, if any.  
        //This lets us perform any cleanup operations
        //For example, marking a cover location as "free"
		if(currentBehaviour && canOverrideBehaviour){
			currentBehaviour.OnEndBehaviour();
		}

		currentBehaviour = b;	
	}

    bool usingOverrideBehaviour = false;		
	public void SetOverrideBehaviour(CustomAIBehaviour c, bool b)
	{
        //Only override behaviours if we don't already have an override behaviour.
		if(canOverrideBehaviour)
			{
                usingOverrideBehaviour = true;
				if(currentBehaviour)
					currentBehaviour.OnEndBehaviour();	
					
				canOverrideBehaviour = !b;						
				currentBehaviour = c;
			}
	}

    //End an override behaviour.
    public void KillOverrideBehaviour()
    {
        if (usingOverrideBehaviour)
        {
            //KillBehaviour automatically calls OnEndBehaviour
            currentBehaviour.KillBehaviour();
        }
    }
	
    //Convert from enum to AIBehaviour
	CustomAIBehaviour GetIdleBehaviour()
	{
		switch(myIdleBehaviour)
			{
				case IdleBehaviour.Search : return GetNewBehaviour(AIBehaviour.Search);
				case IdleBehaviour.Wander : return GetNewBehaviour(AIBehaviour.Wander);
				case IdleBehaviour.Patrol : return GetNewBehaviour(AIBehaviour.Patrolling);
                case IdleBehaviour.MoveToTransform: return GetNewBehaviour(AIBehaviour.MoveToTransform);
			}	
		return null;
	}

    //Convert from enum to AIBehaviour
	CustomAIBehaviour GetCombatBehaviour()	
	{		
		if(myAIType == AIType.Tactical)
			{
				return GetNewBehaviour(AIBehaviour.Cover);
			}	
		else if(myAIType == AIType.Berserker)
			{
				return GetNewBehaviour(AIBehaviour.GoToMoveTarget);			
			}
		else return null;				
	}	

	//Set the default combat and idle behaviours based on the myAIType and myIdleBehaviour variables
	void GetDefaultBehaviours()
	{
		SetCombatBehaviour(GetCombatBehaviour());
		SetIdleBehaviour(GetIdleBehaviour());
	}		
	
    //Convert from AIBehaviour to ParagonAI.CustomBehaviour
	public CustomAIBehaviour GetNewBehaviour(AIBehaviour t)
	{
		ParagonAI.CustomAIBehaviour  bToReturn = null;
		switch(t)
			{					
				case AIBehaviour.Search : bToReturn = (ParagonAI.CustomAIBehaviour)gameObject.AddComponent(typeof(ParagonAI.Search)); break;
				case AIBehaviour.Dodging : bToReturn = (ParagonAI.CustomAIBehaviour)gameObject.AddComponent(typeof(ParagonAI.Dodge)); break;
				case AIBehaviour.Cover : bToReturn = (ParagonAI.CustomAIBehaviour)gameObject.AddComponent(typeof(ParagonAI.Cover)); break;
				case AIBehaviour.Patrolling : bToReturn = (ParagonAI.CustomAIBehaviour)gameObject.AddComponent(typeof(ParagonAI.Patrol)); break;
				case AIBehaviour.Wander : bToReturn = (ParagonAI.CustomAIBehaviour)gameObject.AddComponent(typeof(ParagonAI.Wander)); break;
				case AIBehaviour.GoToMoveTarget : bToReturn = (ParagonAI.CustomAIBehaviour)gameObject.AddComponent(typeof(ParagonAI.ChargeTarget)); break;	
				case AIBehaviour.UseDynamicObject : bToReturn = (ParagonAI.CustomAIBehaviour)gameObject.AddComponent(typeof(ParagonAI.DynamicObject)); break;
				case AIBehaviour.InvestigateSound : bToReturn = (ParagonAI.CustomAIBehaviour)gameObject.AddComponent(typeof(ParagonAI.InvestigateSound)); break;
                case AIBehaviour.MoveToTransform: bToReturn = (ParagonAI.CustomAIBehaviour)gameObject.AddComponent(typeof(ParagonAI.MoveToTransform)); break;
                case AIBehaviour.RunFromGrenade: bToReturn = (ParagonAI.CustomAIBehaviour)gameObject.AddComponent(typeof(ParagonAI.RunAwayFromGrenade)); break; 
			}
		bToReturn.Initiate();	
		return bToReturn;
	}
	
    //Move to the target position designated by the current behaviour
	void MoveAI()
	{
		if(this.enabled && currentBehaviour)
			agent.SetDestination(currentBehaviour.targetVector);
	}	

	public void SetMyTarget(Transform currentEnemyTransform, Transform losTargetTransform)
	{
		targetTransform = currentEnemyTransform;
		headLookScript.SetTargetTransform(targetTransform);	
	    
        //Assign line of sight and firing targets.
		if(gunScript)
			{
				gunScript.AssignTarget(targetTransform, losTargetTransform);		
			}
	}
	
	public void RemoveMyTarget()
	{
		targetTransform = null;
	}
	
    //Switch all components to an engaging state
	public void StartEngage()
	{	
		//print("HEY!");
		if(animationScript)
			animationScript.SetEngaging();
			
		if(audioScript)
			audioScript.PlaySpottedAudio();	
	
		engaging = true;
		
		if(canOverrideBehaviour)
			SetBehaviour();
		
		if(agent)	
			agent.speed = runSpeed;
			
		headLookScript.Activate();
	}
	
    //Stop looking at the target.
	public void EndEngage()
	{	
		//print("YO!");					
		headLookScript.Deactivate();
		
		engaging = false;
		if(canOverrideBehaviour)
			SetBehaviour();
		
		//if(animationScript)
		//	animationScript.SetDisengage();	
		
		//if(agent)
		//	agent.speed = alertSpeed;
			
		if(gunScript)
			gunScript.EndEngage();
	}		
	
	public void SetSpeed(float x)
	{
		agent.speed = x;
	}
	
	public void SetAlertSpeed()
	{
		agent.speed = alertSpeed;
	}

	public void SetSprintSpeed()
	{
		agent.speed = sprintSpeed;
	}

    public void SetProperSpeed()
    {
        if (engaging)
            {
                agent.speed = runSpeed;
            }
        else
            {
                agent.speed = idleSpeed;
            }
    }

	//Start/End Engaging Stuff
	//Deal with stuff on a frame-by-frame basis, like whether or not we are behind cover or not			
	void LateUpdate () 
	{	
        //Because Override Behaviours simply destroy themselves upon completion, we need to check to see if we have a behaviour and if we don't, then set one.
		if(currentBehaviour != null)
			{
				currentBehaviour.EachFrame();
			}
		else 
			{
				canOverrideBehaviour = true;	
				SetBehaviour();		
			}

		if(!inCover)
			{		
				if(targetTransform && shouldTryAndDodge && engaging && !isDodging)
					{
                        //Random dodging if the agent has LoS to their target
                        //Only do it if we're not in cover.
						if(timeUntilNextDodge < 0)
							{					
								CheckToSeeIfWeShouldDodge();
								timeUntilNextDodge = Random.value * timeBetweenLoSDodges;
							}
						else
							{
								timeUntilNextDodge -= Time.deltaTime;	
							}
					}
			}			
	}
	
		
	public void CheckToSeeIfWeShouldDodge()
	{	
        //Make sure we're out of cover, not dodging already, and environment allows for a dodge (ie: make sure we won't dodge into a wall.)
		if(myTransform && targetTransform && !inCover && !isDodging && !Physics.Linecast(myTransform.position, targetTransform.position, layerMask) && Vector3.Distance(myTransform.position, targetTransform.position) > minDistToDodge)
				{	
					if(canOverrideBehaviour)
						SetOverrideBehaviour(GetNewBehaviour(AIBehaviour.Dodging), true);	
				}	
	}			
	//Dodging///////////////////////////////////////////////////////////
	//Melee///////////////////////////////////////////////////////////
	IEnumerator AttackInMelee()
	{
		isMeleeing = true;
		agent.speed = 0;
		
        //Play animation
		if(animationScript)
			{
				StartCoroutine(animationScript.StartMelee());
			}

        //Used to sync the dealing of the damage with the appropriate point in the animation.
		yield return new WaitForSeconds(timeUntilMeleeDamageIsDealt);
        DealDamage(meleeDamage, meleeRange, targetTransform.position);
	}	
	
	//Feel free to change the code here to suit your game
	void DealDamage(float damage,float range,Vector3 enemyPos) 
		{
            //Simple distance check
            //TODO:  Stop agent from doing damage if it's behind a wall
			if(Vector3.SqrMagnitude(enemyPos - myTransform.position) <= (range*range))
				{
                    //The only script we know for sure that will be on our target will be a target script
                    //Don't want to just call ApplyDamage on all scripts because the target script may not be on the same object as the health script
                    //Use the targetScript to pass the damage onto the health script (which can be any script, not just a ParagonAI HealthScript)
                    myTargetScript.currentEnemyTarget.targetScript.ApplyDamage(meleeDamage);
				}

		}

    public void StopMelee()
    {
        SetProperSpeed();
        isMeleeing = false;
    }
	
	//Melee///////////////////////////////////////////////////////////	
	
	
	//Draw Patrol path
	void OnDrawGizmos()
	{
		if(shouldShowPatrolPath && patrolNodes.Length > 1)
			{
				for(int i = 1; i < patrolNodes.Length; i++)
					{
						if(patrolNodes[i])
							{
								Gizmos.color = Color.yellow;
								Gizmos.DrawLine(patrolNodes[i].position, patrolNodes[i-1].position);
							}
					}
			}
	}	

    //Run From Grenade
    public Transform transformToRunFrom = null;
    public float distToRunFromGrenades = 10;
    public bool shouldRunFromGrenades = true;

    public void WarnOfGrenade(Transform t, float d)
    {
        if (canOverrideBehaviour && shouldRunFromGrenades)
        {
            transformToRunFrom = t;
            distToRunFromGrenades = d;
            SetOverrideBehaviour(GetNewBehaviour(AIBehaviour.RunFromGrenade), true);
        }
    }
	

		
	//Dynamic Objects///////////////////////////////////////////////////////////	
	Transform movementTargetTransform;
	string methodToCall;
	AIBehaviour lastBehavior;
	string dynamicObjectAnimation;
	
	[HideInInspector]
	public bool usingDynamicObject = false;
	public bool canUseDynamicObject = true;

	public bool SetDynamicObject(Transform newMovementObjectTransform,string anim,string newMethodToCall, bool requireEngaging)
	{
        //Make sure that we aren't doing anything that would interfere with dynamic object usage,
		if(!isDodging && (!requireEngaging || engaging) && canOverrideBehaviour && canUseDynamicObject)
			{			
				DynamicObject d = (DynamicObject)GetNewBehaviour(AIBehaviour.UseDynamicObject);		
				SetOverrideBehaviour(d, true);
				
				d.StartDynamicObject(newMovementObjectTransform, anim, newMethodToCall, requireEngaging);
	
				return true;
			}
		return false;
	}
	//Dynamic Objects///////////////////////////////////////////////////////////	
	
    //Sounds
	public IEnumerator HearSound(Vector3 s){
        //Investigate the position where we heard the sound
		yield return new WaitForSeconds(0.1f);
		lastHeardNoisePos = s;
		SetOverrideBehaviour(GetNewBehaviour(AIBehaviour.InvestigateSound), false);	
	}

    public float timeUntilBodyIsDestroyedAfterDeath = 60;

	//Kill AI///////////////////////////////////////////////////////////	
	public void KillAI()
	{
		//Check if we can actually do this, to stop errors in wierd edge cases
		if(this.enabled)
			{
                if (combatBehaviour)
				    combatBehaviour.KillBehaviour();
                if (idleBehaviour)
				    idleBehaviour.KillBehaviour();

                //Call method on other scripts on this object, in case extra stuff needs to be done when the AI dies.
                gameObject.SendMessage("OnAIDeath", SendMessageOptions.DontRequireReceiver);
                //GameObject.Destroy(gameObject, timeUntilBodyIsDestroyedAfterDeath);		
                GameObject.Destroy(gameObject);			
            }
        }	

	
    //Setters
	public void SetTargetObj(ParagonAI.TargetScript x)
	{
		myTargetScript = x;
	}	
	public void SetOrigStoppingDistance()
	{
		if(agent)
			agent.stoppingDistance = origAgentStoppingDist;
	}
	public void ShouldFireFromCover(bool b)
	{
		shouldFireFromCover = b;
	}
	
	
	//Getters
	public bool UsingDynamicObject()
	{
		return usingDynamicObject;
	}

	
	public bool HaveCover() 
	{
		return (currentCoverNodeScript != null || foundDynamicCover);
	}	

	
	public Transform GetTranform()
	{
		return myTransform;
	}
	
	public bool IsEnaging()
	{
		return engaging;
	}
	
	public float MaxSpeed()
	{
		return runSpeed;
	}	
	public Vector3 GetCurrentCoverNodePos()
	{
		return currentCoverNodePos;
	}

	
	public LayerMask GetLayerMask()
	{
		return layerMask;
	}
	
	public bool isCurrentlyActive()
	{	
		if(this)
			return this.enabled;
		return false;
	}
	
	public int[] GetEnemyTeamIDs()
	{
		return GetEnemyIDsFromTargetObj();
	}	

    public UnityEngine.AI.NavMeshAgent GetAgent()
	{
		return agent;
	}
	
	public float GetWanderDiameter()
	{
		return wanderDiameter;
	}
	
	public float GetDistToChooseNewWanderPoint()
	{
		return distToChooseNewWanderPoint;
	}
	
	//Helpers
	int[] GetEnemyIDsFromTargetObj()
	{
		if(myTargetScript)
			return myTargetScript.GetEnemyTeamIDs();
		return null;
	}
	
		
}
}

	