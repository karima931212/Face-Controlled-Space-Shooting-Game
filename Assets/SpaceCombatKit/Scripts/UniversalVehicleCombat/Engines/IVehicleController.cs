using UnityEngine;
using System.Collections;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// An interface that allows input to be passed to many different vehicle types. 
    /// </summary>
    /// <remarks>
    /// To be able to control many different kinds of vehicles using this kit, this interface is provided that allows
    /// input values to be passed to many different vehicle controllers for the translation (movement) and rotation 
    /// about x, y and z axes. The component that implements this interface can then use the input values to drive a 
    /// specific vehicle according to its characteristics. 
    /// </remarks>
    public interface IVehicleController 
	{
	
		void SetTranslationInputs(Vector3 newValuesByAxis);
		
		void IncrementTranslationInputs(Vector3 incrementationRatesByAxis);

		Vector3 CurrentTranslationInputs { get; }

	
		void SetRotationInputs(Vector3 newValuesByAxis);

		void IncrementRotationInputs(Vector3 incrementationRatesByAxis);

		Vector3 CurrentRotationInputs { get; }

	
		void SetBoostInputs (Vector3 newValuesByAxis);
	
		Vector3 CurrentBoostInputs { get; }

	
		Vector3 GetMaxSpeedByAxis (bool withBoost);

		bool PhysicsDisabled { get; set; }

	}
}
