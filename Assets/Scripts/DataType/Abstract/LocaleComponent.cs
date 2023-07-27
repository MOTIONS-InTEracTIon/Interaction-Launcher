using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Component subscribes to event that changes its text when the language is changed
public class LocaleComponent : MonoBehaviour
{
    protected virtual void OnEnable()
    {
        LocalizationController.OnLanguageChange += OnLanguageChangeHandler;
    }

    protected virtual void OnDisable()
    {
       LocalizationController.OnLanguageChange -= OnLanguageChangeHandler;
    }

    protected virtual void OnLanguageChangeHandler()
    {
        UpdateText();
    }

    public virtual void UpdateText()
    {
        // To be implemented in the derived classes
    }
}
