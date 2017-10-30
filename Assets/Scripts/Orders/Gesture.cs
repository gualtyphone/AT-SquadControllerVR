using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
//[ExecuteInEditMode]
public class Gesture : MonoBehaviour
{
    public string Name;

    public GameObject obj;

    static float threshold = 0.01f;

    public bool needLeftButtons;
    public bool needRightButtons;

    public HandButtonsState[] PossibleLeftHands;
    public HandButtonsState[] PossibleRightHands;

    public bool needLeftMovement;
    public bool needRightMovement;

    public List<Vector3> L_positions;
    public List<Vector3> R_positions;

    public bool needStartPosition;

    public Vector3 offsetFromHip;

    public bool DebugDisplay;

    public bool temp;

    public UnityEvent OnGestureTrigger;

    LineRenderer lr;

    public bool recording;

    void Start()
    {
        lr = GetComponent<LineRenderer>();
        
    }

    Vector3 originalHip;

    void Update()
    {
        if (recording)
        {
            if (needLeftButtons)
            {
                bool found = false;
                foreach (HandButtonsState hbs in PossibleLeftHands)
                {
                    if (hbs.equals(GesturesController.Instance.handLeft.handButtonsState))
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                { 
                    return;
                }
            }

            if (needRightButtons)
            {
                bool found = false;
                foreach (HandButtonsState hbs in PossibleRightHands)
                {
                    if (hbs.equals(GesturesController.Instance.handRight.handButtonsState))
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    return;
                }
            }
            if (needLeftMovement)
            {
                var hl = GesturesController.Instance.handRight;
                if (L_positions.Count == 0)
                {
                    originalHip = GesturesController.Instance.HipPoint.position;
                    offsetFromHip = hl.lastPositions[hl.lastPositions.Count - 1] - GesturesController.Instance.HipPoint.position;
                    L_positions.Add(hl.lastPositions[hl.lastPositions.Count - 1] - offsetFromHip - GesturesController.Instance.HipPoint.position);
                }
                else if ((L_positions[L_positions.Count - 1] - hl.lastPositions[hl.lastPositions.Count - 1] - offsetFromHip - originalHip).sqrMagnitude > threshold*2)
                    L_positions.Add(hl.lastPositions[hl.lastPositions.Count - 1] - offsetFromHip - originalHip);
            }

            if (needRightMovement)
            {
                
                var hr = GesturesController.Instance.handRight;
                if (R_positions.Count == 0)
                {
                    originalHip = GesturesController.Instance.HipPoint.position;
                    offsetFromHip = hr.lastPositions[hr.lastPositions.Count - 1] - GesturesController.Instance.HipPoint.position;
                    R_positions.Add(hr.lastPositions[hr.lastPositions.Count - 1] - offsetFromHip - GesturesController.Instance.HipPoint.position);
                }
                else if ((R_positions[R_positions.Count - 1] - hr.lastPositions[hr.lastPositions.Count - 1] - offsetFromHip - originalHip).sqrMagnitude > threshold*2)
                    R_positions.Add(hr.lastPositions[hr.lastPositions.Count - 1] - offsetFromHip - originalHip);
            }
        }

        
            //if (DebugDisplay)
            //{
            //    Draw(RotatePointAroundPivot(offsetFromHip + GesturesController.Instance.HipPoint.position,
            //        GesturesController.Instance.HipPoint.transform.position, GesturesController.Instance.HipPoint.rotation), 
            //        GesturesController.Instance.HipPoint.rotation);
            //}
        }

    public static bool ConvalidateMovement(List<Vector3> handPos, List<Quaternion> handRot, List<Vector3> gesturePos, Gesture gest, Transform hip)
    {
        if (!gest.needStartPosition)
        {
            Vector3 MFGPos = new Vector3();
            Quaternion MFGRot = new Quaternion();
            int mostFinishedGesture = 2;
            //foreach possible start
            for (int possibleStart = 0; possibleStart < handPos.Count; possibleStart++)
            {
                int finalGestureFound = ConvalidatePossibleStart(possibleStart, handPos, handRot, gesturePos, gest);
                if (finalGestureFound == gesturePos.Count)
                {
                    gest.DoNotDraw();
                    return true;
                }
                else if (finalGestureFound > mostFinishedGesture)
                {
                    mostFinishedGesture = finalGestureFound;
                    MFGPos = handPos[possibleStart];
                    MFGRot = handRot[possibleStart];
                    //possibleStart = handPos.Count;
                }
            }
            if (mostFinishedGesture > 3 || gest.DebugDisplay)
            {
                gest.Draw(MFGPos, MFGRot);
            }
            else
            {
                gest.DoNotDraw();
            }
        }
        else
        {
            //foreach possible start
            for (int possibleStart = 0; possibleStart < handPos.Count; possibleStart++)
            {
                int finalGestureFound = ConvalidatePossibleStart(possibleStart, handPos, handRot, gesturePos, gest, hip);
                if (finalGestureFound == gesturePos.Count)
                {
                    gest.DoNotDraw();
                    return true;
                }
            }
            if (gest.DebugDisplay)
            {
                gest.Draw(RotatePointAroundPivot(gest.offsetFromHip + hip.position , hip.transform.position, hip.rotation) , hip.rotation);
            }
        }
        return false;
    }

    public static int ConvalidatePossibleStart(int possibleStart, List<Vector3> handPos, List<Quaternion> handRot, List<Vector3> gesturePos, Gesture gest)
    {
        int nextValidGesture = possibleStart;
        //assuming that the gesture start was in the possible start position
        Vector3 diff = handPos[possibleStart] - gesturePos[0]; //5.4
        //foreach gesture
        int posInGestureOrig = 0;
        foreach (Vector3 original in gesturePos)
        {
            posInGestureOrig++;
            do
            {
                nextValidGesture++;
                
                if (nextValidGesture >= handPos.Count)
                {
                    return posInGestureOrig;
                }
            }while (((RotatePointAroundPivot((original + diff), handPos[possibleStart], handRot[possibleStart]) - handPos[nextValidGesture]).sqrMagnitude >= threshold));
            //Destroy(Instantiate(gest.obj, original + diff, Quaternion.identity), 1.0f);
        }
        return gesturePos.Count;
    }

    public static int ConvalidatePossibleStart(int possibleStart, List<Vector3> handPos, List<Quaternion> handRot, List<Vector3> gesturePos, Gesture gest, Transform hip)
    {
        int nextValidGesture = possibleStart;
        //assuming that the gesture start was in the possible start position
        Vector3 diff = (hip.position+gest.offsetFromHip) - gesturePos[0]; //5.4
        //foreach gesture
        int posInGestureOrig = 0;
        foreach (Vector3 original in gesturePos)
        {
            posInGestureOrig++;
            do
            {
                nextValidGesture++;

                if (nextValidGesture >= handPos.Count)
                {
                    return posInGestureOrig;
                }
            } while (((RotatePointAroundPivot(diff + original , hip.transform.position, hip.rotation) - handPos[nextValidGesture]).sqrMagnitude >= threshold));
        }
        return gesturePos.Count;
    }

    public void Draw(Vector3 RstartingPoint, Quaternion handRotation)
    {
        lr.enabled = true;
        lr.positionCount = R_positions.Count;
        Vector3 diff =  RstartingPoint - R_positions[0];
        for (int i = 0; i < R_positions.Count; i++)
        {
            lr.SetPosition(i, RotatePointAroundPivot((R_positions[i] + diff), RstartingPoint, handRotation));
        }
        
    }
    public void DoNotDraw()
    {
        lr.enabled = false;
    }
        static Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Quaternion rotation)
    {
        var dir = point - pivot; // get point direction relative to pivot
        dir = rotation * dir; // rotate it
        point = dir + pivot; // calculate rotated point
        return point; // return it
    }
}




[System.Serializable]
public class HandButtonsState
{
    public bool equals(HandButtonsState other)
    {
        if (TriggerPressed == other.TriggerPressed &&
            GripPressed == other.GripPressed &&
            ButtonAPressed == other.ButtonAPressed &&
            ButtonBPressed == other.ButtonBPressed &&
            MenuButtonPressed == other.MenuButtonPressed &&
            TouchpadPressed == other.TouchpadPressed)
        {
            return true;
        }
        return false;
    }
    public bool TriggerPressed;
    public bool GripPressed;
    public bool ButtonAPressed;
    public bool ButtonBPressed;
    public bool MenuButtonPressed;
    public bool TouchpadPressed;
}