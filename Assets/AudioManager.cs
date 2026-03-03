using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
    [Tooltip("Ссылка на пресет звуков (default if null)")]
    public AudioPreset audioPreset;
    [Tooltip("Ссылка на миксер (default if null)")]
    public AudioMixer audioMixer;

    private AudioSource audioSource;

    void Start()
    {
        if (audioPreset == null)
            audioPreset = Resources.Load<AudioPreset>("Audio/AudioPreset");
        if (audioMixer == null)
            audioMixer = Resources.Load<AudioMixer>("Audio/AudioMixer");
        audioSource = GetComponent<AudioSource>();
        SetupAudioSource();
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

    private void PlaySound(AudioClip[] audios)
    {
        // Случайный клип
        AudioClip randomClip = audios[Random.Range(0, audios.Length)];
        
        // Случайная громкость
        float randomVolume = Random.Range(audioPreset.volMin, audioPreset.volMax);
        
        // Воспроизведение
        audioSource.PlayOneShot(randomClip, randomVolume);
    }

    public void PlayInsertSound()
    {
        if (audioPreset?.insertSounds == null || audioPreset.insertSounds.Length == 0) return;
        PlaySound(audioPreset.insertSounds);
    }

    public void PlayEjectSound()
    {
        if (audioPreset?.ejectSounds == null || audioPreset.ejectSounds.Length == 0) return;
        PlaySound(audioPreset.ejectSounds);
    }

    public void PlayOpenDoorSound()
    {
        if (audioPreset?.openDoorSounds == null || audioPreset.openDoorSounds.Length == 0) return;
        PlaySound(audioPreset.openDoorSounds);
    }

    public void PlayCloseDoorSound()
    {
        if (audioPreset?.closeDoorSounds == null || audioPreset.closeDoorSounds.Length == 0) return;
        PlaySound(audioPreset.closeDoorSounds);
    }
}
