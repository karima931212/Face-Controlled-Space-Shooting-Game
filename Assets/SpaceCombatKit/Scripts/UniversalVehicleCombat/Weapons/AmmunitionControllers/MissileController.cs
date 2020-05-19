using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using VSX.UniversalVehicleCombat;
using VSX.General;

namespace VSX.UniversalVehicleCombat
{

    /// <summary>
    /// The different states that a missile can be in
    /// </summary>
    public enum MissileState
	{
        None,
		Locked,
		LostLock,
		HitTarget,
		HitNotTarget
	}

    /// <summary>
    /// Delegate for attaching event functions to run when the missile is activated
    /// </summary>
    public delegate void OnMissileActivatedEventHandler();

    /// <summary>
    /// Delegate for attaching event functions to run when the missile's state changes.
    /// </summary>
    /// <param name="newMissileState">New missile state.</param>
    public delegate void OnMissileStateChangedEventHandler(MissileState newMissileState);


    /// <summary>
    /// This class provides a controller for missiles in the game.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
	public class MissileController : MonoBehaviour, IMissileThreat, ISceneOriginUser
	{
	
		
		private Rigidbody cachedRigidbody;
		public Rigidbody CachedRigidbody { get { return cachedRigidbody; } }
	
		// For deactivating missile meshes but leaving trail to fade
		private List <MeshRenderer> meshRenderers = new List<MeshRenderer>();
		
		private GameAgent senderGameAgent = null;  
		private GameObject senderRootGameObject = null;

		private Transform cachedTransform;
		public Transform CachedTransform { get { return cachedTransform; } }

		private GameObject cachedGameObject;
		public GameObject CachedGameObject { get { return cachedGameObject; } }

		[Header("Control Settings")]

		[SerializeField]
	   	private float thrust = 800f;
	
		[SerializeField]
		private float maxPitchTorque = 3f;
		
		[SerializeField]
		private float maxYawTorque = 3f;

		[SerializeField]
		private float maxRollTorque = 9f;

		public Vector3 MaxTurningTorques { get { return new Vector3(maxPitchTorque, maxYawTorque, maxRollTorque); } }

		[SerializeField]
		private bool damageEnabled = true;
		public bool DamageEnabled
		{
			get { return damageEnabled; }
			set { damageEnabled = value; }
		}

		[SerializeField]
		private List<float> damageValuesByHealthType = new List<float>();
		public List<float> DamageValuesByHealthType { get { return damageValuesByHealthType; } }

		private Vector3 controlValues = Vector3.zero;

		public float Speed { get { return StaticFunctions.CalculateMaxSpeed(GetComponent<Rigidbody>(), thrust); } }

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
		

		[Header ("Guidance Settings")]

		[SerializeField]
		private HitScanMode hitScanMode;

		[SerializeField]
		private float hitScanInterval;

		private bool hitScanEnabled = true;
		public bool HitScanEnabled
		{
			get { return hitScanEnabled; }
			set { hitScanEnabled = value; }
		}

		private bool disabled;
		public bool Disabled
		{
			get { return disabled; }
			set { disabled = value; }
		}


		[SerializeField]
		private float lockRange;
		public float LockRange { get { return lockRange; } }

		[SerializeField]
		private float maxLockAngle;
		public float MaxLockAngle { get { return maxLockAngle; } }
	
		[SerializeField]
		private float noLockLifetime;
		private float lostLockTime;

		ITrackable targetTrackable;
		Radar targetRadar = null;
		Vector3 targetLeadPosition;

		private MissileState missileState;
		public MissileState MissileState { get { return missileState; } }
		

		[Header("General Settings")]

		[SerializeField]
		private float trailFadeTime = 2f;
	
		[SerializeField]
		private float minLostLockDistance = 100f;	// Missile tends to go out of locking angle just before impact

		[SerializeField]
		private GameObject hitEffectPrefab;      
		

		[SerializeField]
		private EngineEffectsController exhaustController;
        public EngineEffectsController ExhaustController {  get { return exhaustController; } }

		[SerializeField]
		private AudioSource engineAudioSource;
	    
		Vector3 controlIntegralValues;

		Vector3 maxRotationAngles = new Vector3(360, 360, 360);

		[SerializeField]
		private Vector3 PIDCoefficients = new Vector3 (1f, 1f, 1f);

        /// <summary>
        /// Delegate for attaching event functions to run when the missile's state changes.
        /// </summary>
        public OnMissileStateChangedEventHandler onMissileStateChangedEventHandler;

        /// <summary>
        /// Delegate for attaching event functions to run when the missile is activated in the scene.
        /// </summary>
        public OnMissileActivatedEventHandler onMissileActivatedEventHandler;


        // Called when the component is added to a gameobject
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

	    
        // Awake
		void Awake()
		{
	
            // Cache components
			cachedRigidbody = GetComponent<Rigidbody>();
			cachedTransform = transform;
			cachedGameObject = gameObject;

            // Calculate the initial position
            hasOriginReference = originReference != null;
            
		}


        /// <summary>
        /// Update the missile parameters when it is fired.
        /// </summary>
        /// <param name="senderGameAgent">The game agent firing the missile.</param>
        /// <param name="senderRootGameObject">The root game object of the vehicle that fired the missile.</param>
        /// <param name="initialVelocity">The initial velocity for the missile.</param>
        /// <param name="targetTrackable">The ITrackable interface for the locked target</param>
        public void SetMissileParameters(GameAgent senderGameAgent, GameObject senderRootGameObject, Vector3 initialVelocity, ITrackable targetTrackable)
		{
			
			this.senderGameAgent = senderGameAgent;
			this.senderRootGameObject = senderRootGameObject;
			this.targetTrackable = targetTrackable;

			// Give the missile an initial speed equivalent to the speed of the firing vehicle
			cachedRigidbody.velocity = initialVelocity;
			
			if (targetTrackable != null)
			{

				missileState = MissileState.Locked;
                if (onMissileStateChangedEventHandler != null) onMissileStateChangedEventHandler(missileState);

                this.targetRadar = targetTrackable.CachedTransform.GetComponent<Radar>();
				
				if (targetRadar != null)
					targetRadar.UpdateMissileThreat(this);
			
				targetLeadPosition = StaticFunctions.GetLeadPosition(cachedTransform.position, StaticFunctions.CalculateMaxSpeed(cachedRigidbody, thrust), targetTrackable);

			}
			else
			{
				missileState = MissileState.LostLock;
				lostLockTime = Time.time;
                if (onMissileStateChangedEventHandler != null) onMissileStateChangedEventHandler(missileState);
            }

			exhaustController.Set(1, 0);
            if (onMissileActivatedEventHandler != null) onMissileActivatedEventHandler();
		}
	

		/// <summary>
        /// The hit scan coroutine for this missile.
        /// </summary>
        /// <returns>null.</returns>
		IEnumerator UpdateHitScan()
		{
		
			int frameCount = 0;
	
			while (true)
			{
				if (hitScanEnabled)
				{

					switch (hitScanMode)
					{
						case HitScanMode.FrameInterval:
	
							int frameInterval = Mathf.RoundToInt(hitScanInterval);
							if (frameCount >= frameInterval)
							{
								DoHitScan();
							}
							frameCount += 1;

							break;
	
						case HitScanMode.TimeInterval:
	
							yield return new WaitForSeconds(hitScanInterval);
							DoHitScan();

							break;
	
					}
				}

				yield return null;
			}
		}
	

		/// <summary>
        /// Do a single hit scan.
        /// </summary>
		void DoHitScan()
		{

           if (disabled)
				return;
			
			RaycastHit hit;

			Vector3 raycastFromPosition = (hasOriginReference ? originReference.position : Vector3.zero) + previousPosition;

            // Scan from previous position to current position
            float scanDistance = Vector3.Distance(raycastFromPosition, cachedTransform.position);

			if (Physics.Raycast(raycastFromPosition, cachedTransform.forward, out hit, scanDistance))
			{
				
				// Get any Damageable component attached to the obstacle
				IDamageable damageable = hit.collider.GetComponent<IDamageable>();
				bool isDamageable = damageable != null;
					
				// Ignore if the ray somehow hit this vehicle
				bool ignore = isDamageable && (damageable.RootGameObject == senderRootGameObject);
				
				if (!ignore)
				{		
					// Damage the obstacle
					if (isDamageable && damageEnabled)
					{
						damageable.Damage(damageValuesByHealthType, hit.point, senderGameAgent);

						// Call the hit event on the enemy's radar
						bool hitTarget = targetTrackable != null ? damageable.RootGameObject == targetTrackable.CachedGameObject : false;

						missileState = hitTarget ? MissileState.HitTarget : MissileState.HitNotTarget;
                        if (onMissileStateChangedEventHandler != null) onMissileStateChangedEventHandler(missileState);

                    }

					// Blow up the missile
					Destruct(hit.point);
				}
			}

            previousPosition = cachedTransform.position - (hasOriginReference ? originReference.position : Vector3.zero);

        }
	

        /// <summary>
        /// Called to make this missile destruct.
        /// </summary>
        /// <param name="impactPoint">The impact point determined by the hit scan.</param>
		public void Destruct(Vector3 impactPoint)
		{
			
			// Call the destruction event on the enemy's radar
			if (targetRadar != null) targetRadar.UpdateMissileThreat(this);

			// Clear the control values 
			controlValues = Vector3.zero;
	
			// Disable the meshes
			for (int i = 0; i < meshRenderers.Count; ++i)
			{
				meshRenderers[i].enabled = false;
			}

			if (engineAudioSource != null)
				engineAudioSource.Stop();
			
			// Freeze
			cachedRigidbody.velocity = Vector3.zero;
			cachedRigidbody.angularVelocity = Vector3.zero;

			// Make an explosion
			PoolManager.Instance.Get(hitEffectPrefab, impactPoint, cachedTransform.rotation);
			
			// Fade the exhaust
			exhaustController.FadeExhaust(trailFadeTime);
			
			StartCoroutine(WaitForTrailFade());

		}


        /// <summary>
        /// A coroutine to make the trail fade when the missile destructs, rather than disabling it immediately.
        /// </summary>
        /// <returns>WaitForSeconds.</returns>
		IEnumerator WaitForTrailFade()
		{
			// Wait for the trail to fade before returning the missile to the pool
			yield return new WaitForSeconds(trailFadeTime);
			
			cachedGameObject.SetActive(false);

		}


        // Called when the gameobject is enabled
        void OnEnable()
		{

			StopAllCoroutines();
			
			// Enable the meshes
			for (int i = 0; i < meshRenderers.Count; ++i)
			{
				meshRenderers[i].enabled = true;
			}

			// Remove any forces still acting on it
			controlValues = Vector3.zero;
			cachedRigidbody.velocity = Vector3.zero;
			cachedRigidbody.angularVelocity = Vector3.zero;

            // Reset the hitscan
            previousPosition = cachedTransform.position - (hasOriginReference ? originReference.position : Vector3.zero);

            StartCoroutine(UpdateHitScan());
	
			engineAudioSource.Play();
	
			exhaustController.Reset();
			
		}


        // Called when the gameobject is disabled
        private void OnDisable()
        {
            missileState = MissileState.None;
            if (onMissileStateChangedEventHandler != null) onMissileStateChangedEventHandler(missileState);
        }


        /// <summary>
        /// Check if the missile should still be locked onto the target.
        /// </summary>
        /// <returns>Whether the missile should still be locked onto the target.</returns>
        bool IsInLockZone()
		{

			// Check if target has evaded the missile
			Vector3 targetRelPos = cachedTransform.InverseTransformPoint(targetLeadPosition);	
			float angleToTarget = Vector3.Angle(Vector3.forward, targetRelPos);
			float distToTarget = Vector3.Distance(cachedTransform.position, targetLeadPosition);
		
			if (angleToTarget > maxLockAngle)
			{
				return (distToTarget < minLostLockDistance);
			}

			if (distToTarget > lockRange) 
				return false;

			return true;
		}


        // Called every frame
		void Update()
		{
			
			if (disabled)
				return;
			
			switch (missileState)
			{

				case MissileState.Locked:

					bool lostLock = false;

					// Check if target is still around
					if (targetTrackable == null || targetTrackable.CachedGameObject == null || !targetTrackable.CachedGameObject.activeSelf)
						lostLock = true;
					
					// Check if the missile has been evaded
					if (!IsInLockZone())
					{ 
						lostLock = true;
					}
			
					if (lostLock)
					{
						missileState = MissileState.LostLock;
						lostLockTime = Time.time;
						if (targetRadar != null) targetRadar.UpdateMissileThreat(this);
                        if (onMissileStateChangedEventHandler != null) onMissileStateChangedEventHandler(missileState);

                    }

					break;
			

				case (MissileState.LostLock):
					
					if (Time.time - lostLockTime > noLockLifetime)
					{
						missileState = MissileState.HitNotTarget;
						Destruct(transform.position);
                        if (onMissileStateChangedEventHandler != null) onMissileStateChangedEventHandler(missileState);
                    }
					break;


				default:
			
					break;

			}	

			// Intercept the target
			if (missileState == MissileState.Locked)
			{
				targetLeadPosition = StaticFunctions.GetLeadPosition(cachedTransform.position, StaticFunctions.CalculateMaxSpeed(cachedRigidbody, thrust), targetTrackable);
				Maneuvring.TurnToward (cachedTransform, targetLeadPosition, PIDCoefficients, maxRotationAngles, out controlValues, ref controlIntegralValues);
			}
			else
			{
				controlValues = Vector3.zero;
			}
		}

		// Physics update
		void FixedUpdate()
		{
            
			if (missileState == MissileState.HitTarget || missileState == MissileState.HitNotTarget)
				return;
           
            cachedRigidbody.AddRelativeTorque(Vector3.Scale(controlValues, new Vector3(maxPitchTorque, maxYawTorque, maxRollTorque)));
			cachedRigidbody.AddForce(cachedTransform.forward * thrust);
			
		}
	
	}
}
