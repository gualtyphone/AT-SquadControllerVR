using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollowWithEasing : MonoBehaviour
{

    [SerializeField]
    GameObject CameraToFollow;

    [SerializeField]
    [Range(0.00001f, 0.1f)]
    float easing = 0.03f;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector3.Lerp(transform.position, CameraToFollow.transform.position, 20.0f);
        transform.rotation = Quaternion.Slerp(transform.rotation, CameraToFollow.transform.rotation, easing);

    }
}