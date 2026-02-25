using UnityEngine;

[CreateAssetMenu(fileName = "AudioPreset", menuName = "Audio/Audio Preset", order = 1)]
public class AudioPreset : ScriptableObject
{
    public AudioClip[] insertSounds;
    public AudioClip[] ejectSounds;

    [Header("Volume Ranges")]
    [Range(0.5f, 1.5f)] public float insertVolMin = 0.7f;
    [Range(0.5f, 1.5f)] public float insertVolMax = 1.2f;
    [Range(0.5f, 1.5f)] public float ejectVolMin = 0.6f;
    [Range(0.5f, 1.5f)] public float ejectVolMax = 1.1f;
}
