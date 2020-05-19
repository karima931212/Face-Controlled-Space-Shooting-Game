using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(BoxCollider))]
public class pickup : MonoBehaviour {
    static int points = 100;
    [SerializeField] float rotationrange = 100.0f;
    Vector3 randomrotation;
    bool gotHit = false;
    void Start()
    {
   

        //random rotation
        randomrotation.x = Random.Range(-rotationrange, rotationrange);
        randomrotation.y = Random.Range(-rotationrange, rotationrange);
        randomrotation.z = Random.Range(-rotationrange, rotationrange);

        //Debug.Log(randomrotation);
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(randomrotation * Time.deltaTime);
    }


    void OnTriggerEnter(Collider col) {
        if (col.transform.CompareTag("Player")) {

            if (!gotHit) {
                gotHit = true;
                PickupHit();
            }
        
            
        }
       
    }

    public void PickupHit() {
        Debug.Log("Player hit");
        EventHandle.ScorePoints(points);
        EventHandle.RespondPickup();
        Destroy(gameObject);
    }
}
