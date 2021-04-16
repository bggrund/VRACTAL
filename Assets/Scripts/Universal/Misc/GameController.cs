using System.Collections;
using UnityEngine;

public class GameController : MonoBehaviour
{
    #region Singleton
    private static GameController instance;
    public static GameController Instance { get { return instance; } }

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        else
        {
            instance = this;
            //DontDestroyOnLoad(this.gameObject);
        }
    }
    #endregion

    // The user shouldn't be able to toggle portraitMode when in exploration mode, and only one portraitMode toggle will ever be visible in exploration mode, so store its reference here so we can toggle its interactibility.
    [SerializeField] private UnityEngine.UI.Toggle portraitModeToggle;

    private void Start()
    {
        Screen.SetResolution(Screen.resolutions[0].width - 200, (Screen.resolutions[0].width - 200) / 2, false);
    }

    // Update is called once per frame
    void Update()
    {
        ColorspaceSelectorManager.StaticUpdate();
        JuliaSettingsManager.StaticUpdate();
        ExplorationSettingsManager.StaticUpdate();

        if (!Globals.vrMode && Globals.explorationMode && Input.GetKeyUp(KeyCode.Escape))
        {
            ExplorationLocomotion.Instance.ToggleLocomotionActive();
        }
    }

    public void ToggleExploration()
    {
        if (MeshManager.Instance == null)
        {
            return;
        }

        if (Globals.vrMode)
        {
            if (Globals.explorationMode)
            {
                StartCoroutine(DisableVRExploration_Coroutine());
            }
            else
            {
                EnableVRExploration();
            }
        }
        else
        {
            if (Globals.explorationMode)
            {
                StartCoroutine(DisableNonVRExploration_Coroutine());
            }
            else
            {
                EnableNonVRExploration();
            }
        }
    }

    private void EnableVRExploration()
    {
        // Fade out floor
        VRGlobals.fadeOutFloorAnimator.SetTrigger("Forward");

        // Fade out workshop canvases
        VRGlobals.fadeOutCanvasesAnimator.SetTrigger("Forward");

        // Fade in exploration canvases
        VRGlobals.rotatorAnimator.SetTrigger("Forward");

        // Set controller pointer angle
        VRGlobals.rightControllerPointer.transform.localRotation = Quaternion.Euler(50, 0, 0);
        VRGlobals.rightControllerPointer.transform.localPosition = new Vector3(0f, 0.00001f, 0.01f);

        // Disable portraitMode toggle interaction
        SetPortraitModeToggleInteractable(false);

        // Transition fractal mesh
        if (Globals.portraitMode)
        {
            VRGlobals.meshVRTransitionAnimator.SetTrigger("PortraitForward");
        }
        else
        {
            VRGlobals.meshVRTransitionAnimator.SetTrigger("LandscapeForward");
        }

        // Enable locomotion
        ExplorationLocomotion.Instance.SetLocomotionActive(true);

        // Set explorationMode enabled
        Globals.explorationMode = true;

        // Below updates must occur after explorationMode has been toggled as they reference its value

        // Update exploration toggle button text
        ExplorationSettingsManager.UpdateExplorationToggleButtonText();

        // Update fractal interaction canvas position
        PanZoomManager.Instance.UpdateFractalInteractionCanvasZPos();

        // Update fractal mesh depth
        MeshManager.Instance.UpdateDepth();
    }

    private IEnumerator DisableVRExploration_Coroutine()
    {
        // Set explorationMode disabled
        Globals.explorationMode = false;

        // Update exploration toggle button text
        ExplorationSettingsManager.UpdateExplorationToggleButtonText();

        // Disable locomotion
        ExplorationLocomotion.Instance.SetLocomotionActive(false);

        // Reset mesh transform to center of its parent before animating its parent
        yield return StartCoroutine(MeshManager.Instance.TransitionToCenter());

        // Fade out exploration canvases
        VRGlobals.rotatorAnimator.SetTrigger("Reverse");

        // Fade in workshop canvases
        VRGlobals.fadeOutCanvasesAnimator.SetTrigger("Reverse");

        // Fade in floor
        VRGlobals.fadeOutFloorAnimator.SetTrigger("Reverse");

        // Transition fractal mesh
        if (Globals.portraitMode)
        {
            VRGlobals.meshVRTransitionAnimator.SetTrigger("PortraitReverse");
        }
        else
        {
            VRGlobals.meshVRTransitionAnimator.SetTrigger("LandscapeReverse");
        }

        // Enable portraitMode toggle interaction
        SetPortraitModeToggleInteractable(true);
        
        // Set controller pointer angle
        VRGlobals.rightControllerPointer.transform.localRotation = Quaternion.Euler(0, 0, 0);
        VRGlobals.rightControllerPointer.transform.localPosition = new Vector3(0, 0, 0);

        // Wait until the fractal mesh has finished transitioning before depth-related updates
        yield return new WaitForSeconds(VRGlobals.meshVRTransitionAnimator.GetCurrentAnimatorStateInfo(0).length);

        // Update fractal interaction canvas position
        PanZoomManager.Instance.UpdateFractalInteractionCanvasZPos();

        // Update fractal mesh depth
        MeshManager.Instance.UpdateDepth();
    }

    private void EnableNonVRExploration()
    {
        // Disable portraitMode toggle interaction
        SetPortraitModeToggleInteractable(false);

        // Transition fractal mesh
        if (Globals.portraitMode)
        {
            NonVRGlobals.meshNonVRTransitionAnimator.SetTrigger("PortraitForward");
        }
        else
        {
            NonVRGlobals.meshNonVRTransitionAnimator.SetTrigger("LandscapeForward");
        }

        // Enable locomotion
        ExplorationLocomotion.Instance.SetLocomotionActive(true);

        // Set explorationMode enabled
        Globals.explorationMode = true;

        // Below updates must occur after explorationMode has been toggled as they reference its value
        
        // Update exploration toggle button text
        ExplorationSettingsManager.UpdateExplorationToggleButtonText();

        // Update fractal interaction canvas position
        PanZoomManager.Instance.UpdateFractalInteractionCanvasZPos();

        // Update fractal mesh depth
        MeshManager.Instance.UpdateDepth();
    }

    private IEnumerator DisableNonVRExploration_Coroutine()
    {
        // Set explorationMode disabled
        Globals.explorationMode = false;

        // Update exploration toggle button text
        ExplorationSettingsManager.UpdateExplorationToggleButtonText();

        // Smoothly return camera to its default front-facing orientation
        StartCoroutine(ExplorationLocomotion.Instance.ResetCamera());

        // Reset mesh transform to center of its parent before animating its parent
        yield return StartCoroutine(MeshManager.Instance.TransitionToCenter());

        // Transition fractal mesh
        if (Globals.portraitMode)
        {
            NonVRGlobals.meshNonVRTransitionAnimator.SetTrigger("PortraitReverse");
        }
        else
        {
            NonVRGlobals.meshNonVRTransitionAnimator.SetTrigger("LandscapeReverse");
        }

        // Enable portraitMode toggle interaction
        SetPortraitModeToggleInteractable(true);

        // Wait until the fractal mesh has finished transitioning before depth-related updates
        yield return new WaitForSeconds(NonVRGlobals.meshNonVRTransitionAnimator.GetCurrentAnimatorStateInfo(0).length);

        // Update fractal interaction canvas position
        PanZoomManager.Instance.UpdateFractalInteractionCanvasZPos();

        // Update fractal mesh depth
        MeshManager.Instance.UpdateDepth();
    }

    private void SetPortraitModeToggleInteractable(bool interactable)
    {
        portraitModeToggle.interactable = interactable;
    }
}
