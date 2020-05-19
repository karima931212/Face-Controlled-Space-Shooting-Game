using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using VSX.General;

namespace VSX.UniversalVehicleCombat
{

	/// <summary>
    /// This class manages the display of information on a HUD for a single primary (gun) weapon.
    /// </summary>
	public class HUDPrimaryWeaponInfoController : MonoBehaviour 
	{
	
		private GameObject cachedGameObject;
		public GameObject CachedGameObject { get { return cachedGameObject; } }
		
		private Transform cachedTransform;
		public Transform CachedTransform { get { return cachedTransform; } }
	
		[SerializeField]
		private Text labelText;
	

		void Awake()
		{
			cachedGameObject = gameObject;
			cachedTransform = transform;
		}

        /// <summary>
        /// Update the label of the weapon on the HUD.
        /// </summary>
        /// <param name="label">The label of the weapon.</param>
		public void SetLabel(string label)
		{
			labelText.text = label;
		}
	}
}