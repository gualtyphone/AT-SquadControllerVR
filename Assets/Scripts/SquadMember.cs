using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum SquadMemberAction
{
    Attacking,
    Defending,
    Flanking,
    Retreat,
    Following,
    Using,
    Cover
}

public class SquadMember : MonoBehaviour {
    

    //Squad Recognition
    public Color squadColor;
    public int squadID;
    public bool selected = false;

    //Stats
    //??? Role ???
    float health = 100;

    //What to do needed info
    public SquadMemberAction currentAction;
    public Vector3 destination;
    public Vector3 lookAt;
    public GameObject interacting;
    public GameObject[] currentPois;
    public List<CoverSpot> coverSpots;
    public float LineOfSightMultiplier = 2.0f;

    //Others
    NavMeshAgent agent;
    MeshRenderer renderer;
    FindCoverSpots coverFinder;

    protected void OnDrawGizmosSelected()
    {

        if (!this.enabled)
            return;

        float maxValue = 0;
        foreach (CoverSpot cover in coverSpots)
        {
            maxValue = cover.cost > maxValue ? cover.cost : maxValue;
        }

        for (int i = 0; i < this.coverSpots.Count; i++)
        {
            Gizmos.color = new Color(coverSpots[i].cost/maxValue, 1.0f-(coverSpots[i].cost/maxValue), 0.0f);
            var coverPoint = this.coverSpots[i];
            Gizmos.DrawSphere(coverPoint.pos, 0.1f);
        }

    }

    // Use this for initialization
    void Start () {
        squadID = SquadMaster.Instance.getUniqueID(this);
        destination = transform.position;
        agent = GetComponent<NavMeshAgent>();
        renderer = GetComponent<MeshRenderer>();
        coverFinder = FindObjectOfType<FindCoverSpots>();
        //currentPois = new List<GameObject>();
        renderer.material.color = squadColor;
        renderer.material.SetColor("_OutlineColor", new Color(1.0f - squadColor.r, 1.0f - squadColor.g, 1.0f - squadColor.b, 1.0f));
        agent.stoppingDistance = 0.5f;
        coverSpots = new List<CoverSpot>();
        foreach(var cover in coverFinder.coverSpots)
        {
            coverSpots.Add(new CoverSpot(cover.pos));
        }
    }
	
	// Update is called once per frame
	void Update () {

        decideNextMove();

        //if (Vector3.Distance(transform.position, destination) > .1f)
        //{
        //    agent.enabled = true;
        agent.destination = destination;
        //}
        //else
        //{
        //    agent.enabled = false;
        //}


        if (selected)
        {
            renderer.material.SetColor("_OutlineColor", new Color(1.0f - squadColor.r, 1.0f - squadColor.g, 1.0f - squadColor.b, 1.0f));
        }
        else
        {
            renderer.material.SetColor("_OutlineColor", new Color(1.0f - squadColor.r, 1.0f - squadColor.g, 1.0f - squadColor.b, 0.0f));
        }
    }

    void decideAutonomousActions()
    {

    }

    void decideNextMove()
    {
        switch (currentAction)
        {
            case SquadMemberAction.Attacking:
                //set the destination
                //stay with the squad
                //find cover form the point we're attacking
                Vector3 center = SquadMaster.Instance.getCenterPoint(squadID);


                break;
            case SquadMemberAction.Cover:
                {
                    // find best coverpoint based on the list of known threats

                    foreach (var cover in coverSpots)
                    {
                        cover.cost = 0;
                    }
                    foreach (var cover in coverSpots)
                    {
                        cover.cost += Vector3.Distance(transform.position, cover.pos);
                    }
                    currentPois = GameObject.FindGameObjectsWithTag("POI");
                    foreach (var poi in currentPois)
                    {
                        foreach (var cover in coverSpots)
                        {
                            //if it's a threat
                            ThreatValue value = poi.GetComponent<ThreatValue>();
                            if (value)
                            {

                                //if no line of sigth
                                LayerMask layerMask = 1 << 9;
                                if (Physics.Raycast(poi.transform.position, (cover.pos - poi.transform.position), (cover.pos - poi.transform.position).magnitude, layerMask))
                                {
                                    //Debug.DrawRay(poi.transform.position, (cover.pos - poi.transform.position), Color.blue, 0.01f);
                                    if (Vector3.Distance(poi.transform.position, cover.pos) < value.radius)
                                    {
                                        cover.cost += value.value;
                                    }
                                }
                                else
                                {
                                    Debug.DrawRay(poi.transform.position, (cover.pos - poi.transform.position), Color.red, 0.01f);
                                    //add the threat value * LOS multiplier
                                    cover.cost += value.value * LineOfSightMultiplier;
                                }

                            }
                        }
                    }

                    foreach (var member in SquadMaster.Instance.getSquads())
                    {
                        if (member.gameObject != this.gameObject)
                        {
                            foreach (var cover in coverSpots)
                            {
                                if (Vector3.Distance(member.transform.position, cover.pos) < 1.0f)
                                {
                                    cover.cost += 100;
                                }
                            }
                        }
                    }
                    float minCost = 100000000;
                    Vector3 best = Vector3.zero;
                    foreach (var cover in coverSpots)
                    {
                        if (cover.cost < minCost)
                        {
                            best = cover.pos;
                            minCost = cover.cost;
                        }
                    }
                    NavMeshHit hit;
                    if (NavMesh.SamplePosition(best, out hit, 1.0f, NavMesh.AllAreas))
                        destination = hit.position;
                }
                break;
            case SquadMemberAction.Defending:
                // find best position near the point you're protecting based on:
                // known threats
                // distance from the point
                // other teammates
                // find best coverpoint based on the list of known threats
                {
                    foreach (var cover in coverSpots)
                    {
                        cover.cost = 0;
                    }
                    foreach (var cover in coverSpots)
                    {
                        cover.cost += Vector3.Distance(Vector3.zero, cover.pos);
                    }
                    currentPois = GameObject.FindGameObjectsWithTag("POI");
                    foreach (var poi in currentPois)
                    {
                        foreach (var cover in coverSpots)
                        {
                            //if it's a threat
                            ThreatValue value = poi.GetComponent<ThreatValue>();
                            if (value)
                            {

                                //if no line of sigth
                                LayerMask layerMask = 1 << 9;
                                if (Physics.Raycast(poi.transform.position, (cover.pos - poi.transform.position), (cover.pos - poi.transform.position).magnitude, layerMask))
                                {
                                    //Debug.DrawRay(poi.transform.position, (cover.pos - poi.transform.position), Color.blue, 0.01f);
                                    if (Vector3.Distance(poi.transform.position, cover.pos) < value.radius)
                                    {
                                        cover.cost += value.value;
                                    }
                                }
                                else
                                {
                                    Debug.DrawRay(poi.transform.position, (cover.pos - poi.transform.position), Color.red, 0.01f);
                                    //add the threat value * LOS multiplier
                                    cover.cost += value.value * LineOfSightMultiplier;
                                }

                            }
                        }
                    }

                    foreach (var member in SquadMaster.Instance.getSquads())
                    {
                        if (member.gameObject != this.gameObject)
                        {
                            foreach (var cover in coverSpots)
                            {
                                if (Vector3.Distance(member.transform.position, cover.pos) < 1.0f)
                                {
                                    cover.cost += 100;
                                }
                            }
                        }
                    }
                    float minCost = 100000000;
                    Vector3 best = Vector3.zero;
                    foreach (var cover in coverSpots)
                    {
                        if (cover.cost < minCost)
                        {
                            best = cover.pos;
                            minCost = cover.cost;
                        }
                    }
                    NavMeshHit hit;
                    if (NavMesh.SamplePosition(best, out hit, 1.0f, NavMesh.AllAreas))
                        destination = hit.position;
                    break;
                }
            case SquadMemberAction.Flanking:
                
                break;

            case SquadMemberAction.Following:
                
                break;

            case SquadMemberAction.Retreat:
                // face towards threats
                // walk towards destination
                // find cover
                break;
            case SquadMemberAction.Using:
                // Go towards the object you want to use
                // ?? ask team mates for protection if object is vulnerable ??
                break;

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
