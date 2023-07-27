using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class CarouselScrollview : MonoBehaviour
{
    // Prefabs
    [SerializeField] GameObject selectorPrefab;

    // Components
    [SerializeField] ScrollRect scrollView;
    [SerializeField] GameObject scrollViewContent;
    [SerializeField] GameObject selectorContent;

    // Data
    public List<GameObject> scrollViewElements;

    #region Data Operations
    public void PlayVideo()
    {
        VideoPlayer[] players = scrollViewContent.transform.GetComponentsInChildren<VideoPlayer>();
        foreach (VideoPlayer player in players)
        {
            player.Play();
        }
    }
    public List<GameObject> GetData()
    {
        return scrollViewElements;
    }

    public void AddElement(GameObject element)
    {
        // Add element to data
        scrollViewElements.Add(element);
        // Add element to carousel
        element.transform.SetParent(scrollViewContent.transform);
        element.transform.localScale = Vector3.one;
        // Create a knob to control it
        Button knob = Instantiate(selectorPrefab, selectorContent.transform).GetComponent<Button>();
        knob.onClick.AddListener(delegate { scrollViewContent.GetComponent<swipe>().WhichBtnClicked(knob); });
    }

    public void AddElement(List<GameObject> elements)
    {
        foreach (GameObject element in elements)
        {
            AddElement(element);
        }
        PlayVideo();
    }

    public void Clear()
    {
        // Clear knobs
        foreach (Transform child in selectorContent.transform)
        {
            Destroy(child.gameObject);
        }
        // Clear elements
        foreach (Transform child in scrollViewContent.transform)
        {
            Destroy(child.gameObject);
        }
        // Clear data
        scrollViewElements = new List<GameObject>();
    }



    #endregion


}
