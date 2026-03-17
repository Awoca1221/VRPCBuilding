using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class GrabbableCommon : MonoBehaviour
{
    private XRGrabInteractable interactable;
    [field: NonSerialized] public bool isGrabbed { get; set; } = false;


    private void Start()
    {
        interactable = GetComponent<XRGrabInteractable>();
        interactable.selectEntered.AddListener(Grab);
        interactable.selectExited.AddListener(Ungrab);
    }

    private void Grab(SelectEnterEventArgs args)
    {
        isGrabbed = true;
    }

    private void Ungrab(SelectExitEventArgs args)
    {
        isGrabbed = false;
    }
}
