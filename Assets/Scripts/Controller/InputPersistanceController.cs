using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;

// It saves and loads the different input files that come from the different experiences
public class InputPersistanceController : MonoBehaviour
{
    // Data
    public List<ExperienceInputActionData> allExperiencesInputData;

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

        GetAllExperiencesInputActionData();
    }

    // Loads all input data if it exists, it generates it using experiences input_mapping if it doesn't
    private void GetAllExperiencesInputActionData()
    {
        // Load files
        allExperiencesInputData = GenerateAllExperiencesInputActionData();
    }

    private List<ExperienceInputActionData> GenerateAllExperiencesInputActionData()
    {
        // Fill all experiences binding data
        List<ExperienceInputActionData> allExperienceInputData = new List<ExperienceInputActionData>();

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
            if(int.Parse(folder) == 0)
            {
                continue;
            }

            allExperienceInputData.Add(GenerateExperienceInputActionData(int.Parse(folder)));
        }

        return allExperienceInputData;
    }

    private ExperienceInputActionData GenerateExperienceInputActionData(int experienceId)
    {
        InputActionsData inputActions = LoadExperienceInputActionsData(experienceId);

        if (inputActions != null)
        {
            ExperienceInputActionData experienceInputActionData = new ExperienceInputActionData();
            experienceInputActionData.experienceId = experienceId;
            experienceInputActionData.experienceInputActionsData = inputActions;

            return experienceInputActionData;
        }

        return null;
    }

    #endregion

    #region Data Operations

    public InputActionsData GetExperienceInputMapping(int experienceId)
    {
        foreach (ExperienceInputActionData experienceInputData in allExperiencesInputData)
        {
            if(experienceInputData.experienceId == experienceId)
            {
                return experienceInputData.experienceInputActionsData;
            }
        }

        return null;
    }

    public void UpdateAllExperienceBindings(List<InputActionModeData> data, int experienceId)
    {
        // Save persistance
        SetExperienceInputActionData(data, experienceId);
        // Send to experience
        SaveExperienceInputActionData(experienceId);
    }

    private void SetExperienceInputActionData(List<InputActionModeData> data, int experienceId)
    {   
        // Inserts new mode data assigned to an experienceId in the input action data attribute
        foreach(ExperienceInputActionData experienceInputActionData in allExperiencesInputData)
        {
            if(experienceInputActionData.experienceId == experienceId)
            {
                experienceInputActionData.experienceInputActionsData.allInputActionsModeData = data;
            }
        }
    }



    #endregion

    #region Persistence
    // Regarding experience input_mapping
    private InputActionsData LoadExperienceInputActionsData(int experienceId)
    {
        InputActionsData experienceBindingData = new InputActionsData();

        string path = Application.streamingAssetsPath + "/" + experienceId.ToString() + "/build/input_mapping.json";

        if (!File.Exists(path))
        {
            // If there is no input_mapping.json, this could mean that the experience is not downloaded yet, just ignore it
            return null;
        }

        string json = File.ReadAllText(path);

        // Loads all binding data
        experienceBindingData = JsonUtility.FromJson<InputActionsData>(json);

        return experienceBindingData;
    }

    private void SaveExperienceInputActionData(int experienceId)
    {
        InputActionsData experienceBindingsData = new InputActionsData();

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
public class InputActionsData
{
    public List<InputActionModeData> allInputActionsModeData;
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
    public string actionName;
    public string actionMap;
    public string controlType;
    public List<InputBindingData> inputBindings;
}

[Serializable]
public class InputBindingData
{
    public string bindingName;
    public bool isComposite;
    public bool isPartOfComposite;
    // Comes in "Default", it is filled and sent to experience
    public string path;
}

// Regarding launcher binding data
[Serializable]
public class LauncherInputActionSaveData
{
    public List<ExperienceInputActionData> allExperiencesInputActionData;
}

[Serializable]
public class ExperienceInputActionData
{
    public int experienceId;
    public InputActionsData experienceInputActionsData;
}


#endregion