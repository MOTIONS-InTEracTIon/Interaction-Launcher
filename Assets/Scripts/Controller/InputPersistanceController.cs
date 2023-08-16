using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SocialPlatforms;

// Save System, it lets the experiences InputConfiguration to load and save bindings so there is no need to reconfigure everything when you close the app
public class InputPersistanceController : MonoBehaviour
{
    // Data
    public List<ExperienceBindingData> allExperiencesBindingData;

    public static InputPersistanceController instance;

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

        GetAllExperiencesBindingData();
    }

    // Loads all binding data if it exists, it generates it using experiences input_mapping if it doesn't
    private void GetAllExperiencesBindingData()
    {
        // Load files
        if (!LoadAllExperienceBindingData())
        {
            // Doesn't exist, save generated
            GenerateAllExperiencesBindingData();
            SaveAllExperiencesBindingData();
        }
    }

    private void GenerateAllExperiencesBindingData()
    {
        // Fill all experiences binding data
        List<ExperienceBindingData> allBindingData = new List<ExperienceBindingData>();

        // Check number of experiences
        
        int experiences = 0;
        string[] experienceFolders = Directory.GetDirectories(Application.streamingAssetsPath);
        List<string> numberedFolders = new List<string>();

        foreach (string directory in experienceFolders)
        {
            string dirName = Path.GetFileName(directory);
            int dirNumber;

            if (int.TryParse(dirName, out dirNumber))
            {
                experiences++;
                numberedFolders.Add(dirName);
            }
        }

        // For each experience, get its input_mapping if it is already downloaded
        foreach(string folder in numberedFolders)
        {
            InputActionBindingsData inputActionBindings = LoadExperienceBindingsData(int.Parse(folder));

            if(inputActionBindings != null)
            {
                ExperienceBindingData experienceBindingData = new ExperienceBindingData();
                experienceBindingData.experienceId = int.Parse(folder);
                experienceBindingData.experienceInputActionBindingsData = inputActionBindings;

                allBindingData.Add(experienceBindingData);
            }
        }

        allExperiencesBindingData = allBindingData;
    }

    #endregion

    #region Data Operations

    public InputActionBindingsData GetExperienceInputMapping(int experienceId)
    {
        foreach (ExperienceBindingData bindingData in allExperiencesBindingData)
        {
            if(bindingData.experienceId == experienceId)
            {
                return bindingData.experienceInputActionBindingsData;
            }
        }

        return null;
    }

    public void UpdateAllExperienceBindings(List<InputActionModeData> data, int experienceId)
    {
        // Save persistance
        SetBindingsData(data, experienceId);
        SaveAllExperiencesBindingData();
        // Send to experience
        SaveExperienceBindingsData(experienceId);
    }

    private void SetBindingsData(List<InputActionModeData> data, int experienceId)
    {   
        // Inserts new mode data assigned to an experienceId in the binding data attribute
        foreach(ExperienceBindingData experienceBindingData in allExperiencesBindingData)
        {
            if(experienceBindingData.experienceId == experienceId)
            {
                experienceBindingData.experienceInputActionBindingsData.allInputActionModeData = data;
            }
        }
    }


    #endregion

    #region Persistence
    private bool LoadAllExperienceBindingData()
    {
        bool success = false;

        string path = Application.persistentDataPath + "/Experience binding data.json";
        if(File.Exists(path))
        {
            string json = File.ReadAllText(path);

            allExperiencesBindingData = JsonUtility.FromJson<LauncherBindingSaveData>(json).allExperienceBindingData;
            success = true;
        }

        return success;
    }

    private void SaveAllExperiencesBindingData()
    {
        bool success = false;
        
        LauncherBindingSaveData launcherBindingSaveData = new LauncherBindingSaveData();
        launcherBindingSaveData.allExperienceBindingData = allExperiencesBindingData;

        string json = JsonUtility.ToJson(launcherBindingSaveData);

        File.WriteAllText(Application.persistentDataPath + "/Experience binding data.json", json);
    }


    // Regarding experience input_mapping
    private InputActionBindingsData LoadExperienceBindingsData(int experienceId)
    {
        InputActionBindingsData experienceBindingData = new InputActionBindingsData();

        string path = Application.streamingAssetsPath + "/" + experienceId.ToString() + "/build/input_mapping.json";

        if (!File.Exists(path))
        {
            // If there is no input_mapping.json, this could mean that the experience is not downloaded yet, just ignore it
            return null;
        }

        string json = File.ReadAllText(path);

        // Loads all binding data
        experienceBindingData = JsonUtility.FromJson<InputActionBindingsData>(json);

        return experienceBindingData;
    }

    private void SaveExperienceBindingsData(int experienceId)
    {
        InputActionBindingsData experienceBindingsData = new InputActionBindingsData();

        string path = Application.streamingAssetsPath + "/" + experienceId.ToString() + "/build/input_mapping.json";

        if(!File.Exists(path))
        {
            // If there is no input_mapping.json, this means the experience was downloaded but not initialized correctly before releasing
            ErrorController.instance.ShowError(LocalizationController.instance.FetchString("baseStrings", "input_error_nomapping"),5);
            return;
        }

        experienceBindingsData = GetExperienceInputMapping(experienceId);

        string json = JsonUtility.ToJson(experienceBindingsData);

        File.WriteAllText(path, json);
    }
    #endregion

}

#region Persistence classes

// Regarding experience input_mapping
[Serializable]
public class InputActionBindingsData
{
    public List<InputActionModeData> allInputActionModeData;
}

[Serializable]
public class InputActionModeData
{
    public string modeName;
    public List<InputActionData> inputActions;
}

[Serializable]
public class InputActionData
{
    public string actionMap;
    public string actionName;
    public string actionType;
    public string controlType;
    // Comes clean, and it is meant to be overriden using the launcher
    public string resultPathBinding;
}

// Regarding launcher binding data
[Serializable]
public class LauncherBindingSaveData
{
    public List<ExperienceBindingData> allExperienceBindingData;
}

[Serializable]
public class ExperienceBindingData
{
    public int experienceId;
    public InputActionBindingsData experienceInputActionBindingsData;
}


#endregion