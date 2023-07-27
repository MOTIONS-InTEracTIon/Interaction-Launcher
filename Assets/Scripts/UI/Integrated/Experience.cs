using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

[RequireComponent(typeof(FadeUI))]
public class Experience : MonoBehaviour
{
    // Components
    [SerializeField] public CarouselScrollview mediaCarousel;
    [SerializeField] public TextMeshProUGUI statusText; // Info Components 
    [SerializeField] public TextMeshProUGUI currentVersionText;
    [SerializeField] public TextMeshProUGUI latestVersionText;
    [SerializeField] public Slider downloadProgressBar;

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
            // Set repo info
            SetRepoInfo();
            // Get version //// TO GET THIS TO WORK I NEED A WAY TO DOWNLOAD THE VERSIONS TO COMPARE FIRST
            //yield return StartCoroutine(GetLatestVersion());

            // ////TEST
            yield return StartCoroutine(GetLatestReleaseCoroutine());
        }
        initialized = true;
    }

    private void ClearData()
    {
        // Reset all data
        actualVersion = "";
        latestVersion = "";
        statusText.text = "...";
        currentVersionText.text = "...";
        latestVersionText.text = "...";
        githubOwner = null;
        githubRepo = null;
        githubAuthorizationKey = null;
    }

    // Changes status component in version info
    private void SetStatus(string status)
    {
        statusText.text = status;
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
        // Get latest version
        yield return StartCoroutine(GetLatestVersionCoroutine());
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
    private IEnumerator GetLatestVersionCoroutine()
        {

        SetStatus("Fetching version...");
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
            if(releaseInfo.tag_name != null && releaseInfo.tag_name != "")
            {
                latestVersionText.text = releaseInfo.tag_name;
                SetStatus("Comparing versions?");
            }
        }
        else
        {
            SetStatus("Couldn't fetch information: " + www.error);
        }
    }
    private IEnumerator GetLatestReleaseCoroutine()
    {
        SetStatus("Fetching release...");

        if(CheckRepoInfo())
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
                string downloadTitle = releaseInfo.name;


                yield return StartCoroutine(DownloadAsset(downloadUrl, downloadTitle));
            }
            else
            {
                ErrorController.instance.ShowError("ERROR: Failed to fetch release: " + www.error, 5);
            }
        }
        else
        {
            ErrorController.instance.ShowError("ERROR: Credentials not set",5);
        }


    }

    private void ClearFolders()
    {
        string downloadPath = Application.streamingAssetsPath + "/" + experienceId.ToString() + "/download/";
        string applicationPath = Application.streamingAssetsPath + "/" + experienceId.ToString() + "/application/";

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
        if (Directory.Exists(applicationPath))
        {
            string[] applicationFilesAndSubfolders = Directory.GetFileSystemEntries(applicationPath);
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
        SetStatus("Downloading release...");
        string downloadFolderPath = Application.streamingAssetsPath + "/" + experienceId.ToString() + "/download";
        string downloadPath = downloadFolderPath + "/" + downloadName + ".rar";

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


        yield return www.SendWebRequest();

        //Track progress
        float lastProgress = 0f;
        while (!www.isDone)
        {
            float currentProgress = www.downloadProgress;
            // Only change last progress if there is progress
            if (currentProgress != lastProgress)
            {
                downloadProgressBar.value = currentProgress;
                lastProgress = currentProgress;
            }
            yield return null;
        }

        if (www.result == UnityWebRequest.Result.Success)
        {
            Debug.LogError("Downloaded successfully");
        }
        else
        {
            ErrorController.instance.ShowError("ERROR: Failed to download asset: " + www.error,5);
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
    }
    #endregion
}
