using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playermove : MonoBehaviour {
    [SerializeField] float moveSpeed = 2.0f;
    [SerializeField] float turnSpeed = 50.0f;
    [SerializeField] Thruster[] thruster;
    public FreeTrackClientDll FTinput;
    Transform myShip;
    public Transform faceInput;
    public float RotationSpeed = 60F;
    public float PositionSpeed = 0.001f;
    Vector3 startPos = Vector3.zero;

	private float npitch;
	private float nyaw;
	private float yaw;
	private float pitch;
	// Use this for initialization
	void Start () {
        myShip = transform;
        faceInput = GameObject.FindGameObjectWithTag("FaceInput").transform;
        FTinput = faceInput.GetComponent<FreeTrackClientDll>();
        startPos = myShip.position;
	}
	
	// Update is called once per frame
	void Update () {
        //Thrust();
        Turn();
	}


    void Turn() {
        //        float yaw = turnSpeed * Time.deltaTime * Input.GetAxis("Horizontal");
        //        float pitch = turnSpeed * Time.deltaTime * Input.GetAxis("pitch");
        //        float roll = turnSpeed* Time.deltaTime* Input.GetAxis("roll");
        //        myShip.Rotate(-pitch, yaw, -roll);

        //		 yaw = Input.GetAxis ("Mouse X");
        //		 pitch = -Input.GetAxis ("Mouse Y");

        //		 nyaw = Mathf.Lerp (nyaw, yaw, Time.deltaTime * 4);
        //		 npitch = Mathf.Lerp (npitch, pitch, Time.deltaTime * 4);
        //		myShip.Rotate (npitch, nyaw, 0);

        if (FTinput)
        {
            //myShip.position = startPos + new Vector3(-FTinput.X * PositionSpeed, FTinput.Y * PositionSpeed, -FTinput.Z * PositionSpeed);
            myShip.transform.position = myShip.transform.position + myShip.transform.forward * moveSpeed * Time.deltaTime;
            myShip.rotation = Quaternion.Euler(-FTinput.Pitch * RotationSpeed, -FTinput.Yaw * RotationSpeed, FTinput.Roll * RotationSpeed);
        }
        else return;
    }

    void Thrust() {
//        if (Input.GetAxis("Vertical") > 0) { 
//            myShip.transform.position = myShip.transform.position + myShip.transform.forward * moveSpeed * Time.deltaTime * Input.GetAxis("Vertical");
//            foreach (Thruster t in thruster)
//                t.Intensity(Input.GetAxis("Vertical"));
//        }
            
            /*
              if (Input.GetKeyDown(KeyCode.W))
                  foreach (Thruster t in thruster)
                     t.Activate();
              else if (Input.GetKeyUp(KeyCode.W))
                  foreach (Thruster t in thruster)
                     t.Activate(false);
            */

		myShip.transform.position = myShip.transform.position + myShip.transform.forward * moveSpeed * Time.deltaTime;
		foreach (Thruster t in thruster)
			t.Activate();
    }

}
