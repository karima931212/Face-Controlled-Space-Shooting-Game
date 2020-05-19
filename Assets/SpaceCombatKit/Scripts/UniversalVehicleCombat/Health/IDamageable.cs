using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using VSX.UniversalVehicleCombat;

namespace VSX.UniversalVehicleCombat
{

	/// <summary>
    /// An interface that allows damage and healing of an entity such as a vehicle
    /// </summary>
	public interface IDamageable 
	{
	
		// Access to the root gameobject where vital components are
		GameObject RootGameObject { get; }

		// Do damage
		void Damage (List<float> damageValueByHealthType, Vector3 hitPosition, GameAgent sender);

        // Heal
        void Heal(List<float> healValueByHealthType, Vector3 hitPosition, GameAgent sender);

    }
}
