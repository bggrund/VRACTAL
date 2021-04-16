using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class SelectedItemChanged : UnityEvent<GameObject> { }

public class HighlightedOutline : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler {

    [HideInInspector] public OutlineData outlineData;
    
    public SelectedItemChanged SelectedItemChanged;

    [SerializeField] private SelectedOutline selectedOutline;
    [SerializeField] private Canvas canvas;
    [SerializeField] private Transform viewport;
    [SerializeField] private float slideTime;

    // Use this for initialization
    void Start ()
    {
        SelectedItemChanged = new SelectedItemChanged();

        outlineData = new OutlineData(transform, canvas, slideTime);
    }

    // Update is called once per frame
    void Update ()
    {
        outlineData.Update();
    }

    public void OnPointerClick(PointerEventData e)
    {
        selectedOutline.outlineData.SlideTo(outlineData.selectedItem);
        SelectedItemChanged.Invoke(outlineData.selectedItem);
    }

    public void OnPointerDown(PointerEventData e)
    {

    }
    public void OnPointerUp(PointerEventData e)
    {

    }
}
