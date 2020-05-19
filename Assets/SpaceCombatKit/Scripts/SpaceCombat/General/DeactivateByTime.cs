using UnityEngine;
using System.Collections;
using VSX.General;


namespace VSX.UniversalVehicleCombat
{

	/// <summary>
    /// This class disables a GameObject after a period of time, providing a way to return it to a pool.
    /// </summary>
	public class DeactivateByTime : MonoBehaviour {
		
		private GameObject cachedGameObject;
		public GameObject CachedGameObject { get { return cachedGameObject; } }
	
		private Transform cachedTransform;
		public Transform CachedTransform { get { return cachedTransform; } }
	
        [SerializeField]
		private float lifeTime;
        private float lifeStartTime;
	


		void Awake()
		{
			cachedTransform = transform;
			cachedGameObject = gameObject;
		}


        // Called when gameobject is enabled
		void OnEnable()
		{
            lifeStartTime = Time.time;
		}
	

        // Called every frame
        private void Update()
        {
            if (Time.time - lifeStartTime > lifeTime)
            {
                cachedGameObject.SetActive(false);
            }
        }

    }
}