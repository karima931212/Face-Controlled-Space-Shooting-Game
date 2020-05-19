using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class Score : MonoBehaviour {
    [SerializeField] int score;
    [SerializeField] int Highscore;
    [SerializeField] Text HighscoreText;
    [SerializeField] Text scoreText;

  
    void OnEnable() {
        EventHandle.OnStartGame +=ResetScore;
        EventHandle.OnStartGame += LoadHighScore;
        EventHandle.OnPlayerDeath += CheckNewHighScore;
        EventHandle.OnScorePoints += AddScore;
    }

    void OnDisable() {
        EventHandle.OnStartGame -= ResetScore;
        EventHandle.OnStartGame -= LoadHighScore;
        EventHandle.OnPlayerDeath -=  CheckNewHighScore;
        EventHandle.OnScorePoints -= AddScore;
    }

    void ResetScore() {
        score = 0;
        DisplayScore();
    }

    void AddScore(int amt) {
        
        score += amt;
    }
    void LoadHighScore() {
        Highscore = PlayerPrefs.GetInt("Highscore", 0);
        DisplayHighScore();
    }

    void CheckNewHighScore() {
        if (score > Highscore)
        {
            PlayerPrefs.SetInt("Highscore", score);
            DisplayHighScore();
        }
    }

    void DisplayHighScore() {

        HighscoreText.text= Highscore.ToString();
    }

    void DisplayScore()
    {
        Debug.Log("take score" + score);
        scoreText.text = score.ToString();
    }

}
