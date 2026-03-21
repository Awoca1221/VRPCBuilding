using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject tooltipPanel;
    public bool isEnabled = true;

    private int pointerCount = 0;
    private bool IsHovered => pointerCount > 0;
    private bool isShowInProgress = false;

    public void OnPointerEnter(PointerEventData eventData)
    {
        pointerCount += 1;
        UpdatePanelStatus();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        pointerCount -= 1;
        UpdatePanelStatus();
    }

    private void UpdatePanelStatus()
    {
        if (isShowInProgress) return;
        
        if (IsHovered && isEnabled)
        {
            tooltipPanel.SetActive(true);
        }
        else
        {
            tooltipPanel.SetActive(false);
        }
    }

    public void ShowTooltip(float seconds)
    {
        if (isShowInProgress) return;

        StartCoroutine(StartTooltipCoroutine(seconds));
    }

    private IEnumerator StartTooltipCoroutine(float seconds)
    {
        isShowInProgress = true;
        tooltipPanel.SetActive(true);
        yield return new WaitForSeconds(seconds);
        tooltipPanel.SetActive(false);
        isShowInProgress = false;
    }
}
