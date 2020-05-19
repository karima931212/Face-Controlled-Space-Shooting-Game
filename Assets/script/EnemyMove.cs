using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[DisallowMultipleComponent]
[RequireComponent(typeof(EnemyAttack))]
[RequireComponent(typeof(TrailRenderer))]

public class EnemyMove : MonoBehaviour {
    [SerializeField] Transform target;
    [SerializeField] float rotationDamp = 0.5f;
    [SerializeField] float moveDamp = 10f;
    [SerializeField] float detectionDistance = 200f;
    [SerializeField] float rayCastoffset = 2.5f;

    void OnEnable() {
        EventHandle.OnPlayerDeath += FindMainCamera;
        EventHandle.OnStartGame += SelfDestruct;
    }

    void OnDisable() {
        EventHandle.OnPlayerDeath -= FindMainCamera;
        EventHandle.OnStartGame -= SelfDestruct;
    }

    void SelfDestruct() {

        Destroy(gameObject);
    }

    void Update() {
        if (!FindTarget())
            return;

        PathFinding();
        //Turn();
        Move();
    }
    void Move() {
        transform.position = transform.position + transform.forward * moveDamp * Time.deltaTime;
    }

    void Turn() {
        Vector3 pos = target.position - transform.position;
        //turn to target
        Quaternion rotation = Quaternion.LookRotation(pos);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, rotationDamp * Time.deltaTime);
    }

    void PathFinding() {
        RaycastHit hit;
        Vector3 raycastOffset = Vector3.zero;

        Vector3 left = transform.position - transform.right * rayCastoffset;
        Vector3 right = transform.position + transform.right * rayCastoffset;
        Vector3 up = transform.position + transform.up * rayCastoffset;
        Vector3 down = transform.position - transform.up * rayCastoffset;

        Debug.DrawRay(left, transform.forward*detectionDistance, Color.cyan);
        Debug.DrawRay(right, transform.forward * detectionDistance, Color.cyan);
        Debug.DrawRay(up, transform.forward * detectionDistance, Color.cyan);
        Debug.DrawRay(down, transform.forward * detectionDistance, Color.cyan);

        if (Physics.Raycast(left, transform.forward, out hit, detectionDistance))
            raycastOffset += Vector3.right;
        else if (Physics.Raycast(right, transform.forward, out hit, detectionDistance))
            raycastOffset -= Vector3.right;
        if (Physics.Raycast(up, transform.forward, out hit, detectionDistance))
            raycastOffset -= Vector3.up;
        else if (Physics.Raycast(down, transform.forward, out hit, detectionDistance))
            raycastOffset += Vector3.up;

        if (raycastOffset != Vector3.zero)
            transform.Rotate(raycastOffset * 5f * Time.deltaTime);
        else
            Turn();

    }
        bool FindTarget() {
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

    void FindMainCamera() {
        target = GameObject.FindGameObjectWithTag("MainCamera").transform;
    }
}
