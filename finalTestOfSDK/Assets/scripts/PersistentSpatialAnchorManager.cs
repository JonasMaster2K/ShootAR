using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction;
using System;

public class PersistentOVRSpatialAnchorManager : MonoBehaviour
{
    public GameObject anchor;
    private void Update() {
        if(OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch))
            SpawnAnchor();
    }

    public void SpawnAnchor() {
        GameObject obj = Instantiate(anchor, OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch), OVRInput.GetLocalControllerRotation(OVRInput.Controller.RTouch));
        obj.AddComponent<OVRSpatialAnchor>();
    }
}