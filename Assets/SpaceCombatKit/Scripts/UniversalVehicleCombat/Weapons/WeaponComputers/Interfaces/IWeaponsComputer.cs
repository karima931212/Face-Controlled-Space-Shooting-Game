using UnityEngine;
using System.Collections;

namespace VSX.UniversalVehicleCombat
{

	/// <summary>
    /// This interface provides a way to interface with many different kinds of 
    /// weapon computers.
    /// </summary>
	public interface IWeaponsComputer 
	{

		ILockingComputer LockingComputer { get; }
		bool IsLockingComputer { get; }

		ILeadTargetComputer LeadTargetComputer { get; }
		bool IsLeadTargetComputer { get; }

		IAimAssister AimAssister { get; }
		bool IsAimAssister { get; }

	}
}
