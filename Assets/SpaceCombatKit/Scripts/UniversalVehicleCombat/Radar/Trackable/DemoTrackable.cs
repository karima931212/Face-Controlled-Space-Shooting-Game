using UnityEngine;
using System.Collections;
using VSX.UniversalVehicleCombat;


namespace VSX.UniversalVehicleCombat 
{

    /// <summary>
    /// This class is an example of how to implement the ITrackable interface, and can be added to
    /// a gameobject to make it trackable.
    /// </summary>
	public class DemoTrackable : MonoBehaviour, ITrackable 
	{
	
		[SerializeField]
		private string label = "IAmTrackable";
		public string Label { get { return label; } }
		
		private bool hasHealthInfo;
		public bool HasHealthInfo { get { return hasHealthInfo; } }

		private IHealthInfo healthInfo;
		public IHealthInfo HealthInfo { get { return healthInfo; } }

		[SerializeField]
		private Team team;
		public Team Team { get { return team; } }

		[SerializeField]
		private TrackableType trackableType;
		public TrackableType TrackableType { get { return trackableType; } }

		private bool trackableEnabled = true;
		public bool TrackableEnabled 
		{ 
			get { return trackableEnabled; } 
			set { trackableEnabled = value; }
		}

		[SerializeField]
		private bool ignoreTrackingDistance = false;
		public bool IgnoreTrackingDistance { get { return ignoreTrackingDistance; } }

		private bool hasBodyMesh = false;
		public bool HasBodyMesh { get { return hasBodyMesh; } }

		[SerializeField]
		private Mesh bodyMesh;
		public Mesh BodyMesh { get { return bodyMesh; } }

		[SerializeField]
		private Mesh hologramMesh;
		public Mesh HologramMesh { get { return hologramMesh; } }

		[SerializeField]
		private bool hasHologramMesh = false;
		public bool HasHologramMesh { get { return hasHologramMesh;	} }

		[SerializeField]
		private Texture2D hologramNormal;
		public Texture2D HologramNormal { get { return hologramNormal; } }

		private Transform cachedTransform;
		public Transform CachedTransform { get { return cachedTransform; } }

		private GameObject cachedGameObject;
		public GameObject CachedGameObject { get { return cachedGameObject; } }

		private Rigidbody cachedRigidbody;
		public Rigidbody CachedRigidbody { get { return cachedRigidbody; } }

		private bool hasRigidbody;
		public bool HasRigidbody { get { return hasRigidbody; } }

        /// <summary>
        /// Delegate to attach event functions to be called when the trackable's activation state changes
        /// </summary>
        private OnTrackableActivationStateChangedEventHandler onTrackableActivationStateChangedEventHandler;
        public event OnTrackableActivationStateChangedEventHandler OnTrackableActivationStateChangedEventHandler
        {
            add { onTrackableActivationStateChangedEventHandler += value; }
            remove { onTrackableActivationStateChangedEventHandler -= value; }
        }

        protected PhysicsInfo physicsInfo;
        /// <summary>
        /// The physics information for this trackable.
        /// </summary>
        public PhysicsInfo PhysicsInfo { get { return physicsInfo; } }



        void Awake()
		{

			healthInfo = transform.GetComponentInChildren<IHealthInfo>();
			if (healthInfo != null)
				hasHealthInfo = true;

			RadarSceneManager radarSceneManager = GameObject.FindObjectOfType<RadarSceneManager>();
			if (radarSceneManager != null)
			{
				radarSceneManager.Register(this);
			}
			
			cachedGameObject = gameObject;
			cachedTransform = transform;
			cachedRigidbody = GetComponent<Rigidbody>();
			if (cachedRigidbody != null)
				hasRigidbody = true;

            physicsInfo = new PhysicsInfo();
			

			// Try to find a mesh filter on this object and its children
			MeshFilter meshFilter = transform.GetComponentInChildren<MeshFilter>();

			if (meshFilter != null)
			{
				
				// Try to find a mesh for the body mesh
				if (bodyMesh == null)
				{
					bodyMesh = meshFilter.sharedMesh;
					hasBodyMesh = true;
				}
				else
				{
					hasBodyMesh = true;
				}
	
				// Try to find a mesh for the hologram mesh
				if (hologramMesh == null)
				{
					hologramMesh = meshFilter.sharedMesh;
					hasHologramMesh = true;				
				}
				else
				{
					hasHologramMesh = true;		
				}

				// Try to find a normal texture for the hologram mesh
				MeshRenderer meshRenderer = meshFilter.GetComponent<MeshRenderer>();
				if (meshRenderer != null)
				{
					if (hologramNormal == null && meshRenderer.material != null)
					{
						if (meshRenderer.material.HasProperty("_BumpMap"))
						{
							Texture tex = meshRenderer.material.GetTexture("_BumpMap");
							if (tex != null)
								hologramNormal = (Texture2D)tex;
						}
					}
				}
			}
		}


        /// <summary>
        /// Set whether this trackable is enabled (can be tracked).
        /// </summary>
        /// <param name="enable">Whether the trackable should be enabled or not.</param>
		public void SetTrackableEnabled(bool enable)
        {
			trackableEnabled = enable;
		}


        // Called when the gameobject is destroyed
        private void OnDestroy()
        {
            if (onTrackableActivationStateChangedEventHandler != null)
            {
                onTrackableActivationStateChangedEventHandler(TrackableActivationState.RemovedFromScene);
            }
        }
    }
}