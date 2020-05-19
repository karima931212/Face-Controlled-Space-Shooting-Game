using UnityEngine;
using System.Collections;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// This class provides a way to efficiently store a SpriteRenderer element, caching important components.
    /// </summary>
	public class HUDSpriteObject
    {
	
		private Transform cachedTransform;
        public Transform CachedTransform { get { return cachedTransform; } }

        private GameObject cachedGameObject;
        public GameObject CachedGameObject { get { return cachedGameObject; } }

        private SpriteRenderer cachedSpriteRenderer;
        public SpriteRenderer CachedSpriteRenderer { get { return cachedSpriteRenderer; } }


        public HUDSpriteObject(SpriteRenderer spriteRenderer)
		{
            cachedGameObject = spriteRenderer.gameObject;
			cachedTransform = spriteRenderer.transform;
			cachedSpriteRenderer = spriteRenderer;
		}
	}
}
