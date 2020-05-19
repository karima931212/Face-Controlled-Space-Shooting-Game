using UnityEngine;
using UnityEngine.UI;
using System.Collections;


namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// This class provides a way to efficiently store a UI Image element, caching important components.
    /// </summary>
	public class HUDImageObject
    {

        private RectTransform cachedRectTransform;
		public RectTransform CachedRectTransform { get { return cachedRectTransform; } }

		private GameObject cachedGameObject;
        public GameObject CachedGameObject { get { return cachedGameObject; } }

		private Image cachedImage;
        public Image CachedImage {  get { return cachedImage; } }

		
		public HUDImageObject(Image img)
		{
			cachedImage = img;
            cachedGameObject = img.gameObject;
            cachedRectTransform = img.rectTransform;
		}
	}
}