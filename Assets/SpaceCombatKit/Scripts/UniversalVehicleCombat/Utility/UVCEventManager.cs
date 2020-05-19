using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

namespace VSX.UniversalVehicleCombat
{
	
    /// <summary>
    /// This class (which extends Unity's UnityEvent class) is an event class for GameAgent events, to which functions 
    /// can be attached to run when the event occurs.
    /// </summary>
    [System.Serializable]
	public class GameAgentEvent : UnityEvent<GameAgent> { }

    /// <summary>
    /// This class (which extends Unity's UnityEvent class) is an event class for Vehicle events, to which functions 
    /// can be attached to run when the event occurs.
    /// </summary>
	[System.Serializable]
	public class VehicleEvent : UnityEvent<Vehicle> { }

    /// <summary>
    /// This class (which extends Unity's UnityEvent class) is an event class for events involving both a Game
    /// Agent and a Vehicle, to which functions can be attached to run when the event occurs.
    /// </summary>
	[System.Serializable]
	public class VehicleAgentEvent : UnityEvent<Vehicle, GameAgent> { }

    /// <summary>
    /// This class (which extends Unity's UnityEvent class) is an event class for VehicleCameraView events, to which functions 
    /// can be attached to run when the event occurs.
    /// </summary>
	[System.Serializable]
	public class CameraViewEvent : UnityEvent<VehicleCameraView> { }

    /// <summary>
    /// This class (which extends Unity's UnityEvent class) is an event class for VehicleCameraMode events, to which functions 
    /// can be attached to run when the event occurs.
    /// </summary>
	[System.Serializable]
    public class CameraModeEvent : UnityEvent<VehicleCameraMode> { }


    /// <summary>
    /// An enum for all of the different event types that can be run.
    /// </summary>
	public enum UVCEventType
	{
		OnFocusedGameAgentChanged,
		OnFocusedVehicleChanged,
		OnCameraViewChanged,
        OnCameraModeChanged,
        OnVehicleHit,
		OnVehicleDestroyed,
		OnGameAgentDied
    }
	
	/// <summary>
    /// This class is a singleton that allows game-wide events to be listened to or broadcast by any script.
    /// </summary>
	public class UVCEventManager : MonoBehaviour 
	{
	
		private Dictionary <string, GameAgentEvent> agentEventDictionary;
        private Dictionary <string, VehicleEvent> vehicleEventDictionary;
        private Dictionary <string, VehicleAgentEvent> vehicleAgentEventDictionary;
        private Dictionary <string, CameraViewEvent> cameraViewEventDictionary;
        private Dictionary<string, CameraModeEvent> cameraModeEventDictionary;

        public static UVCEventManager Instance;
		
		
		private bool initialized = false;


        // Called when scene begins
		void Awake()
		{
			if (Instance == null)
			{
				Instance = this;
			}
			else
			{
				Destroy(gameObject);
			}

            Initialize();

        }

	    
        /// <summary>
        /// Initialize the class
        /// </summary>
		void Initialize()
		{
			agentEventDictionary = new Dictionary <string, GameAgentEvent>();
			vehicleEventDictionary = new Dictionary <string, VehicleEvent>();
			vehicleAgentEventDictionary = new Dictionary <string, VehicleAgentEvent>();
			cameraViewEventDictionary = new Dictionary <string, CameraViewEvent>();
            cameraModeEventDictionary = new Dictionary<string, CameraModeEvent>();

            initialized = true;
		}


		/// <summary>
        /// Start listening for an event, to run a GameAgent action
        /// </summary>
        /// <param name="eventType">The event type to be listened for.</param>
        /// <param name="action">The action to run when the event occurs.</param>
		public void StartListening(UVCEventType eventType, UnityAction<GameAgent> action)
		{

			if (!initialized) Initialize();

			string eventName = eventType.ToString();

			GameAgentEvent thisEvent = null;

			if (agentEventDictionary.TryGetValue(eventName, out thisEvent))
			{
				thisEvent.AddListener(action);	
			}
			else
			{
				thisEvent = new GameAgentEvent();
				thisEvent.AddListener(action);
				agentEventDictionary.Add(eventName, thisEvent);
			}
		}


        /// <summary>
        /// Start listening for an event of a specified type that contains a Vehicle reference.
        /// </summary>
        /// <param name="eventType">The event type to be listened for.</param>
        /// <param name="action">The action to run when the event occurs.</param>
        public void StartListening(UVCEventType eventType, UnityAction<Vehicle> action)
		{

			if (!initialized) Initialize();
	
			string eventName = eventType.ToString();

			VehicleEvent thisEvent = null;

			if (vehicleEventDictionary.TryGetValue(eventName, out thisEvent))
			{
				thisEvent.AddListener(action);	
			}
			else
			{
				thisEvent = new VehicleEvent();
				thisEvent.AddListener(action);
				vehicleEventDictionary.Add(eventName, thisEvent);
			}
		}


        /// <summary>
        /// Start listening for an event of a specified type that contains a Vehicle and a GameAgent reference.
        /// </summary>
        /// <param name="eventType">The event type to be listened for.</param>
        /// <param name="action">The action to run when the event occurs.</param>
        public void StartListening(UVCEventType eventType, UnityAction<Vehicle, GameAgent> action)
		{

			if (!initialized) Initialize();

			string eventName = eventType.ToString();

			VehicleAgentEvent thisEvent = null;

			if (vehicleAgentEventDictionary.TryGetValue(eventName, out thisEvent))
			{
				thisEvent.AddListener(action);	
			}
			else
			{
				thisEvent = new VehicleAgentEvent();
				thisEvent.AddListener(action);
				vehicleAgentEventDictionary.Add(eventName, thisEvent);
			}
		}


        /// <summary>
        /// Start listening for an event of a specified type that contains a VehicleCameraView reference.
        /// </summary>
        /// <param name="eventType">The event type to be listened for.</param>
        /// <param name="action">The action to run when the event occurs.</param>
        public void StartListening(UVCEventType eventType, UnityAction<VehicleCameraView> action)
		{
		
			if (!initialized) Initialize();

			string eventName = eventType.ToString();

			CameraViewEvent thisEvent = null;

			if (cameraViewEventDictionary.TryGetValue(eventName, out thisEvent))
			{
				thisEvent.AddListener(action);	
			}
			else
			{
				thisEvent = new CameraViewEvent();
				thisEvent.AddListener(action);
				cameraViewEventDictionary.Add(eventName, thisEvent);
			}
		}


        /// <summary>
        /// Start listening for an event of a specified type that contains a VehicleCameraMode reference.
        /// </summary>
        /// <param name="eventType">The event type to be listened for.</param>
        /// <param name="action">The action to run when the event occurs.</param>
        public void StartListening(UVCEventType eventType, UnityAction<VehicleCameraMode> action)
        {

            if (!initialized) Initialize();

            string eventName = eventType.ToString();

            CameraModeEvent thisEvent = null;

            if (cameraModeEventDictionary.TryGetValue(eventName, out thisEvent))
            {
                thisEvent.AddListener(action);
            }
            else
            {
                thisEvent = new CameraModeEvent();
                thisEvent.AddListener(action);
                cameraModeEventDictionary.Add(eventName, thisEvent);
            }
        }


        /// <summary>
        /// Stop listening for an event of a specified type that contains a GameAgent reference.
        /// </summary>
        /// <param name="eventType">The event type to stop listening to.</param>
        /// <param name="action">The action to be un-linked from the event.</param>
        public void StopListening(UVCEventType eventType, UnityAction<GameAgent> action)
		{

			if (!initialized) Initialize();

			string eventName = eventType.ToString();

            GameAgentEvent thisEvent = null;

			if (agentEventDictionary.TryGetValue(eventName, out thisEvent))
			{
				thisEvent.AddListener(action);	
			}
			else
			{
				thisEvent = new GameAgentEvent();
				thisEvent.AddListener(action);
				agentEventDictionary.Add(eventName, thisEvent);
			}
		}


        /// <summary>
        /// Stop listening for an event of a specified type that contains a Vehicle reference.
        /// </summary>
        /// <param name="eventType">The event type to stop listening to.</param>
        /// <param name="action">The action to be un-linked from the event.</param>
        public void StopListening(UVCEventType eventType, UnityAction<Vehicle> action)
		{

			if (!initialized) Initialize();

			string eventName = eventType.ToString();

			VehicleEvent thisEvent = null;

			if (vehicleEventDictionary.TryGetValue(eventName, out thisEvent))
			{
				thisEvent.AddListener(action);	
			}
			else
			{
				thisEvent = new VehicleEvent();
				thisEvent.AddListener(action);
				vehicleEventDictionary.Add(eventName, thisEvent);
			}
		}


        /// <summary>
        /// Stop listening for an event of a specified type that contains a Vehicle and a GameAgent reference.
        /// </summary>
        /// <param name="eventType">The event type to stop listening to.</param>
        /// <param name="action">The action to be un-linked from the event.</param>
        public void StopListening(UVCEventType eventType, UnityAction<Vehicle, GameAgent> action)
		{

			if (!initialized) Initialize();

			string eventName = eventType.ToString();

			VehicleAgentEvent thisEvent = null;

			if (vehicleAgentEventDictionary.TryGetValue(eventName, out thisEvent))
			{
				thisEvent.AddListener(action);	
			}
			else
			{
				thisEvent = new VehicleAgentEvent();
				thisEvent.AddListener(action);
				vehicleAgentEventDictionary.Add(eventName, thisEvent);
			}
		}


        /// <summary>
        /// Stop listening for an event of a specified type that contains a VehicleCameraView reference.
        /// </summary>
        /// <param name="eventType">The event type to stop listening to.</param>
        /// <param name="action">The action to be un-linked from the event.</param>
        public void StopListening(UVCEventType eventType, UnityAction<VehicleCameraView> action)
		{

			if (!initialized) Initialize();

			string eventName = eventType.ToString();

			CameraViewEvent thisEvent = null;

			if (cameraViewEventDictionary.TryGetValue(eventName, out thisEvent))
			{
				thisEvent.AddListener(action);	
			}
			else
			{
				thisEvent = new CameraViewEvent();
				thisEvent.AddListener(action);
				cameraViewEventDictionary.Add(eventName, thisEvent);
			}
		}


        /// <summary>
        /// Stop listening for an event of a specified type that contains a VehicleCameraMode reference.
        /// </summary>
        /// <param name="eventType">The event type to stop listening to.</param>
        /// <param name="action">The action to be un-linked from the event.</param>
        public void StopListening(UVCEventType eventType, UnityAction<VehicleCameraMode> action)
        {

            if (!initialized) Initialize();

            string eventName = eventType.ToString();

            CameraModeEvent thisEvent = null;

            if (cameraModeEventDictionary.TryGetValue(eventName, out thisEvent))
            {
                thisEvent.AddListener(action);
            }
            else
            {
                thisEvent = new CameraModeEvent();
                thisEvent.AddListener(action);
                cameraModeEventDictionary.Add(eventName, thisEvent);
            }
        }


        /// <summary>
        /// Trigger an event of a specified type, passing a GameAgent reference as a parameter.
        /// </summary>
        /// <param name="eventType">The type of event to trigger.</param>
        /// <param name="agent">The GameAgent reference being passed to the event.</param>
        public void TriggerAgentEvent(UVCEventType eventType, GameAgent agent = null)
		{

			if (!initialized) Initialize();

			string eventName = eventType.ToString();

            GameAgentEvent thisEvent = null;
			if (agentEventDictionary.TryGetValue(eventName, out thisEvent))
			{
				thisEvent.Invoke(agent);
			}
		}


        /// <summary>
        /// Trigger an event of a specified type, passing a Vehicle reference as a parameter.
        /// </summary>
        /// <param name="eventType">The type of event to trigger.</param>
        /// <param name="vehicle">The Vehicle reference being passed to the event.</param>
        public void TriggerVehicleEvent(UVCEventType eventType, Vehicle vehicle = null)
		{

			if (!initialized) Initialize();
			
			string eventName = eventType.ToString();

			VehicleEvent thisEvent = null;
			if (vehicleEventDictionary.TryGetValue(eventName, out thisEvent))
			{
				thisEvent.Invoke(vehicle);
			}
		}


        /// <summary>
        /// Trigger an event of a specified type, passing a Vehicle reference and a GameAgent reference as parameters.
        /// </summary>
        /// <param name="eventType">The type of event to trigger.</param>
        /// <param name="vehicle">The Vehicle reference being passed to the event.</param>
        /// <param name="agent">The GameAgent reference being passed to the event.</param>
        public void TriggerVehicleAgentEvent(UVCEventType eventType, Vehicle vehicle = null, GameAgent agent = null)
		{

			if (!initialized) Initialize();

			string eventName = eventType.ToString();

			VehicleAgentEvent thisEvent = null;
			if (vehicleAgentEventDictionary.TryGetValue(eventName, out thisEvent))
			{
				thisEvent.Invoke(vehicle, agent);
			}
		}


        /// <summary>
        /// Trigger an event of a specified type, passing a VehicleCameraView reference as a parameter.
        /// </summary>
        /// <param name="eventType">The type of event to trigger.</param>
        /// <param name="newCameraView">The VehicleCameraView reference being passed to the event.</param>
        public void TriggerCameraViewEvent(UVCEventType eventType, VehicleCameraView newCameraView)
		{

			if (!initialized) Initialize();

			string eventName = eventType.ToString();

			CameraViewEvent thisEvent = null;
			if (cameraViewEventDictionary.TryGetValue(eventName, out thisEvent))
			{
				thisEvent.Invoke(newCameraView);
			}
		}


        /// <summary>
        /// Trigger an event of a specified type, passing a VehicleCameraView reference as a parameter.
        /// </summary>
        /// <param name="eventType">The type of event to trigger.</param>
        /// <param name="newCameraMode">The VehicleCameraMode reference being passed to the event.</param>
        public void TriggerCameraModeEvent(UVCEventType eventType, VehicleCameraMode newCameraMode)
        {

            if (!initialized) Initialize();

            string eventName = eventType.ToString();

            CameraModeEvent thisEvent = null;
            if (cameraModeEventDictionary.TryGetValue(eventName, out thisEvent))
            {
                thisEvent.Invoke(newCameraMode);
            }
        }
    }
}
