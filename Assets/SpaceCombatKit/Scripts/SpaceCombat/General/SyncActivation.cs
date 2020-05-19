using UnityEngine;
using System.Collections;

namespace VSX.UniversalVehicleCombat
{

    /// <summary>
    /// This class is for syncing the enabling/disabling of two objects
    /// </summary>
    public class SyncActivation : MonoBehaviour
    {
	
		[SerializeField]
		private GameObject otherGameObject;
	
		void OnEnable()
		{
			otherGameObject.SetActive(true);
		}
	
		void OnDisable()
		{
			otherGameObject.SetActive(false);
		}
	}
}
