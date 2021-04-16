using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class MeshManager : MonoBehaviour
{
    #region Singleton
    private static MeshManager instance;
    public static MeshManager Instance { get { return instance; } }

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            instance = this;
        }
    }
    #endregion

    [SerializeField] private Shader fixedResShader;
    [SerializeField] private Shader infiniteResShader;
    [SerializeField] private Shader fixedResShader_DP;
    [SerializeField] private Shader infiniteResShader_DP;
    [SerializeField] private int fixedMeshDim;

    private Material mat;
    private Material fixedResMaterial;
    private Material infiniteResMaterial;
    private Material fixedResMaterial_DP;
    private Material infiniteResMaterial_DP;

    private List<List<Mesh>> meshes;

    private Vector3 refPos, refScale;
    [SerializeField] private float meshTransitionThreshold = 0.01f;

    private bool savingPNG = false;


    // Use this for initialization
    void Start()
    {
        fixedResMaterial = new Material(fixedResShader);
        infiniteResMaterial = new Material(infiniteResShader);
        fixedResMaterial_DP = new Material(fixedResShader_DP);
        infiniteResMaterial_DP = new Material(infiniteResShader_DP);

        GenerateMesh();
    }

    /// <summary>
    /// Resets mesh transform to center of its parent and resets its scale smoothly over a short period of time
    /// </summary>
    /// <returns></returns>
    public IEnumerator TransitionToCenter()
    {
        while (true)
        {
            transform.localPosition = Vector3.SmoothDamp(transform.localPosition, Vector3.zero, ref refPos, 0.25f);
            transform.localScale = Vector3.SmoothDamp(transform.localScale, Vector3.one, ref refScale, 0.25f);

            Vector3 localPos = transform.localPosition;
            Vector3 localScale = transform.localScale;

            if (Mathf.Abs(localPos.x) <= meshTransitionThreshold &&
                Mathf.Abs(localPos.y) <= meshTransitionThreshold &&
                Mathf.Abs(localPos.z) <= meshTransitionThreshold &&
                localScale.x < 1 + meshTransitionThreshold &&
                localScale.x > 1 - meshTransitionThreshold &&
                localScale.y < 1 + meshTransitionThreshold &&
                localScale.y > 1 - meshTransitionThreshold &&
                localScale.z < 1 + meshTransitionThreshold &&
                localScale.z > 1 - meshTransitionThreshold)
            {
                break;
            }
            else
            {
                yield return null;
            }
        }
    }

    #region Mesh Generation

    public void GenerateMesh()
    {
        float top = 1f;
        float bottom = -1f;
        float left = -1f;
        float right = 1f;
        float extra = 0.0001f;

        float width = right - left;
        float height = top - bottom;

        int verticesPerMeshSide = 0;

        float incrementX = 2f;
        float incrementY = 2f;

        int meshDim = 1;

        switch (MandelbrotSettingsManager.resolution)
        {
            case ResolutionType.Low:
                verticesPerMeshSide = 100;
                meshDim = fixedMeshDim;
                break;
            case ResolutionType.Med:
                verticesPerMeshSide = 175;
                meshDim = fixedMeshDim;
                break;
            case ResolutionType.High:
                verticesPerMeshSide = 250;
                meshDim = fixedMeshDim;
                break;
            case ResolutionType.Infinite:
                verticesPerMeshSide = 1;
                // Only one mesh for infinite resolution, since only 4 vertices are needed
                meshDim = 1;
                break;
        }

        meshes = new List<List<Mesh>>();
        for (int col = 0; col < meshDim; col++)
        {
            meshes.Add(new List<Mesh>());
            for (int row = 0; row < meshDim; row++)
            {
                meshes[col].Add(new Mesh());
            }
        }

        //set mesh vertices
        List<Vector3> vertices = new List<Vector3>();

        //set increments
        incrementX = width / meshes[0].Count / verticesPerMeshSide;
        incrementY = height / meshes.Count / verticesPerMeshSide;

        for (int col = 0; col < meshes.Count; col++)
        {
            for (int row = 0; row < meshes[0].Count; row++)
            {
                vertices.Clear();
                for (float j = top - (incrementY * verticesPerMeshSide * col); j > top - (incrementY * verticesPerMeshSide * (col + 1)) - extra; j -= incrementY)
                {
                    for (float i = left + (incrementX * verticesPerMeshSide * row); i < left + (incrementX * verticesPerMeshSide * (row + 1)) + extra; i += incrementX)
                    {
                        vertices.Add(new Vector3(i, j));
                    }
                }
                meshes[col][row].SetVertices(vertices);
            }
        }

        //set mesh triangles
        List<int> triangles = new List<int>();

        for (int col = 0; col < meshes.Count; col++)
        {
            for (int row = 0; row < meshes[0].Count; row++)
            {
                triangles.Clear();
                for (int j = 0; j < verticesPerMeshSide; j++)
                {
                    for (int i = 0; i < verticesPerMeshSide; i++)
                    {
                        triangles.Add(((verticesPerMeshSide + 1) * j) + i);
                        triangles.Add(((verticesPerMeshSide + 1) * j) + i + 1);
                        triangles.Add((verticesPerMeshSide + 1) * (j + 1) + (i + 1));
                        triangles.Add((verticesPerMeshSide + 1) * (j + 1) + (i + 1));
                        triangles.Add((verticesPerMeshSide + 1) * (j + 1) + i);
                        triangles.Add(((verticesPerMeshSide + 1) * j) + i);
                    }
                }
                meshes[col][row].triangles = triangles.ToArray();
            }
        }

        //remove all non-canvas children from parent gameobject
        for (int i = 0; i < transform.childCount; i++)
        {
            if (!transform.GetChild(i).CompareTag("Canvas"))
            {
                Destroy(transform.GetChild(i).gameObject);
            }
        }

        //add all meshes to parent gameobject
        foreach (List<Mesh> l in meshes)
        {
            foreach (Mesh m in l)
            {
                GameObject g = new GameObject();
                g.AddComponent<MeshFilter>().mesh = m;
                g.AddComponent<MeshRenderer>();
                g.transform.SetParent(transform, false);
                m.bounds = new Bounds(new Vector3(0, 0, 0), new Vector3(10, 10, 10));
            }
        }

        if (!UpdateMaterial())
        {
            SetMaterial(mat);
            InitializeShader();
        }

        /*
        //set mesh triangles
        List<int> triangles = new List<int>();

        int edgeVertices = verticesPerMeshSide + 1;

        for (int j = 0; j < edgeVertices - 1; j++)
        {
            for (int i = 0; i < edgeVertices - 1; i++)
            {
                triangles.Add((edgeVertices * j) + i);
                triangles.Add((edgeVertices * j) + i + 1);
                triangles.Add(edgeVertices * (j + 1) + (i + 1));
                triangles.Add(edgeVertices * (j + 1) + (i + 1));
                triangles.Add(edgeVertices * (j + 1) + i);
                triangles.Add((edgeVertices * j) + i);
            }
        }
        infMesh.triangles = triangles.ToArray();*/
    }

    private bool UpdateMaterial()
    {
        if(savingPNG)
        {
            return false;
        }

        if (MandelbrotSettingsManager.resolution == ResolutionType.Infinite)
        {
            if (MandelbrotSettingsManager.doublePrecisionMode)
            {
                if(mat != infiniteResMaterial_DP)
                {
                    SetMaterial(infiniteResMaterial_DP);
                    InitializeShader();
                    return true;
                }
            }
            else
            {
                if (mat != infiniteResMaterial)
                {
                    SetMaterial(infiniteResMaterial);
                    InitializeShader();
                    return true;
                }
            }
        }
        else
        {
            if (MandelbrotSettingsManager.doublePrecisionMode)
            {
                if (mat != fixedResMaterial_DP)
                {
                    SetMaterial(fixedResMaterial_DP);
                    InitializeShader();
                    return true;
                }
            }
            else
            {
                if (mat != fixedResMaterial)
                {
                    SetMaterial(fixedResMaterial);
                    InitializeShader();
                    return true;
                }
            }
        }

        return false;
    }

    private void SetMaterial(Material m)
    {
        mat = m;

        for(int i = 0; i < transform.childCount; i++)
        {
            MeshRenderer r;
            if ((r = transform.GetChild(i).GetComponent<MeshRenderer>()) != null)
            {
                r.material = m;
            }
        }
    }

    #endregion

    #region Shader Updates

    public void InitializeShader()
    {
        if(mat == null)
        {
            return;
        }
        
        // Need to set colors array with max size on initialization because all future updates to arrays in shaders are trimmed down to their initial size
        mat.SetColorArray("colors", new Color[ColorspaceCreatorManager.MAX_COLORS * 2 - 2]);
        mat.SetInt("numColors", ColorspaceCreatorManager.MAX_COLORS * 2 - 2);

        UpdateShader();
    }

    public void UpdateResolution()
    {
        GenerateMesh();
    }

    public void UpdateShader()
    {
        UpdateFadeEdges();
        UpdateMaxIter();
        UpdateMandel();
        UpdateKVal();
        UpdateScale();
        UpdateTranslation();
        UpdateColorOffset();
        UpdateColorDensity();
        UpdateColors();

        if (MandelbrotSettingsManager.resolution != ResolutionType.Infinite && Globals.explorationMode)
        {
            UpdateDepth();
        }
    }

    public void UpdateMaxIter()
    {
        mat.SetInt("maxIter", MandelbrotSettingsManager.maxIterations);
    }
    public void UpdateMandel()
    {
        mat.SetInt("mandel", FractalSettingsManager.genType == GenerationType.Mandelbrot ? 1 : 0);
    }

    public void UpdateDepth()
    {
        mat.SetInt("depth", Globals.explorationMode ? 1 : 0);

        if (Globals.explorationMode)
        {
            UpdateDepthScale();
            UpdateInclination();
            UpdateInvertDepth();
        }
    }

    public void UpdateFadeEdges()
    {
        mat.SetInt("fadeEdges", Globals.HighQuality ? 1 : 0);
    }

    public void UpdateColorOffset()
    {
        if(ColorspaceSelectorManager.selectedColorspace == null)
        {
            return;
        }

        mat.SetFloat("colorOffset", (ColorspaceSelectorManager.colorShift + ColorspaceSelectorManager.animatedColorOffset) * ColorspaceSelectorManager.selectedColorspace.Length);
    }
    public void UpdateColorDensity()
    {
        mat.SetFloat("colorDensity", ColorspaceSelectorManager.colorDensity);
    }
    public void UpdateDepthScale()
    {
        mat.SetFloat("depthScale", ExplorationSettingsManager.depth);
    }
    public void UpdateInclination()
    {
        mat.SetFloat("inclination", ExplorationSettingsManager.inclination);
    }
    public void UpdateInvertDepth()
    {
        mat.SetInt("invertDepth", ExplorationSettingsManager.invertDepth ? 1 : 0);
    }
    public void UpdateColors()
    {
        // Array of colors that will be passed to shader
        Color[] colors;

        // Temp list of selected colors
        List<Color> selectedColorspace = new List<Color>(ColorspaceSelectorManager.selectedColorspace);

        if (ColorspaceSelectorManager.wrapColors && selectedColorspace.Count != 1)
        {
            // Initialize colors array with twice the number of selected colors (-2 because we won't duplicate the endpoints for wrapping)
            colors = new Color[selectedColorspace.Count * 2 - 2];

            // Copy forward list to first half of array
            selectedColorspace.CopyTo(colors);

            // Reverse list and remove the endpoints, then copy to second half of array
            selectedColorspace.RemoveAt(0);
            selectedColorspace.RemoveAt(selectedColorspace.Count - 1);
            selectedColorspace.Reverse();
            selectedColorspace.CopyTo(colors, selectedColorspace.Count + 2);
        }
        else
        {
            colors = selectedColorspace.ToArray();
        }

        mat.SetColorArray("colors", colors);
        mat.SetInt("numColors", colors.Length);
        
        // If high quality, set color of workshop light to average of selected colorspace's colors
        if (Globals.vrMode && Globals.HighQuality)
        {
            Vector3 totalColor = Vector3.zero;
            foreach (Color c in selectedColorspace)
            {
                totalColor += new Vector3(c.r, c.g, c.b);
            }
            Vector3 avgColor = totalColor / selectedColorspace.Count;
            VRGlobals.pointLight.color = new Color(avgColor.x, avgColor.y, avgColor.z);
        }
    }

    public void UpdateKVal()
    {
        Vector2d point = new Vector2d(
            JuliaSettingsManager.centerPoint.x + Mathf.Cos(JuliaSettingsManager.angle) * JuliaSettingsManager.radius,
            JuliaSettingsManager.centerPoint.y + Mathf.Sin(JuliaSettingsManager.angle) * JuliaSettingsManager.radius
            );

        if (MandelbrotSettingsManager.doublePrecisionMode)
        {
            mat.SetVector("ds_ka", point.x.toDS());
            mat.SetVector("ds_kb", point.y.toDS());
        }
        else
        {
            mat.SetFloat("ka", (float)point.x);
            mat.SetFloat("kb", (float)point.y);
        }
    }

    public void UpdateScale()
    {
        if (UpdateMaterial())
        {
            return;
        }

        if (MandelbrotSettingsManager.doublePrecisionMode)
        {
            mat.SetVector("ds_scale", MandelbrotSettingsManager.scale.toDS());
        }
        else
        {
            mat.SetFloat("scale", (float)MandelbrotSettingsManager.scale);
        }

        UpdateTranslation();
    }

    public void UpdateTranslation()
    {
        if (MandelbrotSettingsManager.doublePrecisionMode)
        {
            var x = MandelbrotSettingsManager.centerPoint.x.toDS();
            var y = MandelbrotSettingsManager.centerPoint.y.toDS();

            mat.SetVector("ds_centera", MandelbrotSettingsManager.centerPoint.x.toDS());
            mat.SetVector("ds_centerb", MandelbrotSettingsManager.centerPoint.y.toDS());
        }
        else
        {
            mat.SetFloat("centera", (float)MandelbrotSettingsManager.centerPoint.x);
            mat.SetFloat("centerb", (float)MandelbrotSettingsManager.centerPoint.y);
        }
    }
    
    #endregion
    
    public void SavePNG()
    {
        savingPNG = true;

        // First set PNG shader parameters

        Material originalMaterial = mat;

        mat = MandelbrotSettingsManager.doublePrecisionMode ? infiniteResMaterial_DP : infiniteResMaterial;

        UpdateShader();

        mat.SetFloat("fadeEdges", 0);

        if (MandelbrotSettingsManager.doublePrecisionMode)
        {
            mat.SetVector("ds_scale", (MandelbrotSettingsManager.scale * 2).toDS());
        }
        else
        {
            mat.SetFloat("scale", (float)MandelbrotSettingsManager.scale * 2);
        }
        
        if (MandelbrotSettingsManager.doublePrecisionMode)
        {
            mat.SetVector("ds_centera", (MandelbrotSettingsManager.centerPoint.x - MandelbrotSettingsManager.scale).toDS());
            mat.SetVector("ds_centerb", (MandelbrotSettingsManager.centerPoint.y - MandelbrotSettingsManager.scale).toDS());
        }
        else
        {
            mat.SetFloat("centera", (float)(MandelbrotSettingsManager.centerPoint.x - MandelbrotSettingsManager.scale));
            mat.SetFloat("centerb", (float)(MandelbrotSettingsManager.centerPoint.y - MandelbrotSettingsManager.scale));
        }

        RenderTexture buffer = new RenderTexture(
                               5000,
                               5000,
                               0,                            // No depth/stencil buffer
                               RenderTextureFormat.ARGB32,   // Standard colour format
                               RenderTextureReadWrite.Default // No sRGB conversions
                           );

        Graphics.Blit(null, buffer, mat, 0);

        RenderTexture.active = buffer;           // If not using a scene camera

        Texture2D outputTex = new Texture2D(5000, 5000, TextureFormat.ARGB32, false);

        outputTex.ReadPixels(
                  new Rect(0, 0, 5000, 5000), // Capture the whole texture
                  0, 0,                          // Write starting at the top-left texel
                  false                          // No mipmaps
        );

        File.WriteAllBytes(Path.Combine(SaveLoadManager.pngDirectory, DateTime.Now.ToOADate().ToString() + ".png"), outputTex.EncodeToPNG());

        mat = originalMaterial;

        UpdateShader();

        savingPNG = false;
    }

    private void OnApplicationFocus(bool focus)
    {
        if (focus && !Application.isEditor)
        {
            InitializeShader();
        }
    }
}
