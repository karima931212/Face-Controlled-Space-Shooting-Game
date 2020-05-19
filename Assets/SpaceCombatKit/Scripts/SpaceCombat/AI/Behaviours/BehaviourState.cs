using UnityEngine;
using System.Collections;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// An enum that describes the current state of an AI vehicle
    /// </summary>
    public enum BehaviourState
    {
        None,
        Patrolling,
        Combat
    }

    /// <summary>
    /// An enum that describes the current combat state of an AI vehicle.
    /// </summary>
    public enum CombatState
    {
        Attacking,
        Evading
    }
}
