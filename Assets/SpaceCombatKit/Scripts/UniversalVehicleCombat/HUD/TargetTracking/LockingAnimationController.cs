using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// This class manages a locking animation on a target tracking widget.
    /// </summary>
	public class LockingAnimationController : MonoBehaviour
    {


        [SerializeField]
        private float animationTime;

        [SerializeField]
        private float lockingMargin;
		
		RectTransform cachedRectTransform;
		public RectTransform CachedRectTransform { get { return cachedRectTransform; } }
	
		private GameObject cachedGameObject;
		public GameObject CachedGameObject { get { return cachedGameObject; } }

        private Image img;
	
	
		// Use this for initialization
		void Awake () {
	
			cachedRectTransform = GetComponent<RectTransform>();
			cachedGameObject = gameObject;
            img = GetComponent<Image>();

		}
	
        /// <summary>
        /// Update the locking animation controller.
        /// </summary>
        /// <param name="info">Locking information.</param>
        /// <param name="targetBoxSize">The size of the target box.</param>
		public void Set(TargetTracking_MissileTargetingInfo info, Vector2 targetBoxSize)
        {

            Vector2 nextSize = targetBoxSize;
            float amount = (Time.time - info.lockEventTime) / animationTime;

            switch (info.lockState)
            {
                case LockState.Locked:

                    nextSize = targetBoxSize + (1 - Mathf.Min(amount, 1)) * new Vector2(lockingMargin, lockingMargin);
                    img.enabled = true;
                    break;

                case LockState.Locking:

                    nextSize = targetBoxSize + new Vector2(lockingMargin, lockingMargin);
                    img.enabled = true;
                    break;

                default:

                    img.enabled = false;
                    break;

            }

            cachedRectTransform.sizeDelta = nextSize;

        }


        /// <summary>
        /// Set the color of the locking widget.
        /// </summary>
        /// <param name="newColor">The new color for the locking widget.</param>
        public void SetColor(Color newColor)
        {
            img.color = newColor;
        }
	}
}
