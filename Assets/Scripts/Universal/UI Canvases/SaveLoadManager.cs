using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
using System.Xml.Serialization;
using System;
using UnityEngine.EventSystems;

public class SaveLoadManager : BaseUI<SaveLoadManager>
{
    #region UI Elements

    [SerializeField] private Transform content;
    [SerializeField] private SelectedOutline selectedOutline;
    [SerializeField] private HighlightedOutline highlightedOutline;

    [SerializeField] private Button btnSaveSettings;
    [SerializeField] private Button btnSavePNG;
    [SerializeField] private Button btnOpenPNGFolder;
    [SerializeField] private Button btnRemoveSelected;
    [SerializeField] private Button btnRefresh;

    #endregion

    #region Public Instance Members

    #endregion

    #region Private Instance Members

    private List<GameObject> savedSettingsItems;

    #endregion

    #region Public Static Members

    public static string pngDirectory;

    #endregion

    #region Private Static Members

    private static string savedSettingsDirectory;
    private static XmlSerializer serializer;

    #endregion

    #region Public Setters

    #endregion

    #region Initialization + Updates

    protected override void InitializeStatics()
    {
        savedSettingsDirectory = Path.Combine(Application.dataPath, "SavedSettings");
        Directory.CreateDirectory(savedSettingsDirectory);

        pngDirectory = Path.Combine(Application.dataPath, "Saved PNGs");
        Directory.CreateDirectory(pngDirectory);

        serializer = new XmlSerializer(typeof(Settings));
    }

    protected override void InitializeInstance()
    {
        var x = selectedOutline.outlineData;

        savedSettingsItems = new List<GameObject>();

        InstantiateSettingsItems();
    }

    protected override void AddEventListeners()
    {
        btnSaveSettings.onClick.AddListener(SaveSettings);
        btnSavePNG.onClick.AddListener(SavePNG);
        btnOpenPNGFolder.onClick.AddListener(OpenPNGFolder);
        btnRemoveSelected.onClick.AddListener(RemoveSelected);
        btnRefresh.onClick.AddListener(LoadSavedSettings);

        highlightedOutline.SelectedItemChanged.AddListener((v) => LoadSelected());
    }

    #endregion

    #region Methods and Event Handlers

    #endregion

    private void InstantiateSettingsItems()
    {
        DirectoryInfo di = new DirectoryInfo(savedSettingsDirectory);
        FileInfo[] files = di.GetFiles("*.xml");

        foreach (FileInfo f in files)
        {
            InstantiateSettingsItem(f.Name.Substring(0, f.Name.LastIndexOf('.')));
        }
    }

    private void InstantiateSettingsItem(string filenameNoExt)
    {
        // Text displayed by the GameObject
        string displayText;

        double oaDate;
        if (double.TryParse(filenameNoExt, out oaDate))
        {
            // Filename was in OADate format, but the display name should be in a readable format
            DateTime dt = DateTime.FromOADate(oaDate);
            displayText = dt.ToString();
        }
        else
        {
            displayText = filenameNoExt;
        }

        // Item to instantiate.
        GameObject item = new GameObject(filenameNoExt, typeof(Text), typeof(EventTrigger));

        // Configure text of settings object
        Text text = item.GetComponent<Text>();
        text.text = displayText;
        text.font = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
        text.fontStyle = FontStyle.Normal;
        text.alignment = TextAnchor.MiddleCenter;
        text.fontSize = 13;
        text.color = Color.black;
        item.transform.SetParent(content, false);
        RectTransform rt = item.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(content.GetComponent<RectTransform>().rect.width, 25);
        rt.anchorMin = new Vector2(0, 0);
        rt.anchorMax = new Vector2(0, 0);
        rt.pivot = new Vector2(0, 1);

        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerEnter;
        entry.callback.AddListener((e) =>
        {
            highlightedOutline.outlineData.SlideTo(item);
        });
        item.GetComponent<EventTrigger>().triggers.Add(entry);

        savedSettingsItems.Add(item);

        // If this is the first savedSettingsItem added, set it as selected
        if (savedSettingsItems.Count == 1)
        {
            SetOutlineParents(0);
        }
    }

    /// <summary>
    /// Instantiates a settingsItem from the specified filename in all instances
    /// </summary>
    /// <param name="filenameNoExt">Name of settings file, without the .xml extension</param>
    private static void InstantiateNewSettingsItem(string filenameNoExt)
    {
        foreach (SaveLoadManager instance in instances)
        {
            instance.InstantiateSettingsItem(filenameNoExt);
        }
    }

    /// <summary>
    /// Populates list of savedSettingsItems with items representing the XML files present in <see cref="savedSettingsDirectory"/>.
    /// </summary>
    public static void LoadSavedSettings()
    {
        // Remove existing items
        foreach (SaveLoadManager instance in instances)
        {
            // De-child outline objects from any colorspaceItems so they don't get destroyed when we destroy the colorspaceItems
            instance.highlightedOutline.transform.SetParent(instance.content.parent, false);
            instance.selectedOutline.transform.SetParent(instance.content.parent, false);

            // Destroy all savedSettingsItems from each instance and remove them from the instance's savedSettingsItems collection
            for (int i = instance.savedSettingsItems.Count - 1; i >= 0; i--)
            {
                Destroy(instance.savedSettingsItems[i]);
                instance.savedSettingsItems.RemoveAt(i);
            }
        }
        
        DirectoryInfo di = new DirectoryInfo(savedSettingsDirectory);
        FileInfo[] files = di.GetFiles("*.xml");

        foreach (FileInfo f in files)
        {
            InstantiateNewSettingsItem(f.Name.Substring(0, f.Name.LastIndexOf('.')));
        }

        foreach(SaveLoadManager instance in instances)
        {
            if(instance.savedSettingsItems.Count > 0)
            {
                instance.SetOutlineParents(0);
            }
        }
    }

    public void RemoveSelected()
    {
        // Index of item to remove
        int idx = savedSettingsItems.IndexOf(selectedOutline.outlineData.selectedItem);

        // Remove outlines from children of item
        highlightedOutline.transform.SetParent(content.parent, false);
        selectedOutline.transform.SetParent(content.parent, false);

        // Remove file from hard drive
        string filename = savedSettingsDirectory + '\\' + savedSettingsItems[idx].name + ".xml";
        if (File.Exists(filename))
        {
            File.Delete(filename);
        }

        // Remove each instance from the game and its collection
        foreach(SaveLoadManager instance in instances)
        {
            Destroy(instance.savedSettingsItems[idx]);
            instance.savedSettingsItems.RemoveAt(idx);

            // Reset outlines
            if (idx - 1 >= 0)
            {
                SetOutlineParents(idx - 1);
            }
            else if (idx == 0)
            {
                SetOutlineParents(0);
            }
        }
    }

    public void SaveSettings()
    {
        Settings settings = new Settings();

        string filenameNoExt = DateTime.Now.ToOADate().ToString();
        using (FileStream f = new FileStream(savedSettingsDirectory + '\\' + filenameNoExt + ".xml", FileMode.Create))
        {
            serializer.Serialize(f, settings);
            InstantiateNewSettingsItem(filenameNoExt);
        }
    }

    public void SavePNG()
    {
        MeshManager.Instance.SavePNG();
    }

    private void OpenPNGFolder()
    {
        System.Diagnostics.Process.Start(pngDirectory);
    }

    public void LoadSelected()
    {
        Settings settings;

        try
        {
            using (FileStream f = new FileStream(savedSettingsDirectory + '\\' + selectedOutline.outlineData.selectedItem.name + ".xml", FileMode.Open, FileAccess.Read))
            {
                if (!Globals.vrMode) NonVRGlobals.debugLogger.text = $"Attempting to read settings from {f.Name}";
                settings = (Settings)serializer.Deserialize(f);
            }

            if (settings != null)
            {
                if (!Globals.vrMode) NonVRGlobals.debugLogger.text += Environment.NewLine + $"Settings read successfully." + Environment.NewLine + "Attempting to set settings";
                settings.SetSettings();
            }
            else
            {
                if (!Globals.vrMode) NonVRGlobals.debugLogger.text += Environment.NewLine + $"Failed to read settings.";
            }
            if (!Globals.vrMode) NonVRGlobals.debugLogger.text += Environment.NewLine + $"Settings set successfully.";
        }
        catch(Exception e)
        {
            if (!Globals.vrMode) NonVRGlobals.debugLogger.text += Environment.NewLine + $"Failed to set settings: {e.Message}";
        }
    }

    private void SetOutlineParents(int idx)
    {
        highlightedOutline.outlineData.selectedItem = savedSettingsItems[idx];
        selectedOutline.outlineData.selectedItem = savedSettingsItems[idx];
        highlightedOutline.transform.SetParent(savedSettingsItems[idx].transform, false);
        selectedOutline.transform.SetParent(savedSettingsItems[idx].transform, false);
    }
}
