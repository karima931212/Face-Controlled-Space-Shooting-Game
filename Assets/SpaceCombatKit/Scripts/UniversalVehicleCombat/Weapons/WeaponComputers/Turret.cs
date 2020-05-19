using UnityEngine;
using System.Collections;

namespace VSX.UniversalVehicleCombat
{

    /// <summary>
    /// Different targeting modes that a turret can be in.
    /// </summary>
	public enum TurretTargetingMode
	{
		Automatic,
		CameraVanishingPoint,
		None
	}


    /// <summary>
    /// This class provides an easy way to store and manage gimballed weapons, and is used by the 
    /// Weapons Computer to update gimballed weapon targeting.
    /// </summary>
	public class Turret
	{

		private TurretTargetingMode targetingMode;
		public TurretTargetingMode TargetingMode
		{ 
			get { return targetingMode; } 
			set { targetingMode = value; }
		}

		IWeaponModule weaponModule;
		public IWeaponModule WeaponModule { get { return weaponModule; } }

		private GimbalController gimbalController;
		public GimbalController GimbalController { get { return gimbalController; } }

		private ModuleMount moduleMount;
		public ModuleMount ModuleMount { get { return moduleMount; } }


        /// <summary>
        /// Create a new instance of a Turret.
        /// </summary>
        /// <param name="weaponModule">The turret's weapon module.</param>
        /// <param name="moduleMount">The turret's module mount.</param>
        /// <param name="gimbalController">The turret's gimbal controller.</param>
        /// <param name="targetingMode">The turret's targeting mode.</param>
		public Turret (IWeaponModule weaponModule, ModuleMount moduleMount, GimbalController gimbalController, TurretTargetingMode targetingMode)
		{
			this.weaponModule = weaponModule;
			this.moduleMount = moduleMount;
			this.gimbalController = gimbalController;
			this.targetingMode = targetingMode;
		}
		
	}
}
