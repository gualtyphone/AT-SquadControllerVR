using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour {
    float timer = 0.0f;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        timer += Time.deltaTime;
        foreach (SquadMember member in SquadMaster.Instance.getSquads())
        {
            if (LookingAt.IsLookingAt(this.gameObject, member.gameObject))
            {
                if (timer > 1.0f)
                {
                    GetComponent<Character>().Shoot(member.transform.position - transform.position);
                    timer = 00.0f;
                }
            }
        }
	}
}
