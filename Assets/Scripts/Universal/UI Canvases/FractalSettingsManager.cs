using UnityEngine;
using UnityEngine.UI;

public enum GenerationType { Mandelbrot, Julia };

public class FractalSettingsManager : BaseUI<FractalSettingsManager>
{
    #region UI Elements

    [SerializeField] private Transform mandelHeader, juliaHeader;
    [SerializeField] private Button genTypeSwitcherButton;

    #endregion

    #region Public Instance Members

    #endregion

    #region Private Instance Members

    #endregion

    #region Public Static Members

    public static GenerationType genType;

    #endregion

    #region Private Static Members

    private static Color juliaNormal, juliaHighlighted, juliaPressed, mandelNormal, mandelHighlighted, mandelPressed, disabledColor;

    #endregion

    #region Public Setters

    #endregion

    #region Initialization + Updates

    protected override void InitializeStatics()
    {
        genType = GenerationType.Mandelbrot;

        ColorUtility.TryParseHtmlString("#3FA568B4", out juliaNormal);
        ColorUtility.TryParseHtmlString("#3FA568B4", out juliaHighlighted);
        ColorUtility.TryParseHtmlString("#26905196", out juliaPressed);
        ColorUtility.TryParseHtmlString("#40849A80", out mandelNormal);
        ColorUtility.TryParseHtmlString("#40849A80", out mandelHighlighted);
        ColorUtility.TryParseHtmlString("#21708980", out mandelPressed);
        ColorUtility.TryParseHtmlString("#C8C8C880", out disabledColor);        
    }

    protected override void InitializeInstance()
    {
        genTypeSwitcherButton.colors = GetJuliaColorBlock();
        genTypeSwitcherButton.transform.SetParent(juliaHeader, false);
    }

    protected override void AddEventListeners()
    {
        genTypeSwitcherButton.onClick.AddListener(ToggleGenType);
    }

    #endregion

    #region Methods and Event Handlers

    public void ToggleGenType()
    {
        if (genType == GenerationType.Mandelbrot)
        {
            SwitchToJulia();
        }
        else
        {
            SwitchToMandle();
        }
    }
    public static void SwitchToMandle()
    {
        genType = GenerationType.Mandelbrot;

        SetGenSwitcherButtonToJulia();

        MeshManager.Instance.UpdateMandel();
    }
    public static void SwitchToJulia()
    {
        genType = GenerationType.Julia;

        SetGenSwitcherButtonToMandelbrot();

        MeshManager.Instance.UpdateMandel();
    }

    private static void SetGenSwitcherButtonToJulia()
    {
        ColorBlock cb = GetJuliaColorBlock();

        foreach (FractalSettingsManager instance in instances)
        {
            instance.genTypeSwitcherButton.colors = cb;
            instance.genTypeSwitcherButton.transform.SetParent(instance.juliaHeader, false);
        }
    }

    private static void SetGenSwitcherButtonToMandelbrot()
    {
        ColorBlock cb = GetMandelbrotColorBlock();

        foreach (FractalSettingsManager instance in instances)
        {
            instance.genTypeSwitcherButton.colors = cb;
            instance.genTypeSwitcherButton.transform.SetParent(instance.mandelHeader, false);
        }
    }

    private static ColorBlock GetMandelbrotColorBlock()
    {
        return new ColorBlock
        {
            normalColor = mandelNormal,
            highlightedColor = mandelHighlighted,
            pressedColor = mandelPressed,
            disabledColor = disabledColor,

            colorMultiplier = 1,
            fadeDuration = 0.1f
        };
    }

    private static ColorBlock GetJuliaColorBlock()
    {
        return new ColorBlock
        {
            normalColor = juliaNormal,
            highlightedColor = juliaHighlighted,
            pressedColor = juliaPressed,
            disabledColor = disabledColor,

            colorMultiplier = 1,
            fadeDuration = 0.1f
        };
    }

    #endregion
}
