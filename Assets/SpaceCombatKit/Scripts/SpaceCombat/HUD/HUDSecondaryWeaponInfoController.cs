using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using VSX.General;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// This class manages the display of information on a HUD for a single secondary (missile) weapon.
    /// </summary>
    public class HUDSecondaryWeaponInfoController : MonoBehaviour 
	{
	
		private GameObject cachedGameObject;
		public GameObject CachedGameObject { get { return cachedGameObject; } }
		
		private Transform cachedTransform;
		public Transform CachedTransform { get { return cachedTransform; } }
	
		[SerializeField]
		private Text labelText;

		[SerializeField]
		private Text numAmmoText;

		[SerializeField]
		private Image lockedImage;
	
	
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
	
        /// <summary>
        /// Show on the HUD whether the weapon is locked onto a target.
        /// </summary>
        /// <param name="isLocked">Whether the weapon is locked onto a target.</param>
		public void SetIsLocked(bool isLocked)
		{
			lockedImage.enabled = isLocked;
		}

        /// <summary>
        /// Update the amount of ammunition remaining for this weapon that is shown on the HUD.
        /// </summary>
        /// <param name="numAmmo">The amount of ammunition remaining.</param>
		public void SetNumAmmo(int numAmmo)
		{
			numAmmoText.text = numAmmo.ToString();
		}
	}
}
