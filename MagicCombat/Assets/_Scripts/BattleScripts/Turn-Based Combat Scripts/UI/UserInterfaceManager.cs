using System;
using System.Collections.Generic;
using System.Linq;
using TurnBased.UI;
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

    [Header("Selected User Interface Properties")]
    [SerializeField] SelectedUserInterfaceElementProperties selectedUserInterfaceElement;


    [Header("Task bar Properties")]
    [SerializeField, Tooltip("REQUIRED FIELD: SLOT IN POPULATED SCRIPTABLE OBJECT!!")] UICollection_SO UI_PrefabData;
    [SerializeField] TaskbarPrefabData taskBarPrefabData;
    [SerializeField] GameObject ScreenElementsTransform;
    UITaskBarManager TaskBarManager;


    [Header("Cursor Properties")]
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

        if (!GameObject.Find("EventSystem")) {
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }
    }

    private void InitaliseCursorManager()
    {
        this.selectedUserInterfaceElement = new();
        this.CursorManager = new CursorManager(this.transform, this.UI_PrefabData.CursorPrefab, this.CursorImages);
        this.selectedUserInterfaceElement.OnChangeCursorState += this.CursorManager.SetCursorImageState;
    }

    private void InitaliseTaskBarManager()
    {
        if (taskBarPrefabData != null)
        {
            this.TaskBarManager = new(
                this.taskBarPrefabData.GetTaskbarHomeBoxPivotTransform(),
                this.taskBarPrefabData.GetOS_StartButtonPivotTransform(),
                this.taskBarPrefabData.GetWindowGridTransform(),
                (RectTransform)this.ScreenElementsTransform.transform,
                UI_PrefabData
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

    private void PerformGraphicRaycastForSelectedObjects()
    {
        /*  If something is already selected in the UI, we don't want to perform any checks. I.e. if we are resizing something, we don't want to try selecting something else.  */
        if (this.selectedUserInterfaceElement.isSelected){  return; }

        /*  Find the mouse position regardless of resolution and find what we are pointing at. Get the last result. */
        List<RaycastResult> rayRes = new();
        PointerEventData pointerData = new(EventSystem.current)
        {
            position = Input.mousePosition
        };

        /*  Raycast out.    */
        this.raycaster.Raycast(pointerData, rayRes);

        /*  Loop through all results, if it inherits from IUISelectable, add it to the list. We only want to take the first result. */
        List<IUISelectable> selectableUIElements = new();
        foreach (var r in rayRes)
        {
            if(r.gameObject.TryGetComponent(out IUISelectable selectedUI))
            {
                selectableUIElements.Add(selectedUI);
            }
        }
        if(selectableUIElements != null && selectableUIElements.FirstOrDefault() != null)
        {
            this.selectedUserInterfaceElement.hoveredUIObject = selectableUIElements.FirstOrDefault();        
        }
    }

    void ProcessCursorUISelection()
    {
        /*  Do not do any unnessessary checks if the cursor hasn't moved.   */
        if (this.CursorManager.HasCursorMoved(Input.mousePosition))
        {
            PerformGraphicRaycastForSelectedObjects();
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
                PerformGraphicRaycastForSelectedObjects();
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
        this.TaskBarManager?.Update();
    }

    public UITaskBarManager GetTaskBarManager() { return TaskBarManager; }
}
