using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Image = UnityEngine.UI.Image;

public class HoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Tooltip("Время до вызова onFinishEvent (0 для бесконечного зажатия кнопки)")]
    public float targetTime = 0.6f;
    [Tooltip("Заполнение прогресса (работает только при выставлении targetTime)")]
    public Image fillImage;
    public Image backgroundImage;
    [field: SerializeField]
    public bool IsDisabled { get; private set; } = false;
    public UnityEvent onFinishEvent;
    public bool IsHolding { get; private set; } = false;

    private bool IsProgressButton => targetTime > 0;
    private float holdTime = 0;

    private void SetImageAlpha(Image image, float alpha)
    {
        Color tempColor = image.color;
        tempColor.a = alpha;
        image.color = tempColor;
    }

    void Start()
    {
        if (backgroundImage == null) return;

        if (IsDisabled)
        {
            SetImageAlpha(backgroundImage, 0.5f);
        }
        else
        {
            SetImageAlpha(backgroundImage, 1f);
        }
    }

    public void SetIsDisabled(bool status)
    {
        if (backgroundImage == null) return;
        if (status)
        {
            SetImageAlpha(backgroundImage, 0.5f);
            IsDisabled = status;
        }
        else
        {
            SetImageAlpha(backgroundImage, 1f);
            IsDisabled = status;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (IsDisabled) return;

        IsHolding = true;
        if (IsProgressButton)
        {
            fillImage.fillAmount = 0;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        IsHolding = false;
        holdTime = 0;
        if (IsProgressButton)
        {
            fillImage.fillAmount = 0;
        }
    }

    void Update()
    {
        if (IsHolding)
        {
            holdTime += Time.deltaTime;
            if (IsProgressButton)
            {
                float fillAmount = Mathf.Clamp01(holdTime / targetTime);
                fillImage.fillAmount = fillAmount;
                if (holdTime >= targetTime)
                {
                    onFinishEvent?.Invoke();
                    IsHolding = false;
                    fillImage.fillAmount = 0;
                }
            }
        }
    }
}
