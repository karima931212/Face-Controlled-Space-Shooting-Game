using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using VSX.UniversalVehicleCombat;


namespace VSX.UniversalVehicleCombat
{

	/// <summary>
    /// This class provides static methods that can be used for different purposes within the game.
    /// </summary>
	public static class StaticFunctions 
	{
	
		/// <summary>
        /// Resize a generic list, inserting null references into new spaces
        /// </summary>
        /// <typeparam name="T">The object type that the List is holding.</typeparam>
        /// <param name="list">The list to be resized.</param>
        /// <param name="newSize">The new size for the list.</param>
		public static void ResizeList<T>(List<T> list, int newSize)
	    {
	        if (list.Count == newSize)
	            return;
	            
	        if (list.Count < newSize)
	        {
	            for (int i = 0; i < newSize - list.Count; ++i)
	            {
					list.Add(default(T));
	            }
	        } else {
	            for (int i = 0; i < list.Count - newSize; ++i){
	                //Remove the last one in the list
	                list.RemoveAt(list.Count-1);
	                --i;
	            }
	        }
	    }


        /// <summary>
        /// Calculate the maximum speed of a rigidbody, applying a specified force in a constant direction.
        /// </summary>
        /// <param name="rBody">The Rigidbody reference.</param>
        /// <param name="force">The force applied to the Rigidbody.</param>
        /// <returns>The maximum speed attainable by the Rigidbody.</returns>
        public static float CalculateMaxSpeed(Rigidbody rBody, float force){
	
			// Calculate the drag coefficient
			float coeff = 1 - Time.fixedDeltaTime * rBody.drag;
			float accel = (force / rBody.mass) * Time.fixedDeltaTime;
			
			return (Mathf.Max (accel/(1-coeff), 0f));
		}


		// Get the lead target position for aiming something to hit a moving target
        /// <summary>
        /// Get the world position along the path of a moving target that a projectile needs to be aimed at in order
        /// to hit the rigidbody.
        /// </summary>
        /// <param name="projectileStartPosition">The position from which the projectile will be shot.</param>
        /// <param name="projectileSpeed">The speed of the projectile.</param>
        /// <param name="target">The ITrackable reference for the target.</param>
        /// <returns>The world space lead target position</returns>
		public static Vector3 GetLeadPosition(Vector3 projectileStartPosition, float projectileSpeed, ITrackable target){

			// Get target direction
			Vector3 targetDirection = target.CachedTransform.position - projectileStartPosition;

            // Get the target velocity magnitude
            if (target.PhysicsInfo == null) Debug.Log(target.Label);
            Vector3 targetVelocity = target.PhysicsInfo.velocity;//target.HasRigidbody ? target.CachedRigidbody.velocity : Vector3.zero;
			float vE = targetVelocity.magnitude;
			
			// Note that the dot product of a and b (a.b) is also equal to |a||b|cos(theta) where theta = angle between them.
			// This saves using the components of the vectors themselves, only the magnitudes.
			
			// Get the length of the playerRelPos vector
			float playerRelPosMag = targetDirection.magnitude;
			
			// Get angle between player-target axis and target-forward axis
			float angPET = Vector3.Angle (targetDirection, target.CachedTransform.forward)*Mathf.Deg2Rad; // Vector3.Angle returns in degrees
			
			// Get the cosine of this angle
			float cosPET = Mathf.Cos (angPET);			
			
			// Get the coefficients of the quadratic equation
			float a = vE*vE - projectileSpeed * projectileSpeed;
			float b = 2 * playerRelPosMag * vE * cosPET;
			float c = playerRelPosMag * playerRelPosMag;
			
			// Check for solutions. If the discriminant (b*b)-(4*a*c) is:
			// >0: two real solutions - get the maximum one (the other will be a negative time value and can be discarded)
			// =0: one real solution (both solutions the same so either one will do)
			// <0; two imaginary solutions - never will hit the target
			float discriminant = (b*b)-(4*a*c);
			
			// Get the intercept time by solving the quadratic equation. Note that if a = 0 then we will be 
			// trying to divide by zero. But in that case no quadratics are necessary, the equation will be first-order
			// and can simply be rearranged to get the intercept time.		
			float interceptTime;
            if (a != 0)
            {
                // Quadratic solution
                interceptTime = Mathf.Max(((-1 * b) - Mathf.Sqrt(discriminant)) / (2 * a), ((-1 * b) + Mathf.Sqrt(discriminant)) / (2 * a));
            }
            else
            {
                interceptTime = -c / (2 * b);
            }
			
			return (target.CachedTransform.position + (targetVelocity * interceptTime));	
		}
    }
}
