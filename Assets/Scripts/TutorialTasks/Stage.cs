using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Stage : MonoBehaviour
{
    [field: SerializeField] public string StageName { get; set; } = "";
    [field: NonSerialized] public int StagePosition { get; set; } = 0;
    [field: NonSerialized] public int StageAmount { get; set; } = 0;
    [field: NonSerialized] public bool IsActive { get; set; } = false;
    [field: NonSerialized] public UnityEvent completeTask { get; } = new UnityEvent();
    [field: NonSerialized] public UnityEvent unCompleteTask { get; } = new UnityEvent();
    [field: SerializeField] public UnityEvent completeStage { get; set; } = new UnityEvent();

    [field: NonSerialized] public List<GameTask> GameTasks { get; set; } = new List<GameTask>();

    public void Start()
    {
        Transform transform = GetComponent<Transform>();
        GameTask childTask;
        foreach (Transform child in transform)
        {
            childTask = child.GetComponent<GameTask>();
            GameTasks.Add(childTask);
        }

        gameObject.SetActive(IsActive);
    }

    public override string ToString()
    {
        string out_string = "Tasks: ";
        foreach (var task in GameTasks)
        {
            out_string += task.TaskId.ToString() + ", ";
        }
        out_string = out_string.Substring(0, out_string.Length - 2) + ". ";

        // foreach (var task in CompletedGameTasks)
        // {
        //     out_string += task.TaskId.ToString() + ", ";
        // }
        // out_string = out_string.Substring(0, out_string.Length - 2) + ". ";
        return out_string;
    }

    public void TryCompleteTask(string taskId)
    {
        foreach (var task in GameTasks)
        {
            if (task.TaskId == taskId)
            {
                task.Complete();
                completeTask.Invoke();
                if (this.AreAllTasksCompleted())
                    completeStage.Invoke();
            }
        }
    }

    public void TryUncompleteTask(string taskId)
    {
        foreach (var task in GameTasks)
        {
            if (task.TaskId == taskId)
            {
                task.Uncomplete();
                unCompleteTask.Invoke();
            }
        }
    }

    public bool AreAllTasksCompleted()
    {
        foreach (GameTask task in GameTasks)
        {
            if (!task.isComplete)
                return false;
        }
        return true;
    }

    public void ClearTask(GameTask task)
    {
        GameTasks.Remove(task);
    }

    public void ClearTasks(List<GameTask> tasks)
    {
        foreach(var task in tasks)
        {
            ClearTask(task);
        }
    }

    public void ClearAllTasks()
    {
        GameTasks.Clear();
    }

    public void AddTasks(List<GameTask> newGameTasks)
    {
        foreach (var task in newGameTasks)
        {
            AddTask(task);
        }
    }

    public void AddTask(GameTask newGameTask)
    {
        GameTasks.Add(newGameTask);
    }
}
