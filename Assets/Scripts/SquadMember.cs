using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SquadMember : MonoBehaviour {

    public Transform destination;
    public NavMeshAgent agent;

	// Use this for initialization
	void Start () {
        destination = null;
        agent = GetComponent<NavMeshAgent>();
	}
	
	// Update is called once per frame
	void Update () {
		if (destination != null)
        {
            agent.destination = destination.position;
        }
	}
}
