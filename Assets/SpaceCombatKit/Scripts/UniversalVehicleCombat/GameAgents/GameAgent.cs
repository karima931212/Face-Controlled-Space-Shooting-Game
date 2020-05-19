using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace VSX.UniversalVehicleCombat
{

    /// <summary>
    /// An enum that allows the game agent to be in different states within the game
    /// </summary>
    public enum GameAgentActivationState
    {
        ActiveInScene,
        InactiveInScene,
        Dead
    }


    /// <summary>
    /// Delegate for running functions when the game agent changes vehicles
    /// </summary>
    public delegate void OnAgentChangedVehicleEventHandler(GameAgent agent, Vehicle previousVehicle);

    /// <summary>
    /// Delegate for running functions when the game agent changes vehicles
    /// </summary>
    public delegate void OnAgentExitVehicleEventHandler (Vehicle vehicle);

    /// <summary>
    /// Delegate for running functions when the game agent changes vehicles
    /// </summary>
    public delegate void OnAgentEnterVehicleEventHandler (Vehicle vehicle);

    /// <summary>
    /// Delegate for running functions when the game agent changes state
    /// </summary>
    public delegate void OnAgentChangedActivationStateEventHandler(GameAgentActivationState newActivationState);

    /// <summary>
    /// A base class for a character (human or AI).
    /// </summary>
    /// <remarks>
    /// This class provides a base class for an entity that can enter, exit and control vehicles.
    /// It provides a link between control scripts and vehicles, as well as a way to create characters independent
    /// of vehicles.
    /// </remarks>
    public class GameAgent : MonoBehaviour 
	{
		
		[Header("Identification Parameters")]

		[SerializeField]
		protected string label;
		public string Label
        {
            get { return label; }
            set { label = value; }
        }

        protected int agentID;
        public int AgentID { get { return agentID; } }

        [SerializeField]
		protected Team team;
		public Team Team { get { return team; } }

        [Header("General Parameters")]

        [SerializeField]
		private bool setFocusedAtStart = false;

		[SerializeField]
		protected Vehicle startingVehicle;

		protected Vehicle vehicle;
		public Vehicle Vehicle { get { return vehicle; } }
	
		protected bool isInVehicle;
		public bool IsInVehicle { get { return isInVehicle; } }

        [SerializeField]
        private List<IVehicleInput> inputComponents = new List<IVehicleInput>();
        private int selectedInputIndex = -1;

        [SerializeField]
        private bool controlsDisabled;
        public bool ControlsDisabled
        {
            get
            {
                return (controlsDisabled);
            }
            set
            {
                for (int i = 0; i < inputComponents.Count; ++i)
                {
                    inputComponents[i].ControlsDisabled = value;
                }
                controlsDisabled = value;
            }
        }

        protected int kills = 0;
        public int Kills { get { return kills; } }

        protected int deaths = 0;
        public int Deaths { get { return deaths; } }

		private bool registered = false;

        public OnAgentChangedActivationStateEventHandler onAgentChangedActivationStateEventHandler;
        public OnAgentChangedVehicleEventHandler onAgentChangedVehicleEventHandler;

		protected GameAgentActivationState activationState = GameAgentActivationState.ActiveInScene;
		public GameAgentActivationState ActivationState { get { return activationState; } }



		protected virtual void Awake()
		{
			inputComponents = new List<IVehicleInput>(transform.GetComponentsInChildren<IVehicleInput>());
			ControlsDisabled = controlsDisabled;
		}

		void Start()
		{

            // Register in the scene
            GameAgentManager.Instance.Register(this, setFocusedAtStart);
            registered = true;

            if (startingVehicle != null)
			{
				EnterVehicle(startingVehicle);
			}
		}


        /// <summary>
		/// Set a new agent ID.
		/// </summary>
		/// <param name="newAgentID"> New Agent ID value. </param>
        public void SetAgentID(int newAgentID)
        {
            agentID = newAgentID;
        }      


        void OnDestroy()
        {
            GameAgentManager.Instance.Deregister(this);
        }


        /// <summary>
		/// Make the game agent enter a new vehicle.
		/// </summary>
		/// <param name="newVehicle"> New vehicle (may be null to exit vehicle). </param>
        public virtual void EnterVehicle(Vehicle newVehicle)
		{
            Vehicle previousVehicle = vehicle;

			// Register if hasn't been done already
			if (!registered)
			{
				GameAgentManager.Instance.Register(this);
				registered = true;
			}
			
			// Exit last vehicle
			if (isInVehicle)
			{
				vehicle.OnAgentEnterVehicle(null);
                vehicle.onVehicleActivationStateChangedEventHandler -= OnVehicleActivationStateChanged;
			}

			vehicle = newVehicle;
			isInVehicle = vehicle != null;

			// Enter new vehicle
			if (vehicle != null)
			{
                vehicle.OnAgentEnterVehicle(this);
                vehicle.onVehicleActivationStateChangedEventHandler += OnVehicleActivationStateChanged;
            }

			// Call the changed vehicle event
			if (onAgentChangedVehicleEventHandler != null)
			{
				onAgentChangedVehicleEventHandler(this, previousVehicle);
			}
	
			// Finish the last input component
			if (selectedInputIndex != -1)
			{
				inputComponents[selectedInputIndex].Finish();
			}

			selectedInputIndex = -1;
			
			// Look for a suitable input component
			for (int i = 0; i < inputComponents.Count; ++i)
			{
				if (isInVehicle && inputComponents[i].VehicleControlClass == vehicle.VehicleControlClass)
				{
					inputComponents[i].Initialize(this);
					selectedInputIndex = i;
					inputComponents[i].Begin();
					break;
				}
			}
		}

        /// <summary>
        /// Add one to the kill count.
        /// </summary>
        public void AddKill()
        {
            kills++;
        }

        /// <summary>
		/// Add one to the death count.
		/// </summary>
		public void AddDeath()
        {
            deaths++;
        }

        /// <summary>
		/// Set the number of kills for this game agent.
		/// </summary>
		/// <param name="numKills"> New kill count value. </param>
        public void SetKills (int numKills)
        {
            kills = numKills;
        }

        /// <summary>
		/// Set the number of deaths for this game agent.
		/// </summary>
		/// <param name="numDeaths"> New deaths count value. </param>
        public void SetDeaths (int numDeaths)
        {
            deaths = numDeaths;
        }

        /// <summary>
		/// Set a new activation state for this game agent.
		/// </summary>
		/// <param name="newActivationState"> New activation state. </param>
        public virtual void SetActivationState (GameAgentActivationState newActivationState)
        {

            switch (newActivationState)
            {
                case GameAgentActivationState.ActiveInScene:
                    break;
                case GameAgentActivationState.InactiveInScene:
                    
                    // Finish the last input component
                    if (selectedInputIndex != -1)
                    {
                        inputComponents[selectedInputIndex].Finish();
                    }
                    break;

                case GameAgentActivationState.Dead:

                    // Finish the last input component
                    if (selectedInputIndex != -1)
                    {
                        inputComponents[selectedInputIndex].Finish();
                    }
                    break;
            }

            activationState = newActivationState;

            // Call the event
            if (onAgentChangedActivationStateEventHandler != null)
            {
                onAgentChangedActivationStateEventHandler(newActivationState);
            }

        }


        /// <summary>
		/// Event called when the activation state for the vehicle this game agent is in changes.
		/// </summary>
		/// <param name="newVehicleActivationState"> New activation state of the game agent's vehicle. </param>
        public virtual void OnVehicleActivationStateChanged(VehicleActivationState newVehicleActivationState)
        {
            switch (newVehicleActivationState)
            {
                case VehicleActivationState.Destroyed:

                    deaths++;

                    SetActivationState(GameAgentActivationState.Dead);
                    
                    UVCEventManager.Instance.TriggerAgentEvent(UVCEventType.OnGameAgentDied, this);
                    break;

            }
        }
    }
}
