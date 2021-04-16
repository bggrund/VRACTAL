using UnityEngine;


// TODO: Need to clean this up and document it
public class OutlineData
{
    public GameObject selectedItem;
    public Vector3 newPosition;
    public bool settingNewPos;
    private float slideTime;
    public Transform viewportTransform;
    public Canvas canvas;
    private Transform outlineTransform;
    private Vector3 velocity;

    public OutlineData(Transform objTransform, Canvas canvas, float slideTime)
    {
        selectedItem = null;
        this.outlineTransform = objTransform;
        viewportTransform = objTransform.parent;
        this.canvas = canvas;
        this.slideTime = slideTime;
        newPosition = this.outlineTransform.localPosition = Vector3.zero;
        settingNewPos = false;
        velocity = Vector3.zero;
    }

    public void Update()
    {
        if (settingNewPos)
        {
            outlineTransform.localPosition = Vector3.SmoothDamp(outlineTransform.localPosition, newPosition, ref velocity, slideTime);
            if ((outlineTransform.localPosition - newPosition).magnitude <= 0.05f)
            {
                if (selectedItem != null)
                {
                    outlineTransform.SetParent(selectedItem.transform);
                }
                if(outlineTransform.gameObject.GetComponent<HighlightedOutline>() != null)
                {
                    outlineTransform.SetAsFirstSibling();
                }
                outlineTransform.localPosition = newPosition = Vector3.zero;
                settingNewPos = false;
            }
        }
    }

    public void SlideTo(GameObject selectedObj)
    {
        selectedItem = selectedObj;

        float newYPositionInsideViewport = selectedObj.transform.localPosition.y + selectedObj.transform.parent.localPosition.y;

        newPosition = new Vector3(
            outlineTransform.localPosition.x,
            newYPositionInsideViewport,
            outlineTransform.localPosition.z);
        settingNewPos = true;
        outlineTransform.SetParent(viewportTransform, true);
    }
}
