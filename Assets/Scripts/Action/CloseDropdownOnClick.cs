using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class CloseDropdownOnClick : MonoBehaviour, IPointerClickHandler
{
    public TMP_Dropdown dropdown; // Reference to the TMP_Dropdown component


    public void OnPointerClick(PointerEventData eventData)
    {
        if (dropdown && dropdown.IsExpanded)
        {
            // Close the dropdown
            dropdown.Hide();

            // Hide the invisible image
            gameObject.SetActive(false);
        }
    }

}