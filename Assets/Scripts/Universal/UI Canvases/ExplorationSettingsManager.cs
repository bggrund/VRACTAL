using UnityEngine;
using UnityEngine.UI;

public class ExplorationSettingsManager : BaseUI<ExplorationSettingsManager>
{
    #region UI Elements

    [SerializeField] private Toggle invertDepthToggle;
    [SerializeField] private Slider depthSlider;
    [SerializeField] private Slider inclinationSlider;
    [SerializeField] private Slider flySpeedSlider;
    [SerializeField] private Slider depthAnimationSpeedSlider;
    [SerializeField] private Button btnToggleExploration;

    #endregion

    #region Public Instance Members

    #endregion

    #region Private Instance Members

    #endregion

    #region Public Static Members

    public static bool invertDepth;
    public static float depth;
    public static float inclination;
    public static float flySpeed;
    public static float depthAnimationSpeed;

    #endregion
    
    #region Private Static Members

    private static float maxDepth;
    private static float linearlyIncrementedDepth;

    #endregion

    #region Public Setters

    private static bool settingInvertDepth = false;
    public static void SetInvertDepth(bool value, ExplorationSettingsManager instanceToIgnore = null)
    {
        if (settingInvertDepth)
        {
            return;
        }

        settingInvertDepth = true;

        invertDepth = value;

        foreach (ExplorationSettingsManager instance in instances)
        {
            if (instance == instanceToIgnore)
            {
                continue;
            }

            instance.invertDepthToggle.isOn = value;
        }

        settingInvertDepth = false;

        MeshManager.Instance.UpdateInvertDepth();
    }

    private static bool settingDepth = false;
    public static void SetDepth(float value, ExplorationSettingsManager instanceToIgnore = null)
    {
        if (settingDepth)
        {
            return;
        }

        settingDepth = true;

        depth = value;

        foreach (ExplorationSettingsManager instance in instances)
        {
            if (instance == instanceToIgnore)
            {
                continue;
            }

            instance.depthSlider.value = value;
        }

        settingDepth = false;

        MeshManager.Instance.UpdateDepth();
    }

    private static bool settingInclination = false;
    public static void SetInclination(float value, ExplorationSettingsManager instanceToIgnore = null)
    {
        if (settingInclination)
        {
            return;
        }

        settingInclination = true;

        inclination = value;

        foreach (ExplorationSettingsManager instance in instances)
        {
            if (instance == instanceToIgnore)
            {
                continue;
            }

            instance.inclinationSlider.value = value;
        }

        settingInclination = false;

        MeshManager.Instance.UpdateInclination();
    }

    private static bool settingFlySpeed = false;
    public static void SetFlySpeed(float value, ExplorationSettingsManager instanceToIgnore = null)
    {
        if (settingFlySpeed)
        {
            return;
        }

        settingFlySpeed = true;

        flySpeed = value;

        foreach (ExplorationSettingsManager instance in instances)
        {
            if (instance == instanceToIgnore)
            {
                continue;
            }

            instance.flySpeedSlider.value = value;
        }

        settingFlySpeed = false;
    }

    private static bool settingDepthAnimationSpeed = false;
    public static void SetDepthAnimationSpeed(float value, ExplorationSettingsManager instanceToIgnore = null)
    {
        if (settingDepthAnimationSpeed)
        {
            return;
        }

        settingDepthAnimationSpeed = true;

        depthAnimationSpeed = value;

        foreach (ExplorationSettingsManager instance in instances)
        {
            if (instance == instanceToIgnore)
            {
                continue;
            }

            instance.depthAnimationSpeedSlider.value = value;
        }

        settingDepthAnimationSpeed = false;
    }

    #endregion

    #region Initialization + Updates

    protected override void InitializeStatics()
    {
        invertDepth = false;
        depth = 0.25f;
        inclination = 0.25f;
        flySpeed = 0.01f;
        depthAnimationSpeed = 0f;

        maxDepth = 0.5f;
        linearlyIncrementedDepth = 0;
    }

    protected override void InitializeInstance()
    {
        invertDepthToggle.isOn = invertDepth;

        depthSlider.minValue = 0;
        depthSlider.maxValue = 0.5f;
        depthSlider.value = depth;

        inclinationSlider.minValue = 0;
        inclinationSlider.maxValue = 0.5f;
        inclinationSlider.value = inclination;

        flySpeedSlider.minValue = 0.0005f;
        flySpeedSlider.maxValue = 0.02f;
        flySpeedSlider.value = flySpeed;

        depthAnimationSpeedSlider.minValue = 0;
        depthAnimationSpeedSlider.maxValue = 0.1f;
        depthAnimationSpeedSlider.value = depthAnimationSpeed;
    }

    protected override void AddEventListeners()
    {
        invertDepthToggle.onValueChanged.AddListener((v) => SetInvertDepth(v, this));
        depthSlider.onValueChanged.AddListener((v) => SetDepth(v, this));
        inclinationSlider.onValueChanged.AddListener((v) => SetInclination(v, this));
        flySpeedSlider.onValueChanged.AddListener((v) => SetFlySpeed(v, this));
        depthAnimationSpeedSlider.onValueChanged.AddListener((v) => SetDepthAnimationSpeed(v, this));

        btnToggleExploration.onClick.AddListener(GameController.Instance.ToggleExploration);
    }

    public static void StaticUpdate()
    {
        if (depthAnimationSpeed > 0)
        {
            linearlyIncrementedDepth += depthAnimationSpeed * Time.deltaTime * 90;
            SetDepth(maxDepth * (Mathf.Sin(linearlyIncrementedDepth) + 1) / 2);
        }
    }

    #endregion

    #region Methods and Event Handlers

    public static void UpdateExplorationToggleButtonText()
    {
        string text = Globals.explorationMode ? "Return to Workshop" : "Explore!";
        foreach (ExplorationSettingsManager instance in instances)
        {
            instance.btnToggleExploration.GetComponentInChildren<Text>().text = text;
        }
    }

    #endregion
}
