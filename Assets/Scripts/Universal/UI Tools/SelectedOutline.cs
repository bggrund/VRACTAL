using UnityEngine;

public class SelectedOutline : MonoBehaviour {

    public OutlineData outlineData;

    [SerializeField] private Canvas canvas;
    [SerializeField] private float slideTime;

	// Use this for initialization
	void Start ()
    {
        outlineData = new OutlineData(transform, canvas, slideTime);
    }
    
	// Update is called once per frame
	void Update ()
    {
        outlineData.Update();
    }
}
