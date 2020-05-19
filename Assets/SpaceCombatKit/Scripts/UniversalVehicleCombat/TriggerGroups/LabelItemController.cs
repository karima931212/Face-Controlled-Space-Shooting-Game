using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using VSX.General;

namespace VSX.UniversalVehicleCombat 
{ 

	/// <summary>
    /// This class manages a UI element for a trigger item in the trigger groups menu.
    /// </summary>
	public class LabelItemController : MonoBehaviour 
	{

		[SerializeField]
		private Text itemText;

		private Transform cachedTransform;
		public Transform CachedTransform { get { return cachedTransform; } }

		private GameObject cachedGameObject;
		public GameObject CachedGameObject { get { return cachedGameObject; } }

		

		void Awake()
		{
			cachedTransform = transform;
			cachedGameObject = gameObject;
		}


		/// <summary>
        /// Set the triggerable module label in the menu.
        /// </summary>
        /// <param name="newValue">The new label for the triggerable module in the menu.</param>
		public void SetLabel(string newValue)
		{
			itemText.text = newValue;
		}
	}
}