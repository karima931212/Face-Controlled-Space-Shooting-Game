using UnityEngine;
using System.Collections;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// This interface allows 3D radar widgets of many different kinds to be controlled by the HUDRadar3D component.
    /// </summary>
	public interface IRadar3DWidget
    {
	
		void Enable();
	
		void Disable();
	
		void Set(Radar3D_WidgetParameters _parameters);
	
	}
}
