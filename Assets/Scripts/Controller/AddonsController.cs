using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

// It saves and loads the different addons that come from an experience for them to be enabled
public class AddonsController : MonoBehaviour
{
    // Data
    public List<ExperienceAddonData> allExperiencesAddonData;

    public static AddonsController instance;

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

        // Following the dynamic loading layout here would be the input fetching of starting experience inputs but the experience 0 has no addons to fetch
    }

    #endregion

    #region Data Operations
    // Makes sure the addons are on the controller
    public void GetExperienceAddonsData(int experienceId)
    {
        AddonData addonData = LoadExperienceAddonData(experienceId);

        if (addonData != null)
        {
            ExperienceAddonData experienceAddonData = new ExperienceAddonData();
            experienceAddonData.experienceId = experienceId;
            experienceAddonData.experienceAddonData = addonData;

            ListUtils.InsertAtOrFill(allExperiencesAddonData, experienceId, experienceAddonData);
        }
    }
    // Gets the addon values from the controller
    public AddonsData GetExperienceAddons(int experienceId)
    {
        AddonsData experienceAddonsData = new AddonsData();

        if (allExperiencesAddonData[experienceId] == null)
        {
            return null;
        }

        experienceAddonsData.addonsData = allExperiencesAddonData[experienceId].experienceAddonData;

        return experienceAddonsData;
    }

    public void UpdateAllExperienceAddons(List<Addon> data, int experienceId)
    {
        // Save persistance
        SetExperienceAddonData(data, experienceId);
        // Send to experience
        SaveExperienceAddonData(experienceId);
    }

    private void SetExperienceAddonData(List<Addon> data, int experienceId)
    {
        // Inserts new addon list into the experience addon list
        allExperiencesAddonData[experienceId].experienceAddonData.addons = data;
    }

    #endregion

    #region Persistence
    private AddonData LoadExperienceAddonData(int experienceId)
    {
        AddonData experienceAddonData = new AddonData();

        string path = Application.streamingAssetsPath + "/" + experienceId.ToString() + "/build/addons.json";

        if (!File.Exists(path))
        {
            // This line happens only when experience is downloaded, it has to exist.
            ErrorController.instance.ShowError(LocalizationController.instance.FetchString("baseStrings", "addons_error_noaddons"), 5);
            return null;
        }

        string json = File.ReadAllText(path);

        // Loads all addons data
        experienceAddonData = JsonConvert.DeserializeObject<AddonsData>(json).addonsData;

        return experienceAddonData;
    }

    private void SaveExperienceAddonData(int experienceId)
    {
        AddonsData experienceAddonsData = new AddonsData();

        string path = Application.streamingAssetsPath + "/" + experienceId.ToString() + "/build/addons.json";

        if (!File.Exists(path))
        {
            // If there is addons.json, this means the experience was downloaded but not initialized correctly before releasing
            ErrorController.instance.ShowError(LocalizationController.instance.FetchString("baseStrings", "addons_error_noaddons"), 5);
            return;
        }

        experienceAddonsData = GetExperienceAddons(experienceId);

        string json = JsonConvert.SerializeObject(experienceAddonsData);


        File.WriteAllText(path, json);
    }
    #endregion
}

#region Persistence Classes

[Serializable]
public class AddonsData
{
    public AddonData addonsData;
}

[Serializable]
public class AddonData
{
    public List<Addon> addons;
}

[Serializable]
public class Addon
{
    public int addonId;
    public string addonName;
    public string addonType;
    public string addonSize;
    public bool enabled;
    public List<string> addonFileNames;
}

// Regarding launcher addon data
[Serializable]
public class ExperienceAddonData
{
    public int experienceId;
    public AddonData experienceAddonData;
}

#endregion
