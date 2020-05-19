using UnityEngine;
using System.Collections;
using VSX.UniversalVehicleCombat;

namespace VSX.UniversalVehicleCombat
{

    /// <summary>
    /// This class manages the flyby particles effects to help the player to feel the speed of the vehicle.
    /// </summary>
    public class FlybyParticleController : MonoBehaviour 
	{
	
		private ParticleSystem thisParticleSystem;
        ParticleSystem.MainModule effectsParticleSystemMainModule;

        [SerializeField]
		private Color particleColor;
	
		[SerializeField]
		private float velocityToSizeCoefficient = 0.005f;
	
		[SerializeField]
		private float velocityToAlphaCoefficient = 0.005f;
	
		
		void Awake()
		{
			thisParticleSystem = GetComponent<ParticleSystem>();
            effectsParticleSystemMainModule = thisParticleSystem.main;
            
            Color c = particleColor;
			c.a = 0;

            effectsParticleSystemMainModule.startColor = c;
		}


        /// <summary>
        /// Update the flyby particle effects according to the velocity of the vehicle.
        /// </summary>
        /// <param name="vehicleVelocity">The velocity of the vehicle.</param>
        public void UpdateEffect(Vector3 vehicleVelocity)
		{
	
			float size, alpha;
	
			alpha = vehicleVelocity.magnitude * velocityToAlphaCoefficient;
			size = vehicleVelocity.magnitude * velocityToSizeCoefficient;
			
			Color c = particleColor;
			c.a = alpha;
			effectsParticleSystemMainModule.startColor = c;

            ParticleSystem.VelocityOverLifetimeModule velocityOverLifetimeModule = thisParticleSystem.velocityOverLifetime;
            velocityOverLifetimeModule.x = -vehicleVelocity.x;
            velocityOverLifetimeModule.y = -vehicleVelocity.y;
            velocityOverLifetimeModule.z = -vehicleVelocity.z;

            effectsParticleSystemMainModule.startSize = size;

		}
	}
}
