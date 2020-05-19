using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour {
    [SerializeField] GameObject explosion;
    [SerializeField] Rigidbody rigid;
    [SerializeField] float laserHitModifier =10f;
    [SerializeField] Shield shield;
    [SerializeField] GameObject blow;

    void beHitted(Vector3 pos)
    {
        GameObject go = Instantiate(explosion, pos, Quaternion.identity, transform) as GameObject;
        Destroy(go, 6f);

        if (shield == null)
            return;

        shield.TakeDamage();
    }

    void OnCollisionEnter(Collision collision) {
        foreach (ContactPoint contact in collision.contacts)
            beHitted(contact.point);
    }

    public void AddForce(Vector3 hitPosition, Transform hitSource) {
        //Debug.LogWarning("Addforce" + gameObject.name + "-->" + hitSource.name);
        beHitted(hitPosition);
        if (rigid == null) {
            return;
        }
        Vector3 forceVector = (hitSource.position - transform.position).normalized;
        rigid.AddForceAtPosition(forceVector * laserHitModifier, hitPosition,ForceMode.Impulse);

    }

    
    public void BlowUp() {

       GameObject temp = Instantiate(blow,transform.position,Quaternion.identity) as GameObject;
        Destroy(temp, 3f);

        Destroy(gameObject);

    }
}
