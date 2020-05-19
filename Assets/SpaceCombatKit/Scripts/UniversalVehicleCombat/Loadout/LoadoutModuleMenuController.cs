using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace VSX.UniversalVehicleCombat 
{

	/// <summary>
    /// This class manages the module selection part of the loadout menu.
    /// </summary>
	public class LoadoutModuleMenuController : MonoBehaviour 
	{
	
		[SerializeField]
		private ModuleMenuItemController moduleItemButtonPrefab;

		[SerializeField]
		private Transform moduleItemButtonParent;

		List<ModuleMenuItemController> moduleMenuItems = new List<ModuleMenuItemController>();


		/// <summary>
        /// Initialize the module selection UI with all of the module information.
        /// </summary>
        /// <param name="itemManager">A prefab that contains references to all of the module prefabs available in the module selection UI.</param>
		public void Initialize (PlayerItemManager itemManager)
		{

			// Create the module selection menu
			for (int i = 0; i < itemManager.AllModulePrefabs.Count; ++i)
			{

				ModuleMenuItemController buttonController = (ModuleMenuItemController)Instantiate(moduleItemButtonPrefab, moduleItemButtonParent);

				IModule module = itemManager.AllModulePrefabs[i].GetComponent<IModule>();
				buttonController.transform.localPosition = Vector3.zero;
				buttonController.transform.localRotation = Quaternion.identity;
				buttonController.transform.localScale = new Vector3(1, 1, 1);
				moduleMenuItems.Add(buttonController);
				buttonController.itemIndex = i;
				buttonController.SetIcon(module.MenuSprite);
				buttonController.SetLabel(module.Label);
	
				// Deselect by default
				buttonController.Deselect();
	
			}
		}

		
		/// <summary>
        /// Update the module selection menu to show only the modules that are available for a particular module mount.
        /// </summary>
        /// <param name="selectableModuleIndexes">A list of the indexes of all the modules available at a module mount.</param>
        /// <param name="mountedIndex">The index of the module currently mounted at the currently selected module mount.</param>
		public void UpdateModuleSelectionMenu(List<int> selectableModuleIndexes, int mountedIndex)
		{

			// For each of the module menu items ...
			for (int i = 0; i < moduleMenuItems.Count; ++i)
			{
				
				// Set active/inactive depending on if its index is one of the selectable module indexes
				moduleMenuItems[i].gameObject.SetActive(selectableModuleIndexes.Contains(moduleMenuItems[i].itemIndex));

				moduleMenuItems[i].Deselect();

			}

			if (mountedIndex != -1)
			{
				moduleMenuItems[mountedIndex].Select(false);
			}
			
		}


		/// <summary>
        /// Called when the player selects a new module in the module selection UI.
        /// </summary>
        /// <param name="index">The index of the newly selected module.</param>
		public void OnSelectModule(int index)
		{
			
			if (index != -1) moduleMenuItems[index].Select(false);

			// Deselect all the other buttons
			for (int i = 0; i < moduleMenuItems.Count; ++i)
			{
				if (i != index) moduleMenuItems[i].Deselect();
			}
		}

	}
}
