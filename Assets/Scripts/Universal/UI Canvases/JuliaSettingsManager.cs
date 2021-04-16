using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class JuliaSettingsManager : BaseUI<JuliaSettingsManager>
{
    #region UI Elements

    [SerializeField] private InputField centerA, centerB;
    [SerializeField] private InputField radiusInput;
    [SerializeField] private Slider angleSlider;
    [SerializeField] private Slider animationSpeedSlider;
    [SerializeField] private Button btnSlectJuliaRotation;
    [SerializeField] private Button btnIncrementRadius;
    [SerializeField] private Button btnDecrementRadius;

    public Text selectJuliaRotationButtonText;

    #endregion

    #region Public Instance Members

    #endregion

    #region Private Instance Members

    #endregion

    #region Public Static Members

    public static Vector2d centerPoint;
    public static double radius;
    public static float angle;
    public static float angleAnimationSpeed;

    #endregion

    #region Private Static Members

    private static double radiusIncrementFactor;

    private static float minAngleSliderVal;
    private static float maxAngleSliderVal;

    #endregion

    #region Public Setters

    private static bool settingCenterPoint = false;
    public static void SetCenterPoint(Vector2d point, JuliaSettingsManager instanceToIgnore = null)
    {
        if (settingCenterPoint)
        {
            return;
        }

        settingCenterPoint = true;

        centerPoint = point;

        foreach (JuliaSettingsManager instance in instances)
        {
            if (instance == instanceToIgnore)
            {
                continue;
            }

            instance.centerA.text = point.x.ToString();
            instance.centerB.text = point.y.ToString();
        }

        settingCenterPoint = false;

        MeshManager.Instance.UpdateKVal();
    }

    private static bool settingRadius = false;
    public static void SetRadius(double value, JuliaSettingsManager instanceToIgnore = null)
    {
        if (settingRadius)
        {
            return;
        }

        settingRadius = true;

        radius = value;

        foreach (JuliaSettingsManager instance in instances)
        {
            if (instance == instanceToIgnore)
            {
                continue;
            }

            instance.radiusInput.text = value.ToString();
        }

        settingRadius = false;

        MeshManager.Instance.UpdateKVal();
    }

    private static bool settingAngle = false;
    public static void SetAngle(float value, JuliaSettingsManager instanceToIgnore = null)
    {
        if (settingAngle)
        {
            return;
        }

        settingAngle = true;

        angle = value;

        foreach (JuliaSettingsManager instance in instances)
        {
            if (instance == instanceToIgnore)
            {
                continue;
            }

            instance.angleSlider.value = value;
        }

        settingAngle = false;

        MeshManager.Instance.UpdateKVal();
    }

    private static bool settingAnimationSpeed = false;
    public static void SetAngleAnimationSpeed(float value, JuliaSettingsManager instanceToIgnore = null)
    {
        if (settingAnimationSpeed)
        {
            return;
        }

        settingAnimationSpeed = true;

        angleAnimationSpeed = value;

        foreach (JuliaSettingsManager instance in instances)
        {
            if (instance == instanceToIgnore)
            {
                continue;
            }

            instance.animationSpeedSlider.value = value;
        }

        settingAnimationSpeed = false;
    }

    public void DecrementRadius()
    {
        SetRadius(radius / radiusIncrementFactor);
    }
    public void IncrementRadius()
    {
        SetRadius(radius * radiusIncrementFactor);
    }

    public static void SetJuliaRotationButtonText(string text)
    {
        foreach(JuliaSettingsManager instance in instances)
        {
            instance.selectJuliaRotationButtonText.text = text;
        }
    }

    #endregion

    #region Initialization + Updates

    protected override void InitializeStatics()
    {
        centerPoint = new Vector2d(-1.0, 0);
        radius = 0.2505;
        angle = 0;
        angleAnimationSpeed = 0;

        radiusIncrementFactor = 1.01;

        minAngleSliderVal = 0;
        maxAngleSliderVal = Mathf.PI * 2;
}

    protected override void InitializeInstance()
    {
        centerA.text = "-1.0";
        centerB.text = "0";
        radiusInput.text = "0.2505";

        angleSlider.minValue = minAngleSliderVal;
        angleSlider.maxValue = maxAngleSliderVal;
        angleSlider.value = 0;

        animationSpeedSlider.minValue = 0;
        animationSpeedSlider.maxValue = 0.0075f;
        animationSpeedSlider.value = 0;
    }

    protected override void AddEventListeners()
    {
        centerA.onValueChanged.AddListener((v) => SetCenterPoint(new Vector2d(double.Parse(centerA.text), double.Parse(centerB.text)), this));
        centerB.onValueChanged.AddListener((v) => SetCenterPoint(new Vector2d(double.Parse(centerA.text), double.Parse(centerB.text)), this));
        radiusInput.onValueChanged.AddListener((v) => SetRadius(double.Parse(v), this));
        angleSlider.onValueChanged.AddListener((v) => SetAngle(v, this));
        animationSpeedSlider.onValueChanged.AddListener((v) => SetAngleAnimationSpeed(v, this));

        btnIncrementRadius.onClick.AddListener(IncrementRadius);
        btnDecrementRadius.onClick.AddListener(DecrementRadius);

        btnSlectJuliaRotation.onClick.AddListener(StartJuliaSelection);

        // Pointer click event
        EventTrigger.Entry pointerClick = new EventTrigger.Entry();
        pointerClick.eventID = EventTriggerType.PointerClick;
        pointerClick.callback.AddListener(PanelClick);

        EventTrigger eventTrigger = gameObject.AddComponent<EventTrigger>();
        eventTrigger.triggers.Add(pointerClick);
    }

    public static void StaticUpdate()
    {
        if (angleAnimationSpeed > 0f)
        {
            float newAngle = angle + angleAnimationSpeed * Time.deltaTime * 90;

            if (newAngle > maxAngleSliderVal)
            {
                SetAngle(minAngleSliderVal);
            }
            else
            {
                SetAngle(newAngle);
            }
        }
    }

    #endregion

    #region Methods and Event Handlers

    public void PanelClick(BaseEventData e)
    {
        FractalSettingsManager.SwitchToJulia();
    }

    private void StartJuliaSelection()
    {
        PanZoomManager.Instance.StartJuliaSelection();
    }

    #endregion
}
