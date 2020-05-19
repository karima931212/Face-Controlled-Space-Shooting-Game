using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace VSX.UniversalVehicleCombat
{

	/// <summary>
    /// This class manages the power management menu, where the player can see and modify how the power is distributed 
    /// between subsystems.
    /// </summary>
	public class PowerManagementMenuController : MonoBehaviour 
	{
		
		[Header ("General")]

		[SerializeField]
		private GameObject menuObject;	

		[SerializeField]
		private Text totalPowerText;
		
		[Header ("Power Adjustment")]

		[SerializeField]
		private Transform powerBall;

		[SerializeField]
		private float triangleHeight;


		[Header ("Power Indicators")]

		[SerializeField]
		private SubsystemPowerInfoController enginesInfoController;

		[SerializeField]
		private SubsystemPowerInfoController weaponsInfoController;

		[SerializeField]
		private SubsystemPowerInfoController shieldsInfoController;


		[Header("Triangle Slider")]

		[SerializeField]
		private float powerAdjustSpeed;

		Vector3 enginesApexPos;

		Vector3 powerBallPosition;

		float enginesFraction;
		float weaponsFraction;
		float shieldsFraction;

		Vehicle focusedVehicle = null;
		bool hasFocusedVehicle = false;
		
		

		void Awake()
		{

			// Calculate the necessary triangle parameters

			float sideLength = 1f/(Mathf.Sin(60f*Mathf.Deg2Rad));

			enginesApexPos = new Vector3(-sideLength/2f, 1f/3f, 0f);
			
			enginesFraction = 1f/3f;
			weaponsFraction = 1f/3f;
			shieldsFraction = 1f/3f;

			powerBallPosition = Vector3.zero;

			menuObject.SetActive(false);

			UVCEventManager.Instance.StartListening(UVCEventType.OnFocusedVehicleChanged, OnFocusedVehicleChanged);
			
		}


        void Start()
		{
			// Initialize the power distribution
			RecalculatePower();
		}
	
	
		/// <summary>
        /// Activate the menu.
        /// </summary>
        /// <returns>Whether the menu was successfully activated.</returns>
		public bool Activate()
		{
            
			if (!hasFocusedVehicle || !focusedVehicle.HasPower) return false;

			RecalculatePower();
			menuObject.SetActive(true);

			return true;

		}


		/// <summary>
        /// Deactivate the menu.
        /// </summary>
		public void Deactivate()
		{
			menuObject.SetActive(false);
		}

		
		/// <summary>
        /// Event called when the focused vehicle changes.
        /// </summary>
        /// <param name="newVehicle"></param>
		public void OnFocusedVehicleChanged(Vehicle newVehicle)
		{
			if (newVehicle != null && newVehicle.HasPower)
			{
				focusedVehicle = newVehicle;
			}
			else
			{
				focusedVehicle = null;
			}

			hasFocusedVehicle = focusedVehicle != null;
		}


		/// <summary>
        /// Called when the player shifts the power ball horizontally.
        /// </summary>
        /// <param name="moveRight">Whether the ball is being moved right.</param>
		public void MovePowerBallHorizontally(bool moveRight){
			
			if (moveRight) 
				powerBallPosition += Vector3.right*powerAdjustSpeed*Time.unscaledDeltaTime;
			else 
				powerBallPosition += -1f * Vector3.right*powerAdjustSpeed*Time.unscaledDeltaTime;

			ClampInsideTriangle();
			RecalculatePower();

		}


        /// <summary>
        /// Called when the player shifts the power ball vertically.
        /// </summary>
        /// <param name="moveUp">Whether the power ball was shifted up.</param>
        public void MovePowerBallVertically(bool moveUp)
		{
           if (moveUp) 
				powerBallPosition += Vector3.up*powerAdjustSpeed*Time.unscaledDeltaTime;
			else 
				powerBallPosition += -1f * Vector3.up*powerAdjustSpeed*Time.unscaledDeltaTime;

			ClampInsideTriangle();
			RecalculatePower();
			
		}


        /// <summary>
        /// Update the power distribution based on the power triangle setting.
        /// </summary>
		void RecalculatePower()
		{

			if (!hasFocusedVehicle) return;
			
			powerBall.GetComponent<RectTransform>().localPosition = powerBallPosition * triangleHeight;
			
			// Calculate the shield (bottom point) fraction 
			shieldsFraction = Mathf.Abs(powerBallPosition.y - (1f/3f)); // The distance below midpoint minus the length of triangle above midpoint
			
			// Calculate the engines (top left) fraction
			float oppositeSideSlope = Mathf.Tan(60f * Mathf.Deg2Rad);		// Get the slope of the side opposite the engine apex
			float yInt = powerBallPosition.y - oppositeSideSlope * powerBallPosition.x; // Get the y intercept of the line that passes through the powerball pos
			float distToLine = DistToLine(enginesApexPos.x, enginesApexPos.y, oppositeSideSlope, yInt);
			enginesFraction = 1-distToLine;
			weaponsFraction = 1-(shieldsFraction + enginesFraction);
			
			focusedVehicle.Power.SetSubsystemDistributablePowerFraction (SubsystemType.Engines, enginesFraction);
			focusedVehicle.Power.SetSubsystemDistributablePowerFraction (SubsystemType.Weapons, weaponsFraction);
			focusedVehicle.Power.SetSubsystemDistributablePowerFraction (SubsystemType.Health, shieldsFraction);
			
		}

		
        /// <summary>
        /// Clamp the power ball inside the triangle.
        /// </summary>
		void ClampInsideTriangle()
		{

			// Clamp power ball inside the triangle
			powerBallPosition.y = Mathf.Clamp (powerBallPosition.y, -2/3f, 1/3f);
			float maxPosX = (powerBallPosition.y + 2/3f) * Mathf.Tan (30f * Mathf.Deg2Rad);
			powerBallPosition.x = Mathf.Clamp (powerBallPosition.x, -maxPosX, maxPosX);		

		}

		
		// Find the distance from a point to a line (Point (px,py) to Line y = mx + c)
		float DistToLine(float px, float py, float m, float c)
		{
			float numerator = Mathf.Abs (m * px - py + c);
			float denominator = Mathf.Sqrt(m*m + 1);
			return (numerator/denominator);
		}

	
		// Called every frame
		void Update()
		{

			if (!hasFocusedVehicle) return;
			
            RecalculatePower();

			totalPowerText.text = "TOTAL: " + Mathf.RoundToInt(focusedVehicle.Power.TotalPower).ToString() + " kW";
			
			enginesInfoController.SetPowerValues(focusedVehicle.Power.GetSubsystemTotalPower(SubsystemType.Engines), focusedVehicle.Power.TotalPower, 
													focusedVehicle.Power.GetSubsystemDistributablePower(SubsystemType.Engines));

			weaponsInfoController.SetPowerValues(focusedVehicle.Power.GetSubsystemTotalPower(SubsystemType.Weapons), focusedVehicle.Power.TotalPower, 
													focusedVehicle.Power.GetSubsystemDistributablePower(SubsystemType.Weapons));

			shieldsInfoController.SetPowerValues(focusedVehicle.Power.GetSubsystemTotalPower(SubsystemType.Health), focusedVehicle.Power.TotalPower, 
													focusedVehicle.Power.GetSubsystemDistributablePower(SubsystemType.Health));

		}
	}
}