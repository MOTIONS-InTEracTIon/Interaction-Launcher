using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;
using UnityEngine.UI;

namespace Vortices
{
    public class UIExperience : MonoBehaviour
    {
        // Other references
        [SerializeField] public LocaleText nameText;
        [SerializeField] public Toggle selectToggle;

        // Data variables
        public string experienceName;
        public int experienceId;


        #region Data Operation
        public void Init(int id, string name)
        {
            experienceName = name;
            nameText.textBox.text = name;

            experienceId = id;
            // Subscribe to event with id
            selectToggle.onValueChanged.AddListener(delegate { ExperienceController.instance.ChangeCardToggle(selectToggle); });
        }

        public void SetToggle(bool on)
        {
            selectToggle.isOn = on;
        }

        #endregion
    }

}
