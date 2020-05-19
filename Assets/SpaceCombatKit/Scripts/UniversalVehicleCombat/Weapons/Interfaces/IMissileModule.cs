using UnityEngine;
using System.Collections;
using VSX.UniversalVehicleCombat;

namespace VSX.UniversalVehicleCombat
{

	/// <summary>
    /// This interface (which extends the IWeaponModule interface) provides a way to interface
    /// with missile weapons of all kinds.
    /// </summary>
	public interface IMissileModule : IWeaponModule
	{
	
		float Agility { get; }
	
		float MaxSpeed { get; }

		bool LockWithCamera { get; }
	
		float LockingPeriod { get; }
	
		float MaxLockingAngle { get; }
		
		float MaxLockingRange { get; }
	
		LockState LockState { get; }
	
		void SetLockState (LockState newLockState);
	
	}
}
