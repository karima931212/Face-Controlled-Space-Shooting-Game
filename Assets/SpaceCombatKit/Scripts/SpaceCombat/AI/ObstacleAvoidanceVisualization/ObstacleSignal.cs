using UnityEngine;
using System.Collections;


namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// This class manages an obstacle signal, which is an arrow that is shown where obstacles are detected, for debugging purposes.
    /// </summary>
	public class ObstacleSignal : MonoBehaviour
    {
	
		private Material mat;
	
		
		void Awake()
		{
			mat = GetComponent<MeshRenderer>().material;
		}
	
        /// <summary>
        /// Update the obstacle signal.
        /// </summary>
        /// <param name="pos">Position of the obstacle signal.</param>
        /// <param name="dir">The direction where the obstacle signal should be pointing.</param>
        /// <param name="riskFactor">The riskFactor of the obstacle.</param>
		public void Set(Vector3 pos, Vector3 dir, float riskFactor){
	
			transform.position = pos;
			transform.LookAt(pos + dir);

            Color c = mat.color;
            c.a = riskFactor;
            mat.SetColor("_TintColor", c);	
			
		}
	}
}
