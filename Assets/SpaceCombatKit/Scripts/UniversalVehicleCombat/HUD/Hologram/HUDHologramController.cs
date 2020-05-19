using UnityEngine;
using System.Collections;

// This class is for creating a hologram of an trackable object and updating it with the trackable object's
// relative orientation

namespace VSX.UniversalVehicleCombat{

    /// <summary>
    /// This class manages a single dashboard hologram in the HUD of a vehicle.
    /// </summary>
	[RequireComponent(typeof(MeshFilter))]
	[RequireComponent(typeof(MeshRenderer))]
	public class HUDHologramController : MonoBehaviour
    {
	
		// Cached components
		MeshFilter hologramMeshFilter;
		MeshRenderer hologramMeshRenderer;
		Material hologramMat;
	
		[SerializeField]
		private float hologramSize = 0.3f;

		[SerializeField]
		private float hologramOutlineWidth = 0.075f;

		// This is for making the outline appear brighter (less saturated) than the middle
		[SerializeField]
		private float outlineSaturationCoefficient = 0.5f;

		

		void Awake()
		{

			// Cache components
			hologramMeshFilter = GetComponent<MeshFilter>();
			hologramMeshRenderer = GetComponent<MeshRenderer>();
			hologramMat = hologramMeshRenderer.material;
			
			Disable();

		}

        /// <summary>
        /// Set a new mesh for the hologram.
        /// </summary>
        /// <param name="newMesh">The mesh to display in the hologram.</param>
        /// <param name="meshNormal">The normal map for the mesh in the hologram.</param>
        public void Set(Mesh newMesh, Texture2D meshNormal)
		{

            // Set the mesh
			hologramMeshFilter.sharedMesh = newMesh;

            // Set the normal map
			if (meshNormal != null) hologramMat.SetTexture("_NormalMap", meshNormal);

			// Adjust the scale according to the size parameter set in inspector
			Vector3 extents = hologramMeshFilter.sharedMesh.bounds.extents;
	        float greatestDimension = Mathf.Max (new float []{extents.x, extents.y, extents.z});
	        float scale = hologramSize/greatestDimension;
			transform.localScale = new Vector3(scale, scale, scale);

            // Prevent the scale from changing the outline width
			hologramMat.SetFloat("_OutlineWidth", hologramOutlineWidth * scale);
			
		}
	
		/// <summary>
        /// Enable the hologram.
        /// </summary>
		public void Enable()
		{
			hologramMeshRenderer.enabled = true;
	    }
	
		/// <summary>
        /// Disable the hologram.
        /// </summary>
	    public void Disable()
	    {
	        hologramMeshRenderer.enabled = false;
	    }
	
		/// <summary>
        /// Set the hologram color.
        /// </summary>
        /// <param name="newColor"></param>
		public void SetColor(Color newColor)
		{

            // Set the shader rim color
			hologramMat.SetColor("_RimColor", newColor);

            // Set the shader outline color to the same hue with less saturation
			float h, s, v;
			Color.RGBToHSV(newColor, out h, out s, out v);

            // Set the outline color
			hologramMat.SetColor("_OutlineColor", Color.HSVToRGB(h, outlineSaturationCoefficient * s, v));

		}
	
		/// <summary>
        /// Update the hologram orientation. The hologram shows whether the target is facing this vehicle, regardless of 
        /// its relative position.
        /// </summary>
        /// <param name="targetTransform">The target transform.</param>
        /// <param name="trackerTransform">The transform of the tracker (the vehicle that is tracking the target).</param>
		public void UpdateHologram(Transform targetTransform, Transform trackerTransform)
		{
			
			// Calculate orientation
			Vector3 direction = (targetTransform.position - trackerTransform.position).normalized;
	        Quaternion temp = Quaternion.LookRotation(direction, trackerTransform.up);
	        Quaternion rot = Quaternion.Inverse(Quaternion.Inverse(targetTransform.rotation) * temp);

            // Set the rotation
	        transform.localRotation = rot;   

		}	
	}
}