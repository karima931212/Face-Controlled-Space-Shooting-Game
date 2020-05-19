using UnityEngine;
using System.Collections;

namespace VSX.UniversalVehicleCombat
{

	/// <summary>
    /// This class controls a weapon muzzle flash.
    /// </summary>
	public class FlashController : MonoBehaviour 
	{
	
        // The current state of the flash effect
		enum EffectState 
		{
			FadingIn,
			FadingOut,
			Sustaining,
			Off
		}

        [SerializeField]
        private Color effectColor;

		private EffectState effectState = EffectState.Off;
		private float effectStateStartTime;

		[SerializeField]
		private MeshRenderer flashMeshRenderer;
		Material flashMat;
	
		[SerializeField]
		private MeshRenderer glowMeshRenderer;
		Material glowMat;
	
		[SerializeField]
		private bool continuous;
		private bool isOnContinuous = false;

		[SerializeField]
		private float fadeInTime;

		[SerializeField]
		private float fadeOutTime;

		[SerializeField]
		private float sustainTime;

		private Transform cachedTransform;
	
	
		void Awake()
		{

			flashMat = flashMeshRenderer.material;

            // Turn off the flash
            Color c = effectColor;
			c.a = 0;
			flashMat.SetColor("_TintColor", c);
	

			glowMat = glowMeshRenderer.material;
	
			// Turn off the glow
			c = effectColor;
			c.a = 0;
			glowMat.SetColor("_TintColor", c);

			cachedTransform = transform;

		}
	
		
        /// <summary>
        /// Event called when a weapon is fired, to randomize the effect
        /// </summary>
		public void OnFire()
		{
			cachedTransform.localRotation = Quaternion.Euler(new Vector3(0f, 0f, Random.Range(0, 180)));
			if (!continuous || !isOnContinuous) OnFlashStateChange(EffectState.FadingIn);

			if (continuous)
			{
				isOnContinuous = true;
			}
		}

		
		/// <summary>
        /// Event called when the weapon stops firing.
        /// </summary>
		public void OnEndFire()
		{
			if (continuous)
			{
				isOnContinuous = false;
			}
		}


		// Set a new state for the flash effect
		void OnFlashStateChange(EffectState newEffectState)
		{
			effectState = newEffectState;
			effectStateStartTime = Time.time;

			if (effectState == EffectState.Off)
			{
				SetLevel(0);
			}
		}
	

        /// <summary>
        /// Set the 'on' level of the weapon so that the effects can be updated accordingly
        /// </summary>
        /// <param name="level"></param>
		public void SetLevel(float level)
		{

			// Set the flash alpha
			Color c = flashMat.GetColor("_TintColor");
			c.a = level;
			flashMat.SetColor("_TintColor", c);
	
			// Set the glow alpha
			c = glowMat.GetColor("_TintColor");
			c.a = level;
			glowMat.SetColor("_TintColor", c);
		}


		void Update()
		{
	
			float alpha = 0;
			float stateFinishedAmount = 0;
			
			switch (effectState)
			{
				case EffectState.FadingIn:

					stateFinishedAmount = (Time.time - effectStateStartTime) / Mathf.Max(fadeInTime, 0.0001f);
					alpha = Mathf.Clamp(stateFinishedAmount, 0, 1);

					if (stateFinishedAmount > 1)
						OnFlashStateChange(EffectState.Sustaining);

					break;

				case EffectState.FadingOut:

					stateFinishedAmount = (Time.time - effectStateStartTime) / fadeInTime;

					alpha = Mathf.Clamp(1 - stateFinishedAmount, 0, 1);

					if (stateFinishedAmount > 1)
					{
						OnFlashStateChange(EffectState.Off);
					}

					break;
	
				case EffectState.Sustaining:

					stateFinishedAmount = (Time.time - effectStateStartTime) / Mathf.Max(0.0001f, sustainTime);
					alpha = 1;

					if ((continuous && !isOnContinuous) || (!continuous && stateFinishedAmount > 1))
					{
						OnFlashStateChange(EffectState.FadingOut);
					}

					break;

			}
			
			if (effectState != EffectState.Off)
			{
				SetLevel(alpha);
			}
		}
	}
}
