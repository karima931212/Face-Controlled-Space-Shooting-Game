using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using VSX.General;

namespace VSX.UniversalVehicleCombat
{

	/// <summary>
    /// This class is the controller for all sorts of different projectiles of constant velocity.
    /// </summary>
	[RequireComponent(typeof(Rigidbody))]
	public class ProjectileController : MonoBehaviour, ISceneOriginUser
	{

		[Header("Prefab")]

		[SerializeField]
		private GameObject hitEffectPrefab;      
		
		[Header("Hit Scan")]

        // Collision handling
		[SerializeField]
		private HitScanMode hitScanMode = HitScanMode.FrameInterval;

		[SerializeField]
		private float hitScanInterval;

        [SerializeField]
        private Transform originReference;
        public Transform OriginReference
        {
            set
            {
                originReference = value;
                hasOriginReference = originReference != null;
            }
        }

        private bool hasOriginReference;

        Vector3 previousPosition;

        [Header("Damage")]

		[SerializeField]
		private bool damageEnabled = true;
		public bool DamageEnabled
		{
			get { return damageEnabled; }
			set { damageEnabled = value; }
		}
		
		// Specific parameters
		[SerializeField]
		private List<float> damageValuesByHealthType = new List<float>();
		public List<float> DamageValuesByHealthType { get { return damageValuesByHealthType; } }
		
		[Header("General")]

		[SerializeField]
        private float speed = 300f; 
		public float Speed { get { return speed; } }

		[SerializeField]
		private float lifeTime;
		float lifeStartTime;
        
        private ITrackable targetTrackable;		

        // Misc
		[SerializeField]
		private AudioSource audioSource;
		
        TrailRenderer trailRenderer;

		private Rigidbody cachedRigidbody;

		private Transform cachedTransform;
		public Transform CachedTransform { get { return cachedTransform; } }

		private GameObject cachedGameObject;
		public GameObject CachedGameObject { get { return cachedGameObject; } }

		private GameAgent senderAgent = null;  
		private Vehicle senderVehicle = null;

        
	
        // Called when the scene starts
		void Awake()
		{

			cachedRigidbody = GetComponent<Rigidbody>();
			cachedTransform = transform;
			cachedGameObject = gameObject;

            // Calculate the initial position
            hasOriginReference = originReference != null;
            
        }


        // Called when the component is attached to a gameobject
		void Reset()
		{
			// Make sure the damageValuesByHealthType list has the same number of items as the HealthType enum
			StaticFunctions.ResizeList (damageValuesByHealthType, System.Enum.GetNames(typeof(HealthType)).Length);
		}


        // Called when a value in the inspector changes
		void OnValidate()
		{
			// Make sure the damageValuesByHealthType list has the same number of items as the HealthType enum
			StaticFunctions.ResizeList (damageValuesByHealthType, System.Enum.GetNames(typeof(HealthType)).Length);
		}

		
        /// <summary>
        /// Update the projectile parameters when it is fired.
        /// </summary>
        /// <param name="senderVehicle"></param>
		public void SetProjectileParameters(Vehicle senderVehicle)
		{

			this.senderVehicle = senderVehicle;
			this.senderAgent = senderVehicle.Agent;

		}
        

		/// <summary>
        /// Coroutine for projectile hit scanning.
        /// </summary>
        /// <returns>null.</returns>
		IEnumerator UpdateHitScan(){
		
			yield return null;		// Wait one frame for parameters to be initialized before doing any hit scan

			int frameCount = 0;

			while (true)
			{
	
				switch (hitScanMode)
				{
					case HitScanMode.FrameInterval:

						int frameInterval = Mathf.RoundToInt(hitScanInterval);
						if (frameCount >= frameInterval){
							DoHitScan();
						}
						frameCount += 1;
						break;

					case HitScanMode.TimeInterval:

						yield return new WaitForSeconds(hitScanInterval);
						DoHitScan();
						break;

				}
				yield return null;
			}
		}
	


		/// <summary>
        /// Do a single hit scan.
        /// </summary>
		void DoHitScan()
		{
			
			RaycastHit hit;

            Vector3 raycastFromPosition = (hasOriginReference ? originReference.position : Vector3.zero) + previousPosition;

            // Scan from previous position to current position
            float scanDistance = Vector3.Distance(raycastFromPosition, cachedTransform.position);

			if (Physics.Raycast(raycastFromPosition, transform.forward, out hit, scanDistance))
			{
				// Get any Damageable component attached to the obstacle
				IDamageable damageable = hit.collider.GetComponent<IDamageable>();
				bool isDamageable = damageable != null;
					
				// Ignore if the ray somehow hit this vehicle
				bool ignore = isDamageable && (damageable.RootGameObject == senderVehicle.CachedGameObject);
	
				if (!ignore)
				{
					
					// Damage the obstacle
					if (isDamageable && damageEnabled)
						damageable.Damage(damageValuesByHealthType, hit.point, senderAgent);
	
					PoolManager.Instance.Get(hitEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
                    
					cachedGameObject.SetActive(false);

				}
			}

            previousPosition = cachedTransform.position - (hasOriginReference ? originReference.position : Vector3.zero);

        }

		

		/// <summary>
        /// Called when the gameobject is enabled (e.g. instantiated from pool)
        /// </summary>
		void OnEnable()
		{

			StartCoroutine(DisableAfterLifeTime());

			// Set the speed
			cachedRigidbody.velocity = cachedTransform.forward * speed;

            // Reset the hitscan
            previousPosition = cachedTransform.position - (hasOriginReference ? originReference.position : Vector3.zero);

            StartCoroutine(UpdateHitScan());
			
		}

        // Called when the gameobject is disabled
		void OnDisable()
		{
			StopAllCoroutines();
		}


        /// <summary>
        /// Coroutine that manages the projectile deactivation after its lifetime is over
        /// </summary>
        /// <returns>WaitForSeconds.</returns>
		IEnumerator DisableAfterLifeTime()
		{
			yield return new WaitForSeconds(lifeTime);
			cachedGameObject.SetActive(false);
		}
	}
}
