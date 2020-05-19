using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.Generic;

public class AsteroidManager : MonoBehaviour {
    [SerializeField]myAsteroid asteroidPrefabs;
    [SerializeField]GameObject pickupPrefabs;
    [SerializeField]int numberOfAsteroidOnAnAxis = 10;
    [SerializeField] int gridSpacing = 100;

   public List<myAsteroid> asteroid = new List<myAsteroid>();
        // Use this for initialization
    //void Start () {
    //     PlaceAsteroids();
    //}
    void OnEnable() {
        EventHandle.OnStartGame += PlaceAsteroids;
        EventHandle.OnPlayerDeath += DestoryAsteroids;
        EventHandle.OnRespondPickup += PlacePickup;
    }

    void OnDisable() {
        EventHandle.OnRespondPickup -= PlacePickup;
        EventHandle.OnStartGame -= PlaceAsteroids;
        EventHandle.OnPlayerDeath -= DestoryAsteroids;
    }


    void DestoryAsteroids() {
        foreach (myAsteroid ast in asteroid)
            ast.SelfDestruct();
        asteroid.Clear();
    }

    void PlaceAsteroids()
    {
        for (int x = 0; x < numberOfAsteroidOnAnAxis; x++) {
            for (int y = 0; y < numberOfAsteroidOnAnAxis; y++) {
                for (int z = 0; z < numberOfAsteroidOnAnAxis; z++) {
                    InstaiateAsteroid(x, y, z);
                }
            }
        }

        PlacePickup();
        
    }
    void PlacePickup()
    {
        int rnd = Random.Range(0,asteroid.Count);
        Instantiate(pickupPrefabs,asteroid[rnd].transform.position,Quaternion.identity);

        Destroy(asteroid[rnd].gameObject);
        
        asteroid.RemoveAt(rnd);

    }
    void InstaiateAsteroid(int x, int y, int z) {
        myAsteroid temp = Instantiate(asteroidPrefabs,
                     new Vector3(transform.position.x + (x * gridSpacing) + AsteroidOfSet(),
                                  transform.position.y + (y * gridSpacing) + AsteroidOfSet(),
                                  transform.position.z + (z * gridSpacing) + AsteroidOfSet()),
                     Quaternion.identity,
                     transform) as myAsteroid;
        temp.name = "Asteroid" + x + " " + y + " " + z;
        asteroid.Add(temp);
        
    }
    
    float AsteroidOfSet() {
       return Random.Range(-gridSpacing / 2f, gridSpacing / 2f);
       
    }
}
