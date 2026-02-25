using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [Tooltip("Ссылка на миксер")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Slider masterSlider;

    // Start is called before the first frame update
    void Start()
    {
        LoadSettings();
        SetSettings();
    }

    private void LoadSettings()
    {
        masterSlider.value = PlayerPrefs.GetFloat("masterVolume", 0.5f);
    }

    private void SetSettings()
    {
        SetMasterVolume();
        // и т.д. с появлением новых настроек
    }

    public void SetMasterVolume()
    {
        float volume = masterSlider.value;
        if (volume == 0)
        {
            audioMixer.SetFloat("master", -80f);
        }
        else
        {
            audioMixer.SetFloat("master", Mathf.Log10(volume)*20);
        }
        PlayerPrefs.SetFloat("masterVolume", volume);
    }
}
