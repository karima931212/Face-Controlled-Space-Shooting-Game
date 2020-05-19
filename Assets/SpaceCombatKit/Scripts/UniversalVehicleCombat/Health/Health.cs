using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using VSX.General;

namespace VSX.UniversalVehicleCombat 
{

    /// <summary>
    /// An event for running functions when the vehicle is damaged.
    /// </summary>
    /// <param name="damage">The amount of damage.</param>
    /// <param name="hitPoint">The world position where the hit occurred.</param>
    /// <param name="attacker">The game agent responsible for the attack.</param>
	public delegate void DamageEventHandler (float damageValue, Vector3 hitPoint, GameAgent attacker);


    /// <summary>
    /// An event for running functions when the vehicle is healed.
    /// </summary>
    /// <param name="healingValue">The amount of healing.</param>
    /// <param name="interactionPosition">The world position where the healing occurred.</param>
    /// <param name="healer">The game agent responsible for the healing.</param>
	public delegate void HealEventHandler(float healingValue, Vector3 interactionPosition, GameAgent healer);


    /// <summary>
    /// The different health types that vehicles can have.
    /// </summary>
    public enum HealthType
	{
		Armor,
		Shield
	}


    /// <summary>
    /// This class is used by the Health subsystem to keep track of health fixtures and their associated health generators.
    /// </summary>
    [System.Serializable]
	public class HealthFixtureInstance
	{

		public HealthFixture healthFixture;

		public ModuleMount linkedModuleMount;
	
		private IHealthGenerator healthGenerator;
		public IHealthGenerator HealthGenerator { get { return healthGenerator; } }

		private bool hasHealthGenerator = false;
		public bool HasHealthGenerator { get { return hasHealthGenerator; } }

        /// <summary>
        /// Clear the previous health generator for this health fixture instance
        /// </summary>
        public void ClearHealthGenerator()
        {
            if (hasHealthGenerator)
            {
                healthFixture.onDamageEventHandler -= healthGenerator.Damage;
                healthFixture.onHealEventHandler -= healthGenerator.Heal;
            }
        }

        /// <summary>
        /// Set the health generator for this health fixture instance
        /// </summary>
        /// <param name="healthGenerator"></param>
        public void SetGenerator(IHealthGenerator healthGenerator)
		{
			this.healthGenerator = healthGenerator;
			this.hasHealthGenerator = healthGenerator != null;

            if (this.hasHealthGenerator)
            {
                healthFixture.onDamageEventHandler += healthGenerator.Damage;
                healthFixture.onHealEventHandler += healthGenerator.Heal;
            }
		}
	}


    /// <summary>
    /// This class represents the health subsystem of a Vehicle and provides a way to access the vehicle's current health status.
    /// </summary>
    /// <remarks>
    /// This class provides a way to access the current health values for each of the different health types.
    /// It also keeps track of and manages all the health fixtures and health generators on the vehicle.
    /// </remarks>
    public class Health : Subsystem, IHealthInfo 
	{
		
		[SerializeField]
		private bool isDamageable = true;
		public bool IsDamageable
		{ 
			get { return isDamageable; } 
			set 
			{ 
				isDamageable = value; 
				for (int i = 0; i < healthFixtureInstances.Count; ++i)
				{
					healthFixtureInstances[i].healthFixture.IsDamageable = isDamageable;
				}
			}
		}

		[SerializeField]
		private List<HealthFixtureInstance> healthFixtureInstances = new List<HealthFixtureInstance>();

		// Track health values
		private List<float> totalStartingHealthValuesByType = new List<float>();
		private List<float> totalCurrentHealthValuesByType = new List<float>();
		
		// Delegate to attach functions to hit events
		public DamageEventHandler damageEventHandler;
        public HealEventHandler healEventHandler;

        private GameAgent lastAttacker = null;
        public GameAgent LastAttacker
        {
            get { return lastAttacker; }
            set { lastAttacker = value; }
        }

		Vehicle vehicle;

        int frame = 0;
		


		void OnEnable()
		{

			vehicle = GetComponent<Vehicle>();

			UpdateHealthData();

		}

		
		void Awake()
		{
		
			// Initialize the health value tracking
			totalStartingHealthValuesByType.Clear();
			totalCurrentHealthValuesByType.Clear();
			for (int i = 0; i < Enum.GetValues(typeof(HealthType)).Length; ++i)
			{
				totalStartingHealthValuesByType.Add(0);
				totalCurrentHealthValuesByType.Add(0);
			}

			// Gather and deactivate all health fixtures. They will be activated when an appropriate health generator is found.
			for (int i = 0; i < healthFixtureInstances.Count; ++i)
			{
				healthFixtureInstances[i].healthFixture.SetIndex(i);
			}
		}

		void Start()
		{
			// Set the root gameobject for the health fixtures
			for (int i = 0; i < healthFixtureInstances.Count; ++i)
			{
				healthFixtureInstances[i].healthFixture.RootGameObject = vehicle.CachedGameObject;
			}
		}


        /// <summary>
        /// Reset the subsystem to starting conditions
        /// </summary>
		public override void ResetSubsystem()
		{
            // Reset the health fixture activation states to original conditions
			for (int i = 0; i < healthFixtureInstances.Count; ++i)
			{
				healthFixtureInstances[i].healthFixture.SetActivation(CheckCanActivate(i));
			}
		}


		/// <summary>
        /// Event called when a new module is mounted on one of the vehicle's module mounts, to check if it 
        /// is a health generator.
        /// </summary>
        /// <param name="moduleMount">The module mount where the new module was loaded.</param>
		protected override void OnModuleMounted(ModuleMount moduleMount)
		{

            // Get any generator mounted here
			IModule module = moduleMount.Module();
			IHealthGenerator healthGenerator = module == null ? null : module.CachedTransform.GetComponent<IHealthGenerator>();
            
            for (int i = 0; i < healthFixtureInstances.Count; ++i)
			{
				// Update any HealthFixtureInstance class instances that are linked to this module mount
				if (healthFixtureInstances[i].linkedModuleMount == moduleMount)
				{

					if (healthGenerator != null)
					{	

						// Make sure the generator is of correct type
						if (healthGenerator.HealthType != healthFixtureInstances[i].healthFixture.HealthType)
							continue;

						// Unlink the old generator and link the new one
						if (healthFixtureInstances[i].HasHealthGenerator)
						{
							totalStartingHealthValuesByType[(int)healthGenerator.HealthType] -= healthFixtureInstances[i].HealthGenerator.StartingHealthValue;
                            healthFixtureInstances[i].healthFixture.onDamageEventHandler -= healthFixtureInstances[i].HealthGenerator.Damage;

						}

                        healthFixtureInstances[i].SetGenerator(healthGenerator);

                        totalStartingHealthValuesByType[(int)healthGenerator.HealthType] += healthGenerator.StartingHealthValue;

					}
                    
					// Update the health fixture's activation state
					healthFixtureInstances[i].healthFixture.SetActivation(CheckCanActivate(i));
					OnChangeHealthFixtureActivationState(i, true);
				}
			}

			UpdateHealthData();

		}
		

		/// <summary>
        /// Event called when a health fixture's activation state is changed.
        /// </summary>
        /// <param name="fixtureIndex">The list index of the health fixture.</param>
        /// <param name="isActivated">Whether the health fixture is being activated or deactivated.</param>
		void OnChangeHealthFixtureActivationState(int fixtureIndex, bool isActivated)
		{
			for (int i = 0; i < healthFixtureInstances[fixtureIndex].healthFixture.EmbeddedHealthFixtures.Count; ++i)
			{
				int embeddedIndex = healthFixtureInstances[fixtureIndex].healthFixture.EmbeddedHealthFixtures[i].Index;
				
				// If the outer health fixture is being activated, disable the inner ones
				if (isActivated == true)
				{	
					healthFixtureInstances[embeddedIndex].healthFixture.SetActivation(false);
				} 
				// If the outer health fixture is being deactivated, enable the inner ones (if they can be activated)
				else 
				{
					healthFixtureInstances[embeddedIndex].healthFixture.SetActivation(CheckCanActivate(embeddedIndex));
				}
			}
		}


		/// <summary>
        /// Check if a health fixture can be activated (it may be embedded inside another one that is still active).
        /// </summary>
        /// <param name="fixtureIndex">The list index of the health fixture instance that the health fixture is associated with.</param>
        /// <returns>Whether the health fixture can be activated.</returns>
		bool CheckCanActivate(int fixtureIndex)
		{
			
			// If has no generator or generator destroyed, can't activate
			if (!healthFixtureInstances[fixtureIndex].HasHealthGenerator)
				return false;
			if (healthFixtureInstances[fixtureIndex].HealthGenerator.ModuleState != ModuleState.Activated)
				return false;

			// If this fixture is embedded in an active fixture, cannot activate
			for (int i = 0; i < healthFixtureInstances.Count; ++i)
			{
				if (!healthFixtureInstances[i].healthFixture.IsActivated)
					continue;
				for (int j = 0; j < healthFixtureInstances[i].healthFixture.EmbeddedHealthFixtures.Count; ++j)
				{
					if (healthFixtureInstances[i].healthFixture.EmbeddedHealthFixtures[j].Index == fixtureIndex)
					{
						return false;
					}
				}
			}

			return true;

		}


		/// <summary>
        /// Check if the vehicle should be destroyed (no health left on any of the health generators).
        /// </summary>
        /// <returns>Whether the vehicle should be destroyed.</returns>
		private bool CheckIfDestroyed()
		{

			if (!isDamageable) return false;

			for (int i = 0; i < totalCurrentHealthValuesByType.Count; ++i)
			{
				if (totalCurrentHealthValuesByType[i] > 0.0001f) return false;
			}

			return true;

		}


		/// <summary>
        /// Event called when one of the health generators registers a hit
        /// </summary>
        /// <param name="damage">The amount of damage.</param>
        /// <param name="hitPosition">The world position where the damage occurred.</param>
        /// <param name="attacker">The attacker game agent.</param>
		public void OnDamage(float damage, Vector3 hitPosition, GameAgent attacker)
		{
           
			// Update data
			UpdateHealthData();

            lastAttacker = attacker;

			// Call the event
			if (damageEventHandler != null)
				damageEventHandler(damage, hitPosition, attacker);

			if (CheckIfDestroyed())
			{
                vehicle.SetActivationState(VehicleActivationState.Destroyed);
			}
		}


        /// <summary>
        /// Event called when one of the health generators registers a healing
        /// </summary>
        /// <param name="healingValue">The amount of healing</param>
        /// <param name="interactionPosition">The world position where the healing occurred.</param>
        /// <param name="healer">The healer game agent.</param>
		public void OnHeal(float healingValue, Vector3 interactionPosition, GameAgent healer)
        {

            // Update data
            UpdateHealthData();

            // Call the event
            if (healEventHandler != null)
                healEventHandler(healingValue, interactionPosition, healer);

        }


        /// <summary>
        /// Update the health information that is given by this subsystem on request.
        /// </summary>
        void UpdateHealthData()
		{
			
			// If power is available to the Health subsystem, recharge health generators
			if (vehicle.HasPower && vehicle.Power.GetPowerConfiguration(SubsystemType.Health) != SubsystemPowerConfiguration.Unpowered)
			{

				// First collect the total recharge rates of all the health modules
				float totalGeneratorRechargeRate = 0;
				for (int i = 0; i < healthFixtureInstances.Count; ++i)
				{
					if (healthFixtureInstances[i].HasHealthGenerator && healthFixtureInstances[i].HealthGenerator.ModuleState != ModuleState.Destroyed)
					{
						totalGeneratorRechargeRate += healthFixtureInstances[i].HealthGenerator.HealthRechargeRate;
					}
				}
	
				// Recharge the health generators by splitting the power proportionally to each generator's recharge rate
				float rechargeCoefficient = Mathf.Min(vehicle.Power.GetSubsystemTotalPower(SubsystemType.Health) / totalGeneratorRechargeRate, 1);
				for (int i = 0; i < healthFixtureInstances.Count; ++i)
				{
                    
                    if (healthFixtureInstances[i].HasHealthGenerator && healthFixtureInstances[i].HealthGenerator.ModuleState != ModuleState.Destroyed)
					{
                        healthFixtureInstances[i].HealthGenerator.AddHealth(healthFixtureInstances[i].HealthGenerator.HealthRechargeRate * rechargeCoefficient * Time.deltaTime);
					}
				}
			}
			
			// Clear the cached data on current health
			for (int i = 0; i < totalCurrentHealthValuesByType.Count; ++i)
			{
				totalCurrentHealthValuesByType[i] = 0;
			}


			// Update current health data
			for (int i = 0; i < healthFixtureInstances.Count; ++i)
			{	
				if (healthFixtureInstances[i].HasHealthGenerator && healthFixtureInstances[i].HealthGenerator.ModuleState != ModuleState.Destroyed)
				{
					totalCurrentHealthValuesByType[(int)healthFixtureInstances[i].HealthGenerator.HealthType] += healthFixtureInstances[i].HealthGenerator.CurrentHealthValue;
				}
			}
		}

		
		// Called when a collider on this vehicle collides with another collider
		void OnCollisionEnter(Collision collision)
		{

			// Get the health module that was hit
			HealthFixture collisionHealthFixture = collision.contacts[0].thisCollider.GetComponent<HealthFixture>();
			
			// Call the collision event on the health module
			if (collisionHealthFixture != null)
			{
                
				if (healthFixtureInstances[collisionHealthFixture.Index].HasHealthGenerator)
				{
					float damage = collision.relativeVelocity.magnitude * healthFixtureInstances[collisionHealthFixture.Index].HealthGenerator.CollisionVelocityToDamageCoefficient;

					List<float> damageByType = new List<float>();
					float length = System.Enum.GetValues(typeof(HealthType)).Length;

					for (int i = 0; i < length; ++i)
					{
						damageByType.Add(damage);
					}

					healthFixtureInstances[collisionHealthFixture.Index].healthFixture.Damage(damageByType, collision.contacts[0].point, null);

				}
			}
		}


        /// <summary>
        /// Get the current health fraction (the current health divided by the starting health) for a particular health type.
        /// </summary>
        /// <param name="healthType">The health type being queried.</param>
        /// <returns>The vehicle's current health fraction value for the specified health type.</returns>
        public float GetCurrentHealthFraction(HealthType healthType)
        {
            if (GetStartingHealthValue(healthType) < 0.0001f)
            {
                return 0;
            }
            else
            {
                return (totalCurrentHealthValuesByType[(int)healthType] / totalStartingHealthValuesByType[(int)healthType]);
            }
        }


        /// <summary>
        /// Get the current health value for a particular health type.
        /// </summary>
        /// <param name="healthType">The health type being queried.</param>
        /// <returns>The vehicle's current health value for the specified health type.</returns>
        public float GetCurrentHealthValue(HealthType healthType)
		{
			return (totalCurrentHealthValuesByType[(int)healthType]);

		}


		/// <summary>
        /// Get the starting health value for a particular health type.
        /// </summary>
        /// <param name="healthType">The health type being queried.</param>
        /// <returns></returns>
		public float GetStartingHealthValue (HealthType healthType)
		{
			return (totalStartingHealthValuesByType[(int)healthType]);

		}


		// Called every frame
		void Update()
		{
            frame += 1;

            UpdateHealthData();

			// Check each health generator to see if it is destroyed, so as to deactivate the health fixture
			for (int i = 0; i < healthFixtureInstances.Count; ++i)
			{
				if (healthFixtureInstances[i].healthFixture.IsActivated)
				{
					if (healthFixtureInstances[i].HealthGenerator.ModuleState == ModuleState.Destroyed)
					{
						healthFixtureInstances[i].healthFixture.SetActivation(false);
						OnChangeHealthFixtureActivationState(i, false);
					}
				}
			}
		}
	}
}
