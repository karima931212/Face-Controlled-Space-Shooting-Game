using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace VSX.UniversalVehicleCombat
{

    /// <summary>
    /// This class holds data that is used by the AI to make decisions during combat.
    /// </summary>
    public class CombatDecisionInfo
	{
	
		public Vector3 toTargetVector;

		public Vector3 targetPosition;
		
        /// <summary>
        /// How much the AI is facing the target (-1 to 1, using the dot product of the forward vectors)
        /// </summary>
		public float facingTargetAmount;
	
		public float angleToTarget;

        /// <summary>
        /// How much the target is facing the AI (-1 to 1, using the dot product of the forward vectors)
        /// </summary>
        public float targetFacingAmount;
	
		public float distanceToTarget;
	
        /// <summary>
        /// A 0-1 value for evaluating the primary firing solution quality.
        /// </summary>
		public float primaryFiringSolutionQuality;

        /// <summary>
        /// A 0-1 value for evaluating the secondary firing solution quality.
        /// </summary>
        public float secondaryFiringSolutionQuality;
		
	}

	/// <summary>
    /// This class provides an example of AI combat behavior for a spaceship.
    /// </summary>
	public class CombatBehaviour : MonoBehaviour, IVehicleControlBehaviour 
	{
	
		[HideInInspector]
		public CombatState currentCombatState;

		[Header("Combat Parameters")]
	
		[SerializeField]
		private Vector2 minMaxEngageDistance = new Vector2(100, 500);
	
		[SerializeField]
		private Vector2 minMaxAttackTime = new Vector2(10, 20);

		[SerializeField]
		private Vector2 minMaxEvadeTime = new Vector2(5, 10);
	
		[SerializeField]
		private Vector2 minMaxPrimaryFiringPeriod = new Vector2(1,3);

		[SerializeField]
		private Vector2 minMaxPrimaryFiringInterval = new Vector2(0.5f, 2);

		[SerializeField]
		private Vector2 minMaxSecondaryFiringPeriod = new Vector2(7, 15);

		[SerializeField]
		private Vector2 minMaxSecondaryFiringInterval = new Vector2(10, 25);
		
		private float minFiringAngle = 15f;
	
		private float secondaryWeaponActionStartTime = 0;
		private float secondaryWeaponActionPeriod = 0f;
		private bool isFiringSecondary = false;

		private float primaryWeaponActionStartTime = 0;
		private float primaryWeaponActionPeriod = 0f;
		private bool isFiringPrimary = false;
	
		private Vector3 evadePathDirection;
		private Vector3 evadeWeaveDirection;
	
		private float combatBehaviourStartTime = 0;
		private float nextCombatBehaviourPeriod = 0;

		
		[Header("Behaviour Characteristics")]
	
		[SerializeField]
		private Vector2 baseStaminaChangeRate = new Vector2(0.1f, 0.2f);
	
		private float staminaRandomizer;
		
		[SerializeField]
		private Vector2 minMaxStaminaRandomizer = new Vector2(1, 1);
		
		private float aggressionCoefficient = 1;
		private float anxietyCoefficient = 0f;

		private CombatDecisionInfo decisionInfo;
		
		private BehaviourBlackboard blackboard;

		
	
		private void Awake()
		{
			decisionInfo = new CombatDecisionInfo();
		}


        /// <summary>
        /// Initialize this vehicle control behaviour with a blackboard of data
        /// </summary>
        /// <param name="blackboard">The blackboard for this behavior to read from and write to.</param>
        public void Initialize(BehaviourBlackboard blackboard)
		{
			this.blackboard = blackboard;
		}


        // Called when the gameobject is enabled.
		private void OnEnable()
		{
			aggressionCoefficient = 1;
			staminaRandomizer = Random.Range(minMaxStaminaRandomizer.x, minMaxStaminaRandomizer.y);
			evadePathDirection = Vector3.forward;
			evadeWeaveDirection = evadePathDirection;
		}
	

        // Called when the gameobject is disabled.
		private void OnDisable()
		{
			StopAllCoroutines();
		}


        /// <summary>
        /// Do the emotions that regulate the attack/evade cycle of the AI behaviour
        /// </summary>
        private void DoEmotions()
		{
			
			if (!blackboard.Vehicle.Radar.HasSelectedTarget)
				return;
	
			evadePathDirection.y = -blackboard.Vehicle.CachedTransform.position.y / 500f;
			
	
			// ********* Aggression **********		
	
			float distanceAmount = Vector3.Distance(blackboard.Vehicle.CachedTransform.position, blackboard.Vehicle.Radar.SelectedTarget.CachedTransform.position) / 1000;
			
			float facingCoefficient = Mathf.Clamp(Vector3.Dot(blackboard.Vehicle.CachedTransform.forward, decisionInfo.toTargetVector.normalized), 0, 1f);
		
			float isFacedCoefficient = Mathf.Clamp(Vector3.Dot(blackboard.Vehicle.Radar.SelectedTarget.CachedTransform.forward, -(decisionInfo.toTargetVector).normalized), 0, 1f);
		
			distanceAmount *= distanceAmount;
	

			// Update the aggression coefficient
			if (currentCombatState == CombatState.Attacking)
			{
				float amount0 = (1 - facingCoefficient);
				float amount = staminaRandomizer * (baseStaminaChangeRate.x + amount0 * (baseStaminaChangeRate.y - baseStaminaChangeRate.x));
				aggressionCoefficient = Mathf.Clamp(aggressionCoefficient - amount * Time.deltaTime, -1f, 1f);
				
			}
			else if (currentCombatState == CombatState.Evading)
			{
				float amount0 = (1 - isFacedCoefficient);
				float amount = staminaRandomizer * (baseStaminaChangeRate.x + amount0 * (baseStaminaChangeRate.y - baseStaminaChangeRate.x)) + distanceAmount;
				aggressionCoefficient = Mathf.Clamp(aggressionCoefficient + amount * Time.deltaTime, -1f, 1f);
			}
			

            // Update the anxiety coefficient
			if (blackboard.Vehicle.Radar.missileThreats.Count > 0)
			{
				anxietyCoefficient = 1;
			}
			else
			{
				if (blackboard.Vehicle.Radar.HasSelectedTarget)
				{				
					anxietyCoefficient = Mathf.Lerp(anxietyCoefficient, decisionInfo.targetFacingAmount, 1f * Time.deltaTime);					
				}
				else
				{
					anxietyCoefficient = Mathf.Lerp(anxietyCoefficient, 0, 1f * Time.deltaTime);
				}
			}
			
			// Switch to evading
			if (currentCombatState == CombatState.Attacking)
			{			
				if ((Time.time - combatBehaviourStartTime > nextCombatBehaviourPeriod) || decisionInfo.distanceToTarget < minMaxEngageDistance.x)
				{

					evadePathDirection = Quaternion.Euler(new Vector3(0f, Random.Range(-90, 90), 0f)) * -decisionInfo.toTargetVector;

					Vector3 toCenterVec = (Vector3.zero - blackboard.Vehicle.CachedTransform.position);

					float fromCenterAmount = Mathf.Clamp(toCenterVec.magnitude / 1000f, 0f, 1f);
					evadePathDirection = (fromCenterAmount * toCenterVec.normalized + (1 - fromCenterAmount) * evadePathDirection.normalized).normalized;
					
					evadeWeaveDirection = AddWeave(evadePathDirection, 0.2f, 0.5f, anxietyCoefficient);
					
					staminaRandomizer = Random.Range(minMaxStaminaRandomizer.x, minMaxStaminaRandomizer.y);
					aggressionCoefficient = -1;
					currentCombatState = CombatState.Evading;
					combatBehaviourStartTime = Time.time;
					nextCombatBehaviourPeriod = Random.Range(minMaxEvadeTime.x, minMaxEvadeTime.y);
				}
			}
			else
			{
				if (Time.time - combatBehaviourStartTime > nextCombatBehaviourPeriod)
				{
					staminaRandomizer = Random.Range(minMaxStaminaRandomizer.x, minMaxStaminaRandomizer.y);
					aggressionCoefficient = 1;
					currentCombatState = CombatState.Attacking;
					combatBehaviourStartTime = Time.time;
					nextCombatBehaviourPeriod = Random.Range(minMaxAttackTime.x, minMaxAttackTime.y);
				}
			}
		}


		// Add a weave to the path of the vehicle
		private Vector3 AddWeave(Vector3 pathDirection, float weaveSpeed, float weaveSlope, float anxietyCoefficient)
		{
			float offsetX = (Mathf.PerlinNoise(Time.time * weaveSpeed, Time.time * weaveSpeed) - 0.5f) * 2f;
			float offsetY = (Mathf.PerlinNoise(Time.time * weaveSpeed, Time.time * weaveSpeed) - 0.5f) * 2f;
			Vector3 offsetVec = new Vector3(offsetX, offsetY).normalized * anxietyCoefficient * 2f;
			Vector3 offset = new Vector3(offsetVec.x, offsetVec.y, 1).normalized;
			pathDirection = Quaternion.FromToRotation(Vector3.forward, pathDirection) * offset;
	
			return pathDirection;
		}
	
		
		// Update the data that is used to calculate decisions
		private void UpdateDecisionInfo()
		{
			if (!blackboard.Vehicle.Radar.HasSelectedTarget)
				return;
			
			if (blackboard.Vehicle.Weapons.HasWeaponsComputer && blackboard.Vehicle.Weapons.MountedWeaponsComputer.WeaponsComputer.IsLeadTargetComputer)
			{
				int index = blackboard.Vehicle.Weapons.MountedWeaponsComputer.WeaponsComputer.LeadTargetComputer.LeadTargetDataList.Count > 0 ? 0 : -1;
				
				if (index != -1)
				{
					decisionInfo.targetPosition = blackboard.Vehicle.Weapons.MountedWeaponsComputer.WeaponsComputer.LeadTargetComputer.LeadTargetDataList[0].LeadTargetPosition;
					
				}
				else
				{
					decisionInfo.targetPosition = blackboard.Vehicle.Radar.SelectedTarget.CachedTransform.position;
				}
			}
			else
			{
				decisionInfo.targetPosition = blackboard.Vehicle.Radar.SelectedTarget.CachedTransform.position;
			}

			decisionInfo.toTargetVector = decisionInfo.targetPosition - blackboard.Vehicle.CachedTransform.position;
	
			decisionInfo.distanceToTarget = Vector3.Distance(blackboard.Vehicle.CachedTransform.position, decisionInfo.targetPosition);
			decisionInfo.angleToTarget = Vector3.Angle(blackboard.Vehicle.CachedTransform.forward, decisionInfo.toTargetVector);
			decisionInfo.facingTargetAmount = Vector3.Dot(blackboard.Vehicle.CachedTransform.forward, decisionInfo.toTargetVector.normalized);
            if (blackboard.Vehicle.Radar.SelectedTarget.CachedTransform == null) Debug.Log(blackboard.Vehicle.Label);
            decisionInfo.targetFacingAmount = Vector3.Dot(blackboard.Vehicle.Radar.SelectedTarget.CachedTransform.forward, -(decisionInfo.toTargetVector).normalized);

		}
	
	
		// Get a 0-1 value that describes how good of a firing position the primary weapons are in
		float GetPrimaryFiringSolutionQuality()
		{
			
			float distanceFactor = decisionInfo.distanceToTarget < minMaxEngageDistance.y ? 1f : 0f;
			float angleFactor = Mathf.Clamp(1  - decisionInfo.angleToTarget / minFiringAngle, 0f, 1f);
			
			return (distanceFactor * angleFactor);

		}


		// Get a 0-1 value that describes how good of a firing position the secondary weapons are in
		private float GetSecondaryFiringSolutionQuality()
		{
			
			// If there is no locking data, exit
			if (!blackboard.Vehicle.Weapons.HasWeaponsComputer ||
			    !blackboard.Vehicle.Weapons.MountedWeaponsComputer.WeaponsComputer.IsLockingComputer ||
			    blackboard.Vehicle.Weapons.MountedWeaponsComputer.WeaponsComputer.LockingComputer.LockingDataList.Count == 0)
			{
				return 0; 
			}
				

			bool foundLock = false;
			for (int i = 0; i < blackboard.Vehicle.Weapons.MountedWeapons.Count; ++i)
			{
				if (blackboard.Vehicle.Weapons.MountedWeapons[i].IsMissileModule)
				{
					if (blackboard.Vehicle.Weapons.MountedWeapons[i].MissileModule.LockState == LockState.Locked)
					{
						foundLock = true;
					}
				}
			}
			return (foundLock ? 1 : 0);
		}


		// Update whether or not the AI can fire this frame
		private void UpdateFiring()
		{
			
			// Initialize firing flags according to firing solution quality
			bool canFirePrimary = GetPrimaryFiringSolutionQuality() > 0.5f;
			bool canFireSecondary = GetSecondaryFiringSolutionQuality() > 0.5f;

			if (canFirePrimary)
			{ 
				
				// If weapon can fire but has not been firing, check if the cooling off period has finished before firing it again
				if (!isFiringPrimary)
				{
					// If hasn't finished cooling off period, don't fire
					if (Time.time - primaryWeaponActionStartTime < primaryWeaponActionPeriod)
					{
						canFirePrimary = false;
					}
					else
					{
						primaryWeaponActionStartTime = Time.time;
						primaryWeaponActionPeriod = Random.Range(minMaxPrimaryFiringPeriod.x, minMaxPrimaryFiringPeriod.y);
						isFiringPrimary = true;
					}
				}
				// If weapon can fire and has been firing, check if it has finished the firing period
				else
				{
					// If weapon has been firing long enough, stop firing
					if (Time.time - primaryWeaponActionStartTime > primaryWeaponActionPeriod)
					{
						canFirePrimary = false;
						primaryWeaponActionStartTime = Time.time;
						primaryWeaponActionPeriod = Random.Range(minMaxPrimaryFiringInterval.x, minMaxPrimaryFiringInterval.y);
						isFiringPrimary = false;
					}
				}
			}
			
	
			if (canFireSecondary)
			{ 
				// If weapon can fire but has not been firing, check if the cooling off period has finished before firing it again
				if (!isFiringSecondary)
				{
					// If hasn't finished cooling off period, don't fire
					if (Time.time - secondaryWeaponActionStartTime < secondaryWeaponActionPeriod)
					{
						canFireSecondary = false;
					}
					else
					{
						secondaryWeaponActionStartTime = Time.time;
						secondaryWeaponActionPeriod = Random.Range(minMaxSecondaryFiringPeriod.x, minMaxSecondaryFiringPeriod.y);
						isFiringSecondary = true;
					}
				}
				// If weapon can fire and has been firing, check if it has been triggered
				else
				{
					// If weapon has been firing long enough, stop firing
					if (Time.time - secondaryWeaponActionStartTime > secondaryWeaponActionPeriod)
					{
						canFireSecondary = false;
						secondaryWeaponActionStartTime = Time.time;
						secondaryWeaponActionPeriod = Random.Range(minMaxSecondaryFiringInterval.x, minMaxSecondaryFiringInterval.y);
						isFiringSecondary = false;
					}
				}
			}		

			blackboard.canFirePrimaryWeapon = canFirePrimary;
			blackboard.canFireSecondaryWeapon = canFireSecondary;
		}
	
		
		/// <summary>
        /// Called every frame to update this behavor when it is running.
        /// </summary>
		public void Tick()
		{
			
			UpdateDecisionInfo();

			DoEmotions();

			UpdateFiring();	
			
			Vector3 steeringTarget = blackboard.Vehicle.CachedTransform.position + blackboard.Vehicle.CachedTransform.forward;
			float snappiness = 1;


			if (!blackboard.Vehicle.Radar.HasSelectedTarget)
			{
				blackboard.canFirePrimaryWeapon = false;
				blackboard.canFireSecondaryWeapon = false;
				
				return;
			}
				
			switch (currentCombatState)
			{
				case CombatState.Attacking:
					
					Vector3 targetPosition = blackboard.Vehicle.Radar.SelectedTarget.CachedTransform.position;
					
					if (blackboard.Vehicle.Weapons.HasWeaponsComputer && blackboard.Vehicle.Weapons.MountedWeaponsComputer.WeaponsComputer.IsLeadTargetComputer)
					{
						if (blackboard.Vehicle.Weapons.MountedWeaponsComputer.WeaponsComputer.LeadTargetComputer.LeadTargetDataList.Count > 0)
						{
							targetPosition = blackboard.Vehicle.Weapons.MountedWeaponsComputer.WeaponsComputer.LeadTargetComputer.LeadTargetDataList[0].LeadTargetPosition;
						}
					}
					Vector3 targetDirection = (targetPosition - blackboard.Vehicle.CachedTransform.position).normalized;
		
					steeringTarget = blackboard.Vehicle.CachedTransform.position + targetDirection;
					float amountOfEngageDistance = (decisionInfo.distanceToTarget - minMaxEngageDistance.x) / (minMaxEngageDistance.y - minMaxEngageDistance.x);
					
					blackboard.throttleValues = new Vector3(0f, 0f, Mathf.Clamp(amountOfEngageDistance, 0.25f, 1));
					snappiness = Mathf.Clamp(1 - (decisionInfo.angleToTarget / 35f), 0.6f, 1f);
					
					break;

				default:
					
					evadeWeaveDirection = AddWeave(evadePathDirection, 0.2f, 1f, anxietyCoefficient);
					steeringTarget = blackboard.Vehicle.CachedTransform.position + evadeWeaveDirection;			
					blackboard.throttleValues = new Vector3(0, 0, 1);
					snappiness = 0.5f;
					blackboard.integralSteeringVals = Vector3.zero;
					break;
			}
			
			Vector3 controlPIDCoeffs = new Vector3(blackboard.SteeringPIDCoeffs.x, snappiness * blackboard.SteeringPIDCoeffs.y, blackboard.SteeringPIDCoeffs.z);
			Vector3 dirToTarget = (steeringTarget - blackboard.Vehicle.CachedTransform.position).normalized;
	
			steeringTarget = blackboard.obstacleAvoidanceStrength * (blackboard.Vehicle.CachedTransform.position + blackboard.obstacleAvoidanceDirection) +
			(1 - blackboard.obstacleAvoidanceStrength) * (blackboard.Vehicle.CachedTransform.position + dirToTarget);
			
			Maneuvring.TurnToward(blackboard.Vehicle.CachedTransform, steeringTarget, controlPIDCoeffs, 
				blackboard.MaxRotationAngles, out blackboard.steeringValues, ref blackboard.integralSteeringVals);

			blackboard.steeringValues *= snappiness;
			
			blackboard.throttleValues = (1f - blackboard.obstacleAvoidanceStrength * 0.5f) * blackboard.throttleValues;		
			
		}
	}
}
