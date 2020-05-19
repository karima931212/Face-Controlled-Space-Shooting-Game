using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace VSX.UniversalVehicleCombat
{

	/// <summary>
    /// This class represents an entry point for damage and health regeneration for a vehicle, mainly consisting of a collider with
    /// hit event delegates.
    /// </summary>
	public class HealthFixture : MonoBehaviour, IDamageable
	{

		// This is set by the Health subsystem component for fast lookup
		private int index;
		public int Index { get { return index; } }

		// Enable/disable damageable property
		private bool isDamageable = true;
		public bool IsDamageable
		{
			get { return isDamageable; }
			set { isDamageable = value; }
		}

		[SerializeField]
		private HealthType healthType;
		public HealthType HealthType { get { return healthType; } }

		[SerializeField]
		private Collider cachedCollider;
		public Collider CachedCollider { get { return cachedCollider; } } 
	
		[SerializeField]
		private MeshRenderer cachedMeshRenderer;
		public MeshRenderer CachedMeshRenderer { get { return cachedMeshRenderer; } }

        private bool hasMeshRenderer;

		// This is a way for a weapon to access the root gameobject of an IDamageable object, where all the core components are
		private GameObject rootGameObject;
		public GameObject RootGameObject
		{
			get { return rootGameObject; }
			set { rootGameObject = value; }
		}

		// Enable/disable the collider/renderer
		private bool isActivated;
		public bool IsActivated { get { return isActivated; } } 

		[SerializeField]
		private List<HealthFixture> embeddedHealthFixtures = new List<HealthFixture>();
		public List<HealthFixture> EmbeddedHealthFixtures { get { return embeddedHealthFixtures; } }

		// Delegate for calling functions when the health fixture is damaged
		public delegate void OnDamageEventHandler (List<float> damageValueByHealthType, Vector3 hitPosition, GameAgent attacker);
		public OnDamageEventHandler onDamageEventHandler;

        // Delegate for calling functions when the health fixture is healed
        public delegate void OnHealEventHandler(List<float> healValuesByHealthType, Vector3 hitPosition, GameAgent attacker);
        public OnHealEventHandler onHealEventHandler;

        [Header("Effects")]

        [SerializeField]
        private float damageEffectMultiplier = 0.1f;

        [SerializeField]
        private float healEffectMultiplier = 0.1f;

        [SerializeField]
        private float effectFadeSpeed = 1f;

        [SerializeField]
        private float mergeEffectDistance;

        // This is a list of the hit effect data that is passed to the shader to carry out hit effects.
        // First three values correspond to the effect position (local to the shield mesh), fourth value is the effect strength.
        private List<Vector4> effectPositions = new List<Vector4>();



        void OnValidate()
		{
			CheckEmbeddedHierarchy();
		}


		void Awake()
		{

			CheckEmbeddedHierarchy();

			if (cachedCollider == null)
			{
				Debug.LogError("HealthFixture component requires a collider, please add one in the inspector");
			}
			else
			{
				cachedCollider.enabled = false;
			}

            // Initialize all of the effects to zero
            for (int i = 0; i < 10; ++i)
            {
                effectPositions.Add(Vector4.zero);
            }

            hasMeshRenderer = cachedMeshRenderer != null;
        }

        /// <summary>
        /// Make sure that embedded health fixtures are only of lower rank, to prevent recursive activation/deactivation.
        /// </summary>
        void CheckEmbeddedHierarchy()
		{
			for (int i = 0; i < embeddedHealthFixtures.Count; ++i)
			{
				if (embeddedHealthFixtures[i] == null) continue;
				if ((int) embeddedHealthFixtures[i].HealthType >= (int) healthType)
				{
					Debug.LogError("Embedded health fixtures must be of lower HealthType value. Clearing reference to prevent recursive function calls.");
					embeddedHealthFixtures[i] = null;
				}
			}
		}

		
        /// <summary>
        /// Set the health fixture index.
        /// </summary>
        /// <param name="newIndex">Index for this health fixture.</param>
		public void SetIndex(int newIndex)
		{
			index = newIndex;
		}


		/// <summary>
        /// Activate or deactivate the health fixture
        /// </summary>
        /// <param name="activated">Activation or deactivation.</param>
		public void SetActivation(bool activated)
		{

			this.isActivated = activated;
	
			if (activated)
			{
				cachedCollider.enabled = true;
			}
			else
			{
				cachedCollider.enabled = false;
			}
			
		}


		/// <summary>
        /// Damage this health fixture.
        /// </summary>
        /// <param name="damageValueByHealthType">The damage values by health type.</param>
        /// <param name="hitPosition">The world position that the hit occurred at.</param>
        /// <param name="sender">The game agent responsible for the damage.</param>
		public void Damage(List<float> damageValueByHealthType, Vector3 hitPosition, GameAgent sender)
		{
            // Shield effects
            ShowEffect(hitPosition, damageValueByHealthType[(int)healthType], true);
            
            if (isDamageable && onDamageEventHandler != null) onDamageEventHandler(damageValueByHealthType, hitPosition, sender);
		}


        /// <summary>
        /// Heal this health fixture.
        /// </summary>
        /// <param name="healValuesByHealthType">The heal values by health type.</param>
        /// <param name="interactionPosition">The world position where the heal interaction occurred.</param>
        /// <param name="sender">The game agent responsible for the healing.</param>
        public void Heal(List<float> healValuesByHealthType, Vector3 interactionPosition, GameAgent sender)
        {

            // Shield effects
            ShowEffect(interactionPosition, healValuesByHealthType[(int)healthType], false);

            if (onHealEventHandler != null) onHealEventHandler(healValuesByHealthType, interactionPosition, sender);
        }


        /// <summary>
        /// Called when a hit occurs to drive visual effects
        /// </summary>
        /// <param name="hitPosition">The position of the hit</param>
        /// <param name="value">The amount of damage.</param>
        /// <param name="isDamage">Whether it's damage or healing.</param>
        public void ShowEffect(Vector3 hitPosition, float value, bool isDamage)
        {

            // Get the local hit position wrt the shield mesh
            Vector3 localHitPosition = transform.InverseTransformPoint(hitPosition);

            // Find an available effect slot and do the effect
            for (int i = 0; i < 10; ++i)
            {

                // Get the position 
                Vector3 pos = new Vector3(effectPositions[i].x, effectPositions[i].y, effectPositions[i].z);
                bool isInMergeDistance = (Vector3.Distance(localHitPosition, pos) < mergeEffectDistance);

                if (isInMergeDistance || effectPositions[i].w < 5f)
                {

                    // Set the values for the shader
                    Vector4 temp = new Vector4();
                    temp.w = effectPositions[i].w + value * (isDamage ? damageEffectMultiplier : healEffectMultiplier);

                    temp.x = localHitPosition.x;
                    temp.y = localHitPosition.y;
                    temp.z = localHitPosition.z;
                    
                    effectPositions[i] = temp;

                    break;
                }
            }
        }


        /// <summary>
        /// Called every frame to update hit visual effects.
        /// </summary>
        void UpdateEffects()
        {
            
            if (!hasMeshRenderer) return;

            cachedMeshRenderer.material.SetVector("_LocalHitPoint0", effectPositions[0]);
            cachedMeshRenderer.material.SetVector("_LocalHitPoint1", effectPositions[1]);
            cachedMeshRenderer.material.SetVector("_LocalHitPoint2", effectPositions[2]);
            cachedMeshRenderer.material.SetVector("_LocalHitPoint3", effectPositions[3]);
            cachedMeshRenderer.material.SetVector("_LocalHitPoint4", effectPositions[4]);
            cachedMeshRenderer.material.SetVector("_LocalHitPoint5", effectPositions[5]);
            cachedMeshRenderer.material.SetVector("_LocalHitPoint6", effectPositions[6]);
            cachedMeshRenderer.material.SetVector("_LocalHitPoint7", effectPositions[7]);
            cachedMeshRenderer.material.SetVector("_LocalHitPoint8", effectPositions[8]);
            cachedMeshRenderer.material.SetVector("_LocalHitPoint9", effectPositions[9]);

            // For each of the effect slots, lerp the effect strength to zero (fade out)
            for (int i = 0; i < 10; ++i)
            {
                Vector4 temp = effectPositions[i];
                temp.w = Mathf.Lerp(temp.w, 0f, effectFadeSpeed * Time.deltaTime);
                if (temp.w < 0.01f) temp.w = 0f;
                effectPositions[i] = temp;            
            }
        }

        void Update()
        {
            // Update the shield effects frame each frame
            UpdateEffects();
        }
    }
}
