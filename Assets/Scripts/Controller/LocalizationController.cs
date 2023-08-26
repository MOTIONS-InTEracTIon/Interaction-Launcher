using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TMPro;

using System;
using Newtonsoft.Json;
using Unity.VisualScripting;
using UnityEngine.UI;

// Localization system
// To fetch a string you inherit LocaleComponent and use GetString
// First key is string group
// Second key is string element
public class LocalizationController : MonoBehaviour
{
    // Components
    [SerializeField] public TMP_Dropdown languageDropdown;

    // Events
    public static event Action OnLanguageChange;

    // Settings
    private string defaultLocale = "en";
    public string currentLocale;

    // Data
    public List<string> locales;
    public Dictionary<string, string> baseStrings;
    public List<Dictionary<string, string>> experienceStrings;

    public static LocalizationController instance;

    #region Initialize
    public void Initialize()
    {
        // Instance initializing
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        // Load all possible locales
        locales = LoadLocales();

        // Set default text
        ApplyLocale(defaultLocale);
    }

    // Applies locale to every LocaleComponent
    public void ApplyLocale(string locale)
    {
        currentLocale = locale;
        // Get locale strings from file
        LoadBaseStrings(currentLocale);
        // Apply locale to textboxes
        OnLanguageChange?.Invoke();
    }
    // -Current language overload
    public void ApplyLocale()
    {
        // Apply locale to textboxes
        OnLanguageChange?.Invoke();
    }



    #endregion

    #region Data Operations

    // Aplies locale to dropdown
    public void ApplyLocaleDropdown(TMP_Dropdown dropdown)
    {
        languageDropdown = dropdown;
        languageDropdown.onValueChanged.AddListener(UpdateLocale);

        // Connect 
        languageDropdown.ClearOptions();
        int currentOptionIndex = 0;

        for (int i = 0; i < locales.Count; i++)
        {
            if (defaultLocale == locales[i])
            {
                currentOptionIndex = i;
            }
        }

        languageDropdown.AddOptions(locales);
        languageDropdown.value = currentOptionIndex;
        languageDropdown.RefreshShownValue();
    }
    // Fetches string from currentLocaleStrings
    public string FetchString(string groupKey, string stringKey)
    {
        if(stringKey == "")
        {
            return "";
        }

        if(groupKey == "baseStrings")
        {
            if (baseStrings.TryGetValue(stringKey, out var localizedString))
            {
                return localizedString;
            }
        }
        else
        {
            if (experienceStrings[int.Parse(groupKey)].TryGetValue(stringKey, out var localizedString))
            {
                return localizedString;
            }
        }

        return "String not found";
    }

    public void UpdateLocale(int localeIndex)
    {
        // Dropdown changed, this means the baseStrings and the experience string must be fetched again
        currentLocale = languageDropdown.options[localeIndex].text;
        // Get locale strings from file
        LoadBaseStrings(currentLocale);
        LoadExperienceStrings(ExperienceController.instance.actualExperienceCardId, true);
        // Apply locale to textboxes
        OnLanguageChange?.Invoke();
    }

    #endregion

    #region Persistence

    // Locale files are part of the application streamingAssets and they will only be loaded on start and when using the language dropdown
    public List<string> LoadLocales()
    {
        baseStrings = new Dictionary<string, string>();
        experienceStrings = new List<Dictionary<string, string>>();

        string path = Application.streamingAssetsPath + "/baseStrings";
        List<string> locales = new List<string>();

        string[] files = Directory.GetFiles(path);
        foreach(string file in files)
        {
            string localeName = Path.GetFileName(file);
            localeName = localeName.Replace("locale-", "");
            localeName = localeName.Remove(2);
            if (!locales.Contains(localeName))
            {
                locales.Add(localeName);
            }
        }

        return locales;
    }

    public void LoadBaseStrings(string localeName)
    {
        // Load from json base Strings
        string path = Application.streamingAssetsPath + "/baseStrings/locale-" + localeName + ".json";
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            baseStrings = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
        }
        else
        {
            ErrorController.instance.ShowError("baseStrings not found.", 5);
            return;
        }
    }

    public void LoadExperienceStrings(int experienceId, bool update)
    {
        // Load from json experience Strings
        Dictionary<string, string> experienceStrings = new Dictionary<string, string>();

        string path = Application.streamingAssetsPath + "/" + experienceId + "/locale-" + currentLocale + ".json";

        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            experienceStrings = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            ListUtils.InsertAtOrFill(this.experienceStrings, experienceId, experienceStrings);
        }
        else
        {
            ErrorController.instance.ShowError(FetchString("baseStrings", "experienceStrings_error"), 5);
            return;
        }
    }

        #endregion
}
