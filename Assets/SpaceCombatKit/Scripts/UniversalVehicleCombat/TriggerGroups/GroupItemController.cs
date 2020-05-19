using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using VSX.General;

namespace VSX.UniversalVehicleCombat
{

	/// <summary>
    /// This class manages a trigger group item in the trigger groups menu.
    /// </summary>
	public class GroupItemController : MonoBehaviour 
	{
	
		private Transform cachedTransform;
		public Transform CachedTransform { get { return cachedTransform; } }

		private GameObject cachedGameObject;
		public GameObject CachedGameObject { get { return cachedGameObject; } }

		[SerializeField]
		private Text groupLabelText;
	
		[SerializeField]
		private Image buttonImage;

		[SerializeField]
		private Sprite unselectedSprite;

		[SerializeField]
		private Sprite selectedSprite;
	
		[HideInInspector]
		public List<TriggerItemController> triggerItems = new List <TriggerItemController>();
		
		int index = -1;

        private TriggerGroupsMenuController triggerGroupsMenuController;
        /// <summary>
        ///  The trigger groups menu controller to call button events on.
        /// </summary>
        public TriggerGroupsMenuController TriggerGroupsMenuController { set { triggerGroupsMenuController = value; } }
	

		void Awake()
		{
			cachedTransform = transform;
			cachedGameObject = gameObject;
		}

		/// <summary>
        /// Set the trigger group index identifier.
        /// </summary>
        /// <param name="newGroupIndex">The new trigger group index value.</param>
		public void SetGroupIndex(int newGroupIndex)
		{
			index = newGroupIndex;
			groupLabelText.text = "GRP " + index.ToString();
		}
	

		/// <summary>
        /// (Button) Select this trigger group in the menu.
        /// </summary>
        /// <param name="updateMenuController">Whether to update the trigger group menu controller.</param>
		public void Select(bool updateMenuController = true)
		{
			buttonImage.sprite = selectedSprite;
			if (updateMenuController) triggerGroupsMenuController.SelectGroup(index);
		}
	

		/// <summary>
        /// Event called when this trigger group is no longer focused on my the trigger group menu.
        /// </summary>
		public void Deselect()
		{
			buttonImage.sprite = unselectedSprite;
		}
	}
}
