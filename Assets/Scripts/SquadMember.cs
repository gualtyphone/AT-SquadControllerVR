//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.AI;

//public enum SquadMemberAction
//{
//    Attacking,
//    Defending,
//    Flanking,
//    Retreat,
//    Following,
//    Using,
//    Cover
//}

//public class SquadMember : MonoBehaviour {
    

//    //Squad Recognition
//    public Color squadColor;
//    public int squadID;
//    public bool selected = false;

//    //Stats
//    //??? Role ???
//    public float timer = 0;
//    public GameObject rotation;


//    //What to do needed info
//    public SquadMemberAction currentAction;
//    public Vector3 finalDestination;
//    public Vector3 currentDestination;
//    public Vector3 lookAt;
//    public GameObject interacting;
//    public List<CoverSpot> coverSpots;
//    public List<GameObject> currentPois;
//    public float LineOfSightMultiplier = 2.0f;
//    public float maxSightRange = 10.0f;
//    public GameObject bullet;
//    public float bulletRate;

//    //Others
//    NavMeshAgent agent;
//    MeshRenderer renderer;
//    FindCoverSpots coverFinder;
//    ThreatValue threatValue;
//    GameObject threat;

//    protected void OnDrawGizmosSelected()
//    {
//        if (!this.enabled)
//            return;

//        float maxValue = 0;
//        foreach (CoverSpot cover in coverSpots)
//        {
//            maxValue = cover.cost > maxValue ? cover.cost : maxValue;
//        }

//        for (int i = 0; i < this.coverSpots.Count; i++)
//        {
//            Gizmos.color = new Color(coverSpots[i].cost/maxValue, 1.0f-(coverSpots[i].cost/maxValue), 0.0f);
//            var coverPoint = this.coverSpots[i];
//            Gizmos.DrawSphere(coverPoint.pos, 0.2f);
//        }
//    }

//    // Use this for initialization
//    void Start () {
//        squadID = SquadMaster.Instance.getUniqueID(this);
//        currentDestination = transform.position;
//        agent = GetComponent<NavMeshAgent>();
//        renderer = GetComponent<MeshRenderer>();
//        coverFinder = FindObjectOfType<FindCoverSpots>();
//        threatValue = GetComponent<ThreatValue>();
//        //currentPois = new List<GameObject>();
//        renderer.material.color = squadColor;
//        renderer.material.SetColor("_OutlineColor", new Color(1.0f - squadColor.r, 1.0f - squadColor.g, 1.0f - squadColor.b, 1.0f));
//        agent.stoppingDistance = 0.5f;
//        coverSpots = new List<CoverSpot>();
//        foreach(var cover in coverFinder.coverSpots)
//        {
//            coverSpots.Add(new CoverSpot(cover.pos));
//        }
//    }
	
//	// Update is called once per frame
//	void Update () {
//        threat = null;
//        seePointsOfInterest();
//        sharePOIWithTeamMates();

//        decideNextMove();
//        agent.destination = currentDestination;
       
//        timer += Time.deltaTime;
//        if (threat)
//        {
//            rotation.transform.LookAt(threat.transform);
//            if (timer > bulletRate)
//            {
//                GameObject bulletClone = Instantiate(bullet, bullet.transform.position, bullet.transform.rotation) as GameObject;
//                bulletClone.SetActive(true);
//                Rigidbody rb = bulletClone.GetComponent<Rigidbody>();
//                rb.AddForce(-bullet.transform.forward * 100.0f);
//                Destroy(bulletClone, 1.0f);
//                timer = 0.0f;
//            }
//        }


//        if (selected)
//        {
//            renderer.material.SetColor("_OutlineColor", new Color(1.0f - squadColor.r, 1.0f - squadColor.g, 1.0f - squadColor.b, 1.0f));
//        }
//        else
//        {
//            renderer.material.SetColor("_OutlineColor", new Color(1.0f - squadColor.r, 1.0f - squadColor.g, 1.0f - squadColor.b, 0.0f));
//        }
//    }

//    void decideAutonomousActions()
//    {

//    }

//    void decideNextMove()
//    {
//        //Reset Covers
//        foreach (var cover in coverSpots)
//        {
//            cover.cost = 0;
//        }

//        switch (currentAction)
//        {
//            case SquadMemberAction.Attacking:
//                {
//                    threat = findClosestVisibleKnownThreat();
//                    Vector3 squadCenter = SquadMaster.Instance.getCenterPoint(squadID);
//                    float squadRadius = 5.0f; //to be changed based on squad members
//                    Vector3 attackDirection = interacting.transform.position - squadCenter;

//                    // calculate threat value
//                    float threatOnSelf = 0.0f;
//                    foreach (var poi in currentPois)
//                    {
//                        if (poi)
//                        {
//                            ThreatValue value = poi.GetComponent<ThreatValue>();
//                            //if no line of sigth
//                            LayerMask layerMask = 1 << 9;
//                            if (Physics.Raycast(poi.transform.position, (transform.position
//                                - poi.transform.position), (transform.position - poi.transform.position).magnitude, layerMask))
//                            {
//                                if (Vector3.Distance(poi.transform.position, transform.position) < value.radius)
//                                {
//                                    threatOnSelf += value.value;
//                                }
//                            }
//                            else
//                            {
//                                //add the threat value * LOS multiplier
//                                threatOnSelf += value.value * LineOfSightMultiplier;
//                            }
//                        }
//                    }
//                    // if no threat 
//                    if (threatOnSelf == 0.0f)
//                    {
//                        //stay with squad
//                        //formations
//                        //move towards the point
//                        currentDestination = interacting.transform.position;
//                    }
//                    else
//                    {
//                        //find cover from threat near squad
//                        //Find best cover
//                        increaseCoverCostOnDistance(transform.position);
//                        increaseCoverCostOnDistance(squadCenter);
//                        evaluatePointsOfInterest();
//                        //evaluateDistanceFromSquadMembers();
//                        removeOccupiedSpots();
//                        Vector3 best = getBestCoverSpot();
//                        if (Vector3.Distance(squadCenter, best) < squadRadius * 2)
//                        {
//                            NavMeshHit hit;
//                            if (NavMesh.SamplePosition(best, out hit, 1.0f, NavMesh.AllAreas))
//                                currentDestination = hit.position;
//                        }
//                        else
//                        {
//                            //if no cover near squad
//                            // find closest point in squad radius and go there;
//                            float distFromCenter = Vector3.Distance(transform.position, squadCenter);
//                            if (distFromCenter < squadRadius)
//                            {
//                                currentDestination = transform.position;
//                            }
//                            else
//                            {
//                                currentDestination = ((squadCenter - transform.position).normalized) * (distFromCenter - squadRadius);
//                            }
//                            //formations
                           
//                        }
//                        //attack threat
//                    }

//                    //if close to point && no threat at point
//                    //defend point
//                    if (Vector3.Distance(transform.position, interacting.transform.position) < squadRadius)
//                    {
//                        float threatValueAtAttackSpot = 0.0f;
//                        Vector3 threatDirection = Vector3.zero;
//                        foreach (var poi in FindObjectsOfType<ThreatValue>())
//                        {
//                            LayerMask layerMask = 1 << 9;
//                            if (Physics.Raycast(poi.transform.position, (interacting.transform.position
//                                - poi.transform.position), (interacting.transform.position - poi.transform.position).magnitude, layerMask))
//                            {
//                                if (Vector3.Distance(poi.transform.position, interacting.transform.position) < poi.radius)
//                                {
//                                    threatValueAtAttackSpot += poi.value;
//                                    threatDirection += poi.transform.position * poi.value;
//                                }
//                            }
//                            else
//                            {
//                                //add the threat value * LOS multiplier
//                                threatValueAtAttackSpot += poi.value * LineOfSightMultiplier;
//                                threatDirection += poi.transform.position * (poi.value * LineOfSightMultiplier);
//                            }
//                        }
//                        threatDirection.Normalize();
//                        if (threatValueAtAttackSpot == 0.0f)
//                        {
//                            currentAction = SquadMemberAction.Defending;
//                        }
//                    }
//                }
//                break;
//            case SquadMemberAction.Cover:
//                {
//                    //Find best cover
//                    increaseCoverCostOnDistance(transform.position);
//                    evaluatePointsOfInterest();
//                    evaluateDistanceFromSquadMembers();
//                    Vector3 best = getBestCoverSpot();
//                    NavMeshHit hit;
//                    if (NavMesh.SamplePosition(best, out hit, 1.0f, NavMesh.AllAreas))
//                        currentDestination = hit.position;
//                }
//                break;
//            case SquadMemberAction.Defending:
//                {
//                    threat = findClosestVisibleKnownThreat();
//                    //Follow and assemble to defend the objective
//                    int count = 0;
//                    Vector3 point = Vector3.zero;
//                    foreach (var squadMember in SquadMaster.Instance.getSquad(squadID))
//                    {
//                        if (squadMember != this)
//                        {
//                            count++;
//                            if (squadMember)
//                            {
//                                point += squadMember.transform.position;
//                            }
//                        }
//                    }
//                    if (count > 0)
//                    {
//                        point /= count;
//                    }
//                    Vector3 center = point;
//                    Vector3 bestDirFromDefenseSpot = interacting.transform.position - (center);
//                    bestDirFromDefenseSpot.y = 0.0f;
//                    if (bestDirFromDefenseSpot.magnitude < 4.0f)
//                    {
//                        bestDirFromDefenseSpot.Normalize();
//                        bestDirFromDefenseSpot *= 4.0f;
//                    }
//                    Vector3 bestPos = interacting.transform.position + bestDirFromDefenseSpot;
//                    Debug.DrawLine(bestPos, transform.position);

//                    increaseCoverCostOnDistance(interacting.transform.position);
//                    increaseCoverCostOnDistance(bestPos);
//                    evaluatePointsOfInterest();
//                    evaluateDistanceFromSquadMembers();

//                    float threatValueAtDefendSpot = 0.0f;
//                    Vector3 threatDirection = Vector3.zero;
//                    foreach (var poi in currentPois)
//                    {
//                        if (poi)
//                        {
//                            ThreatValue value = poi.GetComponent<ThreatValue>();
//                            //if no line of sigth
//                            LayerMask layerMask = 1 << 9;
//                            if (Physics.Raycast(poi.transform.position, (interacting.transform.position
//                                - poi.transform.position), (interacting.transform.position - poi.transform.position).magnitude, layerMask))
//                            {
//                                if (Vector3.Distance(poi.transform.position, interacting.transform.position) < value.radius)
//                                {
//                                    threatValueAtDefendSpot += value.value;
//                                    threatDirection += value.transform.position * value.value;
//                                }
//                            }
//                            else
//                            {
//                                //add the threat value * LOS multiplier
//                                threatValueAtDefendSpot += value.value * LineOfSightMultiplier;
//                                threatDirection += value.transform.position * (value.value * LineOfSightMultiplier);
//                            }
//                        }
//                    }
//                    threatDirection.Normalize();

//                    foreach (var cover in coverSpots)
//                    {
//                        Vector3 directionFromCenter = cover.pos - interacting.transform.position;
//                        if (Vector3.Dot(directionFromCenter, threatDirection) > 0.0f)
//                        {
//                            cover.cost += threatValueAtDefendSpot;
//                        }
//                        else
//                        {
//                            cover.cost -= threatValueAtDefendSpot;
//                        }
                        
//                    }

//                    Vector3 bestCover = getBestCoverSpot();

//                    //
//                    if (Vector3.Distance(bestCover, bestPos) < 4.0f)
//                    {
//                        bestPos = bestCover;
//                    }


//                    NavMeshHit hit;
//                    if (NavMesh.SamplePosition(bestPos, out hit, Mathf.Infinity, NavMesh.AllAreas))
//                        currentDestination = hit.position;


//                }
//                break;
//            case SquadMemberAction.Flanking:
                
//                break;
//            case SquadMemberAction.Following:
                
//                break;
//            case SquadMemberAction.Retreat:
//                // face towards threats
//                // walk towards destination
//                // find cover
//                break;
//            case SquadMemberAction.Using:
//                // Go towards the object you want to use
//                // ?? ask team mates for protection if object is vulnerable ??
//                break;
//        }
//    }

//    //POIs
//    void seePointsOfInterest()
//    {
//        ThreatValue[] possiblePois = FindObjectsOfType<ThreatValue>();
//        foreach (var poi in possiblePois)
//        {
//            if (!currentPois.Contains(poi.gameObject))
//            {
//                if (LookingAt.IsLookingAt(gameObject, poi.gameObject))
//                {
//                    LayerMask layerMask = 1 << 9;
//                    if (!Physics.Raycast(poi.transform.position, (transform.position - poi.transform.position), 
//                        Mathf.Max((transform.position - poi.transform.position).magnitude, maxSightRange), layerMask))
//                    {
//                        //There is line of sight
//                        currentPois.Add(poi.gameObject);
//                    }
//                }
//            }
//        }
//    }
//    void sharePOIWithTeamMates()
//    {
//        foreach (var teamMate in SquadMaster.Instance.getSquad(squadID))
//        {
//            if (teamMate != this)
//            {
//                foreach(var poi in teamMate.currentPois)
//                {
//                    if (!currentPois.Contains(poi))
//                    {
//                        currentPois.Add(poi);
//                    }
//                }
//            }
//        }
//    }

//    //Options
//    private Vector3 getBestCoverSpot()
//    {
//        float minCost = 100000000;
//        Vector3 best = Vector3.zero;
//        foreach (var cover in coverSpots)
//        {
//            if (cover.cost < minCost)
//            {
//                best = cover.pos;
//                minCost = cover.cost;
//            }
//        }
//        return best;$
//    }
//    private void evaluateDistanceFromSquadMembers()
//    {
//        foreach (var member in SquadMaster.Instance.getSquads())
//        {
//            if (member.gameObject != this.gameObject)
//            {
//                foreach (var cover in coverSpots)
//                {
//                    cover.cost += 100.0f / Vector3.Distance(member.transform.position, cover.pos);
//                    cover.cost += 100.0f / Vector3.Distance(member.currentDestination, cover.pos);
//                }
//            }
//        }
//    }
//    private void removeOccupiedSpots()
//    {
//        foreach (var member in SquadMaster.Instance.getSquads())
//        {
//            if (member.gameObject != this.gameObject)
//            {
//                foreach (var cover in coverSpots)
//                {
//                    if (Vector3.Distance(member.transform.position, cover.pos) < 2.0f)
//                    {
//                        cover.cost += 100.0f;
//                    }
//                }
//            }
//        }
//    }
//    private void evaluatePointsOfInterest()
//    {
//        //currentPois = GameObject.FindGameObjectsWithTag("POI");
//        foreach (var poi in currentPois)
//        {
//            foreach (var cover in coverSpots)
//            {
//                //if it's a threat
//                if (poi)
//                {
//                    ThreatValue value = poi.GetComponent<ThreatValue>();
//                    if (value)
//                    {

//                        //if no line of sigth
//                        LayerMask layerMask = 1 << 9;
//                        if (Physics.Raycast(poi.transform.position, (cover.pos - poi.transform.position), (cover.pos - poi.transform.position).magnitude, layerMask))
//                        {
//                            //Debug.DrawRay(poi.transform.position, (cover.pos - poi.transform.position), Color.blue, 0.01f);
//                            if (Vector3.Distance(poi.transform.position, cover.pos) < value.radius)
//                            {
//                                cover.cost += value.value;
//                            }
//                        }
//                        else
//                        {
//                            //Debug.DrawRay(poi.transform.position, (cover.pos - poi.transform.position), Color.red, 0.01f);
//                            //add the threat value * LOS multiplier
//                            cover.cost += value.value * LineOfSightMultiplier;
//                        }

//                    }
//                }
//            }
//        }
//    }
//    private void increaseCoverCostOnDistance(Vector3 pos)
//    {
//        foreach (var cover in coverSpots)
//        {
//            cover.cost += 10.0f * Vector3.Distance(pos, cover.pos);
//        }
//    }
//    private void increaseCoverCostOnDirection(Vector3 pos, Vector3 direction)
//    {
//        foreach (var cover in coverSpots)
//        {
//            if (LookingAt.IsLookingAt(pos, direction, cover.pos, 0.5f))
//            { 
//                cover.cost -= 10.0f * Vector3.Distance(pos, cover.pos);
//                //Debug.DrawRay(cover.pos, direction, Color.blue);
//            }
//            else
//            {
//                cover.cost += 10.0f * Vector3.Distance(pos, cover.pos);
                
//            }
//        }
//    }

//    private GameObject findClosestVisibleKnownThreat()
//    {
//        GameObject closestVisibleKnownThreat = null;
//        float minDist = 100000.0f;
//        foreach (var threat in currentPois)
//        {
//            if (threat)
//            {
//                LayerMask layerMask = 1 << 9;
//                if (!Physics.Raycast(transform.position, threat.transform.position, (transform.position - threat.transform.position).magnitude, layerMask))
//                {
//                    float dist = Vector3.Distance(transform.position, threat.transform.position);
//                    if (dist < minDist)
//                    {
//                        minDist = dist;
//                        closestVisibleKnownThreat = threat;
//                    }
//                }
//            }
//        }

//        return closestVisibleKnownThreat;
//    }


//}
