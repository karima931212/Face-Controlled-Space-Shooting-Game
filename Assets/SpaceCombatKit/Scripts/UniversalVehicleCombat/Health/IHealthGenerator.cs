using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace VSX.UniversalVehicleCombat
{

    /// <summary>
    /// Delegate to attach methods to be called when a health generator is damaged.
    /// </summary>
    /// <param name="damage">The amount of damage.</param>
    /// <param name="hitPosition">The hit position.</param>
    /// <param name="attacker">The GameAgent responsible for the attack.</param>
	public delegate void OnHealthGeneratorDamagedEventHandler(float damage, Vector3 hitPosition, GameAgent attacker);

    /// <summary>
    /// Delegate to attach methods to be called when the health value of a health generator is set.
    /// </summary>
    /// <param name="newHealthValue"></param>
	public delegate void OnHealthGeneratorSetHealthValueEventHandler(float newHealthValue);

	/// <summary>
    /// Interface for interacting with multiple implementations of health generators.
    /// </summary>
	public interface IHealthGenerator 
	{
	
		HealthType HealthType { get; }
	
		float HealthRechargeRate { get; }
	
		float StartingHealthValue { get; }
	
		float CurrentHealthValue { get; }
		
		ModuleState ModuleState { get; }
	
		float CollisionVelocityToDamageCoefficient { get; }

		void AddHealth(float addedHealthValue);

		void Damage(float damage, Vector3 hitPosition, GameAgent attacker);

		void Damage(List<float> damageValueByHealthType, Vector3 hitPosition, GameAgent attacker);

        void Heal(float healValue, Vector3 interactionPosition, GameAgent healer);

        void Heal(List<float> healValueByHealthType, Vector3 interactionPosition, GameAgent healer);

        void SetModuleState(ModuleState newState);
	
		void SetHealth (float newHealthValue);

	}
}
