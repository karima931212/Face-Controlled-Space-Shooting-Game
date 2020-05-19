using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace VSX.UniversalVehicleCombat
{

    /// <summary>
    /// This class implements engines (movement, steering and other engine-related functionality) for a space vehicle
    /// </summary>
    public class SpaceVehicleEngines : Subsystem, IVehicleController 
	{

        private Vector3 currentTranslationInputs = Vector3.zero;
        public Vector3 CurrentTranslationInputs { get { return currentTranslationInputs; } }


        private Vector3 currentRotationInputs = Vector3.zero;
        public Vector3 CurrentRotationInputs { get { return currentRotationInputs; } }

        private Vector3 currentBoostInputs = Vector3.zero;
        public Vector3 CurrentBoostInputs { get { return currentBoostInputs; } }

        private Vector3 currentAvailableTranslationForces = Vector3.zero;
        private Vector3 currentAvailableRotationForces = Vector3.zero;
        private Vector3 currentAvailableBoostForces = Vector3.zero;
        

        [Header("Defaults")]

        [SerializeField]
        private Vector3 defaultTranslationForces = new Vector3(100f, 100f, 500f);
        public Vector3 DefaultTranslationForces {  get { return defaultTranslationForces; } }

        [SerializeField]
        private Vector3 defaultRotationForces = new Vector3(8f, 8f, 18f);
        public Vector3 DefaultRotationForces { get { return defaultRotationForces; } }

        [SerializeField]
        private Vector3 defaultBoostForces = new Vector3(800f, 800f, 800f);

        [SerializeField]
        private bool applyTranslationForcesDuringBoost = true;


        [Header("Power Coefficients")]

        // Translation power coefficients

        [SerializeField]
        private Vector3 powerToTranslationForceCoefficients = new Vector3(0.2f, 0.2f, 1f);

        // Rotation power coefficients

        [SerializeField]
        private Vector3 powerToRotationForceCoefficients = new Vector3(0.1f, 0.1f, 0.2f);


        [Header("Translation Limits")]

        // Translation limits X axis

        [SerializeField]
        private Vector3 maxTranslationForces = new Vector3(200f, 200f, 800f);
        public Vector3 MaxTranslationForces { get { return maxTranslationForces; } }

        [SerializeField]
        private Vector3 minTranslationInputValues = new Vector3(-1, -1, -0.1f);
        public Vector3 MinTranslationInputValues { get { return minTranslationInputValues; } }

        [SerializeField]
        private Vector3 maxTranslationInputValues = new Vector3(1f, 1f, 1f);
        public Vector3 MaxTranslationInputValues { get { return maxTranslationInputValues; } }


        [Header("Rotation Limits")]

        private Vector3 maxRotationForces = new Vector3(8f, 8f, 18f);
        public Vector3 MaxRotationForces { get { return maxRotationForces; } }

        [Header("Thrust Audio")]
		
		[SerializeField]
		private AudioSource engineAudio;
		bool hasEngineAudio = false;

		[SerializeField]
		private float maxEngineAudioVolume;

		[SerializeField]
		private AudioClip cruisingAudioClip;
		
		[SerializeField]
		private AudioClip burnerAudioClip;


		[Header("Steering Audio")]

		[SerializeField]
		private AudioSource steeringAudio;
		bool hasSteeringAudio = false;

		[SerializeField]
		private float maxSteeringAudioVolume;
		
		[SerializeField]
		private bool physicsDisabled;
		public bool PhysicsDisabled
		{
			get { return physicsDisabled; }
			set { physicsDisabled = value;}
		}

		List <EngineEffectsController> exhaustControllers = new List<EngineEffectsController>();
		
		private Vehicle vehicle;

	
		
		void Awake()
		{
			
			// Get all the exhaust controllers in the ship object hierarchy
			EngineEffectsController[] exhaustControllersArray = transform.GetComponentsInChildren<EngineEffectsController>();
			foreach (EngineEffectsController exhaustController in exhaustControllersArray)
			{
				exhaustControllers.Add(exhaustController);
			}

			// Begin playing engine audio
			hasEngineAudio = engineAudio != null;
			if (hasEngineAudio)
			{ 
				engineAudio.loop = true;
				engineAudio.volume = 0;
				engineAudio.Play();
			}   

			// Begin playing steering audio
			hasSteeringAudio = steeringAudio != null;
			if (hasSteeringAudio)
			{ 
				steeringAudio.loop = true;
				steeringAudio.volume = 0;
				steeringAudio.Play();
			}   

			vehicle = GetComponent<Vehicle>();
		}



        /// <summary>
        /// Get the maximum possible thrust that the engine can deliver
        /// </summary>
        /// <param name="includingBoostThrust">Whether or not to include boost thrust in the calculation.</param>
        /// <returns></returns>
        public float GetMaxPossibleThrust(bool includingBoostThrust)
		{

			float boostValue = includingBoostThrust ? defaultBoostForces.z : 0;
			float result = 0;

			if (vehicle.HasPower && vehicle.Power.GetPowerConfiguration(SubsystemType.Engines) != SubsystemPowerConfiguration.Unpowered)
			{
				result = (vehicle.Power.GetSubsystemMaxPossiblePower(SubsystemType.Engines) * powerToTranslationForceCoefficients.z) + boostValue;
			}
			else
			{
				result = defaultTranslationForces.z + boostValue;
			}

			return (Mathf.Min(result, maxTranslationForces.z));

		}


		/// <summary>
        /// Set the input values for translation of this vehicle.
        /// </summary>
        /// <param name="newValuesByAxis">New translation values (-1 to 1) by axis (x,y,z).</param>
		public void SetTranslationInputs(Vector3 newValuesByAxis)
		{
			
			currentTranslationInputs.x = Mathf.Clamp(newValuesByAxis.x, minTranslationInputValues.x, maxTranslationInputValues.x);
            currentTranslationInputs.y = Mathf.Clamp(newValuesByAxis.y, minTranslationInputValues.y, maxTranslationInputValues.y);
            currentTranslationInputs.z = Mathf.Clamp(newValuesByAxis.z, minTranslationInputValues.z, maxTranslationInputValues.z);	

		}

		
		/// <summary>
        /// Increase/decrease the input values for translation of this vehicle.
        /// </summary>
        /// <param name="incrementationAmountsByAxis">The amount to change the current input values.</param>
		public void IncrementTranslationInputs(Vector3 incrementationAmountsByAxis)
		{

            currentTranslationInputs += incrementationAmountsByAxis;

            currentTranslationInputs.x = Mathf.Clamp(currentTranslationInputs.x, minTranslationInputValues.x, maxTranslationInputValues.x);
            currentTranslationInputs.y = Mathf.Clamp(currentTranslationInputs.y, minTranslationInputValues.y, maxTranslationInputValues.y);
            currentTranslationInputs.z = Mathf.Clamp(currentTranslationInputs.z, minTranslationInputValues.z, maxTranslationInputValues.z);

        }	


		/// <summary>
        /// Set the input values for the rotation/steering of this vehicle.
        /// </summary>
        /// <param name="newValuesByAxis">New rotation input values (-1 to 1) by axis (x,y,z)</param>
		public void SetRotationInputs(Vector3 newValuesByAxis)
		{
            
            // Update the rotation inputs

            currentRotationInputs.x = Mathf.Clamp(newValuesByAxis.x, -1f, 1f);
            currentRotationInputs.y = Mathf.Clamp(newValuesByAxis.y, -1f, 1f);
            currentRotationInputs.z = Mathf.Clamp(newValuesByAxis.z, -1f, 1f);	

			// Steering audio
			if (hasSteeringAudio)
			{
				if (Time.timeScale < 0.0001f)
				{
					steeringAudio.Stop();
				}
				else
				{
					steeringAudio.volume = (currentRotationInputs.magnitude / Vector3.Magnitude(new Vector3(1,1,1))) * maxSteeringAudioVolume;
				}
			}
		}

        /// <summary>
        /// Increment the rotation inputs.
        /// </summary>
        /// <param name="incrementationAmountsByAxis">The amount to increment each of the rotation input values.</param>
		public void IncrementRotationInputs(Vector3 incrementationAmountsByAxis)
		{
			currentRotationInputs += incrementationAmountsByAxis;

            currentRotationInputs.x = Mathf.Clamp(currentRotationInputs.x, -1f, 1f);
            currentRotationInputs.y = Mathf.Clamp(currentRotationInputs.y, -1f, 1f);
            currentRotationInputs.z = Mathf.Clamp(currentRotationInputs.z, -1f, 1f);	

		}

	
		/// <summary>
        /// Called by the input script for setting boost input values.
        /// </summary>
        /// <param name="newValuesByAxis">The new boost input values by axis.</param>
		public void SetBoostInputs (Vector3 newValuesByAxis)
		{
			
			currentBoostInputs.x = Mathf.Clamp(newValuesByAxis.x, -1f, 1f);
            currentBoostInputs.y = Mathf.Clamp(newValuesByAxis.y, -1f, 1f);
            currentBoostInputs.z = Mathf.Clamp(newValuesByAxis.z, -1f, 1f);	
			
			
			if (hasEngineAudio)
			{
				if (currentBoostInputs.z > 0.99f)
				{ 
					engineAudio.clip = burnerAudioClip;
				}
				else
				{
					engineAudio.clip = cruisingAudioClip;
				}

				engineAudio.Play();
			}
		}


		/// <summary>
        /// Get the maximum speed of the vehicle along each of the local axes.
        /// </summary>
        /// <param name="withBoost">Whether to include boost forces in the max speed calculations.</param>
        /// <returns>The maximum speed of the vehicle by axis.</returns>
		public Vector3 GetMaxSpeedByAxis(bool withBoost)
		{

			float maxForwardSpeed = CalculateMaxSpeedFromThrust(GetMaxPossibleThrust(false));

			return (new Vector3(0f, 0f, maxForwardSpeed));

		}


		/// <summary>
        /// Calculate the maximum speed of this Rigidbody for a given force.
        /// </summary>
        /// <param name="force">The linear force to be used in the calculation.</param>
        /// <returns>The maximum speed.</returns>
		float CalculateMaxSpeedFromThrust(float force)
		{
			float delta_V_Thrust = (force / vehicle.CachedRigidbody.mass) * Time.fixedDeltaTime;
			float dragFactor = Time.fixedDeltaTime * vehicle.CachedRigidbody.drag;
			float maxSpeed = delta_V_Thrust / dragFactor;

			return maxSpeed;
		}
	
		
		/// <summary>
        /// Update the forces that are available for movement and steering, based on the power currently available.
        /// </summary>
		void UpdateAvailableForces ()
		{

			// Calculate the current available pitch, yaw and roll torques
			if (vehicle.HasPower && vehicle.Power.GetPowerConfiguration(SubsystemType.Engines) != SubsystemPowerConfiguration.Unpowered)
			{
                currentAvailableRotationForces = vehicle.Power.GetSubsystemTotalPower(SubsystemType.Engines) * powerToRotationForceCoefficients;
			}
			else
			{
                currentAvailableRotationForces = defaultRotationForces;
			}

            // Clamp below maximum limits
            currentAvailableRotationForces.x = Mathf.Min(currentAvailableRotationForces.x, maxRotationForces.x);
            currentAvailableRotationForces.y = Mathf.Min(currentAvailableRotationForces.y, maxRotationForces.y);
            currentAvailableRotationForces.z = Mathf.Min(currentAvailableRotationForces.z, maxRotationForces.z);

			// Calculate the currently available thrust
			if (vehicle.HasPower && vehicle.Power.GetPowerConfiguration(SubsystemType.Engines) != SubsystemPowerConfiguration.Unpowered)
			{
                currentAvailableTranslationForces = vehicle.Power.GetSubsystemTotalPower(SubsystemType.Engines) * powerToTranslationForceCoefficients;
			}
			else
			{
                currentAvailableTranslationForces = defaultTranslationForces;
			}

            // Keep the thrust below the maximum limit
            currentAvailableTranslationForces.x = Mathf.Min(currentAvailableTranslationForces.x, maxTranslationForces.x);
            currentAvailableTranslationForces.y = Mathf.Min(currentAvailableTranslationForces.y, maxTranslationForces.y);
            currentAvailableTranslationForces.z = Mathf.Min(currentAvailableTranslationForces.z, maxTranslationForces.z);

            // Add boost thrust
            currentAvailableBoostForces = Vector3.Scale(currentBoostInputs, defaultBoostForces);
            

		}


        /// <summary>
        /// Enable or disable the exhaust trails.
        /// </summary>
        /// <param name="enable">Whether to enable or disable.</param>
		public void SetExhaustTrailsEnabled(bool enable)
		{
			for (int i = 0; i < exhaustControllers.Count; ++i)
			{
				exhaustControllers[i].SetExhaustTrailsEnabled(enable);
			}
		}

		
		// Called every frame
		void Update()
		{
            
            UpdateAvailableForces();
			
			// Set the engine audio values
			if (hasEngineAudio)
			{
				if (Time.timeScale < 0.0001f)
				{
					engineAudio.volume = 0;
				}
				else
				{
					engineAudio.volume = currentTranslationInputs.magnitude * maxEngineAudioVolume;
				}
			}

			// Update the exhaust effect controllers
			for (int i = 0; i < exhaustControllers.Count; ++i)
			{
				exhaustControllers[i].Set(currentTranslationInputs.z, currentBoostInputs.z);
			}
		}	


		// Apply the physics
		void FixedUpdate()
		{
            if (!physicsDisabled)
			{
				// Implement steering torques
				vehicle.CachedRigidbody.AddRelativeTorque(Vector3.Scale(currentRotationInputs, currentAvailableRotationForces), ForceMode.Acceleration);

                // Add the thrust force
                Vector3 nextTranslationInputs = currentTranslationInputs;

                if (applyTranslationForcesDuringBoost)
                {
                    if (currentBoostInputs.x > 0.5f)
                        nextTranslationInputs.x = Mathf.Clamp(Mathf.Sign(nextTranslationInputs.x), minTranslationInputValues.x, maxTranslationInputValues.x);
                    if (currentBoostInputs.y > 0.5f)
                        nextTranslationInputs.y = Mathf.Clamp(Mathf.Sign(nextTranslationInputs.y), minTranslationInputValues.y, maxTranslationInputValues.y);
                    if (currentBoostInputs.z > 0.5f)
                        nextTranslationInputs.z = Mathf.Clamp(Mathf.Sign(nextTranslationInputs.z), minTranslationInputValues.z, maxTranslationInputValues.z);
                    
                }
                
				vehicle.CachedRigidbody.AddRelativeForce(Vector3.Scale(nextTranslationInputs, currentAvailableTranslationForces) + 
                                                        Vector3.Scale(currentBoostInputs, currentAvailableBoostForces));
			}

		}
	}
}
