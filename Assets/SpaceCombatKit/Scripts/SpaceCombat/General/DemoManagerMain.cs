using UnityEngine;
using System.Collections;
using VSX.UniversalVehicleCombat;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace VSX.UniversalVehicleCombat
{

	/// <summary>
    /// This class is the game manager for the GameplayDemo scene.
    /// </summary>
	public class DemoManagerMain : GameManager 
	{
	
		private bool attackTriggered = false;

        [SerializeField]
        private int focusedFriendlyIndex = 0;
	
		[Header("Friendlies")]
	
		[SerializeField]
		private List<GameAgent> friendlies = new List<GameAgent>();

		[SerializeField]
		private GroupManager friendlyFormationManager;

		[SerializeField]
		private Vehicle friendlyShipPrefab;

		[SerializeField]
		private Transform friendlyStartMarker;
	
		
		[Header("Enemies")]
		
		[SerializeField]
		private List<GameAgent> enemies = new List<GameAgent>();

		[SerializeField]
		private GroupManager enemyFormationManager;

		[SerializeField]
		private Vehicle enemyShipPrefab;

		[SerializeField]
		private Transform enemyStartMarker;
		
	
		[SerializeField]
		private VehicleCamera vehicleCamera;
	
		[SerializeField]
		private AudioSource warpAudio;
	
		[SerializeField]
		private AudioClip warpAudioClip;
	
		[SerializeField]
		private Text triggerAmbushText;
	
		[SerializeField]
		private PlayerItemManager itemManager;


		protected override void Awake()
		{
			base.Awake();

		}
	
		
		protected override void Start()
		{
			
			base.Start();

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.lockState = CursorLockMode.None;

			GameAgentManager.Instance.SetNewFocusedGameAgent(friendlies[focusedFriendlyIndex]);

            // Create friendlies
            for (int i = 0; i < friendlies.Count; ++i)
			{
				Vector3 pos = friendlyStartMarker.position; 
				if (i != 0)
				{
					pos = friendlyStartMarker.TransformPoint(friendlyFormationManager.GetIndexedFormationOffset(i));
				}
                Vehicle vehicle;
				if (friendlies[i].Label == "Player")
				{
					vehicle = CreatePlayerVehicle(pos, friendlyStartMarker.rotation);
                    vehicle.Engines.SetTranslationInputs(new Vector3(0f, 0f, 0.5f));
				}
				else
				{
                    vehicle = GameObject.Instantiate(friendlyShipPrefab, pos, friendlyStartMarker.rotation) as Vehicle;
                    for (int j = 0; j < vehicle.ModuleMounts.Count; ++j)
                    {
                        vehicle.ModuleMounts[j].CreateDefaultModulesAtStart = true;
                    }

				}
				
				friendlies[i].EnterVehicle(vehicle);
			}
	
            // Create enemies
			for (int i = 0; i < enemies.Count; ++i)
			{
				Vector3 pos = enemyStartMarker.position; 
				if (i != 0)
				{
					pos = enemyStartMarker.position + enemyStartMarker.TransformPoint(enemyFormationManager.GetIndexedFormationOffset(i));
				}
				Vehicle vehicle = GameObject.Instantiate(enemyShipPrefab, pos, enemyStartMarker.rotation) as Vehicle;

                for (int j = 0; j < vehicle.ModuleMounts.Count; ++j)
                {
                    vehicle.ModuleMounts[j].CreateDefaultModulesAtStart = true;

                }

                if (vehicle.HasEngines) vehicle.Engines.SetPhysicsState(VehiclePhysicsState.PositionAndRotationFrozen);
				enemies[i].EnterVehicle(vehicle);
			}
			
			// Start listening to determine when objective is complete
			UVCEventManager.Instance.StartListening(UVCEventType.OnVehicleDestroyed, OnVehicleDestroyed);
	
		}
	
		// Create the player vehicle in the scene
		Vehicle CreatePlayerVehicle(Vector3 position, Quaternion rotation)
		{
	
			int playerVehicleIndex = PlayerData.GetSelectedVehicleIndex(itemManager);
			if (playerVehicleIndex == -1)
			{
				return (GameObject.Instantiate(friendlyShipPrefab, position, friendlyStartMarker.rotation) as Vehicle);
			}
	
			// Get the player ship
			Vehicle playerVehicle = null;
			
			if (playerVehicleIndex != -1)
			{
				
				Transform vehicleTransform = ((GameObject)Instantiate(itemManager.Vehicles[playerVehicleIndex].gameObject, position, rotation)).transform;
				playerVehicle = vehicleTransform.GetComponent<Vehicle>();
				playerVehicle.name = "PlayerVehicle";
		
				List<int> selectedModuleIndexesByMount = PlayerData.GetModuleLoadout(playerVehicleIndex, itemManager);

                bool hasLoadout = false;
                for (int i = 0; i < selectedModuleIndexesByMount.Count; ++i)
                {
                    if (selectedModuleIndexesByMount[i] != -1)
                    {
                        hasLoadout = true;
                    }
                }

                // Update the vehicle loadout
                if (hasLoadout)
                {
                    for (int i = 0; i < selectedModuleIndexesByMount.Count; ++i)
                    {

                        if (selectedModuleIndexesByMount[i] == -1) continue;

                        GameObject moduleObject = GameObject.Instantiate(itemManager.AllModulePrefabs[selectedModuleIndexesByMount[i]], null) as GameObject;

                        IModule module = moduleObject.GetComponent<IModule>();
                        playerVehicle.ModuleMounts[i].AddMountableModule(module, itemManager.AllModulePrefabs[selectedModuleIndexesByMount[i]], true);

                    }
                }
                else
                {
                    for (int i = 0; i < playerVehicle.ModuleMounts.Count; ++i)
                    {
                        playerVehicle.ModuleMounts[i].CreateDefaultModulesAtStart = true;
                    }
                }
			}
            
			return playerVehicle;
		}


        /// <summary>
        /// Called when any vehicle in the scene is destroyed
        /// </summary>
        /// <param name="destroyedVehicle">The vehicle that has been destroyed.</param>
        /// <param name="attacker">The game agent responsible for destroying the vehicle.</param>
        public void OnVehicleDestroyed(Vehicle destroyedVehicle, GameAgent attacker)
		{
	
			if (destroyedVehicle.Agent == GameAgentManager.Instance.FocusedGameAgent)
			{
				StartCoroutine(WaitForGameOverMenu());
			}
		}		
	

		// Pause before opening game over menu
		IEnumerator WaitForGameOverMenu()
		{
			currentGameState = GameState.GameOver;
			yield return new WaitForSeconds(4);
			ToggleGameOverMenu();
		}
		
		// Update is called once per frame
		void Update()
		{
			Ambush();
		}
	
        
        /// <summary>
        /// Event called when player presses the main menu button in the gameover menu
        /// </summary>
		public void GoToMainMenu()
		{
			Time.timeScale = 1;
			SceneManager.LoadScene("MainMenu");
		}


        /// <summary>
        /// Event called when player presses the loadout button in the gameover menu.
        /// </summary>
        public void GoToLoadout()
		{
			Time.timeScale = 1;
			SceneManager.LoadScene("Loadout");
		}
	

        /// <summary>
        /// Event called when the player exits the pause menu.
        /// </summary>
		public void Continue()
		{
			Time.timeScale = 1;
			TogglePauseMenu();
		}
		
		
		// Ambush the player
		void Ambush()
		{
	
			if (Input.GetKeyDown(KeyCode.Backspace) && !attackTriggered)
			{
	
				attackTriggered = true;
				triggerAmbushText.enabled = false;
	
				for (int i = 0; i < enemies.Count; ++i)
				{
					if (enemies[i].Vehicle.HasEngines) enemies[i].Vehicle.Engines.SetPhysicsState(VehiclePhysicsState.PositionAndRotationFrozen);
				}
	
				float warpStartDistance = 5000;
				float warpDistance = 4650;
				
				if (GameAgentManager.Instance.FocusedGameAgent == null || !GameAgentManager.Instance.FocusedGameAgent.IsInVehicle) return;
				Vehicle vehicle = GameAgentManager.Instance.FocusedGameAgent.Vehicle;
				Vector3 pos = vehicle.CachedTransform.position + vehicle.CachedTransform.forward * warpStartDistance;
				Quaternion rot = Quaternion.LookRotation(-vehicle.CachedTransform.forward);
		
				List<Vector3> startPositions = new List<Vector3>();
				List <float> warpDelays = new List<float>();
				Vector3 offsetDirection = new Vector3(vehicle.transform.right.x, 0f, vehicle.transform.right.z);
	
				float lastWarp = 0;
				
				for (int i = 0; i < enemies.Count; ++i)
				{
					enemies[i].Vehicle.CachedTransform.position = pos + new Vector3(0f, 110, 0f) + offsetDirection * i * 60;
					enemies[i].Vehicle.CachedTransform.rotation = rot;
					startPositions.Add(enemies[i].Vehicle.CachedTransform.position);
					float delay = Random.Range(0.1f, 0.5f);
					warpDelays.Add(lastWarp + delay);
					lastWarp += delay;
				}
		
				for (int i = 0; i < enemies.Count; ++i)
				{
					StartCoroutine(IndividualWarp(enemies[i].Vehicle, warpDelays[i], 0.5f, startPositions[i], warpDistance));
				}
			}
		}
	
		
		// Warp a single enemy vehicle into the scene
		IEnumerator IndividualWarp(Vehicle warpingVehicle, float warpDelay, float warpTime, Vector3 startPosition, float warpDistance)
		{
	
			warpingVehicle.TrackableEnabled = false;
			warpingVehicle.CachedGameObject.SetActive (true);
	
			yield return new WaitForSeconds(warpDelay);
			warpAudio.PlayOneShot(warpAudioClip);
			
			float startTime = Time.time;
			while (Time.time - startTime < warpTime)
			{
	
				float warpAmount = (Time.time - startTime) / warpTime;
				
				warpingVehicle.CachedTransform.position = startPosition + warpingVehicle.CachedTransform.forward * warpAmount * warpDistance;
				yield return null;
	
			}
	
			vehicleCamera.AddNewCameraShake(0.005f, 0.1f, 0f, 0.5f);
			if (warpingVehicle.HasEngines) warpingVehicle.Engines.SetPhysicsState(VehiclePhysicsState.Unfrozen);
		
			yield return new WaitForSeconds(1);
	
			warpingVehicle.TrackableEnabled = true;
			
		}
	}
}
