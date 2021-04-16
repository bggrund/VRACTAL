using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VRTK;

public class ColorChanged : UnityEvent<Color> { }

public class ColorPickerManager : BaseUI<ColorPickerManager>
{
    #region UI Elements

    [SerializeField] private RectTransform cursor;
    [SerializeField] private RectTransform colorPickerRectTransform;
    [SerializeField] private Image colorPickerImage;

    private Texture2D colorPickerTexture;

    #endregion

    #region Private Instance Members

    private bool pointerOnImage = false;
    private bool triggerPressed = false;

    #endregion

    #region Public Static Members

    public static Color selectedColor;
    public static ColorChanged colorChanged;

    #endregion

    #region Private Static Members

    private static VRTK_ControllerEvents controllerEvents;
    private static VRTK_Pointer pointer;

    #endregion

    #region Initialization + Updates

    protected override void InitializeStatics()
    {
        colorChanged = new ColorChanged();

        selectedColor = Color.white;

        controllerEvents = VRGlobals.rightControllerEvents;
        pointer = VRGlobals.rightControllerPointer;
    }

    protected override void InitializeInstance()
    {
        colorPickerTexture = colorPickerImage.sprite.texture;
    }

    protected override void AddEventListeners()
    {
        // Pointer enter event
        EventTrigger.Entry pointerEnter = new EventTrigger.Entry();
        pointerEnter.eventID = EventTriggerType.PointerEnter;
        pointerEnter.callback.AddListener(OnPointerEnter);

        // Pointer exit event
        EventTrigger.Entry pointerExit = new EventTrigger.Entry();
        pointerExit.eventID = EventTriggerType.PointerExit;
        pointerExit.callback.AddListener(OnPointerExit);

        EventTrigger eventTrigger = gameObject.AddComponent<EventTrigger>();
        eventTrigger.triggers.Add(pointerEnter);
        eventTrigger.triggers.Add(pointerExit);
    }

    public void Update()
    {
        if (isOverlay)
        {
            OverlayUpdate();
        }
        else
        {
            WorldSpaceUpdate();
        }
    }

    /// <summary>
    /// Called every frame if this UI is an overlay
    /// </summary>
    private void OverlayUpdate()
    {
        if (!pointerOnImage)
        {
            return;
        }

        if (ColorspaceCreatorManager.addColorOnClick)
        {
            SetCursorPos();
            if (Input.GetMouseButtonDown(0))
            {
                UpdateColor();
            }
        }
        else
        {
            if (Input.GetMouseButton(0))
            {
                SetCursorPos();
                UpdateColor();
            }
        }
    }

    /// <summary>
    /// Called every frame if this UI is world-space
    /// </summary>
    private void WorldSpaceUpdate()
    {
        if (!pointerOnImage)
        {
            return;
        }

        if (ColorspaceCreatorManager.addColorOnClick)
        {
            SetCursorPos();
            if (!triggerPressed)
            {
                if (controllerEvents.triggerPressed)
                {
                    // This is the first frame of trigger pressed down while pointer on image, so set local triggerPressed to true
                    triggerPressed = true;
                    UpdateColor();
                }
            }
            else if (!controllerEvents.triggerPressed)
            {
                // This is the first frame that the trigger was detected as not pressed, since triggerPressed has been true
                triggerPressed = false;
            }
        }
        else
        {
            if (controllerEvents.triggerPressed)
            {
                SetCursorPos();
                UpdateColor();
            }
        }
    }

    #endregion

    #region Methods and Event Handlers

    /// <summary>
    /// Sets position of the colorPicker's cursor according to either mouse position or pointer position, depending on whether this UI is an overlay or not
    /// </summary>
    private void SetCursorPos()
    {
        if (isOverlay)
        {
            cursor.position = Input.mousePosition;
        }
        else
        {
            cursor.transform.position = pointer.pointerRenderer.GetDestinationHit().point;
        }
    }

    /// <summary>
    /// Finds the color of the pixel under the cursor and invokes the <see cref="colorChanged"/> event with this color as its parameter
    /// </summary>
    private void UpdateColor()
    {
        Vector2 cursorAnchoredPosition = cursor.anchoredPosition;

        float percentWidth = cursorAnchoredPosition.x / colorPickerRectTransform.rect.width;
        float percentHeight = cursorAnchoredPosition.y / colorPickerRectTransform.rect.height;

        selectedColor = colorPickerTexture.GetPixel(Mathf.RoundToInt(colorPickerTexture.width * percentWidth), Mathf.RoundToInt(colorPickerTexture.height * percentHeight));

        colorChanged.Invoke(selectedColor);
    }

    public void OnPointerEnter(BaseEventData e)
    {
        pointerOnImage = true;
    }

    public void OnPointerExit(BaseEventData e)
    {
        pointerOnImage = false;
    }

    #endregion
}
