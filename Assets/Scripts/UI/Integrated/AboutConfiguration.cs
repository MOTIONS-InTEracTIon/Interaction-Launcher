using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using TMPro;
using System;
using System.Text.RegularExpressions;

public class AboutConfiguration : ConfigurationMenu
{
    // Components
    [SerializeField] public LocaleText aboutText;

    #region Initialize

    public override void Initialize(int experienceId)
    {
        // Set up component data
        SetExperienceAbout(experienceId);
        // Load strings into new components
        RefreshStrings();
        // Format string to highlight url links
        aboutText.textBox.text = FormatText(aboutText.textBox.text);

        initialized = true;
    }

    public override void RefreshStrings()
    {
        base.RefreshStrings();
        aboutText.textBox.text = FormatText(LocalizationController.instance.FetchString(ExperienceController.instance.actualExperienceCardId.ToString(), "about"));
    }



    private void SetExperienceAbout(int experienceId)
    {
        id = experienceId;
        // Localization controller has already fetched the strings in Experience.Initialize()
        aboutText.groupKey = experienceId.ToString();
    }

    private string FormatText(string text)
    {
        // Use a regular expression to find [LINK:url] placeholders and replace them with clickable link tags
        string formattedText = Regex.Replace(text, @"\[LINK:(.*?)\]", match => CreateLinkTag(match.Groups[1].Value));
        return formattedText;
    }

    private string CreateLinkTag(string url)
    {
        string colorHex = "#3498db";
        return $"<color={colorHex}><u><link={url}>{url}</link></u></color>";
    }

    #endregion


}


