using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScreenCopycat : MonoBehaviour
{

    [SerializeField]
    GameObject cameraToCopy;

    // Use this for initialization
    void Start()
    {
        GetComponent<Camera>().farClipPlane = cameraToCopy.GetComponent<Camera>().farClipPlane;
        GetComponent<Camera>().nearClipPlane = cameraToCopy.GetComponent<Camera>().nearClipPlane;
    }

    // Update is called once per frame
    void Update()
    {
        GetComponent<Camera>().cullingMask = cameraToCopy.GetComponent<Camera>().cullingMask;
        //GetComponent<Skybox>().material = cameraToCopy.GetComponent<Skybox>().material;

    }
}