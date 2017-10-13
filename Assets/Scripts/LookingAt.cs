using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookingAt
{
    public static bool IsLookingAt(GameObject objA, GameObject objB, float coneSize = 0.5f)
    {
        Vector3 dirFromAtoB = (objB.transform.position - objA.transform.position).normalized;
        float dotProd = Vector3.Dot(dirFromAtoB, objA.transform.forward);

        //Debug.DrawLine(objA.transform.position, objB.transform.position, ((dotProd > coneSize) ? Color.red : Color.green));

        return (dotProd > coneSize);
    }

    public static bool IsLookingAt(Vector3 pos, Vector3 forward, GameObject objB, float coneSize = 0.5f)
    {
        Vector3 dirFromAtoB = (objB.transform.position - pos).normalized;
        float dotProd = Vector3.Dot(dirFromAtoB, forward);

        //Debug.DrawLine(objA.transform.position, objB.transform.position, ((dotProd > coneSize) ? Color.red : Color.green));

        return (dotProd > coneSize);
    }

    public static bool IsLookingAt(Vector3 pos, Vector3 forward, Vector3 objB, float coneSize = 0.5f)
    {
        Vector3 dirFromAtoB = (objB - pos).normalized;
        float dotProd = Vector3.Dot(dirFromAtoB, forward);

        //Debug.DrawLine(objA.transform.position, objB.transform.position, ((dotProd > coneSize) ? Color.red : Color.green));

        return (dotProd > coneSize);
    }
}

