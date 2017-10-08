using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquadMaster : Singleton<SquadMaster>{

    protected SquadMaster() { }

    [SerializeField]
    List<int> SquadIDs;
    [SerializeField]
    Dictionary<int, List<SquadMember>> squads;
    [SerializeField]
    Dictionary<int, Color> squadColors;

    public void Awake()
    {
        SquadIDs = new List<int>();
        squads = new Dictionary<int, List<SquadMember>>();
        squadColors = new Dictionary<int, Color>();
    }

	public int getUniqueID(SquadMember newSquadMember)
    {
        bool found = true;
        int newID = 0;
        do
        {
            found = true;
            foreach (int id in SquadIDs)
            {
                if (id == newID)
                {
                    found = false;
                    newID++;
                    break;
                }
            }
        } while (!found);
        SquadIDs.Add(newID);
        squads.Add(newID, new List<SquadMember>());
        squads[newID].Add(newSquadMember);
        squadColors.Add(newID, Random.ColorHSV(0.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f));
        newSquadMember.squadColor = squadColors[newID];
        return newID;
    }

    public void updateSelected(List<int> ids)
    {
        foreach(var squad in squads)
        {
            foreach (SquadMember member in squad.Value)
            {
                member.selected = ids.Contains(squad.Key);
            }
        }
    }

    public int mergeSquads(List<int> mergingSquads)
    {
        for (int i = 1; i < mergingSquads.Count; i++)
        {
            foreach (SquadMember member in squads[mergingSquads[i]])
            {
                squads[mergingSquads[0]].Add(member);
                member.UpdateSquad(mergingSquads[0], squadColors[mergingSquads[0]]);
            }
            squads.Remove(mergingSquads[i]);
            squadColors.Remove(mergingSquads[i]);
            SquadIDs.Remove(mergingSquads[i]);
        }
        return mergingSquads[0];
    }

    public void breakSquad(int squadID)
    {

    }

    public Vector3 getCenterPoint(List<int> squadsIds)
    {
        int count = 0;
        Vector3 point = new Vector3();
        foreach (var id in squadsIds)
        {
            foreach (var squadMember in squads[id])
            {
                count++;
                point += squadMember.transform.position;
            }
        }
        if (count > 0)
        { 
            point /= count;
        }
        return point;
    }

    public Vector3 getCenterPoint(int id)
    {
        int count = 0;
        Vector3 point = new Vector3();
        foreach (var squadMember in squads[id])
        {
            count++;
            point += squadMember.transform.position;
        }
        if (count > 0)
        {
            point /= count;
        }
        return point;
    }

    public void setWaypoint(List<int> squadsIds, Vector3 pos)
    {
        foreach (int squad in squadsIds)
        {
            foreach (var member in squads[squad])
            {
                member.destination = pos;
            }
        }
    }

    public List<SquadMember> getSquad(int id)
    {
        return squads[id];
    }

    public List<SquadMember> getSquads()
    {
        List<SquadMember> members = new List<SquadMember>();
        foreach (var squad in squads)
        {
            members.AddRange(squad.Value);
        }
        return members;
    }
}
