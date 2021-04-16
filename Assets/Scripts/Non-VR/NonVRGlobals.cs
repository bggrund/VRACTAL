using UnityEngine;

public class NonVRGlobals : MonoBehaviour {

    #region Serialized Members

    [SerializeField] private Animator _overlayAnimator, _meshNonVRTransitionAnimator;
    [SerializeField] private UnityEngine.UI.Text _debugLogger;

    #endregion

    #region Public Static Members

    public static Animator overlayAnimator, meshNonVRTransitionAnimator;
    public static UnityEngine.UI.Text debugLogger;

    #endregion

    private void Start()
    {
        overlayAnimator = _overlayAnimator;
        meshNonVRTransitionAnimator = _meshNonVRTransitionAnimator;
        debugLogger = _debugLogger;
    }
}
