using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerInput : MonoBehaviour {
    [SerializeField] Laser[] laser;
	// Update is called once per frame
	void Update () {
//        if (Input.GetKeyDown(KeyCode.Space))
//        {
//            foreach (Laser l in laser)
//            {
//
//               // Vector3 pos = transform.position + (transform.forward * l.Distance);
//                l.FireLaser();
//
//            }
//             
//        }

		if (Input.GetMouseButton (0)) {
		
			foreach (Laser l in laser) {
				Vector3 pos = transform.position + (transform.forward * l.Distance);
				l.FireLaser ();
			}
		}
	}
}
