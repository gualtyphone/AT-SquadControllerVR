using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class NewSquadMember : MonoBehaviour {
    //stats
    public List<ThreatValue> knownThreats;

    //comands
    public List<Order> orders;

    public ThreatValue threat;

    //Squad Recognition
    public Color squadColor;
    public int squadID;
    private bool Selected;
    public bool selected
    {
        get { return Selected; }
        set
        {
            Selected = value;
            renderer.material.SetColor("_OutlineColor",
                new Color(1.0f - squadColor.r,
                          1.0f - squadColor.g,
                          1.0f - squadColor.b,
                          value == true ? 1.0f : 0.0f));
        }
    } 

//Others
    public NavMeshAgent agent;
    public MeshRenderer renderer;

    // Use this for initialization
    void Start () {
        orders = new List<Order>();

        agent = GetComponent<NavMeshAgent>();
        renderer = GetComponent<MeshRenderer>();

        squadID = SquadMaster.Instance.getUniqueID(this);
        agent.destination = transform.position;

        selected = false;

        renderer.material.color = squadColor;
        renderer.material.SetColor("_OutlineColor", new Color(1.0f - squadColor.r, 1.0f - squadColor.g, 1.0f - squadColor.b, 0.0f));

    }
	
	// Update is called once per frame
	void Update () {
        if (orders.Count > 0)
        {
            foreach (var order in orders)
            {
                if (!order.started)
                {
                    StartCoroutine(Order.startOrderCoroutine(order, this));
                    order.started = true;
                    order.running = true;
                    if (order.temp)
                    {
                        foreach (var ord in orders)
                        {
                            if (ord != order)
                                ord.running = false;
                        }
                    }
                    else
                    {
                        foreach (var ord in orders)
                        {
                            if (ord != order)
                                ord.finished = true;
                        }
                    }
                }
            }
            orders.RemoveAll(order => order.finished);
        }
        

        seePointsOfInterest();
        sharePOIWithTeamMates();

        knownThreats.RemoveAll(threat1 => threat1 == null);

        threat = null;
        threat = findClosestVisibleThreat();
        if (threat != null)
        {
            GetComponent<Character>().Shoot(threat.gameObject);
        }

    }

    public void UpdateSquad(int id, Color col)
    {
        squadID = id;
        squadColor = col;
        renderer.material.color = squadColor;
        renderer.material.SetColor("_OutlineColor", new Color(1.0f - squadColor.r, 1.0f - squadColor.g, 1.0f - squadColor.b, 1.0f));
    }

    //POIs
    void seePointsOfInterest()
    {
        bool enemy = GetComponent<ThreatValue>().enemy;
        ThreatValue[] possiblePois = FindObjectsOfType<ThreatValue>();
        foreach (var poi in possiblePois)
        {
            if (poi.enemy == enemy)
            {
                continue;
            }
            if (!knownThreats.Contains(poi))
            {
                
                //if (LookingAt.IsLookingAt(gameObject, poi.gameObject))
                //{
                //    LayerMask layerMask = 1 << 9;
                //    if (!Physics.Raycast(poi.transform.position, (transform.position - poi.transform.position),
                //        Mathf.Max((transform.position - poi.transform.position).magnitude, 10.0f), layerMask))
                //    {
                        //There is line of sight
                        knownThreats.Add(poi);
                //    }
                //}
            }
        }
    }
    void sharePOIWithTeamMates()
    {
        foreach (var teamMate in SquadMaster.Instance.getSquad(squadID))
        {
            if (teamMate != this)
            {
                foreach (var poi in teamMate.knownThreats)
                {
                    if (!knownThreats.Contains(poi))
                    {
                        knownThreats.Add(poi);
                    }
                }
            }
        }
    }

    ThreatValue findClosestVisibleThreat()
    {
        float dist = float.MaxValue;
        ThreatValue closest = null;
        foreach (ThreatValue threat in knownThreats)
        {
            RaycastHit hit;
            
            if (Physics.Raycast(transform.position, threat.transform.position - transform.position, out hit, dist))
            {
                Debug.DrawLine(transform.position, hit.point);
                var currDist = (threat.transform.position - transform.position).sqrMagnitude;
                if (currDist < dist)
                {
                    if (threat.GetComponent<Rigidbody>() != null)
                    {
                        if (hit.rigidbody == threat.GetComponent<Rigidbody>())
                        {
                            closest = threat;
                            dist = currDist;
                        }
                    }
                    else
                    {
                        if (hit.collider == threat.GetComponent<Collider>())
                        {
                            closest = threat;
                            dist = currDist;
                        }
                    }
                }
            }
        }
        return (closest);
    }
}
