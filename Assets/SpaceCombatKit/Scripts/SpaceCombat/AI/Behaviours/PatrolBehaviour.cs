using UnityEngine;
using System.Collections;

namespace VSX.UniversalVehicleCombat
{

    /// <summary>
    /// This class implements AI patrol behaviour for a space fighter
    /// </summary>
    public class PatrolBehaviour : MonoBehaviour, IVehicleControlBehaviour 
	{

        [SerializeField]
		private float patrolSpeedFactor = 0.5f;

		[SerializeField]
		private float patrolSteeringFactor = 0.33f;

        private BehaviourBlackboard blackboard;


        /// <summary>
        /// Initialize this vehicle control behaviour.
        /// </summary>
        /// <param name="blackboard">The AI's blackboard of data.</param>
        /// <param name="groupMember">The group membership the GameAgent of this vehicle holds.</param>
        public void Initialize(BehaviourBlackboard blackboard)
		{
			this.blackboard = blackboard;

		}


		/// <summary>
        /// Called every frame that this behavior is running
        /// </summary>
		public void Tick()
		{

			Vector3 steeringTarget = Vector3.zero;
			Vector3 dirToTarget = Vector3.zero;
            
            // If this vehicle is not following another vehicle, look for a patrol route to follow
            if (!blackboard.IsGroupMember || blackboard.GroupMember.FormationLeader == null || blackboard.GroupMember.FormationLeader == blackboard.Vehicle)
			{
                if (blackboard.Vehicle.Agent.name == "Friendly2")
                {
                    Debug.Log(blackboard.IsGroupMember);
                }

                //Look for patrol route
                if (blackboard.IsGroupMember && blackboard.GroupMember.PatrolRoute != null && blackboard.GroupMember.PatrolRoute.PatrolTargets.Count > 0)
				{

					if (Vector3.Distance(blackboard.Vehicle.CachedTransform.position, blackboard.GroupMember.PatrolRoute.PatrolTargets[blackboard.GroupMember.CurrentPatrolTargetIndex].position) < 50)
					{
                        blackboard.GroupMember.IncrementPatrolTargetIndex();
					}
                    
					steeringTarget = blackboard.GroupMember.PatrolRoute.PatrolTargets[blackboard.GroupMember.CurrentPatrolTargetIndex].position;
					blackboard.throttleValues = new Vector3(0f, 0f, patrolSpeedFactor);
					Vector3 controlPIDCoeffs = new Vector3(blackboard.SteeringPIDCoeffs.x, patrolSteeringFactor * blackboard.SteeringPIDCoeffs.y, blackboard.SteeringPIDCoeffs.z);
					dirToTarget = (steeringTarget - blackboard.Vehicle.CachedTransform.position).normalized;
			
					steeringTarget = blackboard.obstacleAvoidanceStrength * (blackboard.Vehicle.CachedTransform.position + blackboard.obstacleAvoidanceDirection * 30) +
					(1 - blackboard.obstacleAvoidanceStrength) * (blackboard.Vehicle.CachedTransform.position + dirToTarget * 30);
					
					Maneuvring.TurnToward(blackboard.Vehicle.CachedTransform, steeringTarget, controlPIDCoeffs, 
						blackboard.MaxRotationAngles, out blackboard.steeringValues, ref blackboard.integralSteeringVals);
                    
                    blackboard.steeringValues *= patrolSteeringFactor;	
					
					blackboard.throttleValues = (1f - blackboard.obstacleAvoidanceStrength * 0.5f) * blackboard.throttleValues;

                }
			}
			else
			{
               
                Vector3 controlPIDCoeffs = new Vector3(blackboard.SteeringPIDCoeffs.x, patrolSteeringFactor * blackboard.SteeringPIDCoeffs.y, blackboard.SteeringPIDCoeffs.z);
				
				steeringTarget = Maneuvring.Formation(blackboard.Vehicle, blackboard.GroupMember.FormationLeader, blackboard.GroupMember.FormationOffset, 
                                                        blackboard.ThrottlePIDCoeffs, ref blackboard.integralThrottleVals, out blackboard.throttleValues);

				dirToTarget = (steeringTarget - blackboard.Vehicle.CachedTransform.position).normalized;
			
				float steeringFactor = Mathf.Clamp(1 - (blackboard.obstacleMovingAwaySpeed / 10), 0f, 1f) * blackboard.obstacleAvoidanceStrength;
				steeringTarget = steeringFactor * (blackboard.Vehicle.CachedTransform.position + blackboard.obstacleAvoidanceDirection) +
				(1 - steeringFactor) * (blackboard.Vehicle.CachedTransform.position + dirToTarget);

				Maneuvring.TurnToward(blackboard.Vehicle.CachedTransform, steeringTarget, controlPIDCoeffs, 
						blackboard.MaxRotationAngles, out blackboard.steeringValues, ref blackboard.integralSteeringVals);

				
				blackboard.steeringValues *= patrolSteeringFactor;	
				
				blackboard.throttleValues = (1f - blackboard.obstacleAvoidanceStrength * 0.5f) * blackboard.throttleValues;         
                

            }
		}		
	}
}