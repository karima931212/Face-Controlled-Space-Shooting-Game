using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// This class represents a patrol route - a sequential, loopable series of patrol points that AI travels between
    /// </summary>
    public class PatrolRoute : MonoBehaviour 
	{
	
		[SerializeField]
		private List<Transform> patrolTargets = new List<Transform>();
        /// <summary>
        /// A list of all the patrol targets (nodes) along this route.
        /// </summary>
		public List<Transform> PatrolTargets { get { return patrolTargets; } }
	
	}
}
