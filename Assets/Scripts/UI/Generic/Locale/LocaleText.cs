using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(EventTrigger))]
public class LocaleText : LocaleComponent, IPointerClickHandler
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
        if(LocalizationController.instance.FetchString(groupKey, stringKey) != "")
        {
            textBox.text = LocalizationController.instance.FetchString(groupKey, stringKey);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        TMP_TextInfo textInfo = textBox.textInfo;
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(textBox, eventData.position, null);

        if (linkIndex != -1)
        {
            TMP_LinkInfo linkInfo = textInfo.linkInfo[linkIndex];
            Application.OpenURL(linkInfo.GetLinkID());
        }
    }
}
