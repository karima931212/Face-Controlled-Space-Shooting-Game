using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace VSX.UniversalVehicleCombat
{

	/// <summary>
    /// This class manages a single item in the module mount selection part of the loadout manager.
    /// </summary>
	public class ModuleMountSelectionButtonController : MonoBehaviour 
	{

		int mountIndex = -1;
	
		public Text labelText;
	
		[SerializeField]
		private Sprite selectedSprite;

		[SerializeField]
		private Sprite unselectedSprite;
	
		[SerializeField]
		private Image buttonImage;

		[SerializeField]
		private float unselectedAlpha;

		[SerializeField]
		private float selectedAlpha;
	
	
        /// <summary>
        /// Set the module mount index for this module mount UI item.
        /// </summary>
        /// <param name="newMountIndex">The module mount index for this module mount item.</param>
		public void SetIndex(int newMountIndex)
		{
			mountIndex = newMountIndex;
		}


        /// <summary>
        /// Set the label for this module mount item.
        /// </summary>
        /// <param name="newLabel">The new label for this module mount item.</param>
		public void SetLabel(string newLabel)
		{
			labelText.text = newLabel;
		}
	

        /// <summary>
        /// Button event called when this module mount item is selected in the UI.
        /// </summary>
        /// <param name="updateMenuManager">Whether to pass the event to the loadout manager.</param>
		public void Select(bool updateMenuManager)
		{
			buttonImage.sprite = selectedSprite;
			Color c = buttonImage.color;
			c.a = selectedAlpha;
			buttonImage.color = c;

			if (updateMenuManager) LoadoutManager.Instance.SelectModuleMount(mountIndex);
		}
	

        /// <summary>
        /// Called when this module mount item becomes unselected in the module mount selection UI.
        /// </summary>
		public void Deselect()
		{
	
			buttonImage.sprite = unselectedSprite;
			Color c = buttonImage.color;
			c.a = unselectedAlpha;
			buttonImage.color = c;
	
		}
	}
}