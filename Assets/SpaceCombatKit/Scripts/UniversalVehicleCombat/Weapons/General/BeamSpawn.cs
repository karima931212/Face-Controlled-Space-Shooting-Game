using UnityEngine;
using System.Collections;

namespace VSX.UniversalVehicleCombat
{

	/// <summary>
    /// This class controls the beam effect for a beam weapon.
    /// </summary>
	[RequireComponent(typeof(LineRenderer))]
	public class BeamSpawn : MonoBehaviour 
	{
	
		[SerializeField]
		private Color beamColor;

		[SerializeField]
		private float beamWidth;

		[SerializeField]
	    private BeamHitEffectController beamHitEffectControllerPrefab;
		private BeamHitEffectController beamHitEffectController;
	   
		private Transform cachedTransform;
		public Transform CachedTransform { get { return cachedTransform; } } 

		LineRenderer lineRenderer;
	
	    Material beamMaterial;
	
		IEnumerator beamCoroutine;
	
		float currentBeamLength;
	
	

	    void Awake()
	    {

	        lineRenderer = GetComponent<LineRenderer>();
	        beamMaterial = lineRenderer.material;
			cachedTransform = transform;
	
	    }	

		void Start()
		{

            beamHitEffectController = Instantiate(beamHitEffectControllerPrefab, Vector3.zero, Quaternion.identity, transform) as BeamHitEffectController;
			beamHitEffectController.Set(0);
		}


        /// <summary>
        /// Called by the beam weapon module to update the beam effects.
        /// </summary>
        /// <param name="beamLength">The length of the beam.</param>
        /// <param name="hitNormal">The normal direction of the surface that the beam has hit, for placing the beam hit effect.</param>
        /// <param name="hasHit">Whether the beam has hit anything.</param>
        /// <param name="beamOnAmount">The 0-1 value for how much the beam is turned on.</param>
        public void SetBeam(float beamLength, Vector3 hitNormal, bool hasHit, float beamOnAmount)
		{
	
			this.currentBeamLength = beamLength;
	
			beamColor.a = beamOnAmount;
			beamMaterial.SetColor("_TintColor", beamColor);
            lineRenderer.startWidth = beamOnAmount * beamWidth;
            lineRenderer.endWidth = beamOnAmount * beamWidth;
	
			if (beamHitEffectController != null)
			{

				if (hasHit)
				{
					Vector3 endPoint = cachedTransform.position + cachedTransform.forward * beamLength;
					beamHitEffectController.CachedTransform.position = endPoint;
					beamHitEffectController.CachedTransform.LookAt(endPoint + hitNormal);
					beamHitEffectController.Set(beamOnAmount);
				}
				else
				{
					beamHitEffectController.Set(0f);
				}
	        }
	    }
	
	
		void Update()
		{
			// Make sure line renderer is always rendering along weapon forward vector
			lineRenderer.SetPosition(0, cachedTransform.position);
			lineRenderer.SetPosition(1, cachedTransform.position + cachedTransform.forward * currentBeamLength);
		}
	}
}
