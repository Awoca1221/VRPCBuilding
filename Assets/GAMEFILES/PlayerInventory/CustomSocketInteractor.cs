using System.Collections.Generic;
using System.Collections;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine;

public class CustomSocketInteractor : XRSocketInteractor
{
    private static WaitForSeconds _waitForSeconds0_1 = new(0.1f);
    private readonly List<XRGrabInteractable> allowedToSelect = new();

    public override bool CanSelect(IXRSelectInteractable interactable)
    {
        if (base.CanSelect(interactable) && interactable is XRGrabInteractable grabInteractable)
        {
            if (allowedToSelect.Contains(grabInteractable))
            {
                if (interactable.transform.TryGetComponent<AttachObject>(out var attachObject))
                {
                    if (attachObject.objIsAttached)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        return false;
    }

    public override bool CanHover(IXRHoverInteractable interactable)
    {
        if (base.CanHover(interactable) && interactable is XRGrabInteractable grabInteractable)
        {
            if (grabInteractable.isSelected)
            {
                return true;
            }
        }

        return false;
    }

    protected override void OnHoverEntering(HoverEnterEventArgs args)
    {
        base.OnHoverEntering(args);
        allowedToSelect.Add((XRGrabInteractable)args.interactableObject);
    }

    protected override void OnHoverExiting(HoverExitEventArgs args)
    {
        base.OnHoverExiting(args);
        StartCoroutine(DeleteAllowedObj((XRGrabInteractable)args.interactableObject));
    }

    private IEnumerator DeleteAllowedObj(XRGrabInteractable interactable)
    {
        yield return _waitForSeconds0_1;
        allowedToSelect.Remove(interactable);
    }
}
