using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

public class ThermalPaste : MonoBehaviour
{
    public enum types
    {
        paste,
        cloth
    };
    public types type;
    public Transform needleOrCloth;
    private XRGrabInteractable interact;
    private Collider[] colliders;
    private Transform trans;
    private ChangeMaterial changeScript;
    private GameObject obj;
    private ActivateEvent evnt;

    [field: SerializeField] private UnityEvent OnUseEvents { get; set; } = null;
    
    // Start is called before the first frame update
    void Start()
    {
        evnt = new ActivateEvent();
        trans = GetComponent<Transform>();
        interact = GetComponent<XRGrabInteractable>();
        interact.activated = evnt;
        evnt.AddListener(Use);
    }

    public void Use(ActivateEventArgs arg0)
    {
        colliders = Physics.OverlapSphere(needleOrCloth.position, 0.05f);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].gameObject.GetComponent<ChangeMaterial>() != null)
            {
                obj = colliders[i].gameObject;
                changeScript = obj.GetComponentInParent<ChangeMaterial>();
                if (changeScript.changed && type == types.cloth)
                {
                    changeScript.ChangeBack();
                    OnUseEvents.Invoke();
                    break;
                } else if (type == types.paste)
                {
                    changeScript.Change();
                    OnUseEvents.Invoke();
                    break;
                }
            }
        }
    }
}
