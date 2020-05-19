using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement;
using VSX.General;



namespace VSX.UniversalVehicleCombat 
{

	/// <summary>
    /// This class is a weapon controller for a beam weapon of any kind (continuous or pulsed).
    /// </summary>
	public class BeamWeapon : MonoBehaviour, IModule, IWeaponModule, IGunModule, ITriggerable
    {

		[Header("Module")]

		[SerializeField]
		protected string label;
		public string Label { get { return label; } }
		
		public ModuleType ModuleType { get { return ModuleType.GunWeapon; }  }
		
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

		[SerializeField]
		private int defaultTrigger;
        /// <summary>
        /// The default trigger index for this triggerable module.
        /// </summary>
		public int DefaultTrigger { get { return defaultTrigger; } }

        public float ProjectileSpeed { get { return 0f; } }

		[SerializeField]
		private List<float> damageValueByHealthType = new List<float>();
		public List<float> DamageValueByHealthType { get { return damageValueByHealthType; } }

		[SerializeField]
		private float powerDrawPerSecond;

		bool firing = false;
		
		[SerializeField]
        private float aimAssistAngle = 7f;

	
		[Header("Beam Settings")]

		[SerializeField]
		private bool isPulsed = false;

		[SerializeField]
		private List<BeamSpawn> beamSpawns = new List<BeamSpawn>();

		enum BeamState
		{
			WarmingUp,
			Sustaining,
			Decaying,
			WaitingForNext
		}
		private BeamState beamState;

		float beamOnAmount = 0;
		float beamStateStartTime = 0;

		[SerializeField]
		private float beamWarmUpTime;

		[SerializeField]
		private float beamMaxSustainTime;

		[SerializeField]
		private float beamDecayTime;

		[SerializeField]
		private float beamPauseTime;
		
		[SerializeField]
		private float beamRange;

		[SerializeField]
		private AudioSource audioSource;

		[SerializeField]
		private float beamVolume;
		
		private List<FlashController> flashControllers = new List<FlashController>();

        private OnFireOnceEventHandler onFireOnceEventHandler;
        /// <summary>
        /// Delegate for running functions when this triggerable is fired once.
        /// </summary>
        public event OnFireOnceEventHandler OnFireOnceEventHandler
		{
			add {	onFireOnceEventHandler += value;	} 
			remove {	onFireOnceEventHandler -= value;	}
		}

        private OnSetFireLevelEventHandler onSetFireLevelEventHandler;
        /// <summary>
        /// Delegate for running functions when this triggerable's fire level is set.
        /// </summary>
        public event OnSetFireLevelEventHandler OnSetFireLevelEventHandler
		{
			add {	onSetFireLevelEventHandler += value;	} 
			remove {	onSetFireLevelEventHandler -= value;	}
		}

        private OnSetAimAssistEventHandler onSetAimAssistEventHandler;
        /// <summary>
        /// Delegate for running functions when this weapon's aim assist is set.
        /// </summary>
        public event OnSetAimAssistEventHandler OnSetAimAssistEventHandler
		{
			add {	onSetAimAssistEventHandler += value;	} 
			remove {	onSetAimAssistEventHandler -= value;	}
		}

		private OnClearAimAssistEventHandler onClearAimAssistEventHandler;
        /// <summary>
        /// Delegate for running functions when the aim assist is cleared/removed.
        /// </summary>
       	public event OnClearAimAssistEventHandler OnClearAimAssistEventHandler
		{
			add {	onClearAimAssistEventHandler += value;	} 
			remove {	onClearAimAssistEventHandler -= value;	}
		}



		void Reset()
		{
			// Make sure the damageValueByHealthType list has the same number of items as the HealthType enum
			StaticFunctions.ResizeList(damageValueByHealthType, Enum.GetNames(typeof(HealthType)).Length);
		}

		void OnValidate()
		{
			// Make sure the damageValueByHealthType list has the same number of items as the HealthType enum
			StaticFunctions.ResizeList(damageValueByHealthType, Enum.GetNames(typeof(HealthType)).Length);
		}
	

		void Awake()
		{
			cachedTransform = transform;
			cachedGameObject = gameObject;

			flashControllers = new List<FlashController>(transform.GetComponentsInChildren<FlashController>());
		}

	
		void Start()
		{	
			if (PoolManager.Instance != null)
			{
			
				if (beamSpawns.Count == 0)
				{
					Debug.LogWarning("The " + name + " GunModule has no spawn points assigned, cannot fire");
				}
	
				beamState = BeamState.WaitingForNext;
				
			}
			else
			{
				Debug.LogWarning("No pool manager instance found, cannot create gun module");
				
			}
		}


		/// <summary>
        /// Set a new module state for this module.
        /// </summary>
        /// <param name="newState">The new module state for this module.</param>
		public void SetModuleState(ModuleState newModuleState)
		{
            moduleState = newModuleState;
		}


		/// <summary>
        /// Called when this module is mounted on a module mount.
        /// </summary>
        /// <param name="moduleMount">The module mount where this module is mounted.</param>
		public virtual void OnMount(ModuleMount moduleMount)
		{
			
			this.moduleMount = moduleMount;
			
			cachedTransform.SetParent(moduleMount.CachedTransform);
			cachedTransform.localPosition = Vector3.zero;
			cachedTransform.localRotation = Quaternion.identity;
			
			cachedGameObject.SetActive(true);
			
		}
			
	
		/// <summary>
        /// Called when this module is unmounted from the module mount where it was mounted.
        /// </summary>
		public virtual void OnUnmount()
		{
			this.moduleMount = null;
			cachedGameObject.SetActive(false);
		}
	

		// Called when the beam changes state.
        void OnBeamStateChange(BeamState newState)
		{

			beamState = newState;
			beamStateStartTime = Time.time;

			if (beamState == BeamState.WarmingUp)
			{ 
				audioSource.Play();
			}
			if (beamState == BeamState.WaitingForNext)
			{
				SetFireLevel(0);
			}
		}	


        /// <summary>
        /// Reset the module to starting conditions
        /// </summary>
		public void ResetModule()
		{
			SetModuleState(ModuleState.Activated);
		}


		/// <summary>
        /// Start triggering this triggerable.
        /// </summary>
		public void StartTriggering()
		{
			firing = true;
		}

	
		/// <summary>
        /// Stop triggering this triggerable.
        /// </summary>
		public void StopTriggering()
		{
			firing = false;
		}

        /// <summary>
        /// Fire this weapon once (not applicable to beam weapons).
        /// </summary>
        /// <param name="doDamage">Whether the weapon should cause damage.</param>
		public void FireOnce(bool doDamage = false)
        {
        }


        /// <summary>
        /// Set the fire level of this weapon.
        /// </summary>
        /// <param name="fireLevel">The fire level to be set.</param>
        /// <param name="doDamage">Whether the weapon should cause damage.</param>
		public void SetFireLevel(float fireLevel, bool doDamage = false)
        {

            for (int i = 0; i < beamSpawns.Count; ++i)
            {

                // Set the beam
                bool hasHit = false;

                // Do raycast
                RaycastHit hit;
                if (beamOnAmount > 0.00001f && Physics.Raycast(beamSpawns[i].transform.position, beamSpawns[i].transform.forward, out hit, beamRange))
                {
                    hasHit = true;

                    float beamLength = Vector3.Distance(hit.point, beamSpawns[i].transform.position);
                    beamSpawns[i].SetBeam(beamLength, hit.normal, hasHit, beamOnAmount);

                    // If the other object is a shield, damage it
                    IDamageable damageable = hit.collider.GetComponent<IDamageable>();

                    if (!doDamage && damageable != null)
                    {
                        List<float> thisFrameDamageByHealthType = new List<float>();
                        for (int j = 0; j < damageValueByHealthType.Count; ++j)
                        {
                            thisFrameDamageByHealthType.Add(damageValueByHealthType[j] * Time.deltaTime);
                        }
                        damageable.Damage(thisFrameDamageByHealthType, hit.point, moduleMount.Vehicle.Agent);
                    }
                }
                else
                {
                    beamSpawns[i].SetBeam(beamRange, -beamSpawns[i].CachedTransform.forward, false, beamOnAmount);
                }
            }

            for (int i = 0; i < flashControllers.Count; ++i)
            {
                flashControllers[i].SetLevel(beamOnAmount);
            }

            if (onSetFireLevelEventHandler != null) onSetFireLevelEventHandler(beamOnAmount);
        }


        /// <summary>
        /// Attempt an aim assist toward a position in space (snap to point there if the angle to that position is within aim assist angle)
        /// </summary>
        /// <param name="aimAssistTargetPosition">The world space aim assist target position.</param>
        public void TryAimAssist(Vector3 aimAssistTargetPosition)
		{
			
			// If there is a target and it is within the aim assist angle, aim the weapons, otherwise center the weapons
			float angleToTarget = Vector3.Angle(cachedTransform.forward, aimAssistTargetPosition - cachedTransform.position);
			if (angleToTarget <= aimAssistAngle)
			{
				SetAimAssist(aimAssistTargetPosition);
			}
			else
			{
				ClearAimAssist();
			}
		}


        /// <summary>
        /// Force an aim assist toward a position in space.
        /// </summary>
        /// <param name="aimAssistTargetPosition">The world space aim assist target position.</param>
		public void SetAimAssist(Vector3 aimAssistTargetPosition)
		{
			for (int i = 0; i < beamSpawns.Count; ++i)
			{
				// Aim the shot spawn transform at the target lead position
				beamSpawns[i].CachedTransform.LookAt(aimAssistTargetPosition);
			}

			if (onSetAimAssistEventHandler != null)
				onSetAimAssistEventHandler(aimAssistTargetPosition);
		}


		/// <summary>
        /// Clear aim assist from this weapon (reset the spawn point rotation).
        /// </summary>
		public void ClearAimAssist()
		{
			// Center the weapons
			for (int i = 0; i < beamSpawns.Count; ++i)
			{
				beamSpawns[i].CachedTransform.localRotation = Quaternion.identity;
			}

			if (onClearAimAssistEventHandler != null)
				onClearAimAssistEventHandler();
		}


		/// <summary>
        /// Check if the weapon should be fired and attempt to do it if true.
        /// </summary>
        /// <returns>Whether the weapon successfully fired.</returns>
		private bool TryFire()
		{

			if (!firing)
				return false;

			float powerDraw = 0;

            if (isPulsed)
            {
                // Get the power draw for the full pulse
                powerDraw = powerDrawPerSecond * beamSpawns.Count * (beamWarmUpTime + beamMaxSustainTime + beamWarmUpTime);
            }
            else
            {
                // Get the power draw for a single frame 
                powerDraw = powerDrawPerSecond * beamSpawns.Count * Time.deltaTime;
            }
			
			
			// If non-zero power is drawn and not enough power is available, cannot fire
			if (!Mathf.Approximately(powerDraw, 0) && (!moduleMount.Vehicle.HasPower || !moduleMount.Vehicle.Power.HasStoredPower(SubsystemType.Weapons, powerDraw)))
			{
				return false;
			}

			if (!Mathf.Approximately(powerDraw, 0) && moduleMount.Vehicle.HasPower)
			{
				moduleMount.Vehicle.Power.DrawStoredPower(SubsystemType.Weapons, powerDraw);
			}

			return true;

		}


		void Update()
		{
			
			// Update the beam life cycle
			switch (beamState)
			{
				case BeamState.WarmingUp:
			
					beamOnAmount += (1 / beamWarmUpTime) * Time.deltaTime;
					
					// Handle stopped firing when using non-pulsed beam and firing conditions are not met
					if (!isPulsed && !TryFire())
					{
						OnBeamStateChange(BeamState.Decaying);
					}
					// Transition to sustain when it has fully warmed up	
					else if (beamOnAmount >= 1)	
					{
						beamOnAmount = 1;
						OnBeamStateChange(BeamState.Sustaining);
					}

					break;
	

				case BeamState.Decaying:

					beamOnAmount -= (1 / beamDecayTime) * Time.deltaTime;
					
					// Handle transition to waiting
					if (beamOnAmount <= 0)
					{
						beamOnAmount = 0;
						OnBeamStateChange(BeamState.WaitingForNext);
					}
					
					break;

				case BeamState.Sustaining:

					// Handle transition to decaying due to stopped firing, or finished maximum sustain period
					bool finishedPulse = Time.time - beamStateStartTime >= beamMaxSustainTime;
					if ((!isPulsed && !TryFire()) || (isPulsed && finishedPulse))
					{ 
						OnBeamStateChange(BeamState.Decaying);
					}

					break;


				case BeamState.WaitingForNext:

					// Handle transition to firing
					bool finishedPause = Time.time - beamStateStartTime > beamPauseTime;
					if (!isPulsed || finishedPause)
					{
                        if (TryFire())
                        {
                            OnBeamStateChange(BeamState.WarmingUp);
                        }
					}
					
					
					break;

			}

			SetFireLevel(beamOnAmount);
			
			// Update the sound
			if (!isPulsed && audioSource != null)
			{
				audioSource.volume = beamOnAmount * beamVolume;
			}
		}  
	}
}

