using UnityEngine;
using System.Collections;

namespace VSX.UniversalVehicleCombat
{

	/// <summary>
    /// This class represents a game agent which is a member within a group of game agents.
    /// </summary>
	public class GroupMember : MonoBehaviour
	{
			
		private GameAgent gameAgent;
		public GameAgent GameAgent { get { return gameAgent; } }	

		private Vehicle formationLeader = null;
		public Vehicle FormationLeader{ get { return formationLeader; } }
	
		private bool hasFormationLeader = false;
		public bool HasFormationLeader { get { return hasFormationLeader; } }

		private Vector3 formationOffset = Vector3.zero;
		public Vector3 FormationOffset { get { return formationOffset; } } 

		[SerializeField]
		private PatrolRoute patrolRoute;
		public PatrolRoute PatrolRoute
		{
			get { return patrolRoute; }
			set
			{
				patrolRoute = value;
				hasPatrolRoute = patrolRoute != null;
				if (hasPatrolRoute) ResetPatrolTargetToNearest();
			}
		}
	
		private bool hasPatrolRoute;
		public bool HasPatrolRoute { get { return hasPatrolRoute; } }
	
		private int currentPatrolTargetIndex = 0;
		public int CurrentPatrolTargetIndex { get { return currentPatrolTargetIndex; } }



		void Awake()
		{
			gameAgent = GetComponent<GameAgent>();
		}


		// Set by the group manager to set the formation target
        /// <summary>
        /// Set (by the group manager) to update the formation target position.
        /// </summary>
        /// <param name="formationLeader">The formation leader.</param>
        /// <param name="offset">The position relative to the formation leader for this vehicle.</param>
		public void SetFormationTarget (Vehicle formationLeader, Vector3 offset)
		{
			this.formationLeader = formationLeader;
			this.formationOffset = offset;
		}


		/// <summary>
        /// Move on to the next patrol target in a patrol route.
        /// </summary>
		public void IncrementPatrolTargetIndex()
		{
			currentPatrolTargetIndex = (currentPatrolTargetIndex + 1) % patrolRoute.PatrolTargets.Count;
		}
	

		/// <summary>
        /// Set the next patrol target to the nearest one.
        /// </summary>
		public void ResetPatrolTargetToNearest()
		{
			if (!hasPatrolRoute || !gameAgent.IsInVehicle) return;
			for (int i = 0; i < patrolRoute.PatrolTargets.Count; ++i)
			{
				float distanceToNext = Vector3.Distance(gameAgent.Vehicle.CachedTransform.position, patrolRoute.PatrolTargets[i].position);
				float distanceToCurrentTarget = Vector3.Distance(gameAgent.Vehicle.CachedTransform.position, patrolRoute.PatrolTargets[currentPatrolTargetIndex].position);
				if (distanceToNext < distanceToCurrentTarget)
				{
					currentPatrolTargetIndex = i;
				}
			}
		}
	}
}
