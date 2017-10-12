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


    //What to do needed info
    public SquadMemberAction currentAction;
    public Vector3 finalDestination;
    public Vector3 currentDestination;
    public Vector3 lookAt;
    public GameObject interacting;
    public List<CoverSpot> coverSpots;
    public List<GameObject> currentPois;
    public float LineOfSightMultiplier = 2.0f;
    public float maxSightRange = 10.0f;
    
    //Others
    NavMeshAgent agent;
    MeshRenderer renderer;
    FindCoverSpots coverFinder;
    ThreatValue threatValue;

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
        currentDestination = transform.position;
        agent = GetComponent<NavMeshAgent>();
        renderer = GetComponent<MeshRenderer>();
        coverFinder = FindObjectOfType<FindCoverSpots>();
        threatValue = GetComponent<ThreatValue>();
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

        seePointsOfInterest();
        sharePOIWithTeamMates();

        decideNextMove();

        //if (Vector3.Distance(transform.position, destination) > .1f)
        //{
        //    agent.enabled = true;
        agent.destination = currentDestination;
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
        //Reset Covers
        foreach (var cover in coverSpots)
        {
            cover.cost = 0;
        }

        switch (currentAction)
        {
            case SquadMemberAction.Attacking:
                {
                    //set the destination
                    //stay with the squad
                    //find cover form the point we're attacking
                    Vector3 center = SquadMaster.Instance.getCenterPoint(squadID);
                }
                break;
            case SquadMemberAction.Cover:
                {
                    //Add based on distance from self
                    foreach (var cover in coverSpots)
                    {
                        cover.cost += Vector3.Distance(transform.position, cover.pos);
                    }

                    evaluatePointsOfInterest();
                    removeOccupiedCoverSpots();
                    Vector3 best = getBestCoverSpot();

                    NavMeshHit hit;
                    if (NavMesh.SamplePosition(best, out hit, 1.0f, NavMesh.AllAreas))
                        currentDestination = hit.position;
                }
                break;
            case SquadMemberAction.Defending:
                {
                    //Add based on distance from defendingPoint
                    int count = 0;
                    Vector3 point = Vector3.zero;
                    foreach (var squadMember in SquadMaster.Instance.getSquad(squadID))
                    {
                        if (squadMember != this)
                        {
                            count++;
                            point += squadMember.transform.position;
                        }
                    }
                    if (count > 0)
                    {
                        point /= count;
                    }
                    Vector3 center = point;
                    Vector3 bestDirFromDefenseSpot = interacting.transform.position - (center);
                    bestDirFromDefenseSpot.y = 0.0f;
                    if (bestDirFromDefenseSpot.magnitude < 4.0f)
                    {
                        bestDirFromDefenseSpot.Normalize();
                        bestDirFromDefenseSpot *= 4.0f;
                    }
                    Vector3 bestPos = interacting.transform.position + bestDirFromDefenseSpot;
                    Debug.DrawLine(bestPos, transform.position);


                    foreach (var cover in coverSpots)
                    {
                        cover.cost += 10.0f * Vector3.Distance(interacting.transform.position, cover.pos);
                    }
                    foreach (var cover in coverSpots)
                    {
                        cover.cost += 10.0f * Vector3.Distance(bestPos, cover.pos);
                    }

                    evaluatePointsOfInterest();
                    removeOccupiedCoverSpots();
                    //findOptimalDefendingPosition
                    //dist from center of squad
                    

                    float threatValueAtDefendSpot = 0.0f;
                    Vector3 threatDirection = Vector3.zero;
                    foreach (var poi in currentPois)
                    {
                        ThreatValue value = poi.GetComponent<ThreatValue>();
                        //if no line of sigth
                        LayerMask layerMask = 1 << 9;
                        if (Physics.Raycast(poi.transform.position, (interacting.transform.position
                            - poi.transform.position), (interacting.transform.position - poi.transform.position).magnitude, layerMask))
                        {
                            if (Vector3.Distance(poi.transform.position, interacting.transform.position) < value.radius)
                            {
                                threatValueAtDefendSpot += value.value;
                                threatDirection += value.transform.position * value.value;
                            }
                        }
                        else
                        {
                            //add the threat value * LOS multiplier
                            threatValueAtDefendSpot += value.value * LineOfSightMultiplier;
                            threatDirection += value.transform.position * (value.value * LineOfSightMultiplier);
                        }
                    }
                    threatDirection.Normalize();

                    foreach (var cover in coverSpots)
                    {
                        Vector3 directionFromCenter = cover.pos - interacting.transform.position;
                        if (Vector3.Dot(directionFromCenter, threatDirection) > 0.0f)
                        {
                            cover.cost += threatValueAtDefendSpot;
                        }
                        else
                        {
                            cover.cost -= threatValueAtDefendSpot;
                        }
                        
                    }

                    Vector3 bestCover = getBestCoverSpot();


                    if (Vector3.Distance(bestCover, bestPos) < 4.0f)
                    {
                        bestPos = bestCover;
                    }


                    NavMeshHit hit;
                    if (NavMesh.SamplePosition(bestPos, out hit, 1.0f, NavMesh.AllAreas))
                        currentDestination = hit.position;


                }
                break;
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

    //POIs
    void seePointsOfInterest()
    {
        ThreatValue[] possiblePois = FindObjectsOfType<ThreatValue>();
        foreach (var poi in possiblePois)
        {
            if (!currentPois.Contains(poi.gameObject))
            {
                if (LookingAt.IsLookingAt(gameObject, poi.gameObject))
                {
                    LayerMask layerMask = 1 << 9;
                    if (!Physics.Raycast(poi.transform.position, (transform.position - poi.transform.position), 
                        Mathf.Max((transform.position - poi.transform.position).magnitude, maxSightRange), layerMask))
                    {
                        //There is line of sight
                        currentPois.Add(poi.gameObject);
                    }
                }
            }
        }
    }
    void sharePOIWithTeamMates()
    {
        foreach (var teamMate in SquadMaster.Instance.getSquad(squadID))
        {
            if (teamMate != this)
            {
                foreach(var poi in teamMate.currentPois)
                {
                    if (!currentPois.Contains(poi))
                    {
                        currentPois.Add(poi);
                    }
                }
            }
        }
    }

    //Options
    private Vector3 getBestCoverSpot()
    {
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
        return best;
    }
    private void removeOccupiedCoverSpots()
    {
        foreach (var member in SquadMaster.Instance.getSquads())
        {
            if (member.gameObject != this.gameObject)
            {
                foreach (var cover in coverSpots)
                {
                    cover.cost += 100.0f / Vector3.Distance(member.transform.position, cover.pos);
                }
            }
        }
    }
    private void evaluatePointsOfInterest()
    {
        //currentPois = GameObject.FindGameObjectsWithTag("POI");
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
                        //Debug.DrawRay(poi.transform.position, (cover.pos - poi.transform.position), Color.red, 0.01f);
                        //add the threat value * LOS multiplier
                        cover.cost += value.value * LineOfSightMultiplier;
                    }

                }
            }
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
