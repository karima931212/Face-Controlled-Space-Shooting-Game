using UnityEngine;
using System.Collections;

namespace VSX.UniversalVehicleCombat
{

    /// <summary>
    /// Delegate for attaching event functions to run when a triggerable module is fired once.
    /// </summary>
	public delegate void OnFireOnceEventHandler();

    /// <summary>
    /// Delegate for attaching event functions to run when a triggerable module's fire level is set.
    /// </summary>
    /// <param name="level">The new fire level the triggerable module is at.</param>
	public delegate void OnSetFireLevelEventHandler(float level);


	/// <summary>
    /// An interface for a triggerable module that can be loaded on a vehicle.
    /// </summary>
	public interface ITriggerable 
	{

        GameObject CachedGameObject { get; }

        Transform CachedTransform { get; }

        int DefaultTrigger { get; }
	
		void StartTriggering();
	
		void StopTriggering();

		// Force fire the triggerable module once
		void FireOnce(bool dummyFire = false);
	
		// Force set the fire level
		void SetFireLevel(float level, bool dummyFire = false);

		// The event instance that is called when the triggerable is fired once
		event OnFireOnceEventHandler OnFireOnceEventHandler;

		// The event instance that is called when the triggerable fire level is set
		event OnSetFireLevelEventHandler OnSetFireLevelEventHandler;

	}
}
