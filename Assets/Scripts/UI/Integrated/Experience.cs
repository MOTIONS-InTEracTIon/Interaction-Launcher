using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.NetworkInformation;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using static UnityEditor.Progress;

[RequireComponent(typeof(FadeUI))]
public class Experience : MonoBehaviour
{
    // Components
    [SerializeField] public CarouselScrollview mediaCarousel;
    [SerializeField] public TextMeshProUGUI status; // Info Components 
    [SerializeField] public LocaleText statusTextLocale;
    [SerializeField] public LocaleText currentVersionTextLocale;
    [SerializeField] public TextMeshProUGUI currentVersionText;
    [SerializeField] public TextMeshProUGUI latestVersionText;
    [SerializeField] public ProgressBar downloadProgressBar;
    [SerializeField] public LocaleTextButton launchButton;
    [SerializeField] public Button downloadButton;

    // Data
    private Process experienceProcess;
    public string executableFilePath;

    public string actualVersion;
    public string latestVersion;
    

    // Settings
    public int experienceId;
    private bool initialized;
    private string githubOwner;
    private string githubRepo;
    private string githubAuthorizationKey;
    // Folders inside Data (Assets) that will be not deleted when updating or downloading
    public List<string> resultFolders;
    // Github auth

    // Coroutine
    private bool isApiOperationRunning;
    private bool isExperienceRunning;


    #region Data Operations
    public IEnumerator Initialize()
    {
        // Clear information
        ClearData();
        // Refresh strings
        LocalizationController.instance.ApplyLocale();
        // Set up data THIS WILL CHANGE TO A JSON THAT STORES DATA ABOUT THE APP, LIKE THE EXECUTABLE PATH AND NAME
        // For the time being, change this directly in the inspector
        // Set up images THIS WILL CHANGE TO A JSON THAT STORES DATA ABOUT THE APP, LIKE A LIST OF MEDIA FILES
        if (mediaCarousel != null)
        {
            mediaCarousel.Clear();
            // Get list and add to mediaCarousel
            mediaCarousel.AddElement(MediaController.instance.FetchMedia(experienceId));
        }

        // Set repo info
        SetRepoInfo();
        // Get version
        yield return StartCoroutine(GetLatestVersion());

        initialized = true;
    }

    private void ClearData()
    {
        // Reset all data
        isApiOperationRunning = false;
        actualVersion = "";
        latestVersion = "";
        status.text = "...";
        currentVersionText.text = "...";
        latestVersionText.text = "...";
        githubOwner = null;
        githubRepo = null;
        githubAuthorizationKey = null;
        SetLaunchButton(false);
        SetDownloadButton(false);
    }

    // Experience launching
    public void LaunchExperience()
    {
        // Starting setup
        if (isApiOperationRunning)
        {
            return;
        }

        SetLaunchButton(false);
        SetDownloadButton(false);

        // Inject inputs
        // Injecting input logic here

        // Launch
        if(experienceProcess == null || experienceProcess.HasExited)
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
        SetLaunchButton("baseStrings", "launchButton_running");

        // Will wait for process to end
        while(!experienceProcess.HasExited)
        {
            yield return null;
        }
        isExperienceRunning = false;

        // Once its done, reenable buttons and name
        SetLaunchButton("baseStrings", "launchButton");
        SetLaunchButton(true);
        yield return StartCoroutine(GetLatestVersion());

    }

    //

    #endregion

    #region Settings

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
        status.text = status.text + " " + addedText;
    }
    // Changes actual version component in version info
    private void SetVersion(string groupKey, string stringKey)
    {
        currentVersionTextLocale.groupKey = groupKey;
        currentVersionTextLocale.stringKey = stringKey;
        currentVersionTextLocale.UpdateText();
    }
    private void SetLaunchButton(string groupKey, string stringKey)
    {
        launchButton.localeText.groupKey = groupKey;
        launchButton.localeText.stringKey = stringKey;
        launchButton.localeText.UpdateText();
    }
    private void SetRepoInfo()
    {
        githubOwner = Environment.GetEnvironmentVariable(experienceId.ToString() + "_GITHUB_OWNER", EnvironmentVariableTarget.User);
        githubRepo = Environment.GetEnvironmentVariable(experienceId.ToString() + "_GITHUB_REPO", EnvironmentVariableTarget.User);
        githubAuthorizationKey = Environment.GetEnvironmentVariable(experienceId.ToString() + "_GITHUB_AUTHORIZATION_KEY", EnvironmentVariableTarget.User);
    }
    private bool CheckRepoInfo()
    {
        if (githubOwner == null)
        {
            return false;
        }

        if (githubRepo == null)
        {
            return false;
        }

        if (githubAuthorizationKey == null)
        {
            return false;
        }

        return true;
    }

    private void SetLaunchButton(bool activate)
    {
        if (launchButton != null)
        {
            if (activate)
            {
                launchButton.button.interactable = true;
            }
            else
            {
                launchButton.button.interactable = false;
            }
        }
    }

    private void SetDownloadButton(bool activate)
    {
        if (activate)
        {
            downloadButton.interactable = true;
        }
        else
        {
            downloadButton.interactable = false;
        }
    }

    #endregion

    #region API fetching

    // Get latest version from github and compares with the one downloaded
    private IEnumerator GetLatestVersion()
    {
        // Set github strings
        SetRepoInfo();
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
        }
        else
        {
            SetVersion("baseStrings", "versionMissing");
        }

    }
    // Downloads latest release from github
    private void GetLatestRelease()
    {
        if (initialized)
        {
            StartCoroutine(GetLatestReleaseCoroutine());
        }
    }

    // Github API methods
    private IEnumerator GetLatestVersionCoroutine()
    {
        if(isApiOperationRunning)
        {
            yield break;
        }
        isApiOperationRunning = true;

        if (CheckRepoInfo())
        {
            SetStatus("baseStrings","info");
            string url = $"https://api.github.com/repos/{githubOwner}/{githubRepo}/releases/latest";

            UnityWebRequest www = UnityWebRequest.Get(url);
            www.SetRequestHeader("Authorization", $"token {githubAuthorizationKey}");
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
                    // If the version is correctly downloaded, you compare it to know if download or update is needed
                    SetStatus("baseStrings", "info_done");
                    if(actualVersion == "None")
                    {
                        // There is not a version that can be launched, so launch stays disabled and download is enabled
                        SetDownloadButton(true); // UI Update
                        SetStatus("baseStrings", "versionDownloadNeeded");
                    }
                    else
                    {
                        // There is a version that can be launched so enable the launch button but only on experiences that have a launch button
                        SetLaunchButton(true); // UI Update
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
        }
        else
        {

            SetStatus("baseStrings", "info_error", LocalizationController.instance.FetchString("baseStrings", "credentials_error"));
            ErrorController.instance.ShowError(LocalizationController.instance.FetchString("baseStrings", "info_error") + LocalizationController.instance.FetchString("baseStrings", "credentials_error"), 5);
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

        if (CheckRepoInfo())
        {
            string url = $"https://api.github.com/repos/{githubOwner}/{githubRepo}/releases/latest";

            UnityWebRequest www = UnityWebRequest.Get(url);
            www.SetRequestHeader("Authorization", $"token {githubAuthorizationKey}");
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
        }
        else
        {
            SetStatus("baseStrings", "info_error", LocalizationController.instance.FetchString("baseStrings", "credentials_error"));
            ErrorController.instance.ShowError(LocalizationController.instance.FetchString("baseStrings", "info_error") + LocalizationController.instance.FetchString("baseStrings", "credentials_error"), 5);
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
        www.SetRequestHeader("Authorization", $"token {githubAuthorizationKey}");
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
            if(!(experienceId == 0))
            {
                SetStatus("baseStrings", "install");
                yield return StartCoroutine(UnzipInstall(downloadPath, buildFolderPath));
            }

            // After handling the file and putting it into build, download is officially completed and a file containing the tag version must be created
            if(!(experienceId == 0))
            {
                SetLaunchButton(true); // UI Update
                SetDownloadButton(false); // UI Update

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
        yield return StartCoroutine(UnzipCoroutine(zipPath,unzipPath));
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
    #endregion
}
