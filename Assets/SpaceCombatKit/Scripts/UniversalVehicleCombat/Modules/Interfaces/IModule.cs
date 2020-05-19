using UnityEngine;
using System.Collections;
using System;

namespace VSX.UniversalVehicleCombat
{

    /// <summary>
    /// The different states that a module can be in.
    /// </summary>
	public enum ModuleState
	{
		Activated,
		Deactivated,
		Destroyed
	}
	
    /// <summary>
    /// The type of module (allows certain modules only to be mounted at particular module mounts).
    /// </summary>
	public enum ModuleType
	{
		GunWeapon,
		MissileWeapon,
		ShieldGenerator,
		ArmorGenerator,
		Powerplant,
		Utility
	}
   

	/// <summary>
    /// An interface for a module that can be loaded at a module mount.
    /// </summary>
	public interface IModule 
	{
	
		string Label { get ; }
		
		ModuleType ModuleType { get; }
		
		Sprite MenuSprite { get; }
		
		GameObject CachedGameObject { get; }

		Transform CachedTransform { get; }
		
		ModuleState ModuleState { get; }
	
		void SetModuleState (ModuleState newModuleState);

		void ResetModule();

		void OnMount(ModuleMount moduleMount);

		void OnUnmount();

	}
}
