using UnityEngine;
using TMPro;
using System;

public class ResultItemUI : MonoBehaviour {
    public TMP_Text timeText;
    public TMP_Text dateText;
    public HoldButton deleteButton;

    public void SetData(ResultEntry entry, Action onDelete) {
        Timer time = timeText.GetComponent<Timer>();
        if (time) {
            time.SetTimer(entry.timeSeconds);
        }
        dateText.text = entry.dateTime;
        deleteButton.onFinishEvent.RemoveAllListeners();
        deleteButton.onFinishEvent.AddListener(() => onDelete?.Invoke());
    }
}
