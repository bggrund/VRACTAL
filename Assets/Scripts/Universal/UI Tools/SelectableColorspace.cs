using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SelectableColorspace : MonoBehaviour
{
    public ColorspaceData colorspaceData;

    private HighlightedOutline highlightedOutline;
    public static SelectableColorspace lastSelected;

	// Use this for initialization
	void Start ()
    {
        highlightedOutline = GetComponentInParent<ScrollRect>().GetComponentInChildren<HighlightedOutline>();
    }
	
	// Update is called once per frame
	void Update () {
		
	}
 
    public void OnPointerEnter(PointerEventData e)
    {
        if (lastSelected == this)
        {
            return;
        }

        //Debug.Log("Pointer Entered and Executed");

        lastSelected = this;

        highlightedOutline.outlineData.SlideTo(this.gameObject);
    }
}

public struct ColorspaceData
{
    public string filename;
    public Color[] colorspace;

    public ColorspaceData(string filename, Color[] colorspace)
    {
        this.filename = filename;
        this.colorspace = colorspace;
    }
}