using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SquadMember : MonoBehaviour {

    public Color squadColor;
    public int squadID;

    public Vector3 destination;
    NavMeshAgent agent;

    public bool selected = false;
    MeshRenderer renderer;

    // Use this for initialization
    void Start () {
        squadID = SquadMaster.Instance.getUniqueID(this);
        destination = transform.position;
        agent = GetComponent<NavMeshAgent>();
        renderer = GetComponent<MeshRenderer>();
        renderer.material.color = squadColor;
        renderer.material.SetColor("_OutlineColor", new Color(1.0f - squadColor.r, 1.0f - squadColor.g, 1.0f - squadColor.b, 1.0f));
    }
	
	// Update is called once per frame
	void Update () {
		if (destination != null)
        {
            agent.destination = destination;
        }

        if (selected)
        {
            renderer.material.SetColor("_OutlineColor", new Color(1.0f - squadColor.r, 1.0f - squadColor.g, 1.0f - squadColor.b, 1.0f));
        }
        else
        {
            renderer.material.SetColor("_OutlineColor", new Color(1.0f - squadColor.r, 1.0f - squadColor.g, 1.0f - squadColor.b, 0.0f));
        }
    }

    public void UpdateSquad(int id, Color col)
    {
        squadID = id;
        squadColor = col;
        renderer.material.color = squadColor;
        renderer.material.SetColor("_OutlineColor", new Color(1.0f - squadColor.r, 1.0f - squadColor.g, 1.0f - squadColor.b, 1.0f));
    }
}
