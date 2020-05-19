using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// This class provides a way to efficiently store a Text element, caching important components.
    /// </summary>
    /// </summary>
	public class HUDTextObject
    {

        private RectTransform cachedRectTransform;
        public RectTransform CachedRectTransform { get { return cachedRectTransform; } }

        private GameObject cachedGameObject;
        public GameObject CachedGameObject { get { return cachedGameObject; } }

        private Text cachedText;
        public Text CachedText { get { return cachedText; } }


        public HUDTextObject(Text txt)
        {
            cachedText = txt;
            cachedGameObject = txt.gameObject;
            cachedRectTransform = txt.rectTransform;
        }
    }
}