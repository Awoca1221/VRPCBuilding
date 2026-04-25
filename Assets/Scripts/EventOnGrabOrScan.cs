using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

public class EventOnGrabOrScan : MonoBehaviour
{
    public UnityEvent OnGrab;
    public UnityEvent OnScan;
    private XRGrabInteractable interactable;

    // Start is called before the first frame update
    void Start()
    {
        interactable = GetComponent<XRGrabInteractable>();
        interactable.selectEntered.AddListener(TaskComplete);
    }

    private void TaskComplete(SelectEnterEventArgs args)
    {
        OnGrab?.Invoke();
    }
}
