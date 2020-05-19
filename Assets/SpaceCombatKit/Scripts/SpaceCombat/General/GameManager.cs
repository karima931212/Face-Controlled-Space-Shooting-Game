using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using VSX.UniversalVehicleCombat;


namespace VSX.UniversalVehicleCombat
{

    /// <summary>
    /// An enum that describes the different states the game can be in.
    /// </summary>
	public enum GameState
    {
        Gameplay,
        Paused,
        TriggerGroupsMenu,
        PowerManagementMenu,
        ControlsMenu,
        GameOver
    }

    public delegate void OnGameStateChangedEventHandler(GameState newGameState);
	
	// This class provides a base class for game managers in any scene
    /// <summary>
    /// This class provides a base class (or example) for managing the different states that a game can be in.
    /// </summary>
	public class GameManager : MonoBehaviour 
	{
	
		[Header("Menus")]
		
		[SerializeField]
		protected GameObject pauseMenu;
	
		[SerializeField]
		protected GameObject gameOverMenu;
	
		[SerializeField]
		protected TriggerGroupsMenuController triggerGroupsMenuController;
	
		[SerializeField]
		protected PowerManagementMenuController powerManagementMenuController;
		
		[SerializeField]
		protected GameObject controlsMenu;
	
		protected GameState currentGameState = GameState.Gameplay;
		public GameState CurrentGameState {  get { return currentGameState; } }

        public OnGameStateChangedEventHandler onGameStateChangedEventHandler;




        protected virtual void Awake()
		{

			pauseMenu.SetActive(false);
			gameOverMenu.SetActive(false);

		}
	
	
		protected virtual void Start()
		{
			pauseMenu.SetActive(false);
			controlsMenu.SetActive(false);
		}



	    /// <summary>
        /// Toggle the pause menu on or off.
        /// </summary>
		public virtual void TogglePauseMenu()
		{
			
			if (currentGameState == GameState.Gameplay)
			{
				pauseMenu.SetActive(true);
				Time.timeScale = 0f;
				currentGameState = GameState.Paused;
			}
			else if (currentGameState == GameState.Paused)
			{
				pauseMenu.SetActive(false);
				Time.timeScale = 1f;
				currentGameState = GameState.Gameplay;
			}

            if (onGameStateChangedEventHandler != null) onGameStateChangedEventHandler(currentGameState);
        }
	

        /// <summary>
        /// Toggle the game over menu on or off.
        /// </summary>
		public virtual void ToggleGameOverMenu()
		{
			gameOverMenu.SetActive(true);
			Time.timeScale = 0f;
			currentGameState = GameState.GameOver;

            if (onGameStateChangedEventHandler != null) onGameStateChangedEventHandler(currentGameState);
        }
	

        /// <summary>
        /// Resume the game.
        /// </summary>
		public virtual void Resume()
        { 
			Time.timeScale = 1f;
			pauseMenu.SetActive(false);
		}
	

        /// <summary>
        /// Restart the game.
        /// </summary>
		public virtual void Restart()
        {
            Time.timeScale = 1;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
	
	
        /// <summary>
        /// Toggle the trigger groups menu on or off.
        /// </summary>
		public virtual void ToggleTriggerGroupsMenu()
		{

            if (currentGameState == GameState.Gameplay)
			{
				bool success = triggerGroupsMenuController.Activate();
				if (success)
				{
					Time.timeScale = 0f;
					currentGameState = GameState.TriggerGroupsMenu;
				}
			}
			else if (currentGameState == GameState.TriggerGroupsMenu)
			{
				triggerGroupsMenuController.Deactivate();
				Time.timeScale = 1f;
				currentGameState = GameState.Gameplay;
			}

            if (onGameStateChangedEventHandler != null) onGameStateChangedEventHandler(currentGameState);
        }
	

        /// <summary>
        /// Toggle the power management menu on or off.
        /// </summary>
		public virtual void TogglePowerManagementMenu()
		{ 
	
			if (currentGameState == GameState.Gameplay)
			{
                bool success = powerManagementMenuController.Activate();
                
                if (success)
				{
					Time.timeScale = 0f;
					currentGameState = GameState.PowerManagementMenu;
				}
			}
			else if (currentGameState == GameState.PowerManagementMenu)
			{
				powerManagementMenuController.Deactivate();
				Time.timeScale = 1f;
				currentGameState = GameState.Gameplay;
			}

            if (onGameStateChangedEventHandler != null) onGameStateChangedEventHandler(currentGameState);
        }
	

        /// <summary>
        /// Toggle the controls information menu on or off.
        /// </summary>
		public virtual void ToggleControlsMenu() 
		{ 
			if (currentGameState == GameState.Paused)
			{
                pauseMenu.SetActive(false);
                controlsMenu.SetActive(true);
                currentGameState = GameState.ControlsMenu;
			}
			else if (currentGameState == GameState.ControlsMenu)
			{
                pauseMenu.SetActive(true);
                controlsMenu.SetActive(false);
                currentGameState = GameState.Paused;
			}

            if (onGameStateChangedEventHandler != null) onGameStateChangedEventHandler(currentGameState);
        }
	}	
}
