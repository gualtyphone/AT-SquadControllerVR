using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class Gesture : MonoBehaviour
{
    public string Name;

    static float threshold = 0.1f;

    public bool needLeftButtons;
    public bool needRightButtons;

    public HandButtonsState[] PossibleLeftHands;
    public HandButtonsState[] PossibleRightHands;

    public bool needLeftMovement;
    public bool needRightMovement;

    public List<Vector3> L_positions;
    public List<Vector3> R_positions;

    public UnityEvent OnGestureTrigger;

    public static bool ConvalidateMovement(List<Vector3> handPos, List<Quaternion> handRot, List<Vector3> gesturePos)
    {
        //foreach possible start
        for (int possibleStart = 0; possibleStart < handPos.Count; possibleStart++)
        {
            if (ConvalidatePossibleStart(possibleStart, handPos, handRot, gesturePos))
            {
                return true;
            }
        }
        return false;
    }

    public static bool ConvalidatePossibleStart(int possibleStart, List<Vector3> handPos, List<Quaternion> handRot, List<Vector3> gesturePos)
    {
        int nextValidGesture = possibleStart;
        //assuming that the gesture start was in the possible start position
        Vector3 diff = handPos[possibleStart] - gesturePos[0]; //5.4
        //foreach gesture
        foreach (Vector3 original in gesturePos)
        {
            do
            {
                nextValidGesture++;
                if (nextValidGesture >= handPos.Count)
                {
                    return false;
                }
            } while ((Vector3.Distance(RotatePointAroundPivot((original + diff),handPos[possibleStart] , handRot[possibleStart]), handPos[nextValidGesture]) >= threshold));
        }
        return true;
    }

    public void Draw(Vector3 RstartingPoint, Quaternion handRotation)
    {
        
        LineRenderer lr = GetComponent<LineRenderer>();
        lr.positionCount = R_positions.Count;
        Vector3 diff =  RstartingPoint - R_positions[0];
        for (int i = 0; i < R_positions.Count; i++)
        {
            lr.SetPosition(i, RotatePointAroundPivot((R_positions[i] + diff), RstartingPoint, handRotation));
        }
        
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