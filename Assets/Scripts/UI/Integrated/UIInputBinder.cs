using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.UI;
using System;
using System.Linq;

public class UIInputBinder : MonoBehaviour
{
    // Other references 
    private InputConfiguration inputConfiguration;
    private LocaleOptionDropdown devicesDropdown;

    // Components
    [SerializeField] TMP_InputField nameInputField;
    [SerializeField] TMP_InputField typeInputField;
    [SerializeField] public LocaleOptionDropdown bindingLocaleOptionDropdown;
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI typeText;
    [SerializeField] TextMeshProUGUI pathText;

    // Data
    private string inputMap;
    private string inputName;
    private string inputType;
    private string deviceName;
    public string pathBinding;
    public bool canSelect;
    private InputActionData inputAction;
    private InputBindingData inputBinding;

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

    public void Initialize(InputActionData inputAction, InputBindingData inputBinding, string modeName, InputConfiguration inputWindow)
    {
        bindingLocaleOptionDropdown.dropdown.interactable = false;
        canSelect = false;
        inputConfiguration = inputWindow;
        devicesDropdown = inputConfiguration.devicesDropdown;
        this.inputAction = inputAction;
        this.inputBinding = inputBinding;
        // String loading
        LocalizationController.instance.ApplyLocale();
        // Load info into binder
        LoadInfo(inputAction, inputBinding, modeName);
        // Disable Binder before a mode is chosen
        this.gameObject.SetActive(false);
    }

    private void LoadInfo(InputActionData inputAction, InputBindingData inputBinding, string modeName)
    {
        inputMap = inputAction.actionMap;
        inputModeName = modeName;
        if (inputBinding.isComposite)
        {
            inputName = inputAction.actionName + " - " + inputBinding.bindingName + " (Composite)";
            nameInputField.text = inputName;
            typeInputField.gameObject.SetActive(false);
            typeText.gameObject.SetActive(false);
            pathText.gameObject.SetActive(false);
            bindingLocaleOptionDropdown.gameObject.SetActive(false);
            Color hexColor;
            ColorUtility.TryParseHtmlString("#2E2D57", out hexColor);
            gameObject.GetComponent<Image>().color = hexColor;
        }
        else if (inputBinding.isPartOfComposite)
        {
            inputName = inputAction.actionName + " - " + inputBinding.bindingName;
            nameInputField.text = inputName;
            inputType = "Any";
            typeInputField.text = "Any";
            pathBinding = inputBinding.path;
            bindingLocaleOptionDropdown.SetFirstOptionString(pathBinding);
        }
        else
        {
            inputName = inputAction.actionName;
            nameInputField.text = inputName;
            inputType = inputAction.controlType;
            typeInputField.text = inputAction.controlType;
            pathBinding = inputBinding.path;
            bindingLocaleOptionDropdown.SetFirstOptionString(pathBinding);
        }
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
        // Input Type Compatibility code part
        foreach(InputControl inputControl in inputs)
        {
            string controltype = inputControl.layout;

            if (controltype == inputType || inputType == "Any")
            {
                string[] pathSplit = inputControl.path.Split("/");
                string path = string.Join("/", pathSplit, 2, pathSplit.Length - 2);
                inputPaths.Add(inputConfiguration.devicesDropdown.originalOptions[inputConfiguration.devicesDropdown.dropdown.value] + "/" + path);
            }
            else if (inputType == "Button" && controltype == "Key")
            {
                string[] pathSplit = inputControl.path.Split("/");
                string path = string.Join("/", pathSplit, 1, pathSplit.Length - 1);
                inputPaths.Add(inputConfiguration.devicesDropdown.originalOptions[inputConfiguration.devicesDropdown.dropdown.value] + "/" + path);
            }
        }

        bindingLocaleOptionDropdown.SetFirstOptionString(pathBinding);

        bindingLocaleOptionDropdown.FillDropdown(inputPaths, "Input");

        if(inputPaths.Count > 1)
        {
            canSelect = true;
        }
    }

    public void UpdateBindSaveData()
    {
        // If a path was set, we need to know what it was
        int index = bindingLocaleOptionDropdown.dropdown.value;
        bindingLocaleOptionDropdown.dropdown.Hide();
        if (index == 0)
        {
            pathBinding = "Default";
        }
        else
        {
            pathBinding = bindingLocaleOptionDropdown.originalOptions[bindingLocaleOptionDropdown.dropdown.value];
            string[] pathParts = pathBinding.Split("/");
            pathBinding = pathParts[0] + "/" + string.Join("/", pathParts, 1, pathParts.Length - 1);

        }

        inputBinding.path = pathBinding;
        inputConfiguration.UpdateInputPersistance(inputAction, inputBinding, inputModeName);
    }

    #endregion
}
