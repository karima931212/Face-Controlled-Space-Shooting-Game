using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour {
   // [SerializeField] GameObject playbutton;
    [SerializeField] GameObject mainMenu;
    [SerializeField] GameObject gameUI;

    [SerializeField] GameObject playerPrefab;
    [SerializeField] GameObject playerStartPosition;

    bool isDisplay = true;

    void Start() {
        DelayMainMenuDisplay();
    }

    void OnEnable() {
        EventHandle.OnStartGame += ShowGameUI;
        EventHandle.OnPlayerDeath += ShowMainMenu;
    }

    void OnDisable() {
        EventHandle.OnStartGame -= ShowGameUI;
        EventHandle.OnPlayerDeath -= ShowMainMenu;
    }

    void  ShowMainMenu() {
        Invoke("DelayMainMenuDisplay", myAsteroid.destructDelay * 3);


    }
    void DelayMainMenuDisplay()
    {
        mainMenu.SetActive(true);
        gameUI.SetActive(false);
     

    }
    void ShowGameUI() {
        mainMenu.SetActive(false);
        gameUI.SetActive(true);
        Instantiate(playerPrefab, playerStartPosition.transform.position, playerStartPosition.transform.rotation);
    }
    /*
    void HidePanel() {
        isDisplay = !isDisplay;
        playbutton.SetActive(isDisplay);

    }*/

 
}
