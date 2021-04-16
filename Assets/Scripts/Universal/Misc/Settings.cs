using UnityEngine;

public class Settings
{
    // General Fractal settings
    public GenerationType genType;

    // Mandelbrot settings
    public ResolutionType resolution;
    public int maxIterations;
    public Vector2d mandelbrotCenterPoint;
    public double scale;

    // Julia settings
    public Vector2d juliaCenterPoint;
    public double juliaRadius;
    public float juliaAngle;
    public float juliaAngleAnimationSpeed;

    // Colorspace settings
    public bool wrapColors;
    public float colorDensity;
    public float colorShift;
    public float colorAnimationSpeed;
    public Color[] selectedColorspace;

    // Exploration settings
    public bool invertDepth;
    public float depth;
    public float inclination;
    public float depthAnimationSpeed;

    public Settings()
    {
        // Mandelbrot settings
        genType = FractalSettingsManager.genType;
        resolution = MandelbrotSettingsManager.resolution;
        maxIterations = MandelbrotSettingsManager.maxIterations;
        mandelbrotCenterPoint = MandelbrotSettingsManager.centerPoint;
        scale = MandelbrotSettingsManager.scale;

        // Julia settings
        juliaCenterPoint = JuliaSettingsManager.centerPoint;
        juliaRadius = JuliaSettingsManager.radius;
        juliaAngle = JuliaSettingsManager.angle;
        juliaAngleAnimationSpeed = JuliaSettingsManager.angleAnimationSpeed;

        // Colorspace settings
        wrapColors = ColorspaceSelectorManager.wrapColors;
        colorDensity = ColorspaceSelectorManager.colorDensity;
        colorShift = ColorspaceSelectorManager.colorShift;
        colorAnimationSpeed = ColorspaceSelectorManager.colorAnimationSpeed;
        selectedColorspace = ColorspaceSelectorManager.selectedColorspace;

        // Exploration settings
        invertDepth = ExplorationSettingsManager.invertDepth;
        depth = ExplorationSettingsManager.depth;
        inclination = ExplorationSettingsManager.inclination;
        depthAnimationSpeed = ExplorationSettingsManager.depthAnimationSpeed;
    }    

    public void SetSettings()
    {
        if (genType == GenerationType.Julia)
        {
            FractalSettingsManager.SwitchToJulia();
        }
        else
        {
            FractalSettingsManager.SwitchToMandle();
        }
        MandelbrotSettingsManager.SetResolution(resolution);
        MandelbrotSettingsManager.SetMaxIterations(maxIterations);
        MandelbrotSettingsManager.SetCenterPoint(mandelbrotCenterPoint);
        MandelbrotSettingsManager.SetScale(scale);

        JuliaSettingsManager.SetCenterPoint(juliaCenterPoint);
        JuliaSettingsManager.SetRadius(juliaRadius);
        JuliaSettingsManager.SetAngle(juliaAngle);
        JuliaSettingsManager.SetAngleAnimationSpeed(juliaAngleAnimationSpeed);

        // Colorspace settings
        ColorspaceSelectorManager.SetWrapColors(wrapColors);
        ColorspaceSelectorManager.SetColorDensity(colorDensity);
        ColorspaceSelectorManager.SetColorShift(colorShift);
        ColorspaceSelectorManager.SetColorAnimationSpeed(colorAnimationSpeed);
        ColorspaceSelectorManager.SetSelectedColorspace(selectedColorspace);

        ExplorationSettingsManager.SetInvertDepth(invertDepth);
        ExplorationSettingsManager.SetDepth(depth);
        ExplorationSettingsManager.SetInclination(inclination);
        ExplorationSettingsManager.SetDepthAnimationSpeed(depthAnimationSpeed);

        MeshManager.Instance.UpdateShader();
    }
}