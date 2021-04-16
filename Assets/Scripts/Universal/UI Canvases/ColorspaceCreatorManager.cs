using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorspaceCreatorManager : BaseUI<ColorspaceCreatorManager>
{
    #region UI Elements

    [SerializeField] private Transform colorItemsContent;
    [SerializeField] private Image previewImage;
    [SerializeField] private Image addButtonImage;

    [SerializeField] private Toggle addColorOnClickToggle;
    [SerializeField] private Button btnAddColor;
    [SerializeField] private Button btnGenerateRandomColorspace;
    [SerializeField] private Button btnClearColors;
    [SerializeField] private Button btnSaveColorspace;

    #endregion

    #region Private Instance Members

    private List<GameObject> colorItems = new List<GameObject>();

    #endregion

    #region Public Static Members

    public static bool addColorOnClick;

    public const int MAX_COLORS = 25;

    #endregion

    #region Private Static Members

    private static List<Color> colors;
    private static GameObject colorItemPrefab;

    #endregion

    #region Public Setter Methods

    private static bool settingAddColorOnClick = false;
    public static void SetAddColorOnClick(bool value, ColorspaceCreatorManager instanceToIgnore = null)
    {
        if (settingAddColorOnClick)
        {
            return;
        }

        settingAddColorOnClick = true;

        addColorOnClick = value;

        foreach (ColorspaceCreatorManager instance in instances)
        {
            if (instance == instanceToIgnore)
            {
                continue;
            }

            instance.addColorOnClickToggle.isOn = value;
        }

        settingAddColorOnClick = false;
    }

    #endregion
    
    #region Initialization + Updates

    protected override void InitializeStatics()
    {
        addColorOnClick = true;

        colors = new List<Color>();

        // Set colorItemPrefab to the prefab resource;
        colorItemPrefab = Resources.Load("ColorItem") as GameObject;

        // Subscribe to ColorPickerManager's colorChanged event
        ColorPickerManager.colorChanged.AddListener(ColorChanged);
    }

    protected override void InitializeInstance()
    {
        addColorOnClickToggle.isOn = addColorOnClick;
    }

    protected override void AddEventListeners()
    {
        addColorOnClickToggle.onValueChanged.AddListener((v) => SetAddColorOnClick(v, this));

        btnAddColor.onClick.AddListener(AddColorButton_Click);
        btnGenerateRandomColorspace.onClick.AddListener(GenerateRandomColorspace);
        btnClearColors.onClick.AddListener(ClearAllColors);
        btnSaveColorspace.onClick.AddListener(SaveColorspaceButtonClick);
    }

    #endregion

    #region Methods + Event Handlers

    public void AddColorButton_Click()
    {
        AddColor(ColorPickerManager.selectedColor);

        RegenColorSprite();
    }

    /// <summary>
    /// Adds specified color to <see cref="colors"/> and instantiates a new colorItem in all active instances using the specified color. <see cref="RegenColorSprite"/> should be called after all colors to add are added. <para/>
    /// Does not execute if <see cref="MAX_COLORS"/> has been exceeded
    /// </summary>
    /// <param name="color"><see cref="Color"/> to add</param>
    public static void AddColor(Color color)
    {
        if (colors.Count >= MAX_COLORS)
        {
            return;
        }

        InstantiateColorItem(color);

        colors.Add(color);
    }

    /// <summary>
    /// Instantiates a new colorItem in all active instances using the specified color
    /// </summary>
    /// <param name="color"><see cref="Color"/> of the item to instantiate</param>
    private static void InstantiateColorItem(Color color)
    {
        // Name all objects instantiated with same unique name so that they can be removed from all instances
        string name = DateTime.Now.ToOADate().ToString();

        foreach (ColorspaceCreatorManager instance in instances)
        {
            if (instance == null)
            {
                continue;
            }

            GameObject newColorItem = Instantiate(colorItemPrefab, instance.colorItemsContent, false);
            newColorItem.name = name;

            Button removeBtn = newColorItem.transform.Find("RemoveButton").GetComponent<Button>();
            removeBtn.onClick.AddListener(() => instance.RemoveColorButton_Click(removeBtn));

            Button shiftUpBtn = newColorItem.transform.Find("ShiftUpButton").GetComponent<Button>();
            shiftUpBtn.onClick.AddListener(() => instance.ShiftColorUpButton_Click(removeBtn));

            Image image = newColorItem.GetComponentInChildren<Image>();
            image.color = color;

            instance.colorItems.Add(newColorItem);
        }
    }

    /// <summary>
    /// Shifts the color item associated with the <see cref="Button"/> that activated this event in all active instances, and updates the colors
    /// </summary>
    /// <param name="btn"><see cref="Button"/> that was pressed. The parent of this <see cref="Button"/> is the colorItem <see cref="GameObject"/></param>
    private void ShiftColorUpButton_Click(Button btn)
    {
        int idx = colorItems.IndexOf(btn.transform.parent.gameObject);

        if (idx > 0)
        {

            foreach (ColorspaceCreatorManager instance in instances)
            {
                if (instance == null)
                {
                    continue;
                }

                GameObject colorItem = instance.colorItems[idx];

                instance.colorItems.RemoveAt(idx);
                instance.colorItems.Insert(idx - 1, colorItem);
                colors.RemoveAt(idx);
                colors.Insert(idx - 1, colorItem.GetComponentInChildren<Image>().color);

                colorItem.transform.SetSiblingIndex(idx - 1);
            }

            RegenColorSprite();
        }
    }

    /// <summary>
    /// Removes the color item associated with the <see cref="Button"/> that activated this event in all active instances, and updates the colors
    /// </summary>
    /// <param name="btn"><see cref="Button"/> that was pressed. The parent of this <see cref="Button"/> is the colorItem <see cref="GameObject"/></param>
    public void RemoveColorButton_Click(Button btn)
    {
        // All color items instantiated were given the same unique name; get name from this color item and use it to remove all instantiated color items
        string name = btn.transform.parent.name;

        foreach (ColorspaceCreatorManager instance in instances)
        {
            if (instance == null)
            {
                continue;
            }

            foreach (GameObject obj in instance.colorItems)
            {
                if (string.Equals(obj.name, name))
                {
                    instance.colorItems.Remove(obj);
                    Destroy(obj);
                    break;
                }
            }
        }

        RegenColors();
    }

    /// <summary>
    /// Clears color data and re-initializes it from this instance's colorItems, also regenerates colorspace preview in all instances
    /// </summary>
    private void RegenColors()
    {
        colors.Clear();
        foreach (GameObject obj in colorItems)
        {
            colors.Add(obj.GetComponentInChildren<Image>().color);
        }

        RegenColorSprite();
    }

    /// <summary>
    /// Regenerates colorspace preview in all instances
    /// </summary>
    private static void RegenColorSprite()
    {
        Sprite newSprite;

        if (colors.Count == 0)
        {
            newSprite = null;
        }
        else
        {
            Texture2D tex = new Texture2D(colors.Count, 1);
            tex.SetPixels(colors.ToArray());
            tex.Apply();

            newSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0, 0));
        }

        foreach (ColorspaceCreatorManager instance in instances)
        {
            instance.previewImage.sprite = newSprite;
        }
    }

    /// <summary>
    /// This should be tied to the event that is fired when <see cref="ColorPickerManager"/>'s color is changed. This updates the color of all instances' <see cref="addButtonImage"/> and adds the color to the colorspace if <see cref="addColorOnClick"/> is true.
    /// </summary>
    /// <param name="color">The new <see cref="Color"/></param>
    public static void ColorChanged(Color color)
    {
        foreach (ColorspaceCreatorManager c in instances)
        {
            c.addButtonImage.color = color;
        }

        if (addColorOnClick)
        {
            AddColor(color);

            RegenColorSprite();
        }
    }

    /// <summary>
    /// Creates new colorspace from <see cref="colors"/>,
    /// </summary>
    public static void SaveColorspaceButtonClick()
    {
        ColorspaceSelectorManager.SaveAndAddNewColorspace(colors.ToArray());
    }

    /// <summary>
    /// Clears all colors and repopulates with 2 to 10 random colors
    /// </summary>
    public void GenerateRandomColorspace()
    {
        ClearAllColors();

        int numColors = UnityEngine.Random.Range(2, 10);
        colors = new List<Color>();
        for (int i = 0; i < numColors; i++)
        {
            Color c = UnityEngine.Random.ColorHSV();
            c.a = 1;

            AddColor(c);
        }

        RegenColorSprite();
    }

    /// <summary>
    /// Removes all colors from <see cref="colors"/> and all colorItems from all active instances
    /// </summary>
    public static void ClearAllColors()
    {
        foreach (ColorspaceCreatorManager instance in instances)
        {
            for (int i = instance.colorItems.Count - 1; i >= 0; i--)
            {
                Destroy(instance.colorItems[i]);
                instance.colorItems.RemoveAt(i);
            }
        }

        colors.Clear();

        RegenColorSprite();
    }

    #endregion
}
