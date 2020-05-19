using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using VSX.UniversalVehicleCombat;

namespace VSX.UniversalVehicleCombat
{

	/// <summary>
    /// This class stores a HUD that can be loaded into the scene.
    /// </summary>
	public class LoadableHUD
	{
		public GameObject prefab;
		public HUDManager createdInstance;

		public LoadableHUD (GameObject prefab, HUDManager createdInstance)
		{
			this.prefab = prefab;
			this.createdInstance = createdInstance;
		}
	}

	/// <summary>
    /// This class manages the loading/unloading of HUDs when changing vehicles.
    /// </summary>
	public class HUDSceneManager : MonoBehaviour 
	{

		private VehicleCamera vehicleCamera;

		private Vehicle focusedVehicle;
		private bool hasFocusedVehicle;
		
		List<LoadableHUD> loadableHUDs = new List<LoadableHUD>();

		int selectedHUDIndex = -1;


		/// <summary>
        /// Get the manager component of the currently loaded HUD.
        /// </summary>
		public HUDManager LoadedHUD
		{
			get
			{
				if (selectedHUDIndex == -1)
				{
					return null;
				}
				else
				{
					return (loadableHUDs[selectedHUDIndex].createdInstance);
				}
			}
		}


		// Use this for initialization
		void Awake()
		{
			UVCEventManager.Instance.StartListening(UVCEventType.OnCameraViewChanged, OnCameraViewChanged);
			UVCEventManager.Instance.StartListening(UVCEventType.OnFocusedVehicleChanged, OnFocusedVehicleChanged);
			UVCEventManager.Instance.StartListening(UVCEventType.OnVehicleDestroyed, OnVehicleDestroyed);

			vehicleCamera = GameObject.FindObjectOfType<VehicleCamera>();
		}


        /// <summary>
        /// Event called when a vehicle is destroyed in the scene.
        /// </summary>
        /// <param name="destroyedVehicle">The destroyed vehicle.</param>
		void OnVehicleDestroyed(Vehicle destroyedVehicle){
			if (hasFocusedVehicle && (focusedVehicle == destroyedVehicle))
			{
				if (selectedHUDIndex != -1)
				{
					loadableHUDs[selectedHUDIndex].createdInstance.OnDeactivate();
                    loadableHUDs[selectedHUDIndex].createdInstance.CachedTransform.SetParent(transform);
				}
			}
		}

		
		/// <summary>
        /// Create a loadable HUD and return the list index.
        /// </summary>
        /// <param name="newHUDPrefab">The prefab for the HUD.</param>
        /// <returns></returns>
		public int CreateHUD(GameObject newHUDPrefab)
		{

			if (newHUDPrefab == null) return -1;

			for (int i = 0; i < loadableHUDs.Count; ++i)
			{
				if (loadableHUDs[i].prefab == newHUDPrefab)
				{
					return i;
				}
			}

			HUDManager newHUDInstance = GameObject.Instantiate(newHUDPrefab).GetComponent<HUDManager>();
			LoadableHUD newHUD = new LoadableHUD(newHUDPrefab, newHUDInstance);
            loadableHUDs.Add(newHUD);
			
			return (loadableHUDs.Count - 1);

		}
		
		/// <summary>
        /// Event called when the focused vehicle changes, so the HUD can be updated.
        /// </summary>
        /// <param name="newFocusedVehicle">The new focused vehicle.</param>
		public void OnFocusedVehicleChanged(Vehicle newFocusedVehicle)
		{

            // Remove the previous HUD
			if (focusedVehicle != null)
			{
				if (selectedHUDIndex != -1)
				{
                    loadableHUDs[selectedHUDIndex].createdInstance.OnDeactivate();
                    loadableHUDs[selectedHUDIndex].createdInstance.CachedTransform.SetParent(transform);
				}
			}
	
            // If there is a new focused vehicle with a HUD, load it.
			if (newFocusedVehicle != null && newFocusedVehicle.HUDPrefab != null)
			{
				
				focusedVehicle = newFocusedVehicle;
				hasFocusedVehicle = newFocusedVehicle != null;
				selectedHUDIndex = CreateHUD(focusedVehicle.HUDPrefab);
				loadableHUDs[selectedHUDIndex].createdInstance.OnActivate(newFocusedVehicle, vehicleCamera.HUDCamera.transform);
				OnCameraViewChanged(vehicleCamera.DefaultView);
			
			}
			else
			{
				focusedVehicle = null;
				hasFocusedVehicle = false;
				for (int i = 0; i < loadableHUDs.Count; ++i)
				{
                    loadableHUDs[i].createdInstance.OnDeactivate();
				}
			}
		}

		
		// Event called when the camera view is changed to configure the HUD for the new view
		void OnCameraViewChanged (VehicleCameraView newView) {
			
			if (!hasFocusedVehicle || selectedHUDIndex == -1) return;
			
			loadableHUDs[selectedHUDIndex].createdInstance.OnCameraViewChanged (newView, vehicleCamera.HUDCamera.transform);

		}
	}
}
