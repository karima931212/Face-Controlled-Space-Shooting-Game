using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using VSX.General;



namespace VSX.UniversalVehicleCombat
{

    /// <summary>
    /// This class manages the trigger groups menu, giving the player the ability 
    /// to bind triggerable modules to input triggers at runtime.
    /// </summary>
    public class TriggerGroupsMenuController : MonoBehaviour 
	{
	
		[SerializeField]
		private GameObject menuObject;	

		[Header("Item Labels")]

		[SerializeField]
		private LabelItemController labelItemPrefab;

		[SerializeField]
		private Transform labelItemParent;

		List<LabelItemController> labelItems = new List<LabelItemController>();

		int focusedTriggerableIndex = -1;
	

		[Header("Trigger Groups")]

		[SerializeField]
		private GroupItemController groupItemPrefab;

		[SerializeField]
		private Transform groupItemParent;

		List<GroupItemController> groupItems = new List<GroupItemController>();

		int focusedGroupIndex = -1;


		[Header("Trigger items")]
	
		[SerializeField]
		private TriggerItemController triggerItemPrefab;

		private Vehicle focusedVehicle = null;
		bool hasFocusedVehicle = false;

		

		void Awake()
		{
			UVCEventManager.Instance.StartListening(UVCEventType.OnFocusedVehicleChanged, OnFocusedVehicleChanged);
			menuObject.SetActive(false);
		}
	
        /// <summary>
        /// Called when the trigger groups menu is activated in the scene.
        /// </summary>
        /// <returns>Whether the menu was successfully activated.</returns>
		public bool Activate()
		{

			if (!hasFocusedVehicle || !focusedVehicle.HasTriggerGroupsManager) return false;

			SetMenuState(true);
			menuObject.SetActive(true);

			return true;

		}


		/// <summary>
        /// Called when the trigger groups menu is deactivated in the scene.
        /// </summary>
		public void Deactivate()
		{
			SetMenuState(false);
			menuObject.SetActive(false);
		}

	
		/// <summary>
        /// Event called when the scene-focused vehicle changes.
        /// </summary>
        /// <param name="newVehicle">The new focused vehicle.</param>
		void OnFocusedVehicleChanged(Vehicle newVehicle)
		{
			
			if (newVehicle != null && newVehicle.HasTriggerGroupsManager)
			{
				focusedVehicle = newVehicle;
				hasFocusedVehicle = true;
			}
			else
			{
				focusedVehicle = null;
				hasFocusedVehicle = false;
			}
		}

	
		/// <summary>
        /// Check if the menu can be opened.
        /// </summary>
        /// <returns>Whether the menu can be opened.</returns>
		public bool CheckCanOpenMenu()
		{
			return hasFocusedVehicle;
		}


		/// <summary>
        /// Called when something on the trigger groups menu is changed.
        /// </summary>
		void OnMenuChanged()
		{

			if (!hasFocusedVehicle) return;

			// Add all the triggerables mounted on the player vehicle to the menu
			List <MountedTriggerable> mountedTriggerables = focusedVehicle.TriggerGroupsManager.mountedTriggerables;
			
			for (int i = 0; i < mountedTriggerables.Count; ++i)
			{

				int mountedModuleIndex = mountedTriggerables[i].moduleMount.MountedModuleIndex;
	
				// If the label item hasn't been created, create it
				if (i >= labelItems.Count)
				{
					LabelItemController labelItem = PoolManager.Instance.Get(labelItemPrefab.gameObject, Vector3.zero, Quaternion.identity, 
                                                                                labelItemParent).GetComponent<LabelItemController>();
					labelItem.transform.localPosition = Vector3.zero;
					labelItem.transform.localRotation = Quaternion.identity;
					labelItem.transform.localScale = new Vector3(1, 1, 1);
					
					labelItem.SetLabel(mountedTriggerables[i].moduleMount.MountableModules[mountedModuleIndex].createdModule.Label);
					labelItems.Add(labelItem);
				}

				// Update triggerable item label
				labelItems[i].SetLabel(mountedTriggerables[i].moduleMount.MountableModules[mountedModuleIndex].createdModule.Label);

			}

			
			// Add the trigger groups to the menu
			for (int i = 0; i < focusedVehicle.TriggerGroupsManager.numGroups; ++i)
			{

				// If trigger group doesn't exist at this index, create it
				if (i >= groupItems.Count)
				{

					// Add a new group item, parent it to the group item parent, and get a reference to its controller component
					GroupItemController groupItem = PoolManager.Instance.Get(groupItemPrefab.gameObject, Vector3.zero, Quaternion.identity,
                                                                                groupItemParent).GetComponent<GroupItemController>();

                    groupItem.TriggerGroupsMenuController = this;
					groupItem.transform.localPosition = Vector3.zero;
					groupItem.transform.localRotation = Quaternion.identity;
					groupItem.transform.localScale = new Vector3(1, 1, 1);
					groupItem.SetGroupIndex(i);
					groupItems.Add(groupItem);
					
					// Add the group to the menu at the correct place (index+1 because index 0 is the triggerable items list)
					groupItem.transform.SetSiblingIndex(groupItems.Count);
					
					// Add trigger items to the group item
					for (int j = 0; j < focusedVehicle.TriggerGroupsManager.mountedTriggerables.Count; ++j)
					{

						// Add a new group item
						TriggerItemController triggerItem = PoolManager.Instance.Get(triggerItemPrefab.gameObject, Vector3.zero, Quaternion.identity,
                                                                                        groupItem.CachedTransform).GetComponent<TriggerItemController>();
                        triggerItem.triggerGroupsMenuController = this;
						triggerItem.Init(j, groupItems.Count - 1, focusedVehicle.TriggerGroupsManager.GetTriggerValue(i, j));
						triggerItem.transform.localPosition = Vector3.zero;
						triggerItem.transform.localRotation = Quaternion.identity;
						triggerItem.transform.localScale = new Vector3(1, 1, 1);
						groupItem.triggerItems.Add(triggerItem);

					}
				}

				// Update the trigger values for each of the triggerables in this group
				for (int j = 0; j < groupItems[i].triggerItems.Count; ++j)
				{
					groupItems[i].triggerItems[j].SetTriggerValue (focusedVehicle.TriggerGroupsManager.GetTriggerValue(i, j));
				}
			}

			// Highlight the selected trigger group
			SelectGroup(focusedVehicle.TriggerGroupsManager.selectedTriggerGroupIndex, true);
			
		}
	

		/// <summary>
        /// Set a new menu state (activated or not).
        /// </summary>
        /// <param name="activate">Whether to activate the menu or not.</param>
		void SetMenuState(bool activate)
		{
	
			// If switching menu off ...
			if (!activate)
			{
				
				// Clear the triggerable menu items
				for (int i = 0; i < labelItems.Count; ++i)
				{
					labelItems[i].CachedGameObject.SetActive(false);
				}
				labelItems.Clear();
	
				// Dismantle the menu
				for (int i = 0; i < groupItems.Count; ++i)
				{

					// First clear all the triggerable items associated with this group item
					foreach (TriggerItemController triggerItem in groupItems[i].triggerItems)
					{
						triggerItem.CachedGameObject.SetActive(false);
					}
					groupItems[i].triggerItems.Clear();

					// Return the group item to pool
					groupItems[i].CachedGameObject.SetActive(false);
					groupItems.RemoveAt(i);
				}
			}
			// If switching menu on, update menu
			else
			{
                OnMenuChanged();
			}
		}
	

		/// <summary>
        /// Button event when a new trigger group is added to the menu.
        /// </summary>
		public void AddNewGroup()
		{
			focusedVehicle.TriggerGroupsManager.AddTriggerGroup();
            OnMenuChanged();
		}
	
	
		/// <summary>
        /// Button event called when a trigger group is removed from the menu.
        /// </summary>
		public void RemoveFocusedGroup()
		{
			
			if (focusedGroupIndex == -1)
				return;

			focusedVehicle.TriggerGroupsManager.RemoveTriggerGroup (focusedGroupIndex);
		
			// Return all the trigger items for this group to the pool
			foreach (TriggerItemController triggerItem in groupItems[focusedGroupIndex].triggerItems)
			{
				triggerItem.CachedGameObject.SetActive(false);
			}

			// return the group item to the pool
			groupItems[focusedGroupIndex].triggerItems.Clear();
			groupItems[focusedGroupIndex].CachedGameObject.SetActive(false);
			groupItems.RemoveAt(focusedGroupIndex);
	
			// Update the focused group index
			focusedGroupIndex = Mathf.Clamp(focusedGroupIndex, -1, groupItems.Count - 1);
			SelectGroup(focusedGroupIndex, true);
			focusedVehicle.TriggerGroupsManager.selectedTriggerGroupIndex = focusedGroupIndex;

		}
	
	
		/// <summary>
        /// Select a trigger group.
        /// </summary>
        /// <param name="newSelectedGroupIndex">The index of the new selected trigger group.</param>
        /// <param name="updateGroupItem">Whether to update the trigger group element UI.</param>
		public void SelectGroup(int newSelectedGroupIndex, bool updateGroupItem = false)
		{
	
			if (newSelectedGroupIndex == -1) return;
	
			// Set the selected trigger group
			focusedVehicle.TriggerGroupsManager.selectedTriggerGroupIndex = newSelectedGroupIndex;
			focusedGroupIndex = newSelectedGroupIndex;

			// Update the UI if necessary
			if (updateGroupItem) groupItems[newSelectedGroupIndex].Select(false);
	
			// Deselect all the other groups
			for (int i = 0; i < groupItems.Count; ++i)
			{
				if (i != newSelectedGroupIndex) 
					groupItems[i].Deselect();
			}

		}
	
		
		/// <summary>
        /// Event called when a trigger item is selected in the menu.
        /// </summary>
        /// <param name="newFocusedTriggerableIndex">The index of the triggerable module selected in the menu.</param>
        /// <param name="newFocusedGroupIndex">The index of the trigger group selected in the menu.</param>
		public void OnTriggerItemSelected (int newFocusedTriggerableIndex, int newFocusedGroupIndex)
		{

			// Update the focused triggerable index			
			focusedTriggerableIndex = newFocusedTriggerableIndex;

			// Update the focused group index
			focusedVehicle.TriggerGroupsManager.selectedTriggerGroupIndex = newFocusedGroupIndex;
			SelectGroup(newFocusedGroupIndex, true);
	
			// Deselect other trigger groups
			for (int i = 0; i < groupItems.Count; ++i)
			{
				for (int j = 0; j < groupItems[i].triggerItems.Count; ++j)
				{
					// If it is not the new focused triggerable item in the new focused group, deselect
					if (!(i == focusedGroupIndex && j == focusedTriggerableIndex))
					{
						groupItems[i].triggerItems[j].Deselect();
					}
				}
			}
		}
		
	
		/// <summary>
        /// Called by input to set the trigger index for the focused item on the trigger group menu.
        /// </summary>
        /// <param name="newTriggerValue">The new trigger index.</param>
		public void SetTriggerGroupTriggerValue(int newTriggerValueq)
		{

			int selectedGroupIndex = focusedVehicle.TriggerGroupsManager.selectedTriggerGroupIndex;

			// If no group and/or triggerable is selected, return
			if (selectedGroupIndex < 0 || focusedTriggerableIndex < 0) return;

			// Set the new value on the UI as well as the player vehicle's trigger groups manager
			groupItems[selectedGroupIndex].triggerItems[focusedTriggerableIndex].SetTriggerValue(newTriggerValueq);
			focusedVehicle.TriggerGroupsManager.SetTriggerValue(selectedGroupIndex, focusedTriggerableIndex, newTriggerValueq);

		}
	}
}