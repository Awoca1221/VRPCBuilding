using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.LookDev;

public class ShowStep : MonoBehaviour
{

    [field: NonSerialized] public List<GameTask> Tasks { get; set; } = new List<GameTask>();
    [field: NonSerialized] public List<GameObject> Steps { get; set; } = new List<GameObject>();

    public Stage stage;
    private int enableTask;
    // Start is called before the first frame update
    void Start()
    {
        Transform transform = stage.GetComponent<Transform>();
        GameTask childTask;

        foreach (Transform child in transform)
        {
            childTask = child.GetComponent<GameTask>();
            Tasks.Add(childTask);
        }

        transform = GetComponent<Transform>();
        foreach (Transform child in transform)
        {
            Steps.Add(child.gameObject);
        }

        stage.completeTask.AddListener(OnComplete);
        stage.unCompleteTask.AddListener(OnUnComplete);

        enableTask = 0;
        Change();
    }

    void Change()
    {
        Steps[enableTask].SetActive(false);

        int n = Tasks.Count();
        for (int i = 0; i < n; i++)
        {
            if (!Tasks[i].isComplete)
            {
                enableTask = i;
                Steps[enableTask].SetActive(true);
                break;
            }
        }
    }

    void OnComplete()
    {
        Change();
    }

    void OnUnComplete()
    {
        Change();
    }
}
