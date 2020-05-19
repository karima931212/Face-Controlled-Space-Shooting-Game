using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using VSX.General;

namespace VSX.UniversalVehicleCombat
{

	/// <summary> 
    /// Different vehicle control classes provide a way to link the appropriate control script to a vehicle 
    /// that the player has entered. 
    /// </summary>
	public enum VehicleControlClass
	{
		SpaceshipFighter,
        SpaceshipCapital
	}

    /// <summary>
    /// Different activation states for a vehicle.
    /// </summary>
    public enum VehicleActivationState
    {
        ActiveInScene,
        InactiveInScene,
        Destroyed
    }

    /// <summary> 
    /// Delegate for attaching events to be called when this vehicle's activation state changes. 
    /// </summary>
    public delegate void OnVehicleActivationStateChangedEventHandler(VehicleActivationState newActivationState);

	/// <summary> 
    /// This class is a base class for all kinds of vehicles. It contains cached references to all of the subsystems
    /// in the vehicle, and provides methods to enter/exit the vehicle, destroy it and other vehicle-level actions. It also
    /// implements the ITrackable interface which allows it to be tracked by radar and displayed as a target on a HUD.
    /// </summary>
	public class Vehicle : MonoBehaviour, ITrackable
	{

        /// <summary> The Game Agent that is currently controlling this Vehicle. </summary>
        protected GameAgent agent;
		public GameAgent Agent { get { return agent; } }

		/// <summary> The Team that this vehicle currently belongs to. Used by ITrackable for radar identification. </summary>
		public Team Team
        {
            get
            {
                return agent == null ? Team.Neutral : agent.Team;
            }
        }

		[Header("Vehicle")]

        /// <summary> The identifying label for this vehicle, used by the loadout menu etc. </summary>
        [SerializeField]
		protected string label;
		public string Label
        {
            get { return label; }
            set { label = value; }
        }

        private VehicleActivationState activationState;
        public VehicleActivationState ActivationState { get { return activationState;  } }

        /// <summary> Used by the Game Agent to find the right control script for this vehicle. </summary>
        [SerializeField]
		protected VehicleControlClass vehicleControlClass;
		public VehicleControlClass VehicleControlClass { get { return vehicleControlClass; }  }

        /// <summary> The prefab that is used by the HUD Scene Manager to create the HUD for this vehicle. </summary>
        [SerializeField]
		protected GameObject HUD_Prefab;
		public GameObject HUDPrefab { get { return HUD_Prefab; } }

        [SerializeField]
        protected GameObject explosionPrefab;
        
        /// <summary> A List of all the module mounts on this vehicle. List must be populated in the inspector. </summary>
        [SerializeField]
		protected List<ModuleMount> moduleMounts = new List<ModuleMount>();
		public List<ModuleMount> ModuleMounts { get { return moduleMounts; } }

        /// <summary> List of Camera View Targets for this vehicle (position and rotation targets for different camera views).</summary>
        protected List<CameraViewTarget> cameraViewTargets = new List<CameraViewTarget>();
		public List<CameraViewTarget> CameraViewTargets { get { return cameraViewTargets; } }

		[Header("Trackable")]

        /// <summary> The type of trackable object that this vehicle is. </summary>
        [SerializeField]
		protected TrackableType trackableType;
		public TrackableType TrackableType { get { return trackableType; } }

        /// <summary> The mesh that will be used to calculate the target box size for the HUD. </summary>
        [SerializeField]
		protected Mesh bodyMesh;
		public Mesh BodyMesh { get { return bodyMesh; } }

		protected bool hasBodyMesh = false;
		public bool HasBodyMesh { get { return hasBodyMesh; } }

        /// <summary> The mesh that will be used to holographically display this vehicle when it is being tracked. </summary>
        [SerializeField]
		protected Mesh hologramMesh;
		public Mesh HologramMesh { get { return hologramMesh; } }

		protected bool hasHologramMesh = false;
		public bool HasHologramMesh { get { return hasHologramMesh; } }

        /// <summary> The normal mesh that will be applied to the model that is used to holographically display this vehicle when it is being tracked. </summary>
        [SerializeField]
		protected Texture2D hologramNormal;
		public Texture2D HologramNormal { get { return hologramNormal; } }

        /// <summary> Determine whether this trackable object has any health information to be displayed on the HUD. </summary>
        public bool HasHealthInfo { get { return hasHealth; } }
		public IHealthInfo HealthInfo { get { return health; } }

        /// <summary> Switch on/off the ability for radar to be able to track this vehicle. </summary>
        private bool trackableEnabled = true;
		public bool TrackableEnabled 
		{
			get { return trackableEnabled; } 
			set { trackableEnabled = value; } 
		}

        /// <summary> Enables this object to be tracked at all times, regardless of range. </summary>
        [SerializeField]
		private bool ignoreTrackingDistance = false;
		public bool IgnoreTrackingDistance { get { return ignoreTrackingDistance; } }

        /// <summary> Attach events to be called when this trackable's activation state is changed </summary>
        private OnTrackableActivationStateChangedEventHandler onTrackableActivationStateChangedEventHandler;
        public event OnTrackableActivationStateChangedEventHandler OnTrackableActivationStateChangedEventHandler
        {
            add { onTrackableActivationStateChangedEventHandler += value; }
            remove { onTrackableActivationStateChangedEventHandler -= value; }
        }

        /// <summary> Attach events to be called when this vehicle's activation state is changed </summary>
        public OnVehicleActivationStateChangedEventHandler onVehicleActivationStateChangedEventHandler;


        /// <summary> Get the Engines subsystem of this vehicle. </summary>
        protected Engines engines;
		public Engines Engines { get { return engines; } }

		protected bool hasEngines = false;
		public bool HasEngines { get { return hasEngines; } }


        /// <summary> Get the Radar subsystem of this vehicle. </summary>
        protected Radar radar;
		public Radar Radar { get { return radar; } }

		private bool hasRadar = false;
		public bool HasRadar { get { return hasRadar; } }


        /// <summary> Get the Weapons subsystem of this vehicle. </summary>
        protected Weapons weapons;
		public Weapons Weapons { get { return weapons; } }

		protected bool hasWeapons = false;
		public bool HasWeapons { get { return hasWeapons; } }


        /// <summary> Get the Power subsystem of this vehicle. </summary>
        protected Power power;
		public Power Power { get { return power; } }

		protected bool hasPower = false;
		public bool HasPower { get { return hasPower; } }


        /// <summary> Get the Health subsystem of this vehicle. </summary>
        protected Health health;
		public Health Health { get { return health; } }
		
		protected bool hasHealth = false;
		public bool HasHealth { get { return hasHealth; } }


        /// <summary> Get the trigger groups manager for this vehicle. </summary>
        protected TriggerGroupsManager triggerGroupManager;
		public TriggerGroupsManager TriggerGroupsManager { get { return triggerGroupManager; } }

		protected bool hasTriggerGroupsManager = false;
		public bool HasTriggerGroupsManager { get { return hasTriggerGroupsManager; } }


        /// <summary> Efficiently get the root game object of this vehicle. </summary>
        protected GameObject cachedGameObject;
		public GameObject CachedGameObject { get { return cachedGameObject; } }


        /// <summary> Efficiently get the root transform of this vehicle. </summary>
        protected Transform cachedTransform;
		public Transform CachedTransform { get { return cachedTransform; } }


        /// <summary> Efficiently get the Rigidbody for this vehicle. </summary>
        protected Rigidbody cachedRigidbody;
		public Rigidbody CachedRigidbody { get { return cachedRigidbody; } }

		public bool HasRigidbody { get { return true; } }

        protected PhysicsInfo physicsInfo;
        public PhysicsInfo PhysicsInfo
        {
            get { return physicsInfo; }
            set { physicsInfo = value; }
        }

        private bool updatePhysicsInfoFromRigidbody = true;
        public bool UpdatePhysicsInfoFromRigidbody
        {
            get { return updatePhysicsInfoFromRigidbody; }
            set { updatePhysicsInfoFromRigidbody = value; }
        }


        protected virtual void Awake()
		{
			
			cachedGameObject = gameObject;
			cachedRigidbody = GetComponent<Rigidbody>();
			cachedTransform = transform;
			
			engines = GetComponent<Engines>();
			hasEngines = engines != null;
			
			weapons = GetComponent<Weapons>();
			hasWeapons = weapons != null;

			power = GetComponent<Power>();
			hasPower = power != null;

			health = GetComponent<Health>();
			hasHealth = health != null;
			
			radar = GetComponent<Radar>();
			hasRadar = radar != null;

			triggerGroupManager = GetComponent<TriggerGroupsManager>();
			hasTriggerGroupsManager = triggerGroupManager != null;

			hasHologramMesh = hologramMesh != null;
			hasBodyMesh = bodyMesh != null;

			// Get all the module mounts
			for(int i = 0; i < moduleMounts.Count; ++i)
			{
				moduleMounts[i].ModuleMountIndex = i;
				moduleMounts[i].Vehicle = this;
			}

			// Get all the module mounts
			Subsystem[] subsystems = cachedTransform.GetComponentsInChildren<Subsystem>();
			foreach (Subsystem subsystem in subsystems)
			{
				subsystem.Initialize(moduleMounts);
			}

			// Get all the camera view targets
			CameraViewTarget[] cameraViewTargetsArray = cachedTransform.GetComponentsInChildren<CameraViewTarget>();
			
			foreach (CameraViewTarget cameraViewTarget in cameraViewTargetsArray)
			{
				cameraViewTargets.Add(cameraViewTarget);
			}

            physicsInfo = new PhysicsInfo();

		}


        protected virtual void Start()
        {
            RadarSceneManager.Instance.Register(this);
        }


		/// <summary>
        /// Called when a game agent enters (or exits, if the newAgent variable is null) the vehicle.
        /// </summary>
        /// <param name="newAgent">The game agent entering the vehicle.</param>
		public virtual void OnAgentEnterVehicle(GameAgent newAgent)
		{
            agent = newAgent;
		}


        private void OnDestroy()
        {      
            if (onTrackableActivationStateChangedEventHandler != null)
            {
                onTrackableActivationStateChangedEventHandler(TrackableActivationState.RemovedFromScene);
            }
        }


        /// <summary>
        /// Set a new activation state for the vehicle.
        /// </summary>
        /// <param name="newActivationState">The new activation state.</param>
        public virtual void SetActivationState(VehicleActivationState newActivationState)
        {

            // Update the vehicle
            switch (newActivationState)
            {
                case VehicleActivationState.ActiveInScene:

                    ResetInScene();
                    break;

                case VehicleActivationState.InactiveInScene:

                    SetInactiveInScene();
                    break;

                case VehicleActivationState.Destroyed:

                    UVCEventManager.Instance.TriggerVehicleAgentEvent(UVCEventType.OnVehicleDestroyed, this, health.LastAttacker);
                    UVCEventManager.Instance.TriggerVehicleEvent(UVCEventType.OnVehicleDestroyed, this);

                    if (explosionPrefab != null)
                    {
                        PoolManager.Instance.Get(explosionPrefab, cachedTransform.position, cachedTransform.rotation);
                    }

                    SetInactiveInScene();

                    break;

            }

            // Call the event
            if (onVehicleActivationStateChangedEventHandler != null) onVehicleActivationStateChangedEventHandler(newActivationState);

        }


        /// <summary>
        /// Set this vehicle inactive in the scene (doesn't appear or interact with anything).
        /// </summary>
        protected virtual void SetInactiveInScene()
        {

            activationState = VehicleActivationState.InactiveInScene;

            // Clear control inputs
            if (hasEngines)
            {
                engines.SetTranslationInputs(Vector3.zero);
                engines.SetRotationInputs(Vector3.zero);
            }

            // Clear rigidbody
            cachedRigidbody.velocity = Vector3.zero;
            cachedRigidbody.angularVelocity = Vector3.zero;

            // Deactivate the vehicle 
            cachedGameObject.SetActive(false);
            
        }


        /// <summary>
        /// Reset the vehicle to starting condition.
        /// </summary>
        protected virtual void ResetInScene()
		{

            activationState = VehicleActivationState.ActiveInScene;

			cachedGameObject.SetActive(true);

            Subsystem[] subsystems = cachedTransform.GetComponentsInChildren<Subsystem>();
            foreach (Subsystem subsystem in subsystems)
            {
                subsystem.ResetSubsystem();
            }
		}


        // Called every physics update
        private void FixedUpdate()
        {
            if (updatePhysicsInfoFromRigidbody)
            {
                physicsInfo.position = cachedRigidbody.position;
                physicsInfo.rotation = cachedRigidbody.rotation;
                physicsInfo.velocity = cachedRigidbody.velocity;
                physicsInfo.angularVelocity = cachedRigidbody.angularVelocity;
                physicsInfo.localAngularVelocity = cachedRigidbody.transform.InverseTransformDirection(cachedRigidbody.angularVelocity);
                physicsInfo.acceleration = (cachedRigidbody.velocity - physicsInfo.lastVelocity) / Time.fixedDeltaTime;
                physicsInfo.jerk = (physicsInfo.acceleration - physicsInfo.lastAcceleration) / Time.fixedDeltaTime;

                physicsInfo.lastVelocity = physicsInfo.velocity;
                physicsInfo.lastAcceleration = physicsInfo.acceleration;
            }
        }
    }
}
