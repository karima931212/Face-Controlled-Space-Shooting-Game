using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace VSX.UniversalVehicleCombat
{
	/// <summary>
    /// This interface provides a way to access locking data from a component which is storing
    /// it (such as a weapon computer) so that it can be used e.g. for visualisation by the HUDTargetTracking component.
    /// </summary>
	public interface ILockingComputer 
	{
	
		List<LockingData> LockingDataList { get; }
	}
}
