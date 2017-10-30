using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquadMaster : Singleton<SquadMaster>{

    protected SquadMaster() { }

    [SerializeField]
    public List<int> SquadIDs;
    [SerializeField]
    Dictionary<int, List<NewSquadMember>> squads;
    [SerializeField]
    Dictionary<int, Color> squadColors;

    public void Awake()
    {
        SquadIDs = new List<int>();
        squads = new Dictionary<int, List<NewSquadMember>>();
        squadColors = new Dictionary<int, Color>();
    }

    public void Update()
    {
        //cleanupSquads
        if (squads != null)
        {
            foreach (var squad in squads)
            {

                squad.Value.RemoveAll(isNull);
            }
        }
    }

    private static bool isNull(NewSquadMember s)
    {
        return s == null;
    }

    public int getUniqueID(NewSquadMember newSquadMember)
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
        squads.Add(newID, new List<NewSquadMember>());
        squads[newID].Add(newSquadMember);
        squadColors.Add(newID, Random.ColorHSV(0.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f));
        newSquadMember.squadColor = squadColors[newID];
        return newID;
    }

    public void updateSelected(List<int> ids)
    {
        foreach(var squad in squads)
        {
            foreach (NewSquadMember member in squad.Value)
            {
                member.selected = ids.Contains(squad.Key);
            }
        }
    }

    public void updateSelected(List<NewSquadMember> members)
    {
        foreach (NewSquadMember member in getSquads())
        {
            member.selected = members.Contains(member) ;
        }
        
    }

    public List<NewSquadMember> getSelectedMembers()
    {
        List<NewSquadMember> squadMembers = new List<NewSquadMember>();
        foreach (var squad in squads)
        {
            foreach (NewSquadMember member in squad.Value)
            {
                if (member.selected)
                {
                    squadMembers.Add(member);
                }
            }
        }
        return squadMembers;
    }

    public int mergeSquads(List<int> mergingSquads)
    {
        for (int i = 1; i < mergingSquads.Count; i++)
        {
            foreach (NewSquadMember member in squads[mergingSquads[i]])
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

    public int mergeSquads(List<NewSquadMember> mergingSquads)
    {
        List<int> mergingIds = new List<int>();
        foreach (var member in mergingSquads)
        {
            if (!mergingIds.Contains( member.squadID))
            {
                mergingIds.Add(member.squadID);
            }
        }

        if (mergingIds.Count > 1)
        {
            return mergeSquads(mergingIds);
        }
        return mergingSquads[0].squadID;
    }

    public void breakSquad(int squadID)
    {

    }

    //public void setSquadAction(SquadMemberAction action)
    //{
    //    foreach (NewSquadMember member in FindObjectsOfType<NewSquadMember>())
    //    {
    //       if (member.selected)
    //       {
    //           member.currentAction = action;
    //       }
    //    }
    //}

    //public void setSquadAction(int action)
    //{
    //    foreach (NewSquadMember member in FindObjectsOfType<NewSquadMember>())
    //    {
    //        if (member.selected)
    //        {
    //            member.currentAction = (SquadMemberAction)action;
    //        }
    //    }
    //}

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

    //public void setWaypoint(List<int> squadsIds, Vector3 pos)
    //{
    //    foreach (int squad in squadsIds)
    //    {
    //        foreach (var member in squads[squad])
    //        {
    //            member.finalDestination = pos;
    //        }
    //    }
    //}

    public List<NewSquadMember> getSquad(int id)
    {
        return squads[id];
    }

    public List<NewSquadMember> getSquads()
    {
        List<NewSquadMember> members = new List<NewSquadMember>();
        foreach (var squad in squads)
        {
            members.AddRange(squad.Value);
        }
        return members;
    }

   
}
