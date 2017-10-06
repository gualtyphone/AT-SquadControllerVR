using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GesturesController : MonoBehaviour
{

    public HandController handLeft;
    public HandController handRight;

    public enum Action
    {
        SelectingMembers,
        SelectingDestination
    }

    List<int> squadsSelected;

    GameObject lastPointed;

    public Action currentAction;

    // Use this for initialization
    void Start()
    {
        squadsSelected = new List<int>();
        currentAction = Action.SelectingMembers;
    }

    // Update is called once per frame
    void Update()
    {
        if (currentAction == Action.SelectingMembers)
        {
            Selection();
            Grouping();
            TriggerMoving();
        }
        else if (currentAction == Action.SelectingDestination)
        {
            Moving();
        }
        
    }

    void Selection()
    {
        if (handLeft.handState == HandController.HandState.Pointing &&
                        handRight.handState != HandController.HandState.Pointing)
        {
            RaycastHit hitInfo;
            //int layerMask = 1 << 8;
            //layerMask = ~layerMask;
            //Debug.DrawRay(handLeft.indexPoint.position, handLeft.indexPoint.forward);
            if (Physics.Raycast(handLeft.indexPoint.position, handLeft.indexPoint.forward, out hitInfo/*, layerMask*/))
            {
                if (hitInfo.collider.gameObject != lastPointed)
                {
                    lastPointed = hitInfo.collider.gameObject;
                    if (lastPointed.tag == "SquadMember")
                    {
                        int id = lastPointed.GetComponent<SquadMember>().squadID;
                        if (squadsSelected.Contains(id))
                        {
                            squadsSelected.Remove(id);
                        }
                        else
                        {
                            squadsSelected.Add(id);
                        }
                        SquadMaster.Instance.updateSelected(squadsSelected);
                    }
                }
            }
            else
            {
                lastPointed = null;
            }
        }
        else if (handLeft.handState != HandController.HandState.Pointing &&
                 handRight.handState == HandController.HandState.Pointing)
        {
            RaycastHit hitInfo;
            //int layerMask = 1 << 8;
            //layerMask = ~layerMask;
            //Debug.DrawRay(handRight.indexPoint.position, handRight.indexPoint.forward);
            if (Physics.Raycast(handRight.indexPoint.position, handRight.indexPoint.forward, out hitInfo/*, layerMask*/))
            {
                if (hitInfo.collider.gameObject != lastPointed)
                {
                    lastPointed = hitInfo.collider.gameObject;
                    if (lastPointed.tag == "SquadMember")
                    {
                        int id = lastPointed.GetComponent<SquadMember>().squadID;
                        if (squadsSelected.Contains(id))
                        {
                            squadsSelected.Remove(id);
                        }
                        else
                        {
                            squadsSelected.Add(id);
                        }
                        SquadMaster.Instance.updateSelected(squadsSelected);
                    }
                }
            }
            else
            {
                lastPointed = null;
            }
        }
        else
        {
            lastPointed = null;
        }

    }
    void Grouping()
    {
        if (squadsSelected.Count > 1)
        {
            if (handLeft.handState == HandController.HandState.ThumbsUp ||
                handRight.handState == HandController.HandState.ThumbsUp)
            {
                int newSquad = 
                SquadMaster.Instance.mergeSquads(squadsSelected);
                squadsSelected.Clear();
                squadsSelected.Add(newSquad);
                SquadMaster.Instance.updateSelected(squadsSelected);
            }
            
        }
    }
    void TriggerMoving()
    {
        if (squadsSelected.Count >= 1)
        {
            if (handLeft.handState == HandController.HandState.FourFingers ||
                handRight.handState == HandController.HandState.FourFingers)
            {
                currentAction = Action.SelectingDestination;
                selectionMovement = true;
            }
        }
    }

    bool selectionMovement = false;
    void Moving()
    {
        if (handLeft.handState == HandController.HandState.Pointing &&
            handRight.handState != HandController.HandState.Pointing)
        {
            RaycastHit hitInfo;
            if (Physics.Raycast(handLeft.indexPoint.position, handLeft.indexPoint.forward, out hitInfo))
            {
                NavMeshHit hitInfoNavMesh;
                if (NavMesh.SamplePosition(hitInfo.point, out hitInfoNavMesh, 0.1f, NavMesh.AllAreas))
                {
                    SquadMaster.Instance.setWaypoint(squadsSelected, hitInfo.point);
                    selectionMovement = false;
                }
            }
        }

        else if (handLeft.handState != HandController.HandState.Pointing &&
                 handRight.handState == HandController.HandState.Pointing)
        {
            RaycastHit hitInfo;
            if (Physics.Raycast(handRight.indexPoint.position, handRight.indexPoint.forward, out hitInfo))
            {
                NavMeshHit hitInfoNavMesh;
                if (NavMesh.SamplePosition(hitInfo.point, out hitInfoNavMesh, 0.1f, NavMesh.AllAreas))
                {
                    SquadMaster.Instance.setWaypoint(squadsSelected, hitInfo.point);
                    selectionMovement = false;
                }
            }
        }

        else if (!selectionMovement)
        {
            currentAction = Action.SelectingMembers;
        }
    }
}














        //	foreach(Gesture gesture in gestures)
        //       {
        //           if (gesture.singleHanded)
        //           {
        //               foreach (HandController hand in hands)
        //               {
        //                   if (hand.handState == gesture.handState)
        //                   {
        //                       if (gesture.detector.Detect(hands))
        //                       {
        //                           gesture.action.Execute();
        //                       }
        //                   }
        //               }
        //           }
        //           else
        //           {
        //               foreach (HandController hand in hands)
        //               {
        //                   if (hand.handState == gesture.leftHandState)
        //                   {
        //                       foreach (HandController hand2 in hands)
        //                       {
        //                           if (hand2.handState == gesture.rightHandState)
        //                           {
        //                               if (gesture.detector.Detect(hands))
        //                               {
        //                                   gesture.action.Execute();
        //                               }
        //                           }
        //                       }
        //                   }
        //                   else if (hand.handState == gesture.rightHandState)
        //                   {
        //                       foreach (HandController hand2 in hands)
        //                       {
        //                           if (hand2.handState == gesture.leftHandState)
        //                           {
        //                               if (gesture.detector.Detect(hands))
        //                               {
        //                                   gesture.action.Execute();
        //                               }
        //                           }
        //                       }
        //                   }
        //               }
        //           }
        //       }
        //}