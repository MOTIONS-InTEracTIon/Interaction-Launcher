using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DentedPixel;
using Unity.VisualScripting;
using UnityEngine.UI;
using TMPro;
using System;

public class ProgressBar : MonoBehaviour
{
    // Components
    public Image progress;
    public TextMeshProUGUI fileCounter;

    public void UpdateFill(float value) 
    { 
        progress.fillAmount = value;
    }

    public void ActivateFileCounter(int numberOfFiles)
    {
        if (!fileCounter.gameObject.activeInHierarchy)
        {
            fileCounter.gameObject.SetActive(true);
        }
        fileCounter.text = "0/" + numberOfFiles.ToString();
    }

    public void AddFileToCounter()
    {
        string[] files = fileCounter.text.Split(new string[] { "/" }, StringSplitOptions.None);
        int newValue = int.Parse(files[0]) + 1;
        string newFiles = newValue.ToString() + "/" + files[1];
    }

    public void SetStatus(string status)
    {
        fileCounter.text = status;
    }

    public void ClearStatus()
    {
        fileCounter.text = "";
    }
}
