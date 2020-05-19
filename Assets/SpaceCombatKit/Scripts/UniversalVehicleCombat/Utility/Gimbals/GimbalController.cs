using UnityEngine;
using System.Collections;

namespace VSX.UniversalVehicleCombat
{

    /// <summary>
    /// Delegate to attach event functions to run when the gimbal is moved.
    /// </summary>
    /// <param name="horizontalPivotLocalRotation">The new horizontal pivot local rotation.</param>
    /// <param name="verticalPivotLocalRotation">The new vertical pivot local rotation.</param>
	public delegate void OnGimbalUpdatedEventHandler (Quaternion horizontalPivotLocalRotation, Quaternion verticalPivotLocalRotation);

    /// <summary>
    /// This class manages a gimballed (with vertical and horizontal axis rotation) apparatus such as a weapon.
    /// </summary>
    public class GimbalController : MonoBehaviour 
	{

		private bool disabled = false;
		public bool Disabled
		{
			get { return disabled; }
			set { disabled = value; }
		}

		[Header("Gimbal Parts")]

		[SerializeField]	
		private Transform horizontalPivot;

		[SerializeField]	
		private Transform verticalPivot;

	
		[Header("Constraints")]

		[SerializeField]
		private bool noConstraints = true;

		[SerializeField]
		private float horizontalArc = 180f;

		[SerializeField]
		private float minVerticalPivotAngle = 0;	

		[SerializeField]
		private float maxVerticalPivotAngle = 0;	

	
		[Header("Gimbal Motors")]

		[SerializeField]
		private float proportionalCoefficient = 0.5f;
		
		[SerializeField]
		private float integralCoefficient = 0;

		[SerializeField]
		private float derivativeCoefficient = 0.1f;

		[SerializeField]
		private float maxHorizontalAngularVelocity = 3f;
		
		[SerializeField]
		private float maxVerticalAngularVelocity = 3f;
	
		private float proportionalValue_HorizontalPivot = 0;
		private float integralValue_HorizontalPivot = 0;
		private float derivativeValue_HorizontalPivot = 0;

		private float proportionalValue_VerticalPivot = 0;
		private float integralValue_VerticalPivot = 0;
		private float derivativeValue_VerticalPivot = 0;

		private OnGimbalUpdatedEventHandler onGimbalUpdatedEventHandler;
		public event OnGimbalUpdatedEventHandler OnGimbalUpdatedEventHandler
		{
			add {	onGimbalUpdatedEventHandler += value;	} 
			remove {	onGimbalUpdatedEventHandler -= value;	}
		}

	
		/// <summary>
        /// Convert an angle of any magnitude to a -180 - 180 angle.
        /// </summary>
        /// <param name="angle">The original value.</param>
        /// <returns>The angle within -180 to 180.</returns>
		float WrapTo180Split(float angle)
		{
	
			if (angle < -180f)
			{
				float amountOver = Mathf.Abs(angle) - 180f;
				float tmp = Mathf.Floor(amountOver/360f) + 1;
				return angle + tmp * 360f;
			} 
			else if (angle > 180f) 
			{
				float amountOver = Mathf.Abs(angle) - 180f;
				float tmp = Mathf.Floor(amountOver/360f) + 1;
				return angle - tmp * 360f;
			} 
			else 
			{
				return angle;
			}
		}


		/// <summary>
        /// Directly set the gimbal rotation.
        /// </summary>
        /// <param name="horizontalPivotLocalRotation">The new rotation for the horizontal pivot.</param>
        /// <param name="verticalPivotLocalRotation">The new rotation for the vertical pivot.</param>
        /// <param name="callEvent">Whether to call the event for the gimbal rotation being changed.</param>
		public void SetGimbalRotation(Quaternion horizontalPivotLocalRotation, Quaternion verticalPivotLocalRotation, bool callEvent = true)
		{
			horizontalPivot.localRotation = horizontalPivotLocalRotation;
			verticalPivot.localRotation = verticalPivotLocalRotation;
			if (callEvent && onGimbalUpdatedEventHandler != null) onGimbalUpdatedEventHandler(horizontalPivot.localRotation, verticalPivot.localRotation);
		}


		/// <summary>
        /// Track a position in world 3D space with the gimbal
        /// </summary>
        /// <param name="target">The world space target position.</param>
        /// <param name="angleToTarget">The variable to be updated with the angle to the target.</param>
        /// <param name="snapToTarget">Whether to snap to the target or use the gimbal motors to smoothly move there.</param>
		public void TrackPosition(Vector3 target, out float angleToTarget, bool snapToTarget)
		{

			// For aim assist
			angleToTarget = 180;
			
			if (disabled) return;

			// ****************************** Rotate Horizontally to Target ******************************************

			// Get the local target position wrt the horizontally rotating body
			Vector3 targetLocalPos = transform.InverseTransformPoint(target);
			
			// Get the angle from the base to the target on the local horizontal plane
			float toTargetAngle_HorizontalPlane = Vector3.Angle(Vector3.forward, new Vector3(targetLocalPos.x, 0f, targetLocalPos.z));
		
			// Correct the sign 
			if (targetLocalPos.x < 0)
				toTargetAngle_HorizontalPlane *= -1;

			// Get the desired angle for the horizontal gimbal on the horizontal plane (wrt the gimbal parent forward vector)
			float desiredLocalHorizontalPivotAngle = toTargetAngle_HorizontalPlane;

			// Take constraints into account
			if (!noConstraints) 
				Mathf.Clamp(toTargetAngle_HorizontalPlane, -horizontalArc / 2f, horizontalArc / 2f);
			
			if (snapToTarget)
			{
				horizontalPivot.localRotation = Quaternion.Euler(0f, desiredLocalHorizontalPivotAngle, 0f);
			}
			else
			{

				// Get the current angle of the horizontal gimbal on the horizontal plane (wrt the gimbal parent forward vector)
				float currentHorizontalPivotAngle = WrapTo180Split(horizontalPivot.localRotation.eulerAngles.y);
	
				// Get the angle from current to desired for horizontal gimbal
				float horizontalPivotAngle = desiredLocalHorizontalPivotAngle - currentHorizontalPivotAngle;
	
				// If there are no constraints on the horizontal plane, allow the horizontal gimbal to cross the 180/-180 threshold
				if (Mathf.Abs(horizontalPivotAngle) > 180 && horizontalArc >= 360f)
				{
					horizontalPivotAngle = Mathf.Sign(horizontalPivotAngle) * -1 * (360 - Mathf.Abs(horizontalPivotAngle));
				}
				
				// Get the PID values
				proportionalValue_HorizontalPivot = horizontalPivotAngle * proportionalCoefficient;
				derivativeValue_HorizontalPivot = -horizontalPivotAngle * derivativeCoefficient;
				integralValue_HorizontalPivot += horizontalPivotAngle * integralCoefficient;
	
				// Calculate and constrain the rotation speed
				float rotationSpeedHorizontal = proportionalValue_HorizontalPivot + integralValue_HorizontalPivot + derivativeValue_HorizontalPivot;
				
				rotationSpeedHorizontal = Mathf.Clamp(rotationSpeedHorizontal, -maxHorizontalAngularVelocity, maxHorizontalAngularVelocity);
				
				// Rotate the horizontal gimbal
				horizontalPivot.Rotate(new Vector3(0f, rotationSpeedHorizontal, 0f));
			}



			// ****************************** Rotate Vertically to Target ******************************************

			Vector3 targetLocalPosV = targetLocalPos - transform.InverseTransformDirection(verticalPivot.position - horizontalPivot.position);
			angleToTarget = Vector3.Angle(verticalPivot.forward, target - transform.position);//Vector3.Angle(targetLocalPosV, Vector3.forward);

			// Get the angle from the local target position vector to the horizontal plane
			float desiredLocalVerticalPivotAngle = Vector3.Angle(targetLocalPosV, new Vector3(targetLocalPosV.x, 0f, targetLocalPosV.z));

			// Correct the sign
			if (targetLocalPosV.y > 0)
				desiredLocalVerticalPivotAngle *= -1;

			// Constrain the desired vertical pivot angle
			if (!noConstraints) 
				desiredLocalVerticalPivotAngle = Mathf.Clamp(desiredLocalVerticalPivotAngle, minVerticalPivotAngle, maxVerticalPivotAngle);

			if (snapToTarget)
			{
				verticalPivot.localRotation = Quaternion.Euler(desiredLocalVerticalPivotAngle, 0f, 0f);
			}
			else
			{
				// Get the current angle of the vertically pivoting body
				float currentVerticalPivotAngle = WrapTo180Split(verticalPivot.localRotation.eulerAngles.x);
				float verticalPivotAngle = desiredLocalVerticalPivotAngle - currentVerticalPivotAngle;
				
				// Get the PID values
				proportionalValue_VerticalPivot = verticalPivotAngle * proportionalCoefficient;
				derivativeValue_VerticalPivot = -verticalPivotAngle * derivativeCoefficient;
				integralValue_VerticalPivot += verticalPivotAngle * integralCoefficient;
	
				// Calculate and constrain the rotation speed
				float rotationSpeedVertical = proportionalValue_VerticalPivot + integralValue_VerticalPivot + derivativeValue_VerticalPivot;
				rotationSpeedVertical = Mathf.Clamp(rotationSpeedVertical, -maxVerticalAngularVelocity, maxVerticalAngularVelocity);
				
				// Rotate the vertical gimbal
				verticalPivot.Rotate(new Vector3(rotationSpeedVertical, 0f, 0f));
			}
			
			if (onGimbalUpdatedEventHandler != null) onGimbalUpdatedEventHandler(horizontalPivot.localRotation, verticalPivot.localRotation);

		}


		/// <summary>
        /// Rotate the gimbal incrementally around each of the axes. 
        /// </summary>
        /// <param name="rotationAmounts"></param>
		public void Rotate(Vector2 rotationAmounts)
		{

			if (disabled) return;

			// Get the current angle of the horizontal gimbal on the horizontal plane (wrt the gimbal parent forward vector)
			float currentHorizontalPivotAngle = WrapTo180Split(horizontalPivot.localRotation.eulerAngles.y);

			// Add the rotation
			float desiredHorizontalPivotAngle = currentHorizontalPivotAngle + rotationAmounts.x;
			
			// Constrain the angle
			desiredHorizontalPivotAngle = Mathf.Clamp(desiredHorizontalPivotAngle, -horizontalArc/2f, horizontalArc/2f);
		
			// Set the horizontal pivot
			horizontalPivot.localRotation = Quaternion.Euler(0f, desiredHorizontalPivotAngle, 0f);


			// Get the current angle of the vertical gimbal on the vertical plane 
			float currentVerticalPivotAngle = WrapTo180Split(verticalPivot.localRotation.eulerAngles.x);
			
			// Add the rotation
			float desiredVerticalPivotAngle = currentVerticalPivotAngle + rotationAmounts.y;
			
			// Constrain the angle
			desiredVerticalPivotAngle = Mathf.Clamp(desiredVerticalPivotAngle, minVerticalPivotAngle, maxVerticalPivotAngle);
		
			// Set the horizontal pivot
			verticalPivot.localRotation = Quaternion.Euler(desiredVerticalPivotAngle, 0f, 0f);

			if (onGimbalUpdatedEventHandler != null) onGimbalUpdatedEventHandler(horizontalPivot.localRotation, verticalPivot.localRotation);

		}
	}
}
