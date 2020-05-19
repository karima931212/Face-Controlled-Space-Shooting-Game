using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace VSX.UniversalVehicleCombat
{

	/// <summary>
    /// This class stores a triggerable module that is mounted on one of the vehicle's module mounts.
    /// </summary>
	public class MountedTriggerable 
	{
		public ModuleMount moduleMount;
		public ITriggerable triggerable;

		public int defaultTriggerValue;
		public List<int> triggerValuesByGroup;
	
		public MountedTriggerable(ModuleMount moduleMount, ITriggerable triggerable)
		{
			this.moduleMount = moduleMount;
			this.triggerable = triggerable;
			triggerValuesByGroup = new List<int>();
		}
	}
	
	
    /// <summary>
    /// This class adds trigger groups functionality to a vehicle.
    /// </summary>
	public class TriggerGroupsManager : Subsystem
	{
		
		[HideInInspector]
		public int numGroups;
	
		[HideInInspector]
		public int selectedTriggerGroupIndex = -1;
	
		[HideInInspector]
		public List<MountedTriggerable> mountedTriggerables = new List<MountedTriggerable>();

	
		void Start()
		{
			
			// Add at least one trigger group
			AddTriggerGroup();
	
		}

        
        /// <summary>
        /// Event called when a new module is mounted on one of the module mounts, to check and store
        /// it if it is an ITriggerable.
        /// </summary>
        /// <param name="moduleMount">The module mount where the new module was loaded.</param>
        protected override void OnModuleMounted(ModuleMount moduleMount)
		{
			for (int i = 0; i < mountedTriggerables.Count; ++i)
			{
				// if a weapon has already been recorded at this weapon mount
				if (mountedTriggerables[i].moduleMount == moduleMount)
				{
					mountedTriggerables.RemoveAt(i);
					break;
				}
			}
	
			int newModuleIndex = moduleMount.MountedModuleIndex;
			if (newModuleIndex == -1)
				return;
	
			ITriggerable triggerable = moduleMount.MountableModules[newModuleIndex].createdModule.CachedGameObject.GetComponent<ITriggerable>();
			if (triggerable != null)
			{
				MountedTriggerable newMountedTriggerable = new MountedTriggerable(moduleMount, triggerable);
				mountedTriggerables.Add(newMountedTriggerable);
				newMountedTriggerable.defaultTriggerValue = triggerable.DefaultTrigger;

				for (int i = 0; i < numGroups; ++i)
				{
					newMountedTriggerable.triggerValuesByGroup.Add(newMountedTriggerable.defaultTriggerValue);
				}
				
				
			}
		}


		/// <summary>
        /// Trigger all the triggerable modules at a particular trigger index.
        /// </summary>
        /// <param name="triggerIndex">The trigger index that is being triggered.</param>
		public void StartTriggeringAtIndex(int triggerIndex)
		{
			
			if (selectedTriggerGroupIndex == -1) return;

			for (int i = 0; i < mountedTriggerables.Count; ++i)
			{
				if (mountedTriggerables[i].triggerValuesByGroup[selectedTriggerGroupIndex] == triggerIndex)
				{
					mountedTriggerables[i].triggerable.StartTriggering();
				}
			}
		}


		/// <summary>
        /// Stop triggering all the triggerable modules at a particular trigger index.
        /// </summary>
        /// <param name="triggerIndex">The trigger index to stop triggering.</param>
		public void StopTriggeringAtIndex(int triggerIndex)
		{
			if (selectedTriggerGroupIndex == -1) return;
			for (int i = 0; i < mountedTriggerables.Count; ++i)
			{
				if (mountedTriggerables[i].triggerValuesByGroup[selectedTriggerGroupIndex] == triggerIndex) mountedTriggerables[i].triggerable.StopTriggering();
			}
		}


		/// <summary>
        /// Set the trigger index for a triggerable module (called by the trigger groups menu).
        /// </summary>
        /// <param name="groupIndex">The trigger group index for the newly assigned value.</param>
        /// <param name="elementIndex">The module index for the newly assigned value.</param>
        /// <param name="newTriggerValue">The new trigger index.</param>
		public void SetTriggerValue(int groupIndex, int elementIndex, int newTriggerValue)
		{		
			mountedTriggerables[elementIndex].triggerValuesByGroup[groupIndex] = newTriggerValue;
		}
	
	
		/// <summary>
        /// Get the trigger index of a triggerable module in a trigger group
        /// </summary>
        /// <param name="groupIndex">The trigger group index to look in.</param>
        /// <param name="elementIndex">The module index.</param>
        /// <returns>The trigger index.</returns>
		public int GetTriggerValue(int groupIndex, int elementIndex)
		{
			
			return (mountedTriggerables[elementIndex].triggerValuesByGroup[groupIndex]);

		}
	
	
		/// <summary>
        /// Get an array of trigger index values for a trigger group.
        /// </summary>
        /// <param name="groupIndex">The trigger group index.</param>
        /// <returns>An array of trigger index values.</returns>
		public int[] GetTriggerValues(int groupIndex)
		{
			int[] results = new int[mountedTriggerables.Count];
			
			for (int i = 0; i < mountedTriggerables.Count; ++i)
			{
				results[i] = mountedTriggerables[i].triggerValuesByGroup[groupIndex];
			}

			return results;
			
		}
	
	
		/// <summary>
        /// Add a new trigger group with default values.
        /// </summary>
		public void AddTriggerGroup()
		{
			
			for (int i = 0; i < mountedTriggerables.Count; ++i)
			{
				mountedTriggerables[i].triggerValuesByGroup.Add(mountedTriggerables[i].defaultTriggerValue);
			}

			numGroups += 1;
			
			if (selectedTriggerGroupIndex == -1)
				selectedTriggerGroupIndex = 0;
		}
	
	
		/// <summary>
        /// Remove a trigger group at a specified index.
        /// </summary>
        /// <param name="removeIndex">The index of the trigger group to remove.</param>
		public void RemoveTriggerGroup(int removeIndex)
		{
			for (int i = 0; i < mountedTriggerables.Count; ++i)
			{
				mountedTriggerables[i].triggerValuesByGroup.RemoveAt(removeIndex);
			}
			numGroups -= 1;
			selectedTriggerGroupIndex = Mathf.Clamp(selectedTriggerGroupIndex, -1, numGroups - 1);
		}
	}
}
