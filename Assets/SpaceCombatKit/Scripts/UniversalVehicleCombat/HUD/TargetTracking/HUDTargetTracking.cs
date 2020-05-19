using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;
using VSX.UniversalVehicleCombat;
using VSX.General;

// This script provides screen space target tracking and information

namespace VSX.UniversalVehicleCombat 
{
	
	/// <summary>
    /// This class provides a way to store widget settings for each of the trackable types.
    /// </summary>
	[System.Serializable]
	public class TargetTracking_WidgetSettings
	{

		public GameObject widgetPrefab;

        public bool fadeUnselectedByDistance = false;
        public bool showOffScreenTargets = true;

        public bool showLabelField = true;

        public bool showValueField = true;

        public bool showBarField = true;

	}


    /// <summary>
    /// This class stores information that is used to update a target tracking widget.
    /// </summary>
    public class TargetTracking_WidgetParameters
	{

		public bool isOnScreen = true;

		public bool isWorldSpace = false;
	
		public Vector3 targetUIPosition = Vector3.zero;
		public Quaternion targetUIRotation;

		public float arrowAngle = 0f;

		public bool showLeadUI = false;

		public Vector3 cameraPosition;
		public Transform cameraTransform;

		public bool isSelectedTarget = false; 
		
		public Color widgetColor = Color.white;

		public float alpha;
		
		public string labelFieldValue;
		public bool showLabelField = true;
		
		public string valueFieldValue;
		public bool showValueField = true;
		
		public float barFieldValue;
		public float barFieldValue2;
		public bool showBarField = true;

		public Vector2 targetMeshSize = Vector2.zero;

		public float scale = 1;
	
		public bool expandingTargetBoxes;

		public List<TargetTracking_GunTargetingInfo> gunTargetingInfo = new List<TargetTracking_GunTargetingInfo>();
		public List<TargetTracking_MissileTargetingInfo> missileTargetingInfo = new List<TargetTracking_MissileTargetingInfo>();

	}


    /// <summary>
    /// This class provides a way to pass lead target info for a gun weapon to the target tracking widget.
    /// </summary>
	public class TargetTracking_GunTargetingInfo
	{

		public Vector3 leadTargetPosition = Vector3.zero;

		public Vector3 leadTargetUIPosition = Vector3.zero;
		public Quaternion leadTargetUIRotation = Quaternion.identity;
		
	}


    /// <summary>
    /// This class provides a way to pass locking info for a missile weapon to the target tracking widget.
    /// </summary>
	public class TargetTracking_MissileTargetingInfo
	{

		public LockState lockState;

		public float lockEventTime;

	}


	/// <summary>
    /// This class manages the target tracking part of the vehicle HUD.
    /// </summary>
	public class HUDTargetTracking : MonoBehaviour {
		
		private Transform cachedTransform;
		public Transform CachedTransform { get { return cachedTransform; } }

		private GameObject cachedGameObject;
		public GameObject CachedGameObject { get { return cachedGameObject; } }

		private HUDManager manager;
		private bool hasManager = false;

		// General settings

		[Tooltip("The UI camera. If none assigned, the main scene camera will be used.")]
		[SerializeField]
        private Camera UICamera;
		Transform UICameraTransform;

		RectTransform canvasRT;

		[Tooltip("Use the geometrical center of the mesh bounds as the target position.")]
        [SerializeField]
        private bool useMeshBoundsCenter = true;

		[Tooltip("Enable aspect ratio of target box")]
        [SerializeField]
        private bool enableAspectRatio = true;

		[Tooltip("Toggle between offscreen arrows displayed in radial pattern near screen center, or at the screen border.")]
        [SerializeField]
        private bool centerOffscreenArrows = false;

        [SerializeField]
        private float centerOffscreenArrowsRadius = 30;
	
		[Tooltip("The fraction of the viewport which the UI will use to display target information.")]
        [SerializeField]
        private Vector2 UIViewportCoefficients = new Vector2(1, 1);
		
		[Tooltip("The minimum alpha value of the UI when fading out by distance.")]
        [SerializeField]
        private float fadeMinAlpha = 0.2f;

		[Tooltip("The maximum alpha value of the UI.")]
        [SerializeField]
        private float fadeMaxAlpha = 1f;

		[Tooltip("The maximum number of new targets that can be added each frame, to ensure smooth performance.")]
        [SerializeField]
        private int maxNewTargetsEachFrame = 1;
		int numTargetsLastFrame;

        [Tooltip("Target boxes expand based on the mesh size of the target")]
        [SerializeField]
        private bool expandingTargetBoxes = true;
		

		LockState currentLockUIState;

		// World space UI settings
		private bool worldSpaceUI = false;

		[Tooltip("Position the UI at the same world space position as the target.")]
        [SerializeField]
        private bool useTargetWorldPositions = true;

        [Tooltip("The world space distance from the camera at which the UI will be positioned.")]
        [SerializeField]
        private float worldSpaceTargetTrackingDistance = 0.5f;

		[Tooltip("A scaling coefficient applied to the widgets to enable them to be easily transferred from screen space mode to world space mode.")]
        [SerializeField]
        private float worldSpaceScaleCoefficient = 1;

		Vector2 m_ViewportSize;
		Vector2 m_ViewportOrigin;
		Vector2 m_ViewportMax;

		Vector2 m_ScreenSize;
		Vector2 m_ScreenOrigin;
		Vector2 m_ScreenMax;

		
		// Widget settings

		public List<Color> colorByTeam = new List<Color>();

		List<List<ITargetTrackingWidget>> usedWidgetsByType = new List<List<ITargetTrackingWidget>>();

		List<List<TargetTracking_WidgetParameters>> usedWidgetParamsByType = new List<List<TargetTracking_WidgetParameters>>();

		public List<TargetTracking_WidgetSettings> widgetSettingsByType = new List<TargetTracking_WidgetSettings>();

		List<int> usedWidgetIndexByType = new List<int>();
		int displayedTargetCount = 0;		

		List<ITrackable> targetsList;

		Transform assistantTransform;

		// Used for calculating the mesh size for target box expansion
		Vector3[] extentsCorners = new Vector3[8];

        private bool targetTrackingDisabled = false;
        public bool TargetTrackingDisabled
        {
            get { return targetTrackingDisabled; }
            set { targetTrackingDisabled = value; }
        }

		


		// Called only on the editor when script is loaded or values are changed
		void OnValidate()
		{	
		
			// Make sure that the widget prefab contains a script implementing the ITargetTrackingWidget interface
			for (int i = 0; i < widgetSettingsByType.Count; ++i)
			{
				if (widgetSettingsByType[i].widgetPrefab != null)
				{
					if (widgetSettingsByType[i].widgetPrefab.GetComponent<ITargetTrackingWidget>() == null)
					{
						Debug.LogError("TargetTracking widget prefab must have a component implementing the ITargetTrackingWidget interface");
					}
				}
			}
		}


		void Awake()
		{
			cachedTransform = transform;
			cachedGameObject = gameObject;

			// Get the canvas RectTransform
			Canvas canvas = transform.GetComponent<Canvas>();
			if (canvas == null)
			{
				Debug.LogError("Unable to find canvas in the hierarchy of the HUDTargetTracking component");	
			}
			canvasRT = canvas.GetComponent<RectTransform>();

			worldSpaceUI = canvas.renderMode == RenderMode.WorldSpace;

			// Check the camera
			if (UICamera == null)
			{
				UICamera = Camera.main;
			}
			if (UICamera != null)
			{
				UICameraTransform = UICamera.transform;
			}
		}

		
		void Start()
		{


            for (int i = 0; i < widgetSettingsByType.Count; ++i)
            {

                if (widgetSettingsByType[i].widgetPrefab == null)
                {
                    Debug.LogError("No widget prefab assigned to the widget type " + Enum.GetNames(typeof(TrackableType))[i] + " in the HUDTargetTracking widget settings");
                }
                else
                {
                    usedWidgetIndexByType.Add(-1);
                    usedWidgetsByType.Add(new List<ITargetTrackingWidget>());
                    usedWidgetParamsByType.Add(new List<TargetTracking_WidgetParameters>());
                }
            }      

			// Get the tracker information
			RadarSceneManager radarSceneManager = GameObject.FindObjectOfType<RadarSceneManager>();
			if (radarSceneManager == null)
			{
				Debug.LogError("Please add a RadarSceneManager component to the scene, cannot link target tracking UI to ITracker component");
			}

			assistantTransform = new GameObject().transform;
			assistantTransform.name = "HUDTargetTrackingAssistantTransform";
			assistantTransform.SetParent(UICameraTransform);
			assistantTransform.localPosition = Vector3.zero;
			assistantTransform.localRotation = Quaternion.identity;

            GameManager gameManager = GameObject.FindObjectOfType<GameManager>();
            if (gameManager != null)
            {
                gameManager.onGameStateChangedEventHandler += OnGameStateChanged;
            }

		}


        void OnGameStateChanged(GameState newGameState)
        {

            targetTrackingDisabled = (newGameState != GameState.Gameplay);


        }


        /// <summary>
        /// Set the HUD manager for the target tracking.
        /// </summary>
        /// <param name="manager">The HUD manager.</param>
		public void SetManager(HUDManager manager)
		{
			this.manager = manager;
			this.hasManager = true;
		}


        /// <summary>
        /// Event called when the HUD that this target tracking component is part of is activated in the scene.
        /// </summary>
        /// <param name="targetTrackingAnchor">The parent anchor for the target tracking UI.</param>
		public void OnActivate(Transform targetTrackingAnchor)
		{
			if (worldSpaceUI)
			{
				transform.SetParent(targetTrackingAnchor);
			}
			else
			{
				transform.SetParent(null);
			}
		}


        /// <summary>
        /// Event called when the HUD that this target tracking component is part of is deactivated in the scene.
        /// </summary>
        /// <param name="holdingParent">The transform to parent the target tracking UI to when the HUD is deactivated.</param>
		public void OnDeactivate(Transform holdingParent)
		{
			transform.SetParent(holdingParent);
		}


        /// <summary>
        /// Get the screen-space-canvas position of a position in space, as well as whether it is in the camera frame
        /// </summary>
        /// <param name="pos">The target position</param>
        /// <param name="isOnScreen">Return whether the target is on screen.</param>
        /// <returns>The canvas position of the target.</returns>
        public Vector3 GetCanvasPosition(Vector3 pos, out bool isOnScreen){
			
			// Necessary to project forward to get correct viewportpoint values
			Vector3 forwardProjectedWorldPos = pos;
			bool targetAhead;

			forwardProjectedWorldPos = UICameraTransform.InverseTransformPoint(pos);
			targetAhead = forwardProjectedWorldPos.z > 0;
			forwardProjectedWorldPos.z = Mathf.Abs(forwardProjectedWorldPos.z);
			forwardProjectedWorldPos = UICameraTransform.TransformPoint(forwardProjectedWorldPos);

			Vector3 viewPortPoint = UICamera.WorldToViewportPoint(forwardProjectedWorldPos);

			// Check if the target is inside the viewport bounds as defined by the UI viewport coefficients
			isOnScreen = targetAhead && (viewPortPoint.x > m_ViewportOrigin.x && viewPortPoint.x < m_ViewportMax.x) && 
						(viewPortPoint.y > m_ViewportOrigin.y && viewPortPoint.y < m_ViewportMax.y);
			
			// Because canvas origin is at center and screen origin is at left bottom, need to offset by half canvas
			Vector3 halfCanvas = 0.5f * (Vector3)canvasRT.sizeDelta;

			// Get the canvas-space position of the target
			Vector3 screenPos = Vector3.Scale (viewPortPoint, (Vector3)canvasRT.sizeDelta) - halfCanvas;
			
			return screenPos;
	
		}


		
		 /// <summary>
        /// Get the screen border position of an off-screen target for a screen space canvas.
        /// </summary>
        /// <param name="screenPos">The screen position (without taking the border into account).</param>
        /// <param name="arrowAngle">Return the off-screen arrow angle.</param>
        /// <returns>The canvas border position of this target.</returns>
		Vector2 GetCanvasBorderPosition(Vector3 screenPos, out float arrowAngle)
		{

			// Get the slope of the screen
			float m_ScreenSlope = m_ScreenSize.y/m_ScreenSize.x;

			// Get the origin and the max position of the borders in canvas space
			Vector2 canvasFactor = new Vector2(canvasRT.sizeDelta.x/Screen.width, canvasRT.sizeDelta.y/Screen.height);
			Vector2 m_CanvasOrigin = Vector3.Scale(m_ScreenOrigin, canvasFactor);
			Vector2 m_CanvasMax = Vector3.Scale(m_ScreenMax, canvasFactor);

			// Slope of the target screen position vector relative to the screen center
			float screenPosSlope = screenPos.x != 0 ? screenPos.y/screenPos.x : 1000000;			// Prevent divide by zero

			// Get the position on the screen border
			Vector2 arrowPos = Vector2.zero;
			if (Mathf.Abs (screenPosSlope) < m_ScreenSlope){ // If the slope is shallower than the screen diagonal, arrow will be on the side of the screen
				
				float factor = ((m_CanvasMax.x - m_CanvasOrigin.x)/2)/Vector3.Magnitude(new Vector3(screenPos.x, 0f, screenPos.z));
				arrowPos = screenPos * factor;

			} else {

				float factor = ((m_CanvasMax.y - m_CanvasOrigin.y)/2)/Vector3.Magnitude(new Vector3(0, screenPos.y, screenPos.z));
				arrowPos = screenPos * factor;	

			}
			
			// z angle of arrow relative to the screen
			arrowAngle = Mathf.Atan2(arrowPos.y, arrowPos.x) * Mathf.Rad2Deg;	

			return arrowPos;

		}



		// This function calculates the screens-space position of an arrow against a screen-centered radial border pointing to an off-screen target
        /// <summary>
        /// Get the position and rotation of an arrow indicating an off-screen target, using the radial display mode for off-screen targets.
        /// </summary>
        /// <param name="screenPos">The screen position of the off-screen target.</param>
        /// <param name="arrowAngle">Return the arrow angle.</param>
        /// <returns>Return the canvas position of the off-screen target indicator.</returns>
		Vector2 GetRadialCanvasPosition(Vector2 screenPos, out float arrowAngle)
		{
			Vector2 arrowPos = Vector2.zero;
			
			arrowPos = screenPos.normalized * centerOffscreenArrowsRadius;

			arrowAngle = Mathf.Atan2(arrowPos.y, arrowPos.x) * Mathf.Rad2Deg;	

			return arrowPos;
		}



        /// <summary>
        /// Get the world space position and rotation of a widget that is sitting on the camera-to-target axis at a given distance from the camera
        /// </summary>
        /// <param name="targetPos">The position of the target.</param>
        /// <param name="distanceToUI">The distance along the view axis.</param>
        /// <param name="uiRotation">Return the rotation of the UI that makes it face directly away from the camera.</param>
        /// <param name="isOnScreen">Return whether the target position is on screen.</param>
        /// <param name="adjustDistanceToAngle">Whether to adjust the distance of the UI along the view axis so that it is always the same
        /// distance regardless of angle (spherical display)</param>
        /// <returns></returns>
        public Vector3 GetWorldPositionOnViewAxis(Vector3 targetPos, float distanceToUI, out Quaternion uiRotation, out bool isOnScreen, bool adjustDistanceToAngle)
		{

			Vector3 toTargetDirection = (targetPos - UICameraTransform.position).normalized;

			uiRotation = Quaternion.LookRotation(toTargetDirection, UICameraTransform.up);

			Vector3 viewPortPos = UICamera.WorldToViewportPoint(targetPos);
			
			// Check if the target is inside the viewport bounds as defined by the UI viewport coefficients
			isOnScreen = viewPortPos.z > 0 && (viewPortPos.x > m_ViewportOrigin.x && viewPortPos.x < m_ViewportMax.x) && (viewPortPos.y > m_ViewportOrigin.y && viewPortPos.y < m_ViewportMax.y);
			
			// Adjust the position by a factor such that the distance to the target along the camera's forward axis is always the same no matter what the angle
			float factor = adjustDistanceToAngle ? 1/Vector3.Dot(UICameraTransform.forward, toTargetDirection) : 1;
			
			return (UICameraTransform.position + (toTargetDirection * distanceToUI * factor));		
			
		}


        /// <summary>
        /// Get the world space position and rotation of an arrow that is pointing at an off-screen target and is up against the border of the screen
        /// </summary>
        /// <param name="worldPos">The target world position</param>
        /// <param name="worldRotation">Return the world rotation for the UI.</param>
        /// <param name="arrowAngle">Return the arrow angle.</param>
        /// <returns>The world space screen border position for an off-screen target.</returns>
        Vector3 GetWorldSpaceScreenBorderPosition(Vector3 worldPos, out Quaternion worldRotation, out float arrowAngle)
		{

			// Necessary to project forward to get correct viewportpoint values
			Vector3 forwardProjectedWorldPos = worldPos;
			forwardProjectedWorldPos = UICameraTransform.InverseTransformPoint(worldPos);
			forwardProjectedWorldPos.z = Mathf.Abs(forwardProjectedWorldPos.z);
			forwardProjectedWorldPos = UICameraTransform.TransformPoint(forwardProjectedWorldPos);
			
			Vector3 viewPortPos = UICamera.WorldToViewportPoint(forwardProjectedWorldPos);
			
			float m_ScreenSlope = m_ScreenSize.y/m_ScreenSize.x;

			Vector3 screenPos = Vector3.Scale(viewPortPos, new Vector3(Screen.width, Screen.height, 0f));
			Vector3 centeredScreenPos = screenPos - (0.5f * new Vector3(Screen.width, Screen.height, 0f));
			float screenPosSlope = centeredScreenPos.y/centeredScreenPos.x;
			
			Vector3 arrowPos = Vector3.zero;

			if (Mathf.Abs (screenPosSlope) < m_ScreenSlope){ // If the slope is shallower than the screen diagonal, arrow will be on the side of the screen

				float factor = ((m_ScreenMax.x - m_ScreenOrigin.x)/2)/Vector3.Magnitude(new Vector3(centeredScreenPos.x, 0f, centeredScreenPos.z));
				arrowPos = centeredScreenPos * factor;

			} else {

				float factor = ((m_ScreenMax.y - m_ScreenOrigin.y)/2)/Vector3.Magnitude(new Vector3(0, centeredScreenPos.y, centeredScreenPos.z));
				arrowPos = centeredScreenPos * factor;

			}
			
			// Calculate the local arrow angle
			arrowAngle = Mathf.Atan2(arrowPos.y, arrowPos.x) * Mathf.Rad2Deg;				

			// Convert back to correct screen space (relative to bottom left corner, not center)
			arrowPos += new Vector3(Screen.width, Screen.height, 0) * 0.5f;

			// Convert to world coordinates
			arrowPos.z = worldSpaceTargetTrackingDistance;
			
			arrowPos = UICamera.ScreenToWorldPoint(arrowPos);
			
			// project along the to-target axis by the UI distance from camera value
			arrowPos = UICameraTransform.position + (arrowPos - UICameraTransform.position).normalized * worldSpaceTargetTrackingDistance;
			
			// return the correct rotation
			Vector3 toTargetDirection = (arrowPos - UICameraTransform.position).normalized;
			worldRotation = Quaternion.LookRotation(toTargetDirection, UICameraTransform.up);
			
			// return the position
			return arrowPos;

		}



        /// <summary>
        /// Get the world position and arrow angle of a center-radial arrow indicating an off-screen target.
        /// </summary>
        /// <param name="worldPos">The world position of the target.</param>
        /// <param name="worldRotation">Get the world rotation for the UI.</param>
        /// <param name="arrowAngle">Get the (local) arrow angle for the off-screen arrow.</param>
        /// <returns>The world position of the UI.</returns>
        Vector3 GetWorldSpaceArrowRadialPosition(Vector3 worldPos, out Quaternion worldRotation, out float arrowAngle)
		{

			// Get the relative position and flatten it on the local z axis, this will give the correct relative xy direction
			Vector3 result = UICameraTransform.InverseTransformPoint(worldPos);
			result.z = 0;

			// Arrow angle relative to screen
			arrowAngle = Mathf.Atan2(result.y, result.x) * Mathf.Rad2Deg;	

			// Scale the relative direction and transfer it back to world space
			result = Vector3.forward + result.normalized * centerOffscreenArrowsRadius;
			Vector3 direction = UICameraTransform.TransformDirection(result.normalized);
			result = UICameraTransform.position + direction * worldSpaceTargetTrackingDistance;

			// return the correct rotation
			worldRotation = Quaternion.LookRotation(direction, UICameraTransform.up);

			return result;
			
		}



		/// <summary>
        /// Toggle center-radial offscreen indicators (not screen border)
        /// </summary>
        /// <param name="centerArrows"></param>
		public void SetOffscreenMode(bool centerArrows)
		{
			centerOffscreenArrows = centerArrows;
		}



		// Get the screen-space distance between two points
		float GetScreenDistance(Vector3 pos1, Vector3 pos2)
		{

			Vector3 screenPos1 = UICameraTransform.InverseTransformPoint(pos1);
			screenPos1.z = Mathf.Abs(screenPos1.z);
			screenPos1 = UICamera.WorldToScreenPoint(UICameraTransform.TransformPoint(screenPos1));
			screenPos1.z = 0;
			
			Vector3 screenPos2 = UICameraTransform.InverseTransformPoint(pos2);
			screenPos2.z = Mathf.Abs(screenPos2.z);
			screenPos2 = UICamera.WorldToScreenPoint(UICameraTransform.TransformPoint(screenPos2));
			screenPos2.z = 0;

			return (Vector3.Magnitude(screenPos1 - screenPos2));

		}


		// Get the target size, treating the bounding box as a sphere (no aspect ratio)
		Vector2 GetDiameterOnScreen (Mesh _mesh, Transform _transform)
		{
			
			Vector3 extents = _mesh.bounds.extents;
			float maxExtents = Mathf.Max(new float[]{ extents.x, extents.y, extents.z}) * _transform.localScale.x;

			assistantTransform.LookAt(_transform.position, UICameraTransform.up);
			Vector3 leftBottom = _transform.position - assistantTransform.right * maxExtents - assistantTransform.up * maxExtents;
			Vector3 rightTop = _transform.position + assistantTransform.right * maxExtents + assistantTransform.up * maxExtents;

			bool tmp;
			leftBottom = GetCanvasPosition(leftBottom, out tmp);
			rightTop = GetCanvasPosition(rightTop, out tmp);
			
			Vector3 max = Vector3.Max(leftBottom, rightTop);
			Vector3 min = Vector3.Min(leftBottom, rightTop);
			return (new Vector3(max.x - min.x, max.y - min.y, 0f));
			
		}

		
		// Get the screen-space size of a mesh displayed by a mesh renderer, using its bounding box
		Vector2 GetSizeOnScreen(Mesh mesh, Transform t)
		{

			// Get the positions of all of the corners of the bounding box
			Vector3 extents = mesh.bounds.extents;
			
			extentsCorners[0] = extents;
			extentsCorners[1] = new Vector3(-extents.x, extents.y, extents.z);
			extentsCorners[2] = new Vector3(extents.x, -extents.y, extents.z);
			extentsCorners[3] = new Vector3(extents.x, extents.y, -extents.z);
			extentsCorners[4] = new Vector3(-extents.x, -extents.y, -extents.z);
			extentsCorners[5] = new Vector3(-extents.x, -extents.y, extents.z);
			extentsCorners[6] = new Vector3(-extents.x, extents.y, -extents.z);
			extentsCorners[7] = new Vector3(extents.x, -extents.y, -extents.z);	


			// Get the screen position of all of the box corners
			for (int i = 0; i < 8; ++i)
			{	
				bool tmp;
				extentsCorners[i] = GetCanvasPosition(t.TransformPoint(mesh.bounds.center + extentsCorners[i]), out tmp);
			}

			// Find the minimum and maximum bounding box corners in screen space
			Vector3 min = extentsCorners[0];
			Vector3 max = extentsCorners[0];
			for (int i = 1; i < 8; ++i)
			{
				min = Vector3.Min(extentsCorners[i], min);
				max = Vector3.Max(extentsCorners[i], max);
			}
			
			return (new Vector3(max.x - min.x, max.y - min.y, 0f));
		}



		// Get the target size, treating the bounding box as a sphere (no aspect ratio)
		Vector2 GetDiameterOnTargetTracking(Mesh mesh, Transform t)
		{
			
			Vector3 extents = mesh.bounds.extents;
			float maxExtents = Mathf.Max(new float[]{ extents.x, extents.y, extents.z}) * t.localScale.x;

			assistantTransform.LookAt(t.position, UICameraTransform.up);
			Vector3 leftBottom = t.position - assistantTransform.right * maxExtents - assistantTransform.up * maxExtents;
			Vector3 rightTop = t.position + assistantTransform.right * maxExtents + assistantTransform.up * maxExtents;

			bool tmp;
			Quaternion tempQuat; // not used
			float dist = useTargetWorldPositions ? Vector3.Distance(UICameraTransform.position, t.position) : worldSpaceTargetTrackingDistance;
			leftBottom = GetWorldPositionOnViewAxis(leftBottom, dist, out tempQuat, out tmp, true);
			rightTop = GetWorldPositionOnViewAxis(rightTop, dist, out tempQuat, out tmp, true);
			
			leftBottom = assistantTransform.InverseTransformPoint(leftBottom);
			rightTop = assistantTransform.InverseTransformPoint(rightTop);

			Vector3 max = Vector3.Max(leftBottom, rightTop);
			Vector3 min = Vector3.Min(leftBottom, rightTop);
			return (new Vector3(max.x - min.x, max.y - min.y, 0f));
			
		}


        
		// Get the world-space size (from the cameras perspective) of a mesh displayed by a mesh renderer in the scene
		Vector2 GetSizeOnTargetTracking(Mesh _mesh, Transform _transform)
		{
			// Get the positions of all of the corners of the bounding box
			Vector3 extents = _mesh.bounds.extents;

			float maxExtents = Mathf.Max(new float[]{ extents.x, extents.y, extents.z });

			if (!enableAspectRatio)
				extents = new Vector3(maxExtents, maxExtents, maxExtents);

			extentsCorners[0] = extents;
			extentsCorners[1] = new Vector3(-extents.x, extents.y, extents.z);
			extentsCorners[2] = new Vector3(extents.x, -extents.y, extents.z);
			extentsCorners[3] = new Vector3(extents.x, extents.y, -extents.z);
			extentsCorners[4] = new Vector3(-extents.x, -extents.y, -extents.z);
			extentsCorners[5] = new Vector3(-extents.x, -extents.y, extents.z);
			extentsCorners[6] = new Vector3(-extents.x, extents.y, -extents.z);
			extentsCorners[7] = new Vector3(extents.x, -extents.y, -extents.z);	
			
			for (int i = 0; i < extentsCorners.Length; ++i)
			{	

				bool tmp;
				Quaternion tempQuat; // not used
				float dist = useTargetWorldPositions ? Vector3.Distance(UICameraTransform.position, _transform.position) : worldSpaceTargetTrackingDistance;
				//Debug.Log(extentsCorners[i]);
				extentsCorners[i] = GetWorldPositionOnViewAxis(_transform.TransformPoint(_mesh.bounds.center + extentsCorners[i]), dist, out tempQuat, out tmp, true);
					
			}
			
			assistantTransform.LookAt(_transform.position, UICameraTransform.up);

			Vector3 min = assistantTransform.InverseTransformPoint(extentsCorners[0]);
			Vector3 max = min;
			
			for (int i = 1; i < 8; ++i)
			{
				Vector3 tmp = assistantTransform.InverseTransformPoint(extentsCorners[i]);
				
				min.x = Mathf.Min(tmp.x, min.x);//min = Vector3.Min(tmp, min);
				min.y = Mathf.Min(tmp.y, min.y);

				max.x = Mathf.Max(tmp.x, max.x);
				max.y = Mathf.Max(tmp.y, max.y);
				
			}

			return (new Vector3(max.x - min.x, max.y - min.y, 0f));

		}
	

		
		// Get another target widget from the pool
		void AddWidget(int typeIndex)
        {

			Transform t = PoolManager.Instance.Get(widgetSettingsByType[typeIndex].widgetPrefab, Vector3.zero, Quaternion.identity).transform;
			usedWidgetsByType[typeIndex].Add(t.GetComponent<ITargetTrackingWidget>());
			usedWidgetParamsByType[typeIndex].Add(new TargetTracking_WidgetParameters());
			t.SetParent(transform);
			t.localScale = new Vector3 (1f, 1f, 1f);

		}



		// Track a target
		void Visualize(ITrackable target, bool isSelectedTarget)
		{
			
			int typeIndex = (int)(target.TrackableType);
			int teamIndex = (int)(target.Team);
			
			float distanceToTarget = Vector3.Distance(manager.FocusedVehicle.CachedTransform.position, target.CachedTransform.position);
			float cameraDistToTarget = Vector3.Distance(UICameraTransform.position, target.CachedTransform.position);
			
			bool isOnScreen = true;
			float arrowAngle = 0f;


			// Get the target position

			Vector3 targetPos;
			if (useMeshBoundsCenter && target.HasBodyMesh)
			{
				targetPos = target.CachedTransform.TransformPoint(target.BodyMesh.bounds.center);
			}
			else
			{
				targetPos = target.CachedTransform.position;
			}


			// First determine if target is onscreen so that you can skip if target tracking is set not to show offscreen targets

			Vector3 targetUIPos;
			Quaternion targetUIRotation = Quaternion.identity;

			if (worldSpaceUI)
			{
				float calcDist = useTargetWorldPositions ? cameraDistToTarget : worldSpaceTargetTrackingDistance;
				targetUIPos = GetWorldPositionOnViewAxis(targetPos, calcDist, out targetUIRotation, out isOnScreen, false);
			}
			else
			{
				targetUIPos = GetCanvasPosition(targetPos, out isOnScreen);
			}

			// If not on screen and target tracking is not set to show offscreen targets, skip
			if (!isOnScreen && !widgetSettingsByType[typeIndex].showOffScreenTargets)
				return;
			

			// Update the last used target box, and add one if there aren't enough
			usedWidgetIndexByType[typeIndex] += 1;
			if (usedWidgetsByType[typeIndex].Count < usedWidgetIndexByType[typeIndex] + 1)
			{
				AddWidget(typeIndex);
			}
			int thisWidgetIndex = usedWidgetIndexByType[typeIndex];


			// Tell the widget if is world space or screen space
			usedWidgetParamsByType[typeIndex][thisWidgetIndex].isWorldSpace = worldSpaceUI;
			usedWidgetParamsByType[typeIndex][thisWidgetIndex].expandingTargetBoxes = expandingTargetBoxes;

			// Pass the camera data to the widget
			usedWidgetParamsByType[typeIndex][thisWidgetIndex].cameraPosition = UICameraTransform.position;
			usedWidgetParamsByType[typeIndex][thisWidgetIndex].cameraTransform = UICameraTransform;

			usedWidgetParamsByType[typeIndex][thisWidgetIndex].isOnScreen = isOnScreen;


			if (!isOnScreen)
			{
				
				if (worldSpaceUI)
				{
					if (centerOffscreenArrows)
					{
						targetUIPos = GetWorldSpaceArrowRadialPosition(targetPos, out targetUIRotation, out arrowAngle);
					}
					else
					{
						targetUIPos = GetWorldSpaceScreenBorderPosition(targetPos, out targetUIRotation, out arrowAngle);
					}
				}
				else
				{
					if (centerOffscreenArrows)
						targetUIPos = GetRadialCanvasPosition(targetUIPos, out arrowAngle);
					else
						targetUIPos = GetCanvasBorderPosition(targetUIPos, out arrowAngle);
				}

				usedWidgetParamsByType[typeIndex][thisWidgetIndex].arrowAngle = arrowAngle;
				
			}


			// Pass position and rotation for the widget

			usedWidgetParamsByType[typeIndex][thisWidgetIndex].targetUIPosition = targetUIPos;
			usedWidgetParamsByType[typeIndex][thisWidgetIndex].targetUIRotation = targetUIRotation;


			// Expanding target box

			if (worldSpaceUI)
			{
				if (target.HasBodyMesh)
				{
					if (enableAspectRatio)
					{
						usedWidgetParamsByType[typeIndex][thisWidgetIndex].targetMeshSize = GetSizeOnTargetTracking(target.BodyMesh, target.CachedTransform);
					}
					else
					{
						usedWidgetParamsByType[typeIndex][thisWidgetIndex].targetMeshSize = GetDiameterOnTargetTracking(target.BodyMesh, target.CachedTransform);
					}
				}
				else
				{
					usedWidgetParamsByType[typeIndex][thisWidgetIndex].targetMeshSize = Vector2.zero;
				}
				float scale = Vector3.Distance(targetUIPos, UICameraTransform.position) * worldSpaceScaleCoefficient;

				scale = Mathf.Max(scale, 0.00001f);	// Prevent errors when setting UI parameters

				usedWidgetParamsByType[typeIndex][thisWidgetIndex].scale = scale;
			}
			else
			{
				if (enableAspectRatio)
				{
					usedWidgetParamsByType[typeIndex][thisWidgetIndex].targetMeshSize = target.HasBodyMesh ? GetSizeOnScreen(target.BodyMesh, target.CachedTransform) : Vector2.zero;	
				}
				else
				{
					usedWidgetParamsByType[typeIndex][thisWidgetIndex].targetMeshSize = target.HasBodyMesh ? GetDiameterOnScreen(target.BodyMesh, target.CachedTransform) : Vector2.zero;	
				}
				
				usedWidgetParamsByType[typeIndex][thisWidgetIndex].scale = 1;
			}
			
			// Lead target and locking information
			usedWidgetParamsByType[typeIndex][thisWidgetIndex].isSelectedTarget = isSelectedTarget;

			if (isSelectedTarget)
			{

				List<TargetTracking_GunTargetingInfo> gunTargetingInfo = new List<TargetTracking_GunTargetingInfo>();
				List<TargetTracking_MissileTargetingInfo> missileTargetingInfo = new List<TargetTracking_MissileTargetingInfo>();
				
				if (manager.FocusedVehicle.HasWeapons && manager.FocusedVehicle.Weapons.HasWeaponsComputer)
				{
					for (int i = 0; i < manager.FocusedVehicle.Weapons.MountedWeaponsComputer.WeaponsComputer.LeadTargetComputer.LeadTargetDataList.Count; ++i)
					{

						TargetTracking_GunTargetingInfo newGunInfo = new TargetTracking_GunTargetingInfo();
						bool isOnScreen2;
		
						float calcDist = useTargetWorldPositions ? cameraDistToTarget : worldSpaceTargetTrackingDistance;
						
						Vector3 leadTargetUIPos;
						Quaternion leadTargetUIRotation = Quaternion.identity;
						if (worldSpaceUI)
						{
							// Get the lead target UI position along the camera -> lead target position vector
							leadTargetUIPos = GetWorldPositionOnViewAxis(manager.FocusedVehicle.Weapons.MountedWeaponsComputer.WeaponsComputer.LeadTargetComputer.LeadTargetDataList[i].LeadTargetPosition, calcDist, out leadTargetUIRotation, out isOnScreen2, false);
						}
						else
						{
							leadTargetUIPos = GetCanvasPosition(manager.FocusedVehicle.Weapons.MountedWeaponsComputer.WeaponsComputer.LeadTargetComputer.LeadTargetDataList[i].LeadTargetPosition, out isOnScreen);
						}

						newGunInfo.leadTargetUIPosition = leadTargetUIPos;
						newGunInfo.leadTargetUIRotation = leadTargetUIRotation;
						
						gunTargetingInfo.Add(newGunInfo);
	
					}
					
					for (int i = 0; i < manager.FocusedVehicle.Weapons.MountedWeaponsComputer.WeaponsComputer.LockingComputer.LockingDataList.Count; ++i)
					{
						TargetTracking_MissileTargetingInfo newMissileInfo = new TargetTracking_MissileTargetingInfo();
						newMissileInfo.lockEventTime = manager.FocusedVehicle.Weapons.MountedWeaponsComputer.WeaponsComputer.LockingComputer.LockingDataList[i].LockEventTime;
						newMissileInfo.lockState = manager.FocusedVehicle.Weapons.MountedWeaponsComputer.WeaponsComputer.LockingComputer.LockingDataList[i].LockState;
						missileTargetingInfo.Add(newMissileInfo);
					}
				}

				
			
				usedWidgetParamsByType[typeIndex][thisWidgetIndex].gunTargetingInfo = gunTargetingInfo;
				usedWidgetParamsByType[typeIndex][thisWidgetIndex].missileTargetingInfo = missileTargetingInfo;

			}


			// Color information
			//Debug.Log((Team)teamIndex + "  " + colorByTeam[teamIndex] + "  " + target.Label);
			usedWidgetParamsByType[typeIndex][thisWidgetIndex].widgetColor = colorByTeam[teamIndex];
			usedWidgetParamsByType[typeIndex][thisWidgetIndex].alpha = 1f;
			if (!isSelectedTarget && widgetSettingsByType[typeIndex].fadeUnselectedByDistance)
			{
				float fraction = distanceToTarget / manager.FocusedVehicle.Radar.Range;
				float alphaRange = fadeMaxAlpha - fadeMinAlpha;
				usedWidgetParamsByType[typeIndex][thisWidgetIndex].alpha = fadeMaxAlpha - fraction * alphaRange;
			}
			
			
			// Widget label field

			usedWidgetParamsByType[typeIndex][thisWidgetIndex].labelFieldValue = target.Label;
			usedWidgetParamsByType[typeIndex][thisWidgetIndex].showLabelField = widgetSettingsByType[typeIndex].showLabelField;


			// Widget value field

			usedWidgetParamsByType[typeIndex][thisWidgetIndex].valueFieldValue = HUDDistanceLookup.Lookup(distanceToTarget);
			usedWidgetParamsByType[typeIndex][thisWidgetIndex].showValueField = widgetSettingsByType[typeIndex].showValueField;


			// Widget bar field

			if (target.HasHealthInfo)
			{

                usedWidgetParamsByType[typeIndex][thisWidgetIndex].barFieldValue = target.HealthInfo.GetCurrentHealthFraction(HealthType.Shield);
                
                usedWidgetParamsByType[typeIndex][thisWidgetIndex].barFieldValue2 = target.HealthInfo.GetCurrentHealthFraction(HealthType.Armor);

            }
			
			usedWidgetParamsByType[typeIndex][thisWidgetIndex].showBarField = target.HasHealthInfo && widgetSettingsByType[typeIndex].showBarField;

		
			// Enable the widget and pass the data
			
			usedWidgetsByType[typeIndex][thisWidgetIndex].Enable();
			usedWidgetsByType[typeIndex][thisWidgetIndex].Set(usedWidgetParamsByType[typeIndex][thisWidgetIndex]);

			displayedTargetCount += 1;		
			
		}


		// Return unused widgets to the pool
		void RemoveExcessWidgets()
		{
			
			// Remove superfluous widgets
			for (int i = 0; i < usedWidgetsByType.Count; ++i){
				int usedCount = usedWidgetIndexByType[i] + 1;
				if (usedWidgetsByType[i].Count > usedCount)
				{
					
					int removeAmount = usedWidgetsByType[i].Count - usedCount;
					
					// Disable the widgets
					for (int j = 0; j < removeAmount; ++j)
					{
						usedWidgetsByType[i][usedCount + j].Disable();
					}

					
					usedWidgetsByType[i].RemoveRange(usedCount, removeAmount);
					usedWidgetParamsByType[i].RemoveRange(usedCount, removeAmount);
					
				}
			}
		}

        // Return all widgets to pool
        void ClearWidgets()
        {
            // Remove widgets
            for (int i = 0; i < usedWidgetsByType.Count; ++i)
            {
                // Disable the widgets
                for (int j = 0; j < usedWidgetsByType[i].Count; ++j)
                {
                    usedWidgetsByType[i][j].Disable();
                }

                // Clear the list
                usedWidgetsByType[i].Clear();
            }
        }


        // Do UI after all physics 
        void LateUpdate()
		{

			// If no tracker, remove all widgets, otherwise get target info
			if (!hasManager || !manager.HasFocusedVehicle || !manager.FocusedVehicle.HasRadar || targetTrackingDisabled)
			{
				ClearWidgets();
				return; 	// If no tracker has been found to display target information from, exit 
			}
			else
			{
				// Get targets
				targetsList = manager.FocusedVehicle.Radar.trackedTargets; // Includes selected target
			}
            
			// Calculate the UIViewportCoefficient-scaled viewport origin/max and screen origin/max once per frame, 
			// so it is not being done for every widget
			Vector2 viewPortSize = new Vector2(1,1);
			Vector2 screenSize = new Vector2 (Screen.width, Screen.height);

			m_ViewportSize = Vector2.Scale(UIViewportCoefficients, viewPortSize);
			m_ViewportOrigin = (viewPortSize - m_ViewportSize) * 0.5f;
			m_ViewportMax = viewPortSize - (viewPortSize - m_ViewportSize) * 0.5f;

			m_ScreenSize = Vector2.Scale(m_ViewportSize, screenSize);
			m_ScreenOrigin = Vector2.Scale(m_ViewportOrigin, screenSize);
			m_ScreenMax = Vector2.Scale(m_ViewportMax, screenSize);
	

			// Clamp the world space target tracking distance to a non-zero value

			worldSpaceTargetTrackingDistance = Mathf.Max(worldSpaceTargetTrackingDistance, 0.0001f);


			// Reset the per-frame target-count and used widget index info

			displayedTargetCount = 0;
			for (int i = 0; i < usedWidgetIndexByType.Count; ++i)
			{
				usedWidgetIndexByType[i] = -1;
			}
			
			// Visualise targets
			for (int i = 0; i < targetsList.Count; ++i)
			{

				// Don't add a widget for the tracker itself
				if ((targetsList[i].CachedGameObject == manager.FocusedVehicle.CachedGameObject) || targetsList[i].Equals(null))
				{
					continue;
				}
				
				Visualize(targetsList[i], targetsList[i] == manager.FocusedVehicle.Radar.SelectedTarget);
			
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