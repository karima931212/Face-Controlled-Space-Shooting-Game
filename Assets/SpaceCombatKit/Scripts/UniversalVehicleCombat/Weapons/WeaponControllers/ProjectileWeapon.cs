using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement;
using VSX.General;



namespace VSX.UniversalVehicleCombat 
{

	/// <summary>
    /// This class is base class for projectile weapons of all kinds.
    /// </summary>
	public class ProjectileWeapon : MonoBehaviour, IModule, ITriggerable, IGunModule, IUnitResourceConsumer
	{

		[Header("Module")]

		[SerializeField]
		protected string label;
		public string Label { get { return label; } }
		
		protected ModuleType moduleType = ModuleType.GunWeapon;
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

		[Header("General")]

		[SerializeField]
		private int defaultTrigger;
		public int DefaultTrigger { get { return defaultTrigger; } }

		[Header("Gun Module")]

		[SerializeField]
        private float aimAssistAngle = 7f;
		
		[SerializeField]
		private List<Transform> spawnPoints = new List<Transform>();
	
		private List<FlashController> flashControllers = new List<FlashController>();

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

		// Projectile
		[SerializeField]
		private ProjectileController projectilePrefab;
		
		// Speed
		public float ProjectileSpeed { get { return projectilePrefab.Speed; } }

		public List<float> DamageValueByHealthType { get { return projectilePrefab.DamageValuesByHealthType; } }

		[SerializeField]
		private float powerDrawPerShot; // Per projectile or per second

		[SerializeField]
		private float fireRate = 0.13f;
		bool firing = false;
		private float lastFire = 0f;


		[Header("Audio")]
		
		// Shot-based Audio	
		[SerializeField]
		private GameObject shotAudioPrefab;
		
        /// <summary>
        /// Delegate for running functions when this triggerable is fired once.
        /// </summary>
        private OnFireOnceEventHandler onFireOnceEventHandler;
        public event OnFireOnceEventHandler OnFireOnceEventHandler
        {
            add { onFireOnceEventHandler += value; }
            remove { onFireOnceEventHandler -= value; }
        }

        /// <summary>
        /// Delegate for running functions when this triggerable's fire level is set.
        /// </summary>
		private OnSetFireLevelEventHandler onSetFireLevelEventHandler;
        public event OnSetFireLevelEventHandler OnSetFireLevelEventHandler
        {
            add { onSetFireLevelEventHandler += value; }
            remove { onSetFireLevelEventHandler -= value; }
        }

        /// <summary>
        /// Delegate for running functions when this weapon's aim assist is set.
        /// </summary>
        private OnSetAimAssistEventHandler onSetAimAssistEventHandler;
        public event OnSetAimAssistEventHandler OnSetAimAssistEventHandler
        {
            add { onSetAimAssistEventHandler += value; }
            remove { onSetAimAssistEventHandler -= value; }
        }

        /// <summary>
        /// Delegate for running functions when the aim assist is cleared/removed.
        /// </summary>
		private OnClearAimAssistEventHandler onClearAimAssistEventHandler;
        public event OnClearAimAssistEventHandler OnClearAimAssistEventHandler
        {
            add { onClearAimAssistEventHandler += value; }
            remove { onClearAimAssistEventHandler -= value; }
        }



        void Awake()
		{

			flashControllers = new List<FlashController>(transform.GetComponentsInChildren<FlashController>());

			cachedTransform = transform;
			cachedGameObject = gameObject;

		}


        // Called when the gameobject is activated
        void OnEnable()
        {
            ResetModule();
        }


        /// <summary>
        /// Reset the module to starting conditions.
        /// </summary>
		public void ResetModule()
        {
            currentResourceUnits = startingResourceUnits;
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


        /// <summary>
        /// (ITriggerable) start triggering this triggerable.
        /// </summary>
		public void StartTriggering()
        {
            firing = true;
        }


        /// <summary>
        /// (ITriggerable) Stop triggering this triggerable.
        /// </summary>
        public void StopTriggering()
        {
            firing = false;
        }


        /// <summary>
        /// Attempt an aim assist toward a position in space (snap to point there if the angle to that position is within aim assist angle)
        /// </summary>
        /// <param name="aimAssistTarget">The world space aim assist target position.</param>
        public void TryAimAssist(Vector3 aimAssistTarget)
        {

            // If there is a target and it is within the aim assist angle, aim the weapons, otherwise center the weapons
            float angleToTarget = Vector3.Angle(cachedTransform.forward, aimAssistTarget - cachedTransform.position);
            if (angleToTarget <= aimAssistAngle)
            {
                SetAimAssist(aimAssistTarget);
            }
            else
            {
                ClearAimAssist();
            }
        }


        /// <summary>
        /// Force an aim assist toward a position in space.
        /// </summary>
        /// <param name="aimAssistTarget">The world space aim assist target position.</param>
		public void SetAimAssist(Vector3 aimAssistTarget)
        {
            for (int i = 0; i < spawnPoints.Count; ++i)
            {
                // Aim the shot spawn transform at the target lead position
                spawnPoints[i].LookAt(aimAssistTarget);
            }

            if (onSetAimAssistEventHandler != null)
                onSetAimAssistEventHandler(aimAssistTarget);
        }


        /// <summary>
        /// Clear aim assist from this weapon (reset the spawn point rotation).
        /// </summary>
        public void ClearAimAssist()
		{
			// Center the weapons
			for (int i = 0; i < spawnPoints.Count; ++i)
			{
				spawnPoints[i].localRotation = Quaternion.identity;
			}

			if (onClearAimAssistEventHandler != null)
				onClearAimAssistEventHandler();
		}



		/// <summary>
        /// (ITriggerable) Fire this triggerable once.
        /// </summary>
        /// <param name="doDamage">Whether the weapon should do damage when fired.</param>
		public void FireOnce(bool doDamage = false)
		{
		
			for (int i = 0; i < spawnPoints.Count; ++i)
			{

				currentResourceUnits = Mathf.Max(currentResourceUnits - 1, 0);

				GameObject projectile = PoolManager.Instance.Get(projectilePrefab.gameObject, spawnPoints[i].position, spawnPoints[i].rotation);
				ProjectileController controller = projectile.GetComponent<ProjectileController>();
				controller.SetProjectileParameters(moduleMount.Vehicle);
				controller.DamageEnabled = !doDamage;
				
				if (shotAudioPrefab != null)
					PoolManager.Instance.Get(shotAudioPrefab, transform.position, Quaternion.identity);
	
				if (flashControllers.Count > i)
					flashControllers[i].OnFire();

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


        // Called every frame
		void Update()
		{

			// Firing
			if (firing && (Time.time - lastFire > fireRate))
			{
				
				bool enoughPower = !moduleMount.Vehicle.HasPower || moduleMount.Vehicle.Power.HasStoredPower(SubsystemType.Weapons, powerDrawPerShot * spawnPoints.Count);

				if (enoughPower && (infiniteResourceUnits || currentResourceUnits > 0))
				{
					
					if (moduleMount.Vehicle.HasPower) moduleMount.Vehicle.Power.DrawStoredPower(SubsystemType.Weapons, powerDrawPerShot * spawnPoints.Count);

					FireOnce();					

					lastFire = Time.time;
				}
			}
		}
	}
}

