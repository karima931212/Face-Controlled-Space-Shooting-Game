using UnityEngine;

public class CameraDolly : MonoBehaviour {

	public float rotationSpeed;

	// Update is called once per frame
	void Update () {
		transform.Rotate(0, rotationSpeed, 0);
	}
}
