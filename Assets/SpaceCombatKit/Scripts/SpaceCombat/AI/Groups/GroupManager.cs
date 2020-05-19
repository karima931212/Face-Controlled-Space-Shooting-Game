using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace VSX.UniversalVehicleCombat
{

    /// <summary>
    /// The different formation types that the group can take.
    /// </summary>
	public enum FormationType
	{
		Delta
	}


	// This class manages a group of AI pilots, allowing target distribution, formation behaviour etc
    /// <summary>
    /// This class manages a group of AI game agents, allowing even distribution of targets, formation behavior and many other possibilities.
    /// </summary>
	public class GroupManager : MonoBehaviour 
	{
	
		[SerializeField]
		private List<GroupMember> groupMembers = new List<GroupMember>();

		[SerializeField]
		private FormationType formationType;

		[SerializeField]
		private PatrolRoute patrolRoute;

		[SerializeField]
		private float diamondLength;

		[SerializeField]
		private float diamondWidth;
	



		void Start()
		{
			
			for (int i = 0; i < groupMembers.Count; ++i)
			{
				groupMembers[i].PatrolRoute = patrolRoute;
			}

			UpdateFormation();
		}
	

        // Update the formation rank/positions
		void UpdateFormation()
		{
			
			// Get the first active index
			int leaderIndex = -1;
			int currentActiveVehicleIndex = -1;

           
            for (int i = 0; i < groupMembers.Count; ++i)
			{
				if (groupMembers[i].GameAgent.IsInVehicle)
				{
					currentActiveVehicleIndex += 1;

                    bool isLeader = leaderIndex == -1;

                    if (isLeader)
					{
						leaderIndex = i;
						groupMembers[i].SetFormationTarget(groupMembers[i].GameAgent.Vehicle, Vector3.zero);
					}
					else
					{	
						groupMembers[i].SetFormationTarget(groupMembers[leaderIndex].GameAgent.Vehicle, GetIndexedFormationOffset(currentActiveVehicleIndex));	
					}
				}
			}
		}


		/// <summary>
        /// Get the (relative to leader) formation position for a group member using its formation index.
        /// </summary>
        /// <param name="formationIndex">The formation index of the group member.</param>
        /// <returns></returns>
		public Vector3 GetIndexedFormationOffset(int formationIndex)
		{
            
			Vector3 offset = Vector3.zero;
			
			int triangleIndex = (int)(formationIndex / 3f);
			int posInTriangle = formationIndex % 3;
			if (posInTriangle != 0)
				offset.x = (diamondWidth / 2) - (posInTriangle - 1) * diamondWidth;

			if (posInTriangle != 0)
				offset.y = (triangleIndex + 0.5f) * -10;
			else
				offset.y = triangleIndex * -10;
			
			offset.z = -triangleIndex * diamondLength;
			if (posInTriangle != 0)
				offset.z -= diamondLength / 2;

			return offset;
		}


		// Update even distribution of targets
		void UpdateTargetDistribution()
		{
			
			List<ITrackable> allTargets = new List<ITrackable>();
			for (int i = 0; i < groupMembers.Count; ++i)
			{
                if (!groupMembers[i].GameAgent.IsInVehicle || !groupMembers[i].GameAgent.Vehicle.HasRadar) continue;

				allTargets.AddRange(groupMembers[i].GameAgent.Vehicle.Radar.trackedTargets);
			}

			int nextToAssign = 0;
			for (int i = 0; i < allTargets.Count; ++i)
			{
				if (allTargets[i].Team != Team.Neutral && allTargets[i].Team != groupMembers[nextToAssign].GameAgent.Team)
				{
					if (!groupMembers[nextToAssign].GameAgent.IsInVehicle || !groupMembers[nextToAssign].GameAgent.Vehicle.HasRadar
                        || groupMembers[nextToAssign].GameAgent == GameAgentManager.Instance.FocusedGameAgent) continue;

					groupMembers[nextToAssign].GameAgent.Vehicle.Radar.SetSelectedTarget(allTargets[i]);
					nextToAssign += 1;
					if (nextToAssign >= groupMembers.Count)
					{
						break;
					}
				}
			}
		}


        // Called every frame
		void Update()
		{
			UpdateFormation();
			UpdateTargetDistribution();
		}
	
	}
}