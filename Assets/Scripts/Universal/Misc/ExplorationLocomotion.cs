using System;
using System.Collections;
using UnityEngine;
using VRTK;

/// <summary>
/// This should be attached to the camera rig for VR mode and the main camera for nonVR mode
/// </summary>
public class ExplorationLocomotion : MonoBehaviour {

    #region Singleton
    private static ExplorationLocomotion instance;
    public static ExplorationLocomotion Instance { get { return instance; } }

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

    [SerializeField] private float vrDampTime = 0.75f;
    [SerializeField] private float nonVrDampTime = 0.1f;
    [SerializeField] private float scaleMultiplier = 0.1f;

    private Transform meshTransform;

    private Vector3 newPos, oldPos;
    private Vector3 posVel;

    private float newScale, oldScale;
    private Vector3 scaleVel;
    
    private float camXRefVel, camYRefVel, camZRefVel;

    private bool locomotionActive = false;

    #region NonVR Members

    [SerializeField] private float camTransitionTime = 0.25f;
    [SerializeField] private float camTransitionThreshold = 0.01f;

    [SerializeField] private float mouseSensitivity = 100.0f;
    [SerializeField] private float clampAngle = 80.0f;

    private float rotY = 0.0f; // rotation around the up/y axis
    private float rotX = 0.0f; // rotation around the right/x axis

    #endregion

    #region VR Members

    private Transform leftControllerTransform;
    private VRTK_ControllerEvents leftControllerEvents, rightControllerEvents;

    #endregion

    // Use this for initialization
    void Start ()
    {
        if (Globals.vrMode)
        {
            leftControllerEvents = VRGlobals.leftControllerEvents;
            rightControllerEvents = VRGlobals.rightControllerEvents;
            leftControllerTransform = leftControllerEvents.gameObject.transform;
        }

        meshTransform = MeshManager.Instance.gameObject.transform;

        ResetLocomotion();       
	}

	// Update is called once per frame
	void Update ()
    {
        if (!locomotionActive)
        {
            return;
        }

        if (Globals.vrMode)
        {
            UpdatePositionVR();
            UpdateScaleVR();
        }
        else
        {
            UpdateCameraNonVR();
            UpdatePositionNonVR();
            UpdateScaleNonVR();
        }

        meshTransform.localPosition = Vector3.SmoothDamp(meshTransform.localPosition, newPos, ref posVel, (Globals.vrMode ? vrDampTime : nonVrDampTime));
        meshTransform.localScale = Vector3.SmoothDamp(meshTransform.localScale, new Vector3(newScale, newScale, newScale), ref scaleVel, (Globals.vrMode ? vrDampTime : nonVrDampTime));
    }

    private void UpdatePositionVR()
    {
        if (leftControllerEvents.touchpadPressed)
        {
            Vector2 leftAxis = leftControllerEvents.GetTouchpadAxis();
            oldPos = newPos;
            if (Globals.portraitMode)
            {
                newPos = new Vector3(
                    oldPos.x - (leftControllerTransform.forward.x * leftAxis.y * ExplorationSettingsManager.flySpeed) - ((leftControllerTransform.forward.z + leftControllerTransform.forward.y) * leftAxis.x * ExplorationSettingsManager.flySpeed),
                    oldPos.y - (leftControllerTransform.forward.y * leftAxis.y * ExplorationSettingsManager.flySpeed),
                    oldPos.z - (leftControllerTransform.forward.z * leftAxis.y * ExplorationSettingsManager.flySpeed) + ((leftControllerTransform.forward.x) * leftAxis.x * ExplorationSettingsManager.flySpeed)
                    );
            }
            else
            {
                newPos = new Vector3(
                    oldPos.x - (leftControllerTransform.forward.x * leftAxis.y * ExplorationSettingsManager.flySpeed) - ((leftControllerTransform.forward.z + leftControllerTransform.forward.y) * leftAxis.x * ExplorationSettingsManager.flySpeed),
                    oldPos.y - (leftControllerTransform.forward.z * leftAxis.y * ExplorationSettingsManager.flySpeed) + (leftControllerTransform.forward.x * leftAxis.x * ExplorationSettingsManager.flySpeed),
                    oldPos.z + (leftControllerTransform.forward.y * leftAxis.y * ExplorationSettingsManager.flySpeed)
                    );
            }
        }
    }

    private void UpdateScaleVR()
    {
        if (rightControllerEvents.touchpadPressed)
        {
            // Get angle on touchpad
            Vector2 rightAxis = rightControllerEvents.GetTouchpadAxis();
            float angle = Mathf.Atan2(rightAxis.y, rightAxis.x);

            // Convert angle to degrees 0-360
            angle = (angle > 0 ? angle : (2 * Mathf.PI + angle)) * 360 / (2 * Mathf.PI);

            if (angle <= 45 || angle >= 315)        //increase scale
            {
                oldScale = newScale;
                newScale = oldScale * (MandelbrotSettingsManager.zoomSpeed);
            }
            else if (angle >= 135 && angle <= 225)  //decrease scale
            {
                oldScale = newScale;
                newScale = oldScale / (MandelbrotSettingsManager.zoomSpeed);
            }
        }
    }

    private void UpdateCameraNonVR()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = -Input.GetAxis("Mouse Y");

        if (mouseX != 0 || mouseY != 0)
        {
            rotY += mouseX * mouseSensitivity * Time.deltaTime;
            rotX += mouseY * mouseSensitivity * Time.deltaTime;

            rotX = Mathf.Clamp(rotX, -clampAngle, clampAngle);

            Quaternion localRotation = Quaternion.Euler(rotX, rotY, 0.0f);
            transform.rotation = localRotation;
        }
    }

    private void UpdatePositionNonVR()
    {
        int xAxis = Input.GetKey(KeyCode.D) ? 1 : Input.GetKey(KeyCode.A) ? -1 : 0;
        int zAxis = Input.GetKey(KeyCode.W) ? 1 : Input.GetKey(KeyCode.S) ? -1 : 0;
        int yAxis = Input.GetKey(KeyCode.LeftShift) ? 1 : Input.GetKey(KeyCode.LeftControl) ? -1 : 0;

        oldPos = newPos;
        if (Globals.portraitMode)
        {
            newPos = new Vector3(
                oldPos.x - (transform.forward.x * zAxis * ExplorationSettingsManager.flySpeed) - ((transform.forward.z) * xAxis * ExplorationSettingsManager.flySpeed),
                oldPos.y - (yAxis * ExplorationSettingsManager.flySpeed) - (transform.forward.y * zAxis * ExplorationSettingsManager.flySpeed),
                oldPos.z - ((transform.forward.z) * zAxis * ExplorationSettingsManager.flySpeed) + (transform.forward.x * xAxis * ExplorationSettingsManager.flySpeed)
                );
        }
        else
        {
            newPos = new Vector3(
                oldPos.x - (transform.forward.x * zAxis * ExplorationSettingsManager.flySpeed) - ((transform.forward.z) * xAxis * ExplorationSettingsManager.flySpeed),
                oldPos.y - ((transform.forward.z) * zAxis * ExplorationSettingsManager.flySpeed) + (transform.forward.x * xAxis * ExplorationSettingsManager.flySpeed),
                oldPos.z + (yAxis * ExplorationSettingsManager.flySpeed) + (transform.forward.y * zAxis * ExplorationSettingsManager.flySpeed)
                );
        }        
    }

    private void UpdateScaleNonVR()
    {
        if (Input.GetKey(KeyCode.E))        //increase scale
        {
            oldScale = newScale;
            newScale = oldScale * ((MandelbrotSettingsManager.zoomSpeed - 1) * scaleMultiplier + 1);
        }
        else if (Input.GetKey(KeyCode.Q))   //decrease scale
        {
            oldScale = newScale;
            newScale = oldScale / ((MandelbrotSettingsManager.zoomSpeed - 1) * scaleMultiplier + 1);
        }
    }

    public void SetLocomotionActive(bool active)
    {
        ResetLocomotion();
        locomotionActive = active;

        if (!Globals.vrMode)
        {
            Cursor.visible = !active;

            if (active)
            {
                NonVRGlobals.overlayAnimator.SetTrigger("Forward");
                Cursor.lockState = CursorLockMode.Locked;
            }
            else
            {
                NonVRGlobals.overlayAnimator.SetTrigger("Reverse");
                Cursor.lockState = CursorLockMode.None;
            }
        }
    }

    public void ToggleLocomotionActive()
    {
        SetLocomotionActive(!locomotionActive);
    }

    public void ResetLocomotion()
    {
        oldPos = newPos = meshTransform.localPosition;
        oldScale = newScale = meshTransform.localScale.x;

        posVel = scaleVel = Vector3.zero;
    }

    /// <summary>
    /// Resets camera rotation smoothly over a short period of time. This should only be called when locomotion is inactive.
    /// </summary>
    /// <returns></returns>
    public IEnumerator ResetCamera()
    {
        rotX = rotY = 0;

        while (true)
        {
            Vector3 localEulers = transform.localEulerAngles;

            float x = Mathf.SmoothDampAngle(localEulers.x, 0, ref camXRefVel, camTransitionTime);
            float y = Mathf.SmoothDampAngle(localEulers.y, 0, ref camYRefVel, camTransitionTime);
            float z = Mathf.SmoothDampAngle(localEulers.z, 0, ref camZRefVel, camTransitionTime);

            transform.localEulerAngles = new Vector3(x, y, z);

            if (Mathf.Abs(x) <= camTransitionThreshold &&
                Mathf.Abs(y) <= camTransitionThreshold &&
                Mathf.Abs(z) <= camTransitionThreshold)
            {
                break;
            }
            else
            {
                yield return null;
            }
        }
    }
}
