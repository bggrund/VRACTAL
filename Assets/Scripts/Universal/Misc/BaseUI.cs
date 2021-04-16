using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Multiple instances of each UI can exist and be accessible by other scripts.
/// If one instance's UI element is updated, the static property (single value accessible by all scripts) representing the updated UI element's data should be updated,
/// and all other instances' UIs should be updated to reflect the change. Methods that update the UI should also update the other instances' UIs.
/// A specific instance should never need to be referenced by other scripts. <para/>
/// UI managers should also know if the UI is an overlay so they know whether to check for mouse position or VR pointer position when handling pointer interactions.
/// </summary>
/// <typeparam name="T">The derived class type</typeparam>
public abstract class BaseUI<T> : MonoBehaviour
    where T : class
{
    protected static List<T> instances = new List<T>();

    protected bool isOverlay;

    private static bool staticsInitialized = false;

    /// <summary>
    /// Use this to initialize any static data. It will be called on Start() by the first instance.
    /// </summary>
    protected abstract void InitializeStatics();

    /// <summary>
    /// Use this to initialize any instance data. It will be called on Start() by each instance.
    /// </summary>
    protected abstract void InitializeInstance();

    /// <summary>
    /// Use this to add event listeners to the UI elements that require them. It will be called on Start() by each instance. <para/>
    /// Doing this within the script requires that this UI contains these elements and also prevents the user from needing to assign the event listeners manually on every instance from within Unity.
    /// </summary>
    protected abstract void AddEventListeners();

    /// <summary>
    /// Unity's initialization method, called by each script instance once at the start of its GameObject's instantiation. Adds the instance to the <see cref="instances"/> collection and sets <see cref="isOverlay"/> based on the Canvas's renderMode.
    /// </summary>
    public void Awake()
    {
        instances.Add(this as T);

        if(GetComponentInParent<Canvas>().renderMode == RenderMode.WorldSpace)
        {
            isOverlay = false;
        }
        else
        {
            isOverlay = true;
        }
    }

    /// <summary>
    /// Unity's initialization method, called by each instance when first enabled. Calls the abstract initialization methods listed above.
    /// </summary>
    public void Start()
    {
        if (!staticsInitialized)
        {
            InitializeStatics();
            staticsInitialized = true;
        }

        InitializeInstance();
        AddEventListeners();
    }
    
    private void OnDestroy()
    {
        instances.Remove(this as T);
    }

}