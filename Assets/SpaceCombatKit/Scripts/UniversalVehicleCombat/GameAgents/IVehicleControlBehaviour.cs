using UnityEngine;
using System.Collections;

namespace VSX.UniversalVehicleCombat
{

    /// <summary>
    /// An interface that allows a game agent to enable or disable a vehicle control behaviour (e.g. obstacle avoidance, combat etc).
    /// </summary>
    public interface IVehicleControlBehaviour 
	{
	
		void Initialize(BehaviourBlackboard blackboard);

		void Tick();
	
	}

}
