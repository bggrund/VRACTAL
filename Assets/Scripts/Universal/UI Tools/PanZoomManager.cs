using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VRTK;

public class PanZoomManager : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    #region Singleton
    private static PanZoomManager instance;
    public static PanZoomManager Instance { get { return instance; } }

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
        }
    }
    #endregion

    [SerializeField] private Image circle;
    [SerializeField] private float canvasOffset = 0.15f;

    private Canvas canvas;
    private Image canvasImage;

    private bool pointerOnImage = false;
    private bool pointerDraggedOntoImage = false;
    private bool triggerPressed = false;
    private bool dragging = false;
    private bool selectingJuliaRotation = false;
    private bool explorationInteraction = false;
    private Vector2 dragStartRaw = Vector3.zero;
    private Vector2 dragStartLocal = Vector3.zero;
    private Vector2d centerPointStart = Vector3d.zero;

    #region VR Members

    private VRTK_ControllerEvents rightControllerEvents;
    private VRTK_Pointer pointer;

    #endregion

    // Use this for initialization
    void Start () {
        if (Globals.vrMode)
        {
            rightControllerEvents = VRGlobals.rightControllerEvents;
            pointer = VRGlobals.rightControllerPointer;

            rightControllerEvents.ButtonTwoPressed += RightControllerEvents_MenuButtonPressed;
        }

        canvas = GetComponentInParent<Canvas>();
        canvasImage = GetComponentInParent<Image>();
    }

    private void RightControllerEvents_MenuButtonPressed(object sender, ControllerInteractionEventArgs e)
    {
        canvas.gameObject.SetActive(!canvas.gameObject.activeSelf);
    }

    // Update is called once per frame
    void Update ()
    {
        if (pointerOnImage)
        {
            if (Globals.vrMode)
            {
                if (pointerDraggedOntoImage)
                {
                    if (rightControllerEvents.triggerPressed)
                    {
                        return;
                    }
                    else
                    {
                        pointerDraggedOntoImage = false;
                    }
                }
                if (rightControllerEvents.triggerPressed && !triggerPressed)
                //initial trigger press; set flag for drag and get start position info
                {
                    dragging = true;
                    triggerPressed = true;
                    Vector3 rawPoint = pointer.pointerRenderer.GetDestinationHit().point;
                    dragStartRaw = (Globals.explorationMode && !Globals.portraitMode) ? new Vector2(rawPoint.x, rawPoint.z) : new Vector2(rawPoint.x, rawPoint.y);
                    centerPointStart = MandelbrotSettingsManager.centerPoint;

                    if (selectingJuliaRotation)     //drag start
                    {
                        dragStartLocal = transform.InverseTransformPoint(rawPoint);
                        JuliaSettingsManager.SetCenterPoint(centerPointStart + (Vector2d)dragStartLocal * MandelbrotSettingsManager.scale);

                        circle.rectTransform.localPosition = new Vector3(dragStartLocal.x, dragStartLocal.y, Globals.explorationMode ? 0 : -0.08f);
                        circle.rectTransform.sizeDelta = new Vector2(0, 0);
                    }
                }
                if (rightControllerEvents.touchpadPressed)
                {
                    //get angle on touchpad
                    Vector2 axis = rightControllerEvents.GetTouchpadAxis();
                    float angle = Mathf.Atan2(axis.y, axis.x);

                    //convert angle to degrees 0-360
                    angle = (angle > 0 ? angle : (2 * Mathf.PI + angle)) * 360 / (2 * Mathf.PI);
                    if (angle >= 45 && angle <= 135)
                    {
                        MandelbrotSettingsManager.DecrementScale();
                    }
                    else if (angle >= 225 && angle <= 315)
                    {
                        MandelbrotSettingsManager.IncrementScale();
                    }
                    MeshManager.Instance.UpdateScale();
                }
            }
            else
            {
                if (Input.GetMouseButtonDown(0))
                //initial mouse down; set flag to drag and get start position info
                {
                    dragging = true;
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(GetComponent<RectTransform>(), Input.mousePosition, Camera.main, out dragStartLocal);
                    centerPointStart = MandelbrotSettingsManager.centerPoint;

                    if (selectingJuliaRotation)     //drag start
                    {
                        JuliaSettingsManager.SetCenterPoint(centerPointStart + (Vector2d)dragStartLocal * MandelbrotSettingsManager.scale);

                        circle.rectTransform.localPosition = new Vector3(dragStartLocal.x, dragStartLocal.y, Globals.explorationMode ? 0 : -0.08f);
                        circle.rectTransform.sizeDelta = new Vector2(0, 0);
                    }
                }
                else if (Input.GetAxisRaw("Mouse ScrollWheel") > 0)
                {
                    MandelbrotSettingsManager.DecrementScale();
                }
                else if (Input.GetAxisRaw("Mouse ScrollWheel") < 0)
                {
                    MandelbrotSettingsManager.IncrementScale();
                }
            }
        }
        if (dragging)
        {
            if (Globals.vrMode)
            {
                Vector3 currentRawPoint = pointer.pointerRenderer.GetDestinationHit().point;

                // First check if no longer dragging
                if (!rightControllerEvents.triggerPressed)
                {
                    dragging = false;
                    triggerPressed = false;

                    if (selectingJuliaRotation)
                    {
                        UpdateJuliaSelection(transform.InverseTransformPoint(currentRawPoint));

                        StopJuliaSelection();
                    }

                    return;
                }

                // Otherwise, continue with controller drag logic

                if (selectingJuliaRotation)
                {
                    UpdateJuliaSelection(transform.InverseTransformPoint(currentRawPoint));
                }
                else
                {
                    Vector2 currentRawPos = (Globals.explorationMode && !Globals.portraitMode) ? new Vector2(currentRawPoint.x, currentRawPoint.z) : new Vector2(currentRawPoint.x, currentRawPoint.y);
                    Vector2d fractalShift = (Vector2d)(currentRawPos - dragStartRaw) * (MandelbrotSettingsManager.scale / transform.lossyScale.x);

                    MandelbrotSettingsManager.SetCenterPoint(centerPointStart - fractalShift);
                }
            }
            else
            {
                Vector2 currentLocalCursorPos;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(GetComponent<RectTransform>(), Input.mousePosition, Camera.main, out currentLocalCursorPos);

                // First check if no longer dragging
                if (Input.GetMouseButtonUp(0))
                {
                    dragging = false;

                    if (selectingJuliaRotation)
                    {
                        UpdateJuliaSelection(currentLocalCursorPos);

                        StopJuliaSelection();
                    }

                    return;
                }

                // Otherwise, continue with mouse drag logic

                if (selectingJuliaRotation)
                {
                    UpdateJuliaSelection(currentLocalCursorPos);
                }
                else
                {
                    Vector2d fractalShift = (Vector2d)(currentLocalCursorPos - dragStartLocal) * MandelbrotSettingsManager.scale;   //TODO - test if need to divide by lossy scale of this transform

                    MandelbrotSettingsManager.SetCenterPoint(centerPointStart - fractalShift);
                }
            }
        }
    }

    public void UpdateJuliaSelection(Vector2 currentLocalPoint)
    {
        // Convert local point to fractal point        
        Vector2d currentFractalPoint = centerPointStart + (Vector2d)currentLocalPoint * MandelbrotSettingsManager.scale;

        // Set circle
        float localRadius = (currentLocalPoint - dragStartLocal).magnitude;
        circle.rectTransform.sizeDelta = new Vector2(localRadius * 2, localRadius * 2);

        // Get angle from start to current point
        Vector2d angleVector = currentFractalPoint - JuliaSettingsManager.centerPoint;
        float angle = Mathf.Atan2((float)angleVector.y, (float)angleVector.x);

        // Normalize angle between 0 and 2*PI
        angle = (angle > 0) ? angle : (2 * Mathf.PI + angle);

        JuliaSettingsManager.SetAngle(angle);
        JuliaSettingsManager.SetRadius(angleVector.magnitude);
    }

    public void StartJuliaSelection()
    {
        selectingJuliaRotation = true;

        JuliaSettingsManager.SetJuliaRotationButtonText("(Click and drag across fractal)");

        FractalSettingsManager.SwitchToMandle();
    }

    private void StopJuliaSelection()
    {
        selectingJuliaRotation = false;

        circle.rectTransform.sizeDelta = new Vector2(0, 0);

        JuliaSettingsManager.SetJuliaRotationButtonText("Select Julia Rotation");

        MeshManager.Instance.UpdateKVal();

        FractalSettingsManager.SwitchToJulia();
    }

    /// <summary>
    /// Shifts the canvas off the z-axis a bit in exploration mode, where the mesh can normally extend out from the z-axis in front of the canvas and hide the Julia selection circle
    /// </summary>
    public void UpdateFractalInteractionCanvasZPos()
    {
        if (Globals.explorationMode)
        {
            canvas.transform.position = new Vector3(canvas.transform.position.x, canvas.transform.position.y, -canvasOffset);

            if(Globals.vrMode)
            {
                canvas.gameObject.SetActive(false);

                // Add translucence to canvas
                Color c = canvasImage.material.color;
                c.a = 0.05f;
                canvasImage.color = c;
            }
        }
        else
        {
            canvas.transform.position = new Vector3(canvas.transform.position.x, canvas.transform.position.y, 0);

            if(Globals.vrMode)
            {
                canvas.gameObject.SetActive(true);

                // Remove translucence
                Color c = canvasImage.color;
                c.a = 0;
                canvasImage.color = c;
            }
        }
    }

    public void OnPointerEnter(PointerEventData e)
    {
        pointerOnImage = true;
        if (Globals.vrMode && rightControllerEvents.triggerPressed && !dragging)
        {
            pointerDraggedOntoImage = true;
        }
    }

    public void OnPointerExit(PointerEventData e)
    {
        pointerOnImage = false;
    }
}
