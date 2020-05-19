using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(Explosion))]
public class myAsteroid : MonoBehaviour {
    [SerializeField] float minScale = 0.8f;
    [SerializeField] float maxScale = 1.2f;
    [SerializeField] float rotationrange = 50.0f;
    Vector3 randomrotation;

    public static float destructDelay = 1.0f;
    // Use this for initialization
    void Awake()
    {
        Vector3 scale = Vector3.one;
        //random scale
        scale.x = Random.Range(minScale, maxScale);
        scale.y = Random.Range(minScale, maxScale);
        scale.z = Random.Range(maxScale, maxScale);
        transform.localScale = scale;

        //random rotation
        randomrotation.x = Random.Range(-rotationrange, rotationrange);
        randomrotation.y = Random.Range(-rotationrange, rotationrange);
        randomrotation.z = Random.Range(-rotationrange, rotationrange);

        //Debug.Log(randomrotation);
    }

    // Update is called once per frame
    void Update () {
        transform.Rotate(randomrotation * Time.deltaTime);
    }

    public void SelfDestruct() {
        float timer = Random.Range(0, destructDelay);
        Invoke("GoBoom", timer);
    }

    public void GoBoom(){
        GetComponent<Explosion>().BlowUp();
    }
}
