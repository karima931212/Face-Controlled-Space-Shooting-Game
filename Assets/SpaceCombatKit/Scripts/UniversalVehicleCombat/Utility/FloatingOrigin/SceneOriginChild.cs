using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat
{

    public class SceneOriginChild : MonoBehaviour
    {

        private List<TrailRenderer> trailRenderers = new List<TrailRenderer>();

        // Use this for initialization
        void Awake()
        {

            if (SceneOriginManager.Instance != null)
            {

                // Register this scene origin child
                SceneOriginManager.Instance.Register(this);

                // Parent to scene origin manager if not already
                if (transform.root != SceneOriginManager.Instance.CachedTransform)
                {
                    transform.SetParent(SceneOriginManager.Instance.CachedTransform);
                    transform.localPosition = transform.position;
                }

                // Provide a reference to the scene origin manager transform for all scene origin users
                ISceneOriginUser[] sceneOriginUsers = transform.GetComponentsInChildren<ISceneOriginUser>();
                foreach (ISceneOriginUser sceneOriginUser in sceneOriginUsers)
                {
                    sceneOriginUser.OriginReference = SceneOriginManager.Instance.CachedTransform;
                }
            }

            trailRenderers = new List<TrailRenderer>(transform.GetComponentsInChildren<TrailRenderer>());

        }

        /// <summary>
        /// Called before an origin shift.
        /// </summary>
        public void OnPreOriginShift()
        {
            for (int i = 0; i < trailRenderers.Count; ++i)
            {
                trailRenderers[i].Clear();
            }
        }

        /// <summary>
        /// Called after an origin shift.
        /// </summary>
        public void OnPostOriginShift()
        {
            
        }
    }
}