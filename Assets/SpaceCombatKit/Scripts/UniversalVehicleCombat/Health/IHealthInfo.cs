using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// An interface that allows access of health information regardless of how health is implemented
    /// </summary>
	public interface IHealthInfo 
	{

        float GetCurrentHealthFraction (HealthType healthType);

        float GetCurrentHealthValue (HealthType healthType);
	
		float GetStartingHealthValue (HealthType healthType);
	
	}
}
