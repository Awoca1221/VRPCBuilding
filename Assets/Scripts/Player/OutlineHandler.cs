using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class OutlineHandler : MonoBehaviour
{
    private XRBaseInteractor baseInteractor;
    private XRInteractionManager interactionManager;
    private XRBaseInteractable target;
    private int oldLayer;
    private bool hover = false;
    private bool lineRendererWasDisabled = false;

    void Start()
    {
        baseInteractor = GetComponent<XRBaseInteractor>();
        interactionManager = GameObject.Find("XR Interaction Manager").GetComponent<XRInteractionManager>();
        baseInteractor.hoverEntered.AddListener(HoverEnter);
        baseInteractor.selectEntered.AddListener(SelectEnter);
        baseInteractor.selectExited.AddListener(SelectExit);
    }

    private void HoverEnter(HoverEnterEventArgs args)
    {
        if (!hover)
        {
            StartCoroutine(UpdateOutline());
            hover = true;
        }
    }

    // Метод отслеживает необходимость outline и останавливается при отсутствии valid targets
    private IEnumerator UpdateOutline()
    {
        List<IXRInteractable> targets = new();
        interactionManager.GetValidTargets(baseInteractor, targets);
        target = targets[0] as XRBaseInteractable;
        TryGetComponent(out LineRenderer line);
        if (!target.isSelected)
        {
            oldLayer = target.gameObject.layer;
            SetLayerRecursively(target.gameObject, 10);
        }
        
        while (true)
        {
            yield return null;
            if (baseInteractor.isSelectActive)
                continue;
            if (baseInteractor is XRRayInteractor)
            {
                if (!line.enabled && !lineRendererWasDisabled)
                {
                    SetLayerRecursively(target.gameObject, oldLayer);
                    lineRendererWasDisabled = true;
                    continue;
                }
                if (line.enabled && lineRendererWasDisabled)
                {
                    if (!target.isSelected)
                        SetLayerRecursively(target.gameObject, 10);
                    lineRendererWasDisabled = false;
                    continue;
                }
                if (!line.enabled)
                    continue;
            }
            interactionManager.GetValidTargets(baseInteractor, targets);
            if (targets.Count == 0)
            {
                SetLayerRecursively(target.gameObject, 0);
                break;
            }
            if (target.transform.gameObject != targets[0].transform.gameObject)
            {
                SetLayerRecursively(target.gameObject, oldLayer);
                target = targets[0] as XRBaseInteractable;
                oldLayer = target.gameObject.layer;
            }
            if (!target.isSelected)
                SetLayerRecursively(target.gameObject, 10);
        }
        hover = false;
    }

    void SetLayerRecursively(GameObject obj, int layer)
    {
        // подсветку и слой Instrument пропускаем
        if (obj == null || obj.name.Contains("_highlight") || oldLayer == 6) return;
        
        obj.layer = layer;
        
        for (int i = 0; i < obj.transform.childCount; i++)
        {
            SetLayerRecursively(obj.transform.GetChild(i).gameObject, layer);
        }
    }

    private void SelectEnter(SelectEnterEventArgs args)
    {
        IXRSelectInteractable obj = args.interactableObject;
        SetLayerRecursively(obj.transform.gameObject, oldLayer);
    }

    private void SelectExit(SelectExitEventArgs args)
    {
        IXRSelectInteractable obj = args.interactableObject;
        if (!target.isSelected)
                SetLayerRecursively(obj.transform.gameObject, 10);
    }
}
