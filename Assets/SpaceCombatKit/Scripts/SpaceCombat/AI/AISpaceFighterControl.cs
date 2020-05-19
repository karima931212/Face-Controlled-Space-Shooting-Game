using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace VSX.UniversalVehicleCombat
{

	/// <summary>
    /// This class provides a control script for an AI space fighter.
    /// </summary>
	public class AISpaceFighterControl : MonoBehaviour, IVehicleInput 
	{
	
		private VehicleControlClass vehicleControlClass = VehicleControlClass.SpaceshipFighter;
		public VehicleControlClass VehicleControlClass { get { return vehicleControlClass; } }

		BehaviourBlackboard blackboard;
	
		[HideInInspector]
		public BehaviourState currentBehaviourState = BehaviourState.Patrolling;
	
		[HideInInspector]
		public CombatState currentCombatState;
	
		[Header("Base PID Coefficients")]

		[SerializeField]
		private Vector3 throttlePIDCoeffs = new Vector3(0.1f, 0.1f, 0.01f);
		
		[SerializeField]
		private Vector3 steeringPIDCoeffs = new Vector3(0.1f, 0.1f, 0.01f);
		
		[Header("Control Limits")]

		[SerializeField]
		private Vector3 maxRotationAngles = new Vector3(360, 360, 45);	

		[Header("Behaviours")]

		[SerializeField]
		private CombatBehaviour combatBehaviourPrefab;
		private CombatBehaviour combatBehaviour;
	
		[SerializeField]
		private ObstacleAvoidanceBehaviour obstacleAvoidanceBehaviourPrefab;
		private ObstacleAvoidanceBehaviour obstacleAvoidanceBehaviour;
		public ObstacleAvoidanceBehaviour ObstacleAvoidanceBehaviour { get { return obstacleAvoidanceBehaviour; } }

		[SerializeField]
		private PatrolBehaviour patrolBehaviourPrefab;
		private PatrolBehaviour patrolBehaviour;

		private GroupMember groupMember;

		private bool running = false;
		public bool Running { get { return running; } }

		private bool controlsDisabled = false;
        /// <summary>
        /// Set the controls to be disabled temporarily.
        /// </summary>
		public bool ControlsDisabled
		{
			get { return controlsDisabled; }
			set { controlsDisabled = value; }
		}



        /// <summary>
        /// Initialize this vehicle input script
        /// </summary>
        /// <param name="agent"></param>
        public void Initialize(GameAgent agent)
		{

            blackboard = new BehaviourBlackboard();

            groupMember = agent.GetComponent<GroupMember>();
            if (groupMember != null && agent.IsInVehicle) groupMember.ResetPatrolTargetToNearest();

            blackboard.GroupMember = groupMember;

            // Instantiate the behaviours
            if (obstacleAvoidanceBehaviourPrefab != null)
            {
                obstacleAvoidanceBehaviour = Instantiate(obstacleAvoidanceBehaviourPrefab, transform) as ObstacleAvoidanceBehaviour;
                obstacleAvoidanceBehaviour.Initialize(blackboard);
            }

            if (combatBehaviourPrefab != null)
            {
                combatBehaviour = Instantiate(combatBehaviourPrefab, transform) as CombatBehaviour;
                combatBehaviour.Initialize(blackboard);
            }

            if (patrolBehaviourPrefab != null)
            {
                patrolBehaviour = Instantiate(patrolBehaviourPrefab, transform) as PatrolBehaviour;
                patrolBehaviour.Initialize(blackboard);
            }

            blackboard.Initialize(steeringPIDCoeffs, throttlePIDCoeffs, maxRotationAngles);
			blackboard.SetVehicle(agent.Vehicle);
			
		}

	
		/// <summary>
        /// Begin running this vehicle input script.
        /// </summary>
		public void Begin()
		{
			running = true;
		}


		/// <summary>
        /// Finish running this vehicle input script.
        /// </summary>
		public void Finish()
		{
			running = false;
		}


		/// <summary>
        /// Set the target (e.g. by the group manager)
        /// </summary>
        /// <param name="newTarget">The new target for this game agent.</param>
		public void SetTarget(ITrackable newTarget)
		{
			blackboard.Vehicle.Radar.SetSelectedTarget (newTarget);
		}
	
	
		// Fire or stop firing weapons
		void WeaponActions()
		{
			
			if (!blackboard.Vehicle.HasWeapons) return;

			if (blackboard.canFirePrimaryWeapon)
			{ 
				blackboard.Vehicle.Weapons.StartFiringOnTrigger(0);
			}
			else
			{
				blackboard.Vehicle.Weapons.StopFiringOnTrigger(0);
			}
	
			if (blackboard.canFireSecondaryWeapon)
			{ 
				
				blackboard.Vehicle.Weapons.StartFiringOnTrigger(1);
			}
			else
			{
				blackboard.Vehicle.Weapons.StopFiringOnTrigger(1);
			}
		}	


        // Called every frame
		public void Update()
		{
			
			if (!running)
				return;
					
			if (!controlsDisabled)
			{

                // Do obstacle avoidance first
				if (obstacleAvoidanceBehaviour != null)
				{
					obstacleAvoidanceBehaviour.Tick();
				}
	
                // Set the state to combat if there is a target
				if (blackboard.Vehicle.HasRadar && blackboard.Vehicle.Radar.HasSelectedTarget && combatBehaviour != null)
				{
					if (currentBehaviourState != BehaviourState.Combat)
					{
						
						blackboard.integralSteeringVals = Vector3.zero;
						blackboard.integralThrottleVals = Vector3.zero;
						currentBehaviourState = BehaviourState.Combat;
					
					}
				}
				else
				{
                    // Set the state to patrolling if there is no target
					if (currentBehaviourState != BehaviourState.Patrolling)
					{
						blackboard.integralSteeringVals = Vector3.zero;
						blackboard.canFirePrimaryWeapon = false;
						blackboard.canFireSecondaryWeapon = false;
	
						if (groupMember != null)
							groupMember.ResetPatrolTargetToNearest();
	
						currentBehaviourState = BehaviourState.Patrolling;
					}
				}
				

                // Tick the appropriate behavior according to the current state
				switch (currentBehaviourState)
				{
	
					case BehaviourState.Combat:
						combatBehaviour.Tick();
						break;
					default: 
						patrolBehaviour.Tick();
						break;		
				}
				
				// Implement thrust and steering
				if (blackboard.Vehicle.HasEngines)
				{
					blackboard.throttleValues = new Vector3(0f, 0f, (1f - blackboard.obstacleAvoidanceStrength * 0.5f) * blackboard.throttleValues.z);		
					blackboard.Vehicle.Engines.SetRotationInputs(blackboard.steeringValues);
					blackboard.Vehicle.Engines.SetTranslationInputs(blackboard.throttleValues);
				}
				
                // Implement weapon actions
				WeaponActions();

			}
		}
	}
}
