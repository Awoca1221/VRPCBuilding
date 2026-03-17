using UnityEngine;
using System;

[RequireComponent(typeof(AudioManager))]
public class ConnectionPoint : MonoBehaviour
{
    public GameObject ConnectedDevice { get; private set; } = null;
    [Tooltip("id слота (уникальное значение между разъёмами одинакогово вида)")]
    public uint slotID = 1;

    private AudioManager audioManager;

    void Start()
    {
        audioManager = GetComponent<AudioManager>();
    }
    
    public void OnConnect(GameObject PCComponent, bool playSound = true)
    {
        ConnectedDevice = PCComponent;
        if (playSound) audioManager.PlayInsertSound();
    }
    
    public void OnDisconnect(bool playSound = true)
    {
        ConnectedDevice = null;
        if (playSound) audioManager.PlayEjectSound();
    }
}
