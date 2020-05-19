using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

namespace VSX.UniversalVehicleCombat
{
	/// <summary>
    /// This class controls the menu in the MainMenu scene.
    /// </summary>
	public class MainMenuController : MonoBehaviour 
	{
	
        /// <summary>
        /// Event called when player presses the button to start the demo.
        /// </summary>
		public void OnStartMainDemo()
		{
			SceneManager.LoadScene("Loadout");
		}
	}
}
