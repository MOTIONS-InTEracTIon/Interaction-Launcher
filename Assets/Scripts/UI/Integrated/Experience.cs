using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.NetworkInformation;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;


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

    // Data
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

    #region Data Operations
    public IEnumerator Initialize()
    {
        // Clear information
        ClearData();
        // Refresh strings
        LocalizationController.instance.ApplyLocale();
        // Set up images
        if (mediaCarousel != null)
        {
            mediaCarousel.Clear();
            // Get list and add to mediaCarousel
            mediaCarousel.AddElement(MediaController.instance.FetchMedia(experienceId));
        }

        // Set repo info
        SetRepoInfo();
        // Get version //// TO GET THIS TO WORK I NEED A WAY TO DOWNLOAD THE VERSIONS TO COMPARE FIRST
        yield return StartCoroutine(GetLatestVersion());

        initialized = true;
    }

    private void ClearData()
    {
        // Reset all data
        actualVersion = "";
        latestVersion = "";
        status.text = "...";
        currentVersionText.text = "...";
        latestVersionText.text = "...";
        githubOwner = null;
        githubRepo = null;
        githubAuthorizationKey = null;
    }

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

    #endregion

    #region API fetching

    // Get latest version from github and compares with the one downloaded
    private IEnumerator GetLatestVersion()
    {
        // Set github strings
        SetRepoInfo();
        // Get actual version
        GetActualRelease();
        // Get latest version
        yield return StartCoroutine(GetLatestVersionCoroutine());
    }

    // Checks for actual release from build folder
    private void GetActualRelease()
    {
        actualVersion = "None";
        string versionPath = Path.GetDirectoryName(Application.dataPath) + "/version.txt";
        if (File.Exists(versionPath))
        {
            currentVersionText.text = File.ReadAllText(versionPath);
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
                        SetStatus("baseStrings", "versionDownloadNeeded");
                    }
                    else
                    {
                        if (actualVersion == releaseInfo.tag_name)
                        {
                            SetStatus("baseStrings", "versionUpToDate");
                        }
                        else
                        {
                            SetStatus("baseStrings", "versionUpdateNeeded");
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
    }
    private IEnumerator GetLatestReleaseCoroutine()
    {
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

        // Turn resultFolders into paths
        List<string> resultPaths = new List<string>();
        for (int i = 0; i < resultFolders.Count; i++)
        {
            resultPaths.Add(Application.dataPath + "/" + resultFolders[i]);
        }

        // Delete all files from application path except the output folders specified in resultFolders
        if (Directory.Exists(buildPath))
        {
            string[] applicationFilesAndSubfolders = Directory.GetFileSystemEntries(buildPath);
            foreach (string item in applicationFilesAndSubfolders)
            {
                if (resultFolders.Contains(item))
                {
                    continue;
                }

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
    }

    private IEnumerator DownloadAsset(string downloadUrl, string downloadName)
    {
        // Clear before download
        ClearFolders();
        // Download
        SetStatus("baseStrings", "download");
        string downloadFolderPath = Application.streamingAssetsPath + "/" + experienceId.ToString() + "/download";
        string downloadPath = downloadFolderPath + "/" + downloadName + ".rar";
        // Build path is different for main app
        string buildFolderPath;
        if (experienceId == 0)
        {
            buildFolderPath = Path.GetDirectoryName(Application.dataPath);
        }
        else
        {
            buildFolderPath = Application.streamingAssetsPath + "/" + experienceId.ToString() + "/build";
        }
        // Is this a clear download or an update?
        bool update = false;
        if (Directory.Exists(buildFolderPath))
        {
            update = true;
        }


        // Verify that it exists
        if (!Directory.Exists(downloadFolderPath))
        {
            Directory.CreateDirectory(downloadFolderPath);
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
            SetStatus("baseStrings", "unzip");
            yield return StartCoroutine(UnzipInstall(downloadPath, buildFolderPath, downloadName));
            // After handling the file and putting it into build, download is officially completed and a file containing the tag version must be created
            File.WriteAllText(Path.GetDirectoryName(Application.dataPath) + "/version.txt", downloadName);
            SetStatus("baseStrings", "install");
            if (update)
            {
                SetStatus("baseStrings", "update");
            }
        }


    }

    // Needs path for the zip
    private IEnumerator UnzipInstall(string zipPath, string unzipPath, string versionTag)
    {
        string buildPath = Application.streamingAssetsPath + "/" + experienceId.ToString() + "/build/";

        // Verify that it exists
        if (!Directory.Exists(buildPath))
        {
            Directory.CreateDirectory(buildPath);
        }
        // Unzip
        if (!UnzipperHelper.Unzip(zipPath, unzipPath))
        {
            SetStatus("baseStrings", "unzip_error");
            yield break;
        }
        else
        {
            // Write version for successfull install 
            SetStatus("baseStrings", "unzip_done");
        }

        yield return null;
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
