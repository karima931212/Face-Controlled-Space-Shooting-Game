using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using VSX.General;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// This class visualizes the obstacle avoidance data for debugging purposes
    /// </summary>
	public class ObstacleSignalManager : MonoBehaviour 
	{
	
        [SerializeField]
		private GameObject obstacleSignalPrefab;
		
		private List<ObstacleSignal> usedObstacleSignals = new List<ObstacleSignal>();

        private AISpaceFighterControl controlScript;
		public AISpaceFighterControl ControlScript
        {
            set { controlScript = value; }
        }
	
		public static ObstacleSignalManager Instance;
	
	
	
		void Awake()
        {
			if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
		}

	
		
        // Called every frame        
		void Update()
		{
			if (controlScript != null)
			{
				ClearList();
				
				for (int i = 0; i < controlScript.ObstacleAvoidanceBehaviour.ObstacleDataList.Count; ++i)
				{
					ObstacleSignal signal = PoolManager.Instance.Get(obstacleSignalPrefab).GetComponent<ObstacleSignal>();
					usedObstacleSignals.Add(signal);
					signal.Set(controlScript.ObstacleAvoidanceBehaviour.ObstacleDataList[i].currentPos, 
								controlScript.ObstacleAvoidanceBehaviour.ObstacleDataList[i].currentAvoidanceDirection, 
								controlScript.ObstacleAvoidanceBehaviour.ObstacleDataList[i].currentRiskFactor); 

				}
			}	
		}
	

        // Clear the list of used obstacle signals
		void ClearList()
        {
			for (int i = 0; i < usedObstacleSignals.Count; ++i){
				usedObstacleSignals[i].gameObject.SetActive(false);
			}
			usedObstacleSignals.Clear();
		}
	}
}