using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using UnityEngine.EventSystems;
using System;
using System.Linq;

public class ColorspaceSelectorManager : BaseUI<ColorspaceSelectorManager>
{
    #region UI Elements

    [SerializeField] private Transform content;

    [SerializeField] private SelectedOutline selectedOutline;
    [SerializeField] private HighlightedOutline highlightedOutline;

    [SerializeField] private Slider colorDensitySlider;
    [SerializeField] private Slider colorShiftSlider;
    [SerializeField] private Slider colorAnimationSpeedSlider;
    [SerializeField] private Toggle wrapColorsToggle;

    [SerializeField] private Button btnReloadColorspaces;
    [SerializeField] private Button btnRemoveSelectedColorspace;

    #endregion

    #region Private Instance Members

    private List<GameObject> colorspaceItems;

    #endregion

    #region Public Static Members

    public static bool wrapColors;
    public static float colorDensity;
    public static float colorShift;
    public static float colorAnimationSpeed;
    public static Color[] selectedColorspace;

    public static float sqrtColorDensity;
    public static float animatedColorOffset;

    #endregion

    #region Private Static Members

    private static List<ColorspaceData> colorspaceDataList;
    private static int selectedColorspaceIndex = -1;
    private static GameObject colorspaceImagePrefab;
    private static string colorspaceDirectory;

    #endregion

    #region Public Setters

    private static bool settingWrapColors = false;
    public static void SetWrapColors(bool value, ColorspaceSelectorManager instanceToIgnore = null)
    {
        if (settingWrapColors)
        {
            return;
        }

        settingWrapColors = true;

        wrapColors = value;

        foreach (ColorspaceSelectorManager instance in instances)
        {
            if (instance == instanceToIgnore)
            {
                continue;
            }

            instance.wrapColorsToggle.isOn = value;
        }

        settingWrapColors = false;

        MeshManager.Instance.UpdateColors();
    }

    private static bool settingColorDensity = false;
    public static void SetColorDensity(float value, ColorspaceSelectorManager instanceToIgnore = null)
    {
        if (settingColorDensity)
        {
            return;
        }

        settingColorDensity = true;

        colorDensity = value;
        sqrtColorDensity = Mathf.Sqrt(value);

        foreach (ColorspaceSelectorManager instance in instances)
        {
            if (instance == instanceToIgnore)
            {
                continue;
            }

            instance.colorDensitySlider.value = value;
        }

        settingColorDensity = false;

        MeshManager.Instance.UpdateColorDensity();
    }

    private static bool settingColorShift = false;
    public static void SetColorShift(float value, ColorspaceSelectorManager instanceToIgnore = null)
    {
        if (settingColorShift)
        {
            return;
        }

        settingColorShift = true;

        colorShift = value;
        animatedColorOffset = 0;

        foreach (ColorspaceSelectorManager instance in instances)
        {
            if (instance == instanceToIgnore)
            {
                continue;
            }

            instance.colorShiftSlider.value = value;
        }

        settingColorShift = false;

        MeshManager.Instance.UpdateColorOffset();
    }

    private static bool settingColorAnimationSpeed = false;
    public static void SetColorAnimationSpeed(float value, ColorspaceSelectorManager instanceToIgnore = null)
    {
        if (settingColorAnimationSpeed)
        {
            return;
        }

        settingColorAnimationSpeed = true;

        colorAnimationSpeed = value;

        foreach (ColorspaceSelectorManager instance in instances)
        {
            if (instance == instanceToIgnore)
            {
                continue;
            }

            instance.colorAnimationSpeedSlider.value = value;
        }

        settingColorAnimationSpeed = false;
    }
    
    /// <summary>
    /// Sets the selected colorspace based on the passed in colorspace. This will add the colorspace to the collection and instantiate a new colorspaceItem in all instances if it doesn't already exist.
    /// </summary>
    /// <param name="colorspace">Colorspace to add</param>
    public static void SetSelectedColorspace(Color[] colorspace, ColorspaceSelectorManager instanceToIgnore = null)
    {
        if (!ColorspaceDataListContains(colorspace))
        {
            SaveAndAddNewColorspace(colorspace);
        }

        selectedColorspace = colorspace;
        selectedColorspaceIndex = colorspaceDataList.FindIndex(c => c.colorspace.SequenceEqual(colorspace));

        foreach (ColorspaceSelectorManager instance in instances)
        {
            if(instance == instanceToIgnore)
            {
                continue;
            }

            instance.SetOutlineSelection(selectedColorspaceIndex);
        }

        MeshManager.Instance.UpdateColors();
    }

    /*
    /// <summary>
    /// Sets the selected colorspace based on the passed in colorspaceItem, which should be a member of this instance's colorspaceItems collection. Also updates all other instances to have their instance of this colorspaceItem selected.
    /// </summary>
    /// <param name="colorspaceItem">The colorspaceItem that has been selected</param>
    private void SetSelectedColorspace(GameObject colorspaceItem)
    {
        if (!colorspaceItems.Contains(colorspaceItem))
        {
            return;
        }

        selectedColorspaceIndex = colorspaceItems.IndexOf(colorspaceItem);
        selectedColorspace = colorspaces[selectedColorspaceIndex];

        foreach (ColorspaceSelectorManager instance in Instances)
        {
            if(instance == this)
            {
                continue;
            }

            instance.SetOutlineSelection(selectedColorspaceIndex);
        }
    }
    */
    #endregion

    #region Initialization + Updates

    protected override void InitializeStatics()
    {
        wrapColors = true;
        colorDensity = 0.0075f;
        sqrtColorDensity = Mathf.Sqrt(colorDensity);
        colorShift = 0;
        colorAnimationSpeed = 0;
        animatedColorOffset = 0;

        // Set colorspaceDirectory and create if it doesn't already exist
        colorspaceDirectory = Path.Combine(Application.dataPath, "Colorspaces");
        Directory.CreateDirectory(colorspaceDirectory);

        // Set colorspaceImagePrefab to the prefab resource;
        colorspaceImagePrefab = Resources.Load("ColorspaceItem") as GameObject;

        colorspaceDataList = new List<ColorspaceData>();

        InitializeColorspaceDataList();
    }

    protected override void InitializeInstance()
    {
        colorspaceItems = new List<GameObject>();

        wrapColorsToggle.isOn = wrapColors;

        colorDensitySlider.minValue = 0.001f;
        colorDensitySlider.maxValue = 0.02f;
        colorDensitySlider.value = colorDensity;

        colorShiftSlider.minValue = 0;
        colorShiftSlider.maxValue = 1;
        colorShiftSlider.value = colorShift;

        colorAnimationSpeedSlider.minValue = 0;
        colorAnimationSpeedSlider.maxValue = 0.02f;
        colorAnimationSpeedSlider.value = colorAnimationSpeed;

        InstantiateColorspaceItems();
    }

    protected override void AddEventListeners()
    {
        wrapColorsToggle.onValueChanged.AddListener((v) => SetWrapColors(v, this));
        colorDensitySlider.onValueChanged.AddListener((v) => SetColorDensity(v, this));
        colorShiftSlider.onValueChanged.AddListener((v) => SetColorShift(v, this));
        colorAnimationSpeedSlider.onValueChanged.AddListener((v) => SetColorAnimationSpeed(v, this));

        highlightedOutline.SelectedItemChanged.AddListener((v) => SetSelectedColorspace(v.GetComponent<SelectableColorspace>().colorspaceData.colorspace, this));

        btnReloadColorspaces.onClick.AddListener(ReloadColorspaces);
        btnRemoveSelectedColorspace.onClick.AddListener(RemoveSelected);
    }

    public static void StaticUpdate()
    {
        // Animate colors
        if (colorAnimationSpeed > 0f)
        {
            animatedColorOffset += colorAnimationSpeed * Time.deltaTime * 90;// * sqrtColorDensity;
            MeshManager.Instance.UpdateColorOffset();
        }
    }

    #endregion

    #region Methods and Event Handlers

    private void InstantiateColorspaceItems()
    {
        foreach(ColorspaceData colorspaceData in colorspaceDataList)
        {
            Texture2D tex = new Texture2D(colorspaceData.colorspace.Length, 1);
            tex.SetPixels(colorspaceData.colorspace);
            tex.Apply();

            InstantiateColorspaceItem(tex, colorspaceData);
        }
    }

    private void InstantiateColorspaceItem(Texture2D tex, ColorspaceData colorspaceData)
    {
        GameObject item = Instantiate(colorspaceImagePrefab, content, false);
        Sprite s = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0, 0));
        item.GetComponent<Image>().sprite = s;

        SelectableColorspace selectableColorspace = item.GetComponent<SelectableColorspace>();
        selectableColorspace.colorspaceData = colorspaceData;

        EventTrigger.Entry eventTriggerEntry = new EventTrigger.Entry();
        eventTriggerEntry.eventID = EventTriggerType.PointerEnter;
        eventTriggerEntry.callback.AddListener((e) => highlightedOutline.outlineData.SlideTo(item));
        item.AddComponent<EventTrigger>().triggers.Add(eventTriggerEntry);

        colorspaceItems.Add(item);

        // If this was the first colorspaceItem instantiated, set the selection outlines as its children
        if (colorspaceItems.Count == 1)
        {
            SetOutlineSelection(0);
        }
    }
    
    public static void InstantiateNewColorspaceItem(ColorspaceData colorspaceData)
    {
        Texture2D tex = new Texture2D(colorspaceData.colorspace.Length, 1);
        tex.SetPixels(colorspaceData.colorspace);
        tex.Apply();

        foreach (ColorspaceSelectorManager instance in instances)
        {
            instance.InstantiateColorspaceItem(tex, colorspaceData);
        }
    }

    /// <summary>
    /// If the specified colorspace doesn't already exist in <see cref="colorspaceDataList"/>, this saves the specified colorspace to a PNG in colorspaceDirectory, adds the colorspace to <see cref="colorspaceDataList"/> and instantiates a new colorspaceItem from the specified colors in all instances
    /// </summary>
    /// <param name="newColorspace">Colors to create colorspace from</param>
    public static void SaveAndAddNewColorspace(Color[] newColorspace)
    {
        if (ColorspaceDataListContains(newColorspace))
        {
            return;
        }

        ColorspaceData colorspaceData = new ColorspaceData(Path.Combine(colorspaceDirectory, DateTime.Now.ToOADate().ToString() + ".png"), newColorspace);

        SaveColorspace(colorspaceData);

        colorspaceDataList.Add(colorspaceData);

        InstantiateNewColorspaceItem(colorspaceData);
    }

    private static bool ColorspaceDataListContains(Color[] colorspaceToCompare)
    {
        foreach (ColorspaceData colorspaceData in colorspaceDataList)
        {
            if (colorspaceData.colorspace.SequenceEqual(colorspaceToCompare))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Removes all colorspaces from <see cref="colorspaceDataList"/> and all colorspaceItems from all active instances
    /// </summary>
    public static void ClearAllColorspaces()
    {
        foreach (ColorspaceSelectorManager instance in instances)
        {
            // De-child outline objects from any colorspaceItems so they don't get destroyed when we destroy the colorspaceItems
            instance.highlightedOutline.transform.SetParent(instance.content.parent, false);
            instance.selectedOutline.transform.SetParent(instance.content.parent, false);

            // Destroy all colorspaceItems from each instance and remove them from the instance's colorspaceItems collection
            for (int i = instance.colorspaceItems.Count - 1; i >= 0; i--)
            {
                Destroy(instance.colorspaceItems[i]);
                instance.colorspaceItems.RemoveAt(i);
            }
        }

        colorspaceDataList.Clear();
    }

    public static void SaveColorspace(ColorspaceData colorspaceData)
    {
        Texture2D tex = new Texture2D(colorspaceData.colorspace.Length, 1);
        tex.SetPixels(colorspaceData.colorspace);

        File.WriteAllBytes(Path.Combine(colorspaceDirectory, colorspaceData.filename), tex.EncodeToPNG());
    }

    /// <summary>
    /// Clears all colorspace data, loads colorspaces from the PNGs in colorspaceDirectory into <see cref="colorspaceDataList"/>, and selects the first one if it exists
    /// </summary>
    public static void InitializeColorspaceDataList()
    {
        DirectoryInfo di = new DirectoryInfo(colorspaceDirectory);
        FileInfo[] files = di.GetFiles("*.png");

        // Load each colorspace PNG from colorspaceDirectory into a texture and store 
        foreach (FileInfo f in files)
        {
            LoadColorspaceData(f.FullName);
        }

        if(colorspaceDataList.Count == 0)
        {
            selectedColorspace = new Color[] { Color.red, Color.green, Color.blue };
        }
        else
        {
            selectedColorspace = colorspaceDataList[0].colorspace;
        }
    }

    /// <summary>
    /// Loads a colorspace from a specified PNG filename, adding it to <see cref="colorspaceDataList"/>
    /// </summary>
    /// <param name="filename">Full name of PNG file to load colors from</param>
    public static void LoadColorspaceData(string filename)
    {
        Texture2D fullPNG = new Texture2D(1, 1);
        fullPNG.LoadImage(File.ReadAllBytes(filename));

        Color[] colors = fullPNG.GetPixels(0, fullPNG.height - 1, fullPNG.width, 1);

        if (ColorspaceDataListContains(colors))
        {
            return;
        }

        ColorspaceData colorspaceData = new ColorspaceData(filename, colors);

        colorspaceDataList.Add(colorspaceData);
    }

    private static void ReloadColorspaces()
    {
        ClearAllColorspaces();

        InitializeColorspaceDataList();

        foreach(ColorspaceSelectorManager instance in instances)
        {
            instance.InstantiateColorspaceItems();
        }
    }

    public static void RemoveSelected()
    {
        // First, remove file from hard drive
        string filename = colorspaceDataList[selectedColorspaceIndex].filename;
        if (File.Exists(filename))
        {
            File.Delete(filename);
        }

        // Destroy all colorspaceItems in all instances and remove them from all instances' colorspaceItems collection
        foreach (ColorspaceSelectorManager instance in instances)
        {
            // De-child outline objects from the selected colorspaceItem so it doesn't get destroyed when we destroy the colorspaceItem
            instance.highlightedOutline.transform.SetParent(instance.content.parent, false);
            instance.selectedOutline.transform.SetParent(instance.content.parent, false);

            Destroy(instance.colorspaceItems[selectedColorspaceIndex]);
            instance.colorspaceItems.RemoveAt(selectedColorspaceIndex);
        }

        colorspaceDataList.RemoveAt(selectedColorspaceIndex);

        if (selectedColorspaceIndex - 1 >= 0)
        {
            SetSelectedColorspace(colorspaceDataList[selectedColorspaceIndex - 1].colorspace);
        }
        else if (selectedColorspaceIndex == 0 && colorspaceDataList.Count > 0)
        {
            SetSelectedColorspace(colorspaceDataList[0].colorspace);
        }
    }

    private void SetOutlineSelection(int idx)
    {
        if (idx >= colorspaceItems.Count || idx < 0)
        {
            return;
        }

        SetOutlineSelection(colorspaceItems[idx]);
    }

    private void SetOutlineSelection(GameObject obj)
    {
        if (!colorspaceItems.Contains(obj))
        {
            return;
        }

        highlightedOutline.outlineData.selectedItem = obj;
        selectedOutline.outlineData.selectedItem = obj;
        highlightedOutline.transform.SetParent(obj.transform, false);
        selectedOutline.transform.SetParent(obj.transform, false);
    }

    #endregion
}
