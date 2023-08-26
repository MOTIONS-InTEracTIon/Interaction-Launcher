using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.Video;

public class MediaController : MonoBehaviour
{
    // Prefabs
    public GameObject imagePrefab; 
    public GameObject videoPrefab;

    // Data
    public List<GameObject> media;

    public static MediaController instance;

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
    }

    #endregion

    #region Persistence

    public IEnumerator FetchMedia(int experienceId, List<string> imagePaths)
    {
        media = new List<GameObject>();

        // Add them to media if they are the correct extension
        foreach (string file in imagePaths)
        {
            string extension = Path.GetExtension(file).ToLower();

            if (extension == ".png" || extension == ".jpg" || extension == ".jpeg")
            {
                // Load image
                yield return StartCoroutine(LoadImage(file));
            }
            else if (extension == ".mp4" || extension == ".mov")
            {
                // Load video
                media.Add(LoadVideo(file));
            }
        }
    }

    private IEnumerator LoadImage(string imagePath)
    {
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(imagePath);

        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            Texture2D texture = DownloadHandlerTexture.GetContent(www);
            GameObject mediaImage = Instantiate(imagePrefab);
            RawImage rawImage = mediaImage.GetComponentInChildren<RawImage>();
            rawImage.texture = texture;

            ResizeParentRectTransform(rawImage, texture.width, texture.height);

            media.Add(mediaImage);
        }
        else
        {
            ErrorController.instance.ShowError(LocalizationController.instance.FetchString("baseStrings", "media_error") + www.error, 5);
        }
    }

    void ResizeParentRectTransform(RawImage rawImage, int imageWidth, int imageHeight)
    {
        // Get the RectTransform of the parent GameObject containing the RawImage
        RectTransform parentRectTransform = rawImage.transform.parent.GetComponent<RectTransform>();

        // Calculate the aspect ratio of the image
        float aspectRatio = (float)rawImage.texture.width / rawImage.texture.height;

        // Calculate the target size of the RawImage to fit inside the parent while maintaining aspect ratio
        float targetWidth = parentRectTransform.rect.width;
        float targetHeight = parentRectTransform.rect.height;

        // Calculate the target size of the RawImage to fit inside the parent while maintaining aspect ratio
        float widthRatio = targetWidth / rawImage.texture.width;
        float heightRatio = targetHeight / rawImage.texture.height;

        // Use the smaller ratio to maintain the aspect ratio and fit the RawImage inside the parent
        float scaleRatio = Mathf.Min(widthRatio, heightRatio);

        // Set the size of the RawImage RectTransform based on the calculated scale ratio
        rawImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, rawImage.texture.width * scaleRatio);
        rawImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, rawImage.texture.height * scaleRatio);
    }

    private GameObject LoadVideo(string videoPath)
    {
        // Get Prefab and configure
        GameObject mediaVideo = Instantiate(videoPrefab);
        RawImage rawImage = mediaVideo.transform.GetComponentInChildren<RawImage>();
        VideoPlayer videoPlayer = mediaVideo.transform.GetComponentInChildren<VideoPlayer>();
        videoPlayer.playOnAwake = true;
        videoPlayer.url = videoPath;
        videoPlayer.isLooping = true;
        videoPlayer.audioOutputMode = VideoAudioOutputMode.None;
        videoPlayer.Prepare();

        // Register a callback for when the video is prepared
        videoPlayer.prepareCompleted += (vp) =>
        {
            // Set the video texture to the RawImage
            rawImage.texture = videoPlayer.texture;

            // Resize the RawImage to fit inside the parent while maintaining aspect ratio
            ResizeRawImage(videoPlayer, rawImage);
        };

        return mediaVideo;
    }

    void ResizeRawImage(VideoPlayer videoPlayer, RawImage rawImage)
    {
        // Get the RectTransform of the parent GameObject containing the RawImage
        RectTransform parentRectTransform = rawImage.transform.parent.GetComponent<RectTransform>();

        // Get the video's resolution (width and height)
        int videoWidth = (int)videoPlayer.width;
        int videoHeight = (int)videoPlayer.height;

        // Calculate the aspect ratio of the video
        float aspectRatio = (float)videoWidth / videoHeight;

        // Calculate the target size of the RawImage to fit inside the parent while maintaining aspect ratio
        float targetWidth = parentRectTransform.rect.width;
        float targetHeight = parentRectTransform.rect.height;

        // Calculate the target size of the RawImage to fit inside the parent while maintaining aspect ratio
        float widthRatio = targetWidth / videoWidth;
        float heightRatio = targetHeight / videoHeight;

        // Use the smaller ratio to maintain the aspect ratio and fit the RawImage inside the parent
        float scaleRatio = Mathf.Min(widthRatio, heightRatio);

        // Set the size of the RawImage RectTransform based on the calculated scale ratio
        rawImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, videoWidth * scaleRatio);
        rawImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, videoHeight * scaleRatio);
    }


    #endregion
}
