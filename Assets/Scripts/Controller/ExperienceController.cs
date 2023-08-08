using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.UI;
using Vortices;

// Main controller of the application, it starts populating the experience list with Experience cards allowing:
// Viewing - Downloading - Updating - Launching
// To add a experience add a Experience card to container with a configured Experience script on it and the links will be made automatically
public class ExperienceController : MonoBehaviour
{
    // Prefabs
    [SerializeField] private GameObject uiExperiencePrefab;
    // Components
    [SerializeField] private GameObject experienceCardContainer;
    [SerializeField] private GameObject experienceScrollviewContent;

    // Data
    [SerializeField] private List<Experience> experiences;
    [SerializeField] private List<Toggle> experienceToggles;

    // Coroutine
    private bool isChangeCardRunning;

    // Properties
    public int actualExperienceCardId;

    // Other
    Color normalColor = new Color(0.2956568f, 0.3553756f, 0.4150943f, 1.0f);
    Color disabledColor = Color.black;

    public static ExperienceController instance;

    #region Data Operations
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

        // Experience fetching
        StartCoroutine(InitializeExperiences());
    }

    public IEnumerator InitializeExperiences()
    {
        experiences = new List<Experience>();
        // experienceToggles always starts with logo Toggle so we don't initialize it
        // Fill experiences
        GetExperiences();
        // Fill experienceToggle list with toggles
        GetExperienceToggles();
        // Initialize component
        yield return StartCoroutine(experiences[0].Initialize());
    }

    private void GetExperiences()
    {
        for (int i = 0; i < experienceCardContainer.gameObject.transform.childCount; i++) 
        {
            Experience experienceCard = experienceCardContainer.transform.GetChild(i).gameObject.GetComponent<Experience>();
            experienceCard.experienceId = i;
            LocaleText[] localeTexts = experienceCard.GetComponentsInChildren<LocaleText>();
            foreach (LocaleText text in localeTexts)
            {
                if(text.groupKey != "baseStrings")
                {
                    text.groupKey = i.ToString();
                }
            }
            experiences.Add(experienceCard);

        }
    }

    private void GetExperienceToggles()
    {
        for (int i = 1; i < experiences.Count; i++)
        {
            // Create toggle
            UIExperience uiExperience = Instantiate(uiExperiencePrefab, experienceScrollviewContent.transform).gameObject.GetComponent<UIExperience>();
            uiExperience.Init(i);
            // Add toggle to list
            experienceToggles.Add(uiExperience.selectToggle);
        }
    }


    #endregion

    #region Experience switching

    // UI Cards will change according to user configurations one by one
    public void ChangeVisibleComponent(int componentId)
    {
        StartCoroutine(ChangeComponent(componentId));
    }
    private IEnumerator ChangeComponent(int componentId)
    {
        // FadeOut actual component
        FadeUI actualCardFader = experiences[actualExperienceCardId].GetComponent<FadeUI>();
        yield return StartCoroutine(actualCardFader.FadeOut());
        // Disable actual component
        experiences[actualExperienceCardId].gameObject.SetActive(false);
        // Enable new component
        experiences[componentId].gameObject.SetActive(true);
        actualExperienceCardId = componentId;
        // Initialize component
        yield return StartCoroutine(experiences[componentId].Initialize());
        // FadeIn new component
        FadeUI newComponentFader = experiences[componentId].GetComponent<FadeUI>();
        yield return StartCoroutine(newComponentFader.FadeIn());
    }

    public void ChangeCardToggle(Toggle toggle)
    {
        if (!isChangeCardRunning)
        {
            StartCoroutine(ChangeCardToggleCoroutine(toggle));
        }
    }

    public IEnumerator ChangeCardToggleCoroutine(Toggle toggle)
    {
        isChangeCardRunning = true;

        UIExperience uiExperience = toggle.GetComponentInParent<UIExperience>();

        // Turn all toggles uninteractable with color normal except the one thats pressed which will have color disabled / except if it is the first one
        for (int i = 0; i < experienceToggles.Count; i++)
        {
            if (uiExperience.experienceId == 0) 
            {
                if(!(experienceToggles[i] == toggle))
                {
                    // They have to have color disabled normal
                    ColorBlock disabledNormal = experienceToggles[1].colors;
                    disabledNormal.disabledColor = normalColor;
                    experienceToggles[i].colors = disabledNormal;
                }
            }
            else
            {
                if (!(experienceToggles[i] == toggle) && experienceToggles[i].GetComponentInParent<UIExperience>().experienceId != 0)
                {
                    // They have to have color disabled normal
                    ColorBlock disabledNormal = experienceToggles[1].colors;
                    disabledNormal.disabledColor = normalColor;
                    experienceToggles[i].colors = disabledNormal;
                }
            }

            experienceToggles[i].interactable = false;
        }

        // Change component using toggle UIExperience id
        yield return StartCoroutine(ChangeComponent(uiExperience.experienceId));

        // Turn all toggles interactable with color normal except the one that was pressed
        for (int i = 0; i < experienceToggles.Count; i++)
        {
            if (!(experienceToggles[i] == toggle) && experienceToggles[i].GetComponentInParent<UIExperience>().experienceId != 0)
            {
                // They have to have color disabled normal
                ColorBlock disabledBlack = experienceToggles[i].colors;
                disabledBlack.disabledColor = disabledColor;
                experienceToggles[i].colors = disabledBlack;
            }

            if (!(experienceToggles[i] == toggle))
            {
                experienceToggles[i].interactable = true;
            }
        }

        isChangeCardRunning = false;
    }

    // Does initial tasks in a 

    #endregion
}
