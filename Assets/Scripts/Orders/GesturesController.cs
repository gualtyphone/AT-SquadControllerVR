using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;



public class GesturesController : Singleton<GesturesController>
{

    public HandController handLeft;
    public HandController handRight;

    public Transform HipPoint;

    public List<Gesture> gestures;
    public bool gestureFound = false;

    public Text headsetCanvasText;
    //bool selectionMovement = false;
    bool selectedDestination = false;
    public bool orderStarted = false;

    List<NewSquadMember> squadMembersSelected;

    GameObject lastPointed;
    Vector3 destination;
    bool pointing = false;

    // Use this for initialization
    void Start()
    {
        squadMembersSelected = new List<NewSquadMember>();
        gestures.AddRange(transform.GetComponentsInChildren<Gesture>());
    }

    // Update is called once per frame
    void Update()
    {
        pointing = false;
        if (gestureFound)
        {
            handLeft.lastPositions.RemoveRange(0, handLeft.lastPositions.Count);
            handLeft.lastPositions.Add(handLeft.transform.position);
            handLeft.lastRotations.RemoveRange(0, handLeft.lastRotations.Count);
            handLeft.lastRotations.Add(handLeft.transform.rotation);

            handRight.lastPositions.RemoveRange(0, handRight.lastPositions.Count);
            handRight.lastPositions.Add(handRight.transform.position);
            handRight.lastRotations.RemoveRange(0, handRight.lastRotations.Count);
            handRight.lastRotations.Add(handRight.transform.rotation);
            gestureFound = false;
        }
        else
        {
            CompareGestures();
            if (selectedDestination && orderStarted && !pointing)
            {
                OrderFactory.Instance.CompleteOrder(destination);
                orderStarted = false;
                selectedDestination = false;
            }
        }

        

    }

    bool drawing = false;
    Vector3 startPos;
    Quaternion startRot;

    void CompareGestures()
    {
        foreach (Gesture gesture in gestures)
        {
            tryGesture(gesture);
        }
    }

    void tryGesture(Gesture gesture)
    {
        if (gesture.needLeftButtons)
        {
            bool found = false;
            foreach (HandButtonsState hbs in gesture.PossibleLeftHands)
            {
                if (hbs.equals(handLeft.handButtonsState))
                {
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                gesture.DoNotDraw();
                return;
            }
        }

        if (gesture.needRightButtons)
        {
            bool found = false;
            foreach (HandButtonsState hbs in gesture.PossibleRightHands)
            {
                if (hbs.equals(handRight.handButtonsState))
                {
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                gesture.DoNotDraw();
                return;
            }
        }

        if (gesture.needLeftMovement)
        {
            if (!Gesture.ConvalidateMovement(handLeft.lastPositions, handLeft.lastRotations, gesture.L_positions, gesture, HipPoint))
            {
                return;
            }
        }
        if (gesture.needRightMovement)
        {
            if (!Gesture.ConvalidateMovement(handRight.lastPositions, handRight.lastRotations, gesture.R_positions, gesture, HipPoint))
            {
                return;
            }
        }

        headsetCanvasText.text = gesture.Name;
        gesture.OnGestureTrigger.Invoke();
        gestureFound = true;
    }

    public void selectAll()
    {
        bool found = false;
        foreach (var squadMember in SquadMaster.Instance.getSquads())
        {
            if (!squadMember.GetComponent<ThreatValue>().enemy)
            {
                if (!squadMembersSelected.Contains(squadMember))
                {
                    found = true;
                    squadMembersSelected.Add(squadMember);
                }
            }
        }
        if (!found)
        {
            squadMembersSelected.RemoveRange(0, squadMembersSelected.Count);
        }
        SquadMaster.Instance.updateSelected(squadMembersSelected);
    }
     
    public void Selection()
    {
        pointing = true;
        if (orderStarted)
        {
            if (handLeft.handState == HandController.HandState.Pointing &&
                handRight.handState != HandController.HandState.Pointing)
            {
                RaycastHit hitInfo;
                if (Physics.Raycast(handLeft.indexPoint.position, handLeft.indexPoint.forward, out hitInfo))
                {
                    if (hitInfo.collider.tag == "MiniMap")
                    {
                        var localPoint = hitInfo.textureCoord;
                        Camera portalCamera = hitInfo.collider.GetComponent<Minimap>().camera;
                        Ray ray = portalCamera.ScreenPointToRay(new Vector2(localPoint.x * portalCamera.pixelWidth, localPoint.y * portalCamera.pixelHeight));
                        if (Physics.Raycast(ray, out hitInfo, 9999f))
                        {
                            // hit should now contain information about what was hit through secondCamera
                            NavMeshHit hitInfoNavMesh1;
                            if (NavMesh.SamplePosition(hitInfo.point, out hitInfoNavMesh1, 0.1f, NavMesh.AllAreas))
                            {
                                //SquadMaster.Instance.setWaypoint(squadsSelected, hitInfo.point);
                                destination = hitInfo.point;
                                selectedDestination = true;
                            }
                        }
                    }
                    NavMeshHit hitInfoNavMesh;
                    if (NavMesh.SamplePosition(hitInfo.point, out hitInfoNavMesh, 0.1f, NavMesh.AllAreas))
                    {
                        //SquadMaster.Instance.setWaypoint(squadsSelected, hitInfo.point);
                        destination = hitInfo.point;
                        selectedDestination = true;
                    }
                }
            }

            else if (handLeft.handState != HandController.HandState.Pointing &&
                     handRight.handState == HandController.HandState.Pointing)
            {
                RaycastHit hitInfo;
                if (Physics.Raycast(handRight.indexPoint.position, handRight.indexPoint.forward, out hitInfo))
                {
                    if (hitInfo.collider.tag == "MiniMap")
                    {
                        var localPoint = hitInfo.textureCoord;
                        Camera portalCamera = hitInfo.collider.GetComponent<Minimap>().camera;
                        Ray ray = portalCamera.ScreenPointToRay(new Vector2(localPoint.x * portalCamera.pixelWidth, localPoint.y * portalCamera.pixelHeight));
                        if (Physics.Raycast(ray, out hitInfo, 9999f))
                        {
                            // hit should now contain information about what was hit through secondCamera
                            NavMeshHit hitInfoNavMesh1;
                            if (NavMesh.SamplePosition(hitInfo.point, out hitInfoNavMesh1, 0.1f, NavMesh.AllAreas))
                            {
                                //SquadMaster.Instance.setWaypoint(squadsSelected, hitInfo.point);
                                destination = hitInfo.point;
                                selectedDestination = true;
                            }
                        }
                    }
                    NavMeshHit hitInfoNavMesh;
                    if (NavMesh.SamplePosition(hitInfo.point, out hitInfoNavMesh, 0.1f, NavMesh.AllAreas))
                    {
                        //SquadMaster.Instance.setWaypoint(squadsSelected, hitInfo.point);
                        destination = hitInfo.point;
                        selectedDestination = true;
                    }
                }
            }
            return;
        }
        else
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
                    if (hitInfo.collider.tag == "MiniMap")
                    {
                        var localPoint = hitInfo.textureCoord;
                        Camera portalCamera = hitInfo.collider.GetComponent<Minimap>().camera;
                        Ray ray = portalCamera.ScreenPointToRay(new Vector2(localPoint.x * portalCamera.pixelWidth, localPoint.y * portalCamera.pixelHeight));
                        if (Physics.Raycast(ray, out hitInfo, 9999f))
                        {
                            
                        }
                    }
                    if (hitInfo.collider.gameObject != lastPointed)
                    {
                        lastPointed = hitInfo.collider.gameObject;
                        if (lastPointed.tag == "SquadMember")
                        {
                            var id = lastPointed.GetComponent<NewSquadMember>();
                            if (!id.GetComponent<ThreatValue>().enemy)
                            {
                                if (squadMembersSelected.Contains(id))
                                {
                                    squadMembersSelected.Remove(id);
                                }
                                else
                                {
                                    squadMembersSelected.Add(id);
                                }
                            }
                            SquadMaster.Instance.updateSelected(squadMembersSelected);
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
                    if (hitInfo.collider.tag == "MiniMap")
                    {
                        var localPoint = hitInfo.textureCoord;
                        Camera portalCamera = hitInfo.collider.GetComponent<Minimap>().camera;
                        Ray ray = portalCamera.ScreenPointToRay(new Vector2(localPoint.x * portalCamera.pixelWidth, localPoint.y * portalCamera.pixelHeight));
                        if (Physics.Raycast(ray, out hitInfo, 9999f))
                        {

                        }
                    }
                    if (hitInfo.collider.gameObject != lastPointed)
                    {
                        lastPointed = hitInfo.collider.gameObject;
                        if (lastPointed.tag == "SquadMember")
                        {
                            var id = lastPointed.GetComponent<NewSquadMember>();
                            if (!id.GetComponent<ThreatValue>().enemy)
                            {
                                if (squadMembersSelected.Contains(id))
                                {
                                    squadMembersSelected.Remove(id);
                                }
                                else
                                {
                                    squadMembersSelected.Add(id);
                                }
                            }
                            SquadMaster.Instance.updateSelected(squadMembersSelected);
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
    }
    public void Grouping()
    {
        if (squadMembersSelected.Count > 1)
        {
            if (handLeft.handState == HandController.HandState.ThumbsUp ||
                handRight.handState == HandController.HandState.ThumbsUp)
            {
                SquadMaster.Instance.mergeSquads(squadMembersSelected);
            }
            
        }
    }    
}