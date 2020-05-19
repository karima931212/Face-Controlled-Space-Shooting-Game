using UnityEngine;
using System.Collections;

namespace VSX.UniversalVehicleCombat
{

    /// <summary>
    /// This class represents a blackboard of data that is shared among AI behaviours so that behaviors can be
    /// blended.
    /// </summary>
    public class BehaviourBlackboard 
	{
	
        private GroupMember groupMember;
        /// <summary>
        /// The group membership of the game agent.
        /// </summary>
        public GroupMember GroupMember
        {
            get { return groupMember; }
            set
            {
                groupMember = value;
                isGroupMember = groupMember != null;
            }
        }

        private bool isGroupMember;
        public bool IsGroupMember
        {
            get { return isGroupMember; }
        }

		private Vehicle vehicle;
		public Vehicle Vehicle { get { return vehicle; } }
		
		public bool hasVehicle;
		public bool HasVehicle { get { return hasVehicle; } }

		private Vector3 steeringPIDCoeffs;
        /// <summary>
        /// The PID controller coefficients for the steering.
        /// </summary>
		public Vector3 SteeringPIDCoeffs { get { return steeringPIDCoeffs; } }

		private Vector3 throttlePIDCoeffs;
        /// <summary>
        /// The PID controller coefficients for the throttle (mainly used for during formation maneuvers).
        /// </summary>
		public Vector3 ThrottlePIDCoeffs { get { return throttlePIDCoeffs; } }

        /// <summary>
        /// The integral value of the steering PID controller.
        /// </summary>
        public Vector3 integralSteeringVals;

        /// <summary>
        /// The integral value of the throttle PID controller.
        /// </summary>
        public Vector3 integralThrottleVals;

        private Vector3 maxRotationAngles;
        /// <summary>
        /// How far the vehicle is allowed to rotate (negative or positive) about each spatial axis.
        /// </summary>
		public Vector3 MaxRotationAngles { get { return maxRotationAngles; } }
			
		public Vector3 steeringValues;
		
		public Vector3 throttleValues;
		
		public Vector3 obstacleAvoidanceDirection;
		
        /// <summary>
        /// A 0-1 value for the obstacle avoidance strength
        /// </summary>
		public float obstacleAvoidanceStrength;

        /// <summary>
        /// How fast the primary obstacle detected is moving away from the vehicle (may be negative if approaching).
        /// </summary>
		public float obstacleMovingAwaySpeed;	
		
		public float obstacleAvoidanceMargin;

		public bool canFirePrimaryWeapon;

		public bool canFireSecondaryWeapon;
		public bool secondaryWeaponFired;


        /// <summary>
        /// Initialize the blackboard.
        /// </summary>
        /// <param name="steeringPIDCoeffs">The PID controller coefficients for the steering.</param>
        /// <param name="throttlePIDCoeffs">The PID controller coefficients for the throttle.</param>
        /// <param name="maxRotationAngles">The maximum rotation angles for the vehicle.</param>
		public void Initialize(Vector3 steeringPIDCoeffs, Vector3 throttlePIDCoeffs, Vector3 maxRotationAngles){

			this.steeringPIDCoeffs = steeringPIDCoeffs;

			this.throttlePIDCoeffs = throttlePIDCoeffs;
	
			this.maxRotationAngles = maxRotationAngles;
		}

        /// <summary>
        /// Set the vehicle that this blackboard refers to.
        /// </summary>
        /// <param name="newVehicle">The vehicle that this blackboard refers to.</param>
		public void SetVehicle(Vehicle vehicle)
        {
			this.vehicle = vehicle;
			this.hasVehicle = vehicle != null;
		}
		
	}
}