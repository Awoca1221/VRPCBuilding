using UnityEngine;

[CreateAssetMenu(fileName = "AudioPreset", menuName = "Audio/Audio Preset", order = 1)]
public class AudioPreset : ScriptableObject
{
    public AudioClip[] insertSounds;
    public AudioClip[] ejectSounds;
    public AudioClip[] openDoorSounds;
    public AudioClip[] closeDoorSounds;

    [Header("Volume Range")]
    [Range(0.5f, 1.5f)] public float volMin = 0.6f;
    [Range(0.5f, 1.5f)] public float volMax = 1.1f;
}
