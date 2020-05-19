using UnityEngine;
using System.Collections;

namespace VSX.UniversalVehicleCombat
{

    /// <summary>
    /// This class controls a planet camera (follows rotation and field of view of the vehicle camera, but not position)
    /// </summary>
    public class PlanetCameraController : MonoBehaviour 
	{
	
		VehicleCamera vehicleCamera;
		Transform chaseCameraTransform;
	
		Transform cachedTransform;
	
		Camera thisCam;
	
		
	
		// Use this for initialization
		void Start()
		{
	
			thisCam = GetComponent<Camera>();
			cachedTransform = transform;

			vehicleCamera = GameObject.FindObjectOfType<VehicleCamera>();
	
			chaseCameraTransform = vehicleCamera.ChaseCamera.transform;
			
		}
		
		// Update is called once per frame
		void Update()
		{
			cachedTransform.rotation = chaseCameraTransform.rotation;
			thisCam.fieldOfView = vehicleCamera.ChaseCamera.fieldOfView;
		}
	}
}
