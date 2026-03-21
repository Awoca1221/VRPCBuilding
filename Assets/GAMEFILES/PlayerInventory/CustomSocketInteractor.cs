using UnityEngine.XR.Interaction.Toolkit;

public class CustomSocketInteractor : XRSocketInteractor
{
    public override bool CanSelect(IXRSelectInteractable interactable)
    {
        if (base.CanSelect(interactable))
        {
            if (interactable.transform.TryGetComponent<AttachObject>(out var attachObject))
            {
                if (attachObject.objIsAttached)
                {
                    return false;
                }

                return true;
            }

            return true;
        }

        return false;
    }
}
