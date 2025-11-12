using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerManagerEvents : MonoBehaviour
{
    private readonly static UnityEvent<int> ChangeSceneListener = new();

    public void RaiseChangeScene(int sceneIndex) {
        ChangeSceneListener.Invoke(sceneIndex);
    }

    public void RegisterChangeScene(UnityAction<int> listener)
    {
        ChangeSceneListener.AddListener(listener);
    }

    public void UnregisterChangeScene(UnityAction<int> listener)
    {
        ChangeSceneListener.RemoveListener(listener);
    }
}
