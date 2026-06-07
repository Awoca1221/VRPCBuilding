using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskManager : MonoBehaviour
{
    [field: SerializeField] private List<TaskViewer> TaskViewers { get; set; } = null;

    public void CompleteTask(string taskId)
    {
        foreach (var taskViewer in TaskViewers)
        {
            taskViewer.TryCompleteTask(taskId);
        }
    }

    public void UncompleteTask(string taskId)
    {
        foreach (var taskViewer in TaskViewers)
        {
            taskViewer.TryUncompleteTask(taskId);
        }
    }
} 