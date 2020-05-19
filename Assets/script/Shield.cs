using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shield : MonoBehaviour {
    [SerializeField] int maxHealth = 20;
    [SerializeField] int currentHealth;
    [SerializeField] int regenerateAmount = 1;
    [SerializeField] float regenerateRate = 2f;
	// Use this for initialization
	void Start () {
        currentHealth = maxHealth;
        InvokeRepeating("Regenerate", regenerateRate,regenerateRate);
	}

    void Regenerate() {
        if (currentHealth < maxHealth)
            currentHealth += regenerateAmount;

        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
            EventHandle.TakeDamage(currentHealth / (float)maxHealth);
        }

    }

    public void TakeDamage(int dmg = 1)
    {

        currentHealth -= dmg;
        if (currentHealth < 0)
            currentHealth = 0;

        EventHandle.TakeDamage(currentHealth / (float)maxHealth);
        if (currentHealth < 1)
        {
            EventHandle.PlayerDeath();
            GetComponent<Explosion>().BlowUp();
            Debug.Log("Died");
        }

           

    }
}
