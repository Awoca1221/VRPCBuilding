using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Image = UnityEngine.UI.Image;

public class HoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public float targetTime = 0.6f;
    public Image fillImage;
    public UnityEvent onFinishEvent;

    private float holdTime = 0;
    private bool isHolding = false;

    public void OnPointerDown(PointerEventData eventData)
    {
        isHolding = true;
        holdTime = 0;
        fillImage.fillAmount = 0;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isHolding = false;
        fillImage.fillAmount = 0;
    }

    void Update()
    {
        if (isHolding)
        {
            holdTime += Time.deltaTime;
            float fillAmount = Mathf.Clamp01(holdTime / targetTime);
            fillImage.fillAmount = fillAmount;
            if (holdTime >= targetTime)
            {
                onFinishEvent.Invoke();
                isHolding = false;
                fillImage.fillAmount = 0;
            }
        }
    }
}
