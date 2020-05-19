using UnityEngine;
using System.Collections;

namespace VSX.UniversalVehicleCombat
{
	/// <summary>
    /// This interface is for a powerplant module that can be loaded onto a vehicle to provide power to engines and other
    /// subsystems.
    /// </summary>
	public interface IPowerPlant 
	{
		float Output { get; }
	}
}
