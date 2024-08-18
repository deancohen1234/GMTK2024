using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class SettingsMenu : MonoBehaviour
{
    public Slider MasterVolumeSlider;
    public Slider MusicVolumeSlider;
    public Slider MouseSensitivitySlider;
    public AudioMixer MasterMixer;

    public const string MASTER_VOLUME_PREF_KEY = "MasterVolume";
    public const string MUSIC_VOLUME_PREF_KEY = "MusicVolume";
    public const string MOUSE_SENSITIVITY_PREF_KEY = "MouseSensitivity";

    // Start is called before the first frame update
    void Start()
    {
        LoadSavedSliderValues();
    }

    public void UpdateMasterVolume(float NewValue)
    {
        NewValue = Mathf.Max(NewValue, 0.001f);
        PlayerPrefs.SetFloat(MASTER_VOLUME_PREF_KEY, NewValue);
        MasterMixer.SetFloat("MasterVolume", Mathf.Log10(NewValue) * 20f);
    }

    public void UpdateMusicVolume(float NewValue)
    {
        NewValue = Mathf.Max(NewValue, 0.001f);
        PlayerPrefs.SetFloat(MUSIC_VOLUME_PREF_KEY, NewValue);
        MasterMixer.SetFloat("MusicVolume", Mathf.Log10(NewValue) * 20f);
    }

    public void UpdateSensitivity(float NewValue)
    {
        PlayerPrefs.SetFloat(MOUSE_SENSITIVITY_PREF_KEY, NewValue);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    private void LoadSavedSliderValues()
    {
        if (PlayerPrefs.HasKey(MASTER_VOLUME_PREF_KEY))
        {
            MasterVolumeSlider.value = PlayerPrefs.GetFloat(MASTER_VOLUME_PREF_KEY);
        }

        if (PlayerPrefs.HasKey(MUSIC_VOLUME_PREF_KEY))
        {
            MusicVolumeSlider.value = PlayerPrefs.GetFloat(MUSIC_VOLUME_PREF_KEY);
        }

        if (PlayerPrefs.HasKey(MOUSE_SENSITIVITY_PREF_KEY))
        {
            MouseSensitivitySlider.value = PlayerPrefs.GetFloat(MOUSE_SENSITIVITY_PREF_KEY);
        }
    }
}
