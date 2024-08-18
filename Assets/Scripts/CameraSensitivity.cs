using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraSensitivity : MonoBehaviour
{
    public CinemachineFreeLook FreeLook;

    private float StartingSensitivityX = 0;
    private float StartingSensitivityY = 0;

    private void Awake()
    {
        if (!PlayerPrefs.HasKey("MouseSensitivity"))
        {
            PlayerPrefs.SetFloat("MouseSensitivity", 0.75f);
        }
        
        StartingSensitivityX = FreeLook.m_XAxis.m_MaxSpeed;
        StartingSensitivityY = FreeLook.m_YAxis.m_MaxSpeed;

        UpdateSensitivity(PlayerPrefs.GetFloat("MouseSensitivity"));
    }

    public void Update()
    {
        UpdateSensitivity(PlayerPrefs.GetFloat("MouseSensitivity"));
    }
    // Update is called once per frame
    private void UpdateSensitivity(float Mult)
    {
        //change range 
        FreeLook.m_XAxis.m_MaxSpeed = StartingSensitivityX * Mult;
        FreeLook.m_YAxis.m_MaxSpeed = StartingSensitivityY * Mult;
    }
}
