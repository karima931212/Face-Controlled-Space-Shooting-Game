using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using VSX.General;

// This script registers and tracks all the ITrackable objects in the scene and provides ITracker objects with a list of the
// ITrackable objects in range

namespace VSX.UniversalVehicleCombat 
{

	/// <summary>
    /// This class is a singleton that provides a way to easily access information about all the trackables
    /// in the scene.
    /// </summary>
	public class RadarSceneManager : MonoBehaviour 
	{
	
		// A list of all the trackables in the scene
		List<ITrackable> trackables = new List<ITrackable>();
		
		public static RadarSceneManager Instance;
		

		void Awake()
		{
		
			// Get singleton reference
			if (Instance == null) 
				Instance = this;
			else 
				Destroy(gameObject);

		}


		/// <summary>
        /// Register a new trackable in the scene.
        /// </summary>
        /// <param name="newTrackable">The new trackable to be registered.</param>
		public void Register(ITrackable newTrackable)
		{
			trackables.Add(newTrackable);
		}
		

		/// <summary>
        /// Unregister a trackable from the scene.
        /// </summary>
        /// <param name="removedTrackable">The trackable to be removed from the scene</param>
		public void Unregister(ITrackable removedTrackable)
		{
			int index = trackables.IndexOf(removedTrackable);
			
			if (index != -1) trackables.RemoveAt(index);
		}

		
        /// <summary>
        /// Destroy a trackable in the scene.
        /// </summary>
        /// <param name="trackableToDestroy">The trackable to destroy.</param>
		public void DestroyTrackable(ITrackable trackableToDestroy)
		{
	
			// Unregister
			Unregister(trackableToDestroy);
			
			// Start a coroutine to destroy it
			StartCoroutine(DestroyTrackableCoroutine(trackableToDestroy));			

		}


        /// <summary>
        /// Coroutine for destroying trackable object after 1 frame
        /// </summary>
        /// <param name="trackableToDestroy">The trackable to destroy.</param>
        /// <returns>Null.</returns>
		IEnumerator DestroyTrackableCoroutine(ITrackable trackableToDestroy)
		{

			trackableToDestroy.CachedGameObject.SetActive(false);

			// Wait one frame before destroying
			yield return null;
			
			Destroy(trackableToDestroy.CachedGameObject);

		}


        /// <summary>
        /// Get a list of trackables that satisfy certain parameters.
        /// </summary>
        /// <param name="targetsList">The trackable list to update.</param>
        /// <param name="selectionPriority">The Radar Selection Priority for the target search.</param>
        /// <param name="trackerTeam">The team that the tracker is on.</param>
        /// <param name="trackerPosition">The tracker position.</param>
        /// <param name="trackerRange">The tracker's range.</param>
        public void GetTrackables(List<ITrackable> targetsList, RadarSelectionPriority selectionPriority, Team trackerTeam, 
			Vector3 trackerPosition = default(Vector3),  float trackerRange = -1)

		{
			
			// Reference to the last used index in the list to update, for use when trimming excess off the end
			int usedIndex = -1;
			
			for (int i = 0; i < trackables.Count; ++i)
			{
				if (!trackables[i].Equals(null) && trackables[i].CachedGameObject.activeSelf && trackables[i].TrackableEnabled)
				{
					
					// Make sure target is in range if ITracker is specified
					if (!trackables[i].IgnoreTrackingDistance && trackerRange > 0 && Vector3.Distance(trackerPosition, trackables[i].CachedTransform.position) > trackerRange)
					{
						continue;
					}
					
					// Make sure target is part of the target team(s) specified by the selection ID
					bool skip = false;
					switch (selectionPriority)
					{
						case RadarSelectionPriority.Hostile:
							if (trackables[i].Team == trackerTeam){
								skip = true;
							}
							break;
						case RadarSelectionPriority.NonHostile:
							if (trackables[i].Team != trackerTeam){
								skip = true;
							}
							break;
					}
					
					if (skip) continue;

					usedIndex += 1;

					if (usedIndex >= targetsList.Count)
					{

						targetsList.Add(trackables[i]);
						
					}
					else
					{

						targetsList[usedIndex] = trackables[i];
						
					}
				}
			}
			
			// Remove excess references
			if (targetsList.Count > usedIndex + 1)
			{
				int removeAmount = targetsList.Count - (usedIndex + 1);
				targetsList.RemoveRange(usedIndex + 1, removeAmount);
			}
		}


		/// <summary>
        /// Get all the trackables in the scene, optionally only the active ones.
        /// </summary>
        /// <param name="targetsList">The targets list to update.</param>
        /// <param name="getOnlyActive">Whether to get only the active trackables.</param>
		public void GetTrackablesInScene(List<ITrackable> targetsList, bool getOnlyActive)
		{
			
			// Reference to the last used index in the list to update, for use when trimming excess off the end
			int usedIndex = -1;

			for (int i = 0; i < trackables.Count; ++i)
			{

				if (trackables[i].Equals(null) || getOnlyActive && !trackables[i].CachedGameObject.activeSelf)
				{
					continue;
				}

				usedIndex += 1;

				if (usedIndex >= targetsList.Count)
				{

					targetsList.Add(trackables[i]);
					
				}
				else
				{

					targetsList[usedIndex] = trackables[i];
					
				}
			}
			
			// Remove excess references
			if (targetsList.Count > usedIndex + 1)
			{
				int removeAmount = targetsList.Count - (usedIndex + 1);
				targetsList.RemoveRange(usedIndex + 1, removeAmount);
			}
		}

	}
}
