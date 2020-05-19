using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using VSX.General;

namespace VSX.UniversalVehicleCombat
{

    /// <summary>
    /// This class manages a UI element for a trigger item (an element that can be clicked on 
    /// in the Trigger Groups Menu to set a new trigger value) 
    /// </summary>
    public class TriggerItemController : MonoBehaviour
	{
	
		[SerializeField]
		private Text triggerValueText;
		
		[SerializeField]
		private Image buttonImage;

		[SerializeField]
		private Sprite unselectedSprite;

		[SerializeField]
		private Sprite selectedSprite;
	
		[SerializeField]
		private Color selectedTextColor;
		Color unselectedTextColor;
	
		int triggerableIndex;
		int groupIndex;
	
		int triggerValue = -1;
		public int TriggerValue { get { return triggerValue; } }

		private Transform cachedTransform;
		public Transform CachedTransform { get { return cachedTransform; } }

		private GameObject cachedGameObject;
		public GameObject CachedGameObject { get { return cachedGameObject; } }

		public TriggerGroupsMenuController triggerGroupsMenuController;




		void Awake()
		{
			cachedTransform = transform;
			cachedGameObject = gameObject;
		}


        /// <summary>
        /// Initialize the trigger item information.
        /// </summary>
        /// <param name="newTriggerableIndex">The index of the triggerable module that this trigger item represents.</param>
        /// <param name="newGroupIndex">The index of the triggerable group that this item belongs to.</param>
        /// <param name="defaultTriggerValue">The trigger index to default to.</param>
        public void Init(int newTriggerableIndex, int newGroupIndex, int defaultTriggerValue)
		{
			
			triggerableIndex = newTriggerableIndex;

			groupIndex = newGroupIndex;
	
			buttonImage.sprite = unselectedSprite;
	
			SetTriggerValue(defaultTriggerValue);

			unselectedTextColor = triggerValueText.color;
				
	
		}
	

		/// <summary>
        /// Called when this trigger item is selected in the trigger groups menu.
        /// </summary>
		public void Select()
		{
			buttonImage.sprite = selectedSprite;
			triggerValueText.text = "_";
			triggerValueText.color = selectedTextColor;
			triggerGroupsMenuController.OnTriggerItemSelected (triggerableIndex, groupIndex);
		}
	
		
		/// <summary>
        /// Called when this trigger item is no longer focused on by the UI.
        /// </summary>
		public void Deselect()
		{

			buttonImage.sprite = unselectedSprite;
			
			triggerValueText.text = triggerValue.ToString();
		
			triggerValueText.color = unselectedTextColor;

		}
	
		
		/// <summary>
        /// Set a new trigger index for the triggerable module in the trigger group
        /// </summary>
        /// <param name="newTriggerValue">The new trigger index.</param>
		public void SetTriggerValue(int newTriggerValue)
		{
			triggerValue = newTriggerValue;
			triggerValueText.text = triggerValue.ToString();
		}
	}
}