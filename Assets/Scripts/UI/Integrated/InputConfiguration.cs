using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using TMPro;
using System;

public class InputConfiguration : ConfigurationMenu
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
    private List<UIInputBinder> activeBinders;
    private UIInputBinder currentOpenedInputBinder;
    public List<string> deviceNames = new List<string>();
    public List<string> modeNames = new List<string>();
    private bool isScrolling = false;
    private bool enableDropdowns = true;
    public List<InputActionModeData> experienceAllInputActionModeData;

    // Events
    public static event Action OnInputDeviceSelected;


    #region Initialize
    public override void Initialize(int experienceId)
    {
        id = experienceId;
        inputBinders = new List<UIInputBinder>();
        // Load strings into new component
        LocalizationController.instance.ApplyLocale();
        // Update InputController with experience inputs
        InputController.instance.GetExperienceInputActionData(experienceId); 
        // Fill in interface devices
        FillDevices();
        // Fill in inputs of experience
        FillExperienceInputs();
        // Load input saved data to input binders

        initialized = true;
    }

    public override void RefreshStrings()
    {
        base.RefreshStrings();
        if(devicesDropdown.dropdown.value == 0)
        {
            devicesDropdown.dropdown.captionText.text = LocalizationController.instance.FetchString("baseStrings", "input_binder_defaultdevice");
        }
        if(modeDropdown.dropdown.value == 0)
        {
            modeDropdown.dropdown.captionText.text = LocalizationController.instance.FetchString("baseStrings", "input_binder_defaultmode");
        }
    }

    private void FillDevices()
    {
        ReadOnlyArray<InputDevice> devices = InputSystem.devices;

        // Create list of Dropdown with input device names
        foreach (InputDevice device in devices)
        {
            string[] deviceName = device.ToString().Split(":/");
            deviceNames.Add(deviceName[0]);
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
        experienceAllInputActionModeData = InputController.instance.GetExperienceInputMapping(id).allInputActionsModeData;

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
                foreach (InputBindingData inputBinding in inputAction.inputBindings)
                {
                    UIInputBinder inputBinder = Instantiate(inputBinderPrefab, inputBinderContainer.transform).GetComponent<UIInputBinder>();
                    inputBinder.Initialize(inputAction, inputBinding, inputModeName, this);
                    inputBinders.Add(inputBinder);
                }
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
        activeBinders = new List<UIInputBinder>();
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
                activeBinders.Add((inputBinder));
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
        if (!(activeBinders != null && activeBinders.Count > 0))
        {
            return;
        }

        foreach (UIInputBinder inputBinder in inputBinders)
        {
            TMP_Dropdown dropdown = inputBinder.bindingLocaleOptionDropdown.dropdown;
            if(dropdown.IsExpanded)
            {
                dropdown.Hide();
            }

            dropdown.interactable = false;
            inputBinder.bindingLocaleOptionDropdown.SetFirstOptionString(inputBinder.pathBinding);
            
        }

        Invoke("EnableDropdowns", 0.5f);
    }

    private void EnableDropdowns()
    {
        if (!(activeBinders != null && activeBinders.Count > 0))
        {
            return;
        }

        foreach (UIInputBinder inputBinder in inputBinders)
        {
            if (inputBinder.canSelect)
            {
                TMP_Dropdown dropdown = inputBinder.bindingLocaleOptionDropdown.dropdown;
                dropdown.interactable = true;
                inputBinder.bindingLocaleOptionDropdown.SetFirstOptionString(inputBinder.pathBinding);
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
    public void UpdateInputPersistance(InputActionData inputAction, InputBindingData inputBinding, string mode)
    {
        // Modify the data
        SetBinding(inputAction, inputBinding, mode);
        // Save and send the data to experience
        InputController.instance.UpdateAllExperienceBindings(experienceAllInputActionModeData, id);
    }

    private void SetBinding(InputActionData inputAction, InputBindingData inputBinding, string mode)
    {
        foreach (InputActionModeData modeData in experienceAllInputActionModeData)
        {
            if (modeData.modeName == mode)
            {
                foreach (InputActionData inputActionData in modeData.inputActions)
                {
                    if (inputActionData.actionMap == inputAction.actionMap &&
                        inputActionData.actionName == inputAction.actionName &&
                        inputActionData.controlType == inputAction.controlType)
                    {
                        foreach (InputBindingData inputBindingData in inputActionData.inputBindings)
                        {
                            if(inputBindingData.bindingName == inputBinding.bindingName &&
                               inputBindingData.isComposite == inputBinding.isComposite &&
                               inputBindingData.isPartOfComposite == inputBinding.isPartOfComposite)
                            {
                                inputBindingData.path = inputBinding.path;
                            }
                        }
                    }
                }
            }
        }
    }

    #endregion
}


