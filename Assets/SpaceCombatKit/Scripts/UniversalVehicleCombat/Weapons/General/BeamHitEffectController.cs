using UnityEngine;
using System.Collections;

namespace VSX.UniversalVehicleCombat
{

	/// <summary>
    /// This class controls the hit effect that is shown when a beam strikes a surface.
    /// </summary>
	public class BeamHitEffectController : MonoBehaviour 
	{
	
		private float beamStrength;

        [SerializeField]
        private Color effectColor;

		[SerializeField]
		private float maxGlowSize;

		[SerializeField]
		private float maxGlowAlpha;
	
		[SerializeField]
		private float maxSparkSize;

		[SerializeField]
		private float maxSparkAlpha;

		Material glowMaterial;
		Material sparkMaterial;
	
		[SerializeField]
		private ParticleSystem glowParticleSystem;
        private ParticleSystem.MainModule glowParticleSystemMainModule;

		[SerializeField]
		private ParticleSystem sparkParticleSystem;
        private ParticleSystem.MainModule sparkParticleSystemMainModule;

        private Transform cachedTransform;
		public Transform CachedTransform { get { return cachedTransform; } }
	
		
			
		void Awake()
        {
			
			// Get the materials
			glowMaterial = glowParticleSystem.GetComponent<ParticleSystemRenderer>().material;
            glowMaterial.SetColor("_TintColor", effectColor);
            glowParticleSystemMainModule = glowParticleSystem.main;

			sparkMaterial = sparkParticleSystem.gameObject.GetComponent<ParticleSystemRenderer>().material;
            sparkMaterial.SetColor("_TintColor", effectColor);
            sparkParticleSystemMainModule = sparkParticleSystem.main;

            cachedTransform = transform;

		}

	
		/// <summary>
        /// Called by the BeamSpawn component to update this effect.
        /// </summary>
        /// <param name="beamOnAmount">The 0-1 amount that the beam is on.</param>
		public void Set(float beamOnAmount)
		{

			// Update the particle sizes
			glowParticleSystemMainModule.startSize = beamOnAmount * maxGlowSize;
			sparkParticleSystemMainModule.startSize = beamOnAmount * maxSparkSize;

			// Set the glow alpha
			Color c = glowMaterial.GetColor("_TintColor");
			c.a = beamOnAmount;
			glowMaterial.SetColor("_TintColor", c);

			// Set the spark alpha
			c = sparkMaterial.GetColor("_TintColor");
			c.a = beamOnAmount;
			sparkMaterial.SetColor("_TintColor", c);

		}
	}
}