using UnityEngine;
using System.Collections;

namespace VSX.UniversalVehicleCombat 
{

	/// <summary>
    /// This class manages the HUD for a spaceship
    /// </summary>
	public class ShipHUDManager : HUDManager 
	{
	
        /// <summary>
        /// Event called when the camera view changes on the vehicle camera.
        /// </summary>
        /// <param name="newCameraView">The new camera view.</param>
        /// <param name="cameraTransform">The camera transform.</param>
		public override void OnCameraViewChanged(VehicleCameraView newCameraView, Transform cameraTransform)
		{
			
			if (!hasFocusedVehicle)
				return;
			
			// Parent to camera or cockpit depending on the camera view
			switch (newCameraView)
			{
				
				case VehicleCameraView.Interior:
	
					CachedTransform.SetParent(focusedVehicle.CachedTransform);
					CachedTransform.localRotation = Quaternion.identity;
					CachedTransform.localPosition = Vector3.zero;

					if (HasRadar3D)
						Radar3D.CachedGameObject.SetActive(true);

					if (HasHologram)
						Hologram.CachedGameObject.SetActive(true);
					
					break;

	
				case VehicleCameraView.Exterior:
					
					cachedTransform.SetParent(cameraTransform);
					cachedTransform.localRotation = Quaternion.identity;

					Vector3 localPos = Vector3.zero;
					if (GameAgentManager.Instance.FocusedGameAgent.IsInVehicle)
					{
						Vehicle vehicle = GameAgentManager.Instance.FocusedGameAgent.Vehicle;
						for (int i = 0; i < vehicle.CameraViewTargets.Count; ++i)
						{
							if (vehicle.CameraViewTargets[i].CameraView == VehicleCameraView.Interior)
							{
								localPos = -(vehicle.CachedTransform.InverseTransformPoint(vehicle.CameraViewTargets[i].CachedTransform.position));
							}
						}
					}
					cachedTransform.localPosition = localPos;
				
					if (HasRadar3D) Radar3D.CachedGameObject.SetActive(false);

					if (HasHologram) Hologram.CachedGameObject.SetActive(false);

					break;
	
			}
		}
	
	}
}
