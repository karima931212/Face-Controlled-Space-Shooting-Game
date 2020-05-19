using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using VSX.General;

namespace VSX.UniversalVehicleCombat
{

	/// <summary>
    /// This class provides a base class for missile weapons (weapons that fire missiles, not missiles themselves).
    /// </summary>
	public class MissileWeapon : MonoBehaviour, IModule, IWeaponModule, IMissileModule, IUnitResourceConsumer, ITriggerable 
	{
	
		[Header("Module")]

		[SerializeField]
		protected string label;
		public string Label { get { return label; } }
		
		protected ModuleType moduleType = ModuleType.MissileWeapon;
		public ModuleType ModuleType { get { return moduleType; }  }
		
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
		protected bool hasModuleMount = false;

		[SerializeField]
		protected int defaultTrigger;
		public int DefaultTrigger { get { return defaultTrigger; } }

		public List<float> DamageValueByHealthType { get { return missilePrefab.DamageValuesByHealthType; } }

		public float MaxSpeed { get { return missilePrefab.Speed; } }
	
		public float Agility { get { return ((missilePrefab.MaxTurningTorques.x + missilePrefab.MaxTurningTorques.y)/2); } }

		public float MaxLockingAngle { get { return missilePrefab.MaxLockAngle; } }

		public float MaxLockingRange { get { return missilePrefab.LockRange; } }

		
		[Header("Missile Weapon Parameters")]

		[SerializeField]
		protected bool lockWithCamera;
		public bool LockWithCamera { get { return lockWithCamera; } }

		[SerializeField]
		protected float lockingPeriod;
		public float LockingPeriod { get { return lockingPeriod; } }

		private LockState lockState;
		public LockState LockState { get { return lockState; } }


		// Projectile
		[SerializeField]
		private bool infiniteResourceUnits = true;
		public bool InfiniteResourceUnits
		{ 
			get { return infiniteResourceUnits; }
			set { infiniteResourceUnits = value; }
		}

		[SerializeField]
		protected int startingResourceUnits;
		public int StartingResourceUnits { get { return startingResourceUnits; } }

		protected int currentResourceUnits;
		public int CurrentResourceUnits { get { return currentResourceUnits; } }

		[SerializeField]
		private float powerDraw; // Per shot or per second

		// Private stuff

		[SerializeField]
		private MissileController missilePrefab;
		private GameObject missilePrefabGameObject;

	    
		[SerializeField]
		protected bool continuousFire;
		protected bool preventContinuousFireCheck = false;
	
		[SerializeField]
		protected List<Transform> spawnPoints = new List<Transform>();
		
		protected MeshRenderer displayUnit;
	
		[SerializeField]
		protected float fireRate;

		protected float lastFire = -100;
	
	    // Audio

		[SerializeField]
		private AudioSource launchAudioSource;
	   
		
	    bool firing = false;

        /// <summary>
        /// Delegate for running functions when this triggerable is fired once.
        /// </summary>
		private OnFireOnceEventHandler onFireOnceEventHandler;
		public event OnFireOnceEventHandler OnFireOnceEventHandler
		{
			add {	onFireOnceEventHandler += value;	} 
			remove {	onFireOnceEventHandler -= value;	}
		}

        /// <summary>
        /// Delegate for running functions when this triggerable's fire level is set.
        /// </summary>
		private OnSetFireLevelEventHandler onSetFireLevelEventHandler;
       	public event OnSetFireLevelEventHandler OnSetFireLevelEventHandler
		{
			add {	onSetFireLevelEventHandler += value;	} 
			remove {	onSetFireLevelEventHandler -= value;	}
		}
	

	
	    void Awake()
	    {
	
			cachedTransform = transform;
			cachedGameObject = gameObject;

			missilePrefabGameObject = missilePrefab.gameObject;
			
	    }


        /// <summary>
        /// Reset the module to starting conditions.
        /// </summary>
		public void ResetModule()
		{
			SetModuleState(ModuleState.Activated);
            currentResourceUnits = startingResourceUnits;
        }


        // Called when the gameobject is toggled
		void OnEnable()
		{
			ResetModule();
		}
	
		
        /// <summary>
        /// Set a new module state for this module
        /// </summary>
        /// <param name="newState">The new module state</param>
		public void SetModuleState(ModuleState newModuleState)
		{
            moduleState = newModuleState;

        }

		
        /// <summary>
        /// Event called when this module is mounted onto a vehicle's module mount.
        /// </summary>
        /// <param name="moduleMount">The module mount where this module is mounted.</param>
		public virtual void OnMount(ModuleMount moduleMount)
		{
			
			this.moduleMount = moduleMount;
			hasModuleMount = moduleMount == null;
			
			cachedTransform.SetParent(moduleMount.CachedTransform);
			cachedTransform.localPosition = Vector3.zero;
			cachedTransform.localRotation = Quaternion.identity;
			
			cachedGameObject.SetActive(true);

		}
			
		
        /// <summary>
        /// Event called when this module is unmounted from a vehicle's module mount.
        /// </summary>
		public virtual void OnUnmount()
		{
			this.moduleMount = null;
			cachedGameObject.SetActive(false);
		}

	
		/// <summary>
        /// Set the lock state of this missile weapon (set by the weapon computer, through the Weapon subsystem component).
        /// </summary>
        /// <param name="newLockState">The new lock state of this missile weapon.</param>
		public virtual void SetLockState (LockState newLockState)
		{
			lockState = newLockState;
		}


		/// <summary>
        /// (ITriggerable) Start triggering this triggerable.
        /// </summary>
		public virtual void StartTriggering()
		{
			
			if (!continuousFire)
			{ 
				if (!preventContinuousFireCheck)
				{
					preventContinuousFireCheck = true;
					firing = true;
				}
			}
			else
			{
				firing = true;
			}
	    }
	
		
		/// <summary>
        /// (ITriggerable) stop triggering this triggerable
        /// </summary>
		public virtual void StopTriggering()
		{
	        firing = false;
			preventContinuousFireCheck = false;
		}


        /// <summary>
        /// (ITriggerable) Fire this triggerable once.
        /// </summary>
        /// <param name="doDamage">Whether the weapon should cause damage.</param>
		public void FireOnce(bool doDamage = false)
		{
			
			foreach (Transform spawnPoint in spawnPoints)
			{
				
				if (hasModuleMount && moduleMount.Vehicle.HasPower && !moduleMount.Vehicle.Power.HasStoredPower(SubsystemType.Weapons, powerDraw))
					break;

				currentResourceUnits = Mathf.Max(currentResourceUnits - 1, 0);
                
				GameObject missile = PoolManager.Instance.Get(missilePrefabGameObject, spawnPoint.position, spawnPoint.rotation);
				MissileController missileController = missile.GetComponent<MissileController>();
	
				bool isLocked = lockState == LockState.Locked;
				
				missileController.SetMissileParameters(moduleMount.Vehicle.Agent, moduleMount.Vehicle.CachedGameObject, moduleMount.Vehicle.CachedRigidbody.velocity, 
														isLocked ? moduleMount.Vehicle.Radar.SelectedTarget : null);

				missileController.DamageEnabled = !doDamage;

				if (launchAudioSource != null) launchAudioSource.Play();

				lastFire = Time.time;

            }

			if (onFireOnceEventHandler != null) onFireOnceEventHandler();

		}


        /// <summary>
        /// (ITriggerable) Set the fire level of this triggerable (not applicable for missile weapons).
        /// </summary>
        /// <param name="fireLevel">The new fire level.</param>
        /// <param name="doDamage">Whether the weapon should do damage when firing.</param>
        public void SetFireLevel(float fireLevel, bool doDamage = false)
        {
        }

        /// <summary>
        /// (IUnitResourceConsumer) Add resource units to this resource unit consumer.
        /// </summary>
        /// <param name="numAdditionalUnits">The number of resource units to add.</param>
		public void AddResourceUnits(int numAdditionalUnits)
		{
			currentResourceUnits += numAdditionalUnits;
		}


        /// <summary>
        /// (IUnitResourceConsumer) Set the number of resource units available for this resource unit consumer.
        /// </summary>
        /// <param name="numResourceUnits">The number of resource units to add.</param>
		public void SetResourceUnits(int numResourceUnits)
        {
            currentResourceUnits = numResourceUnits;
        }


		void Update()
	    {
			
			// Update the firing of this weapon
			if (firing && (infiniteResourceUnits || currentResourceUnits > 0) && (Time.time - lastFire) > fireRate)
            {
            	
				FireOnce();
	            
	            if (!continuousFire) firing = false;
	
	        }
	        
	    }
	}
}
