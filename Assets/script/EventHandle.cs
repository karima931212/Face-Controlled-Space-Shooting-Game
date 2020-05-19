using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventHandle : MonoBehaviour {
    public delegate void StartGameDelegate();
    public static StartGameDelegate OnStartGame;

    public delegate void TakeDamageDelegate(float amt);
    public static TakeDamageDelegate OnTakeDamage;

    public delegate void ScorePointsDelegate(int amt);
    public static ScorePointsDelegate OnScorePoints;

    public static StartGameDelegate OnPlayerDeath;
    public static StartGameDelegate OnRespondPickup;

    public static void StartGame() {
    //    Debug.Log("START GAME");
        if (OnStartGame != null)
            OnStartGame();
    }

    public static void RespondPickup()
    {
      
        if (OnRespondPickup != null)
            OnRespondPickup();
    }

    public static void TakeDamage(float percent)
    {
      //  Debug.Log("take damage"+percent );
        if (OnTakeDamage != null)
            OnTakeDamage(percent);
    }

    public static void ScorePoints(int num)
    {
        //Debug.Log("take score" + num);
        if (OnScorePoints != null)
            OnScorePoints(num);
    }


    public static void PlayerDeath() {
   //     Debug.Log("Player Died");
        if (OnPlayerDeath != null)
            OnPlayerDeath();
    }
}
