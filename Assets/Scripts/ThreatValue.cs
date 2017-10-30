using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThreatValue : MonoBehaviour {

    protected void OnDrawGizmosSelected()
    {

        if (!this.enabled)
            return;
        Gizmos.color = new Color(1.0f, 0.0f, 0.0f, .6f);
        Gizmos.DrawSphere(transform.position, radius);
    }

    [Range(0.0f, 100.0f)]
    public float value = 0.0f;

    public float radius = 0.0f;

    public bool enemy = true;
}
