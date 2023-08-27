using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net.NetworkInformation;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Vortices;

// Configures a experience fetched by the Experience controller allowing downloading, updating, button mapping and launching among other features
// NOTE: Will work only with public repositories, no authentication keys required
// NOTE: Repository must have a folder called Launcher/Localization/ with one or more localization files in the format locale-<lang> and these will be picked up by the launcher
[RequireComponent(typeof(FadeUI))]
public class Experience : MonoBehaviour
{
    // Components
    [SerializeField] public LocaleText experienceTitle;
    [SerializeField] public LocaleText experienceDescription;
    [SerializeField] public CarouselScrollview mediaCarousel; // Integrated
    [SerializeField] public InputConfiguration inputConfigurationMenu; // Integrated
    [SerializeField] public LocaleTextButton inputConfigurationButton;
    [SerializeField] public AboutConfiguration aboutConfigurationMenu; // Integrated
    [SerializeField] public LocaleTextButton aboutConfigurationButton;
    [SerializeField] public LocaleTextButton launchButton;
    [SerializeField] public GameObject languageDropdown;


    [SerializeField] public TextMeshProUGUI status; // Info Components 
    [SerializeField] public LocaleText statusTextLocale;
    [SerializeField] public LocaleText currentVersionTextLocale;
    [SerializeField] public TextMeshProUGUI currentVersionText;
    [SerializeField] public TextMeshProUGUI latestVersionText;
    [SerializeField] public ProgressBar downloadProgressBar;
    [SerializeField] public Button downloadButton;

    // Data
    private Process experienceProcess;
    public string executableFilePath;

    public string actualVersion;
    public string latestVersion;

    public ConfigurationMenu actualOpenConfigurationMenu; 


    // Settings
    public int experienceId;
    private List<string> experienceStrings;
    // Folders inside Data (Assets) that will be not deleted when updating or downloading
    public List<string> resultFolders;
    // Github 
    private string githubOwner;
    private string githubRepo;
    private List<string> imageUrls;

    public bool initialized;

    // Coroutine
    private bool isApiOperationRunning;
    private bool isExperienceRunning;
    private bool isOpenMenuOperationRunning;

    private void OnEnable()
    {
        ConfigurationMenu.onConfigurationMenuClosed += ClearActualMenu;
    }

    private void OnDisable()
    {
        ConfigurationMenu.onConfigurationMenuClosed -= ClearActualMenu;
    }

    #region Initialize
    public IEnumerator Initialize(int experienceId, ExperienceData experienceData)
    {
        ErrorController.instance.ShowLoading(true);
        // Clear information
        ClearData();

        // Set up data THIS WILL CHANGE TO A JSON THAT STORES DATA ABOUT THE APP, LIKE THE EXECUTABLE PATH AND NAME
        yield return StartCoroutine(SetExperienceData(experienceId, experienceData.executableName, experienceData.resultFolders, experienceData.githubOwner, experienceData.githubRepo, experienceData.imageUrls));
        // Get version
        yield return StartCoroutine(GetLatestVersion());

        ErrorController.instance.ShowLoading(false);
        initialized = true;
    }

    private void ClearData()
    {
        // Reset all data
        experienceId = -1;
        isApiOperationRunning = false;
        actualVersion = "";
        latestVersion = "";
        status.text = "...";
        currentVersionText.text = "...";
        latestVersionText.text = "...";
        SetLaunchButton(false);
        SetDownloadButton(false);
        SetExperienceFeatureButtons(false);
    }

    private IEnumerator SetExperienceData(int experienceId, string executableName, List<string> resultFolders, string githubOwner, string githubRepo, List<string> imageUrls)
    {
        if (experienceId < 0 || resultFolders == null || githubOwner == null || githubRepo == null)
        {
            ErrorController.instance.ShowError(LocalizationController.instance.FetchString("baseStrings", "experience_error"), 5);
            yield break;
        }

        // Set up basic info
        SetupExperienceInfo(experienceId, executableName, resultFolders, githubOwner, githubRepo);
        // Set up text
        yield return StartCoroutine(SetupExperienceStrings());
        // Set up conditional components
        SetupExperienceComponents();
        // Set up bundles?

        // Set up images THIS WILL CHANGE TO A JSON THAT STORES DATA ABOUT THE APP, LIKE A LIST OF MEDIA FILES
        if (mediaCarousel.gameObject.activeInHierarchy)
        {
            if (imageUrls == null)
            {
                yield break;
            }

            mediaCarousel.Clear();
            // Get list and add to mediaCarousel
            yield return MediaController.instance.FetchMedia(experienceId, imageUrls);
            mediaCarousel.AddElement(MediaController.instance.media);
        }
    }

    private void SetupExperienceInfo(int experienceId, string executableName, List<string> resultFolders, string githubOwner, string githubRepo)
    {
        this.experienceId = experienceId;
        this.executableFilePath = executableName;
        this.resultFolders = resultFolders;
        this.githubOwner = githubOwner;
        this.githubRepo = githubRepo;

        // Create a directory if there is none
        string path = Application.streamingAssetsPath + "/" + experienceId;
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }

    private void SetupExperienceComponents()
    {
        if (experienceId == 0)
        {
            mediaCarousel.gameObject.SetActive(false);
            launchButton.transform.GetChild(0).gameObject.SetActive(false);
            inputConfigurationButton.transform.GetChild(0).gameObject.SetActive(false);
            aboutConfigurationButton.transform.GetChild(0).gameObject.SetActive(false);

            languageDropdown.transform.GetChild(0).gameObject.SetActive(true);
            TMP_Dropdown dropdown = languageDropdown.GetComponentInChildren<TMP_Dropdown>();
            LocalizationController.instance.ApplyLocaleDropdown(dropdown);

            CanvasGroup canvas = GetComponent<CanvasGroup>();
            canvas.alpha = 1;
            canvas.blocksRaycasts = true;

        }
        else
        {
            languageDropdown.transform.GetChild(0).gameObject.SetActive(false);
        }
    }

    private IEnumerator SetupExperienceStrings()
    {
        // The LocaleText in the Experiences need the experienceId to fetch strings
        experienceTitle.groupKey = experienceId.ToString();
        experienceDescription.groupKey = experienceId.ToString();
        yield return StartCoroutine(GetLocalizationFiles());
    }

    #endregion

    #region Experience Launching
    public void LaunchExperience()
    {
        // Starting setup
        if (isApiOperationRunning || isOpenMenuOperationRunning)
        {
            return;
        }

        SetLaunchButton(false);
        SetExperienceFeatureButtons(false);
        SetDownloadButton(false);

        // Inject inputs
        // Injecting input logic here

        // Launch
        if (experienceProcess == null || experienceProcess.HasExited)
        {
            StartCoroutine(RunProcess());
        }
    }

    private IEnumerator RunProcess()
    {
        // Run the process
        experienceProcess = new Process();
        experienceProcess.StartInfo.FileName = Application.streamingAssetsPath + "/" + experienceId.ToString() + "/build/" + executableFilePath;
        experienceProcess.Start();
        isExperienceRunning = true;

        // Change Launch text to running
        SetLaunch("baseStrings", "launchButton_running");

        // Will wait for process to end
        while (!experienceProcess.HasExited)
        {
            yield return null;
        }
        isExperienceRunning = false;

        // Once its done, reenable buttons and name
        SetLaunch("baseStrings", "launchButton");
        SetLaunchButton(true);
        SetExperienceFeatureButtons(true);
        yield return StartCoroutine(GetLatestVersion());

    }

    #endregion

    #region Configuration Menu windows
    public void OpenMenu(string menuName)
    {
        if (isOpenMenuOperationRunning)
        {
            return;
        }
        isOpenMenuOperationRunning = true;
        StartCoroutine(OpenMenuCoroutine(menuName));
    }
    public IEnumerator OpenMenuCoroutine(string menuName)
    {
        ConfigurationMenu configurationMenuToOpen = null;

        // Open menu
        switch (menuName)
        {
            case "input":
                configurationMenuToOpen = inputConfigurationMenu;
                break;
            case "about":
                configurationMenuToOpen = aboutConfigurationMenu;
                break;
            case "bundles":
                
                break;
        }

        // Close menu if one is already open
        if (actualOpenConfigurationMenu != null)
        {
            // If the menu chosen is the one open, dont do anything
            if (actualOpenConfigurationMenu == configurationMenuToOpen)
            {
                isOpenMenuOperationRunning = false;
                yield break;
            }
            yield return StartCoroutine(OpenMenu(actualOpenConfigurationMenu));
        }

        yield return StartCoroutine(OpenMenu(configurationMenuToOpen));

        isOpenMenuOperationRunning = false;
    }

    private IEnumerator OpenMenu(ConfigurationMenu configurationMenu)
    {
        bool activate = true;

        if (configurationMenu.isOpen)
        {
            activate = false;
        }

        if (activate)
        {
            actualOpenConfigurationMenu = configurationMenu;
            configurationMenu.isOpen = true;
            configurationMenu.gameObject.SetActive(true);
            // Initialize component
            if (!configurationMenu.initialized)
            {
                configurationMenu.Initialize(experienceId);
            }

            // FadeIn component
            FadeUI configurationMenuFader = configurationMenu.gameObject.GetComponent<FadeUI>();
            yield return StartCoroutine(configurationMenuFader.FadeIn());
        }
        else
        {
            // FadeOut component
            FadeUI configurationMenuFader = configurationMenu.gameObject.GetComponent<FadeUI>();
            yield return StartCoroutine(configurationMenuFader.FadeOut());
            // Clear component
            configurationMenu.isOpen = false;
            configurationMenu.gameObject.SetActive(false);
        }
    }

    private void ClearActualMenu()
    {
        actualOpenConfigurationMenu = null;
    }

    #endregion

    #region Component setup

    // Changes status component in version info
    private void SetStatus(string groupKey, string stringKey)
    {
        statusTextLocale.groupKey = groupKey;
        statusTextLocale.stringKey = stringKey;
        statusTextLocale.UpdateText();
    }
    private void SetStatus(string groupKey, string stringKey, string addedText)
    {
        SetStatus(groupKey, stringKey);
        statusTextLocale.textBox.text = statusTextLocale.textBox.text + " " + addedText;
    }
    private void SetLaunch(string groupKey, string stringKey)
    {
        launchButton.localeText.groupKey = groupKey;
        launchButton.localeText.stringKey = stringKey;
        launchButton.localeText.UpdateText();
    }

    private void SetLaunchButton(bool activate)
    {
        if (launchButton == null)
        {
            return;
        }

        if (activate)
        {
            launchButton.button.interactable = true;
        }
        else
        {
            launchButton.button.interactable = false;
        }
    }

    private void SetDownloadButton(bool activate)
    {
        if (launchButton == null)
        {
            return;
        }

        if (activate)
        {
            downloadButton.interactable = true;
        }
        else
        {
            downloadButton.interactable = false;
        }
    }

    private void SetInputButton(bool activate)
    {
        if (inputConfigurationButton == null)
        {
            return;
        }

        if (activate)
        {
            inputConfigurationButton.button.interactable = true;
        }
        else
        {
            inputConfigurationButton.button.interactable = false;
        }
    }

    private void SetAboutButton(bool activate)
    {
        if (aboutConfigurationButton == null)
        {
            return;
        }

        if (activate)
        {
            aboutConfigurationButton.button.interactable = true;
        }
        else
        {
            aboutConfigurationButton.button.interactable = false;
        }
    }

    private void SetExperienceFeatureButtons(bool activate)
    {
        SetInputButton(activate);
        SetAboutButton(activate);
        // SetBundlesButton(activate);
    }
    #endregion

    #region API fetching
    // Get localization files of the experience to set up strings
    private IEnumerator GetLocalizationFiles()
    {
        yield return StartCoroutine(GetLocalizationFilesCoroutine());
        // Refresh strings
        LocalizationController.instance.LoadExperienceStrings(experienceId);
        LocalizationController.instance.ApplyLocale();
    }

    // Get latest version from github and compares with the one downloaded
    private IEnumerator GetLatestVersion()
    {
        // Get actual version
        GetCurrentRelease();
        // Get latest version
        yield return StartCoroutine(GetLatestVersionCoroutine());
    }
    // Checks for current release from build folder
    private void GetCurrentRelease()
    {
        actualVersion = "None";
        string versionPath;
        // Get version.txt
        if (experienceId == 0)
        {
            versionPath = Path.GetDirectoryName(Application.dataPath) + "/version.txt";
        }
        else
        {
            versionPath = Application.streamingAssetsPath + "/" + experienceId.ToString() + "/build" + "/version.txt";
        }

        // Set current release
        if (File.Exists(versionPath))
        {
            currentVersionText.text = File.ReadAllText(versionPath);
            actualVersion = currentVersionText.text;
            // There is a version that can be launched so enable the launch button but only on experiences that have a launch button
            SetExperienceFeatureButtons(true);
        }
        else
        {
            currentVersionText.text = LocalizationController.instance.FetchString("baseStrings", "versionMissing");
        }

    }

    // Downloads latest release from github
    public void GetLatestRelease()
    {
        if (initialized)
        {
            StartCoroutine(GetLatestReleaseCoroutine());
        }
    }

    // Github API methods
    private IEnumerator GetLocalizationFilesCoroutine()
    {
        if (isApiOperationRunning)
        {
            yield break;
        }
        isApiOperationRunning = true;

        SetStatus("baseStrings", "data");

        string url = $"https://api.github.com/repos/{githubOwner}/{githubRepo}/contents/Launcher/Localization";

        UnityWebRequest www = UnityWebRequest.Get(url);

        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            string responseJson = www.downloadHandler.text;
            List<GithubFile> files = JsonConvert.DeserializeObject<List<GithubFile>>(responseJson);

            foreach (GithubFile file in files)
            {
                // Check if the file is downloaded already
                string downloadFolderPath = Application.streamingAssetsPath + "/" + experienceId.ToString();
                string path = downloadFolderPath + "/" + file.name;

                if (!File.Exists(path))
                {
                    yield return StartCoroutine(DownloadLocalizationFiles(file.download_url, file.name, path));
                }
            }
            SetStatus("baseStrings", "data_done");
        }
        else
        {
            SetStatus("baseStrings", "data_error", www.error);
            ErrorController.instance.ShowError(LocalizationController.instance.FetchString("baseStrings", "data_error") + www.error, 5);
        }

        isApiOperationRunning = false;
    }
    private IEnumerator GetLatestVersionCoroutine()
    {
        if (isApiOperationRunning)
        {
            yield break;
        }
        isApiOperationRunning = true;

        
        SetStatus("baseStrings", "info");
        // DYNAMIC 
        string url = $"https://api.github.com/repos/{githubOwner}/{githubRepo}/releases/latest";

        UnityWebRequest www = UnityWebRequest.Get(url);
        www.SetRequestHeader("User-Agent", "InTeraciOn Launcher");
        www.SetRequestHeader("Accept", "application/vnd.github.v3+json");

        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            string responseJson = www.downloadHandler.text;
            ReleaseInfo releaseInfo = JsonUtility.FromJson<ReleaseInfo>(responseJson);
            if (releaseInfo.tag_name != null && releaseInfo.tag_name != "")
            {
                latestVersionText.text = releaseInfo.tag_name + " (" + SizeFormatter.FormatSize(releaseInfo.assets[0].size) + ")";
                latestVersion = releaseInfo.tag_name;
                // If the version is correctly downloaded, you compare it to know if download or update is needed
                SetStatus("baseStrings", "info_done");
                if (actualVersion == "None")
                {
                    // There is not a version that can be launched, so launch stays disabled and download is enabled
                    SetDownloadButton(true); // UI Update
                    SetStatus("baseStrings", "versionDownloadNeeded");
                }
                else
                {
                    SetLaunchButton(true);
                    SetExperienceFeatureButtons(true);
                    if (actualVersion == releaseInfo.tag_name)
                    {
                        if (!(experienceId == 0))
                        {
                            SetStatus("baseStrings", "versionUpToDate");
                        }
                        else
                        {
                            SetStatus("baseStrings", "versionUpToDateMain");
                        }

                    }
                    else
                    {
                        // There is a version that can be launched so launch was enabled and download is enabled
                        SetDownloadButton(true);
                        if (!(experienceId == 0))
                        {
                            SetStatus("baseStrings", "versionUpdateNeeded");
                        }
                        else
                        {
                            SetStatus("baseStrings", "versionUpdateMainNeeded");
                        }
                    }
                }

            }
        }
        else
        {
            SetStatus("baseStrings", "info_error", www.error);
            ErrorController.instance.ShowError(LocalizationController.instance.FetchString("baseStrings", "info_error") + www.error, 5);
        }

        isApiOperationRunning = false;
    }
    private IEnumerator GetLatestReleaseCoroutine()
    {
        if (isApiOperationRunning)
        {
            yield break;
        }
        isApiOperationRunning = true;

        SetStatus("baseStrings", "info");

        string url = $"https://api.github.com/repos/{githubOwner}/{githubRepo}/releases/latest";

        UnityWebRequest www = UnityWebRequest.Get(url);
        www.SetRequestHeader("User-Agent", "InTeractiOn Launcher");
        www.SetRequestHeader("Accept", "application/vnd.github.v3+json");

        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            string responseJson = www.downloadHandler.text;
            ReleaseInfo releaseInfo = JsonUtility.FromJson<ReleaseInfo>(responseJson);
            // Parse the JSON response to get the asset's download URL
            string downloadUrl = releaseInfo.assets[0].url;
            string downloadTitle = releaseInfo.tag_name;


            yield return StartCoroutine(DownloadAsset(downloadUrl, downloadTitle));
        }
        else
        {
            SetStatus("baseStrings", "info_error", www.error);
            ErrorController.instance.ShowError(LocalizationController.instance.FetchString("baseStrings", "info_error") + www.error, 5);
        }
       
        isApiOperationRunning = false;
    }

    private void ClearFolders()
    {
        string downloadPath = Application.streamingAssetsPath + "/" + experienceId.ToString() + "/download/";
        string buildPath = Application.streamingAssetsPath + "/" + experienceId.ToString() + "/build/";

        // Delete all files from download path
        if (Directory.Exists(downloadPath))
        {
            string[] downloadFilesAndSubfolders = Directory.GetFileSystemEntries(downloadPath);
            foreach (string item in downloadFilesAndSubfolders)
            {
                if (File.Exists(item))
                {
                    File.Delete(item);
                }
                else if (Directory.Exists(item))
                {
                    Directory.Delete(item, true);
                }
            }
        }

        // Delete all files from application path except the output folders specified in resultFolders
        if (Directory.Exists(buildPath))
        {
            FolderHelper.DeleteFoldersRecursively(buildPath, resultFolders);
        }
    }

    private IEnumerator DownloadLocalizationFiles(string url, string fileName, string path)
    {
        UnityWebRequest www = UnityWebRequest.Get(url);
        www.SetRequestHeader("User-Agent", "InTeractiOn Launcher");
        www.SetRequestHeader("Accept", "application/vnd.github.v3+json");

        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            string fileContent = www.downloadHandler.text;

            File.WriteAllText(path, fileContent);
        }
        else
        {
            SetStatus("baseStrings", "data_error", www.error);
            ErrorController.instance.ShowError(LocalizationController.instance.FetchString("baseStrings", "data_error") + www.error, 5);
        }
    }

    private IEnumerator DownloadAsset(string downloadUrl, string downloadName)
    {
        // Clear before download
        ClearFolders();
        // Download
        SetStatus("baseStrings", "download");
        string downloadFolderPath = Application.streamingAssetsPath + "/" + experienceId.ToString() + "/download";
        string downloadPath = downloadFolderPath + "/" + downloadName + ".zip";
        string buildFolderPath = Application.streamingAssetsPath + "/" + experienceId.ToString() + "/build";
        string versionPath = buildFolderPath + "/version.txt";

        // Is this a clear download or an update?
        bool update = false;
        if (actualVersion != "None")
        {
            update = true;
        }


        // Verify that it exists
        if (!Directory.Exists(downloadFolderPath))
        {
            Directory.CreateDirectory(downloadFolderPath);
        }
        // Delete version if download fails midway
        if (File.Exists(versionPath))
        {
            File.Delete(versionPath);
        }

        UnityWebRequest www = UnityWebRequest.Get(downloadUrl);
        www.SetRequestHeader("User-Agent", "InTeraciOn Launcher");
        www.SetRequestHeader("Accept", "application/octet-stream");

        DownloadHandlerFile downloadHandler = new DownloadHandlerFile(downloadPath);
        www.downloadHandler = downloadHandler;

        www.SendWebRequest();
        //Track progress download
        float lastProgress = 0f;
        while (!www.isDone)
        {
            float currentProgress = www.downloadProgress;
            // Only change last progress if there is progress
            if (currentProgress != lastProgress)
            {
                downloadProgressBar.UpdateFill(currentProgress);
                lastProgress = currentProgress;
            }
            yield return null;
        }

        if (!(www.result == UnityWebRequest.Result.Success))
        {
            SetStatus("baseStrings", "download_error", www.error);
            ErrorController.instance.ShowError(LocalizationController.instance.FetchString("baseStrings", "download_error") + www.error, 5);
            yield break;
        }
        else
        {
            SetStatus("baseStrings", "download_done");
            // If the file is downloaded successfully it must be handled to be used (Unzipping, etc)
            if (!(experienceId == 0))
            {
                SetStatus("baseStrings", "install");
                yield return StartCoroutine(UnzipInstall(downloadPath, buildFolderPath));
            }

            // After handling the file and putting it into build, download is officially completed and a file containing the tag version must be created
            if (!(experienceId == 0))
            {
                SetLaunchButton(true); // UI Update
                SetDownloadButton(false); // UI Update
                SetExperienceFeatureButtons(true);

                File.WriteAllText(versionPath, downloadName);
                SetStatus("baseStrings", "install_done");
                if (update)
                {
                    SetStatus("baseStrings", "update_done");
                }
                currentVersionText.text = downloadName;

                // Delete downloaded file
                File.Delete(downloadPath);
            }
            else
            {
                SetStatus("baseStrings", "updatemain_done");
            }
        }
    }
    // Needs path for the zip
    private IEnumerator UnzipInstall(string zipPath, string unzipPath)
    {
        if (isExperienceRunning || !Path.GetExtension(zipPath).Equals(".zip"))
        {
            SetStatus("baseStrings", "install_error");
            yield break;
        }

        string buildPath = Application.streamingAssetsPath + "/" + experienceId.ToString() + "/build/";

        // Verify that it exists
        if (!Directory.Exists(buildPath))
        {
            Directory.CreateDirectory(buildPath);
        }
        // Unzip
        yield return StartCoroutine(UnzipCoroutine(zipPath, unzipPath));
    }
    private IEnumerator UnzipCoroutine(string zipFilePath, string extractionPath)
    {
        using (var zipFile = new ZipFile(zipFilePath))
        {
            long totalBytes = 0;
            foreach (ZipEntry entry in zipFile)
            {
                if (!entry.IsFile)
                    continue;

                totalBytes += entry.Size;
            }

            long processedBytes = 0;
            byte[] buffer = new byte[4096 * 16]; // Larger buffer

            foreach (ZipEntry entry in zipFile)
            {
                if (!entry.IsFile)
                    continue;

                var entryStream = zipFile.GetInputStream(entry);
                var entryPath = Path.Combine(extractionPath, entry.Name);

                // Create directories if they don't exist
                Directory.CreateDirectory(Path.GetDirectoryName(entryPath));

                using (var outputStream = File.Create(entryPath))
                {
                    int bytesRead;
                    float lastProgress = 0f;
                    while ((bytesRead = entryStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        outputStream.Write(buffer, 0, bytesRead);
                        processedBytes += bytesRead;

                        float progress = (float)processedBytes / totalBytes;

                        // Only change last progress if there is progress
                        if (progress != lastProgress)
                        {
                            downloadProgressBar.UpdateFill(progress);
                            lastProgress = progress;
                        }
                    }

                    yield return null; // Yield once per file
                }
            }
        }
    }

    #endregion
}

#region API classes
[Serializable]
    public class ReleaseInfo
    {
        public string name;
        public string tag_name;
        public DateTime published_at;
        public List<AssetInfo> assets;
    }

    [Serializable]
    public class AssetInfo
    {
        public string url;
        public long size;
    }

    [Serializable]
    public class GithubFile
    {
        public string name;
        public string download_url;
    }
#endregion
