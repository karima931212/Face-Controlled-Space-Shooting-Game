using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class Timer : MonoBehaviour {
    [SerializeField] float timePassed;
    [SerializeField] Text time;

    bool keepTime = false;

    void OnEnable() {
        EventHandle.OnStartGame += StartTimer;
        EventHandle.OnPlayerDeath += StartTimer;
    }

    void OnDisable() {
        EventHandle.OnStartGame -= StopTimer;
        EventHandle.OnPlayerDeath -= StopTimer;
    }

    void Update() {
        if (keepTime) {
            timePassed += Time.deltaTime;
            UpdateTimerDisplay();
        }
    }

    void StartTimer() {
        timePassed = 0;
        keepTime = true;
    }

    void StopTimer() {
        keepTime = false;
    }

    void UpdateTimerDisplay() {
        int minutes;
        float seconds;

        minutes = Mathf.FloorToInt(timePassed / 60);
        seconds = timePassed % 60;
        time.text = string.Format("{0}:{1:00.00}", minutes, seconds);
       // Debug.Log(minutes);
        //Debug.Log(seconds);

        
    }
}
