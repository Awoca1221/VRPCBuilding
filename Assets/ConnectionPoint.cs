using UnityEngine;
using UnityEngine.Audio;
using System;
using Random = UnityEngine.Random;

[RequireComponent(typeof(AudioSource))]
public class ConnectionPoint : MonoBehaviour
{
    [Tooltip("Ссылка на пресет звуков (default if null)")]
    public AudioPreset audioPreset;
    [Tooltip("Ссылка на миксер (обычно пусто)")]
    public AudioMixer audioMixer;
    
    private AudioSource audioSource;
    public GameObject ConnectedDevice { get; private set; } = null;

    void Start()
    {
        if (audioPreset == null)
            audioPreset = Resources.Load<AudioPreset>("Audio/AudioPreset");
        if (audioMixer == null)
            audioMixer = Resources.Load<AudioMixer>("Audio/AudioMixer");
        audioSource = GetComponent<AudioSource>();
        SetupAudioSource();
    }
    
    public void OnConnect(GameObject PCComponent)
    {
        ConnectedDevice = PCComponent;
        PlayInsertSound();
    }
    
    public void OnDisconnect()
    {
        ConnectedDevice = null;
        PlayEjectSound();
    }

    private void SetupAudioSource()
    {
        audioSource.outputAudioMixerGroup = audioMixer.FindMatchingGroups("SFX")[0];
        audioSource.spatialBlend = 1.0f;  // Полностью 3D звук
        audioSource.volume = 1.0f;        // Базовая громкость
        //audioSource.dopplerLevel = 0.5f;   // Доплер для движения
        //audioSource.spread = 15f;         // Распространение звука
        audioSource.spatialBlend = 1f;
        audioSource.minDistance = 0.5f;
        audioSource.maxDistance = 5f;
        //audioSource.rolloffMode = AudioRolloffMode.Linear; // Линейное затухание
    }
    
    public void PlayInsertSound()
    {
        if (audioPreset?.insertSounds == null || audioPreset.insertSounds.Length == 0) return;
        
        // Случайный клип
        AudioClip randomClip = audioPreset.insertSounds[Random.Range(0, audioPreset.insertSounds.Length)];
        
        // Случайная громкость
        float randomVolume = Random.Range(audioPreset.insertVolMin, audioPreset.insertVolMax);
        
        // Воспроизведение
        audioSource.PlayOneShot(randomClip, randomVolume);
    }
    
    public void PlayEjectSound()
    {
        if (audioPreset?.ejectSounds == null || audioPreset.ejectSounds.Length == 0) return;
        
        // Случайный клип
        AudioClip randomClip = audioPreset.ejectSounds[Random.Range(0, audioPreset.ejectSounds.Length)];
        
        // Случайная громкость (чуть тише)
        float randomVolume = Random.Range(audioPreset.ejectVolMin, audioPreset.ejectVolMax);
        
        // Воспроизведение
        audioSource.PlayOneShot(randomClip, randomVolume);
    }
}
