using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TaskViewer : MonoBehaviour
{
    [field: NonSerialized] public List<Stage> Stages { get; set; } = new List<Stage>();
    [field: NonSerialized] private Dictionary<string, Stage> id_to_stage = new Dictionary<string, Stage>();

    private void Start()
    {
        Transform transform = GetComponent<Transform>();
        Stage childStage;

        foreach (Transform child in transform)
        {
            childStage = child.GetComponent<Stage>();
            Stages.Add(childStage);
        }

        foreach (Stage stage in Stages)
        {
            Transform transformStage = stage.GetComponent<Transform>();
            GameTask childTask;
            foreach (Transform child in transformStage)
            {
                childTask = child.GetComponent<GameTask>();
                id_to_stage.Add(childTask.TaskId, stage);
            }
        }
    }

    public void TryCompleteTask(string taskId)
    {
        Stage stage = id_to_stage[taskId];
        if (stage == null)
        {
            Debug.Log("No such task in this TaskViewer");
            return;
        }
        stage.TryCompleteTask(taskId);
    }

    public void TryUncompleteTask(string taskId)
    {
        Stage stage = id_to_stage[taskId];
        if (stage == null)
        {
            Debug.Log("No such task in this TaskViewer");
            return;
        }
        stage.TryUncompleteTask(taskId);
    }

    public bool AreAllTasksCompleted()
    {
        foreach (var stage in Stages)
        {
            foreach (var task in stage.GameTasks)
            {
                if (!task.isComplete)
                    return false;
            }
        }
        return true;
    }
}
