using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAttack : MonoBehaviour {
    [SerializeField] Transform target;
    [SerializeField] Laser laser;
    Vector3 hitPosition;
    void Update() {
        if (!FindTarget())
            return;

        Infront();
        HaveLineOfSight();

        if (Infront() && HaveLineOfSight()) {
            FireLaser();
        }
    }
    //angle from 90 - 270
    bool Infront() {
        Vector3 directionToTarget = transform.position - target.transform.position;
        float angle = Vector3.Angle(transform.forward, directionToTarget);

        if (Mathf.Abs(angle) > 90 && Mathf.Abs(angle) < 270)
        {
          //  Debug.DrawLine(transform.position, target.position, Color.green);
            return true;
        }
        else {
         //  Debug.DrawLine(transform.position, target.position, Color.yellow);
            return false;
        }
    }

    bool HaveLineOfSight() {
        RaycastHit hit;

        Vector3 direction = target.position - transform.position;


        if (Physics.Raycast(laser.transform.position, direction, out hit, laser.Distance)) {
            //Debug.DrawLine(laser.transform.position, hit.point);
            //Debug.Log(hit.transform.tag);
            if (hit.transform.CompareTag("Player")) {
                Debug.DrawRay(laser.transform.position, direction, Color.green);
                hitPosition = hit.transform.position;
                return true;
            }
        }
        return false;
    }

    void FireLaser() {
        laser.FireLaser(hitPosition, target);
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
