using UnityEngine;
using System;

public class ConnectionPoint : MonoBehaviour
{
    public GameObject ConnectedDevice { get; private set; } = null;
    [Tooltip("id слота (уникальное значение между разъёмами одинакогово вида)")]
    public uint slotID = 1;
    
    public void OnConnect(GameObject PCComponent)
    {
        ConnectedDevice = PCComponent;
    }
    
    public void OnDisconnect()
    {
        ConnectedDevice = null;
    }
}
