using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Submenu of experience that closes if the experience changes
[RequireComponent(typeof(FadeUI))]
public abstract class ConfigurationMenu : MonoBehaviour
{
    // Settings
    protected int id;
    public bool isOpen;
    public bool initialized;

    // Coroutine
    protected bool isCloseMenuCoroutineRunning;

    // Event
    public static event Action onConfigurationMenuClosed;

    private void OnEnable()
    {
        ExperienceController.onExperienceCardChange += SetMenuClosed;
        RefreshStrings();
    }

    private void OnDisable()
    {
        ExperienceController.onExperienceCardChange -= SetMenuClosed;
    }

    #region Menu Closing
    public void SetMenuClosed()
    {
        onConfigurationMenuClosed?.Invoke();
        isOpen = false;
        StopAllCoroutines();

        this.gameObject.SetActive(false);
    }

    public void CloseMenu()
    {
        if(isCloseMenuCoroutineRunning)
        {
            return;
        }

        StartCoroutine(CloseMenuCoroutine());
    }

    public IEnumerator CloseMenuCoroutine()
    {
        isCloseMenuCoroutineRunning = true;
        FadeUI actualMenuFader = GetComponent<FadeUI>();
        yield return StartCoroutine(actualMenuFader.FadeOut());
        isCloseMenuCoroutineRunning = false;
        SetMenuClosed();
    }

    #endregion

    #region Initialize
    public abstract void Initialize(int experienceId);
    #endregion

    #region Data Operations

    // This method refreshes the strings and executes any post fetch code menu specific
    public virtual void RefreshStrings()
    {
        LocalizationController.instance.ApplyLocale();
    }

    #endregion
}
