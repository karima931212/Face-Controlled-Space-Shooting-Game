using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace VSX.UniversalVehicleCombat
{

    /// <summary>
    /// The different types of subsystems that can be in a vehicle.
    /// </summary>
	public enum SubsystemType
    {
        Engines,
        Weapons,
        Health
    }

    /// <summary>
    /// This class provides a way to manage modules of a specific type that are mounted on a vehicle. It listens for
    /// module mount events and caches modules that belong to it, providing functions for interacting with these modules. It
    /// is a base class for all of the vehicle subsystem classes, such as Engines, Weapons, Health etc.
    /// </summary>
    public class Subsystem : MonoBehaviour 
	{
	
        /// <summary>
        /// Event called when a new module is mounted onto one of the vehicle's module mounts.
        /// </summary>
        /// <param name="moduleMount">The module mount where the new module was mounted.</param>
		protected virtual void OnModuleMounted(ModuleMount moduleMount)
		{
		}


        /// <summary>
        /// Initialize the subsystem with a list of all the module mounts on the vehicle, to link events.
        /// </summary>
        /// <param name="moduleMounts">The list of module mounts.</param>
		public virtual void Initialize(List<ModuleMount> moduleMounts)
		{
			for (int i = 0; i < moduleMounts.Count; ++i)
			{
				moduleMounts[i].NewModuleMountedEventHandler += OnModuleMounted;
			}
		}

        /// <summary>
        /// Reset the subsystem to starting conditions.
        /// </summary>
		public virtual void ResetSubsystem(){ }
	}
}
