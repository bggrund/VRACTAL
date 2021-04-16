using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Startup : MonoBehaviour {

    [SerializeField] private Toggle highQualityToggle;
    [SerializeField] private Toggle vrModeToggle;

    private void Start()
    {
        Screen.SetResolution(800, 800, false);
    }

    public void StartButton_Click()
    {
        QualitySettings.SetQualityLevel(highQualityToggle.isOn ? 1 : 0);

        SceneManager.LoadSceneAsync(vrModeToggle.isOn ? "VRScene" : "NonVRScene");
        SceneManager.UnloadSceneAsync("StartupScene");
    }
}
