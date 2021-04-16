using UnityEngine;

public class Globals : MonoBehaviour
{
    #region Private Serialized Fields

    [SerializeField] private bool _vrMode;

    #endregion

    #region Public Static Fields

    public static bool explorationMode = false;
    public static bool portraitMode = true;
    public static bool vrMode = false;

    public static bool HighQuality { get { return QualitySettings.names[QualitySettings.GetQualityLevel()].Equals("High"); } }

    #endregion

    private void Start()
    {
        vrMode = _vrMode;
    }

    public void SetPortraitMode(bool value)
    {
        portraitMode = value;
    }
}
