using System.Linq;
using UnityEngine;

namespace TurnBased.UI
{
    [RequireComponent(typeof(RectTransform), typeof(Canvas), typeof(UnityEngine.UI.CanvasScaler))]
    [RequireComponent(typeof(UnityEngine.UI.GraphicRaycaster))]
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

        /*  Manager Components and Children.    */
        [Header("Components")]
        private RectTransform rectTransform;
        private Canvas canvas;
        private UnityEngine.UI.CanvasScaler scaler;
        private UnityEngine.UI.GraphicRaycaster raycaster;


        [SerializeField] UnityEngine.UI.RawImage GameScreen;
        [SerializeField] Transform GameOverPanelTransform;

        private EndOfBattlePopupBehaviour instanciatedPopupBehaviour;
        public EndOfBattlePopupBehaviour PopUpBehviour
        {
            get
            {
                return instanciatedPopupBehaviour;
            }
            private set
            {
                this.instanciatedPopupBehaviour = value;    
            }
        }

        [Header("Task bar Properties")]
        [SerializeField, Tooltip("REQUIRED FIELD: SLOT IN POPULATED SCRIPTABLE OBJECT!!")] UICollection_SO UI_PrefabData;
        [SerializeField] private TaskbarPrefabData taskBarPrefabData;
        [SerializeField] GameObject ScreenElementsTransform;
        private UITaskBarManager TaskBarManager;


        [Header("Cursor Properties")]
        [SerializeField, Tooltip("Required Field. Populate with a referance to the Cursor Image Spritesheet.")] Texture2D CursorImages;
        private CursorManager CursorManager;

        private readonly UserInterfaceUserInput UserInterfaceUserInput = new();
        private readonly UserInterfaceElementSelectorManager UserInterfaceElementSelectorManager = new();
        private readonly UserInterfaceGameStateManager UserInterfaceGameStateVisualisationManager = new();

        private void InitialiseComponents()
        {
            this.rectTransform = GetComponent<RectTransform>();
            this.canvas = GetComponent<Canvas>();
            this.canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            this.scaler = GetComponent<UnityEngine.UI.CanvasScaler>();
            this.scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            this.scaler.referenceResolution = new Vector2(1920.0f, 1080.0f);
            this.raycaster = GetComponent<UnityEngine.UI.GraphicRaycaster>();

            if (!GameObject.Find("EventSystem"))
            {
                new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(UnityEngine.EventSystems.StandaloneInputModule));
            }

            // Initalise the UI Input
            this.UserInterfaceUserInput.Initalise();
        }

        private void InitaliseCursorManager()
        {
            /*  Disable the normal cursor so we can use ours.   */
            Cursor.visible = false;

            this.CursorManager = new CursorManager(this.transform, this.UI_PrefabData.CursorPrefab, this.CursorImages);
            this.UserInterfaceElementSelectorManager.Awake(this.CursorManager.SetCursorImageState);
        }

        private void InitaliseTaskBarManager()
        {
            if (this.taskBarPrefabData != null)
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

        private void InitaliseGameOverPanel()
        {
            if (this.UI_PrefabData == null || this.GameOverPanelTransform == null) { return; }  
            GameObject instanciatedPopupObject = GameObject.Instantiate(this.UI_PrefabData.EndOfBattlePopupPrefab, this.GameOverPanelTransform);

            if (instanciatedPopupObject == null) { return; }

            if (instanciatedPopupObject.TryGetComponent(out EndOfBattlePopupBehaviour behaviour))
            {
                this.PopUpBehviour = behaviour;
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
            InitaliseGameOverPanel();
            this.UserInterfaceGameStateVisualisationManager.Awake(this.GameScreen);
        }

        // Update is called once per frame
        void Update()
        {
            this.UserInterfaceElementSelectorManager.ProcessCursorUISelection(this.CursorManager, this.raycaster);
            this.UserInterfaceGameStateVisualisationManager.Update();

            this.CursorManager.Update();
            this.TaskBarManager?.Update();
        }

        public UITaskBarManager GetTaskBarManager() { return TaskBarManager; }
    }

    public class UserInterfaceElementSelectorManager
    {
        private class SelectedUserInterfaceElementProperties
        {
            [SerializeField] public IUISelectable hoveredUIObject;
            [SerializeField] public bool isSelected;

            [SerializeField] private CursorIcons cursorState;
            public event System.Action<CursorIcons> OnChangeCursorState;

            public void SetCursorState(CursorIcons newCursorState)
            {
                this.cursorState = newCursorState;
                OnChangeCursorState?.Invoke(this.cursorState);
            }
        }
        private SelectedUserInterfaceElementProperties selectedUserInterfaceElement;

        public void Awake(System.Action<CursorIcons> OnCursorIconChange)
        {
            this.selectedUserInterfaceElement = new();
            this.selectedUserInterfaceElement.OnChangeCursorState += OnCursorIconChange;
        }

        public void Update(CursorManager cursorManager, UnityEngine.UI.GraphicRaycaster raycaster)
        {
            ProcessCursorUISelection(cursorManager, raycaster);
        }

        public void ProcessCursorUISelection(CursorManager cursorManager, UnityEngine.UI.GraphicRaycaster raycaster)
        {
            /*  Do not do any unnessessary checks if the cursor hasn't moved.   */
            if (cursorManager.HasCursorMoved(Input.mousePosition))
            {
                PerformGraphicRaycastForSelectedObjects(cursorManager, raycaster);
            }

            if (this.selectedUserInterfaceElement.hoveredUIObject != null)
            {
                /*  If we are selecting a DialogueBox, we want to pull it to the front. */
                if (this.selectedUserInterfaceElement.hoveredUIObject is DialogueBoxBehaviour && this.selectedUserInterfaceElement.hoveredUIObject as DialogueBoxBehaviour != null)
                {
                    ((DialogueBoxBehaviour)this.selectedUserInterfaceElement.hoveredUIObject).transform.SetAsLastSibling();
                }

                CursorIcons enm = this.selectedUserInterfaceElement.hoveredUIObject.GetCurrentMouseStateSuggestion();

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
                    ClearSelectedUIElement(cursorManager);
                    PerformGraphicRaycastForSelectedObjects(cursorManager, raycaster);
                }

                if (Input.GetMouseButtonDown(0) && !this.selectedUserInterfaceElement.isSelected)
                {
                    this.selectedUserInterfaceElement.isSelected = true;
                    this.selectedUserInterfaceElement.hoveredUIObject?.OnSelect(Input.mousePosition);
                }
            }
        }

        private void PerformGraphicRaycastForSelectedObjects(CursorManager cursorManager, UnityEngine.UI.GraphicRaycaster raycaster)
        {
            /*  If something is already selected in the UI, we don't want to perform any checks. I.e. if we are resizing something, we don't want to try selecting something else.  */
            if (this.selectedUserInterfaceElement.isSelected) { return; }

            /*  Find the mouse position regardless of resolution and find what we are pointing at. Get the last result. */
            System.Collections.Generic.List<UnityEngine.EventSystems.RaycastResult> rayRes = new();
            UnityEngine.EventSystems.PointerEventData pointerData = new(UnityEngine.EventSystems.EventSystem.current)
            {
                position = Input.mousePosition
            };

            /*  Raycast out.    */
            raycaster.Raycast(pointerData, rayRes);

            /*  Loop through all results, if it inherits from IUISelectable, add it to the list. We only want to take the first result. */
            System.Collections.Generic.List<IUISelectable> selectableUIElements = new();
            foreach (var r in rayRes)
            {
                if (r.gameObject.TryGetComponent(out IUISelectable selectedUI))
                {
                    selectableUIElements.Add(selectedUI);
                }
            }
            if (selectableUIElements != null && selectableUIElements.FirstOrDefault() != null)
            {
                this.selectedUserInterfaceElement.hoveredUIObject = selectableUIElements.FirstOrDefault();
            }
        }

        private void ClearSelectedUIElement(CursorManager cursorManager)
        {
            this.selectedUserInterfaceElement.hoveredUIObject?.OnDeselect(cursorManager.GetPreviousMousePosition());
            this.selectedUserInterfaceElement.hoveredUIObject = null;
            this.selectedUserInterfaceElement.isSelected = false;
            this.selectedUserInterfaceElement.SetCursorState(CursorIcons.Cursor);
        }
    }

    public static class UserInterfaceUtility
    {
        public static Color COLOR_SLATEGRAY = new()
        {
            r = 0.3551086f,
            b = 0.5660378f,
            g = 0.5036566f,
            a = 1f
        };
        public static Color COLOR_BLUEVIOLET = new()
        {
            r = 0.25f,
            b = 1f,
            g = 0.3299609f,
            a = 1f
        };

        public static float GetValueNormalisation(float minimum, float maximum, float current)
        {
            return (current - minimum) / (maximum - minimum);
        }

        public static string GetMoveTargetText(IBattleMove move)
        {
            return move.GetMoveTargetType() switch
            {
                MoveTarget.SingleEnemy => "a Single Enemy",
                MoveTarget.SingleAlly => "a Single Ally",
                MoveTarget.AllEnemies => "All Enemies",
                MoveTarget.AllAllies => "All Allies",
                MoveTarget.Area => "the Entire Area",
                _ => "Itself",
            };
        }

        /// <summary>
        /// Gets the Unit Name if the intention targets a single Unit. Otherwise, it defaults to GetMoveTargetText
        /// </summary>
        /// <param name="intention"></param>
        /// <returns></returns>
        public static bool TryGetIntentionTargetText(Intention.UnitIntention intention, out string text)
        {
            text = string.Empty;

            if (intention.TargetIndexList.Count <= 0)   { return false; }
            if (intention.MoveSelection == null)        { return false; }

            MoveTarget targetType = intention.MoveSelection.GetMoveTargetType();
            if (targetType == MoveTarget.SingleEnemy  || targetType == MoveTarget.SingleAlly)
            {
                /*  Get the 0-Index of the Targetting list and return it's name.    */
                StationIndex firstStationIndex = intention.TargetIndexList[0];
                if (!StationManager.Instance.TryGetUnitDataOnStation(firstStationIndex, out UnitData unitData)) {  return false; }

                text = unitData.name;
                return true;
            }
            else
            {
                text = GetMoveTargetText(intention.MoveSelection);
                return true;
            }
        }
    }
}