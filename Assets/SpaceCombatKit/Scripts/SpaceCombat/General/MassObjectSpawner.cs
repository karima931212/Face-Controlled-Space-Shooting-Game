using UnityEngine;
using System.Collections;

namespace VSX.UniversalVehicleCombat
{

	/// <summary>
    /// This class spawns a group of objects in the scene (e.g. asteroids).
    /// </summary>
	public class MassObjectSpawner : MonoBehaviour 
	{
	
		Transform player;

		[SerializeField]
		private GameObject prefab;
	
		[SerializeField]
		private int numX;

		[SerializeField]
		private int numY;

		[SerializeField]
		private int numZ;

		[SerializeField]
		private Vector3 center;
		
		[SerializeField]
		private float spacingX;

		[SerializeField]
		private float spacingY;

		[SerializeField]
		private float spacingZ;
		
		
		[SerializeField]
		private float minRandomOffset;

		[SerializeField]
		private float maxRandomOffset;
	
		[SerializeField]
		private float minRandomScale;

		[SerializeField]
		private float maxRandomScale;
	
		[SerializeField]
		private float maxDistFromCenter;

		[SerializeField]
		private float maxDistMargin;
	
	

		// Use this for initialization
		void Start () 
		{
            CreateObjects();	
		}
	
	    
        /// <summary>
        /// Create the objects in the scene
        /// </summary>
		void CreateObjects()
		{
			for (int i = 0; i < numX; ++i)
			{
				for (int j = 0; j < numY; ++j)
				{
					for (int k = 0; k < numZ; ++k)
					{
	
						Vector3 spawnPos = Vector3.zero;
						
                        // Get a random offset for the position
						Vector3 offsetVector = Random.Range(minRandomOffset, maxRandomOffset) * Random.insideUnitSphere;
						
                        // Calculate the spawn position
						spawnPos.x = center.x - ((numX - 1) * spacingX) / 2 + (i * spacingX);
						spawnPos.y = center.y - ((numY - 1) * spacingY) / 2 + (j * spacingY);
						spawnPos.z = center.z - ((numZ - 1) * spacingZ) / 2 + (k * spacingZ);
	
						spawnPos += offsetVector;
	
                        // Spawn objects within a radius from the center, pulling in those objects that are close to the boundary
						float distFromCenter = Vector3.Distance(spawnPos, center);
						if (distFromCenter > maxDistFromCenter)
						{
							if (distFromCenter - maxDistFromCenter < maxDistMargin)
							{
								spawnPos = center + (spawnPos - center).normalized * maxDistFromCenter;
							}
							else
							{
								continue;
							}
						}
						
                        // Calculate a random rotation
						Quaternion spawnRot = Quaternion.Euler (Random.Range (0, 360), Random.Range (0, 360),
						                                        Random.Range (0, 360));
						
                        // Create the object
						GameObject temp = (GameObject)Instantiate(prefab, spawnPos, spawnRot, transform);
						
                        // Random scale
						float scale = Random.Range (minRandomScale, maxRandomScale);
						temp.transform.localScale = new Vector3(scale, scale, scale);

					}
				}
			}
		}
	
	}
}
