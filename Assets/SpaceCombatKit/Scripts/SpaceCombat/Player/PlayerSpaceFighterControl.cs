using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.SceneManagement;
using System.Collections;
//using UnityStandardAssets.CrossPlatformInput;

namespace VSX.UniversalVehicleCombat
{

	/// <summary>
    /// The different control modes for a space vehicle
    /// </summary>
	public enum SpaceVehicleControlMode
	{
		SeparateAxes,
		LinkedYawRoll
	}

	/// <summary>
    /// The different input types that are available to the player.
    /// </summary>
	public enum InputType
	{
		Mouse,
		Keyboard	
	}

    
    /// <summary>
    /// This class provides an example control script for a space fighter.
    /// </summary>
    public class PlayerSpaceFighterControl : MonoBehaviour, IVehicleInput
    {

        [SerializeField]
        private VehicleControlClass vehicleControlClass = VehicleControlClass.SpaceshipFighter;
        /// <summary>
        /// The class of vehicle that this input can control.
        /// </summary>
        public VehicleControlClass VehicleControlClass { get { return vehicleControlClass; } }

        // Camera
        VehicleCamera vehicleCamera;
        bool hasVehicleCamera;

        [Header("Mouse Parameters")]

        // Mouse parameters
        [SerializeField] private float mousePitchSensitivity = 3f;
        [SerializeField] private float mouseRollSensitivity = 3f;
        [SerializeField] private float mouseYawSensitivity = 3f;
        [SerializeField] private float mouseDeadRadius = 0.025f;

        [SerializeField] private SpaceVehicleControlMode controlMode = SpaceVehicleControlMode.SeparateAxes;
        [SerializeField] private float yawRollRatio = 0.6f; // The yaw/roll ratio for a given turning input


        [Header("Input Type")]

        [SerializeField] private InputType inputType = InputType.Mouse;


        [Header("Control Settings")]

        [SerializeField]
        private Vector3 throttleSensitivity = new Vector3(1f, 1f, 0.5f);

        bool controlsDisabled = false;
        /// <summary>
        /// Enable or disable this input script.
        /// </summary>
        public bool ControlsDisabled
        {
            get { return controlsDisabled; }
            set { controlsDisabled = value; }	
		}

		private HUDSceneManager hudSceneManager;

		private PowerManagementMenuController powerManagementMenuController;
		
		private TriggerGroupsMenuController triggerGroupsMenuController;

		private GameAgent agent;
		private bool hasAgent;

		bool running = false;
		public bool Running { get { return running; } }

		private bool hasGameManager;
		GameManager gameManager;

        private bool isFreeLookMode = false;

        [SerializeField]
        private float freeLookModeSensitivity = 3;

        private bool fire1Down = false;
        private bool fire2Down = false;


        private void Awake()
        {
            fire1Down = Input.GetAxis("Fire1") > 0.5f;
            fire1Down = Input.GetAxis("Fire2") > 0.5f;
        }

        void Start()
		{

			// Get relevant components in the scene

			vehicleCamera = GameObject.FindObjectOfType<VehicleCamera>();
			hasVehicleCamera = vehicleCamera != null;

			hudSceneManager = GameObject.FindObjectOfType<HUDSceneManager>();

			powerManagementMenuController = GameObject.FindObjectOfType<PowerManagementMenuController>();

			triggerGroupsMenuController = GameObject.FindObjectOfType<TriggerGroupsMenuController>();

			gameManager = GameObject.FindObjectOfType<GameManager>();
			hasGameManager = gameManager != null;

		}



        /// <summary>
        /// Initialize this input script with the game agent reference.
        /// </summary>
        /// <param name="agent">The game agent using this input script.</param>
        public void Initialize(GameAgent agent)
		{

			this.agent = agent;
			hasAgent = agent != null;

        }

		/// <summary>
        /// Begin running this input script.
        /// </summary>
		public void Begin()
		{
			running = true;
		}


		/// <summary>
        /// Stop running this input script.
        /// </summary>
		public void Finish()
		{
			running = false;
		}

		
        // Set the control values for the vehicle
		void SetControlValues()
		{
			
			if (!agent.Vehicle.HasEngines) return;

			// Values to be passed to the ship
			float pitch = 0; 
			float yaw = 0;
			float roll = 0;

			if (!isFreeLookMode)
			{
				// Mouse Controls
				if (inputType == InputType.Mouse)
				{	
					// Get the mouse position in the viewport - with 0,0 at the center of the screen
					Vector3 mousePos = Camera.main.ScreenToViewportPoint(Input.mousePosition) - new Vector3(0.5f, 0.5f, 0f);
						
					// Get the mouse distance from the center (0,0)
					float mouseDist = Vector3.Magnitude(mousePos);
						
					// Correct it according to the dead radius
					mouseDist = Mathf.Max(mouseDist - mouseDeadRadius, 0);
					mousePos = mousePos.normalized * mouseDist;
		
					// Set the control values
					pitch = Mathf.Clamp(-mousePos.y * mousePitchSensitivity, -1f, 1f);
					if (controlMode == SpaceVehicleControlMode.LinkedYawRoll)
					{
						roll = Mathf.Clamp(-mousePos.x * mouseRollSensitivity, -1f, 1f);
						yaw = Mathf.Clamp(-roll * yawRollRatio, -1f, 1f);
					}
					else
					{

						if (Input.GetKey(KeyCode.Q)) 
							roll = 1;
						else if (Input.GetKey(KeyCode.E)) 
							roll = -1;

						yaw = Mathf.Clamp(mousePos.x * mouseYawSensitivity, -1f, 1f);
					}
				}
				else
				{
					if (controlMode == SpaceVehicleControlMode.LinkedYawRoll)
					{
						roll = -Input.GetAxis("Horizontal");
						yaw = Mathf.Clamp(-roll * yawRollRatio, -1f, 1f);
					}
					else
					{
						if (Input.GetKey(KeyCode.Q)) 
							roll = 1;
						else if (Input.GetKey(KeyCode.E)) 
							roll = -1;

						yaw = Input.GetAxis("Horizontal");
					}
					pitch = Input.GetAxis("Vertical");
				}


				// ************************** Throttle ******************************
					
				if (Input.GetKey(KeyCode.W))
				{
					agent.Vehicle.Engines.IncrementTranslationInputs(new Vector3(0f, 0f, throttleSensitivity.z * Time.deltaTime));
				}
				else if (Input.GetKey(KeyCode.S))
				{
					agent.Vehicle.Engines.IncrementTranslationInputs(new Vector3(0f, 0f, -throttleSensitivity.z * Time.deltaTime));
				}

                Vector3 nextTranslationInputs = agent.Vehicle.Engines.VehicleController.CurrentTranslationInputs;

                // Lateral translation
                if (Input.GetKey(KeyCode.D))
                {   
                    nextTranslationInputs.x = 1;
                }
                else if (Input.GetKey(KeyCode.A))
                {
                    nextTranslationInputs.x = -1;
                }
                else
                {
                    nextTranslationInputs.x = 0;                 
                }

                // Vertical translation
                if (Input.GetKey(KeyCode.Space))
                {
                    nextTranslationInputs.y = 1;
                }
                else if (Input.GetKey(KeyCode.LeftControl))
                {
                    nextTranslationInputs.y = -1;
                }
                else
                {
                    nextTranslationInputs.y = 0;
                }

                agent.Vehicle.Engines.SetTranslationInputs(nextTranslationInputs);


                if (Input.GetKeyDown(KeyCode.Tab))
				{
					agent.Vehicle.Engines.SetBoostInputs(new Vector3(0f, 0f, 1f));
				}
				else if (Input.GetKeyUp(KeyCode.Tab))
				{
					agent.Vehicle.Engines.SetBoostInputs(Vector3.zero);
				}
			}
            
			if (agent.Vehicle != null) agent.Vehicle.Engines.SetRotationInputs(new Vector3(pitch, yaw, roll));		

		}

		
		// Toggle and interact with power management menu
		void PowerManagementMenuControls()
		{

            if (!hasGameManager) return;

			if (Input.GetKeyDown(KeyCode.P))
			{
                gameManager.TogglePowerManagementMenu();
			}
			
            // Power Management menu
			if (powerManagementMenuController != null && gameManager.CurrentGameState == GameState.PowerManagementMenu){
                
				if (Input.GetKey(KeyCode.Keypad4))
				{
					powerManagementMenuController.MovePowerBallHorizontally(false);
				}
				if (Input.GetKey(KeyCode.Keypad6))
				{	
					powerManagementMenuController.MovePowerBallHorizontally(true);
				}
				if (Input.GetKey(KeyCode.Keypad8))
				{
                    powerManagementMenuController.MovePowerBallVertically(true);
				}
				if (Input.GetKey(KeyCode.Keypad2))
				{
					powerManagementMenuController.MovePowerBallVertically(false);
				}
			}
		}


		// Toggle and interact with trigger groups menu
		void TriggerGroupsMenuControls()
		{

            if (!hasGameManager) return;

			// Toggle trigger groups menu
			if (Input.GetKeyDown(KeyCode.G))
			{
				gameManager.ToggleTriggerGroupsMenu();
			}

			// Set the trigger value for a weapon in the trigger groups menu			
			if (triggerGroupsMenuController != null && gameManager.CurrentGameState == GameState.TriggerGroupsMenu){
					
				if (Input.GetKeyDown(KeyCode.Alpha0) && gameManager.CurrentGameState == GameState.TriggerGroupsMenu)
				{
					triggerGroupsMenuController.SetTriggerGroupTriggerValue(0);
				}
	
				if (Input.GetKeyDown(KeyCode.Alpha1) && gameManager.CurrentGameState == GameState.TriggerGroupsMenu)
				{
					triggerGroupsMenuController.SetTriggerGroupTriggerValue(1);
				}
			}
		}

		
        // Radar controls
		void RadarControls()
		{
	
			if (!agent.Vehicle.HasRadar)
				return;
	
            // Target in front
			if (Input.GetKeyDown(KeyCode.U))
			{
				agent.Vehicle.Radar.GetNewTarget(RadarSelectionPriority.Any, RadarSelectionMode.Front, true);
			}
	
            // Next/previous hostile target
			if (Input.GetKeyDown(KeyCode.T))
			{
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    agent.Vehicle.Radar.GetNewTarget(RadarSelectionPriority.Hostile, RadarSelectionMode.Previous, true);
                }
                else
                {
                    agent.Vehicle.Radar.GetNewTarget(RadarSelectionPriority.Hostile, RadarSelectionMode.Next, true);
                }
			}

            // Next/previous non-hostile target
            if (Input.GetKeyDown(KeyCode.F))
            {
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    agent.Vehicle.Radar.GetNewTarget(RadarSelectionPriority.NonHostile, RadarSelectionMode.Previous, true);
                }
                else
                {
                    agent.Vehicle.Radar.GetNewTarget(RadarSelectionPriority.NonHostile, RadarSelectionMode.Next, true);
                }
            }

            // Nearest hostile target
            if (Input.GetKeyDown(KeyCode.Y))
			{
				agent.Vehicle.Radar.GetNewTarget(RadarSelectionPriority.Hostile, RadarSelectionMode.Nearest, true);
			}

            // Nearest non-hostile target
            if (Input.GetKeyDown(KeyCode.H))
            {
                agent.Vehicle.Radar.GetNewTarget(RadarSelectionPriority.NonHostile, RadarSelectionMode.Nearest, true);
            }

            // 3D radar zoom out
            if (Input.GetKey(KeyCode.Comma))
			{
				if (hudSceneManager.LoadedHUD != null)
				{
					if (hudSceneManager.LoadedHUD.HasRadar3D)
					{
						hudSceneManager.LoadedHUD.Radar3D.IncrementZoom(false);
					}
				}
			}

            // 3D radar zoom in
			if (Input.GetKey(KeyCode.Period))
			{
				if (hudSceneManager.LoadedHUD != null)
				{
					if (hudSceneManager.LoadedHUD.HasRadar3D)
					{
						hudSceneManager.LoadedHUD.Radar3D.IncrementZoom(true);
					}
				}
			}
		}


        // Weapon controls
		void WeaponControls()
		{
            
			if (!agent.Vehicle.HasWeapons)
				return;
            
            // Toggle aim assist
			if (Input.GetKeyDown(KeyCode.Slash))
			{
				if (agent.Vehicle.HasWeapons)
                {
                    agent.Vehicle.Weapons.ToggleAimAssist();
				}
			}

            // Fire trigger group 0 (default is guns)
			if (Input.GetAxis("Fire1") > 0.5f && !fire1Down)
			{
                fire1Down = true;
				if (agent.Vehicle.HasTriggerGroupsManager)
				{
					agent.Vehicle.TriggerGroupsManager.StartTriggeringAtIndex(0);
				}
				else if (agent.Vehicle.HasWeapons)
				{
					agent.Vehicle.Weapons.StartFiringOnTrigger(0);
				}
			}

            // Fire trigger group 1 (default is missiles)
            if (Input.GetAxis("Fire2") > 0.5f && !fire2Down)
            {
                fire2Down = true;
				if (agent.Vehicle.HasTriggerGroupsManager)
				{
					agent.Vehicle.TriggerGroupsManager.StartTriggeringAtIndex(1);
				}
				else if (agent.Vehicle.HasWeapons)
				{
					agent.Vehicle.Weapons.StartFiringOnTrigger(1);
				}
			}

		    // Stop firing trigger group 0
			if (Input.GetAxis("Fire1") < 0.5f && fire1Down)
			{
                fire1Down = false;
				if (agent.Vehicle.HasTriggerGroupsManager)
				{
					agent.Vehicle.TriggerGroupsManager.StopTriggeringAtIndex(0);
				}
				else if (agent.Vehicle.HasWeapons)
				{
					agent.Vehicle.Weapons.StopFiringOnTrigger(0);
				}
			}

            // Stop firing trigger group 1	
            if (Input.GetAxis("Fire2") < 0.5f && fire2Down)
            {
                fire2Down = false;
				if (agent.Vehicle.HasTriggerGroupsManager)
				{
					agent.Vehicle.TriggerGroupsManager.StopTriggeringAtIndex(1);
				}
				else if (agent.Vehicle.HasWeapons)
				{
					agent.Vehicle.Weapons.StopFiringOnTrigger(1);
				}
			}
		}


        // Camera controls
		void CameraControls()
		{
			if (!hasVehicleCamera || (hasGameManager && (gameManager.CurrentGameState != GameState.Gameplay))) return;

            // Interior view
			if (Input.GetKeyDown(KeyCode.Alpha1))
			{
				vehicleCamera.SetView(VehicleCameraView.Interior);
			}
		
            // Exterior view
			if (Input.GetKeyDown(KeyCode.Alpha2))
			{
				vehicleCamera.SetView(VehicleCameraView.Exterior);
			}
	
            // Free look mode
			if (Input.GetMouseButtonDown(2))
			{
                isFreeLookMode = true;
			}
			else if (Input.GetMouseButtonUp(2))
			{
                isFreeLookMode = false;
                if (vehicleCamera.HasLookController)
                {
                    vehicleCamera.LookController.SetGimbalRotation(Quaternion.identity, Quaternion.identity);
                }
			}

            // Free look mouse controls
            if (isFreeLookMode)
            {
                vehicleCamera.LookController.Rotate(new Vector2(Input.GetAxis("Mouse X") * freeLookModeSensitivity, -Input.GetAxis("Mouse Y") * freeLookModeSensitivity));
            }
		}
       

        // Called every frame
        private void Update()
        {
            
            if (!hasAgent || !running || controlsDisabled) return;

            if (!hasGameManager || gameManager.CurrentGameState == GameState.Gameplay)
            {

                // Steering and throttle
                SetControlValues();

                // Weapons
                WeaponControls();

                // Radar 
                RadarControls();
                
                // Camera views
                CameraControls();

            }

            // Power management
            PowerManagementMenuControls();

            // Trigger bindings menu
            TriggerGroupsMenuControls();

        }
    }
}
