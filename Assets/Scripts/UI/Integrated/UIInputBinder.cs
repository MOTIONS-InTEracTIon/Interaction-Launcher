using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.UI;
using System;

public class UIInputBinder : MonoBehaviour
{
    // Other references 
    private InputConfiguration inputConfiguration;
    private LocaleOptionDropdown devicesDropdown;

    // Components
    [SerializeField] TMP_InputField nameInputField;
    [SerializeField] TMP_InputField typeInputField;
    [SerializeField] public LocaleOptionDropdown bindingLocaleOptionDropdown;

    // Data
    private string inputMap;
    private string inputName;
    private string inputType;
    private string deviceName;
    private string pathBinding;
    public bool canSelect;

    // Settings
    public string inputModeName;

    #region Initialize

    private void OnEnable()
    {
        InputConfiguration.OnInputDeviceSelected += SetBindingDropdown;
    }

    private void OnDisable()
    {
        InputConfiguration.OnInputDeviceSelected -= SetBindingDropdown;
    }

    public void Initialize(string map, string name, string type, string modeName, string pathBinding, InputConfiguration inputWindow)
    {
        bindingLocaleOptionDropdown.dropdown.interactable = false;
        canSelect = false;
        inputConfiguration = inputWindow;
        devicesDropdown = inputConfiguration.devicesDropdown;
        // Load strings into new component
        LocalizationController.instance.ApplyLocale();
        // Load info into binder
        LoadInfo(map, name, type, modeName, pathBinding);
        // Disable Binder before a mode is chosen
        this.gameObject.SetActive(false);
    }

    private void LoadInfo(string map, string name, string type, string modeName, string pathBinding)
    {
        inputMap = map;
        inputName = name;
        inputType = type;
        inputModeName = modeName;
        this.pathBinding = pathBinding;

        bindingLocaleOptionDropdown.SetFirstOptionString(pathBinding);

        nameInputField.text = name + " (" + map + ")";
        typeInputField.text = inputType;
    }

    #endregion

    #region Data Operations

    private void SetBindingDropdown()
    {
        if(devicesDropdown.dropdown.value == 0)
        {
            bindingLocaleOptionDropdown.dropdown.interactable = false;
            canSelect = false;
            return;
        }

        // Get device inputs
        ReadOnlyArray<InputControl> inputs = new ReadOnlyArray<InputControl>();
        try
        {
            TMP_Dropdown.OptionData deviceOption = devicesDropdown.dropdown.options[devicesDropdown.dropdown.value];
            string deviceName = deviceOption.text;
            InputDevice device = InputSystem.GetDevice(deviceName);

            inputs = device.allControls;
        }
        catch
        {
            ErrorController.instance.ShowError(LocalizationController.instance.FetchString("baseStrings", "input_error_device"),5);

        }


        // Cycle through every and change the options depending on the binder type 
        List<string> inputPaths = new List<string>();
        // Input Type Compatibility code part, will eventually use GetCompatibleControlType
        foreach(InputControl inputControl in inputs)
        {
            string controltype = inputControl.layout;

            if (controltype == inputType)
            {
                inputPaths.Add(inputConfiguration.devicesDropdown.originalOptions[inputConfiguration.devicesDropdown.dropdown.value - 1] + inputControl.path);
            }
            else if (inputType == "Button" && controltype == "Key")
            {
                inputPaths.Add(inputConfiguration.devicesDropdown.originalOptions[inputConfiguration.devicesDropdown.dropdown.value - 1] + inputControl.path);
            }
        }

        bindingLocaleOptionDropdown.FillDropdown(inputPaths, "Input");

        if(inputPaths.Count > 1)
        {
            canSelect = true;
        }
    }

    //This function will get more robust with type conversion patches (For example, to map to a Vector3 with a Vector2 or to use Keys or buttons to make an Axis go up and down)
    //Note that the type conversion patches are experience bound, this means the experience has to implement the way to convert these types
    //private List<string> GetCompatibleControlType(string controltype)
    //{
        // Retrieve the supported conversions in InputConfiguration and pass them up
        // FOR NOW JUST LET SAME TYPE AND BUTTON/KEY
        
    //}

    public void UpdateBindSaveData()
    {
        // If a path was set, we need to know what it was
        int index = bindingLocaleOptionDropdown.dropdown.value - 1;
        if (index == -1 && pathBinding == LocalizationController.instance.FetchString("baseStrings", "input_binder_defaultbinding"))
        {
            pathBinding = "Default";
        }
        else
        {
            pathBinding = bindingLocaleOptionDropdown.originalOptions[bindingLocaleOptionDropdown.dropdown.value - 1];
            string[] pathParts = pathBinding.Split("/");
            pathBinding = pathParts[0] + "/" + string.Join("/", pathParts, 2, pathParts.Length - 2);
        }
        
        inputConfiguration.UpdateInputPersistance(inputMap, inputName, inputType, inputModeName, pathBinding);
    }

    #endregion
}
