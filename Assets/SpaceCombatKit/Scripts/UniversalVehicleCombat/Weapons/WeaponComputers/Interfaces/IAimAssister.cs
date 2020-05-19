using UnityEngine;
using System.Collections;

namespace VSX.UniversalVehicleCombat
{

    /// <summary>
    /// This interface provides a way to enable or disable the aim assist on a component
    /// that is doing it (such as a weapon computer).
    /// </summary>
	public interface IAimAssister 
	{

		bool AimAssist { get; set; }
        
    }
}
