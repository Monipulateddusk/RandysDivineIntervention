using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler))]
[RequireComponent (typeof(GraphicRaycaster))]
public class UserInterfaceManager : MonoBehaviour
{
    /*  Manager Components and Children.    */
    [Header("Components")]
    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasScaler scaler;
    private GraphicRaycaster raycaster;
    private GameObject eventSystem;

    [Header("User Interface Features")]

    public bool test;

    private void InitialiseComponents()
    {
        this.rectTransform = GetComponent<RectTransform>();
        this.canvas = GetComponent<Canvas>();
        this.canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        this.scaler = GetComponent<CanvasScaler>();
        this.scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        this.scaler.referenceResolution = new Vector2(1920.0f, 1080.0f);
        this.raycaster = GetComponent<GraphicRaycaster>();

        if (this.eventSystem == null && !GameObject.Find("EventSystem")) {
            eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
        }
    }
    private void OnValidate()
    {
        InitialiseComponents();
    }

    private void Awake()
    {
        InitialiseComponents();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
