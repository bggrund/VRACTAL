using UnityEngine;
using VRTK;

public class RotatorManager : MonoBehaviour
{
    public GameObject rotatorObject;

    [SerializeField] private float rotationMultiplier = 180;
    [SerializeField] private float dampTime = 0.1f;
    
    private VRTK_ControllerEvents events;

    private float currentX, lastX;
    private Vector3 rotVel = Vector3.zero;
    private Vector3 newEulers, oldEulers, dampedEulers;
    private bool touchpadTouched = false;

    private void Start()
    {
        events = VRGlobals.leftControllerEvents;

        dampedEulers = rotatorObject.transform.localRotation.eulerAngles;

        events.TouchpadTouchStart += LeftControllerEvents_TouchpadTouchStart;
        events.TouchpadTouchEnd += LeftControllerEvents_TouchpadTouchEnd;
        events.ButtonTwoPressed += LeftControllerEvents_StartMenuPressed;
    }

    private void LeftControllerEvents_StartMenuPressed(object sender, ControllerInteractionEventArgs e)
    {
        GameController.Instance.ToggleExploration();
    }

    private void LeftControllerEvents_TouchpadTouchStart(object sender, VRTK.ControllerInteractionEventArgs e)
    {
        if (!Globals.explorationMode)
        {
            return;
        }
        
        currentX = lastX = e.touchpadAxis.x;
        newEulers = oldEulers = dampedEulers;
        touchpadTouched = true;
    }

    private void LeftControllerEvents_TouchpadTouchEnd(object sender, VRTK.ControllerInteractionEventArgs e)
    {
        if (!Globals.explorationMode)
        {
            return;
        }

        touchpadTouched = false;
    }

    private void Update()
    {
        if (!Globals.explorationMode)
        {
            return;
        }

        if (touchpadTouched && !events.touchpadPressed)
        {
            lastX = currentX;
            currentX = events.GetTouchpadAxis().x;

            oldEulers = newEulers;
            newEulers = new Vector3(oldEulers.x, oldEulers.y, oldEulers.z + (lastX - currentX) * rotationMultiplier);
        }

        if(dampedEulers - newEulers == Vector3.zero)
        {
            return;
        }

        dampedEulers = Vector3.SmoothDamp(dampedEulers, newEulers, ref rotVel, dampTime);
        rotatorObject.transform.localRotation = Quaternion.Euler(dampedEulers);
    }
}
