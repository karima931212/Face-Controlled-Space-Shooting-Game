using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


namespace VSX.UniversalVehicleCombat
{

	/// <summary>
    /// A class that makes it easy to store and access weapons mounted on this vehicle
    /// </summary>
	public class MountedWeapon
	{

		private ModuleMount moduleMount;
		public ModuleMount ModuleMount { get { return moduleMount; } }

		private IWeaponModule weaponModule;
		public IWeaponModule WeaponModule { get { return weaponModule; } }

		private bool isMissileModule;
		public bool IsMissileModule { get { return isMissileModule; } }

		private IMissileModule missileModule;
		public IMissileModule MissileModule { get { return missileModule; } }

		private bool isGunModule;
		public bool IsGunModule { get { return isGunModule; } }

		private IGunModule gunModule;
		public IGunModule GunModule { get { return gunModule; } }

		private bool isUnitResourceConsumer;
		public bool IsUnitResourceConsumer { get { return isUnitResourceConsumer; } }

		private IUnitResourceConsumer unitResourceConsumer;
		public IUnitResourceConsumer UnitResourceConsumer { get { return unitResourceConsumer; } }

		private bool isGimballed;
		public bool IsGimballed { get { return isGimballed; } }

		private GimbalController gimbalController;
		public GimbalController GimbalController { get { return gimbalController; } }

		

        /// <summary>
        /// Create a new instance of the MountedWeapon class.
        /// </summary>
        /// <param name="moduleMount">The module mount where the weapon is mounted.</param>
        /// <param name="weaponModule">The weapon module interface reference for the mounted weapon.</param>
		public MountedWeapon(ModuleMount moduleMount, IWeaponModule weaponModule)
		{

			this.moduleMount = moduleMount;
			this.weaponModule = weaponModule;
			
			this.missileModule = weaponModule.CachedGameObject.GetComponent<IMissileModule>();
			this.isMissileModule = this.missileModule != null;

			this.gunModule = weaponModule.CachedGameObject.GetComponent<IGunModule>();
			this.isGunModule = this.gunModule != null;

			this.unitResourceConsumer = weaponModule.CachedGameObject.GetComponent<IUnitResourceConsumer>();
			this.isUnitResourceConsumer = this.unitResourceConsumer != null;

			this.gimbalController = weaponModule.CachedGameObject.GetComponent<GimbalController>();
			this.isGimballed = this.gimbalController != null;

		}
	
	}


    public delegate void OnAimAssistChangedEventHandler(bool isOn);


    /// <summary>
    /// A class that makes it easy to access the weapon computer currently mounted on one of the 
    /// vehicle's module mounts.
    /// </summary>
    public class MountedWeaponsComputer
	{
		private IWeaponsComputer weaponsComputer;
		public IWeaponsComputer WeaponsComputer { get { return weaponsComputer; } }

		private ModuleMount moduleMount;
		public ModuleMount ModuleMount { get { return moduleMount; } }

		public MountedWeaponsComputer(IWeaponsComputer weaponsComputer, ModuleMount moduleMount)
		{
			this.weaponsComputer = weaponsComputer;
			this.moduleMount = moduleMount;
		}
	}


	/// <summary>
    /// This class (derived from the Subsystem class) is a component that stores and manages weapons mounted
    /// on the vehicle, including firing and recharging.
    /// </summary>
	public class Weapons : Subsystem 
	{

		// Mounted weapons		
		private List<MountedWeapon> mountedWeapons = new List<MountedWeapon>();
		public List<MountedWeapon> MountedWeapons { get { return mountedWeapons; } }

		private MountedWeaponsComputer mountedWeaponsComputer;
		public MountedWeaponsComputer MountedWeaponsComputer
		{
			get { return mountedWeaponsComputer; }
		}

		private bool hasWeaponsComputer;
		public bool HasWeaponsComputer { get { return hasWeaponsComputer; } }

		[SerializeField]
		private bool aimAssist;
		public bool AimAssist { get { return aimAssist; } }

        public OnAimAssistChangedEventHandler onAimAssistChangedEventHandler;

		

		/// <summary>
        /// Event called when a new module is mounted onto the vehicle.
        /// </summary>
        /// <param name="moduleMount">The module mount where the new module is mounted.</param>
		protected override void OnModuleMounted(ModuleMount moduleMount)
		{
			
			for (int i = 0; i < mountedWeapons.Count; ++i)
			{
				// if a weapon has already been recorded at this weapon mount
				if (mountedWeapons[i].ModuleMount == moduleMount)
				{
					mountedWeapons.RemoveAt(i);
					break;
				}
			}

			if (moduleMount.MountedModuleIndex == -1)
				return;

			IWeaponModule weaponModule = moduleMount.Module().CachedGameObject.GetComponent<IWeaponModule>();
			if (weaponModule != null)
			{
				MountedWeapon newMountedWeapon = new MountedWeapon(moduleMount, weaponModule);
				mountedWeapons.Add(newMountedWeapon);
			}

			IWeaponsComputer weaponsComputerInterface = moduleMount.Module().CachedGameObject.GetComponent<IWeaponsComputer>();
			if (weaponsComputerInterface != null)
			{
				mountedWeaponsComputer = new MountedWeaponsComputer(weaponsComputerInterface, moduleMount);
				hasWeaponsComputer = true;
			}
			else
			{
				if (hasWeaponsComputer && mountedWeaponsComputer.ModuleMount == moduleMount)
				{
					mountedWeaponsComputer = null;
					hasWeaponsComputer = false;
				}
			}
		}

		
		/// <summary>
        /// Fire weapons assigned to a particular trigger index.
        /// </summary>
        /// <param name="triggerIndex">The trigger index for the weapons that are to be fired.</param>
		public void StartFiringOnTrigger(int triggerIndex)
		{
			// Go through the selected weapon group and fire all the weapons on this trigger
			for (int i = 0; i < mountedWeapons.Count; ++i)
			{
				if (mountedWeapons[i].WeaponModule.DefaultTrigger == triggerIndex)
				{
					mountedWeapons[i].WeaponModule.StartTriggering();
				}
			}
		}


        /// <summary>
        /// Stop firing weapons assigned to a particular trigger index.
        /// </summary>
        /// <param name="triggerIndex">The trigger index for the weapons that are to be stopped.</param>
        public void StopFiringOnTrigger(int triggerIndex)
		{

			// Go through all the weapons in the selected weapon group and stop firing all the weapons on this trigger
			for (int i = 0; i < mountedWeapons.Count; ++i)
			{
				if (mountedWeapons[i].WeaponModule.DefaultTrigger == triggerIndex)
				{
					mountedWeapons[i].WeaponModule.StopTriggering();
				}
			}
		}

        /// <summary>
        /// Called by an input script to toggle aim assist on/off.
        /// </summary>
        public void ToggleAimAssist()
        {
            SetAimAssist(!aimAssist);
        }


        /// <summary>
        /// Set the aim assist on or off.
        /// </summary>
        /// <param name="setOn">New aim assist setting.</param>
        public void SetAimAssist(bool setOn)
        {

            // Set the aim assist
            aimAssist = setOn;

            if (hasWeaponsComputer)
            {
                mountedWeaponsComputer.WeaponsComputer.AimAssister.AimAssist = aimAssist;
            }

            // Call the event
            if (onAimAssistChangedEventHandler != null) onAimAssistChangedEventHandler(aimAssist);

        }


		/// <summary>
        /// Stop firing all weapons.
        /// </summary>
		public void StopFiringAllWeapons()
		{
			for (int i = 0; i < mountedWeapons.Count; ++i)
			{
				mountedWeapons[i].WeaponModule.StopTriggering();
			}
		}
	}
}
