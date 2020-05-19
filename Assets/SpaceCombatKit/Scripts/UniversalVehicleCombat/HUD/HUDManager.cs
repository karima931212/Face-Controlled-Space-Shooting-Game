using UnityEngine;
using System.Collections;

namespace VSX.UniversalVehicleCombat 
{

	/// <summary>
    /// This class is a base class for managing a single HUD, including loading/unloading and camera view context switching.
    /// </summary>
	public class HUDManager : MonoBehaviour
	{
	

		protected HUDShipCockpit cockpit;
		public HUDShipCockpit Cockpit { get { return cockpit; } }

		protected bool hasCockpit;
		public bool HasCockpit { get { return hasCockpit; } }


		protected HUDTargetTracking targetTracking;
		public HUDTargetTracking TargetTracking { get { return targetTracking; } }

		protected bool hasTargetTracking;
		public bool HasTargetTracking { get { return hasTargetTracking; } }

		
		protected HUDRadar3D radar3D;
		public HUDRadar3D Radar3D { get { return radar3D; } }

		protected bool hasRadar3D;
		public bool HasRadar3D { get { return hasRadar3D; } }

		
		protected HUDHologram hologram;
		public HUDHologram Hologram { get { return hologram; } }

		protected bool hasHologram;
		public bool HasHologram { get { return hasHologram; } }


		protected HUDMessages messages;
		public HUDMessages Messages { get { return messages; } }

		protected bool hasMessages;
		public bool HasMessages { get { return hasMessages; } }


		protected Transform cachedTransform;
		public Transform CachedTransform { get { return cachedTransform; } } 

		protected Vehicle focusedVehicle;
		public Vehicle FocusedVehicle { get { return focusedVehicle; } }

		protected bool hasFocusedVehicle;
		public bool HasFocusedVehicle { get { return hasFocusedVehicle; } }

        [SerializeField]
        private bool disableHologramInExteriorView = true;

        protected GameManager gameManager;
		


		protected virtual void Awake()
		{

			cachedTransform = transform;

            cockpit = transform.GetComponentInChildren<HUDShipCockpit>();
			hasCockpit = cockpit != null;

            radar3D = transform.GetComponentInChildren<HUDRadar3D>();
            hasRadar3D = radar3D != null;

            targetTracking = transform.GetComponentInChildren<HUDTargetTracking>();
            hasTargetTracking = targetTracking != null;	

            hologram = transform.GetComponentInChildren<HUDHologram>();
            hasHologram = hologram != null;

            messages = transform.GetComponentInChildren<HUDMessages>();
            hasMessages = messages != null;

			if (hasCockpit)
			{
				cockpit.SetManager(this);
			}

			if (hasRadar3D)
			{
				radar3D.SetManager(this);
			}

			if (hasTargetTracking)
			{
				targetTracking.SetManager(this);
			}

			if (hasHologram)
			{
				hologram.SetManager(this);
			}

			if (hasMessages)
			{
				messages.SetManager(this);
			}
		}


		/// <summary>
        /// Event called when the HUD is activated in the scene.
        /// </summary>
        /// <param name="newFocusedVehicle">The vehicle this HUD belongs to.</param>
        /// <param name="targetTrackingAnchor">The parent anchor for the target tracking canvas. This must be null for screen-space-overlay.</param>
		public virtual void OnActivate(Vehicle newFocusedVehicle, Transform targetTrackingAnchor)
		{

			if (newFocusedVehicle == null)
			{
				Debug.LogError("Cannot activate HUD component with a null vehicle reference");
				return;
			}

			gameObject.SetActive(true);

			focusedVehicle = newFocusedVehicle;
			hasFocusedVehicle = true;

            if (focusedVehicle.HasWeapons)
            {
                focusedVehicle.Weapons.onAimAssistChangedEventHandler += OnAimAssistChanged;
            }

            // Activate the different parts of the HUD
			if (hasCockpit) cockpit.OnActivate();
			if (hasTargetTracking) targetTracking.OnActivate(targetTrackingAnchor);
			if (hasRadar3D) radar3D.OnActivate();
			if (hasHologram) hologram.OnActivate();
			if (hasMessages) messages.OnActivate();
		}

	
		/// <summary>
        /// Event called when this HUD is deactivated in the scene.
        /// </summary>
		public virtual void OnDeactivate()
		{

            if (focusedVehicle != null)
            {
                if (focusedVehicle.HasWeapons)
                {
                    // Unlink from the previous vehicle's toggle aim assist event 
                    focusedVehicle.Weapons.onAimAssistChangedEventHandler -= OnAimAssistChanged;
                }
            }

			focusedVehicle = null;
			hasFocusedVehicle = false;

			if (hasCockpit) cockpit.OnDeactivate();
			if (hasTargetTracking) targetTracking.OnDeactivate(transform);
			if (hasRadar3D) radar3D.OnDeactivate();
			if (hasHologram) radar3D.OnDeactivate();
			if (hasMessages) messages.OnDeactivate();

			gameObject.SetActive(false);

		}


        private void OnAimAssistChanged(bool isOn)
        {
            if (hasMessages)
            {
                messages.AddMessage("AIM ASSIST " + (isOn ? "ON" : "OFF"));
            }
        }



        /// <summary>
        /// Called when the camera view is changed on the focused vehicle.
        /// </summary>
        /// <param name="newCameraView"></param>
        /// <param name="cameraTransform"></param>
        public virtual void OnCameraViewChanged(VehicleCameraView newCameraView, Transform cameraTransform)
		{
            if (!hasFocusedVehicle)
                return;


            Vector3 localPos = Vector3.zero;

            // Parent to camera or cockpit depending on the camera view
            switch (newCameraView)
            {

                case VehicleCameraView.Interior:

                    CachedTransform.SetParent(focusedVehicle.CachedTransform);
                    CachedTransform.localRotation = Quaternion.identity;
                    
                    if (GameAgentManager.Instance.FocusedGameAgent.IsInVehicle)
                    {
                        Vehicle vehicle = GameAgentManager.Instance.FocusedGameAgent.Vehicle;
                        for (int i = 0; i < vehicle.CameraViewTargets.Count; ++i)
                        {
                            if (vehicle.CameraViewTargets[i].CameraView == VehicleCameraView.Interior)
                            {
                                localPos = vehicle.CachedTransform.InverseTransformPoint(vehicle.CameraViewTargets[i].CachedTransform.position);
                            }
                        }
                    }
                    cachedTransform.localPosition = localPos;

                    if (HasRadar3D)
                        Radar3D.CachedGameObject.SetActive(true);

                    if (HasHologram)
                        Hologram.CachedGameObject.SetActive(true);

                    break;


                case VehicleCameraView.Exterior:

                    cachedTransform.SetParent(cameraTransform);
                    cachedTransform.localRotation = Quaternion.identity;

                    if (GameAgentManager.Instance.FocusedGameAgent.IsInVehicle)
                    {
                        Vehicle vehicle = GameAgentManager.Instance.FocusedGameAgent.Vehicle;
                        for (int i = 0; i < vehicle.CameraViewTargets.Count; ++i)
                        {
                            if (vehicle.CameraViewTargets[i].CameraView == VehicleCameraView.Interior)
                            {
                                localPos = Vector3.zero;
                            }
                        }
                    }
                    cachedTransform.localPosition = localPos;

                    if (HasRadar3D) Radar3D.CachedGameObject.SetActive(false);

                    if (HasHologram) Hologram.CachedGameObject.SetActive(!disableHologramInExteriorView);

                    break;
                    
            }
        }
    }
}
