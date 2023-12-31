using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System;
using System.Linq;
using System.IO;
using UnityEngine.Networking;

public class AddonsConfiguration : ConfigurationMenu
{
    // Prefabs
    [SerializeField] private GameObject addonPrefab;

    // Components
    [SerializeField] public LocaleOptionDropdown addonTypeDropdown;
    [SerializeField] private GameObject addonContainer;
    [SerializeField] private ScrollRect addonScrollview;

    // Data
    private List<UIAddon> addonElements;
    private List<UIAddon> activeAddonElements;
    public List<string> addonSizes;
    public List<Addon> experienceAddons;
    public bool sizeFetched;

    // Events
    public static event Action OnAddonTypeSelected;

    // Coroutine
    public bool isApiOperationRunning;

    #region Initialize
    public override void Initialize(int experienceId)
    {
        this.experienceId = experienceId;
        addonElements = new List<UIAddon>();
        addonSizes = new List<string>();
        sizeFetched = false;
        // Load strings into new component
        LocalizationController.instance.ApplyLocale();
        // Update InputController with experience addons
        AddonsController.instance.GetExperienceAddonsData(experienceId);
        // Fill in addon types
        FillTypes();
        // Fill in addons of experience
        FillExperienceAddons();

        initialized = true;
    }

    public override void RefreshConfigurationMenu()
    {
        base.RefreshConfigurationMenu();
        if (addonTypeDropdown.dropdown.value == 0)
        {
            addonTypeDropdown.dropdown.captionText.text = LocalizationController.instance.FetchString("baseStrings", "addons_selector_defaulttype");
        }
    }

    private void FillTypes()
    {
        // Create list of Dropdown with types of addons
        HashSet<string> uniqueAddonTypes = new HashSet<string>();

        foreach (Addon addon in AddonsController.instance.allExperiencesAddonData[experienceId].experienceAddonData.addons)
        {
            if (!string.IsNullOrEmpty(addon.addonType))
            {
                uniqueAddonTypes.Add(addon.addonType);
            }
        }
        
        List<string> types = uniqueAddonTypes.ToList();

        addonTypeDropdown.FillDropdown(types, "Types");
    }

    private void FillExperienceAddons()
    {
        if (!(addonElements.Count == 0))
        {
            return;
        }
        // Get addons file
        experienceAddons = AddonsController.instance.GetExperienceAddons(experienceId).addonsData.addons;

        if (experienceAddons == null)
        {
            return;
        }

        // Create one UIAddon for each Addon
        for (int i = 0; i < experienceAddons.Count; i++)
        {
            // Set individual addon info
            UIAddon uiAddon = Instantiate(addonPrefab, addonContainer.transform).GetComponent<UIAddon>();
            uiAddon.Initialize(i, experienceAddons[i], this);
            ListUtils.InsertAtOrFill(addonElements, experienceAddons[i].addonId, uiAddon); // Makes sure it is inserted at id position, every addon of an application has to have a distinct id

        }
    }

    #endregion

    #region Data Operation

    // Turn on and off respective binders using mode
    public void UpdateAddons()
    {
        activeAddonElements = new List<UIAddon>();
        // Get dropdown option
        string type = addonTypeDropdown.dropdown.options[addonTypeDropdown.dropdown.value].text;
        // Enable and disable respective addonElements
        int enabledAddonElements = 0;
        foreach (UIAddon addonElement in addonElements)
        {
            if (addonElement.addonType == type)
            {
                addonElement.gameObject.SetActive(true);
                enabledAddonElements++;
                activeAddonElements.Add((addonElement));
            }
            else
            {
                addonElement.gameObject.SetActive(false);
            }
        }
    }

    #endregion

    #region Persistance

    public void UpdateAddonPersistance(Addon addon, int addonId)
    {
        // Modify the data
        SetAddon(addon, addonId);
        // Save and send the data to experience
        AddonsController.instance.UpdateAllExperienceAddons(experienceAddons, experienceId);
    }

    private void SetAddon(Addon addon, int addonId)
    {
        experienceAddons[addonId] = addon;
    }

    #endregion
}
