using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CheckMultipleConnections : MonoBehaviour
{
    [field: SerializeField] public List<GameObject> Objects { get; set; } = new List<GameObject>();
    [field: NonSerialized] private Dictionary<GameObject, bool> Connections { get; set; } = new Dictionary<GameObject, bool>();

    [field: SerializeField] private UnityEvent OnAllConnectEvents { get; set; } = null;
    [field: SerializeField] private UnityEvent OnDisconnectEvents { get; set; } = null;

    private void Start()
    {
        foreach (var obj in Objects)
        {
            Connections.Add(obj, false);
        }
    }

    public void Restart()
    {
        Connections.Clear();
        foreach (var obj in Objects)
        {
            Connections.Add(obj, false);
        }
    }

    public void ConnectObject(GameObject obj)
    {
        Connections[obj] = true;
        if (AreAllConnected())
        {
            OnAllConnectEvents?.Invoke();
        }
    }

    public void DisconnectObject(GameObject obj)
    {
        Connections[obj] = false;
        OnDisconnectEvents?.Invoke();
    }

    public bool AreAllConnected()
    {
        foreach (var (_, isConnected) in Connections)
        {
            if (!isConnected)
                return false;
        }
        return true;
    }
}
