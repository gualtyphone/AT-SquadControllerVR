using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class Minimap : VRTK_InteractableObject {
    public GameObject playerHipHolster;
    public Camera camera;
    override protected void Update()
    {
        base.Update();
        if(!IsGrabbed())
        {
            transform.position = playerHipHolster.transform.position;
            transform.rotation = playerHipHolster.transform.rotation;
            //transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        }
    }
}
