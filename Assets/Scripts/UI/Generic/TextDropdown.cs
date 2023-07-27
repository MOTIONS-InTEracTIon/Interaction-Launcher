using TMPro;
using UnityEngine;

public class TextDropdown : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown dropdown;

    public int GetData()
    {
        return dropdown.value;
    }

}
