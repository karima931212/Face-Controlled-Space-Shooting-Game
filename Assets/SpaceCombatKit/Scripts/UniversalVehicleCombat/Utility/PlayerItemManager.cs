using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace VSX.UniversalVehicleCombat
{
	/// <summary>
    /// This class holds references to vehicles and modules that are shown on the loadout menu.
    /// </summary>
	public class PlayerItemManager : MonoBehaviour 
	{
		[SerializeField]
		private List<Vehicle> vehicles = new List<Vehicle>();
        /// <summary>
        /// The vehicles available on the loadout menu.
        /// </summary>
		public List<Vehicle> Vehicles { get { return vehicles; } }
	
		[SerializeField]
		private List<GameObject> allModulePrefabs = new List<GameObject>();
        /// <summary>
        /// The modules available on the loadout menu.
        /// </summary>
		public List<GameObject> AllModulePrefabs { get { return allModulePrefabs; } }
		
	}
}