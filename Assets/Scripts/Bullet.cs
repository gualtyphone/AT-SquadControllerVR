using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour {

    public GameObject ImpactParticles;

	void OnCollisionEnter(Collision col)
    {
        if (col.other.GetComponent<Character>())
        {
            
            col.other.GetComponent<Character>().Damage(4.0f);
            
        }
        var part = Instantiate(ImpactParticles);
        part.transform.position = col.contacts[0].point;
        part.transform.LookAt(col.contacts[0].point + col.contacts[0].normal);

        Destroy(this.gameObject);
    }
}
