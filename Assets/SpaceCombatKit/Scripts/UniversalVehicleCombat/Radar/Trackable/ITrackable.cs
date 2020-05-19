using UnityEngine;
using System.Collections;

namespace VSX.UniversalVehicleCombat
{

	/// <summary>
    /// The different trackable types in the game
    /// </summary>
	public enum TrackableType
	{
		Ship,
		Waypoint
	}

    /// <summary>
    /// The different activation states the trackable can be in
    /// </summary>
    public enum TrackableActivationState
    {
        ActiveInScene,
        InactiveInScene,
        RemovedFromScene
    }

    /// <summary>
    /// Delegate for attaching event functions to run when the trackable's activation state changes
    /// </summary>
    /// <param name="newState">The new activation state of the trackable.</param>
    public delegate void OnTrackableActivationStateChangedEventHandler(TrackableActivationState newState);

    
    /// <summary>
    /// This interface provides a way to easily track different objects in the game.
    /// </summary>
    public interface ITrackable
    {
	
		string Label { get; }
		
		TrackableType TrackableType { get; }	

		bool TrackableEnabled { get; set; }

		bool IgnoreTrackingDistance { get; }
		
		bool HasHealthInfo { get; }
		IHealthInfo HealthInfo { get; }

		Transform CachedTransform { get; }
		GameObject CachedGameObject { get; }

		bool HasRigidbody { get; }
		Rigidbody CachedRigidbody { get; }

        PhysicsInfo PhysicsInfo { get; }

		// Used for calculating target box dimensions
		bool HasBodyMesh { get;}
		Mesh BodyMesh { get; }
	
		Team Team { get; }

		bool HasHologramMesh { get; }
		Mesh HologramMesh { get; }

		// A normal map for the model displayed on the dash hologram for this trackable
		Texture2D HologramNormal { get; }

        /// <summary>
        /// Interface event for the OnTrackableActivationStateChangedEventHandler delegate
        /// </summary>
        event OnTrackableActivationStateChangedEventHandler OnTrackableActivationStateChangedEventHandler;

    }
}
