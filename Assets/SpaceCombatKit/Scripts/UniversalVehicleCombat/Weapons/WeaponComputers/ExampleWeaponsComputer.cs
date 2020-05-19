using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace VSX.UniversalVehicleCombat
{

	/// <summary>
    /// This class is an example (or base) class for implementing weapon computers for vehicles. This class
    /// provides aim assist, lead target calculation, missile locking and turret management.
    /// </summary>
	public class ExampleWeaponsComputer : MonoBehaviour, IModule, IWeaponsComputer, ILeadTargetComputer, ILockingComputer, IAimAssister
	{
	
		[Header("Module")]

		[SerializeField]
		protected string label;
		public string Label { get { return label; } }
		
		[SerializeField]
		public ModuleType ModuleType { get { return ModuleType.Utility; }  }
		
		[SerializeField]
		protected Sprite menuSprite;
		public Sprite MenuSprite { get { return menuSprite; } }
		
		protected GameObject cachedGameObject;
		public GameObject CachedGameObject { get { return cachedGameObject; } }
		public GameObject GameObject { get { return gameObject; } }
	
		protected Transform cachedTransform;
		public Transform CachedTransform { get { return cachedTransform; } }
		
		protected ModuleState moduleState;
		public ModuleState ModuleState { get { return moduleState; } }
	
		protected ModuleMount moduleMount;

		[SerializeField]
		private bool aimAssist = true;
		public bool AimAssist
		{
			get { return aimAssist; }
			set 
			{ 	
				aimAssist = value; 
				if (aimAssist)
				{
					for (int i = 0; i < leadTargetDataList.Count; ++i)
					{
						leadTargetDataList[i].GunModule.ClearAimAssist();
					}
				}
			}
		}
	
		public IAimAssister AimAssister { get { return this; } }
		public bool IsAimAssister { get { return true; } }

		public ILockingComputer LockingComputer { get { return this; } }
		public bool IsLockingComputer { get { return true; } }

		public ILeadTargetComputer LeadTargetComputer { get { return this; } }
		public bool IsLeadTargetComputer { get { return true; } }

		private List<LockingData> lockingDataList = new List<LockingData>();
		public List<LockingData> LockingDataList { get { return lockingDataList; } }

		private List<LeadTargetData> leadTargetDataList = new List<LeadTargetData>();
		public List<LeadTargetData> LeadTargetDataList { get { return leadTargetDataList; } }
	
		[SerializeField]
		private TurretTargetingMode defaultTurretTargetingMode = TurretTargetingMode.CameraVanishingPoint;

		private List<Turret> turretsList = new List<Turret>();
		public List<Turret> TurretsList { get { return turretsList; } }

		[SerializeField]
		private float maxTurretFireAngle = 5;

		private Transform missileLockingPointer;
		private Transform gimbalAimingPointer;

       
	
		void Awake()
		{

			cachedTransform = transform;
			cachedGameObject = gameObject;

		}		

		void Start()
		{
			VehicleCamera vehicleCamera = GameObject.FindObjectOfType<VehicleCamera>();
			if (vehicleCamera != null)
			{
				missileLockingPointer = vehicleCamera.ChaseCamera.transform;
				gimbalAimingPointer = vehicleCamera.ChaseCamera.transform;
			}
			else
			{
				missileLockingPointer = moduleMount.Vehicle.CachedTransform;
				gimbalAimingPointer = moduleMount.Vehicle.CachedTransform;
			}
		}


        /// <summary>
        /// Set a new module state for this module.
        /// </summary>
        /// <param name="newModuleState">The new module state.</param>
        public void SetModuleState(ModuleState newModuleState)
		{
            moduleState = newModuleState;
		}


		/// <summary>
        /// Event called when this module is mounted onto a module mount on a vehicle.
        /// </summary>
        /// <param name="moduleMount">The module mount where this module is mounted.</param>
		public void OnMount(ModuleMount moduleMount)
		{

			this.moduleMount = moduleMount;

			// Link to the new module mounted event for each module mount
			for (int i = 0; i < moduleMount.Vehicle.ModuleMounts.Count; ++i)
			{
				if (moduleMount.Vehicle.ModuleMounts[i].MountedModuleIndex != -1)
				{
					OnModuleMounted(moduleMount.Vehicle.ModuleMounts[i]);
				}
				moduleMount.Vehicle.ModuleMounts[i].NewModuleMountedEventHandler += OnModuleMounted;
			}
		}

			
		/// <summary>
        /// Called when this module is unmounted from a module mount on a vehicle.
        /// </summary>
		public virtual void OnUnmount()
		{
			this.moduleMount = null;
			cachedGameObject.SetActive(false);
		}

	
        // Called when the gameobject is deactivated
		void OnDisable()
		{
			// Unlink from the new module mounted event for each module mount
			if (moduleMount != null)
			{
				for (int i = 0; i < moduleMount.Vehicle.ModuleMounts.Count; ++i)
				{
					moduleMount.Vehicle.ModuleMounts[i].NewModuleMountedEventHandler -= OnModuleMounted;
				}
			}
		}	


        /// <summary>
        /// Reset the module to starting conditions.
        /// </summary>
		public void ResetModule()
		{
			SetModuleState(ModuleState.Activated);
		}


		// Update the lead target position for HUD visualisation and aim assist
        /// <summary>
        /// Calculate the lead target position for a particular projectile speed.
        /// </summary>
        /// <param name="projectileSpeed">The projectile speed.</param>
        /// <returns>The world space lead target position.</returns>
		public Vector3 GetLeadTargetData(float projectileSpeed)
		{
			if (!moduleMount.Vehicle.HasRadar || !moduleMount.Vehicle.Radar.HasSelectedTarget)
				return Vector3.zero;

			if (projectileSpeed < 0.0001f) return moduleMount.Vehicle.Radar.SelectedTarget.CachedTransform.position;

            return (StaticFunctions.GetLeadPosition(moduleMount.Vehicle.CachedTransform.position, projectileSpeed, moduleMount.Vehicle.Radar.SelectedTarget));
            
        }


        /// <summary>
        /// Event called when a new module is mounted onto one of the vehicle's module mounts.
        /// </summary>
        /// <param name="mountedModuleMount">The module mount where the new module was mounted.</param>
        public void OnModuleMounted(ModuleMount mountedModuleMount)
		{
			
			if (!gameObject.activeSelf)
				return;

			// Remove any previous lead target data associated with this mount
			for (int i = 0; i < leadTargetDataList.Count; ++i)
			{
				if (leadTargetDataList[i].ModuleMount == mountedModuleMount)
				{
					leadTargetDataList.RemoveAt(i);
					break;
				}
			}
			
			// Remove any previous locking data associated with this mount
			for (int i = 0; i < lockingDataList.Count; ++i)
			{
				if (lockingDataList[i].ModuleMount == mountedModuleMount)
				{
					lockingDataList.RemoveAt(i);
					break;
				}
			}

			// Remove any turrets associated with this mount
			for (int i = 0; i < turretsList.Count; ++i)
			{
				if (turretsList[i].ModuleMount == mountedModuleMount)
				{
					turretsList.RemoveAt(i);
					break;
				}
			}
			
			if (mountedModuleMount.MountedModuleIndex != -1)
			{

				IMissileModule missileModule = mountedModuleMount.Module().CachedGameObject.GetComponent<IMissileModule>();
				IUnitResourceConsumer unitConsumer = mountedModuleMount.Module().CachedGameObject.GetComponent<IUnitResourceConsumer>();
				if (missileModule != null)
				{
					LockingData newLockingData = new LockingData(mountedModuleMount, missileModule, unitConsumer);
					Coroutine cor = StartCoroutine(LockingCoroutine(newLockingData));
					newLockingData.Initialize(cor);
						
					lockingDataList.Add(newLockingData);

				}

				IGunModule gunModule = mountedModuleMount.Module().CachedGameObject.GetComponent<IGunModule>();
				if (gunModule != null)
				{
					LeadTargetData newLeadTargetData = new LeadTargetData(mountedModuleMount, gunModule);
					leadTargetDataList.Add(newLeadTargetData);
				}

				GimbalController gimbalController = mountedModuleMount.Module().CachedGameObject.GetComponent<GimbalController>();
				IWeaponModule weaponModule = mountedModuleMount.Module().CachedGameObject.GetComponent<IWeaponModule>();
				if (weaponModule != null && gimbalController != null)
				{
					turretsList.Add(new Turret(weaponModule, mountedModuleMount, gimbalController, defaultTurretTargetingMode));
				}
			}
		}


		/// <summary>
        /// Get whether the vehicle's selected target is within the lock zone for a missile weapon.
        /// </summary>
        /// <param name="missileWeaponData">The missile weapon info.</param>
        /// <returns></returns>
		bool TargetInMissileLockZone(LockingData missileWeaponData)
		{
            
			if (!moduleMount.Vehicle.HasRadar || !moduleMount.Vehicle.Radar.HasSelectedTarget)
				return false;
	
			float angleToTarget;
			if (missileWeaponData.MissileModule.LockWithCamera)
			{
				angleToTarget = Vector3.Angle(Vector3.forward, missileLockingPointer.InverseTransformPoint(moduleMount.Vehicle.Radar.SelectedTarget.CachedTransform.position));
			}
			else
			{
				angleToTarget = Vector3.Angle(Vector3.forward, missileWeaponData.MissileModule.CachedTransform.InverseTransformPoint(moduleMount.Vehicle.Radar.SelectedTarget.CachedTransform.position));
			}
            
			float distToTarget = Vector3.Distance(moduleMount.Vehicle.Radar.SelectedTarget.CachedTransform.position, missileWeaponData.MissileModule.CachedTransform.position);
	
			return (angleToTarget < missileWeaponData.MissileModule.MaxLockingAngle && distToTarget < missileWeaponData.MissileModule.MaxLockingRange);
	
		}


		/// <summary>
        /// Coroutine for updating the lock state of a missile weapon.
        /// </summary>
        /// <param name="lockingData">The missile weapon info.</param>
        /// <returns>null.</returns>
		IEnumerator LockingCoroutine(LockingData lockingData)
		{
			
			yield return null;
	
			float lockStartTime = Time.time;
            
            while (true)
			{
                
                lockingData.SetIsChangedLockStateEvent(false);

				if (!lockingData.UnitResourceConsumer.InfiniteResourceUnits && lockingData.UnitResourceConsumer.CurrentResourceUnits == 0)
				{
                    if (lockingData.LockState != LockState.NoLock)
					{
						lockingData.SetLockState (LockState.NoLock);
						lockingData.SetIsChangedLockStateEvent(true);
					}
					yield return null;
					continue;
				}

				switch (lockingData.LockState)
				{
	
					case LockState.NoLock:
                        if (TargetInMissileLockZone(lockingData))
						{
							lockStartTime = Time.time;
							lockingData.SetLockState(LockState.Locking);
							lockingData.SetIsChangedLockStateEvent(true);
						}
						break;
	
	
					case LockState.Locking:
	
						if (!TargetInMissileLockZone(lockingData))
						{
							lockingData.SetLockState(LockState.NoLock);
							lockingData.SetIsChangedLockStateEvent(true);
						}
						else if (Time.time - lockStartTime > lockingData.MissileModule.LockingPeriod)
						{
							lockingData.SetLockState(LockState.Locked);
							lockingData.SetIsChangedLockStateEvent(true);
						}
						
						break;
	
					case LockState.Locked:
	
						if (!TargetInMissileLockZone(lockingData))
						{
							lockingData.SetLockState(LockState.NoLock);
							lockingData.SetIsChangedLockStateEvent(true);
						}
						break;
				
				}
	
				yield return null;
			}
		}


		/// <summary>
        /// Implement aim assist for the vehicle's weapons.
        /// </summary>
		public void DoAimAssist()
		{

			if (moduleMount == null) return;

			for (int i = 0; i < leadTargetDataList.Count; ++i)
			{
				if (moduleMount.Vehicle.HasRadar && moduleMount.Vehicle.Radar.HasSelectedTarget)
				{                  
					leadTargetDataList[i].GunModule.TryAimAssist(leadTargetDataList[i].LeadTargetPosition);
				}
				else
				{
					leadTargetDataList[i].GunModule.ClearAimAssist();
				}
			}
		}


        // Called every frame
		void Update()
		{		

			if (aimAssist) DoAimAssist();
			
			for (int i = 0; i < leadTargetDataList.Count; ++i)
			{
				leadTargetDataList[i].SetLeadTargetPosition(GetLeadTargetData(leadTargetDataList[i].GunModule.ProjectileSpeed));		
				
			}
			
			// Aim the turrets
			for (int i = 0; i < turretsList.Count; ++i)
			{
				float angleToTarget;
				Vector3 aimTarget;
				switch (turretsList[i].TargetingMode)
				{
					case TurretTargetingMode.Automatic:
						if (moduleMount.Vehicle.HasRadar && moduleMount.Vehicle.Radar.HasSelectedTarget)
						{
							aimTarget = moduleMount.Vehicle.Radar.SelectedTarget.CachedTransform.position;
						}
						else
						{
							aimTarget = moduleMount.Vehicle.CachedTransform.position + moduleMount.Vehicle.CachedTransform.forward * 1000;
						}
						break;

					default:
						aimTarget = gimbalAimingPointer.position + gimbalAimingPointer.forward * 1000;
						break;
					
				}
				
				turretsList[i].GimbalController.TrackPosition(aimTarget, out angleToTarget, false);
				if (moduleMount.Vehicle.Radar.HasSelectedTarget && angleToTarget < maxTurretFireAngle)
				{
					turretsList[i].WeaponModule.StartTriggering();
				}
				else
				{
					turretsList[i].WeaponModule.StopTriggering();
				}
			}
		}		
	}
}
