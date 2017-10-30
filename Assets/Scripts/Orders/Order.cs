using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[System.Serializable]
//Returns if finished
public delegate bool Execution(NewSquadMember squadMember, Vector3 wayPoint);

[System.Serializable]
public class Order
{
    //Where
    public GameObject objectPosition;
    public Vector3 waypoint;
    //How
    public Execution execution;
    //Finished
    public bool started = false;
    public bool running = false;
    public bool finished = false;
    public bool temp = false;
    //Importance
    public int priority = 10;

    public Order()
    {
        started = false;
        finished = false;
    }

    public static IEnumerator startOrderCoroutine(Order order, NewSquadMember member)
    {
        //GameObject waypoint = Instantiate();
        while (!order.execution(member, order.waypoint))
        {
            yield return new WaitForSeconds(.1f);
            while (!order.running)
            {
                yield return new WaitForSeconds(.1f);
                if (order.finished)
                {
                    yield return null;
                }
            }
        }
        //if (order.temp)
        //{
        //    var ord = member.orders.Find(o => o != order && !o.finished && o.started && !o.running && o.temp);
        //    if (ord != null)
        //    {
        //        ord.running = true;
        //    }
        //    else
        //    {
        //        ord = member.orders.Find(o => o != order && !o.finished && o.started && !o.running);
        //        if (ord != null)
        //        {
        //            ord.running = true;
        //        }
        //    }
        //}
        order.finished = true;
        yield return null;
    }

    public Order(GameObject obj, Vector3 wp, Execution ex, int prior)
    {
        objectPosition = obj;
        waypoint = wp;
        execution = ex;
        priority = prior;
        started = false;
        finished = false;
}
    public Order(Order other)
    {
        objectPosition = other.objectPosition;
        waypoint = other.waypoint;
        execution = other.execution;
        priority = other.priority;
        started = false;
        finished = false;
    }

    public static bool Stay(NewSquadMember squadMember, Vector3 wayPoint)
    {
        squadMember.agent.destination = wayPoint;
        foreach (var ord in squadMember.orders)
        {
            if (ord.waypoint != wayPoint)
            {
                ord.finished = true;
            }
        }
        return true;
    }

    public static bool Attack(NewSquadMember squadMember, Vector3 wayPoint)
    {
        squadMember.agent.destination = wayPoint;
        Vector3 squadCenter = SquadMaster.Instance.getCenterPoint(squadMember.squadID);
        float squadRadius = 20.0f; //to be changed based on squad members
        Vector3 attackDirection = wayPoint - squadCenter;

        // calculate threat value
        float threatOnSelf = EvaluateThreatOnPoint(squadMember.transform.position, squadMember.knownThreats);
        // if no threat 
        if (threatOnSelf == 0.0f)
        {
            //stay with squad
            //formations
            //move towards the point
            squadMember.agent.destination = wayPoint;
        }
        else
        {
            //find cover from threat near squad
            //Find best cover
            var covers = CreateCoverSpotsList();
            covers = RemoveCoversOutsideRadius(covers, squadCenter, squadRadius);
            covers = IncreaseCoverCostOnDistance(covers, squadMember.transform.position, 1.0f);
            covers = IncreaseCoverCostOnDistance(covers, squadCenter, 1.0f);
            covers = EvaluateThreatsOnCovers(covers, squadMember.knownThreats);
            covers = RemoveOccupiedCovers(covers, squadMember, 1.5f);
            var best = GetBestCoverSpot(covers);
            if (best != null)
            {
                NavMeshHit hit;
                if (NavMesh.SamplePosition(best.pos, out hit, 1.0f, NavMesh.AllAreas))
                    squadMember.agent.destination = hit.position;
            }
            else
            {
                //if no cover near squad
                // find closest point in squad radius and go there;
                float distFromCenter = (squadMember.transform.position- squadCenter).sqrMagnitude;
                if (distFromCenter < squadRadius)
                {
                    squadMember.agent.destination = squadMember.transform.position;
                }
                else
                {
                    squadMember.agent.destination = ((squadCenter - squadMember.transform.position).normalized) * (distFromCenter - squadRadius);
                }
                //formations

            }
            //attack threat
        }

        //if close to point && no threat at point
        //defend point
        if ((squadMember.transform.position- wayPoint).sqrMagnitude < squadRadius)
        {
            float threatValueAtAttackSpot = EvaluateThreatOnPoint(wayPoint, squadMember.knownThreats);
            if (threatValueAtAttackSpot == 0.0f)
            {
                return true;
            }
        }
        return false;
    }

    public static bool Defend(NewSquadMember squadMember, Vector3 wayPoint)
    {
        //threat = findClosestVisibleKnownThreat();
        //Follow and assemble to defend the objective
        int count = 0;
        Vector3 point = Vector3.zero;
        foreach (var member in SquadMaster.Instance.getSquad(squadMember.squadID))
        {
            if (member != squadMember)
            {
                count++;
                if (member)
                {
                    point += member.transform.position;
                }
            }
        }
        if (count > 0)
        {
            point /= count;
        }
        else
        {
            squadMember.agent.destination = wayPoint;
            return true;
        }
        Vector3 center = point;
        Vector3 bestDirFromDefenseSpot = wayPoint - (center);
        bestDirFromDefenseSpot.y = 0.0f;
        if (bestDirFromDefenseSpot.magnitude < 4.0f)
        {
            bestDirFromDefenseSpot.Normalize();
            bestDirFromDefenseSpot *= 4.0f;
        }
        Vector3 bestPos = wayPoint + bestDirFromDefenseSpot;
        //Debug.DrawLine(bestPos, transform.position);

        var covers = CreateCoverSpotsList();
        //covers = RemoveUnreachableCovers(covers, squadMember.transform.position);
        covers = RemoveOccupiedCovers(covers, squadMember, 1.5f);
        covers = RemoveCoversOutsideRadius(covers, center, 10.0f);
        covers = EvaluateThreatsOnCovers(covers, squadMember.knownThreats);

        CoverSpot bestCover = GetBestCoverSpot(covers);
        //
        if (bestCover != null)
        {
            if ((bestCover.pos - bestPos).sqrMagnitude < 16.0f)
            {
                bestPos = bestCover.pos;
            }
        }
        
        NavMeshHit hit;
        if (NavMesh.SamplePosition(bestPos, out hit, Mathf.Infinity, NavMesh.AllAreas))
            squadMember.agent.destination = hit.position;
        return false;
        return (EvaluateThreatOnPoint(wayPoint, squadMember.knownThreats) == 0 &&
            (squadMember.transform.position- hit.position).sqrMagnitude < 4.0f);
    }

    public static bool Cover(NewSquadMember squadMember, Vector3 wayPoint)
    {
        //Find best cover
        var covers = CreateCoverSpotsList();
        //covers = RemoveCoversOutsideRadius(covers, squadCenter, squadRadius);
        covers = IncreaseCoverCostOnDistance(covers, squadMember.transform.position, 1.0f);
        //covers = IncreaseCoverCostOnDistance(covers, squadCenter, 1.0f);
        covers = EvaluateThreatsOnCovers(covers, squadMember.knownThreats);
        covers = RemoveOccupiedCovers(covers, squadMember, 1.5f);
        //increaseCoverCostOnDistance(transform.position);
        //evaluatePointsOfInterest();
        //evaluateDistanceFromSquadMembers();
        var best = GetBestCoverSpot(covers);
        NavMeshHit hit;
        if (NavMesh.SamplePosition(best.pos, out hit, 1.0f, NavMesh.AllAreas))
            squadMember.agent.SetDestination(hit.position);
        if((squadMember.transform.position- hit.position).sqrMagnitude < 4.0f)
        {
            return true;
        }
        return false;
    }

    public static bool Flank(NewSquadMember squadMember, Vector3 wayPoint)
    {
        squadMember.agent.destination = wayPoint;
        Vector3 squadCenter = SquadMaster.Instance.getCenterPoint(squadMember.squadID);
        float squadRadius = 10.0f; //to be changed based on squad members
        Vector3 attackDirection = wayPoint - squadCenter;

        // calculate threat value
        float threatOnSelf = EvaluateThreatOnPoint(squadMember.transform.position, squadMember.knownThreats);
        // if no threat 
        if (threatOnSelf == 0.0f)
        {
            //stay with squad
            //formations
            //move towards the point
            squadMember.agent.destination = wayPoint;
        }
        else
        {
            //find cover from threat near squad
            //Find best cover
            var covers = CreateCoverSpotsList();
            //covers = RemoveCoversOutsideRadius(covers, squadCenter, squadRadius);
            covers = IncreaseCoverCostOnDistance(covers, squadMember.transform.position, 1.0f);
            //covers = IncreaseCoverCostOnDistance(covers, squadCenter, 1.0f);
            covers = EvaluateThreatsOnCovers(covers, squadMember.knownThreats);
            covers = RemoveOccupiedCovers(covers, squadMember, 1.5f);
            var best = GetBestCoverSpot(covers);
            if (best != null)
            {
                NavMeshHit hit;
                if (NavMesh.SamplePosition(best.pos, out hit, 1.0f, NavMesh.AllAreas))
                    squadMember.agent.destination = hit.position;
            }
            else
            {
                //if no cover near squad
                // find closest point in squad radius and go there;
                float distFromCenter = (squadMember.transform.position - squadCenter).sqrMagnitude;
                if (distFromCenter < squadRadius)
                {
                    squadMember.agent.destination = squadMember.transform.position;
                }
                else
                {
                    squadMember.agent.destination = ((squadCenter - squadMember.transform.position).normalized) * (distFromCenter - squadRadius);
                }
                //formations

            }
            //attack threat
        }

        //if close to point && no threat at point
        //defend point
        if ((squadMember.transform.position - wayPoint).sqrMagnitude < squadRadius)
        {
            float threatValueAtAttackSpot = EvaluateThreatOnPoint(wayPoint, squadMember.knownThreats);
            if (threatValueAtAttackSpot == 0.0f)
            {
                return true;
            }
        }
        return false;
    }

    public static bool Goto(NewSquadMember squadMember, Vector3 wayPoint)
    {
        squadMember.agent.destination = wayPoint;
        return (squadMember.transform.position- wayPoint).sqrMagnitude < 4.0f;
    }


    //Utils
    public static List<CoverSpot> CreateCoverSpotsList()
    {
        List<CoverSpot> coverSpots = new List<CoverSpot>();
        foreach (var cover in FindCoverSpots.Instance.coverSpots)
        {
            coverSpots.Add(new CoverSpot(cover.pos));
        }
        return coverSpots;
    }

    public static List<CoverSpot> RemoveCoversOutsideRadius(List<CoverSpot> covers,Vector3 point, float radius)
    {
        covers.RemoveAll(item => (item.pos-point).sqrMagnitude > (radius * radius));
        return covers;

    }

    public static List<CoverSpot> RemoveUnreachableCovers(List<CoverSpot> covers, Vector3 point)
    {
        NavMeshPath path = new NavMeshPath();
        covers.RemoveAll(item => !NavMesh.CalculatePath(point, item.pos, NavMesh.AllAreas, path));
        return covers;

    }

    public static List<CoverSpot> RemoveOccupiedCovers(List<CoverSpot> covers, NewSquadMember self, float threshold)
    {
        foreach (var squadMember in SquadMaster.Instance.getSquads())
        { 
            if (squadMember != self)
            {
                covers.RemoveAll(item => (item.pos - squadMember.transform.position).sqrMagnitude < threshold * 2.5f);
            }
        }
        return covers;

    }

    public static List<CoverSpot> IncreaseCoverCostOnDistance(List<CoverSpot> covers, Vector3 point, float multiplier)
    {
        foreach (var cover in covers)
        {
            cover.cost += (cover.pos- point).sqrMagnitude * multiplier;
        }
        return covers;
    }

    public static List<CoverSpot> EvaluateThreatsOnCovers(List<CoverSpot> covers, List<ThreatValue> threats)
    {
        foreach(var threat in threats)
        {
            foreach (var cover in covers)
            {
                if (!Physics.Raycast(threat.transform.position, cover.pos))
                cover.cost += threat.value;
            }
        }
        return covers;
    }

    public static CoverSpot GetBestCoverSpot(List<CoverSpot> covers)
    {
        float minCost = 100000000;
        CoverSpot best = null;
        foreach (var cover in covers)
        {
            if (cover.cost < minCost)
            {
                best = cover;
                minCost = cover.cost;
            }
        }
        return best;
    }

    public static float EvaluateThreatOnPoint(Vector3 point, List<ThreatValue> threats)
    {
        float threatVal = 0;
        foreach (var threat in threats)
        {
            if (!Physics.Raycast(threat.transform.position, point - threat.transform.position))
                threatVal += threat.value;
        }
        return threatVal;
    }

    public static Vector3 CalculateThreatDirection(Vector3 point, List<ThreatValue> threats)
    {
        Vector3 dir = Vector3.zero;



        return dir;
    }
}

