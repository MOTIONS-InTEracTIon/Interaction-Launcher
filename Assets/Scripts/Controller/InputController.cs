using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;

// It saves and loads the different input files that come from the different experiences
public class InputController : MonoBehaviour
{
    // Data
    public List<ExperienceInputActionData> allExperiencesInputData;

    public static InputController instance;

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

        // Following the dynamic loading layout here would be the input fetching of starting experience inputs but the experience 0 has no inputs to fetch
    }

    #endregion

    #region Data Operations
    // Makes sure the inputs are on the controller
    public void GetExperienceInputActionData(int experienceId)
    {
        InputActionsData inputActions = LoadExperienceInputActionsData(experienceId);

        if (inputActions != null)
        {
            ExperienceInputActionData experienceInputActionData = new ExperienceInputActionData();
            experienceInputActionData.experienceId = experienceId;
            experienceInputActionData.experienceInputActionsData = inputActions;

            ListUtils.InsertAtOrFill(allExperiencesInputData, experienceId, experienceInputActionData);
        }
    }
    // Gets the input values from the controller
    public InputActionsData GetExperienceInputMapping(int experienceId)
    {
        if (allExperiencesInputData[experienceId] == null)
        {
            return null;
        }

        return allExperiencesInputData[experienceId].experienceInputActionsData;
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
        allExperiencesInputData[experienceId].experienceInputActionsData.allInputActionsModeData = data;
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
            // This line happens only when experience is downloaded, it has to exist.
            ErrorController.instance.ShowError(LocalizationController.instance.FetchString("baseStrings", "input_error_nomapping"), 5);
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
public class ExperienceInputActionData
{
    public int experienceId;
    public InputActionsData experienceInputActionsData;
}


#endregion