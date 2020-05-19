using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using VSX.UniversalVehicleCombat;

namespace VSX.UniversalVehicleCombat
{

	/// <summary>
    /// This interface (which extends the ITriggerable interface) provides a way to interact with weapons of
    /// all kinds. It is also a base interface for the IMissileModule and IGunModule interfaces.
    /// </summary>
	public interface IWeaponModule : ITriggerable
	{

		string Label { get ; }

		List<float> DamageValueByHealthType { get; }

	}
}
