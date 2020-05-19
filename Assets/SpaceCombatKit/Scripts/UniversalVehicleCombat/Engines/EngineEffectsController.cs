using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace VSX.UniversalVehicleCombat
{

    /// <summary>
    /// This class manages the audio and visual effects for an engine.
    /// </summary>
    /// <remarks>
    /// The Engine Effects Controller uses throttle values to implement audio
    /// and visual effects. In this case the exhaust is composed of 'Halo' meshes, 'Glow' meshes, 
    /// Particle Systems and Trail Renderers. This component can be used for vehicles, missiles or 
    /// other types of entities that use exhausts.
    /// </remarks>
    public class EngineEffectsController : MonoBehaviour
    {

        /// <summary> A curve that describes how the effects change as the throttle and steering values change. </summary> 
        [SerializeField]
		private AnimationCurve effectsCurve;

		[Header ("General")]

        /// <summary> The primary color of the exhaust. </summary> 
        [SerializeField]
		private Color exhaustColor = new Color (1f, 0.5f, 0f, 1f);
	

		[Header ("Glow")]

        [SerializeField]
		private List<MeshRenderer> exhaustGlowRenderers = new List<MeshRenderer>();
		List<Material> exhaustGlowMaterials = new List<Material>();

		[SerializeField]
		private float maxCruisingGlowAlpha = 0.75f;

		[SerializeField]
		private float boostGlowAlpha = 1f;
	

		[Header ("Halo")]
	
		[SerializeField]
		private List<MeshRenderer> exhaustHaloRenderers = new List<MeshRenderer>();
		List<Material> exhaustHaloMaterials = new List<Material>();

		[SerializeField]
		private float maxCruisingHaloAlpha = 0.75f;

		[SerializeField]
		private float boostHaloAlpha = 1f;
	
	
		[Header ("Particles")]
	
		[SerializeField]
		private List<ParticleSystem> exhaustParticleSystems = new List<ParticleSystem>();

        private ParticleSystem.MainModule[] exhaustParticleSystemMainModules;
        private List<Material> exhaustParticleMaterials = new List<Material>();
        private List<float> exhaustParticleStartSpeeds = new List<float>();

		[SerializeField]
		private float maxCruisingParticleAlpha = 0.75f;

		[SerializeField]
		private float boostParticleAlpha = 1f;

		[SerializeField]
		private float maxCruisingParticleSpeedFactor = 1f;

		[SerializeField]
		private float boostParticleSpeedFactor = 2f;
	
	    
		[Header ("Trails")]
	
		[SerializeField]
		private List<TrailRenderer> exhaustTrailRenderers;
		List<Material> exhaustTrailMaterials = new List<Material>();

		[SerializeField]
		private float maxCruisingTrailAlpha = 0.75f;

		[SerializeField]
		private float boostTrailAlpha = 1f;

		Coroutine fadeExhaustCoroutine;
		
	
	
		void Awake()
		{

            // Cache all of the materials that need to be updated

            for (int i = 0; i < exhaustGlowRenderers.Count; ++i)
			{
				exhaustGlowMaterials.Add(exhaustGlowRenderers[i].material);
			}

			for (int i = 0; i < exhaustHaloRenderers.Count; ++i)
			{
				exhaustHaloMaterials.Add(exhaustHaloRenderers[i].material);
			}

            exhaustParticleSystemMainModules = new ParticleSystem.MainModule[exhaustParticleSystems.Count];
			for (int i = 0; i < exhaustParticleSystems.Count; ++i)
			{
                exhaustParticleSystemMainModules[i] = exhaustParticleSystems[i].main;
                exhaustParticleMaterials.Add(exhaustParticleSystems[i].gameObject.GetComponent<ParticleSystemRenderer>().material);
                exhaustParticleStartSpeeds.Add(exhaustParticleSystemMainModules[i].startSpeed.constant);
			}
            
			for (int i = 0; i < exhaustTrailRenderers.Count; ++i)
			{
				exhaustTrailMaterials.Add(exhaustTrailRenderers[i].material);
			}
		}

        /// <summary>
		/// Reset and clear the exhaust effects.
		/// </summary>
		public void Reset()
		{

			StopAllCoroutines();    // Stop fading exhaust coroutine

			for (int i = 0; i < exhaustTrailRenderers.Count; ++i)
			{
				exhaustTrailRenderers[i].Clear();
			}
		}


        /// <summary>
        /// Enable or disable the trail renderers 
        /// </summary>
        /// <param name="setEnabled"> Whether the trail renderers will be enabled or disabled. </param>
        public void SetExhaustTrailsEnabled(bool setEnabled)
		{
			for (int i = 0; i < exhaustTrailRenderers.Count; ++i)
			{
				exhaustTrailRenderers[i].enabled = setEnabled;
			}
		}


        /// <summary>
        /// Fade out the exhaust over time.
        /// </summary>
        /// <param name="fadeTime"> How long the exhaust will take to fade. </param>
        public void FadeExhaust(float fadeTime)
		{
			if (fadeExhaustCoroutine != null)
			{
				StopCoroutine(fadeExhaustCoroutine);
			}

			fadeExhaustCoroutine = StartCoroutine (FadeExhaustCoroutine(fadeTime));
		}


		// Coroutine for fading out the exhaust over time
		IEnumerator FadeExhaustCoroutine (float fadeTime)
		{
			float startTime = Time.time;
			while (Time.time - startTime < fadeTime)
			{

				// Fade out the trail renderer material alpha 
				float fadeAmount = (Time.time - startTime) / fadeTime;
				for (int i = 0; i < exhaustTrailRenderers.Count; ++i)
				{
					Color c = exhaustColor;
					c.a = 1 - fadeAmount;
					exhaustTrailMaterials[i].SetColor("_TintColor", c);
				}

				yield return null;

			}
		}


        /// <summary>
        /// Fade out the exhaust over time.
        /// </summary>
        /// <param name="throttleValue"> The throttle value for the engine effects. </param
        /// <param name="boostOn"> Whether the engine's boost function is on or off. </param>
        public void Set(float throttleValue, float boostValue)
		{
			
			float particleAlpha = 0;
			float particleSpeedFactor = 0;
			float haloAlpha = 0;
			float glowAlpha = 0;
			float trailAlpha = 0;


            particleAlpha = effectsCurve.Evaluate(throttleValue) * maxCruisingParticleAlpha;
            particleSpeedFactor = effectsCurve.Evaluate(throttleValue) * maxCruisingParticleSpeedFactor;
            haloAlpha = effectsCurve.Evaluate(throttleValue) * maxCruisingHaloAlpha;
            glowAlpha = effectsCurve.Evaluate(throttleValue) * maxCruisingGlowAlpha;
            trailAlpha = effectsCurve.Evaluate(throttleValue) * maxCruisingTrailAlpha;


            particleAlpha = particleAlpha + boostValue * (boostParticleAlpha - particleAlpha);
			particleSpeedFactor = particleSpeedFactor + boostValue * (boostParticleSpeedFactor - particleSpeedFactor);
			haloAlpha = haloAlpha + boostValue * (boostHaloAlpha - haloAlpha);
            glowAlpha = glowAlpha + boostValue * (boostGlowAlpha - glowAlpha);
            trailAlpha = trailAlpha + boostValue * (boostTrailAlpha - trailAlpha);
        
			
			for (int i = 0; i < exhaustHaloMaterials.Count; ++i)
			{
				Color c = exhaustColor;
				c.a = haloAlpha;
				exhaustHaloMaterials[i].SetColor("_TintColor", c);
			}
			
			for (int i = 0; i < exhaustGlowMaterials.Count; ++i)
			{
				Color c = exhaustColor;
				float h, s, v;
				Color.RGBToHSV(c, out h, out s, out v);
				c = Color.HSVToRGB(h, s, v);
				c.a = glowAlpha;
				exhaustGlowMaterials[i].SetColor("_TintColor", c);
			}
			
			for (int i = 0; i < exhaustParticleMaterials.Count; ++i)
			{
				Color c = exhaustColor;
				c.a = particleAlpha;
				exhaustParticleMaterials[i].SetColor("_TintColor", c);
			}
	
			for (int i = 0; i < exhaustParticleSystemMainModules.Length; ++i)
			{
                exhaustParticleSystemMainModules[i].startSpeed = particleSpeedFactor * exhaustParticleStartSpeeds[i];
			}
				
			for (int i = 0; i < exhaustTrailMaterials.Count; ++i)
			{
				Color c = exhaustColor;
				c.a = trailAlpha;
				exhaustTrailMaterials[i].SetColor("_TintColor", c);
			}
		}
	}
}
