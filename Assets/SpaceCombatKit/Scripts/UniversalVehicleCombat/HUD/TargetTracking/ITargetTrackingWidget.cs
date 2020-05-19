using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// This interface allows widgets of many different kinds to be controlled by the HUDVisor component, through 
// the Visor_WidgetParameters class

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// This interface provides a common way to communicate with many different implementations of target tracking widgets.
    /// </summary>
	public interface ITargetTrackingWidget
    {
	
		void Enable();

		void Disable();

		void Set(TargetTracking_WidgetParameters widgetParameters);

	}

}
