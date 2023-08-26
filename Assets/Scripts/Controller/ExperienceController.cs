using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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
    [SerializeField] private GameObject experienceCardPrefab;
    // Components
    [SerializeField] private GameObject experienceCardContainer;
    [SerializeField] private GameObject experienceScrollviewContent;

    // Data
    public List<ExperienceData> allExperiencesData;

    public string filePath;
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

        // Create JSON
        filePath = Path.GetDirectoryName(Application.dataPath) + "/settings.json";

        // Load Experiences Settings
        LoadSettingsFile();
        // Fetch Experiences
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
        yield return null;
        //yield return StartCoroutine(experiences[0].Initialize(0));
    }

    private void GetExperiences()
    {
        for (int i = 0; i < allExperiencesData.Count; i++)
        {
            ExperienceData newExperienceData = allExperiencesData[i];

            Experience newExperience = Instantiate(experienceCardPrefab, experienceCardContainer.transform).GetComponent<Experience>();

            newExperience.Initialize(i, newExperienceData.resultFolders, newExperienceData.githubOwner, newExperienceData.githubRepo, newExperienceData.imageUrls);

            experiences.Add(newExperience);
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
        // VAS A INICIALIZARLO EN OTRO LADO yield return StartCoroutine(experiences[componentId].Initialize());
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

    #endregion

    #region Persistence
    private void LoadSettingsFile()
    {
        if (!File.Exists(filePath))
        {
            ErrorController.instance.ShowError(LocalizationController.instance.FetchString("baseStrings", "settings_error"), 5);

            ExperiencesData baseExperiencesData = new ExperiencesData();

            baseExperiencesData.allExperiencesData = new List<ExperienceData>();

            ExperienceData baseExperienceData = new ExperienceData();
            baseExperienceData.name = "Interaction Launcher";
            baseExperienceData.resultFolders = null;
            baseExperienceData.githubOwner = "MoriyarnnOrg"; // Change this when moving to Organization
            baseExperienceData.githubRepo = "Interaction-Launcher";
            baseExperienceData.imageUrls = null;

            baseExperiencesData.allExperiencesData.Add(baseExperienceData);

            allExperiencesData = baseExperiencesData.allExperiencesData;
            SaveSettingsFile();
            
            return ;
        }

        string json = File.ReadAllText(filePath);

        // Loads all experience data
        allExperiencesData = JsonUtility.FromJson<ExperiencesData>(json).allExperiencesData;

    }

    private void SaveSettingsFile()
    {
        ExperiencesData experiencesData = new ExperiencesData();

        experiencesData.allExperiencesData = allExperiencesData;

        string json = JsonUtility.ToJson(experiencesData);

        File.WriteAllText(filePath, json);
    }
    #endregion


}


#region Persistence classes

// Regarding experience input_mapping
[Serializable]
public class ExperiencesData
{
    public List<ExperienceData> allExperiencesData;
}

[Serializable]
public class ExperienceData
{
    public string name;
    public List<string> resultFolders;
    public string githubOwner;
    public string githubRepo;
    public List<string> imageUrls;
}

#endregion