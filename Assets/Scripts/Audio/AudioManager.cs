using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes.Test;
using UnityEngine;
using UnityEngine.Audio;

[RequireComponent(typeof(AudioSource), typeof(AudioLowPassFilter), typeof(AudioHighPassFilter))]
public class AudioManager : MonoBehaviour
{
    [Tooltip("Ссылка на пресет звуков (default if null)")]
    public AudioPreset audioPreset;
    [Tooltip("Ссылка на миксер (default if null)")]
    public AudioMixer audioMixer;

    private AudioSource audioSource;
    private AudioLowPassFilter lowPassFilter;
    private AudioHighPassFilter highPassFilter;
    
    void Start()
    {
        if (audioPreset == null)
            audioPreset = Resources.Load<AudioPreset>("Audio/AudioPreset");
        if (audioMixer == null)
            audioMixer = Resources.Load<AudioMixer>("Audio/AudioMixer");
        audioSource = GetComponent<AudioSource>();
        lowPassFilter = GetComponent<AudioLowPassFilter>();
        highPassFilter = GetComponent<AudioHighPassFilter>();
        SetupAudioSource();
    }

    private void SetupAudioSource()
    {
        audioSource.outputAudioMixerGroup = audioMixer.FindMatchingGroups("SFX")[0];
        audioSource.playOnAwake = false;
        audioSource.loop = true;
        audioSource.spatialBlend = 1f; // Полностью 3D звук
        audioSource.volume = 1f; // Базовая громкость
        audioSource.minDistance = 1f;
        audioSource.maxDistance = 5f;
        
        lowPassFilter.cutoffFrequency = 22000f;
        highPassFilter.cutoffFrequency = 10f;
    }

    private void PlaySound(AudioClip[] audios, float highPassCutoff = 10f, float lowPassCutoff = 22000f, bool isLoop = false)
    {
        // Случайный клип
        AudioClip randomClip = audios[Random.Range(0, audios.Length)];
        
        // Случайная громкость
        float randomVolume = Random.Range(audioPreset.volMin, audioPreset.volMax);
        
        // Применяем изменение частот звука
        highPassFilter.cutoffFrequency = highPassCutoff;
        lowPassFilter.cutoffFrequency = lowPassCutoff;

        // Воспроизведение
        if (isLoop)
        {
            audioSource.clip = randomClip;
            audioSource.Play();
        }
        else
        {
            audioSource.PlayOneShot(randomClip, randomVolume);
        }
    }

    public void PlayInsertSound()
    {
        if (audioPreset.insertSounds == null || audioPreset.insertSounds.Length == 0) return;
        PlaySound(audioPreset.insertSounds, 1000f);
    }

    public void PlayEjectSound()
    {
        if (audioPreset.ejectSounds == null || audioPreset.ejectSounds.Length == 0) return;
        PlaySound(audioPreset.ejectSounds, 1000f);
    }

    public void PlayOpenDoorSound()
    {
        if (audioPreset.openDoorSounds == null || audioPreset.openDoorSounds.Length == 0) return;
        PlaySound(audioPreset.openDoorSounds);
    }

    public void PlayCloseDoorSound()
    {
        if (audioPreset.closeDoorSounds == null || audioPreset.closeDoorSounds.Length == 0) return;
        PlaySound(audioPreset.closeDoorSounds);
    }

    public void StartScrewLoopSound()
    {
        if (audioPreset.screwSounds == null || audioPreset.screwSounds.Length == 0) return;
        PlaySound(audioPreset.screwSounds, isLoop: true);
    }

    public void SetVolume(float value)
    {
        audioSource.volume = value;
    }

    public void StopSound()
    {
        if (audioSource.isPlaying) audioSource.Stop();
    }
}
