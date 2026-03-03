using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class AttachObjectCable : AttachObject
{
    [System.Obsolete]
    void FixedUpdate()
    {
        if (interactable.isSelected){
            if (Vector3.Distance(attachPoint.transform.position, interactable.interactorsSelecting[0].transform.position) > 0.4f)
                interactable.interactionManager.CancelInteractableSelection(interactable);
        }
        if (attachPoint.GetComponent<FixedJoint>() != null && attachPoint.GetComponent<FixedJoint>().connectedBody != null)
            if (Vector3.Distance(attachPoint.transform.position, attachPoint.GetComponent<FixedJoint>().connectedBody.transform.position) > 0.4f)
            {
                checkCollider.tag = attachPoint.tag;
                Destroy(attachPoint.GetComponent<FixedJoint>());
                EndHighlight();
                objIsAttached = false;
                checkCollider = null;
            }
    }

    protected override void TryAttach()
    {   
        if (checkCollider != null)
        {
            // Сохранение прошлой иерархии и смена на новую
            Transform oldPlace = attachPoint.transform.parent;
            attachPoint.transform.SetParent(checkCollider.gameObject.transform);
            attachPoint.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            
            // Возврат к прошлой иерархии
            attachPoint.transform.SetParent(oldPlace);
            
            // Скрепление объекта с разъёмом через FixedJoint
            attachPoint.AddComponent<FixedJoint>();
            attachPoint.GetComponent<FixedJoint>().connectedBody = checkCollider.GetComponentInParent<Rigidbody>();
            attachPoint.GetComponent<FixedJoint>().enablePreprocessing = false;
            
            checkCollider.tag = "Unavailable";
            objIsAttached = true;
            checkCollider.GetComponent<SetupPoint>().SetSecured();

            if (interactor != null)
                interactionManager.SelectExit(interactor, interactable);

            OnConnectEvents.Invoke();
        }
    }

    public override void TryUnattach()
    {
        if (objIsAttached)
        {
            if (checkCollider != null) checkCollider.tag = tag;
            Destroy(attachPoint.GetComponent<FixedJoint>());
            objIsAttached = false;
            checkCollider.GetComponent<SetupPoint>().SetUnsecured();
            OnDisconnectEvents.Invoke();
        }
    }

    protected override void OnTriggerEnter(Collider collider)
    {
        if (objIsAttached) return;

        if (!collider.gameObject.CompareTag(tag))
        {
            return;
        }

        StartHighlight(collider);
        checkCollider = collider;
    }

    protected override void OnTriggerStay(Collider collider)
    {
        if (_highlightParent == null || _currentMatForHightlight == wrong)
            return;
        
        if (objIsAttached || interactor == null)
        {
            if (_currentMatForHightlight != invis)
                ChangeHighlightColor(invis);
        }
        else
        {
            if (_currentMatForHightlight != correct)
                ChangeHighlightColor(correct);
        }
    }

    protected override void OnTriggerExit(Collider collider)
    {
        if (collider == _currentColliderForHighlight) EndHighlight();

        if (checkCollider != null && !objIsAttached && checkCollider.gameObject == collider.gameObject)
        {
            checkCollider = null;
        }
    }
}
