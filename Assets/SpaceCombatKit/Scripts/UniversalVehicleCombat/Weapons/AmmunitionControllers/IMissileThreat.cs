using UnityEngine;
using System.Collections;
using VSX.UniversalVehicleCombat;

namespace VSX.UniversalVehicleCombat
{
	/// <summary>
    /// This interface provides a way to store missile threats for a vehicle's Radar subsystem.
    /// </summary>
	public interface IMissileThreat 
	{
	
		MissileState MissileState { get; }
	
		GameObject CachedGameObject { get; }
	
	}
}