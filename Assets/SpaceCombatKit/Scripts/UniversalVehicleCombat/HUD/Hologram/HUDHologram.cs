using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using VSX.UniversalVehicleCombat;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// This class manages the dashboard holograms of the selected target and/or the player vehicle
    /// </summary>
	public class HUDHologram : MonoBehaviour
    {
	
		private Transform cachedTransform;
		public Transform CachedTransform { get { return cachedTransform; } }

		private GameObject cachedGameObject;
		public GameObject CachedGameObject { get { return cachedGameObject; } }

		private HUDManager manager;
		private bool hasManager = false;

        [SerializeField]
		private HUDHologramController targetHologram;

		public List<Color> colorByTeam = new List<Color>();
	
		ITrackable previousTarget;
		
	
		void Awake()
		{
			cachedTransform = transform;
			cachedGameObject = gameObject;
		}

		
        /// <summary>
        /// Set the HUD manager for this HUD element
        /// </summary>
        /// <param name="manager">The HUD manager for this HUD element.</param>
		public void SetManager(HUDManager manager)
		{
			this.manager = manager;
			this.hasManager = true;
		}


        /// <summary>
        /// Event called when the HUD that this component is part of is activated in the scene.
        /// </summary>
		public void OnActivate()
		{
		}


        /// <summary>
        /// Event called when the HUD that this component is part of is deactivated in the scene.
        /// </summary>
		public void OnDeactivate()
		{
		}


        
		void LateUpdate()
		{

			if (!hasManager || !manager.HasFocusedVehicle || !manager.FocusedVehicle.HasRadar || !manager.FocusedVehicle.Radar.HasSelectedTarget || 
					!manager.FocusedVehicle.Radar.SelectedTarget.HasHologramMesh)
			{ 
				targetHologram.Disable();
				return;
			}
			
			// If target has changed, update the mesh
			if (manager.FocusedVehicle.Radar.SelectedTarget != previousTarget)
			{
				previousTarget = manager.FocusedVehicle.Radar.SelectedTarget;
				if (!manager.FocusedVehicle.Radar.SelectedTarget.Equals(null))
				{
					
					// Update color
					int colorIndex = (int)(manager.FocusedVehicle.Radar.SelectedTarget.Team);
					targetHologram.SetColor(colorByTeam[colorIndex]);
	
					// Update mesh
					Mesh _mesh = manager.FocusedVehicle.Radar.SelectedTarget.HologramMesh;
					Texture2D _norm = manager.FocusedVehicle.Radar.SelectedTarget.HologramNormal;
					targetHologram.Set(_mesh, _norm);

				}
			}
	
			// Make sure the hologram is enabled
			targetHologram.Enable();
			
			// Update it
			targetHologram.UpdateHologram(manager.FocusedVehicle.Radar.SelectedTarget.CachedTransform, manager.FocusedVehicle.CachedTransform);
		}
	}
}
