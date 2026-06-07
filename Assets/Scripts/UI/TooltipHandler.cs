using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class TooltipHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject tooltipPanel;
    [Tooltip("Показывать подсказку при наведении")]
    public bool isEnabled = true;

    private int pointerCount = 0;
    private bool IsHovered => pointerCount > 0;
    private bool isShowInProgress = false;
    private Button button;

    void Start()
    {
        button = GetComponent<Button>();
    }

    // 1 вариант работы: появление подсказки при наведении на кнопку
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
        
        if (IsHovered && isEnabled && !button.interactable)
        {
            tooltipPanel.SetActive(true);
        }
        else
        {
            tooltipPanel.SetActive(false);
        }
    }

    // 2 вариант работы: вызов появления подсказки на определённое время
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
