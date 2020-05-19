using UnityEngine;
using System.Collections;


namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// This class contains physics information for rigidbodies, kinematic rigidbodies or entities that move around without rigidbodies,
    /// so that they can all be treated as physical entities e.g. to calculate lead target position
    /// </summary>
    public class PhysicsInfo
    {

        // Rigidbody position
        public Vector3 position = Vector3.zero;

        // Rigidbody rotation
        public Quaternion rotation = Quaternion.identity;

        // Rigidbody linear velocity
        public Vector3 velocity = Vector3.zero;

        public Vector3 lastVelocity = Vector3.zero;

        // Rigidbody angular velocity
        public Vector3 angularVelocity = Vector3.zero;

        // Rigidbody local angular velocity
        public Vector3 localAngularVelocity = Vector3.zero;

        // Rigidbody linear acceleration
        public Vector3 acceleration = Vector3.zero;

        public Vector3 lastAcceleration = Vector3.zero;

        // Jerk is the derivative of acceleration (how fast the acceleration is changing)
        public Vector3 jerk = Vector3.zero;

    }
}
