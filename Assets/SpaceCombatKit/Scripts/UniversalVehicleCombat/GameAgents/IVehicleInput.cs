using UnityEngine;
using System.Collections;

namespace VSX.UniversalVehicleCombat
{

    /// <summary>
    /// An interface that allows a game agent to enable or disable different control scripts for different vehicles.
    /// </summary>
    public interface IVehicleInput 
	{
	
		VehicleControlClass VehicleControlClass { get; }

		bool ControlsDisabled { get; set; }

		void Initialize (GameAgent agent);
		
		void Begin();

		void Finish();

		bool Running { get; }

    }
}