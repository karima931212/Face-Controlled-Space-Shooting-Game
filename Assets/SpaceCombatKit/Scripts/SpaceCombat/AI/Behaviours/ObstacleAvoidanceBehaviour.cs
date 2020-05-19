using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace VSX.UniversalVehicleCombat
{

    /// <summary>
    /// This class represents an obstacle that is perceived by the AI
    /// </summary>
    public class ObstacleData
	{
	
		public bool hasRigidbody;
		
		public Rigidbody rigidBody;
	
		public RaycastHit raycastHit;
	
		public float raycastHitTime;
	
		public Vector3 obstacleVelocity;

		public Vector3 currentPos;
		
		public Vector3 currentAvoidanceDirection;
			
		public float currentRiskFactor;
	
		public float movingAwaySpeed;

		public ObstacleData(RaycastHit hit, float detectionTime, Rigidbody rigidBody)
		{
	
			this.raycastHit = hit;

			this.currentPos = hit.point;
	
			this.raycastHitTime = detectionTime;
	
			if (rigidBody != null)
			{
				this.rigidBody = rigidBody;
				this.hasRigidbody = true;
				this.obstacleVelocity = rigidBody.velocity;
			} 
			else 
			{
				this.rigidBody = null;
				this.hasRigidbody = false;
				this.obstacleVelocity = Vector3.zero;
			}
		}
	}


	/// <summary>
    /// This class calculates risk factors for all the obstacles perceived by the AI, and calculates a point in space
    /// each frame that enables the AI to avoid the obstacles.
    /// </summary>
	public class ObstacleAvoidanceBehaviour : MonoBehaviour, IVehicleControlBehaviour
	{
	
		[Header("Obstacles")]
	
		[SerializeField]
		private float sphereCastRadius = 20f;

		public Vector2 minMaxCollisionReactionTime = new Vector3(1, 3);
		public Vector2 minMaxObstaclePassDistance = new Vector3(30, 100);

		public float obstacleAvoidanceMargin = 50;
	
		[SerializeField]
		private bool useMemory = true;
		
		[SerializeField]
		private int maxObstacleDataInstances = 3;

		[SerializeField]
		private float obstacleImportanceFadeRate = 0.3f;

		[SerializeField]
		private float obstacleMergeDistance = 30f;
		
		private List<ObstacleData> obstacleDataList = new List<ObstacleData>();
		public List<ObstacleData> ObstacleDataList { get { return obstacleDataList; } }

		private BehaviourBlackboard blackboard;
		

		/// <summary>
        /// Initialize the behaviour with the blackboard it will use.
        /// </summary>
        /// <param name="blackboard"></param>
		public void Initialize(BehaviourBlackboard blackboard)
		{
			this.blackboard = blackboard;
		}


		// Create obstacle data from raycast hits
		void UpdateObstacleData(RaycastHit[] hits)
		{
	
			// If no obstacles should be 'remembered' clear the obstacle data list
			if (!useMemory)
			{ 
				obstacleDataList.Clear();
			}

			// Update the risk factors for all the obstacle data instances
			for (int i = 0; i < obstacleDataList.Count; ++i)
			{
				UpdateRiskFactor(obstacleDataList[i], true);
			}

			// Add the new data to the list
			for (int i = 0; i < hits.Length; ++i)
			{
				ObstacleData newData = new ObstacleData(hits[i], Time.time, hits[i].collider.attachedRigidbody);
				UpdateRiskFactor(newData, false);
				AddObstacleData(newData);
			}
			
			// Initialize the blackboard values
			blackboard.obstacleAvoidanceStrength = 0;
			blackboard.obstacleAvoidanceDirection = Vector3.forward;
			blackboard.obstacleMovingAwaySpeed = 0;
			float totalRiskFactor = 0;			
			
			// Get the total risk factor for calculating the influence of this instance on the final calculated direction
			// Also update the blackboard with the maximum collision risk value
			for (int i = 0; i < obstacleDataList.Count; ++i)
			{

				totalRiskFactor += obstacleDataList[i].currentRiskFactor;

				if (obstacleDataList[i].currentRiskFactor > blackboard.obstacleAvoidanceStrength)
					blackboard.obstacleAvoidanceStrength = obstacleDataList[i].currentRiskFactor;

			}
			
			// Update blackboard data
			if (totalRiskFactor > 0.0001f)
			{
				// Update the obstacle avoidance direction
				for (int i = 0; i < obstacleDataList.Count; ++i)
				{
					blackboard.obstacleAvoidanceDirection += (obstacleDataList[i].currentRiskFactor / totalRiskFactor) * obstacleDataList[i].currentAvoidanceDirection;
					
					blackboard.obstacleMovingAwaySpeed += (obstacleDataList[i].currentRiskFactor / totalRiskFactor) * obstacleDataList[i].movingAwaySpeed;
				}
			}
		}


		/// <summary>
        /// Add another piece of data for a new obstacle.
        /// </summary>
        /// <param name="newData">The new obstacle data.</param>
		public void AddObstacleData(ObstacleData newData)
		{
	
			Rigidbody rBody = newData.raycastHit.collider.attachedRigidbody;
			bool hasRigidbody = rBody != null;
			
			// Prevent obstacle avoidance of self
			if (hasRigidbody && (rBody == blackboard.Vehicle.CachedRigidbody))
				return;
			
			// Merge obstacle data that are in close proximity
			bool merged = false;
			for (int i = 0; i < obstacleDataList.Count; ++i)
			{
				
				if (Vector3.Distance(newData.currentPos, obstacleDataList[i].currentPos) < obstacleMergeDistance)
				{
					obstacleDataList[i] = newData;
					merged = true;
					break;
				}
			}	
			
			// If obstacle has not been merged, replace any that have a lower risk
			if (!merged)
			{

				if (obstacleDataList.Count < maxObstacleDataInstances)
				{
					obstacleDataList.Add(newData);
				}
				else
				{

					for (int i = 0; i < obstacleDataList.Count; ++i)
					{
						if (newData.currentRiskFactor > obstacleDataList[i].currentRiskFactor)
						{
							obstacleDataList[i] = newData;
							break;
						}
					}

				}
			}
		}


		// Update the risk factors for the obstacles
		void UpdateRiskFactor(ObstacleData obstacleData, bool show)
		{

			// Update the position
			obstacleData.currentPos = obstacleData.raycastHit.point + obstacleData.obstacleVelocity *
			(Time.time - obstacleData.raycastHitTime);

			// Update the avoid target direction
			Vector3 toObstacleVector = obstacleData.currentPos - blackboard.Vehicle.CachedTransform.position;
	
			// Get the velocity of the collision point relative to this ship
			Vector3 collisionRelVelocity = obstacleData.obstacleVelocity - blackboard.Vehicle.CachedRigidbody.velocity;
			float closingVelocityAmount = Vector3.Dot(collisionRelVelocity.normalized, -toObstacleVector.normalized);
			
			// Get the closest distance that the point obstacle will get to this ship
			float tmp = Vector3.Dot(-toObstacleVector.normalized, collisionRelVelocity.normalized);
			Vector3 nearestPointOnLine = obstacleData.currentPos + (tmp * Vector3.Magnitude(toObstacleVector) * collisionRelVelocity.normalized);
			
			float timeToImpact = Vector3.Distance(obstacleData.currentPos, blackboard.Vehicle.CachedTransform.position) / Mathf.Max(closingVelocityAmount * collisionRelVelocity.magnitude, 0.0001f);

			obstacleData.movingAwaySpeed = Vector3.Dot(obstacleData.obstacleVelocity.normalized, toObstacleVector.normalized) * obstacleData.obstacleVelocity.magnitude;

			// Calculate the avoidance target position and the direction to it
			Vector3 perpendicularAvoidDirection = (blackboard.Vehicle.CachedTransform.position - nearestPointOnLine).normalized;
			Vector3 avoidTarget = obstacleData.currentPos + perpendicularAvoidDirection * obstacleAvoidanceMargin;
			
			obstacleData.currentAvoidanceDirection = (avoidTarget - blackboard.Vehicle.CachedTransform.position).normalized;

			float directionalityFactor = Mathf.Clamp(Vector3.Dot(blackboard.Vehicle.CachedTransform.forward, toObstacleVector.normalized), 0f, 1f);			
	
			float proximityFactor = timeToImpact < minMaxCollisionReactionTime.x ? 1 : 1 - Mathf.Clamp((timeToImpact - minMaxCollisionReactionTime.x) /
				                        (minMaxCollisionReactionTime.y - minMaxCollisionReactionTime.x), 0f, 1f);
	
			float nearestDist = Vector3.Distance(blackboard.Vehicle.CachedTransform.position, nearestPointOnLine);
			proximityFactor *= nearestDist < minMaxObstaclePassDistance.x ? 1 : 1 - Mathf.Clamp((nearestDist - minMaxObstaclePassDistance.x) /
				(minMaxObstaclePassDistance.y - minMaxObstaclePassDistance.x), 0f, 1f);
			
			float timeFactor = Mathf.Clamp(1 - (Time.time - obstacleData.raycastHitTime) / (1 / obstacleImportanceFadeRate), 0f, 1f);
			
			obstacleData.currentRiskFactor = directionalityFactor * timeFactor * proximityFactor;
			
		}


		/// <summary>
        /// Called by the control script every frame when this behaviour is running.
        /// </summary>
		public void Tick()
		{
			// Do collision avoidance 
			float obstacleScanDistance = blackboard.Vehicle.CachedRigidbody.velocity.magnitude * minMaxCollisionReactionTime.y;

			RaycastHit[] hits = Physics.SphereCastAll(blackboard.Vehicle.CachedTransform.position, sphereCastRadius, 
														blackboard.Vehicle.CachedRigidbody.velocity.normalized, obstacleScanDistance);

			UpdateObstacleData(hits);
		}
	}
}
