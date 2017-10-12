using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour {

    [Range(0.0f, 100.0f)]
    public float health = 100.0f;
    [Range(0.0f, 100.0f)]
    public float damageValue = 1.0f;

    public Transform gunPoint;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void Damage(float damageValue)
    {
        health -= damageValue;
        if (health <= 0.0f)
        {
            Destroy(this.gameObject);
        }
    }

    public void Shoot(Vector3 direction)
    {
        //play sound
        //muzzle particle
        //line bullet thingy
        RaycastHit hitInfo;
        if(Physics.Raycast(gunPoint.transform.position, direction, out hitInfo))
        {
            var character = hitInfo.collider.gameObject.GetComponent<Character>();
            if (character)
            {
                character.Damage(damageValue);
            }
        }
    }
}
