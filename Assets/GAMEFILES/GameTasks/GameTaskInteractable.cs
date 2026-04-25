using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(XRGrabInteractable))]
public class GameTaskInteractable : MonoBehaviour
{
    private XRGrabInteractable grabInteractable;
    public Canvas canvas;
    public UnityEvent OnInventoryGrab;

    void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnGrabbed);
            grabInteractable.selectExited.AddListener(OnReleased);
        }
    }

    private void OnDestroy()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnGrabbed);
            grabInteractable.selectExited.RemoveListener(OnReleased);
        }
    }

    private void OnGrabbed(SelectEnterEventArgs args)
    {
        if (args.interactorObject is XRDirectInteractor)
            canvas.enabled = true;
        if (args.interactorObject is XRSocketInteractor)
            OnInventoryGrab?.Invoke();
    }

    private void OnReleased(SelectExitEventArgs args)
    {
        canvas.enabled = false;
    }
}
