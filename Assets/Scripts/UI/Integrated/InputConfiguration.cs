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
using JetBrains.Annotations;

public class InputConfiguration : MonoBehaviour
{
    // Prefabs
    [SerializeField] private GameObject inputBinderPrefab;

    // Components
    [SerializeField] public LocaleOptionDropdown devicesDropdown;
    [SerializeField] public LocaleOptionDropdown modeDropdown;
    [SerializeField] private GameObject inputBinderContainer;
    [SerializeField] private ScrollRect inputBinderScrollview;


    // Data
    private List<UIInputBinder> inputBinders;
    private UIInputBinder currentOpenedInputBinder;
    public List<string> deviceNames = new List<string>();
    public List<string> modeNames = new List<string>();
    private bool isScrolling = false;
    private bool enableDropdowns = true;
    public List<InputActionModeData> experienceAllInputActionModeData;

    // Settings
    private int id;
    public bool initialized;

    // Events
    public static event Action OnInputDeviceSelected;

    #region Initialize
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
            deviceNames.Add(device.name);
        }

        devicesDropdown.FillDropdown(deviceNames, "Device");
        devicesDropdown.dropdown.interactable = false;

    }

    private void FillExperienceInputs()
    {
        if (!(inputBinders.Count == 0))
        {
            return;
        }
        // Get input_mapping file
        experienceAllInputActionModeData = InputPersistanceController.instance.GetExperienceInputMapping(id).allInputActionModeData;

        if (experienceAllInputActionModeData == null)
        {
            return;
        }

        // Create one inputBinder for each inputAction
        foreach (InputActionModeData modeData in experienceAllInputActionModeData)
        {
            string inputModeName = modeData.modeName;
            modeNames.Add(inputModeName);

            foreach (InputActionData inputAction in modeData.inputActions)
            {
                UIInputBinder inputBinder = Instantiate(inputBinderPrefab, inputBinderContainer.transform).GetComponent<UIInputBinder>();
                inputBinder.Initialize(inputAction.actionMap, inputAction.actionName, inputAction.controlType, inputModeName, inputAction.resultPathBinding, this);
                inputBinders.Add(inputBinder);
            }
        }

        modeDropdown.FillDropdown(modeNames, null);
        inputBinderScrollview.onValueChanged.AddListener(OnScrollViewValueChanged);

    }

    #endregion

    #region Data Operation

    // Turn on and off respective binders using mode
    public void UpdateBinders()
    {
        // Restart devices
        devicesDropdown.dropdown.value = 0;
        devicesDropdown.dropdown.RefreshShownValue();
        // Get dropdown option
        string mode = modeDropdown.dropdown.options[modeDropdown.dropdown.value].text;
        // Enable and disable respective binders
        int enabledBinders = 0;
        foreach (UIInputBinder inputBinder in inputBinders)
        {
            if (inputBinder.inputModeName == mode)
            {
                inputBinder.gameObject.SetActive(true);
                enabledBinders++;
            }
            else
            {
                inputBinder.gameObject.SetActive(false);
            }
        }

        if (enabledBinders != 0)
        {
            devicesDropdown.dropdown.interactable = true;
        }
        else
        {
            devicesDropdown.dropdown.interactable = false;
        }
    }

    // Apply respective typed binds to every binder dropdown
    public void UpdateBindersInput()
    {
        OnInputDeviceSelected?.Invoke();
    }

    // Handling scrollview
    private void OnScrollViewValueChanged(Vector2 scrollValue)
    {
        isScrolling = true;
        enableDropdowns = false;

        HideDropdowns();

        CancelInvoke("EnableDropdowns");
        Invoke("EnableDropdowns", 0.5f);
    }

    private void HideDropdowns()
    {
        if (!(inputBinders != null && inputBinders.Count > 0))
        {
            return;
        }

        foreach (UIInputBinder inputBinder in inputBinders)
        {
            TMP_Dropdown dropdown = inputBinder.bindingLocaleOptionDropdown.dropdown;
            dropdown.Hide();
            dropdown.interactable = false;
        }

        Invoke("EnableDropdowns", 0.5f);
    }

    private void EnableDropdowns()
    {
        if (!(inputBinders != null && inputBinders.Count > 0))
        {
            return;
        }

        foreach (UIInputBinder inputBinder in inputBinders)
        {
            if (inputBinder.canSelect)
            {
                TMP_Dropdown dropdown = inputBinder.bindingLocaleOptionDropdown.dropdown;
                dropdown.interactable = true;
            }

        }

        isScrolling = false;
    }
    private void Update()
    {
        if (!isScrolling && enableDropdowns)
        {
            EnableDropdowns();
        }
    }
    //

    // Persistance
    public void UpdateInputPersistance(string map, string name, string type, string modeName, string pathBinding)
    {
        // Modify the data
        SetBinding(map, name, type, modeName, pathBinding);
        // Save and send the data to experience
        InputPersistanceController.instance.UpdateAllExperienceBindings(experienceAllInputActionModeData, id);
    }

    private void SetBinding(string map, string name, string type, string modeName, string pathBinding)
    {
        foreach(InputActionModeData modeData in experienceAllInputActionModeData)
        {
            if(modeData.modeName == modeName)
            {
                foreach(InputActionData inputActionData in modeData.inputActions)
                {
                    if(inputActionData.actionMap == map && inputActionData.actionName == name && inputActionData.controlType == type)
                    {
                        inputActionData.resultPathBinding = pathBinding;
                    }
                }
            }
        }
    }

    #endregion
}


