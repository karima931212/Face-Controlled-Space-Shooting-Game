using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace VSX.UniversalVehicleCombat 
{
    /// <summary>
    /// This class manages the module mount selection UI within the loadout menu.
    /// </summary>
	public class LoadoutMountMenuController : MonoBehaviour 
	{
	
		[SerializeField]
		private GameObject moduleMountButtonPrefab;

		[SerializeField]
		private Transform moduleMountButtonsParent;

		List<ModuleMountSelectionButtonController> moduleMountSelectionButtonsList = new List<ModuleMountSelectionButtonController>();


		/// <summary>
        /// Update the module mount selection UI when a new vehicle is selected.
        /// </summary>
        /// <param name="vehicle">The newly selected vehicle.</param>
		public void UpdateMenu(Vehicle vehicle)
		{
			// Update the number of weapon mount buttons
			int diff = vehicle.ModuleMounts.Count - moduleMountSelectionButtonsList.Count;
			if (diff > 0)
			{
				for (int i = 0; i < diff; ++i)
				{
					Transform mountButtonTransform = ((GameObject)GameObject.Instantiate(moduleMountButtonPrefab, Vector3.zero, Quaternion.identity)).transform;
					mountButtonTransform.SetParent(moduleMountButtonsParent);
					mountButtonTransform.localPosition = Vector3.zero;
					mountButtonTransform.localRotation = Quaternion.identity;
					mountButtonTransform.localScale = new Vector3(1f, 1f, 1f);	
	
					ModuleMountSelectionButtonController mountButtonController = mountButtonTransform.GetComponent<ModuleMountSelectionButtonController>();
					moduleMountSelectionButtonsList.Add(mountButtonController);
					mountButtonController.SetIndex(moduleMountSelectionButtonsList.Count - 1);
				}
			}
			else
			{
				for (int i = 0; i < diff; ++i)
				{
					int nextIndex = vehicle.ModuleMounts.Count + i;
					moduleMountSelectionButtonsList[nextIndex].gameObject.SetActive(false);
				}
			}
	
			// Label and activate all the mount buttons
			for (int i = 0; i < vehicle.ModuleMounts.Count; ++i)
			{
				moduleMountSelectionButtonsList[i].SetLabel(vehicle.ModuleMounts[i].Label);
				moduleMountSelectionButtonsList[i].gameObject.SetActive(true);
			}
		}


		/// <summary>
        /// Called when the player selects a new module mount in the loadout menu.
        /// </summary>
        /// <param name="index"></param>
		public void OnSelectMount(int index)
		{
			moduleMountSelectionButtonsList[index].Select(false);

			// Deselect all the other buttons
			for (int i = 0; i < moduleMountSelectionButtonsList.Count; ++i)
			{
				if (i != index) moduleMountSelectionButtonsList[i].Deselect();
			}
		}

	}
}
