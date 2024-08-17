using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    public Slider MasterVolumeSlider;
    public Slider MouseSensitivitySlider;
    public CameraSensitivity CameraSensitivity;

    public const string MASTER_VOLUME_PREF_KEY = "MasterVolume";
    public const string MOUSE_SENSITIVITY_PREF_KEY = "MouseSensitivity";

    // Start is called before the first frame update
    void Start()
    {
        LoadSavedSliderValues();
    }

    public void UpdateVolume(float NewValue)
    {
        PlayerPrefs.SetFloat(MASTER_VOLUME_PREF_KEY, NewValue);
    }

    public void UpdateSensitivity(float NewValue)
    {
        PlayerPrefs.SetFloat(MOUSE_SENSITIVITY_PREF_KEY, NewValue);

        if (CameraSensitivity)
        {
            CameraSensitivity.UpdateSensitivity(NewValue);
        }
    }

    private void LoadSavedSliderValues()
    {
        if (PlayerPrefs.HasKey(MASTER_VOLUME_PREF_KEY))
        {
            MasterVolumeSlider.value = PlayerPrefs.GetFloat(MASTER_VOLUME_PREF_KEY);
        }

        if (PlayerPrefs.HasKey(MOUSE_SENSITIVITY_PREF_KEY))
        {
            MouseSensitivitySlider.value = PlayerPrefs.GetFloat(MOUSE_SENSITIVITY_PREF_KEY);
        }
    }
}
