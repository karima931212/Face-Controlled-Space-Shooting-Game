using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using VSX.UniversalVehicleCombat;

//This script provides target tracking and selection for a vehicle
namespace VSX.UniversalVehicleCombat 
{
	
		/// <summary>
        /// The mode for searching for a new selected target
        /// </summary>
		public enum RadarSelectionMode
		{
			Next,
            Previous,
			Nearest,
			Front
		}

		/// <summary>
        /// The priority types of target to search for.
        /// </summary>
		public enum RadarSelectionPriority
		{
			Hostile,
			NonHostile,
			Any
		}

    /// <summary>
    /// This class represents the radar subsystem of a vehicle, and provides it with the ability to track and select targets
    /// in the scene.
    /// </summary>
	public class Radar : Subsystem  
	{
	
		[Header("Radar Parameters")]

		[SerializeField]
		private List <TrackableType> selectableTypes;
		public List<TrackableType> SelectableTypes { get { return selectableTypes; } } 

		[SerializeField]
		private RadarSelectionMode defaultRadarSelectionMode = RadarSelectionMode.Next;
		
		[SerializeField]
		private float range = 1000;
		public float Range { get { return range; } }

		[SerializeField]
		public float frontTargetAngle = 10f;
		public float FrontTargetAngle { get { return frontTargetAngle; } }

		ITrackable selectedTarget;
		public ITrackable SelectedTarget { get { return (selectedTarget); } }

		bool hasSelectedTarget;
		public bool HasSelectedTarget { get { return (hasSelectedTarget); } }
		
		[HideInInspector]
		public List<ITrackable> trackedTargets = new List<ITrackable>();

	
		[Header("Audio")]

		public AudioClip switchTargetAudioClip;
		bool hasSwitchTargetAudioClip = false;

		public AudioClip enemyDetectedAudioClip;
		bool hasEnemyDetectedAudioClip = false;	
	
		public AudioClip friendlyDetectedAudioClip;
		bool hasFriendlyDetectedAudioClip = false;	

		bool enemiesCurrentlyDetected = false;
		bool friendliesCurrentlyDetected = false;

		[SerializeField]
		private AudioSource audioSource;
		bool hasAudioSource = false;

		[HideInInspector]
		public List<IMissileThreat> missileThreats = new List<IMissileThreat>();

		Vehicle vehicle;


		void Awake()
		{

			// Get the trackable associated with this tracker
			vehicle = GetComponent<Vehicle>();
			
			if (vehicle == null)
			{
				Debug.LogError("A component implementing the ITracker interface must also be trackable (reference an ITrackable component).");
			}			

			hasSwitchTargetAudioClip = switchTargetAudioClip != null;
			hasEnemyDetectedAudioClip = enemyDetectedAudioClip != null;
			hasFriendlyDetectedAudioClip = friendlyDetectedAudioClip != null;

			hasAudioSource = audioSource != null;

		}


        // Called when the gameobject is deactivated
		void OnDisable()
		{
			trackedTargets.Clear();
		}


		/// <summary>
        /// Event called when a target is detected for the first time after no targets have been detected.
        /// </summary>
		void OnTargetDetection()
		{

			bool enemyFound = false;
			bool friendlyFound = false;

			for (int i = 0; i < trackedTargets.Count; ++i)
			{
				if (trackedTargets[i].Team != vehicle.Team)
				{
					enemyFound = true;
				}
				else if (trackedTargets[i].CachedGameObject != vehicle.CachedGameObject)
				{
					friendlyFound = true;
				}
			}
			
			if (enemyFound && !enemiesCurrentlyDetected)
			{
				if (hasAudioSource && hasEnemyDetectedAudioClip)
				{
					audioSource.Stop();
					audioSource.PlayOneShot(enemyDetectedAudioClip);	
				}	
			}

			if (friendlyFound && !friendliesCurrentlyDetected)
			{
				if (hasAudioSource && hasFriendlyDetectedAudioClip)
				{
					audioSource.Stop();
					audioSource.PlayOneShot(friendlyDetectedAudioClip);
				}
			}

			enemiesCurrentlyDetected = enemyFound;
			friendliesCurrentlyDetected = friendlyFound;
		}


		/// <summary>
        /// Event called by missile controller to update the missile threats for the vehicle.
        /// </summary>
        /// <param name="threat">An interface to the new missile threat.</param>
		public void UpdateMissileThreat(IMissileThreat threat)
		{
			
			for (int i = 0; i < missileThreats.Count; ++i)
			{
				if (missileThreats[i].CachedGameObject == threat.CachedGameObject)
				{
					missileThreats.RemoveAt(i);
				}
			}
			
			if (threat.MissileState == MissileState.Locked) missileThreats.Add(threat);
		}


        /// <summary>
        /// Event called when the selected target's Trackable Activation State changes.
        /// </summary>
        /// <param name="newActivationState">The new activation state for the trackable.</param>
        void OnSelectedTargetActivationStateChanged(TrackableActivationState newActivationState)
        {
            switch (newActivationState)
            {
                case TrackableActivationState.InactiveInScene:
                    selectedTarget = null;
                    hasSelectedTarget = false;
                    break;
                case TrackableActivationState.RemovedFromScene:
                    selectedTarget = null;
                    hasSelectedTarget = false;
                    break;
            }
        }


        /// <summary>
        /// Set a new selected target for the radar.
        /// </summary>
        /// <param name="newSelectedTarget">The new selected target.</param>
        /// <param name="playAudio">Whether to play audio for a new target.</param>
        public void SetSelectedTarget(ITrackable newSelectedTarget, bool playAudio = false)
        {

            // Unsubscribe from previous target's events
            if (hasSelectedTarget)
            {
                selectedTarget.OnTrackableActivationStateChangedEventHandler -= OnSelectedTargetActivationStateChanged;
            }

            // Update selected target
            if (newSelectedTarget != null)
            {

                // Play audio
                if (!newSelectedTarget.Equals(selectedTarget))
                {

                    // Reset the lock
                    if (hasAudioSource) audioSource.Stop();

                    // Play audio
                    if (playAudio && hasAudioSource && hasSwitchTargetAudioClip) audioSource.PlayOneShot(switchTargetAudioClip);

                }

                selectedTarget = newSelectedTarget;
                hasSelectedTarget = true;
                
                // Subscribe to new target's events
                selectedTarget.OnTrackableActivationStateChangedEventHandler += OnSelectedTargetActivationStateChanged;
            }
            else
            {
                selectedTarget = null;
                hasSelectedTarget = false;
            }
        }


        /// <summary>
        /// Check if the selected target should still be tracked.
        /// </summary>
        void CheckSelectedTargetStatus()
        {
            if (!hasSelectedTarget || trackedTargets.IndexOf(selectedTarget) == -1)
            {
                GetNewTarget(RadarSelectionPriority.Hostile, defaultRadarSelectionMode);
            }
        }


        /// <summary>
        /// Get a new target for the radar.
        /// </summary>
        /// <param name="selectionPriority">The selection priority for the new target.</param>
        /// <param name="mode">The selection mode for the new target.</param>
        /// <param name="playSwitchTargetAudio">Whether to play the audio for switching targets.</param>
        public void GetNewTarget(RadarSelectionPriority selectionPriority, RadarSelectionMode mode, bool playSwitchTargetAudio = false)
		{

			if (vehicle == null)
				return;
            
            ITrackable previousTarget = selectedTarget;

			// Find the team that the new target will be on
			List<Team> targetTeams = new List<Team>();
			switch (selectionPriority)
			{
				case RadarSelectionPriority.Hostile:
					if (vehicle.Team == Team.Friendly)
					{
                        targetTeams.Add(Team.Enemy);
					}
					else
					{
                        targetTeams.Add(Team.Friendly);
                    }
					break;

                case RadarSelectionPriority.NonHostile:
                    if (vehicle.Team == Team.Friendly)
                    {
                        targetTeams.Add(Team.Friendly);
                    }
                    else
                    {
                        targetTeams.Add(Team.Enemy);
                    }
                    targetTeams.Add(Team.Neutral);
                    break;

                default:
                    targetTeams = new List<Team>((Team[])Enum.GetValues(typeof(Team)));
					break;
			}
			

			switch (mode)
			{
	
				case RadarSelectionMode.Next:
					
					// If the ship's had no previous target, just use the first active hostile target
					if (!vehicle.Radar.HasSelectedTarget)
					{ 
						for (int i = 0; i < trackedTargets.Count; ++i)
						{
							
							// Check attributes
							if (trackedTargets[i].CachedGameObject == vehicle.CachedGameObject) continue;	
							if (!vehicle.Radar.SelectableTypes.Contains(trackedTargets[i].TrackableType)) continue;
                            if (!targetTeams.Contains(trackedTargets[i].Team)) continue;
							if (trackedTargets[i] == vehicle.Radar.SelectedTarget) continue;
							if (trackedTargets[i].Equals(null)) continue;
							SetSelectedTarget(trackedTargets[i], true);
							break;
						}
					}
					else
					{
						int currentTargetIndex = trackedTargets.IndexOf(vehicle.Radar.SelectedTarget);
						bool found = false;

						// Check for an active hostile from the current index to the end
						for (int i = currentTargetIndex + 1; i < trackedTargets.Count; ++i)
						{
							// Check attributes
							if (trackedTargets[i].CachedGameObject == vehicle.CachedGameObject) continue;	
							if (!vehicle.Radar.SelectableTypes.Contains(trackedTargets[i].TrackableType)) continue;
                            if (!targetTeams.Contains(trackedTargets[i].Team)) continue;
                            if (trackedTargets[i] == vehicle.Radar.SelectedTarget) continue;
							if (trackedTargets[i].Equals(null)) continue;
                            SetSelectedTarget(trackedTargets[i], true);
                            found = true;
							break;
						}
                        
                        // Check for an active hostile from the start to the current index (inclusive)
                        if (!found)
						{
							for (int i = 0; i <= currentTargetIndex; ++i)
							{
                                // Check attributes
								if (trackedTargets[i].CachedGameObject == vehicle.CachedGameObject) continue;	
								if (!vehicle.Radar.SelectableTypes.Contains(trackedTargets[i].TrackableType)) continue;
                                if (!targetTeams.Contains(trackedTargets[i].Team)) continue;
                                if (trackedTargets[i].Equals(null)) continue;

                                SetSelectedTarget(trackedTargets[i], true);
                                found = true;
                                break;

							}	
						}
                        if (!found) SetSelectedTarget(null);
					}
					break;

                case RadarSelectionMode.Previous:

                    // If the ship's had no previous target, just use the first active hostile target
                    if (!vehicle.Radar.HasSelectedTarget)
                    {
                        for (int i = 0; i < trackedTargets.Count; ++i)
                        {

                            // Check attributes
                            if (trackedTargets[i].CachedGameObject == vehicle.CachedGameObject) continue;
                            if (!vehicle.Radar.SelectableTypes.Contains(trackedTargets[i].TrackableType)) continue;
                            if (!targetTeams.Contains(trackedTargets[i].Team)) continue;
                            if (trackedTargets[i] == vehicle.Radar.SelectedTarget) continue;
                            if (trackedTargets[i].Equals(null)) continue;
                            SetSelectedTarget(trackedTargets[i], true);
                            break;
                        }
                    }
                    else
                    {
                        int currentTargetIndex = trackedTargets.IndexOf(vehicle.Radar.SelectedTarget);
                        bool found = false;

                        // Check for an active hostile from the current index back to the beginning of the list
                        for (int i = currentTargetIndex - 1; i >= 0; ++i)
                        {
                            // Check attributes
                            if (trackedTargets[i].CachedGameObject == vehicle.CachedGameObject) continue;
                            if (!vehicle.Radar.SelectableTypes.Contains(trackedTargets[i].TrackableType)) continue;
                            if (!targetTeams.Contains(trackedTargets[i].Team)) continue;
                            if (trackedTargets[i] == vehicle.Radar.SelectedTarget) continue;
                            if (trackedTargets[i].Equals(null)) continue;
                            SetSelectedTarget(trackedTargets[i], true);
                            found = true;
                            break;
                        }

                        // Check for an active hostile from the end back to the current index (inclusive)
                        if (!found)
                        {
                            for (int i = trackedTargets.Count - 1; i >= currentTargetIndex; ++i)
                            {
                                // Check attributes
                                if (trackedTargets[i].CachedGameObject == vehicle.CachedGameObject) continue;
                                if (!vehicle.Radar.SelectableTypes.Contains(trackedTargets[i].TrackableType)) continue;
                                if (!targetTeams.Contains(trackedTargets[i].Team)) continue;
                                if (trackedTargets[i].Equals(null)) continue;

                                SetSelectedTarget(trackedTargets[i], true);
                                found = true;
                                break;

                            }
                        }
                        if (!found) SetSelectedTarget(null);
                    }
                    break;

                case RadarSelectionMode.Nearest:
	
					int closestTargetIndex = -1;
					float minDistance = vehicle.Radar.Range + 1; // More than possible

					for (int i = 0; i < trackedTargets.Count; ++i)
					{

						// Check attributes
						if (trackedTargets[i].CachedGameObject == vehicle.CachedGameObject) continue;	
						if (!vehicle.Radar.SelectableTypes.Contains(trackedTargets[i].TrackableType)) continue;
                        if (!targetTeams.Contains(trackedTargets[i].Team)) continue;
                        if (trackedTargets[i].Equals(null)) continue;

						// Check if it is the nearest
						float distance = Vector3.Distance(vehicle.CachedTransform.position, trackedTargets[i].CachedTransform.position);
						if (distance < minDistance)
						{
							minDistance = distance;
							closestTargetIndex = i;
						}
					}

					// update the result
					if (closestTargetIndex == -1)
					{
                        SetSelectedTarget(null);
                    }
					else
					{
                        SetSelectedTarget(trackedTargets[closestTargetIndex], true);
					}
					break;
	
				case RadarSelectionMode.Front:
					
					ITrackable targetUnderNose = vehicle.Radar.SelectedTarget;
			
					float minAngle = 181f; // More than maximum possible

					for (int i = 0; i < trackedTargets.Count; ++i){

						// Check attributes
						if (trackedTargets[i].CachedGameObject == vehicle.CachedGameObject) continue;	
						if (!vehicle.Radar.SelectableTypes.Contains(trackedTargets[i].TrackableType)) continue;
                        if (!targetTeams.Contains(trackedTargets[i].Team)) continue;
                        if (trackedTargets[i].Equals(null)) continue;
						
						// Check if the angle to the target is smaller
						float angle = Vector3.Angle (vehicle.CachedTransform.forward, trackedTargets[i].CachedTransform.position - vehicle.CachedTransform.position);
						if (angle <= vehicle.Radar.FrontTargetAngle && angle < minAngle){
							minAngle = angle;
							targetUnderNose = trackedTargets[i];
						}

					}
                    SetSelectedTarget(targetUnderNose, true);
                    break;
	
			}
            
			if (previousTarget != null && hasSelectedTarget && !selectedTarget.Equals(previousTarget))
			{
				if (hasAudioSource)
				{ 
					audioSource.Stop();

					// Play audio
					if (playSwitchTargetAudio && hasAudioSource && hasSwitchTargetAudioClip)
					{
						audioSource.PlayOneShot(switchTargetAudioClip);
					}
				}
			}
		}
		

		// Called every frame
		void Update()
		{
			if (RadarSceneManager.Instance == null)
			{		
				trackedTargets.Clear();
				return;
			}	
			
			// Update targets
			RadarSceneManager.Instance.GetTrackables(trackedTargets, RadarSelectionPriority.Any, vehicle.Team, vehicle.CachedTransform.position, Range);

            CheckSelectedTargetStatus();

			OnTargetDetection();
          
		}
	}
}
