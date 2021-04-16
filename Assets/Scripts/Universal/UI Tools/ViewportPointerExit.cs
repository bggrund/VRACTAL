using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ViewportPointerExit : MonoBehaviour, IPointerExitHandler {

    [SerializeField] private HighlightedOutline highlightedOutline;
    [SerializeField] private SelectedOutline selectedOutline;

    private RectTransform positionTester;

	// Use this for initialization
	void Start () {
        positionTester = (new GameObject("", typeof(RectTransform))).GetComponent<RectTransform>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void OnPointerExit(PointerEventData e)
    {
        RectTransform r = (RectTransform)transform;

        positionTester.position = e.pointerCurrentRaycast.worldPosition;

        if (r.rect.Contains(new Vector3(positionTester.anchoredPosition.x, -positionTester.anchoredPosition.y)))
        {
            return;
        }
        
        SelectableColorspace.lastSelected = null;

        if (selectedOutline.outlineData.selectedItem != null)
        {
            highlightedOutline.outlineData.SlideTo(selectedOutline.outlineData.selectedItem);
        }
    }
}
