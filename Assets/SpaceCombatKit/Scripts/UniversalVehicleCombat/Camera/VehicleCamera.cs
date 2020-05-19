using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using VSX.UniversalVehicleCombat;

namespace VSX.UniversalVehicleCombat{


	/// <summary>
    /// The different camera views that can be selected.
    /// </summary>
	public enum VehicleCameraView
	{
        None,
		Interior,
		Exterior
	}

    /// <summary>
    /// The different camera modes that can be selected.
    /// </summary>
	public enum VehicleCameraMode
    {
        Active,
        Passive,
        Death
    }


    /// <summary>
    /// This class is used to store information about an instance of a camera shake sequence.
    /// </summary>
    public class CameraShake
	{

		public float strength;

		public float attackTime;
		public float sustainTime;
		public float decayTime;

		public float startTime;

		public CameraShake(float strength, float attackTime, float sustainTime, float decayTime)
		{

			this.strength = strength;

			this.attackTime = attackTime;
			this.sustainTime = sustainTime;
			this.decayTime = decayTime;

			this.startTime = Time.time;
		}
			
	}

	/// <summary>
    /// This class manages the vehicle camera in the game.
    /// </summary>
	public class VehicleCamera : MonoBehaviour 
	{

		private Vehicle targetVehicle;
		private bool hasTargetVehicle;

		Transform cachedTransform;
		public Transform CachedTransform { get { return cachedTransform; } }

		[Header("General")]
		
		[SerializeField]
		private VehicleCameraView defaultView;
		public VehicleCameraView DefaultView { get { return defaultView; } }

        private CameraViewTarget currentViewTarget;
        private bool hasViewTarget;
        public VehicleCameraView CurrentView
        {
            get { return hasViewTarget ? currentViewTarget.CameraView : VehicleCameraView.None; }
        }

        private VehicleCameraMode currentMode = VehicleCameraMode.Active;

        [SerializeField]
		private Camera chaseCamera;
		public Camera ChaseCamera{ get { return chaseCamera; } }

		[SerializeField]
		private Camera hudCamera;
		public Camera HUDCamera { get { return hudCamera; } }

		[SerializeField]
		private GimbalController lookController;
        public GimbalController LookController { get { return lookController; } }

        private bool hasLookController;
        public bool HasLookController {  get { return hasLookController; } }

		[SerializeField]
		private FlybyParticleController flybyParticleController;
		
		[Header("Boost Mode Effects")]

		[SerializeField]
		private float boostShakeStrength = 0.0085f;

		[SerializeField]
		private float boostFOV = 75f;

		[SerializeField]
		private float boostFOVLerpSpeed = 0.05f;

		[Header ("Death Sequence")]
		
		[SerializeField]
		private Vector3 deathOffset;

		[SerializeField]
		private float deathOrbitSpeed;

		[SerializeField]
		private float deathShakeStrength;

		[SerializeField]
		private float deathShakeAttackTime;

		[SerializeField]
		private float deathShakeSustainTime;

		[SerializeField]
		private float deathShakeFadeTime;

	
		private Vector3 deathLookPosition;

		private float startFOV;
		
		float currentSingleShakeStrength = 0;
		List<CameraShake> cameraShakes = new List<CameraShake>();

		/// <summary>
        /// A delegate for attaching event functions to run when the camera has finished upating its position/rotation etc
        /// </summary>
		public delegate void OnCameraUpdated();
		public OnCameraUpdated onCameraUpdatedEventHandler;

		private bool disabled = false;
        /// <summary>
        /// Disable the vehicle camera (allow the position/rotation to be updated externally)
        /// </summary>
		public bool Disabled 
		{
			get { return disabled; }
			set { disabled = value; }
		}



		void Awake()
		{

			cachedTransform = transform;
            hasLookController = lookController != null;

			// Initialize the fov
			startFOV = chaseCamera.fieldOfView;
			
			// Keep track of the focused vehicle
			UVCEventManager.Instance.StartListening(UVCEventType.OnFocusedVehicleChanged, OnFocusedVehicleChanged);
			UVCEventManager.Instance.StartListening(UVCEventType.OnVehicleDestroyed, OnVehicleDestroyed);	
			
		}		

		

		void Start()
		{
            if (hasTargetVehicle)
            {
                SetView(defaultView);
            }
		}


		/// <summary>
        /// Event called when the scene-focused vehicle changes.
        /// </summary>
        /// <param name="newVehicle">The new focused vehicle.</param>
		void OnFocusedVehicleChanged(Vehicle newVehicle)
		{
			
			bool changed = newVehicle != targetVehicle;
			targetVehicle = newVehicle;
			hasTargetVehicle = targetVehicle != null;
			
			if (hasTargetVehicle)
			{ 
				if (changed)
				{
					SetView(defaultView);
	
					// Disable the trail renderers as they get in the way in chase mode
					SpaceVehicleEngines spaceVehicleEngines = targetVehicle.GetComponent<SpaceVehicleEngines>();
					if (spaceVehicleEngines != null)
					{
						spaceVehicleEngines.SetExhaustTrailsEnabled(false);
					}
				}
			}
			else
			{
                currentViewTarget = null;
                hasViewTarget = false;
			}
		}

		
		/// <summary>
        /// Add a new camera shake sequence.
        /// </summary>
        /// <param name="strength">The shake strength.</param>
        /// <param name="attackTime">How fast the camera shake builds up.</param>
        /// <param name="sustainTime">How long the shake sustains at maximum strength.</param>
        /// <param name="decayTime">How long the shake takes to decay/disappear.</param>
		public void AddNewCameraShake(float strength, float attackTime, float sustainTime, float decayTime)
		{

			CameraShake newShake = new CameraShake(strength, attackTime, sustainTime, decayTime);
			cameraShakes.Add(newShake);

		}


		/// <summary>
        /// Set a new camera view.
        /// </summary>
        /// <param name="newView">The new camera view.</param>
		public void SetView(VehicleCameraView newView)
		{

            if (!hasTargetVehicle) return;

            // Reset the look controller
			if (hasLookController) lookController.SetGimbalRotation(Quaternion.identity, Quaternion.identity);

            // Clear the camera view information
            currentViewTarget = null;
			hasViewTarget = false;
			
            // Search all camera views on vehicle for desired view
			for (int i = 0; i < targetVehicle.CameraViewTargets.Count; ++i)
			{
				if (targetVehicle.CameraViewTargets[i].CameraView == newView)
				{
					currentViewTarget = targetVehicle.CameraViewTargets[i];
					hasViewTarget = true;

                    chaseCamera.transform.localPosition = Vector3.zero;
				}
			}

            // If none found, default to the first available
            if (!hasViewTarget)
            {
                if (targetVehicle.CameraViewTargets.Count > 0)
                {
                    currentViewTarget = targetVehicle.CameraViewTargets[0];
                    hasViewTarget = true;

                    chaseCamera.transform.localPosition = Vector3.zero;

                    Debug.LogWarning("No CameraViewTarget found for VehicleCameraView " + newView.ToString() + ". Defaulting to " + 
                        currentViewTarget.CameraView.ToString());
                }
                else
                {
                    Debug.LogWarning("No CameraViewTarget found on vehicle");
                }
            }		

			UVCEventManager.Instance.TriggerCameraViewEvent(UVCEventType.OnCameraViewChanged, newView);
			
		}


        /// <summary>
        /// Set a new camera mode for the vehicle camera.
        /// </summary>
        /// <param name="newMode">The new mode for the vehicle camera</param>
        public void SetCameraMode(VehicleCameraMode newMode)
        {

            // Reset the look controller
            if (hasLookController) lookController.SetGimbalRotation(Quaternion.identity, Quaternion.identity);
            
            currentMode = newMode;

            // Make sure the camera rotation is reset for death mode
            if (currentMode == VehicleCameraMode.Death)
            {
                cachedTransform.rotation = Quaternion.identity;
            }

            UVCEventManager.Instance.TriggerCameraModeEvent(UVCEventType.OnCameraModeChanged, newMode);
        }

		
        /// <summary>
        /// Event called when a vehicle in the game is destroyed.
        /// </summary>
        /// <param name="destroyedVehicle">The vehicle that has been destroyed</param>
        /// <param name="attacker">The attacker that destroyed the vehicle.</param>
		private void OnVehicleDestroyed(Vehicle destroyedVehicle, GameAgent attacker)
		{
			if (hasTargetVehicle && targetVehicle == destroyedVehicle)
			{

				deathLookPosition = destroyedVehicle.CachedTransform.position;

				AddNewCameraShake(deathShakeStrength, deathShakeAttackTime, deathShakeSustainTime, deathShakeFadeTime);
                cachedTransform.position = deathLookPosition + deathOffset;
				SetCameraMode(VehicleCameraMode.Death);
			}
		}


        // Physics update
       	void FixedUpdate()
		{

			if (disabled || !hasViewTarget)
				return;
			
			// Exit if has no follow target
			if (currentMode == VehicleCameraMode.Death)
			{
				cachedTransform.RotateAround(deathLookPosition, Vector3.up, deathOrbitSpeed * Time.fixedDeltaTime);
				cachedTransform.LookAt(deathLookPosition);
			}
			else
			{
				if (hasViewTarget)
				{

					float spinLateralOffset;
				    Vector3 desiredPosition;
                    Vector3 spin = hasTargetVehicle ? targetVehicle.CachedRigidbody.angularVelocity : Vector3.zero;


                    spinLateralOffset = currentViewTarget.SpinOffsetCorefficient * 
                                        currentViewTarget.CachedTransform.InverseTransformDirection(spin).z;
                    		
					desiredPosition = currentViewTarget.CachedTransform.TransformPoint(new Vector3(-spinLateralOffset, 0f, 0f));

					cachedTransform.position = (1 - currentViewTarget.PositionFollowStrength) * cachedTransform.position + currentViewTarget.PositionFollowStrength * desiredPosition;
		
					cachedTransform.rotation = Quaternion.Slerp(cachedTransform.rotation, currentViewTarget.CachedTransform.rotation, currentViewTarget.RotationFollowStrength);
					cachedTransform.rotation = Quaternion.LookRotation(currentViewTarget.CachedTransform.forward, cachedTransform.up);
				}
			}
			if (onCameraUpdatedEventHandler != null) onCameraUpdatedEventHandler();
		}


        
		void LateUpdate()
		{
			if (disabled)
				return;

            // Positioning of the locked interior camera must be done in late update to make sure that there is no position lag, so that the 
            // aiming reticle lines up with the forward vector			
			if (hasViewTarget)
			{
                if (currentViewTarget.LockPosition)
                {
                    cachedTransform.position = currentViewTarget.CachedTransform.position;
                }
                if (currentViewTarget.LockRotation)
                {
                    cachedTransform.rotation = currentViewTarget.CachedTransform.rotation;
                }
            }

			if (onCameraUpdatedEventHandler != null) onCameraUpdatedEventHandler();
		}


        // Called every frame
		void Update()
		{

			if (disabled)
				return;

			// Do camera effects
			if (hasTargetVehicle)
			{
				
				flybyParticleController.UpdateEffect(targetVehicle.CachedRigidbody.velocity);

				if (targetVehicle.HasEngines)
				{
					// If boost is on, shake camera and change the FOV
					currentSingleShakeStrength = Mathf.Max(currentSingleShakeStrength, targetVehicle.Engines.GetCurrentBoostValues().z * boostShakeStrength);
				

					float targetFOV = targetVehicle.Engines.GetCurrentBoostValues().z * boostFOV + (1 - targetVehicle.Engines.GetCurrentBoostValues().z) * startFOV;
					chaseCamera.fieldOfView = Mathf.Lerp(chaseCamera.fieldOfView, targetFOV, boostFOVLerpSpeed);	
				}
	
				// FOV should only affect the HUD when in the interior modes
				if (CurrentView == VehicleCameraView.Interior)
				{
					hudCamera.fieldOfView = chaseCamera.fieldOfView;
				}
				else
				{	
					hudCamera.fieldOfView = startFOV;
				}
				
				float maxShakeStrength = 0;
				for (int i = 0; i < cameraShakes.Count; ++i)
				{
					
					float timeSinceStart = Time.time - cameraShakes[i].startTime;
					float currentStrengthAmount = 0;
					
					if (timeSinceStart < cameraShakes[i].attackTime)
					{
						currentStrengthAmount = timeSinceStart / cameraShakes[i].attackTime;
					}
					else if (timeSinceStart < cameraShakes[i].attackTime + cameraShakes[i].sustainTime)
					{
						currentStrengthAmount = 1;
					}
					else
					{
						float timeSinceBeganDecay = timeSinceStart - cameraShakes[i].attackTime - cameraShakes[i].sustainTime;
						currentStrengthAmount = Mathf.Clamp(1 - timeSinceBeganDecay / cameraShakes[i].decayTime, 0f, 1f);
					}
					maxShakeStrength = Mathf.Max(maxShakeStrength, currentStrengthAmount * cameraShakes[i].strength);
				}
				
				maxShakeStrength = Mathf.Max(maxShakeStrength, currentSingleShakeStrength);
				
				// Do a single shake
				if (maxShakeStrength > 0f)
				{
					
					// Get a random vector on the xy plane
					Vector3 shakeVector = new Vector3(UnityEngine.Random.Range (-1, 1), UnityEngine.Random.Range (-1, 1), 0f).normalized;
			
					// Scale according to desired shake magnitude
					shakeVector *= maxShakeStrength;
	
					// Look at shake vector
					Vector3 lookToVector = cachedTransform.forward + shakeVector;
					cachedTransform.rotation = Quaternion.LookRotation (lookToVector, cachedTransform.up);

				}
				currentSingleShakeStrength = 0;
			}
		}
	}
}
