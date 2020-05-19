using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// This class manages the display of actual vehicles and modules in the loadout menu
    /// (not the menu graphics).
    /// </summary>
	public class LoadoutDisplayManager : MonoBehaviour 
	{

		[Header("Vehicle")]

		[SerializeField]
		private Transform vehicleDisplayParent;

		[SerializeField]
		private float vehicleDisplayRotationSpeed = 0f;
	
		[SerializeField]
		private float vehicleDropTime = 0.5f;

		[SerializeField]
		private float maxVehicleDropDistance = 0.175f;

		[SerializeField]
		private float maxVehicleDropRotation = 3f;

		[SerializeField]
		private AnimationCurve vehicleDropInCurve;
	
		Coroutine dropvehicleCoroutine;

		List<Vehicle> vehicles = new List<Vehicle>();

		private Transform focusedVehicleTransform = null;
		private Transform focusedMount = null;

		[SerializeField]
		private bool focusCameraOnMount = false;

		[SerializeField]
		private LoadoutCameraPositionController cameraPositionController;

		[SerializeField]
		private Transform cameraTransform;

		[SerializeField]
		private float cameraMoveSpeed;

		[SerializeField]
		private float cameraRotateSpeed;


		void Start()
		{
			StartCoroutine(CameraFocus());
		}


		/// <summary>
        /// Called to make a vehicle do a drop animation into the loadout menu.
        /// </summary>
		public void DropVehicle()
		{

			if (dropvehicleCoroutine != null) StopCoroutine (dropvehicleCoroutine);
	
			vehicleDisplayParent.localPosition = Vector3.zero;
			vehicleDisplayParent.localRotation = Quaternion.identity;
	
			dropvehicleCoroutine = StartCoroutine(DoVehicleDrop());
			
		}



        /// <summary>
        /// Coroutine executing a drop animation for a vehicle in the loadout menu.
        /// </summary>
        /// <returns>Null.</returns>
        IEnumerator DoVehicleDrop()
		{
	
			float startTime = Time.time;
			while (Time.time - startTime < vehicleDropTime)
			{
				float timeFraction = (Time.time - startTime)/vehicleDropTime;
	
				float nextOffsetY = vehicleDropInCurve.Evaluate(timeFraction) * maxVehicleDropDistance;
				vehicleDisplayParent.localPosition = new Vector3(0f, nextOffsetY, 0f);
	
				float nextRotX = vehicleDropInCurve.Evaluate(timeFraction) * maxVehicleDropRotation;
				vehicleDisplayParent.localRotation = Quaternion.Euler(new Vector3(nextRotX, 0f, 0f));
	
				yield return null;
			}
		}

	
        /// <summary>
        /// Create all of the vehicles that will be displayed in the Loadout menu.
        /// </summary>
        /// <param name="vehiclePrefabs">A list of all the vehicle prefabs.</param>
        /// <param name="itemManager">A prefab containing references to all the vehicles and modules available in the menu. </param>
        /// <returns>A list of all the created vehicles. </returns>
		public List<Vehicle> AddDisplayVehicles(List<Vehicle> vehiclePrefabs, PlayerItemManager itemManager)
		{

			// Add ships
			for (int i = 0; i < vehiclePrefabs.Count; ++i)
			{
	
				// Instantiate and position the vehicle

				GameObject newVehicleGameObject = (GameObject)Instantiate(vehiclePrefabs[i].gameObject, Vector3.zero, Quaternion.identity);	
				Transform newVehicleTransform = newVehicleGameObject.transform;				

				newVehicleTransform.SetParent(vehicleDisplayParent);
				newVehicleTransform.localPosition = Vector3.zero;
				newVehicleTransform.localRotation = Quaternion.identity;
				newVehicleTransform.localScale = new Vector3(1f, 1f, 1f);


				// Add the vehicle to display list
				Vehicle createdVehicle = newVehicleGameObject.GetComponent<Vehicle>();
				vehicles.Add(createdVehicle);	

				createdVehicle.CachedRigidbody.isKinematic = true;
		

				// Mount modules
				foreach (ModuleMount moduleMount in createdVehicle.ModuleMounts)
				{

					// Clear anything that's already been loaded onto the prefab as a mountable module
					moduleMount.RemoveAllMountableModules();
					
					// Add mountable modules at this mount for all compatible modules
					foreach (GameObject modulePrefab in itemManager.AllModulePrefabs)
					{
						IModule module = modulePrefab.GetComponent<IModule>();
						if (module != null)
						{
							if (moduleMount.MountableTypes.Contains(module.ModuleType))
							{
								GameObject moduleObject = GameObject.Instantiate(modulePrefab, null) as GameObject;

								IModule createdModule = moduleObject.GetComponent<IModule>();
								moduleMount.AddMountableModule(createdModule, modulePrefab);
							}
						}
					}					
				}

				// Get the loadout configuration
				List<int> moduleIndexesByMount = PlayerData.GetModuleLoadout(i, itemManager);
						
				// Mount modules on each module mount
				for (int j = 0; j < createdVehicle.ModuleMounts.Count; ++j)
				{
					// If no selection has been saved...
					if (moduleIndexesByMount[j] == -1)
					{

						// If there is no module loaded already
						if (createdVehicle.ModuleMounts[j].MountedModuleIndex == -1)
						{
							// If there is a default module referenced
							if (createdVehicle.ModuleMounts[j].DefaultModules.Count > 0)
							{
								// Load the default module
								GameObject moduleObject = GameObject.Instantiate(createdVehicle.ModuleMounts[j].DefaultModules[0], null) as GameObject;
							
								IModule module = moduleObject.GetComponent<IModule>();

								createdVehicle.ModuleMounts[j].AddMountableModule(module, createdVehicle.ModuleMounts[j].DefaultModules[0], true);
							}
							else
							{
								int firstSelectableIndex = Mathf.Clamp(0, -1, createdVehicle.ModuleMounts[j].MountableModules.Count - 1);
								if (firstSelectableIndex != -1)
								{
									createdVehicle.ModuleMounts[j].MountModule(firstSelectableIndex);
										
								}
							}
						}
					}
					else
					{
						// Load the module according to the saved configuration
						for (int k = 0; k < createdVehicle.ModuleMounts[j].MountableModules.Count; ++k)
						{
							if (createdVehicle.ModuleMounts[j].MountableModules[k].modulePrefab == itemManager.AllModulePrefabs[moduleIndexesByMount[j]])
							{
								createdVehicle.ModuleMounts[j].MountModule(k);
								break;
							}
						}
		
						
					}
				}	

				// Deactivate the vehicle
				newVehicleGameObject.SetActive(false);
				
			}

			return vehicles;
		}

        /// <summary>
        /// Event called when a vehicle is selected in the loadout menu.
        /// </summary>
        /// <param name="newSelectionIndex">The index of the newly selected vehicle.</param>
        /// <param name="previousSelectionIndex">The index of the previously selected vehicle.</param>
        /// <param name="itemManager">A prefab containing references to all the vehicles and modules available in the menu. </param>
        public void OnVehicleSelection(int newSelectionIndex, int previousSelectionIndex, PlayerItemManager itemManager)
		{

			// Disable the last ship
			if (previousSelectionIndex != -1)
			{ 
				vehicles[previousSelectionIndex].CachedGameObject.SetActive(false);
			}
			
			// Activate the new ship
			vehicles[newSelectionIndex].CachedGameObject.SetActive(true);

			// Do the drop animation
			DropVehicle();

			focusedVehicleTransform = vehicles[newSelectionIndex].CachedTransform;

			cameraPositionController.OnVehicleSelection(vehicles[newSelectionIndex]);
			
		}


		/// <summary>
        /// Event called when a new module is selected in the module selection part of the loadout menu, to mount
        /// the new module on the display vehicle.
        /// </summary>
        /// <param name="vehicleIndex">The index of the vehicle on which to mount the new module.</param>
        /// <param name="mountIndex">The index of the module mount at which to load the new module.</param>
        /// <param name="moduleIndex">The index of the newly selected module.</param>
		public void OnModuleSelection(int vehicleIndex, int mountIndex, int moduleIndex)
		{
			
			if (vehicleIndex == -1 || mountIndex == -1)
				return;

			if (moduleIndex != -1)
			{
				vehicles[vehicleIndex].ModuleMounts[mountIndex].MountModule(moduleIndex);
			}
		}


        /// <summary>
        /// Event called when a different module mount is focused on in the loadout menu.
        /// </summary>
        /// <param name="moduleMount">The new module mount to focus on.</param>
        public void FocusModuleMount(ModuleMount moduleMount)
		{
			if (moduleMount != null)
			{
				focusedMount = moduleMount.CachedTransform;
			}
			else
			{
				focusedMount = null;
			}
		}


		/// <summary>
        /// Coroutine for focusing the camera on a module mount on a display vehicle in the loadout menu.
        /// </summary>
        /// <returns>Null.</returns>
		IEnumerator CameraFocus()
		{
	
			while (true)
			{
		
				Vector3 targetPosition;
				Vector3 lookPosition;
				
				if (!focusCameraOnMount || focusedVehicleTransform == null || focusedMount == null)
				{
					targetPosition = cameraPositionController.CachedTransform.position;
					lookPosition = cameraPositionController.CachedTransform.position + cameraPositionController.CachedTransform.forward * 1000;
					transform.Rotate(new Vector3(0f, vehicleDisplayRotationSpeed * Time.deltaTime, 0f));
				}
				else
				{
					targetPosition = focusedMount.position + (focusedMount.position - focusedVehicleTransform.position).normalized * 2 + focusedMount.up * 2;
					lookPosition = focusedMount.position;
					transform.rotation = Quaternion.Euler(0f, 180f, 0f);
				}
				
				cameraTransform.position = Vector3.Lerp(cameraTransform.position, targetPosition, cameraMoveSpeed * Time.deltaTime);
				cameraTransform.LookAt(lookPosition);//rotation = Quaternion.Lerp(cameraTransform.rotation, targetRotation, cameraRotateSpeed * Time.deltaTime);

				yield return null;

			}
		}
	}
}
