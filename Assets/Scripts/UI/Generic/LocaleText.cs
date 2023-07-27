using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LocaleText : LocaleComponent
{
    // Components
    [SerializeField] public TextMeshProUGUI textBox;

    // Setting
    public string groupKey;
    public string stringKey;


    public string GetData()
    {
        return textBox.text;
    }
    public void SetText(string text)
    {
        textBox.text = text;
    }

    public override void UpdateText()
    {
        textBox.text = LocalizationController.instance.FetchString(groupKey,stringKey);
    }
}
