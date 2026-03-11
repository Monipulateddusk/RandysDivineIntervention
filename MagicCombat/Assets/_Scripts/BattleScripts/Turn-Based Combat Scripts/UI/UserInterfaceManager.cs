using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler))]
[RequireComponent(typeof(GraphicRaycaster))]
public class UserInterfaceManager : MonoBehaviour
{
    private static UserInterfaceManager instance;
    public static UserInterfaceManager Instance
    {
        get
        {
            if (instance == null)
                Debug.Log("UserInterfaceManager is NULL");
            return instance;
        }
    }

    [Serializable]class SelectedUserInterfaceElementProperties
    {
        [SerializeField]public IUISelectable hoveredUIObject;
        [SerializeField]public bool isSelected;

        [SerializeField]private CursorManager.CursorIcons cursorState;
        public event Action<CursorManager.CursorIcons> OnChangeCursorState;

        public void SetCursorState(CursorManager.CursorIcons newCursorState)
        {
            this.cursorState = newCursorState;
            OnChangeCursorState?.Invoke(this.cursorState);
        }
    }

    /*  Manager Components and Children.    */
    [Header("Components")]
    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasScaler scaler;
    private GraphicRaycaster raycaster;
    private EventSystem eventSystem;


    [Header("Selected User Interface Properties")]
    [SerializeField] SelectedUserInterfaceElementProperties selectedUserInterfaceElement;


    [Header("Task bar Properties")]
    [SerializeField] GameObject dialogueBoxPrefab;
    [SerializeField] GameObject windowMinimisationPrefab;
    [SerializeField] GameObject taskBarObject;
    [SerializeField] GameObject ScreenElementsTransform;
    UITaskBarManager TaskBarManager;


    [Header("Cursor Properties")]
    [SerializeField, Tooltip("Required Field. Populate with the Prefab of the Cursor")]                     GameObject CursorPrefab;
    [SerializeField, Tooltip("Required Field. Populate with a referance to the Cursor Image Spritesheet.")] Texture2D CursorImages;
    CursorManager CursorManager;

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
            eventSystem = new GameObject("EventSystem").AddComponent<EventSystem>();
            eventSystem.gameObject.AddComponent<StandaloneInputModule>();
        }
    }

    private void InitaliseCursorManager()
    {
        this.selectedUserInterfaceElement = new();
        this.CursorManager = new CursorManager(this.transform, this.CursorPrefab, this.CursorImages);
        this.selectedUserInterfaceElement.OnChangeCursorState += this.CursorManager.SetCursorImageState;
    }

    private void InitaliseTaskBarManager()
    {
        if (taskBarObject != null)
        {
            this.TaskBarManager = new(
                (RectTransform)this.taskBarObject.transform.Find("TaskBarHomeBox").transform,
                (RectTransform)this.taskBarObject.transform.Find("Object_Elements").Find("WindowGrid").transform,
                (RectTransform)this.ScreenElementsTransform.transform,
                windowMinimisationPrefab,
                dialogueBoxPrefab
                );
        }
    }
    private void OnValidate()
    {
        InitialiseComponents();
        InitaliseCursorManager();
    }

    private void InitaliseSingleton()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            DestroyImmediate(this);
        }
    }

    private void Awake()
    {
        InitaliseSingleton();
        InitialiseComponents();
        InitaliseCursorManager();
        InitaliseTaskBarManager();
    }

    void ClearSelectedUIElement()
    {
        this.selectedUserInterfaceElement.hoveredUIObject?.OnDeselect(this.CursorManager.GetPreviousMousePosition());
        this.selectedUserInterfaceElement.hoveredUIObject = null;
        this.selectedUserInterfaceElement.isSelected = false;
        this.selectedUserInterfaceElement.SetCursorState(CursorManager.CursorIcons.Cursor);
    }

    private void HandleRaycastUISelection()
    {
        /*  Throw out a raycast from the camera to the point where the cursor is at scanning for UI elements. */
        Collider2D hit = Physics2D.OverlapPoint(Input.mousePosition, LayerMask.GetMask("UI"));

        /*  
         *  If we got something that implements IUISelectable, save that locally. 
         *  If we didn't hit something with the raycast, we should deselect anything we could have been selecting before. 
         */
        if (hit != null && hit.TryGetComponent(out IUISelectable selectedUI))
        {
            this.selectedUserInterfaceElement.hoveredUIObject = selectedUI;
        }
        /*  We only want to clear the selected UI IF it isn't selected. Something can be selected and not under the mouse via Dragging while holding down the click. */
        else if (!this.selectedUserInterfaceElement.isSelected)
        {
            ClearSelectedUIElement();
        }
    }

    void ProcessCursorUISelection()
    {
        /*  Do not do any unnessessary checks if the cursor hasn't moved.   */
        if (this.CursorManager.HasCursorMoved(Input.mousePosition))
        {
            HandleRaycastUISelection();
        }

        if (this.selectedUserInterfaceElement.hoveredUIObject != null)
        {
            /*  If we are selecting a DialogueBox, we want to pull it to the front. */
            if (selectedUserInterfaceElement.hoveredUIObject is DialogueBoxBehaviour && selectedUserInterfaceElement.hoveredUIObject as DialogueBoxBehaviour != null)
            {
                ((DialogueBoxBehaviour)this.selectedUserInterfaceElement.hoveredUIObject).transform.SetAsLastSibling();
            }

            CursorManager.CursorIcons enm = this.selectedUserInterfaceElement.hoveredUIObject.GetCurrentMouseStateSuggestion();

            this.selectedUserInterfaceElement.SetCursorState(enm);

            /*  If we have something valid from the Raycast and it isn't selected, we are hovering. Otherwise, we are dragging.*/
            if (this.selectedUserInterfaceElement.isSelected)
            {
                this.selectedUserInterfaceElement.hoveredUIObject?.OnDrag(Input.mousePosition);
            }
            else
            {
                this.selectedUserInterfaceElement.hoveredUIObject?.OnHover(Input.mousePosition);
            }

            /*  Irregardless of if we are hovering or dragging, we want to handle the input buttons independantly. */
            if (Input.GetMouseButtonUp(0) && this.selectedUserInterfaceElement.isSelected)
            {
                ClearSelectedUIElement();
                HandleRaycastUISelection();
            }

            if (Input.GetMouseButtonDown(0) && !this.selectedUserInterfaceElement.isSelected)
            {


                this.selectedUserInterfaceElement.isSelected = true;
                this.selectedUserInterfaceElement.hoveredUIObject?.OnSelect(Input.mousePosition);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        ProcessCursorUISelection();

        this.CursorManager.Update();
        this.TaskBarManager.Update();
    }


    public UITaskBarManager GetTaskBarManager() { return TaskBarManager; }
}
