using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace VSX.UniversalVehicleCombat
{

	/// <summary>
    /// This represents a health generator module for a vehicle (which can be damaged or healed).
    /// </summary>
	public class HealthGenerator : MonoBehaviour, IModule, IHealthGenerator
	{

		[Header("Module")]

		[SerializeField]
		protected string label;
		public string Label { get { return label; } }
		
		protected ModuleType moduleType;
		public virtual ModuleType ModuleType {
            get
            {
                switch (healthType)
                {
                    case HealthType.Armor:
                        return ModuleType.ArmorGenerator;
                    case HealthType.Shield:
                        return ModuleType.ShieldGenerator;
                }
                return ModuleType.Utility;
            }
        }
		
		[SerializeField]
		protected Sprite menuSprite;
		public Sprite MenuSprite { get { return menuSprite; } }
		
		protected GameObject cachedGameObject;
		public GameObject CachedGameObject { get { return cachedGameObject; } }
		
		protected Transform cachedTransform;
		public Transform CachedTransform { get { return cachedTransform; } }

		protected ModuleState moduleState;
		public ModuleState ModuleState { get { return moduleState; } }
	
		protected ModuleMount moduleMount;
	
        [Header("Health Module")]

		[SerializeField]
		protected HealthType healthType;
		public HealthType HealthType { get { return healthType; } }

		[SerializeField]
		protected float healthRechargeRate;
		public float HealthRechargeRate { get { return healthRechargeRate; } }

		[SerializeField]
		protected float startingHealthValue;
		public float StartingHealthValue { get { return startingHealthValue; } }

		protected float currentHealthValue;
		public float CurrentHealthValue { get { return currentHealthValue; } }

        [SerializeField]
        protected float collisionVelocityToDamageCoefficient = 10;
		public float CollisionVelocityToDamageCoefficient { get { return collisionVelocityToDamageCoefficient; } }

		[Header("Audio")]

		[SerializeField]
        protected float collisionDamageToVolumeCoefficient = 0.1f;	

		[SerializeField]
		protected AudioSource audioSource;
		private bool hasAudioSource = false;
	
		[SerializeField]
		protected AudioClip collisionAudioClip;
		protected bool hasCollisionAudioClip;

		[SerializeField]
		protected AudioClip weaponHitAudioClip;
		protected bool hasWeaponHitAudioClip;

		[SerializeField]
		protected float maxCollisionVolume = 1f;

		
		protected virtual void Awake()
		{
			cachedTransform = transform;
			cachedGameObject = gameObject;

			hasAudioSource = audioSource != null;

		}


		// Reset health values on enable
	    void OnEnable()
		{
			ResetModule();
	    }


        /// <summary>
        /// Reset the module to starting conditions
        /// </summary>
		public void ResetModule()
		{
			// Reset the health value
			currentHealthValue = startingHealthValue;

			// If module has not already been deactivated for some reason then activate it
			if (moduleState != ModuleState.Deactivated) SetModuleState (ModuleState.Activated);
		}


		/// <summary>
        /// Set a new module state for this health generator module.
        /// </summary>
        /// <param name="newState">The new state for the module.</param>
		public virtual void SetModuleState(ModuleState newState)
		{
            moduleState = newState;
		}

		
		/// <summary>
        /// Recharge the health generator
        /// </summary>
        /// <param name="healthValueToAdd">The amount of health to add.</param>
		public virtual void AddHealth(float healthValueToAdd)
		{
            currentHealthValue = Mathf.Clamp(currentHealthValue + healthValueToAdd, 0f, startingHealthValue);
		}

		
        /// <summary>
        /// Set the health of this health generator.
        /// </summary>
        /// <param name="newHealthValue">The new health value.</param>
		public virtual void SetHealth(float newHealthValue)
		{

			// Set the new healthValue
			currentHealthValue = newHealthValue;

            if (moduleState == ModuleState.Destroyed)
            {
                moduleState = ModuleState.Activated;
            }
		}


		/// <summary>
        /// Damage event passed from the health fixture.
        /// </summary>
        /// <param name="damage">The amount of damage.</param>
        /// <param name="hitPosition">The world position of the hit.</param>
        /// <param name="attacker">The game agent responsible for the damage.</param>
		public virtual void Damage(float damage, Vector3 hitPosition, GameAgent attacker)
		{
            
            currentHealthValue -= damage;
				
			// If the generator is depleted, disable it
			if (currentHealthValue <= 0f)
			{ 
				SetModuleState(ModuleState.Destroyed);
			}

           if (moduleMount.Vehicle.HasHealth)
				moduleMount.Vehicle.Health.OnDamage(damage, hitPosition, attacker);

			bool isCollision = attacker == null;

			if (isCollision)
			{
				if (hasCollisionAudioClip)
				{
					if (hasAudioSource)
					{
						audioSource.volume = Mathf.Min(collisionDamageToVolumeCoefficient * damage, maxCollisionVolume);
					}
					audioSource.PlayOneShot(collisionAudioClip);
				}
			}
			else
			{
				if (hasWeaponHitAudioClip)
					audioSource.PlayOneShot(weaponHitAudioClip);
			}
		}


        /// <summary>
        /// Damage event passed from the health fixture.
        /// </summary>
        /// <param name="damageValueByHealthType">The amount of damage by health type.</param>
        /// <param name="hitPosition">The world position of the hit.</param>
        /// <param name="attacker">The game agent responsible for the damage.</param>
        public virtual void Damage(List<float> damageValueByHealthType, Vector3 hitPosition, GameAgent attacker)
		{

			float damage = damageValueByHealthType[(int)healthType];

			Damage(damage, hitPosition, attacker);	

		}


        /// <summary>
        /// Heal event passed from the health fixture.
        /// </summary>
        /// <param name="healingValue">The amount of healing.</param>
        /// <param name="interactionPosition">The world position of the healing interaction.</param>
        /// <param name="healer">The game agent responsible for the healing.</param>
		public virtual void Heal(float healingValue, Vector3 interactionPosition, GameAgent healer)
        {

            currentHealthValue = Mathf.Clamp(currentHealthValue + healingValue, 0f, startingHealthValue);

            
            if (moduleMount.Vehicle.HasHealth)
                moduleMount.Vehicle.Health.OnHeal(healingValue, interactionPosition, healer);

        }


        /// <summary>
        /// Heal event passed from the health fixture.
        /// </summary>
        /// <param name="healingValuesByHealthType">The amount of healing by health type.</param>
        /// <param name="interactionPosition">The world position of the healing interaction.</param>
        /// <param name="healer">The game agent responsible for the healing.</param>
		public virtual void Heal(List<float> healingValuesByHealthType, Vector3 interactionPosition, GameAgent healer)
        {

            float healingValue = healingValuesByHealthType[(int)healthType];

            Heal(healingValue, interactionPosition, healer);

        }



        /// <summary>
        /// Event called when this health generator module is mounted at a module mount
        /// </summary>
        /// <param name="moduleMount">The module mount where this health generator module has been loaded. </param>
        public virtual void OnMount(ModuleMount moduleMount)
		{
			
			this.moduleMount = moduleMount;
			
			cachedTransform.SetParent(moduleMount.CachedTransform);
			cachedTransform.localPosition = Vector3.zero;
			cachedTransform.localRotation = Quaternion.identity;
			
			cachedGameObject.SetActive(true);

		}
			

		/// <summary>
        /// Event called when this health generator module is unmounted from a module mount
        /// </summary>
		public virtual void OnUnmount()
		{
			this.moduleMount = null;
			cachedGameObject.SetActive(false);
			
		}

	}
}
