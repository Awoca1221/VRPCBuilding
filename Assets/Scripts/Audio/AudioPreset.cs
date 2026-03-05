using UnityEngine;

[CreateAssetMenu(fileName = "AudioPreset", menuName = "Audio/Audio Preset", order = 1)]
public class AudioPreset : ScriptableObject
{
    public AudioClip[] insertSounds;
    public AudioClip[] ejectSounds;
    public AudioClip[] openDoorSounds;
    public AudioClip[] closeDoorSounds;
    public AudioClip[] screwSounds;

    [Header("Volume Range")]
    [Range(0.5f, 1.5f)] public float volMin = 0.8f;
    [Range(0.5f, 1.5f)] public float volMax = 1f;
}
