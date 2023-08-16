using System.Collections;
using UnityEngine;

public class MainManager : MonoBehaviour
{
    // Component
    [SerializeField] public FadeUI fadeScreen;

    // Controllers
    [SerializeField] public LocalizationController localizationController;
    [SerializeField] public ErrorController errorController;
    [SerializeField] public ExperienceController experienceController;
    [SerializeField] public MediaController mediaController;
    [SerializeField] public InputPersistanceController inputPersistanceController;

    public static MainManager instance;
    private void Start()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        // Write data
        VersionInfo.WriteVersionToFile();

        // Initialize controllers
        errorController.Initialize();
        mediaController.Initialize();
        localizationController.Initialize();
        experienceController.Initialize();
        inputPersistanceController.Initialize();
        
    }

    #region Data Operations

    public void CloseAplication()
    {
        StartCoroutine(CloseApplicationCoroutine());
    }
    public IEnumerator CloseApplicationCoroutine()
    {
        yield return StartCoroutine(fadeScreen.FadeIn());
        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #endif
                Application.Quit();
    }


    #endregion

}
