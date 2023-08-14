using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.UI;

public class UIInputBinder : MonoBehaviour
{
    // Other references 
    private LocaleOptionDropdown devicesDropdown; 

    // Components
    [SerializeField] TMP_InputField nameInputField;
    [SerializeField] TMP_InputField typeInputField;
    [SerializeField] LocaleOptionDropdown bindingLocaleOptionDropdown;

    // Data
    private string inputName;
    private string inputType;

    #region Data Operations

    private void OnEnable()
    {
        InputConfiguration.OnInputDeviceSelected += SetBindingDropdown;
    }

    private void OnDisable()
    {
        InputConfiguration.OnInputDeviceSelected -= SetBindingDropdown;
    }

    public void Initialize(string map, string name, string type, LocaleOptionDropdown dropdown)
    {
        bindingLocaleOptionDropdown.dropdown.interactable = false;
        devicesDropdown = dropdown;
        // Load strings into new component
        LocalizationController.instance.ApplyLocale();
        // Load info into binder
        LoadInfo(map, name, type);
    }

    private void LoadInfo(string map, string name, string type)
    {
        inputName = map + " (" + name + ")";
        inputType = type;

        nameInputField.text = inputName;
        typeInputField.text = inputType;
    }

    private void SetBindingDropdown()
    {
        if(devicesDropdown.dropdown.value == 0)
        {
            return;
        }

        // Get device inputs
        TMP_Dropdown.OptionData deviceOption = devicesDropdown.dropdown.options[devicesDropdown.dropdown.value];
        string deviceName = deviceOption.text;
        InputDevice device = InputSystem.GetDevice(deviceName);

        ReadOnlyArray<InputControl> inputs = device.allControls;
        // Cycle through every and change the options depending on the binder type 
        List<string> inputPaths = new List<string>();
        foreach(InputControl inputControl in inputs)
        {
            string controltype = GetInputControlType(inputControl);
            
            if(controltype == inputType) 
            {
                inputPaths.Add(inputControl.path);
            }
            else if (inputType == "Button" && controltype == "Key")
            {
                inputPaths.Add(inputControl.path);
            }
        }

        bindingLocaleOptionDropdown.FillDropdown(inputPaths);

        if(inputPaths.Count > 1)
        {
            bindingLocaleOptionDropdown.dropdown.interactable = true;
        }
    }

    private string GetInputControlType(InputControl inputControl)
    {
        string[] pathParts = inputControl.path.Split('/');
        string controlType = pathParts[0];

        return controlType;
    }

    //This function will get more robust with type conversion patches (For example, to map to a Vector3 with a Vector2 or to use Keys or buttons to make an Axis go up and down)
    //Note that the type conversion patches are experience bound, this means the experience has to implement the way to convert these types
    //private List<string> GetCompatibleControlType(string controltype)
    //{
        // Retrieve the supported conversions in InputConfiguration and pass them up
        // FOR NOW JUST LET SAME TYPE AND BUTTON/KEY
        
    //}

    #endregion


}