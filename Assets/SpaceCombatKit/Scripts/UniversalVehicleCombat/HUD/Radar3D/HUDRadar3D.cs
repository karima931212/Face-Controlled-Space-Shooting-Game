using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using VSX.UniversalVehicleCombat;
using VSX.General;


// This script controls and updates a 3D radar according to information provided by an ITracker component

namespace VSX.UniversalVehicleCombat
{

	/// <summary>
    /// This class provides a way to store settings and style information for 3D radar widgets that represent a particular trackable type.
    /// </summary>
	[System.Serializable]
	public class Radar3D_WidgetSettings 
	{

		public bool ignore = false;

		public GameObject widgetPrefab = null;

		public bool fadeUnselectedByDistance = false;
		
	}

    
    /// <summary>
    /// This class stores information that will be used to update a single 3D radar widget.
    /// </summary>
    public class Radar3D_WidgetParameters
	{

		public Vector3 widgetLocalPosition;

		public bool isSelected;

		public Color widgetColor;	

		public float alpha;	

	}


	/// <summary>
    /// This class manages the 3D radar for a vehicle.
    /// </summary>
	public class HUDRadar3D : MonoBehaviour
    {

		private Transform cachedTransform;
		public Transform CachedTransform { get { return cachedTransform; } }

		private GameObject cachedGameObject;
		public GameObject CachedGameObject { get { return cachedGameObject; } }

		private HUDManager manager;
		private bool hasManager = false;

		[Header("Settings")]

        [SerializeField]
		private float equatorRadius = 0.5f; // Radius of the equator plane

        [SerializeField]
        private float scaleExponent = 1f;

        [SerializeField]
        private float zoomSpeed = 0.5f;
        float currentZoomValue = 0.75f;

        [SerializeField]
        private float fadeMaxAlpha;

        [SerializeField]
        private float fadeMinAlpha;

        [SerializeField]
        private int maxNewTargetsEachFrame = 1;
		int numTargetsLastFrame;
		int displayedTargetCount;

		[Header("Widgets")]

		public List<Color> colorByTeam = new List<Color>();
		
        // A list of the 3D radar widget settings for each trackable type
		public List<Radar3D_WidgetSettings> widgetSettingsByType = new List<Radar3D_WidgetSettings>();

        // A list containing lists of all of the widgets used for each of the trackable types on this 3D radar.
		private List<List<IRadar3DWidget>> usedWidgetsByType = new List<List<IRadar3DWidget>>();

        // A list containing lists (by trackable type) of the settings that will be used to update the widgets being used on this 3D radar.
        private List<List<Radar3D_WidgetParameters>> usedWidgetParamsByType = new List<List<Radar3D_WidgetParameters>>();

        // The current index of the last used widget by trackable type
        private List<int> usedWidgetIndexByType = new List<int>();

        // The targets list 
        private List<ITrackable> targetsList = new List<ITrackable>();



	    /// <summary>
        /// Called in the editor when script is loaded or a value is changed.
        /// </summary>
		void OnValidate()
		{	

			// Make sure that the prefab for the widget contains a script implementing the IRadar3DWidget interface
			for (int i = 0; i < widgetSettingsByType.Count; ++i)
			{
				if (widgetSettingsByType[i].widgetPrefab != null)
				{
					if (widgetSettingsByType[i].widgetPrefab.GetComponent<IRadar3DWidget>() == null)
					{
						widgetSettingsByType[i].widgetPrefab = null;
						Debug.LogError("Object assigned to Widget Prefab must contain a component implementing the IRadar3DWidget interface");
					}
				}
			}
		}


		void Awake()
		{
			cachedTransform = transform;
			cachedGameObject = gameObject;
		}
		 

		// Use this for initialization
		void Start ()
        {

			for (int i = 0; i < widgetSettingsByType.Count; ++i)
            {
                if (widgetSettingsByType[i].widgetPrefab == null)
                {
                    Debug.LogError("No widget prefab assigned to widget settings instance field in the Radar3D component");
                }
                else
                {
                    usedWidgetsByType.Add(new List<IRadar3DWidget>());
                    usedWidgetParamsByType.Add(new List<Radar3D_WidgetParameters>());
                    usedWidgetIndexByType.Add(-1);
                }
            }
		}


        /// <summary>
        /// Set the manager for the 3D radar.
        /// </summary>
        /// <param name="manager">The manager for this 3D radar.</param>
        public void SetManager(HUDManager manager)
		{
			this.manager = manager;
			this.hasManager = true;
		}


        /// <summary>
        /// Event called when the HUD that this 3D radar is part of is activated in the scene
        /// </summary>
		public void OnActivate()
		{
		}


        /// <summary>
        /// Event called when the HUD that this 3D radar is part of is deactivated in the scene
        /// </summary>
		public void OnDeactivate()
		{			
		}


		/// <summary>
        /// Increment the zoom of the 3D radar
        /// </summary>
        /// <param name="zoomIn"></param>
		public void IncrementZoom(bool zoomIn)
		{
			if (zoomIn)
			{ 
				currentZoomValue += zoomSpeed * Time.deltaTime;
			}
			else
			{ 
				currentZoomValue -= zoomSpeed * Time.deltaTime;
			}
			currentZoomValue = Mathf.Clamp(currentZoomValue, 0, 1);
			
		}

        
		/// <summary>
        /// Set the zoom of the 3D radar as a fraction of the amount between the minimum and maximum zoom limits.
        /// </summary>
        /// <param name="zoomFraction">The current zoom fraction.</param>
		public void SetZoom (float zoomFraction)
        {
			currentZoomValue = Mathf.Clamp(zoomFraction, 0, 1);	
		}	


		// Add a widget from the widget pool
		void AddWidget(int typeIndex)
		{
			Transform t = PoolManager.Instance.Get(widgetSettingsByType[typeIndex].widgetPrefab, Vector3.zero, Quaternion.identity).transform;
            t.SetParent(transform);
			t.localPosition = Vector3.zero;
			t.localRotation = Quaternion.identity;
            IRadar3DWidget h = t.GetComponent<IRadar3DWidget>();
			usedWidgetsByType[typeIndex].Add(h);

			usedWidgetParamsByType[typeIndex].Add(new Radar3D_WidgetParameters());
		}


		// Visualize a single target on the 3D radar
		void Visualize(ITrackable target)
		{
			
			int typeIndex = (int)(target.TrackableType);
			int teamIndex = (int)(target.Team);
	
			
			bool isSelectedTarget = manager.FocusedVehicle.Radar.HasSelectedTarget && 
										(target.CachedGameObject == manager.FocusedVehicle.Radar.SelectedTarget.CachedGameObject);

			float radarDisplayRange = (1 - currentZoomValue) * manager.FocusedVehicle.Radar.Range;

			Vector3 targetRelPos = manager.FocusedVehicle.CachedTransform.InverseTransformPoint(target.CachedTransform.position);
			float distToTarget = targetRelPos.magnitude;

			// If the target is outside the zoom-adjusted range of this radar, continue
			if (distToTarget > radarDisplayRange)
			{
				return;
			}

			// Update the last used widget index, and add one if there aren't enough
			usedWidgetIndexByType[typeIndex] += 1;
			if (usedWidgetsByType[typeIndex].Count < usedWidgetIndexByType[typeIndex] + 1)
			{
				AddWidget(typeIndex);
			}
			int thisWidgetIndex = usedWidgetIndexByType[typeIndex];
			
			// Scale the target position to the radar scale 
			float factor = 1 - Mathf.Pow(-(targetRelPos.magnitude / radarDisplayRange) + 1, scaleExponent);
			usedWidgetParamsByType[typeIndex][thisWidgetIndex].widgetLocalPosition = targetRelPos.normalized * (factor * equatorRadius);

			usedWidgetParamsByType[typeIndex][thisWidgetIndex].isSelected = isSelectedTarget;

			usedWidgetParamsByType[typeIndex][thisWidgetIndex].widgetColor = colorByTeam[teamIndex];
			usedWidgetParamsByType[typeIndex][thisWidgetIndex].alpha = 1f;
			if (!isSelectedTarget && widgetSettingsByType[typeIndex].fadeUnselectedByDistance)
			{
				float fraction = distToTarget / manager.FocusedVehicle.Radar.Range;
				float alphaRange = fadeMaxAlpha - fadeMinAlpha;
				usedWidgetParamsByType[typeIndex][thisWidgetIndex].alpha = fadeMaxAlpha - fraction * alphaRange;
			}

			// Set the widget
			usedWidgetsByType[typeIndex][thisWidgetIndex].Set(usedWidgetParamsByType[typeIndex][thisWidgetIndex]);

			// Update the count - used to prevent adding too many widgets per frame
			displayedTargetCount += 1;
			
		}


        // Remove any excess widgets being displayed on the 3D radar.
		void RemoveExcessWidgets()
        {
			// Remove excess widgets
			for (int i = 0; i < usedWidgetsByType.Count; ++i)
            {
				int usedCount = usedWidgetIndexByType[i] + 1;
				if (usedWidgetsByType[i].Count > usedCount)
				{

					int removeCount = usedWidgetsByType[i].Count - usedCount;

					// Disable unneeded widgets
					for (int j = 0; j < usedWidgetsByType[i].Count - usedCount; ++j)
					{
						usedWidgetsByType[i][usedCount + j].Disable();
					}

					usedWidgetsByType[i].RemoveRange(usedCount, removeCount);
					usedWidgetParamsByType[i].RemoveRange(usedCount, removeCount);
				}
			}
		}


		
		void LateUpdate()
		{

            // If this 3D radar should not be running, clear widgets and exit
			if (!hasManager || !manager.HasFocusedVehicle || !manager.FocusedVehicle.HasRadar)
			{ 
				RemoveExcessWidgets();
				return; 	// If no tracker has been found to display target information from, exit 
			}
			
			// Reset the used widget indexes
			displayedTargetCount = 0;
			for (int i = 0; i < usedWidgetIndexByType.Count; ++i)
			{
				usedWidgetIndexByType[i] = -1;
			}

			// Get targets
			targetsList = manager.FocusedVehicle.Radar.trackedTargets;

			// Visualize the targets
			for (int i = 0; i < targetsList.Count; ++i)
			{

				// Don't add a widget for the tracker itself
				if (targetsList[i].CachedGameObject == manager.FocusedVehicle.CachedGameObject || targetsList[i].Equals(null))
				{
					continue;
				}

				// Don't add a widget for ignored trackable types
				int trackableTypeIndex = (int)targetsList[i].TrackableType;
				if (widgetSettingsByType[trackableTypeIndex].ignore)
				{
					continue;
				}

				Visualize(targetsList[i]);

				// Don't add more than the specified amount of widgets per frame
				if (displayedTargetCount - numTargetsLastFrame >= maxNewTargetsEachFrame)
				{
					break;
				}

			}

			numTargetsLastFrame = displayedTargetCount;

			RemoveExcessWidgets();

		}
	}
}
