using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefensePoint : MonoBehaviour {

    public float captureLevel;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnTriggerStay(Collider other)
    {
        if (other.GetComponent<ThreatValue>() != null)
        {
            captureLevel += other.GetComponent<ThreatValue>().enemy ? 0.01f : -0.01f;
            captureLevel = Mathf.Min(100.0f, Mathf.Max(0.0f, captureLevel));
            GetComponent<MeshRenderer>().material.color = new Color(captureLevel / 100, 1 - captureLevel / 100, 0.0f);
        }
    }
}
