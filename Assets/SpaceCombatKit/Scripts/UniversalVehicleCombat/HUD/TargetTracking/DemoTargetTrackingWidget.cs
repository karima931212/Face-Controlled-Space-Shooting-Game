using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using VSX.General;


namespace VSX.UniversalVehicleCombat
{

    /// <summary>
    /// This enum contains the different display states for a target tracking widget.
    /// </summary>
	enum TargetTrackingWidgetState
	{
		OnScreenSelected,
		OnScreenUnselected,
		OffScreenSelected,
		OffScreenUnselected
	}


    /// <summary>
    /// This class manages a target tracking widget on the HUD.
    /// </summary>
	[RequireComponent(typeof(CanvasGroup))]
	public class DemoTargetTrackingWidget : MonoBehaviour, ITargetTrackingWidget
    {
	
		private GameObject cachedGameObject;
		public GameObject CachedGameObject { get { return cachedGameObject; } }
		
		private Transform cachedTransform;
		public Transform CachedTransform { get { return cachedTransform; } }
	
		private RectTransform cachedRectTransform;
		
		// For fading out many canvas objects at the same time
		CanvasGroup canvasGroup;
	
		[Header("Target Box")]

		[SerializeField]
		private Image targetBoxImage;
		private HUDImageObject targetBoxImageObject;

		[SerializeField]
		private float minTargetBoxSize;

        [SerializeField]
        private Vector2 defaultTargetBoxSize;

        Vector2 nextTargetBoxSize;

        [SerializeField]
        private float targetBoxBuffer;
	
		[Header("Lead Target Box")]

		[SerializeField]
		private float minLeadTargetOffset;
	
		[Header("Lead Target Line")]

		[SerializeField]
		private Image leadTargetLineImage;
		private HUDImageObject leadTargetLineImageObject;
	
		[Header("Offscreen Arrow")]

		[SerializeField]
		private Image arrowImage;

		[SerializeField]
		private Sprite unselectedArrowSprite;

		[SerializeField]
		private Sprite selectedArrowSprite;

		private HUDImageObject arrowImageObject;

		[Header("Label")]

		[SerializeField]
		private Text labelText;
		private HUDTextObject labelTextObject;

		[SerializeField]
		private Vector2 labelFieldOffset;
	
		[Header("Distance")]

		[SerializeField]
		private Text distanceText;
		private HUDTextObject distanceTextObject;
		public float displayKMThreshold;

		[SerializeField]
		private Vector2 onScreenValueFieldOffset;

		[Header("Health Bar")]

		float previousBarVal = 0;
		float previousBarVal2 = 0;

		[SerializeField]
		private Image healthBarImage;
		private HUDImageObject healthBarImageObject;

		[SerializeField]
		private Image healthBarBackgroundImage;
		private HUDImageObject healthBarBackgroundImageObject;

		[SerializeField]
		private Image healthBarImage2;
		private HUDImageObject healthBarImageObject2;

		[SerializeField]
		private Image healthBarBackgroundImage2;
		private HUDImageObject healthBarBackgroundImageObject2;

		[SerializeField]
		private Vector2 barFieldOffset;
	
		[Header("Lock")]

		[SerializeField]
		private Image lockImage;
		private HUDImageObject lockImageObject;

		[SerializeField]
		private Text numLocksText;
		private HUDTextObject numLocksTextObject;
	
		Coroutine lockAnimCoroutine;

		enum LockAnimationState
        {
			Off,
			Running,
			Finished
		}
		
		[SerializeField]
		private float lockAnimSpeed = 0.2f;
		[SerializeField]
		public float lockingOffset = 15f;
		[SerializeField]
		public float lockedOffset = 5f;

		public GameObject leadTargetBoxPrefab;
		List<HUDImageObject> leadTargetBoxList = new List<HUDImageObject>();

		public GameObject lockingAnimationPrefab;
		List<LockingAnimationController> lockingAnimationsList = new List<LockingAnimationController>();
		
	
		void Awake()
		{
	
			// Prepare all of the image and text objects for easy access

			cachedRectTransform = GetComponent<RectTransform>();
			cachedGameObject = gameObject;
			cachedTransform = transform;

            // Create the storage class instances for the different parts of the widget
		
			targetBoxImageObject = new HUDImageObject(targetBoxImage);
			
			leadTargetLineImageObject = new HUDImageObject(leadTargetLineImage);
			
			arrowImageObject = new HUDImageObject(arrowImage);

			lockImageObject = new HUDImageObject(lockImage);
	
			healthBarImageObject = new HUDImageObject(healthBarImage);
            healthBarImageObject.CachedImage.fillAmount = 0;

            healthBarImageObject2 = new HUDImageObject(healthBarImage2);
            healthBarImageObject2.CachedImage.fillAmount = 0;

            healthBarBackgroundImageObject = new HUDImageObject(healthBarBackgroundImage);

			healthBarBackgroundImageObject2 = new HUDImageObject(healthBarBackgroundImage2);

			distanceTextObject = new HUDTextObject(distanceText);
	
			labelTextObject = new HUDTextObject(labelText);

			numLocksTextObject = new HUDTextObject(numLocksText);


			// Used to dynamically adjust alpha of multiple elements
			canvasGroup = GetComponent<CanvasGroup>();

		}

	
		/// <summary>
        /// Anything that needs to be done when the widget is enabled.
        /// </summary>
		public void Enable()
		{
            cachedGameObject.SetActive(true);
		}


        /// <summary>
        /// Anything that needs to be done when the widget is disabled.
        /// </summary>
        public void Disable()
		{
            StopAllCoroutines();
			cachedGameObject.SetActive(false);
		}


        /// <summary>
        /// Set the number of lead target boxes to be displayed on this widget.
        /// </summary>
        /// <param name="newSize">The new size of the list of lead target boxes on this widget.</param>
		void UpdateLeadTargetBoxList(int newSize)
		{

			if (leadTargetBoxList.Count == newSize)
				return;
	
			newSize = Mathf.Max(0, newSize);

            // If too many, remove one
			if (leadTargetBoxList.Count > newSize)
			{
				for (int i = 0; i < leadTargetBoxList.Count - newSize; ++i)
				{
					leadTargetBoxList[leadTargetBoxList.Count - 1].CachedGameObject.SetActive(false);	// return to pool
					leadTargetBoxList.RemoveAt(leadTargetBoxList.Count - 1);
				}
			}
            // If not enough, add necessary number
			else
			{
				int num = newSize - leadTargetBoxList.Count;
				for (int i = 0; i < num; ++i)
				{
					Image img = PoolManager.Instance.Get(leadTargetBoxPrefab, Vector3.zero, Quaternion.identity, transform).GetComponent<Image>();
					HUDImageObject imgObject = new HUDImageObject(img);
					imgObject.CachedRectTransform.localScale = new Vector3 (1,1,1);
					imgObject.CachedRectTransform.localRotation = Quaternion.identity;
					leadTargetBoxList.Add(imgObject);
				}
			}
		}	

		
        /// <summary>
        /// Update the number of locking animations on the widget.
        /// </summary>
        /// <param name="newSize">The new number of locking animations on the widget.</param>
		void UpdateLockingAnimationsList(int newSize)
		{

			if (lockingAnimationsList.Count == newSize)
				return;
	
			newSize = Mathf.Max(0, newSize);

            // If too many, remove one
			if (lockingAnimationsList.Count > newSize)
			{
				for (int i = 0; i < lockingAnimationsList.Count - newSize; ++i)
				{
					lockingAnimationsList[lockingAnimationsList.Count - 1].CachedGameObject.SetActive(false);
					lockingAnimationsList.RemoveAt(lockingAnimationsList.Count - 1);
				}
			}
            // If not enough, add necessary number
			else
			{
				int num = newSize - lockingAnimationsList.Count;
				for (int i = 0; i < num; ++i)
				{
					LockingAnimationController controller = PoolManager.Instance.Get(lockingAnimationPrefab, Vector3.zero, Quaternion.identity, 
                                                                                        transform).GetComponent<LockingAnimationController>();
					controller.CachedRectTransform.anchoredPosition3D = Vector3.zero;
					controller.CachedRectTransform.localScale = new Vector3(1,1,1);
					lockingAnimationsList.Add(controller);
				}
			}
		}	


        /// <summary>
        /// Update a widget with target information from the HUD Target Tracking
        /// </summary>
        /// <param name="widgetParameters">The parameters for this widget.</param>
		public void Set(TargetTracking_WidgetParameters widgetParameters)
		{
			
			// Set position and rotation of entire widget
			if (widgetParameters.isWorldSpace)
			{
				cachedRectTransform.position = widgetParameters.targetUIPosition;
			}
			else
			{
				cachedRectTransform.anchoredPosition3D = widgetParameters.targetUIPosition;
			}

			cachedRectTransform.rotation = widgetParameters.targetUIRotation;

			// Set the scale of the entire widget
			cachedRectTransform.localScale = new Vector3(widgetParameters.scale, widgetParameters.scale, widgetParameters.scale);
			
			SetWidgetState(widgetParameters.isOnScreen, widgetParameters.isSelectedTarget, widgetParameters.gunTargetingInfo.Count > 0);


			// Do settings depending on on or off screen
			if (widgetParameters.isOnScreen)
			{

				// Set the target box size
				if (widgetParameters.expandingTargetBoxes)
				{

					// Set the target box size value
					nextTargetBoxSize = widgetParameters.targetMeshSize / widgetParameters.scale + new Vector2(targetBoxBuffer * 2, targetBoxBuffer * 2);
                    nextTargetBoxSize.x = Mathf.Max(nextTargetBoxSize.x, minTargetBoxSize);
                    nextTargetBoxSize.y = Mathf.Max(nextTargetBoxSize.y, minTargetBoxSize);

				}
				else
				{
                    nextTargetBoxSize = Vector2.Max(defaultTargetBoxSize, new Vector2(minTargetBoxSize, minTargetBoxSize));
				}
				
				targetBoxImageObject.CachedRectTransform.sizeDelta = nextTargetBoxSize;
				
				// If selected target
				if (widgetParameters.isSelectedTarget)
				{

					// Update the locking animations
					UpdateLockingAnimationsList(widgetParameters.missileTargetingInfo.Count);
					Vector2 lockBoxSize = nextTargetBoxSize + new Vector2(lockedOffset, lockedOffset);
					bool showLock = false;
					int numLocks = 0;
					
					for (int i = 0; i < widgetParameters.missileTargetingInfo.Count; ++i)
					{
                        lockingAnimationsList[i].Set(widgetParameters.missileTargetingInfo[i], nextTargetBoxSize);
                        lockingAnimationsList[i].SetColor(widgetParameters.widgetColor);
                        if (widgetParameters.missileTargetingInfo[i].lockState == LockState.Locked) numLocks += 1;
					}
					lockImageObject.CachedRectTransform.sizeDelta = lockBoxSize;
					lockImageObject.CachedGameObject.SetActive(showLock);	
					numLocksTextObject.CachedText.text = numLocks.ToString();


					// Update the lead target boxes
					UpdateLeadTargetBoxList(widgetParameters.gunTargetingInfo.Count);
					int maxOffsetIndex = widgetParameters.gunTargetingInfo.Count > 0 ? 0 : -1;
					float maxOffsetMagnitude = 0;



					for (int i = 0; i < widgetParameters.gunTargetingInfo.Count; ++i)
					{
						Vector3 offset = widgetParameters.gunTargetingInfo[i].leadTargetUIPosition - widgetParameters.targetUIPosition;
						float offsetMagnitude = Vector3.Magnitude(offset);
						if (offsetMagnitude > maxOffsetMagnitude)
						{
							maxOffsetMagnitude = offsetMagnitude;
							maxOffsetIndex = i;
						}
	
						if (widgetParameters.isWorldSpace)
						{
							leadTargetBoxList[i].CachedRectTransform.position = widgetParameters.gunTargetingInfo[i].leadTargetUIPosition;
						}
						else
						{
							leadTargetBoxList[i].CachedRectTransform.anchoredPosition3D = offset / widgetParameters.scale;
						}
					}
					
					if (maxOffsetIndex >= 0)
					{

						// Update the lead target line
						Vector2 size = leadTargetLineImageObject.CachedRectTransform.sizeDelta;
						size.x = maxOffsetMagnitude / widgetParameters.scale;
						leadTargetLineImageObject.CachedRectTransform.sizeDelta = size;

						Vector3 linePos = (widgetParameters.gunTargetingInfo[maxOffsetIndex].leadTargetUIPosition + widgetParameters.targetUIPosition) / 2f;
						Vector3 lineOffset = (widgetParameters.gunTargetingInfo[maxOffsetIndex].leadTargetUIPosition - widgetParameters.targetUIPosition) / 2f;

						if (widgetParameters.isWorldSpace)
						{
							leadTargetLineImageObject.CachedRectTransform.position = linePos;
						}
						else
						{
							leadTargetLineImageObject.CachedRectTransform.anchoredPosition3D = lineOffset;
						}
	
						Vector3 lineForwardVector = widgetParameters.isWorldSpace ? (linePos - widgetParameters.cameraPosition).normalized : Vector3.forward;
						Vector3 lineUpVector = Quaternion.AngleAxis(90f, lineForwardVector) * lineOffset.normalized;

						leadTargetLineImageObject.CachedRectTransform.LookAt(leadTargetLineImageObject.CachedRectTransform.position + lineForwardVector, lineUpVector);

					}
				
				}
				
			}

			arrowImageObject.CachedRectTransform.localRotation = Quaternion.Euler(0f, 0f, widgetParameters.arrowAngle);

			// Set label field values
			labelText.text = widgetParameters.labelFieldValue;
			labelTextObject.CachedGameObject.SetActive(widgetParameters.showLabelField && widgetParameters.isOnScreen);
			
			// Set value field values
			distanceText.text = widgetParameters.valueFieldValue;
			distanceTextObject.CachedGameObject.SetActive(widgetParameters.showValueField && widgetParameters.isOnScreen);
			
			// Only update fill amount when necessary as it causes some GC overhead
			if (Mathf.Abs(widgetParameters.barFieldValue - previousBarVal) > 0.0001f)
			{
				healthBarImage.fillAmount = widgetParameters.barFieldValue;
				previousBarVal = widgetParameters.barFieldValue;
			}

			// Only update fill amount when necessary as it causes some GC overhead
			if (Mathf.Abs(widgetParameters.barFieldValue2 - previousBarVal2) > 0.0001f)
			{
				healthBarImage2.fillAmount = widgetParameters.barFieldValue2;
				previousBarVal2 = widgetParameters.barFieldValue2;
			}
			
			// Update bar field
			healthBarImageObject.CachedGameObject.SetActive(widgetParameters.showBarField && widgetParameters.isOnScreen);
			healthBarBackgroundImageObject.CachedGameObject.SetActive(widgetParameters.showBarField && widgetParameters.isOnScreen);

			healthBarImageObject2.CachedGameObject.SetActive(widgetParameters.showBarField && widgetParameters.isOnScreen);
			healthBarBackgroundImageObject2.CachedGameObject.SetActive(widgetParameters.showBarField && widgetParameters.isOnScreen);
			
			
			// Color
			targetBoxImage.color = widgetParameters.widgetColor;
			arrowImage.color = widgetParameters.widgetColor;
			lockImage.color = widgetParameters.widgetColor;

			for (int i = 0; i < leadTargetBoxList.Count; ++i)
			{
				leadTargetBoxList[i].CachedImage.color = widgetParameters.widgetColor;
			}
			leadTargetLineImage.color = widgetParameters.widgetColor;
			healthBarImage.color = widgetParameters.widgetColor;
			healthBarImage2.color = widgetParameters.widgetColor;
			labelText.color = widgetParameters.widgetColor;
			distanceText.color = widgetParameters.widgetColor;
			numLocksTextObject.CachedText.color = widgetParameters.widgetColor;

			canvasGroup.alpha = widgetParameters.alpha;

		}


        /// <summary>
        /// Set the current widget state.
        /// </summary>
        /// <param name="isOnScreen">Whether this widget is on screen.</param>
        /// <param name="isSelected">Whether this widget is the representing the selected target.</param>
        /// <param name="hasLeadTargetInfo">Whether this widget is showing lead target info.</param>
		void SetWidgetState(bool isOnScreen, bool isSelected, bool hasLeadTargetInfo)
		{

			targetBoxImageObject.CachedGameObject.SetActive(isOnScreen);
			leadTargetLineImageObject.CachedGameObject.SetActive(isOnScreen && isSelected && hasLeadTargetInfo);
			lockImageObject.CachedGameObject.SetActive(isOnScreen && isSelected );
			numLocksTextObject.CachedGameObject.SetActive(isOnScreen && isSelected);

			if (!isOnScreen || !isSelected)
			{ 
				UpdateLeadTargetBoxList(0);
				UpdateLockingAnimationsList(0);
			}
			arrowImageObject.CachedGameObject.SetActive(!isOnScreen);
			arrowImageObject.CachedImage.sprite = isSelected ? selectedArrowSprite : unselectedArrowSprite;
		
		}


		// Coroutine for lock animation
		IEnumerator LockAnimation()
		{

			// Reset
			lockImageObject.CachedRectTransform.sizeDelta = nextTargetBoxSize + new Vector2(lockingOffset * 2f, lockingOffset * 2f);
			float startTime = Time.time;
	
			// Animate the locking box
			while (Time.time - startTime < lockAnimSpeed)
			{
				// Shrink over time
				float fraction = (Time.time - startTime)/lockAnimSpeed;
				float currentOffset = fraction * lockedOffset + (1-fraction) * lockingOffset;
				lockImageObject.CachedRectTransform.sizeDelta = nextTargetBoxSize + new Vector2(currentOffset * 2f, currentOffset * 2f);
				yield return null;
			}
		}
	}
}