using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using VSX.General;


namespace VSX.UniversalVehicleCombat 
{

	/// <summary>
    /// This class manages the cockpit HUD for a vehicle
    /// </summary>
	public class HUDShipCockpit : MonoBehaviour 
	{

        // Weapon info

        [SerializeField]
        private GameObject autopilotOnWidget;

		[SerializeField]
		private GameObject primaryInfoInstancePrefab;
		
		[SerializeField]
		private Transform primaryInfoParent;
		private List<HUDPrimaryWeaponInfoController> primaryInfoInstances = new List<HUDPrimaryWeaponInfoController>();
	
		[SerializeField]
		private GameObject secondaryInfoInstancePrefab;
		
		[SerializeField]
		private Transform secondaryInfoParent;
		private List<HUDSecondaryWeaponInfoController> secondaryInfoInstances = new List<HUDSecondaryWeaponInfoController>();
	
		[SerializeField]
		private MeshRenderer speedBarRenderer;

		[SerializeField]
		private Text speedValueText;
	

		[SerializeField]
		private MeshRenderer energyBarRenderer;

		[SerializeField]
		private Text energyValueText;

	
		[SerializeField]
		private Image shieldBar;

		[SerializeField]
		private Image armorBar;
	
		private Transform cachedTransform;
		public Transform CachedTransform { get { return cachedTransform; } }
	
		private GameObject cachedGameObject;
		public GameObject CachedGameObject { get { return cachedGameObject; } }
	
		private HUDManager manager;
		private bool hasManager;
		
	
		void Start()
		{

			cachedGameObject = gameObject;
			cachedTransform = transform;

		}


        /// <summary>
        /// Implement anything that needs to happen when the HUD that this component is part of is activated in the scene.
        /// </summary>
        public void OnActivate()
		{
		}


        /// <summary>
        /// Implement anything that needs to happen when the HUD that this component is part of is removed from the active scene.
        /// </summary>
        public void OnDeactivate()
		{
		}


		// Update the weapon info on the HUD
		void UpdateWeaponInfo()
		{
			
			if (!manager.FocusedVehicle.HasWeapons) return;

			int gunModuleIndex = -1;
			int missileModuleIndex = -1;

			for (int i = 0; i < manager.FocusedVehicle.Weapons.MountedWeapons.Count; ++i)
			{
				if (manager.FocusedVehicle.Weapons.MountedWeapons[i].IsGunModule)
				{

					gunModuleIndex += 1;
			
					// Make sure all of the gun modules are showing on the HUD	
					if (primaryInfoInstances.Count <= gunModuleIndex)
					{
						HUDPrimaryWeaponInfoController controller = PoolManager.Instance.Get(primaryInfoInstancePrefab, Vector3.zero, Quaternion.identity, 
                                                                                                primaryInfoParent).GetComponent<HUDPrimaryWeaponInfoController>();
	
						controller.transform.localPosition = Vector3.zero;
						controller.transform.localRotation = Quaternion.identity;
						controller.transform.localScale = new Vector3(1, 1, 1);
		
						controller.SetLabel(manager.FocusedVehicle.Weapons.MountedWeapons[i].WeaponModule.Label);
		
						primaryInfoInstances.Add(controller);
					}
				}
				else if (manager.FocusedVehicle.Weapons.MountedWeapons[i].IsMissileModule)
				{
					
					missileModuleIndex += 1;

					// Make sure all of the missile modules are showing on the HUD	
					if (secondaryInfoInstances.Count <= missileModuleIndex)
					{
						HUDSecondaryWeaponInfoController controller = PoolManager.Instance.Get(secondaryInfoInstancePrefab, Vector3.zero, Quaternion.identity, 
                                                                                                secondaryInfoParent).GetComponent<HUDSecondaryWeaponInfoController>();
			
						controller.transform.localPosition = Vector3.zero;
						controller.transform.localRotation = Quaternion.identity;
						controller.transform.localScale = new Vector3(1,1,1);
		
						controller.SetLabel(manager.FocusedVehicle.Weapons.MountedWeapons[i].WeaponModule.Label);
						secondaryInfoInstances.Add(controller);
					}

					// Update the UI showing if the missile weapon is locked or not, and how many units are left
					if (manager.FocusedVehicle.Weapons.MountedWeapons[i].IsUnitResourceConsumer) 
						secondaryInfoInstances[missileModuleIndex].SetNumAmmo(manager.FocusedVehicle.Weapons.MountedWeapons[i].UnitResourceConsumer.CurrentResourceUnits);						

				 	secondaryInfoInstances[missileModuleIndex].SetIsLocked(manager.FocusedVehicle.Weapons.MountedWeapons[i].MissileModule.LockState == LockState.Locked);
					
				}
			}	
		}

	
        /// <summary>
        /// Set the HUDManager that manages this part of the HUD.
        /// </summary>
        /// <param name="manager">The manager for this HUD.</param>
		public void SetManager(HUDManager manager)
		{
			this.manager = manager;
			this.hasManager = true;
		}
	
		void Update()
		{
			
			if (!hasManager || !manager.HasFocusedVehicle)
				return;
	
	
			// Update the speed bar
			if (manager.FocusedVehicle.HasEngines)
			{
				float speedAmount = manager.FocusedVehicle.CachedRigidbody.velocity.magnitude / manager.FocusedVehicle.Engines.GetMaxSpeedByAxis(false).z;
				
				speedBarRenderer.material.SetFloat("_FillAmount", speedAmount);
		
				speedValueText.text = ((int)manager.FocusedVehicle.CachedRigidbody.velocity.magnitude).ToString();
			}
			
	
			// Update the weapon energy bar
			if (manager.FocusedVehicle.HasPower)
			{
				float weaponPowerAmount = manager.FocusedVehicle.Power.GetStoredPower(SubsystemType.Weapons) /
				                          manager.FocusedVehicle.Power.GetStorageCapacity(SubsystemType.Weapons);
                
				energyBarRenderer.material.SetFloat("_FillAmount", weaponPowerAmount);
		
				energyValueText.text = ((int)manager.FocusedVehicle.Power.GetStoredPower(SubsystemType.Weapons)).ToString();
			}
	
			// Update the health bar
			if (manager.FocusedVehicle.HasHealth)
			{
				shieldBar.fillAmount = manager.FocusedVehicle.Health.GetCurrentHealthValue(HealthType.Shield) /
				manager.FocusedVehicle.Health.GetStartingHealthValue(HealthType.Shield);
		
				armorBar.fillAmount = manager.FocusedVehicle.Health.GetCurrentHealthValue(HealthType.Armor) /
				manager.FocusedVehicle.Health.GetStartingHealthValue(HealthType.Armor);
			}

			UpdateWeaponInfo();
	
		}
	}
}
