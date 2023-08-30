using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Networking;
using UnityEngine.UI;
using static UnityEngine.InputSystem.InputBindingCompositeContext;

public class UIAddon : MonoBehaviour
{
    // Other references 
    private AddonsConfiguration addonsConfiguration;
    private LocaleOptionDropdown typeDropdown;

    // Components
    [SerializeField] TMP_InputField nameInputField;
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI sizeText;
    [SerializeField] Toggle enabledToggle;
    [SerializeField] Button downloadButton;
    [SerializeField] ProgressBar progressBar;
    [SerializeField] GameObject toggleObject;
    [SerializeField] GameObject downloadObject;

    // Data
    public int addonId;
    public string addonName;
    public string addonType;
    private bool enabled;
    private bool downloaded;
    private Addon addon;

    private string addonsPath;

    // Coroutine
    private bool isApiOperationRunning;

    #region Initialize

    public IEnumerator Initialize(int addonId, Addon addon, AddonsConfiguration inputWindow)
    {
        this.addonId = addonId;
        addonsConfiguration = inputWindow;
        // String loading
        LocalizationController.instance.ApplyLocale();
        // Load info into addon
        yield return StartCoroutine(LoadInfo(addon));
        // Disable Addon before a type is chosen
        this.gameObject.SetActive(false);
    }

    private IEnumerator LoadInfo(Addon addon)
    {
        addonsPath = Application.streamingAssetsPath + "/" + addonsConfiguration.id + "/build/Addons/";

        addonName = addon.addonName;
        addonType = addon.addonType;
        enabled = addon.enabled;

        this.addon = addon;

        nameInputField.text = addonName;

        // Get total addon size from github
        yield return StartCoroutine(GetAddonSizeCoroutine());
        // Check if it was downloaded and set enable toggle or download button
        SetDownloadedStatus();
    }
    private void SetDownloadedStatus()
    {
        // Get if downloaded
        downloaded = SetDownloadedStatus(GetAddonDownloadedFiles());
        // Update addon options
        UpdateAddonOptions();
    }

    private List<string> GetAddonDownloadedFiles()
    {
        List<string> downloadedFiles = new List<string>();

        // Checks if every file of the bundle is downloaded
        if (addonType == "Environment")
        {
            string path = addonsPath + addonType;

            // Get all files in directory
            // If there is none, return the empty list
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                return downloadedFiles;
            }

            string[] filePaths = Directory.GetFiles(path);
            List<string> fileNames = new List<string>();
            foreach (string filePath in filePaths)
            {
                fileNames.Add(Path.GetFileName(filePath));
            }
            // Check for panel
            if (fileNames.Contains(addon.addonFileNames[0]))
            {
                downloadedFiles.Add(addon.addonFileNames[0]);
            }
            // Check for scene
            if (fileNames.Contains(addon.addonFileNames[1]))
            {
                downloadedFiles.Add(addon.addonFileNames[1]);
            }
        }

        return downloadedFiles;
    }

    private bool SetDownloadedStatus(List<string> downloadedFiles)
    {
        if(downloadedFiles == null)
        {
            return false;
        }

        if(downloadedFiles.Count == addon.addonFileNames.Count)
        {
            return true;
        }

        return false;
    }

    // Changes status of download button and toggle depending on variables
    private void UpdateAddonOptions()
    {
        if (downloaded)
        {
            SetEnableToggle(true);
            SetDownloadButton(false);
        }
        else
        {
            SetEnableToggle(false);
            SetDownloadButton(true);
        }
    }


    #endregion

    #region Data Operations
    public void UpdateAddonSaveData()
    {
        // If an addon was toggled we send the signal
        enabled = enabledToggle.isOn;
        addon.enabled = enabled;
        addonsConfiguration.UpdateAddonPersistance(addon, addonId);
    }

    // Downloads addon files from github
    public void GetAddonData()
    {
        StartCoroutine(GetAddonCoroutine());
    }

    #endregion

    #region Github API methods
    private IEnumerator GetAddonSizeCoroutine()
    {
        if (isApiOperationRunning)
        {
            yield break;
        }
        isApiOperationRunning = true;

        string url = $"https://api.github.com/repos/" + ExperienceController.instance.experiences[addonsConfiguration.id].githubOwner +
                                          "/" + ExperienceController.instance.experiences[addonsConfiguration.id].githubRepo +
                                          "/releases/latest";

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
                // Only the addon file
                int assetIndex = -1;
                for (int i = 0; i < releaseInfo.assets.Count; i++)
                {
                    if (StringUtils.GithubAssetToNormal(releaseInfo.assets[i].name) == addonName)
                    {
                        assetIndex = i;
                    }
                }
                sizeText.text = SizeFormatter.FormatSize(releaseInfo.assets[assetIndex].size);
            }
        }
        else
        {
            ErrorController.instance.ShowError(LocalizationController.instance.FetchString("baseStrings", "addons_selector_info_error") + www.error, 5);
        }

        isApiOperationRunning = false;
    }

    public IEnumerator GetAddonCoroutine()
    {
        if (isApiOperationRunning)
        {
            yield break;
        }
        isApiOperationRunning = true;
        downloadButton.interactable = false;

        // Get list of files inside addon folder of addon type
        string url = $"https://api.github.com/repos/" + ExperienceController.instance.experiences[addonsConfiguration.id].githubOwner +
                                                  "/" + ExperienceController.instance.experiences[addonsConfiguration.id].githubRepo +
                                                  "/releases/latest";
        string saveUrl = addonsPath + addonType;

        UnityWebRequest www = UnityWebRequest.Get(url);
        www.SetRequestHeader("User-Agent", "InTeractiOn Launcher");
        www.SetRequestHeader("Accept", "application/vnd.github.v3+json");

        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            string responseJson = www.downloadHandler.text;
            ReleaseInfo releaseInfo = JsonUtility.FromJson<ReleaseInfo>(responseJson);
            // Parse the JSON response to get the asset's download URL
            // Only the addon file
            int assetIndex = -1;
            for (int i = 0; i < releaseInfo.assets.Count; i++)
            {
                if (StringUtils.GithubAssetToNormal(releaseInfo.assets[i].name) == addonName)
                {
                    assetIndex = i;
                }
            }
            string downloadUrl = releaseInfo.assets[assetIndex].url;
            string downloadTitle = addonName;

            // If there is at least one of the addon.addonFileNames inside the Addons/Addontype/ folder, delete it and download the complete package
            // Get list of downloaded files
            List<string> downloadedFiles = GetAddonDownloadedFiles();
            // Delete them
            foreach(string downloadedfile in downloadedFiles)
            {
                string filePath = saveUrl + downloadedfile;
                File.Delete(filePath);
            }

            yield return StartCoroutine(DownloadAddonFile(downloadUrl, downloadTitle, saveUrl));

        }
        else
        {
            ErrorController.instance.ShowError(LocalizationController.instance.FetchString("baseStrings", "addons_selector_download_error") + www.error, 5);
        }
        downloadButton.interactable = true;
        SetDownloadedStatus();

        isApiOperationRunning = false;
    }

    private IEnumerator DownloadAddonFile(string url, string fileName, string path)
    {
        string downloadPath = path + "/" + fileName + ".zip";

        // Delete zip if it already exists
        if (File.Exists(downloadPath))
        {
            File.Delete(downloadPath);
        }

        UnityWebRequest www = UnityWebRequest.Get(url);
        www.SetRequestHeader("User-Agent", "InTeractiOn Launcher");
        www.SetRequestHeader("Accept", "application/octet-stream");

        DownloadHandlerFile downloadHandler = new DownloadHandlerFile(downloadPath);
        www.downloadHandler = downloadHandler;

        www.SendWebRequest();
        //Track progress download
        float lastProgress = 0f;
        progressBar.fileCounter.gameObject.SetActive(true);
        progressBar.SetStatus(LocalizationController.instance.FetchString("baseStrings", "addons_selector_downloading"));
        while (!www.isDone)
        {
            float currentProgress = www.downloadProgress;
            // Only change last progress if there is progress
            if (currentProgress != lastProgress)
            {
                progressBar.UpdateFill(currentProgress);
                lastProgress = currentProgress;
            }
            yield return null;
        }

        if (www.result == UnityWebRequest.Result.Success)
        {
            // Unzip the file
            yield return StartCoroutine(UnzipInstall(downloadPath, path));
            // Delete the zip
            File.Delete(downloadPath);
        }
        else
        {
            ErrorController.instance.ShowError(LocalizationController.instance.FetchString("baseStrings", "addons_selector_download_error") + www.error, 5);
        }
    }

    private IEnumerator UnzipInstall(string zipPath, string unzipPath)
    {
        if (!Path.GetExtension(zipPath).Equals(".zip"))
        {
            ErrorController.instance.ShowError(LocalizationController.instance.FetchString("baseStrings", "addons_selector_install_error"), 5);
            yield break;
        }

        // Unzip
        yield return StartCoroutine(UnzipCoroutine(zipPath, unzipPath));
        progressBar.ClearStatus();
    }

    public IEnumerator UnzipCoroutine(string zipFilePath, string extractionPath)
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
                        progressBar.SetStatus(LocalizationController.instance.FetchString("baseStrings", "addons_selector_extracting"));
                        // Only change last progress if there is progress
                        if (progress != lastProgress)
                        {
                            progressBar.UpdateFill(progress);
                            lastProgress = progress;
                        }
                    }

                    yield return null; // Yield once per file
                }
            }
        }
    }

    #endregion

    #region Component setup

    // Changes status component in version info
    private void SetDownloadButton(bool activate)
    {
        if (downloadButton == null)
        {
            return;
        }

        downloadObject.gameObject.SetActive(activate);

    }
    private void SetEnableToggle(bool activate)
    {
        if (enabledToggle == null)
        {
            return;
        }

        toggleObject.gameObject.SetActive(activate);
        enabledToggle.isOn = enabled;
    }

    #endregion

}
