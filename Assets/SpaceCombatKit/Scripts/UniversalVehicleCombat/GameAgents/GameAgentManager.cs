using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace VSX.UniversalVehicleCombat
{

    /// <summary>
    /// This singleton class provides a way to keep track of and manage all the game agents in the scene.
    /// </summary>
    /// <remarks>
    /// The Game Agent Manager allows game agents to be registered and deregistered in the scene at a place where they
    /// can easily be found. This component also allows the game to focus on a game agent such that scene level
    /// components (e.g. the HUD and the camera) represent that game agent and hook into its events.
    /// </remarks>
    public class GameAgentManager : MonoBehaviour 
	{

        /// <summary>
        /// A list of all the game agents in the scene
        /// </summary>
		[HideInInspector]
		protected List <GameAgent> gameAgents = new List<GameAgent>();
		public List<GameAgent> GameAgents
		{
			get { return gameAgents; }
		}

        /// <summary>
        ///  The game agent currently focused on by the scene
        /// </summary>
		protected GameAgent focusedGameAgent;
		public GameAgent FocusedGameAgent { get { return focusedGameAgent; } }
	
		public static GameAgentManager Instance;

		
		protected virtual void Awake()
		{
			// Singleton - make sure only one instance in scene
			if (Instance == null)
			{
				Instance = this;
			}
			else
			{
				Destroy(gameObject);
			}
		}


        /// <summary>
        /// Register a new game agent in the scene.
        /// </summary>
        /// <param name="newAgent"> New game agent. </param>
        /// <param name="focusThisAgent"> Whether to focus the scene on this game agent or not. </param>
        public virtual void Register(GameAgent newAgent, bool focusThisAgent = false)
		{

			gameAgents.Add(newAgent);
			
			newAgent.onAgentChangedVehicleEventHandler += OnVehicleChanged;
			
			if (focusThisAgent)
			{ 
				SetNewFocusedGameAgent(newAgent);
			}
		}


        /// <summary>
        /// Deregister a game agent from the scene.
        /// </summary>
        /// <param name="gameAgent"> Game agent to be deregistered from the scene. </param>
        public virtual void Deregister(GameAgent gameAgent)
        {

            gameAgent.onAgentChangedVehicleEventHandler += OnVehicleChanged;

            int index = gameAgents.IndexOf(gameAgent);
            if (index != -1)
            {
                gameAgents.RemoveAt(index);
            }
        }

        /// <summary>
        /// Focus the scene on a different game agent.
        /// </summary>
        /// <param name="newFocusedGameAgent"> Game agent to be focused on. </param>
        public virtual void SetNewFocusedGameAgent(GameAgent newFocusedGameAgent)
		{
			
			focusedGameAgent = newFocusedGameAgent;
			
			if (focusedGameAgent != null)
			{
				if (focusedGameAgent.IsInVehicle)
				{
					UVCEventManager.Instance.TriggerVehicleEvent(UVCEventType.OnFocusedVehicleChanged, newFocusedGameAgent.Vehicle);
				}	
			}

			UVCEventManager.Instance.TriggerAgentEvent(UVCEventType.OnFocusedGameAgentChanged, newFocusedGameAgent);
		}

        /// <summary>
        /// Event called when a game agent changes vehicles.
        /// </summary>
        /// <param name="agent"> The Game Agent that changed vehicles. </param>
        protected virtual void OnVehicleChanged(GameAgent agent, Vehicle previousVehicle)
		{
			if (agent == focusedGameAgent)
			{
				UVCEventManager.Instance.TriggerVehicleEvent (UVCEventType.OnFocusedVehicleChanged, agent.Vehicle);
			}
		}
	}
}
