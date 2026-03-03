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
    
    public void OnConnect(GameObject PCComponent)
    {
        ConnectedDevice = PCComponent;
        audioManager.PlayInsertSound();
    }
    
    public void OnDisconnect()
    {
        ConnectedDevice = null;
        audioManager.PlayEjectSound();
    }
}
