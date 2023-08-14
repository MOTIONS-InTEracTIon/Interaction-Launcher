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

    // Data
    public List<string> originalOptions;

    private void OnEnable()
    {
        if(groupKey != null && stringKey != null)
        {
            SetFirstOptionLocale();
        }
    }

    public void FillDropdown(List<string> options, string contentType)
    {
        dropdown.interactable = false;

        dropdown.options.Clear();
        SetFirstOptionLocale();
        // Get current options
        originalOptions = options;
        List<TMP_Dropdown.OptionData> optionData = dropdown.options;
        // Add options
        foreach (string option in options)
        {
            if (contentType == "Input")
            {
                string betterVisualOption = option.Replace(@"\s{2,}", "");
                string[] optionParts = betterVisualOption.Split("/");
                if (optionParts[1].Length > 4)
                {
                    optionParts[1] = optionParts[1].Substring(0, 4) + "...";
                }
                betterVisualOption = "/" + optionParts[1] + "/" + optionParts[2];
                optionData.Add(new TMP_Dropdown.OptionData(betterVisualOption));
            }
            else if (contentType == "Device")
            {
                optionData.Add(new TMP_Dropdown.OptionData(option));
            }

        }

        if (optionData.Count > 1)
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
