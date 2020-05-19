using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class followCam : MonoBehaviour {
    [SerializeField] Transform target;
    [SerializeField] Vector3 defaultDistance = new Vector3(0f, 2f, -5f);
    [SerializeField] float distanceDamp = 10f;
    [SerializeField] float rotationDamp = 20f;
    public Vector3 velocity = Vector3.one;
	// Use this for initialization

    // Lateupdate after update
    void LateUpdate() {
        if (!FindTarget())
            return;

        SmoothFollow();
        /*
        Vector3 toPos = target.position +(target.rotation * defaultDistance);
        Vector3 curPos = Vector3.Lerp(transform.position, toPos, distanceDamp * Time.deltaTime);
        transform.position = curPos;

        Quaternion toRot = Quaternion.LookRotation(target.position - transform.position, target.up);
        Quaternion curRot = Quaternion.Slerp(transform.rotation, toRot, rotationDamp * Time.deltaTime);
        transform.rotation = curRot;*/
	}


    void SmoothFollow() {
        Vector3 toPos = target.position + (target.rotation * defaultDistance);
        Vector3 curPos = Vector3.SmoothDamp(transform.position, toPos,ref velocity, distanceDamp);
        transform.position = curPos;

        transform.LookAt(target, target.up);

    }

    bool FindTarget()
    {
        if (target == null)
        {
            GameObject temp = GameObject.FindGameObjectWithTag("Player");
            if (temp != null)
                target = temp.transform;
        }

        if (target == null)
            return false;

        return true;

    }
}
