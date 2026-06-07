using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(XRGrabInteractable))]
public class ThermalPaste : MonoBehaviour
{
    public enum Types
    {
        paste,
        cloth
    };
    public Types type;
    public Transform needleOrCloth;
    private XRGrabInteractable interact;
    private Collider[] colliders;

    [field: SerializeField] private UnityEvent OnUseEvents { get; set; } = null;
    
    // Start is called before the first frame update
    void Start()
    {
        interact = GetComponent<XRGrabInteractable>();
        interact.activated.AddListener(Use);
    }

    public void Use(ActivateEventArgs arg0)
    {
        colliders = Physics.OverlapSphere(needleOrCloth.position, 0.05f);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].attachedRigidbody == null) continue; 
            if (colliders[i].attachedRigidbody.gameObject.TryGetComponent<CPUPasteState>(out var changeScript))
            {
                if (changeScript.IsPasteActive && type == Types.cloth)
                {
                    changeScript.Deactivate();
                    OnUseEvents.Invoke();
                    break;
                } else if (type == Types.paste)
                {
                    changeScript.Activate();
                    OnUseEvents.Invoke();
                    break;
                }
            }
        }
    }
}
