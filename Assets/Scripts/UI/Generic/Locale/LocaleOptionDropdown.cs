using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class LocaleOptionDropdown : MonoBehaviour
{
    // Components
    [SerializeField] public TMP_Dropdown dropdown;

    // Settings
    public string groupKey;
    public string stringKey;

    public void FillDropdown(List<string> options)
    {
        dropdown.interactable = false;

        dropdown.options.Clear();
        SetFirstOptionLocale();
        // Get current options
        List<TMP_Dropdown.OptionData> optionData = dropdown.options;
        // Add options
        foreach (string option in options)
        {
            optionData.Add(new TMP_Dropdown.OptionData(option));
        }

        if(optionData.Count > 1)
        {
            dropdown.interactable = true;
        }
    }

    private void SetFirstOptionLocale()
    {
        // Get current options
        List<TMP_Dropdown.OptionData> optionData = dropdown.options;
        // Create locale option
        optionData.Add(new TMP_Dropdown.OptionData(LocalizationController.instance.FetchString(groupKey,stringKey)));
    }
}
