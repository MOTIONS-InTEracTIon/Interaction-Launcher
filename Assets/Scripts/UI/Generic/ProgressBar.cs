using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DentedPixel;
using Unity.VisualScripting;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour
{
    // Components
    public Image progress;

    public void UpdateFill(float value) 
    { 
        progress.fillAmount = value;
    }
}
