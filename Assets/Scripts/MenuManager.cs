using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance;

    public float FadeTime = 1.0f;
    public CanvasGroup SettingsGroup;

    private CanvasGroup CurrentlyOpenGroup;
    // Start is called before the first frame update
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    private void Start()
    {
        SettingsGroup.alpha = 0;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (CurrentlyOpenGroup != null)
            {
                HideGroup(SettingsGroup);
            }
            else
            {
                ShowGroup(SettingsGroup);
            }
        }  
        
    }

    public void OpenMenu(CanvasGroup NewGroup)
    {
        if (CurrentlyOpenGroup != null)
        {
            HideGroup(CurrentlyOpenGroup);
        }

        ShowGroup(NewGroup);
    }

    private void HideGroup(CanvasGroup Group)
    {
        Group.DOFade(0, FadeTime).SetEase(Ease.OutQuad);

        CurrentlyOpenGroup = null;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void ShowGroup(CanvasGroup Group)
    {
        Group.DOFade(1, FadeTime).SetEase(Ease.OutQuad);

        CurrentlyOpenGroup = Group;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;

    }
}