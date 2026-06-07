using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class GameTask : MonoBehaviour
{
    [field: SerializeField] public string TaskId { get; set; } = null;
    [field: SerializeField] public bool DefaultPreset { get; set; } = true;
    [field: NonSerialized] public bool isComplete { get; set; } = false;

    [field: SerializeField] private UnityEvent OnCompleteEvents { get; set; } = null;
    [field: SerializeField] private UnityEvent OnUncompleteEvents { get; set; } = null;
    [field: NonSerialized] private ChangeText ChangeText { get; set; } = null;
    [field: NonSerialized] private Stage Stage { get; set; } = null;
    [field: NonSerialized] private StageManager StageManager { get; set; } = null;

    private void Start()
    {
        if (DefaultPreset)
        {
            ChangeText = GetComponent<ChangeText>();
            Transform transform = GetComponent<Transform>();
            Stage = transform.parent.GetComponent<Stage>();
            for (int i = 0; i < 3; i++)
                transform = transform.parent;
            foreach (Transform child in transform)
                if (child.name == "Extensions")
                {
                    foreach (Transform child2 in child)
                        if (child2.name == "Stage Manager")
                        {
                            StageManager = child2.GetComponent<StageManager>();
                            break;
                        }
                    break;
                }
            
            OnCompleteEvents.AddListener(DefaultOnCompleteEvent);
            OnUncompleteEvents.AddListener(DefaultOnUncompleteEvent);
        }
    }

    private void DefaultOnCompleteEvent()
    {
        ChangeText.TurnOn();
        StageManager.TryStageAfterIfCopmleted(Stage);
    }

    private void DefaultOnUncompleteEvent()
    {
        ChangeText.TurnOff();
        StageManager.TryStageIfUncompleted(Stage);
    }

    public void Complete()
    {
        isComplete = true;
        OnCompleteEvents?.Invoke();
    }

    public void Uncomplete()
    {
        isComplete = false;
        OnUncompleteEvents?.Invoke();
    }

    public override string ToString()
    {
        return $"{TaskId} - {isComplete}";
    }
}
