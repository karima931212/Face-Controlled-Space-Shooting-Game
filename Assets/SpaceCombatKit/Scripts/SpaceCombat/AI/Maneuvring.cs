using UnityEngine;
using System.Collections;
using VSX.UniversalVehicleCombat;

namespace VSX.UniversalVehicleCombat
{

	// This class contains several static methods for PID controlled vehicle movement
    /// <summary>
    /// This class contains static methods for calculating PID-controller-based maneuvers for AI and autopilots.
    /// </summary>
	public class Maneuvring : MonoBehaviour 
	{
	
		/// <summary>
        /// Turn toward a world position in space.
        /// </summary>
        /// <param name="vehicleTransform">The transform of the vehicle.</param>
        /// <param name="targetPosition">The position the vehicle must turn toward.</param>
        /// <param name="PIDCoeffs">The PID coefficients for this vehicle.</param>
        /// <param name="maxRotationAngles">The maximum rotation angles about each axis that the vehicle can reach during maneuvers.</param>
        /// <param name="controlValues">Get the control values calculated for the maneuver.</param>
        /// <param name="integralVals">Get the updated integral values for the PID controller.</param>
		public static void TurnToward (Transform vehicleTransform, Vector3 targetPosition, Vector3 PIDCoeffs, Vector3 maxRotationAngles, out Vector3 controlValues, ref Vector3 integralVals)
		{
			
			Vector3 tmpRelPos = GetRollAndPitchUnaffectedRelPos(vehicleTransform, targetPosition);
			float zxAngleGlobal = Vector3.Angle(Vector3.forward, new Vector3(tmpRelPos.x, 0f, tmpRelPos.z));
			if (tmpRelPos.y > 0)
			{
				zxAngleGlobal *= -1;
			}
	
			// Get the roll angle
			float xyAngleGlobal = Vector3.Angle(Vector3.up, new Vector3(tmpRelPos.x, tmpRelPos.y, 0));
			if (tmpRelPos.x > 0)
			{
				xyAngleGlobal *= -1;
			}
	
			// Get the ship's current roll
			float currentRollAngle = GetRollAngleFromHorizon(vehicleTransform);
			
			float desiredRollAngle;
			if (Mathf.Abs(xyAngleGlobal) > maxRotationAngles.z)
			{
				desiredRollAngle = maxRotationAngles.z * Mathf.Sign(xyAngleGlobal);
			}
			else
			{
				desiredRollAngle = xyAngleGlobal;
			}
	
			desiredRollAngle *= Mathf.Clamp((Mathf.Abs(zxAngleGlobal) / 45f) - 0.05f, 0, 1);
			float relativeRollAngle = desiredRollAngle - currentRollAngle;
	
			Vector3 toTargetDir = (targetPosition - vehicleTransform.position).normalized;
	
			Vector3 tmp = GetRollAndPitchUnaffectedRelPos(vehicleTransform, targetPosition);
	
			// This is the pitch from the totarget vector to the vertically flattened totarget vector
			float pitchAngleToTarget_Global = Vector3.Angle(toTargetDir, new Vector3(toTargetDir.x, 0, toTargetDir.z));
			pitchAngleToTarget_Global = tmp.y > 0 ? pitchAngleToTarget_Global * -1 : pitchAngleToTarget_Global;
			
			// this is the pitch from the ship's forward vector to the vertically flattened ships forward vector
			float currentPitchAngle_Global = Vector3.Angle(vehicleTransform.forward, new Vector3(vehicleTransform.forward.x, 0f, vehicleTransform.forward.z));
			currentPitchAngle_Global = vehicleTransform.forward.y > 0 ? currentPitchAngle_Global * -1 : currentPitchAngle_Global;
			float clampedTargetPitch = Mathf.Clamp(pitchAngleToTarget_Global, -maxRotationAngles.x, maxRotationAngles.x);
			
			float relativePitchAngle = clampedTargetPitch - currentPitchAngle_Global;
	
			// THis is is yaw angle from the vertically flattened ship forward, to the vertically flattened totarget
			float yawAngleToTarget_Global = Vector3.Angle(Vector3.forward, new Vector3(tmp.x, 0f, tmp.z));
			yawAngleToTarget_Global = tmp.x < 0 ? yawAngleToTarget_Global * -1 : yawAngleToTarget_Global;
	
			Vector3 pitchAndYaw = new Vector3(yawAngleToTarget_Global, relativePitchAngle, 0f);
			pitchAndYaw = Quaternion.Euler(0f, 0f, currentRollAngle) * pitchAndYaw;
			
			Vector3 proportionalVals;
	
			proportionalVals.x = pitchAndYaw.y;
			proportionalVals.y = pitchAndYaw.x;
			proportionalVals.z = relativeRollAngle;
			proportionalVals *= PIDCoeffs.x;
			
			//  ****** PID coeffs ******
	
			// Derivative (braking) torque
	
			Vector3 derivativeVals = -1f * PIDCoeffs.z * proportionalVals;
			
			// Integral
			float angleToTarget = Vector3.Angle(vehicleTransform.forward, targetPosition - vehicleTransform.position);
			if (angleToTarget > 25)
			{
				integralVals = Vector3.zero;
			}
			else
			{
				integralVals += PIDCoeffs.y * proportionalVals;
			}
			
			integralVals.x = Mathf.Clamp(integralVals.x, -1f, 1f);
			integralVals.z = Mathf.Clamp(integralVals.z, -1f, 1f);
			integralVals.y = Mathf.Clamp(integralVals.y, -1f, 1f);
			
			// Calculate control values
			controlValues = proportionalVals + derivativeVals + integralVals;
			controlValues.z = proportionalVals.z;

			controlValues.x = Mathf.Clamp(controlValues.x, -1f, 1f);
			controlValues.z = Mathf.Clamp(controlValues.z, -1f, 1f);
			controlValues.y = Mathf.Clamp(controlValues.y, -1f, 1f);
	
			
		}


        /// <summary>
        /// Do formation behaviour (using throttle to control forward/backward, rather than always turning toward target)
        /// </summary>
        /// <param name="vehicle">The vehicle that is maneuvring.</param>
        /// <param name="formationLeader">The formation leader of the vehicle.</param>
        /// <param name="offset">The offset from the formation leader that this vehicle must maintain.</param>
        /// <param name="throttlePIDCoeffs">The PID controller coefficients for the throttle.</param>
        /// <param name="integralThrottleVals">The integral valued for the throttle PID controller.</param>
        /// <param name="throttleValues">Get the calculated throttle values.</param>
        /// <returns>The world position steering target for this maneuver.</returns>
        public static Vector3 Formation(Vehicle vehicle, Vehicle formationLeader, Vector3 offset, Vector3 throttlePIDCoeffs, 
                                        ref Vector3 integralThrottleVals, out Vector3 throttleValues)
		{
		
			// Get the relative target vector
			Vector3 targetPos = formationLeader.CachedTransform.position + formationLeader.CachedTransform.TransformDirection(offset);
			Vector3 targetRelPos = vehicle.CachedTransform.InverseTransformPoint(targetPos);
	
			float forwardProjection = Mathf.Clamp(1 - Vector3.Distance(vehicle.CachedTransform.position, targetPos)/200, 0f, 1f) * 100;
			
			Vector3 steeringTarget = formationLeader.CachedTransform.position + formationLeader.CachedTransform.TransformDirection(offset) + formationLeader.CachedTransform.forward * forwardProjection;
			
			Vector3 closingVelocity = vehicle.CachedRigidbody.velocity - formationLeader.CachedRigidbody.velocity;
			float closingDirectionAmount = Vector3.Dot(closingVelocity.normalized, (steeringTarget - vehicle.CachedTransform.position).normalized);

			// calculate the throttle
			float proportionalThrottle = targetRelPos.z * throttlePIDCoeffs.x;
										
			float derivativeThrottle = -1 * Mathf.Clamp(closingDirectionAmount, 0, 1) * closingVelocity.magnitude * throttlePIDCoeffs.z;
	
			float integralThrottle = throttlePIDCoeffs.y * proportionalThrottle;
			integralThrottleVals.z += integralThrottle;
			integralThrottleVals.z = Mathf.Clamp(integralThrottleVals.z, 0, 1);
			
			throttleValues = new Vector3(0f, 0f, Mathf.Clamp(proportionalThrottle + derivativeThrottle + integralThrottleVals.z, 0f, 1f));
			
			return steeringTarget;
	
		}

	
		// Get the relative position of a point from a transform, regardless of the roll (local z axis rotation) of the transform
		private static Vector3 GetRollUnaffectedRelPos(Transform t, Vector3 point){
	
			Quaternion rot = Quaternion.LookRotation(t.forward, Vector3.up);
			Vector3 result = Vector3.Scale(new Vector3(1/t.lossyScale.x, 1/t.lossyScale.y, 1/t.lossyScale.z), Quaternion.Inverse(rot) * (point - t.position));
			return result; 
	
		}
	

		// Get the relative position of a point from a transform, regardless of the roll (local z axis rotation) or pitch (local x axis rotation)of the transform
		private static Vector3 GetRollAndPitchUnaffectedRelPos(Transform t, Vector3 point){
	
			Quaternion rot = Quaternion.LookRotation(new Vector3(t.forward.x, 0f, t.forward.z), Vector3.up);
			Vector3 result = Vector3.Scale(new Vector3(1/t.lossyScale.x, 1/t.lossyScale.y, 1/t.lossyScale.z), Quaternion.Inverse(rot) * (point - t.position));
			return result; 
	
		}
	
		
		// Project a vector onto a plane
		private static Vector3 ProjectVectorOnPlane(Vector3 projectedVector, Vector3 planeNormal)
		{
			return (projectedVector - (Vector3.Dot(projectedVector, planeNormal)/planeNormal.sqrMagnitude) * planeNormal);
		}
	
		
		// Get the roll angle from horizon (how much the vehicle is rotated on its z axis wrt the horizon, regardless of pitch)
		private static float GetRollAngleFromHorizon(Transform vehicleTransform){
			
			Vector3 rollRefVector;
	
			// Project the forward vector onto the world horizontal plane
			rollRefVector = new Vector3(vehicleTransform.forward.x, 0f, vehicleTransform.forward.z);
	
			// Rotate it 90 degrees
			rollRefVector = Quaternion.AngleAxis(90, Vector3.up) * rollRefVector;
	
			
			float rollAngle = Vector3.Angle(vehicleTransform.right, rollRefVector);
	
			if ((vehicleTransform.right - rollRefVector).y < 0f) rollAngle *= -1;
			
			// Now get the angle
			return (rollAngle);
	
		}	
	}
}
