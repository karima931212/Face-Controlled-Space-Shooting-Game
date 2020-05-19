using UnityEngine;
using System.Collections;
using VSX.UniversalVehicleCombat;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// This class manages the position and rotation of the camera during loadout.
    /// </summary>
	public class LoadoutCameraPositionController : MonoBehaviour 
	{
	
		[SerializeField]
		private Camera cam;
	
		[SerializeField]
		private Vector2 targetCenterViewportPosition = new Vector2(0.5f, 0.6f);
		
		[SerializeField]
		private float maxViewportVehicleDiameter = 0.33f;
	
		private Transform cachedTransform;
		public Transform CachedTransform { get { return cachedTransform; } }
	
		[SerializeField]
		private bool considerMeshPositions = true;
	
		
		// Use this for initialization
		void Awake () 
		{
			cachedTransform = transform;
		}


        /// <summary>
        /// Event called when the player clicks on a vehicle in the vehicle selection part of the menu.
        /// </summary>
        /// <param name="newSelectedVehicle">The newly selected vehicle.</param>
        public void OnVehicleSelection(Vehicle newSelectedVehicle)
		{
			
			float diameter = 1;
	
			//Go through all the meshes on the vehicle to find the one sticking out the furthest, to determine the bounding sphere diameter
			MeshFilter[] meshFilters = newSelectedVehicle.transform.GetComponentsInChildren<MeshFilter>();
			foreach (MeshFilter meshFilter in meshFilters)
			{
	
				Mesh mesh = meshFilter.mesh;
				if (mesh == null)
					continue;
	
				float tempDiameter = Mathf.Max(new float[]{mesh.bounds.size.x * meshFilter.transform.localScale.x, 
															mesh.bounds.size.y * meshFilter.transform.localScale.y, 
															mesh.bounds.size.z * meshFilter.transform.localScale.z});
				
				// Take into account if the mesh is offset from the vehicle (increasing the bounding sphere size)
				if (considerMeshPositions)
				{
					Vector3 meshCenterWorldPosition = meshFilter.transform.TransformPoint(mesh.bounds.center);
					Vector3 worldOffset = newSelectedVehicle.CachedTransform.position - meshCenterWorldPosition;
					Vector3 localOffset = newSelectedVehicle.CachedTransform.InverseTransformPoint(worldOffset);
					
					tempDiameter += localOffset.magnitude * 2;
				}
	
				diameter = Mathf.Max(diameter, tempDiameter);
				
			}
			
			// Get the smaller dimension of the screen for determining the angle used to calculate the distance
			// the camera has to be to achieve the max viewport size set in the inspector
			bool useHorizontalAngle = cam.aspect < 1;
			float halfAngle;
			if (useHorizontalAngle)
			{
				float tmp = 0.5f / Mathf.Tan((cam.fieldOfView / 2) * Mathf.Deg2Rad);
				halfAngle = Mathf.Atan((0.5f * cam.aspect) / tmp) * Mathf.Rad2Deg;
			}
			else
			{
				halfAngle = cam.fieldOfView / 2;
			}
	
			// Calculate the distance of the camera to the target vehicle to achieve the viewport size
			float distance = ((diameter / 2) / maxViewportVehicleDiameter) / Mathf.Tan(halfAngle * Mathf.Deg2Rad);
			transform.position = newSelectedVehicle.CachedTransform.position - transform.forward * distance;
	
			// Position the camera such that the target vehicle appears centered at the viewport coordinates
			// set in the inspector
			Vector2 viewportHalfDimensions = Vector2.zero;
			viewportHalfDimensions.x = distance * Mathf.Tan(((cam.fieldOfView * cam.aspect)/2) * Mathf.Deg2Rad);
			viewportHalfDimensions.y = distance * Mathf.Tan((cam.fieldOfView/2) * Mathf.Deg2Rad);
	
			transform.position += -transform.right * ((targetCenterViewportPosition.x - 0.5f) * (viewportHalfDimensions.x * 2));
			
			transform.position += -transform.up * ((targetCenterViewportPosition.y - 0.5f) * (viewportHalfDimensions.y * 2));
	
	
		}		
	}
}
