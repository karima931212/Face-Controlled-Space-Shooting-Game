using UnityEngine;
using System.Collections;

namespace VSX.UniversalVehicleCombat
{

    /// <summary>
    /// Delegate for attaching event functions for when the aim assist is set.
    /// </summary>
    /// <param name="aimAssistTarget">The aim assist target</param>
	public delegate void OnSetAimAssistEventHandler(Vector3 aimAssistTarget);

    /// <summary>
    /// Delegate for attaching event functions to be run when the aim assist is cleared.
    /// </summary>
	public delegate void OnClearAimAssistEventHandler();

	// Interface for any kind of gun module (which is basically defined by the ability to fire projectiles that move at constant velocity)
    /// <summary>
    /// This interface provides a way to interface with gun modules of 
    /// many different kinds.
    /// </summary>
	public interface IGunModule : IWeaponModule
	{
	
		float ProjectileSpeed { get; }

		void TryAimAssist (Vector3 aimAssistTarget);

		void SetAimAssist (Vector3 aimAssistTarget);

		void ClearAimAssist ();

		event OnSetAimAssistEventHandler OnSetAimAssistEventHandler;

		event OnClearAimAssistEventHandler OnClearAimAssistEventHandler;

	}
}
