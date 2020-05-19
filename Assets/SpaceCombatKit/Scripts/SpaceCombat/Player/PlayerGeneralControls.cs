using UnityEngine;
using System.Collections;

namespace VSX.UniversalVehicleCombat
{

	/// <summary>
    /// This class implements player input for actions that are not related to a specific vehicle.
    /// </summary>
	public class PlayerGeneralControls : MonoBehaviour 
	{
	
		private GameManager gameManager;
		private bool hasGameManager;


		void Start()
		{
			gameManager = GameObject.FindObjectOfType<GameManager>();
			hasGameManager = gameManager != null;
		}


		void Update()
		{
            if (hasGameManager)
            {
                if (Input.GetKeyDown(KeyCode.X))
                {
                    gameManager.TogglePauseMenu();
                }
            }
		}
	}
}