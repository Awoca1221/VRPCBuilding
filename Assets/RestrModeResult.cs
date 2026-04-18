using TMPro;
using UnityEngine;
using System;

public class RestrModeResult : MonoBehaviour
{
    public TMP_Text text;
    public HoldButton deleteButton;

    public void SetData(RestrictModeEntry entry, Action onDelete) {
        text.text = $"Ограничение: {entry.type}\nТребуемое значение: {entry.restrValue}\nЗначение сборки: {entry.buildValue}\n<color=#ADD8E6>Итоговый счёт: {entry.points}";
        deleteButton.onFinishEvent.RemoveAllListeners();
        deleteButton.onFinishEvent.AddListener(() => onDelete?.Invoke());
    }
}
