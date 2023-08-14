using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using TMPro;
using System;
using System.IO;

public class InputConfiguration : MonoBehaviour
{
    // Prefabs
    [SerializeField] private GameObject inputBinderPrefab;

    // Components
    [SerializeField] private LocaleOptionDropdown devicesDropdown;
    [SerializeField] private GameObject inputBinderContainer;
    [SerializeField] private ScrollRect inputBinderScrollviewContent;

    // Data
    private List<UIInputBinder> inputBinders;
    public List<string> deviceNames = new List<string>();
    private InputActionData[] experienceActionData;

    // Settings
    private int id;
    public bool initialized;

    // Events
    public static event Action OnInputDeviceSelected;


    #region Data Operation

    public void Initialize(int experienceId)
    {
        id = experienceId;
        inputBinders = new List<UIInputBinder>();
        // Load strings into new component
        LocalizationController.instance.ApplyLocale();
        // Fill in interface devices
        FillDevices();
        // Fill in inputs of experience
        FillExperienceInputs();
        // Load input saved data to input binders

        initialized = true;
    }

    private void FillDevices()
    {
        ReadOnlyArray<InputDevice> devices = InputSystem.devices;

        // Create list of Dropdown with input device names
        foreach (InputDevice device in devices)
        {
            deviceNames.Add(device.displayName);
        }

        devicesDropdown.FillDropdown(deviceNames);

    }

    private void FillExperienceInputs() 
    {
        if(!(inputBinders.Count == 0))
        {
            return;
        }
        // Get input_mapping file
        LoadInputActionBindingsData();

        if (experienceActionData == null)
        {
            return;
        }

        // Create one inputBinder for each inputAction
        foreach(InputActionData inputAction in experienceActionData)
        {
            UIInputBinder inputBinder = Instantiate(inputBinderPrefab, inputBinderContainer.transform).GetComponent<UIInputBinder>();
            inputBinder.Initialize(inputAction.actionMap, inputAction.actionName , inputAction.controlType, devicesDropdown);
            inputBinders.Add(inputBinder);
        }
    }

    public void UpdateBinders()
    {
        // Apply respective typed binds to every binder dropdown
        OnInputDeviceSelected?.Invoke();
    }

    #endregion


    #region Persistence

    private void LoadInputActionBindingsData()
    {
        string path = Application.streamingAssetsPath + "/" + id.ToString() + "/build/input_mapping.json";
        
        if(!File.Exists(path))
        {
            // If there is no input_mapping.json this will stop the initialization as a whole with error
            ErrorController.instance.ShowError(LocalizationController.instance.FetchString("baseStrings", "input_error_nomapping"), 5);
            return;
        }

        string json = File.ReadAllText(path);

        // Loads all binding data
        experienceActionData = JsonUtility.FromJson<InputActionBindingsData>(json).inputActions;
    }

    #endregion
}

#region Persistence classes
[Serializable]
    public class InputActionBindingsData
    {
        public InputActionData[] inputActions;
    }

    [Serializable]
    public class InputActionData
    {
        public string actionMap;
        public string actionName;
        public string actionType;
        public string controlType;
}
#endregion
