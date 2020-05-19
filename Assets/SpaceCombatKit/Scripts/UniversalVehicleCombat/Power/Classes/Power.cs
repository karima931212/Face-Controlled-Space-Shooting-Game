using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace VSX.UniversalVehicleCombat
{
	
	/// <summary>
    /// The different power configurations that each of the subsystems can be in.
    /// </summary>
	public enum SubsystemPowerConfiguration
	{
		Collective,
		Independent,
		Unpowered
	}


	// This class represents an instance of a powered subsystem - a subsystem that can be configured to store, draw and recharge power
    /// <summary>
    /// This class represents an instance of a powered subsystem, and is used by the Power subsystem class to 
    /// store settings for the different subsystems in the vehicle.
    /// </summary>
	[System.Serializable]
	public class PoweredSubsystem
	{
	
		public SubsystemType type;
	
		public SubsystemPowerConfiguration powerConfiguration;
	
		public float independentPowerOutput;
	
		public float fixedPowerFraction;
		public float defaultDistributablePowerFraction;
		public float distributablePowerFraction;
	
		public float directPowerFraction;
		
		public float rechargePowerFraction;
	
		public float maxRechargeRate;
	
		public float storageCapacity;
		
		public float currentStorageValue;
		
	}
	

	/// <summary>
    /// This class represents the power subsystem of a vehicle. This class redistributes power among the different
    /// powered subsystems in the vehicle from a power plant module loaded onto one of the vehicle's module mounts.
    /// It also allows some or all of the power to be distributed in different ways during gameplay by the player.
    /// </summary>
	public class Power : Subsystem 
	{
		
		[Header ("Output")]
	
		private bool hasPowerPlant = false;
		IPowerPlant mountedPowerPlant;

		public float TotalPower { get { return hasPowerPlant ? mountedPowerPlant.Output : 0f; } }

		private float distributablePowerFraction;

		private float fixedPowerFraction;
		
		
		[Header ("Subsystems")]
		// This list is displayed in a custom inspector
		public List<PoweredSubsystem> poweredSubsystems = new List<PoweredSubsystem>();



		void Awake()
		{

			// Get the fraction of the power that is distributable by the player, and the fraction that is routed in a fixed manner. 
			fixedPowerFraction = 0;
			for (int i = 0; i < poweredSubsystems.Count; ++i)
			{
				if (poweredSubsystems[i].powerConfiguration == SubsystemPowerConfiguration.Collective)
				{
					fixedPowerFraction += poweredSubsystems[i].fixedPowerFraction;
					poweredSubsystems[i].distributablePowerFraction = poweredSubsystems[i].defaultDistributablePowerFraction;
				}
			}
			distributablePowerFraction = 1 - fixedPowerFraction;

			for (int i = 0; i < poweredSubsystems.Count; ++i)
			{
				poweredSubsystems[i].currentStorageValue = poweredSubsystems[i].storageCapacity;
			}

		}


		/// <summary>
        /// Event called when a new module is mounted at a module mount.
        /// </summary>
        /// <param name="moduleMount"></param>
		protected override void OnModuleMounted(ModuleMount moduleMount)
		{

			IModule module = moduleMount.Module();
			IPowerPlant newPowerPlant = module == null ? null : module.CachedTransform.GetComponent<IPowerPlant>();

			if (newPowerPlant != null)
			{ 
				mountedPowerPlant = newPowerPlant; 
				hasPowerPlant = true;
			}
		}


		// Called every frame
		void Update()
		{

            // Add power from the power plant to the different powered subsystems
			for (int i = 0; i < poweredSubsystems.Count; ++i)
			{
				float addValue;
				switch (poweredSubsystems[i].powerConfiguration)
				{
					case SubsystemPowerConfiguration.Collective:
						addValue = poweredSubsystems[i].rechargePowerFraction * TotalPower * Time.deltaTime *
							(poweredSubsystems[i].fixedPowerFraction + poweredSubsystems[i].distributablePowerFraction);

						poweredSubsystems[i].currentStorageValue = Mathf.Clamp(poweredSubsystems[i].currentStorageValue + addValue, 0f, poweredSubsystems[i].storageCapacity);
						break;

					case SubsystemPowerConfiguration.Independent:
						addValue = poweredSubsystems[i].rechargePowerFraction * Time.deltaTime * poweredSubsystems[i].independentPowerOutput;
						poweredSubsystems[i].currentStorageValue = Mathf.Clamp(poweredSubsystems[i].currentStorageValue + addValue, 0f, poweredSubsystems[i].storageCapacity);
						break;
				}
			}
		} 

		
		/// <summary>
        /// Get the power configuration of a particular powered subsystem.
        /// </summary>
        /// <param name="subsystemType">The powered subsystem type.</param>
        /// <returns>The power configuration of the subsystem.</returns>
		public SubsystemPowerConfiguration GetPowerConfiguration(SubsystemType subsystemType)
		{
            return (poweredSubsystems[(int)subsystemType].powerConfiguration);
		}
		

		/// <summary>
        /// Get the total power available for a particular subsystem
        /// </summary>
        /// <param name="subsystem"></param>
        /// <returns></returns>
		public float GetSubsystemTotalPower(SubsystemType subsystem)
		{
			int index = (int)subsystem;

			switch (poweredSubsystems[index].powerConfiguration)
			{

				case SubsystemPowerConfiguration.Collective:
					return ((poweredSubsystems[index].fixedPowerFraction * TotalPower) + (poweredSubsystems[index].distributablePowerFraction * distributablePowerFraction * TotalPower));

				case SubsystemPowerConfiguration.Independent:
					return (poweredSubsystems[index].independentPowerOutput);

				default:
					return 0f;

			}
		}


        /// <summary>
        /// Get the total power that a particular subsystem can access.
        /// </summary>
        /// <param name="subsystemType">The subsystem type.</param>
        /// <returns>The maximum power that this subsystem can access.</returns>
        public float GetSubsystemMaxPossiblePower(SubsystemType subsystemType)
		{

			int index = (int)subsystemType;

			switch (poweredSubsystems[index].powerConfiguration)
			{

				case SubsystemPowerConfiguration.Collective:
					return (poweredSubsystems[index].fixedPowerFraction * TotalPower + distributablePowerFraction * TotalPower);
					
				case SubsystemPowerConfiguration.Independent:
					return (poweredSubsystems[index].independentPowerOutput);

				default:
					return 0f;

			}
		}


        /// <summary>
        /// Get the fixed (non-distributable) power that a subsystem can access.
        /// </summary>
        /// <param name="subsystemType">The subsystem type.</param>
        /// <returns>The fixed power available to the subsystem.</returns>
        public float GetSubsystemFixedPower(SubsystemType subsystemType)
		{

			int index = (int)subsystemType;

			switch (poweredSubsystems[index].powerConfiguration)
			{

				case SubsystemPowerConfiguration.Collective:
					return (poweredSubsystems[index].fixedPowerFraction * TotalPower);
					
				case SubsystemPowerConfiguration.Independent:
					return (poweredSubsystems[index].independentPowerOutput);

				default:
					return 0f;

			}
		}


        /// <summary>
        /// Get the distributable power currently available for a particular subsystem.
        /// </summary>
        /// <param name="subsystemType">The subsystem type.</param>
        /// <returns>The distributable power available to the subsystem.</returns>
        public float GetSubsystemDistributablePower(SubsystemType subsystemType)
		{

			int index = (int)subsystemType;

			switch (poweredSubsystems[index].powerConfiguration)
			{

				case SubsystemPowerConfiguration.Collective:
					return (poweredSubsystems[index].distributablePowerFraction * distributablePowerFraction * TotalPower);
					
				case SubsystemPowerConfiguration.Independent:
					return 0f;

				default:
					return 0f;

			}
		}


        /// <summary>
        /// Set the distributable power for a particular subsystem.
        /// </summary>
        /// <param name="subsystemType">The subsystem type.</param>
        /// <param name="newDistributablePowerFraction">The new distributable power fraction.</param>
        public void SetSubsystemDistributablePowerFraction(SubsystemType subsystemType, float newDistributablePowerFraction)
		{
			
			int index = (int)subsystemType;
			
			if (poweredSubsystems[index].powerConfiguration != SubsystemPowerConfiguration.Collective) return;

			poweredSubsystems[index].distributablePowerFraction = newDistributablePowerFraction;

		}	
	

		/// <summary>
        /// Get the stored power for a given subsystem.
        /// </summary>
        /// <param name="subsystemType">The subsystem type.</param>
        /// <returns>The stored power for the subsystem.</returns>
		public float GetStoredPower(SubsystemType subsystemType)
		{

			int index = (int)subsystemType;
			return poweredSubsystems[index].currentStorageValue;
		}


        /// <summary>
        /// Get the power storage capacity for a given subsystem.
        /// </summary>
        /// <param name="subsystemType">The type of subsystem.</param>
        /// <returns>The power storage capacity for the subsystem.</returns>
        public float GetStorageCapacity(SubsystemType subsystemType)
        {

			int index = (int)subsystemType;
			return poweredSubsystems[index].storageCapacity;
		}


        /// <summary>
        /// Check if there is a given amount of stored power available for a subsystem.
        /// </summary>
        /// <param name="subsystemType">The subsystem type.</param>
        /// <param name="amount">The amount to query for.</param>
        /// <returns>Whether or not there is enough stored power.</returns>
        public bool HasStoredPower(SubsystemType subsystemType, float amount)
		{

			int index = (int)subsystemType;

			return (poweredSubsystems[index].currentStorageValue >= amount);
		}


        /// <summary>
        /// Draw the stored power for a given subsystem.
        /// </summary>
        /// <param name="subsystemType">The type of subsystem.</param>
        /// <param name="amount">The amount of power to draw.</param>
        /// <returns>Whether the power was successfully drawn.</returns>
        public bool DrawStoredPower(SubsystemType subsystemType, float amount)
		{

			int index = (int)subsystemType;

			switch (poweredSubsystems[index].powerConfiguration)
			{

				case SubsystemPowerConfiguration.Unpowered:

					return true;
				

				default:

					if (poweredSubsystems[index].currentStorageValue >= amount)
					{
						
						poweredSubsystems[index].currentStorageValue -= amount;
						return true;
					}
					else
					{
						return false;
					}
			
			}
		}
	}
}
