using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    [field: SerializeField] private Transform TaskViewer { get; set; } = null;
    [field: NonSerialized] private List<Stage> Stages { get; set; } = new List<Stage>();
    [field: SerializeField] private Stage CurrentStage { get; set; } = null;

    private void Start()
    {
        Stage childStage;
        foreach (Transform child in TaskViewer)
        {
            childStage = child.GetComponent<Stage>();
            Stages.Add(childStage);
        }
        if (Stages.Count <= 0)
        {
            Debug.Log("what");
            return;
        }
        int len = Stages.Count;
        for (int i = 0; i < len; i++)
        {
            Stages[i].StagePosition = i + 1;
            Stages[i].StageAmount = len;
        }
        if (CurrentStage == null)
        {
            CurrentStage = Stages[0];
        }
        CurrentStage.gameObject.SetActive(true);
        CurrentStage.IsActive = true;
    }

    public void ActivateStage(Stage stage)
    {
        StageSetActive(stage);
    }

    private void StageSetActive(Stage stageToActivate)
    {
        if (stageToActivate == null || !Stages.Contains(stageToActivate))
        {
            return;
        }
        foreach (var stage in Stages)
        {
            stage.gameObject.SetActive(false);
            stage.IsActive = false;
        }
        stageToActivate.gameObject.SetActive(true);
        stageToActivate.IsActive = true;
        CurrentStage = stageToActivate;
    }

    public Stage GetCurrentStage()
    {
        return CurrentStage;
    }

    public void TryStageAfterIfCopmleted(Stage stageSentByTask)
    {
        if (!Stages.Contains(stageSentByTask)) { return; }
        if (CurrentStage.StagePosition != stageSentByTask.StagePosition) { return; }

        Stage stageToActivate = null;
        for (int i = stageSentByTask.StagePosition - 1; i < Stages.Count - 1; i++)
        {
            if (Stages[i].AreAllTasksCompleted())
                stageToActivate = Stages[i + 1];
            else
                break;
        }
        if (stageToActivate == null) { return; }
        StageSetActive(stageToActivate);
    }

    public void TryStageIfUncompleted(Stage stageSentByTask)
    {
        if (!Stages.Contains(stageSentByTask)) { return; }
        if (CurrentStage.StagePosition <= stageSentByTask.StagePosition || CurrentStage.StagePosition <= 1) { return; }

        for (int i = 0; i < stageSentByTask.StagePosition; i++)
        {
            if (!Stages[i].AreAllTasksCompleted())
            {
                StageSetActive(Stages[i]);
                return;
            }
        }
    }
}
