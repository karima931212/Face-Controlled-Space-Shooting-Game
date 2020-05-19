using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(Light))]
public class Laser : MonoBehaviour {
    [SerializeField] float maxDistance = 150f;
    [SerializeField] float laserOffTime = 0.25f;
    [SerializeField] float fireDelay = 2f;

    LineRenderer lr;
    Light LaserLight;
    bool canFire;
    void Awake() {
        lr = GetComponent<LineRenderer>();
        LaserLight = GetComponent<Light>();
    }
    void Start() {
        lr.enabled = false;
        LaserLight.enabled = false;
        canFire = true;
    }
    void Update() {
        Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * maxDistance, Color.yellow);
    }

    Vector3 CastRay() {

        RaycastHit Hit;
        Vector3 fwd = transform.TransformDirection(Vector3.forward)* maxDistance;
        if (Physics.Raycast(transform.position, fwd, out Hit))
        {
            Debug.Log("we HIT: " + Hit.transform.name);
            SpawnExplosion(Hit.point, Hit.transform);
            /*
            Explosion temp = Hit.transform.GetComponent<Explosion>();
            if(temp != null)
              temp.beHitted(Hit.point);
            */
            return Hit.point;
        }
        else {
            Debug.Log("we miss");
            return transform.position + (transform.forward * maxDistance);
        }

    }

    void SpawnExplosion(Vector3 hitPosition, Transform target) {
        Explosion temp = target.GetComponent<Explosion>();
        if (temp != null)
        {
  
            temp.AddForce(hitPosition, transform);
        }
            


    }

    //for Player 
    public void FireLaser() {
        Vector3 pos = CastRay();
        FireLaser(pos);
    }

    //for Enemy
    public void FireLaser(Vector3 targetPosition, Transform target = null) {
        if (canFire)
        {
            if(target != null)
            {
                SpawnExplosion(targetPosition, target);
            }
                

            lr.SetPosition(0, transform.position);
            lr.SetPosition(1, targetPosition);
            lr.enabled = true;
            LaserLight.enabled = true;
            canFire = false;
            Invoke("TurnOffLaser", laserOffTime);
            Invoke("CanFire", fireDelay);
        }
    }
   

    void TurnOffLaser() {
        lr.enabled = false;
        LaserLight.enabled = false;
    }

    public float Distance {
        get { return maxDistance; }
           
    }

    void CanFire() {
        canFire = true;
    }


}
