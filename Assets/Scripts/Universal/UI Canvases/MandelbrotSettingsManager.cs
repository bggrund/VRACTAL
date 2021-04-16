using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum ResolutionType { Low, Med, High, Infinite };

public class MandelbrotSettingsManager : BaseUI<MandelbrotSettingsManager>
{
    #region UI Elements

    [SerializeField] private InputField maxIterationsInput;
    [SerializeField] private Toggle low, med, high, infinite;
    [SerializeField] private InputField centerA, centerB;
    [SerializeField] private InputField scaleInput;
    [SerializeField] private Slider zoomSpeedSlider;
    [SerializeField] private Button btnIncrementMaxResolution;
    [SerializeField] private Button btnDecrementMaxResolution;
    [SerializeField] private Button btnIncrementScale;
    [SerializeField] private Button btnDecrementScale;

    #endregion

    #region Public Instance Members

    #endregion

    #region Private Instance Members

    #endregion

    #region Public Static Members

    public static ResolutionType resolution;
    public static int maxIterations;
    public static Vector2d centerPoint;
    public static double scale;
    public static float zoomSpeed;

    public static double minSinglePrecisionScale;

    public static bool doublePrecisionMode { get { return scale < minSinglePrecisionScale; } }

    #endregion

    #region Private Static Members

    private static int maxIterationsIncrement;

    #endregion

    #region Public Setters

    private static bool settingResolution = false;
    public static void SetResolution(ResolutionType value, MandelbrotSettingsManager instanceToIgnore = null)
    {
        if (settingResolution)
        {
            return;
        }

        settingResolution = true;

        resolution = value;

        foreach (MandelbrotSettingsManager instance in instances)
        {
            if (instance == instanceToIgnore)
            {
                continue;
            }

            switch (value)
            {
                case ResolutionType.Low: instance.low.isOn = true; break;
                case ResolutionType.Med: instance.med.isOn = true; break;
                case ResolutionType.High: instance.high.isOn = true; break;
                case ResolutionType.Infinite: instance.infinite.isOn = true; break;
            }
        }

        settingResolution = false;

        MeshManager.Instance.UpdateResolution();
    }

    private static bool settingMaxIterations = false;
    public static void SetMaxIterations(int value, MandelbrotSettingsManager instanceToIgnore = null)
    {
        if (settingMaxIterations)
        {
            return;
        }

        settingMaxIterations = true;

        maxIterations = value;

        foreach (MandelbrotSettingsManager instance in instances)
        {
            if (instance == instanceToIgnore)
            {
                continue;
            }

            instance.maxIterationsInput.text = value.ToString();
        }

        settingMaxIterations = false;

        MeshManager.Instance.UpdateMaxIter();
    }

    private static bool settingCenterPoint = false;
    public static void SetCenterPoint(Vector2d point, MandelbrotSettingsManager instanceToIgnore = null)
    {
        if (settingCenterPoint)
        {
            return;
        }

        settingCenterPoint = true;
        
        centerPoint = point;

        foreach (MandelbrotSettingsManager instance in instances)
        {
            if (instance == instanceToIgnore)
            {
                continue;
            }

            instance.centerA.text = point.x.ToString();
            instance.centerB.text = point.y.ToString();
        }

        settingCenterPoint = false;

        MeshManager.Instance.UpdateTranslation();
    }

    private static bool settingScale = false;
    public static void SetScale(double value, MandelbrotSettingsManager instanceToIgnore = null)
    {
        if (settingScale)
        {
            return;
        }

        settingScale = true;

        scale = value;

        foreach (MandelbrotSettingsManager instance in instances)
        {
            if (instance == instanceToIgnore)
            {
                continue;
            }

            instance.scaleInput.text = value.ToString();
        }

        settingScale = false;

        MeshManager.Instance.UpdateScale();
    }

    private static bool settingZoomSpeed = false;
    public static void SetZoomSpeed(float value, MandelbrotSettingsManager instanceToIgnore = null)
    {
        if (settingZoomSpeed)
        {
            return;
        }

        settingZoomSpeed = true;

        zoomSpeed = ConvertZoomSpeed_UIToValue(value);

        foreach (MandelbrotSettingsManager instance in instances)
        {
            if (instance == instanceToIgnore)
            {
                continue;
            }

            instance.zoomSpeedSlider.value = value;
        }

        settingZoomSpeed = false;
    }

    public static float ConvertZoomSpeed_UIToValue(float value)
    {
        return Globals.vrMode ? (value - 1) / 4 + 1 : value;
    }

    public static void DecrementMaxIterations()
    {
        SetMaxIterations(maxIterations - maxIterationsIncrement);
    }
    public static void IncrementMaxIterations()
    {
        SetMaxIterations(maxIterations + maxIterationsIncrement);
    }
    public static void DecrementScale()
    {
        SetScale(scale / zoomSpeed);
    }
    public static void IncrementScale()
    {
        SetScale(scale * zoomSpeed);
    }

    #endregion

    #region Initialization + Updates

    protected override void InitializeStatics()
    {
        resolution = Globals.HighQuality ? ResolutionType.Med : ResolutionType.Low;
        maxIterations = 750;
        centerPoint = new Vector2d(-0.75, 0);
        scale = 1.25;
        zoomSpeed = 1.102f;

        minSinglePrecisionScale = 0.0001;

        maxIterationsIncrement = 250;
    }

    protected override void InitializeInstance()
    {
        switch (resolution)
        {
            case ResolutionType.Low: low.isOn = true; break;
            case ResolutionType.Med: med.isOn = true; break;
            case ResolutionType.High: high.isOn = true; break;
            case ResolutionType.Infinite: infinite.isOn = true; break;
        }

        maxIterationsInput.text = maxIterations.ToString();
        centerA.text = centerPoint.x.ToString();
        centerB.text = centerPoint.y.ToString();
        scaleInput.text = scale.ToString();

        zoomSpeedSlider.minValue = 1;
        zoomSpeedSlider.maxValue = 1.3f;
        zoomSpeedSlider.value = zoomSpeed;
    }

    protected override void AddEventListeners()
    {
        low.onValueChanged.AddListener((v) => { if(v) SetResolution(ResolutionType.Low, this); });
        med.onValueChanged.AddListener((v) => { if (v) SetResolution(ResolutionType.Med, this); });
        high.onValueChanged.AddListener((v) => { if (v) SetResolution(ResolutionType.High, this); });
        infinite.onValueChanged.AddListener((v) => { if (v) SetResolution(ResolutionType.Infinite, this); });
        maxIterationsInput.onValueChanged.AddListener((v) => SetMaxIterations(int.Parse(v), this));
        centerA.onValueChanged.AddListener((v) => SetCenterPoint(new Vector2d(double.Parse(centerA.text), double.Parse(centerB.text)), this));
        centerB.onValueChanged.AddListener((v) => SetCenterPoint(new Vector2d(double.Parse(centerA.text), double.Parse(centerB.text)), this));
        scaleInput.onValueChanged.AddListener((v) => SetScale(double.Parse(v), this));
        zoomSpeedSlider.onValueChanged.AddListener((v) => SetZoomSpeed(v, this));

        btnIncrementMaxResolution.onClick.AddListener(IncrementMaxIterations);
        btnDecrementMaxResolution.onClick.AddListener(DecrementMaxIterations);
        btnIncrementScale.onClick.AddListener(IncrementScale);
        btnDecrementScale.onClick.AddListener(DecrementScale);

        // Pointer click event
        EventTrigger.Entry pointerClick = new EventTrigger.Entry();
        pointerClick.eventID = EventTriggerType.PointerClick;
        pointerClick.callback.AddListener(PanelClick);

        EventTrigger eventTrigger = gameObject.AddComponent<EventTrigger>();
        eventTrigger.triggers.Add(pointerClick);
    }

    #endregion

    #region Methods and Event Handlers

    public void PanelClick(BaseEventData e)
    {
        FractalSettingsManager.SwitchToMandle();
    }

    #endregion
}
