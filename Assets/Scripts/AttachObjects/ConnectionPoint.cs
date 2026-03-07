using UnityEngine;
using UnityEngine.Audio;
using System;
using Random = UnityEngine.Random;

[RequireComponent(typeof(AudioManager))]
public class ConnectionPoint : MonoBehaviour
{
    public GameObject ConnectedDevice { get; private set; } = null;

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
