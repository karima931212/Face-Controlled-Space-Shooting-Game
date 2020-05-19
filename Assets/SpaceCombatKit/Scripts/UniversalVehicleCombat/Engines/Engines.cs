using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;


namespace VSX.UniversalVehicleCombat 
{

	/// <summary> Delegate for attaching functions to the event of a change in the vehicle translation (movement) input values. </summary>
	public delegate void OnEnginesTranslationInputsChangedEventHandler();

	/// <summary> Delegate for attaching functions to the event of a change in the vehicle rotation input values. </summary>
	public delegate void OnEnginesRotationInputsChangedEventHandler();

	/// <summary> Delegate for attaching functions to the event of a change in the vehicle boost input values. </summary>
	public delegate void OnEnginesBoostInputsChangedEventHandler();

	/// <summary> Provides a way to efficiently get/set constraints on the vehicle's physics state. </summary>
	public enum VehiclePhysicsState
	{
		RotationFrozen,
		PositionFrozen,
		PositionAndRotationFrozen,
		Unfrozen
	}

	/// <summary>
	/// This class, which derives from the Subsystem base class, provides a way for an input script to interface with 
    /// all kinds of vehicle engines in a generalised way.
	/// </summary>
	/// <remarks>
	/// The Engines component is a generalised way to interface with many different kinds of vehicle 'controllers'. To do
	/// this, the Engines component passes -1 to 1 values for translation (movement) and rotation (steering), on each 
	/// of the three axes, to a specialized vehicle controller component via the IVehicleController interface. The 
	/// vehicle controller can then implement movement and steering according to its own code, using these values.
	/// The Engines component can also exchange other information with the vehicle controller, such as setting boost 
	/// values, or getting the maximum speed of the vehicle.
	/// </remarks>
	public class Engines : Subsystem 
	{

		private Vehicle vehicle;

		private RigidbodyConstraints constraints;

		private VehiclePhysicsState currentPhysicsState = VehiclePhysicsState.Unfrozen;
		/// <summary> Get the current constraints applied to the vehicle. </summary> 
		public VehiclePhysicsState CurrentPhysicsState { get { return currentPhysicsState; } }

		private bool hasVehicleController;
		public bool HasVehicleController { get { return hasVehicleController; } }

		private IVehicleController vehicleController;
		public IVehicleController VehicleController { get { return vehicleController; } }
		
		private OnEnginesTranslationInputsChangedEventHandler onEnginesTranslationInputsChangedEventHandler;
		/// <summary> Attach functions to be called when the inputs for vehicle translation (movement) are changed. </summary> 
		public event OnEnginesTranslationInputsChangedEventHandler OnEnginesTranslationInputsChangedEventHandler
		{
			add {	onEnginesTranslationInputsChangedEventHandler += value;	} 
			remove {	onEnginesTranslationInputsChangedEventHandler -= value;	}
		}

		private OnEnginesRotationInputsChangedEventHandler onEnginesRotationInputsChangedEventHandler;
		/// <summary> Attach functions to be called when the inputs for vehicle rotation are changed. </summary> 
		public event OnEnginesRotationInputsChangedEventHandler OnEnginesRotationInputsChangedEventHandler
		{
			add {	onEnginesRotationInputsChangedEventHandler += value;	} 
			remove {	onEnginesRotationInputsChangedEventHandler -= value;	}
		}

		private OnEnginesBoostInputsChangedEventHandler onEnginesBoostInputsChangedEventHandler;
		/// <summary> Attach functions to be called when the inputs for vehicle boost are changed. </summary> 
		public event OnEnginesBoostInputsChangedEventHandler OnEnginesBoostInputsChangedEventHandler
		{
			add {	onEnginesBoostInputsChangedEventHandler += value;	} 
			remove {	onEnginesBoostInputsChangedEventHandler -= value;	}
		}



		void Awake()
		{

			vehicle = GetComponent<Vehicle>();

			vehicleController = GetComponent<IVehicleController>();
			hasVehicleController = vehicleController != null;
			if (!hasVehicleController)
			{
				Debug.LogError ("No vehicle controller assigned to the " + name + " transform where the Engines component has been assigned. Please assign one.");
			}
			
		}

			
		/// <summary>
		/// This method allows you to easily lock part of the vehicle's physics, such as during cutscenes 
		/// or immediately after loading the game scene.
		/// </summary>
		/// <param name="newState"> New physics state. </param>
		public void SetPhysicsState(VehiclePhysicsState newState)
		{
			
			switch (newState)
			{
				case VehiclePhysicsState.PositionAndRotationFrozen:

					// Set constraints on rigidbody
					constraints = RigidbodyConstraints.FreezeAll;
					vehicle.CachedRigidbody.constraints = constraints;
		
					// Remove any momentum that the rigidbody might be holding onto
					vehicle.CachedRigidbody.velocity = Vector3.zero;
					vehicle.CachedRigidbody.angularVelocity = Vector3.zero;
		
					currentPhysicsState = VehiclePhysicsState.PositionAndRotationFrozen;

					break;

				case VehiclePhysicsState.Unfrozen:

					// Remove constraints from rigidbody
					constraints = RigidbodyConstraints.None;
					vehicle.CachedRigidbody.constraints = constraints;
			
					currentPhysicsState = VehiclePhysicsState.Unfrozen;
		
					break;

				case VehiclePhysicsState.PositionFrozen:
	
					// Update the rigidbody constraints
					constraints = RigidbodyConstraints.FreezePosition;
					vehicle.CachedRigidbody.constraints = constraints;

					// Remove any velocity
					vehicle.CachedRigidbody.velocity = Vector3.zero;
					
					currentPhysicsState = VehiclePhysicsState.PositionFrozen;
		
					break;

				case VehiclePhysicsState.RotationFrozen:
	
					// Update the rigidbody constraints
					constraints = RigidbodyConstraints.FreezeRotation;
					vehicle.CachedRigidbody.constraints = constraints;

					// Remove any velocity
					vehicle.CachedRigidbody.angularVelocity = Vector3.zero;
					
					currentPhysicsState = VehiclePhysicsState.RotationFrozen;
		
					break;
		
			}
		}


		/// <summary>
		/// Called by input script for directly setting the translation (movement) inputs.
		/// </summary>
		/// <param name="newValuesByAxis"> New translation (movement) input values by axis. </param>
		public void SetTranslationInputs(Vector3 newValuesByAxis)
		{
			
			if (!hasVehicleController) return;

			if (currentPhysicsState == VehiclePhysicsState.PositionFrozen ||
			    currentPhysicsState == VehiclePhysicsState.PositionAndRotationFrozen)
			{
				newValuesByAxis = Vector3.zero;
			}

			vehicleController.SetTranslationInputs(newValuesByAxis);

			if (onEnginesTranslationInputsChangedEventHandler != null)
				onEnginesTranslationInputsChangedEventHandler();

		}


		/// <summary>
		/// Called by input script for directly setting the rotation inputs.
		/// </summary>
		/// <param name="newValuesByAxis"> New rotation input values by axis. </param>
		public void SetRotationInputs(Vector3 newValuesByAxis)
		{
			
			if (!hasVehicleController) return;

			if (currentPhysicsState == VehiclePhysicsState.RotationFrozen ||
			    currentPhysicsState == VehiclePhysicsState.PositionAndRotationFrozen)
			{
				newValuesByAxis = Vector3.zero;
			}

			vehicleController.SetRotationInputs(newValuesByAxis);

			if (onEnginesRotationInputsChangedEventHandler != null)
				onEnginesRotationInputsChangedEventHandler();
			
		}


		/// <summary>
		/// Called by input script for increasing/decreasing translation (movement) throttles
		/// </summary>
		/// <param name="incrementationRatesByAxis"> Incrementation amounts by axis. </param>
		public void IncrementTranslationInputs(Vector3 incrementationAmountsByAxis)
		{

			if (!hasVehicleController) return;

			vehicleController.IncrementTranslationInputs(incrementationAmountsByAxis);

			if (onEnginesTranslationInputsChangedEventHandler != null)
				onEnginesTranslationInputsChangedEventHandler();

		}


		/// <summary>
		/// Called by input script for increasing/decreasing rotation throttles.
		/// </summary>
		/// <param name="incrementationRatesByAxis"> Rotation input incrementation amounts by axis. </param>
		public void IncrementRotationInputs(Vector3 incrementationAmountsByAxis)
		{

			if (!hasVehicleController) return;

			vehicleController.IncrementRotationInputs(incrementationAmountsByAxis);

			if (onEnginesRotationInputsChangedEventHandler != null)
				onEnginesRotationInputsChangedEventHandler();

		}

	
		/// <summary>
		/// Called by input script for setting boost values on each axis.
		/// </summary>
		/// <param name="newValues"> New boost values by axis. </param>
		public void SetBoostInputs(Vector3 newValues)
		{

			if (!hasVehicleController)
				return;

			vehicleController.SetBoostInputs(newValues);

			if (onEnginesBoostInputsChangedEventHandler != null)
			{
				onEnginesBoostInputsChangedEventHandler();
			}
		}


        /// <summary>
        /// Get the maximum speed on each axis, for example for normalizing speed indicators.
        /// </summary>
        /// <param name="withBoost"> Include boost in max. speed calculations. </param>
        public Vector3 GetMaxSpeedByAxis(bool withBoost)
		{

			if (!hasVehicleController) return Vector3.zero;

			return vehicleController.GetMaxSpeedByAxis(withBoost);

		}


		/// <summary>
		/// Get the boost value (0-1) for each translation axis.
		/// </summary>
		public Vector3 GetCurrentBoostValues()
		{
			
			if (!hasVehicleController) return Vector3.zero;

			return vehicleController.CurrentBoostInputs;

		}
	}
}
