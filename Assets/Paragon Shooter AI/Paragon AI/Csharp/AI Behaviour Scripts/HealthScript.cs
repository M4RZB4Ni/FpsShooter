using UnityEngine;
using System.Collections;

/*
 * Manages the agent's health. 
 * Will trigger the suppresion state on agents using cover if shields are down.
 * */

namespace ParagonAI{
public class HealthScript : MonoBehaviour {

	public float health = 100;
	public float shields = 25;	
	private float maxShields = 10;	
	public bool shieldsBlockDamage = false;	
	public float timeBeforeShieldRegen = 5;
	private float currentTimeBeforeShieldRegen;
	public float shieldRegenRate = 10;	
	public ParagonAI.TargetScript myTargetScript;
	public ParagonAI.BaseScript myAIBaseScript;	
	public ParagonAI.SoundScript soundScript;
	
	public Rigidbody[] rigidbodies;
	public Collider[] collidersToEnable;
	public ParagonAI.RotateToAimGunScript rotateToAimGunScript;
	public Animator animator;
	
	public ParagonAI.GunScript gunScript;
		
	private bool beenHitYetThisFrame = false;

    //Initiation stuff.
	void Awake()
		{
			soundScript = gameObject.GetComponent<ParagonAI.SoundScript>();
            if (shields < 0)
            {
                shields = 0.1f;
            }
			maxShields = shields;
		}
	
	void Update()
	{
		currentTimeBeforeShieldRegen -= Time.deltaTime;

        //Only let us take explosion damage once per frame. (could also be used for weapons that would pass through an agent's body)
        //This will prevent the agent from taking the damage multiple times- once for each hitbox.
		beenHitYetThisFrame = false;
		

		if(currentTimeBeforeShieldRegen < 0  && shields < maxShields)
			{
				shields = Mathf.Clamp(shields + shieldRegenRate*Time.deltaTime, 0, maxShields);

                //When our shields are fully charged, stop being suppressed.
                if (shields == maxShields)
                { 
				    myAIBaseScript.ShouldFireFromCover(true);
                }
			}
	}
	
	public void Damage(float damage)
		{	
            //Look for the source of the damage.
			if(myTargetScript)
				myTargetScript.CheckForLOSAwareness(true);	
		
			ReduceHealthAndShields(damage);
			myAIBaseScript.CheckToSeeIfWeShouldDodge();
				
			if(health <= 0)
				{
					DeathCheck();
				}	
		}
	
	public IEnumerator SingleHitBoxDamage(float damage)
		{
            //Look for the source of the damage.
			if(myTargetScript)
				myTargetScript.CheckForLOSAwareness(true);

            //Only let us take explosion damage once per frame. (could also be used for weapons that would pass through an agent's body)
            //This will prevent the agent from taking the damage multiple times- once for each hitbox.
			if(!beenHitYetThisFrame)
				{
					ReduceHealthAndShields(damage);
									
					if(health <= 0)
						{
							DeathCheck();
						}
					beenHitYetThisFrame = true;	
				}
					
			yield return null;
				beenHitYetThisFrame = false;
		}
	
	
	public void ReduceHealthAndShields(float damage)
		{
            //Shields are mandatory for the suppressioon mechanic to work.
            //However, as you may not want the agent to have any sort of regenerating health, you can choose whether or not they will actually block damage or merely work as a recent damage counter.
			if(shieldsBlockDamage)
				{
					if(damage > shields)
						{
							if(soundScript && myAIBaseScript.HaveCover() && shields > 0)
								soundScript.PlaySuppressedAudio();				
						
                            //Eliminate shields and pass on remaining damage to health.
							damage -= shields;
							shields = 0;
							health -= damage;
							
	                        //If the agent's shields go down, become suppressed (ie: agent will stay in cover as much as possible, and will avoid standing up to fire)
							myAIBaseScript.ShouldFireFromCover(false);								
						}
					else
						{
							shields -= damage;
						}
				}
			else
				{
					if(damage > shields)
						{
							if(soundScript && myAIBaseScript.HaveCover() && shields > 0)
								soundScript.PlaySuppressedAudio();

                            //If the agent's shields go down, become suppressed (ie: agent will stay in cover as much as possible, and will avoid standing up to fire)
							myAIBaseScript.ShouldFireFromCover(false);
						}	
				    
	                //Do the same amount of damage to shields AND health.
					shields = Mathf.Max(shields-damage, 0);
					health -= damage;										
				}
					
			currentTimeBeforeShieldRegen = timeBeforeShieldRegen;
			
			//Sound
			if(soundScript)
				soundScript.PlayDamagedAudio();
		}
	
    //Check to see if we are dead.
	void DeathCheck()
		{
			KillAI();
		
			if(myAIBaseScript)
				myAIBaseScript.KillAI();
			this.enabled = false;
		}
	
	void KillAI()
		{
			//Check if we've done this before
			if(this.enabled)
				{
					int i;
						
                    //Enable the ragdoll
					for(i = 0; i < rigidbodies.Length; i++)
						{
							rigidbodies[i].isKinematic = false;
						}
					for(i = 0; i < collidersToEnable.Length; i++)
						{				
							collidersToEnable[i].enabled = true;
						}

						
                    //Disable scripts
					if(rotateToAimGunScript)
						rotateToAimGunScript.enabled = false;		
						
					if(animator)
						animator.enabled = false;				
											
					if(gunScript)
						{
							gunScript.enabled = false;
						}
							
					this.enabled = false;
				}
		}
}
}
