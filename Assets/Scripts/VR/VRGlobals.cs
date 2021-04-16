using UnityEngine;
using VRTK;

public class VRGlobals : MonoBehaviour
{
    #region Serialized Members

    [SerializeField] private VRTK_ControllerEvents _leftControllerEvents, _rightControllerEvents;
    [SerializeField] private VRTK_Pointer _rightControllerPointer;

    [SerializeField] private Animator _fadeOutCanvasesAnimator, _fadeOutFloorAnimator, _meshVRTransitionAnimator, _rotatorAnimator;

    [SerializeField] private Light _pointLight;

    #endregion

    #region Public Static Members

    public static VRTK_ControllerEvents leftControllerEvents;
    public static VRTK_ControllerEvents rightControllerEvents;
    public static VRTK_Pointer rightControllerPointer;

    public static Animator fadeOutCanvasesAnimator, fadeOutFloorAnimator, meshVRTransitionAnimator, rotatorAnimator;

    public static Light pointLight;

    #endregion

    private void Start()
    {
        leftControllerEvents = _leftControllerEvents;
        rightControllerEvents = _rightControllerEvents;
        rightControllerPointer = _rightControllerPointer;

        fadeOutCanvasesAnimator = _fadeOutCanvasesAnimator;
        fadeOutFloorAnimator = _fadeOutFloorAnimator;
        meshVRTransitionAnimator = _meshVRTransitionAnimator;
        rotatorAnimator = _rotatorAnimator;

        pointLight = _pointLight;
    }
}
