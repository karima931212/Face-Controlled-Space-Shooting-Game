using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Light))]
[RequireComponent(typeof(TrailRenderer))]
public class Thruster : MonoBehaviour {
    TrailRenderer tr;
    Light thrusterlight;
	// Use this for initialization
	void Awake () {
		tr = GetComponent<TrailRenderer>();
        thrusterlight = GetComponent<Light>();
	}
    void Start() {
        /*
        thrusterlight.enabled = false;
        tr.enabled = false;*/
        thrusterlight.intensity = 0;
    }
    
    public void Activate(bool activate = true) {
        if (activate)
        {
            tr.enabled = true;
            thrusterlight.enabled = true;
        }
        else {
            tr.enabled = false;
            thrusterlight.enabled = false;
        }
    }
    

    public void Intensity(float inten)
    {
        thrusterlight.intensity = inten * 2f;

    }
}
