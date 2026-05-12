using TurnBased.AttackResolution;
using TurnBased.TurnOrder;
using UnityEngine;

namespace TurnBased.UI
{
    public class SummarySelectionUIBehaviour : MonoBehaviour
    {
        [Header("Tab Variables")]
        [SerializeField, Tooltip("Assign with the 'SummaryTab'   Object in TabsBuffer")]                private UnityEngine.UI.Button SummaryTabButton;
        [SerializeField, Tooltip("Assign with the 'SelectionTab' Object in TabsBuffer")]                private UnityEngine.UI.Button SelectionTabButton;

        [Header("Summary Variables")]
        [SerializeField, Tooltip("Assign with the 'SummaryRoot' Object in Content")]                    private GameObject SummaryRootGameObject;
        [SerializeField, Tooltip("Assign with the 'Image' Object in UnitIcon")]                         private UnityEngine.UI.Image UnitImage;
        [SerializeField, Tooltip("Assign with the 'UnitIntentionText' object in UnitIntention")]        private TMPro.TextMeshProUGUI UnitIntentionText;
        [SerializeField, Tooltip("Assign with the 'UnitHealthBuffer' object in 'HealthBuffer'")]        private UnitHealthDisplayUI healthDisplayUI;
        //  Status when created       

        [Header("MoveSelection Variables")]
        [SerializeField, Tooltip("Assign with the 'MoveSelectionRoot' Object in Content")]              private GameObject MoveSelectionRootGameObject;
        [SerializeField, Tooltip("Assign with the 'Content' Object in ScrollRectMask")]                 private Transform MoveSelectionContentTransform;

        [Header("MoveSelection Variables")]
        [SerializeField, Tooltip("Assign with the 'MainTargetRoot' Object in Content")]                 private GameObject MainTargetRootGameObject;
        [SerializeField, Tooltip("Assign with the 'Content' Object in ScrollRectMask")]                 private Transform TargetSelectionContentTransform;
        [SerializeField, Tooltip("Assign with the 'Content' Object in ScrollRectMask")]                 private TMPro.TextMeshProUGUI TargetSelectionMoveReminderText;


        [Header("Prefabs")]
        [SerializeField] private GameObject MoveUIPrefab;
        [SerializeField] private GameObject TargetUIPrefab;

        private System.Collections.Generic.List<InspectionMoveUIPrefab>   InstanciatedMoveUIElements = new();
        private System.Collections.Generic.List<InspectionTargetUIPrefab> InstanciatedTargetUIElements = new();

        private SummaryInspectionUIBehaviourStates currentState = SummaryInspectionUIBehaviourStates.UnitSummary;

        private void Awake()
        {
            StationSelectorManager.OnSelectionChange                += StationSelectorManager_OnSelectionChange;
            Intention.UnitIntentionManager.OnUnitIntentionChanged   += UnitIntentionManager_OnUnitIntentionChanged;

            this.SummaryTabButton.onClick.AddListener(OnSummaryTabButtonPressed);
            this.SelectionTabButton.onClick.AddListener(OnSelectionTabButtonPressed);
        }

        private void OnDestroy()
        {
            StationSelectorManager.OnSelectionChange                -= StationSelectorManager_OnSelectionChange;
            Intention.UnitIntentionManager.OnUnitIntentionChanged   -= UnitIntentionManager_OnUnitIntentionChanged;

            this.SummaryTabButton.onClick.RemoveListener(OnSummaryTabButtonPressed);
            this.SelectionTabButton.onClick.RemoveListener(OnSelectionTabButtonPressed);

        }

        private void Start()
        {
            InitaliseSummarySelectionUI();
        }

        private void InitaliseSummarySelectionUI()
        {
            /*  Initalise the Health UI with the current selected UnitIndex.    */
            StationIndex selectedStationIndex = StationSelectorManager.Instance.GetSelectedStationIndex();
            if (!StationManager.Instance.TryGetUnitIndexOnStation(selectedStationIndex, out UnitIndex unitIndex)) { return; }

            this.healthDisplayUI.Initalise(unitIndex);

            /*  Once everything is Initalised, Update the state so it is visualised.    */
            SetState(SummaryInspectionUIBehaviourStates.UnitSummary);
        }

        private void SetState(SummaryInspectionUIBehaviourStates nextState)
        {
            this.currentState = nextState;
            UpdateBehaviourState();
        }

        private void OnSummaryTabButtonPressed()
        {
            SetState(SummaryInspectionUIBehaviourStates.UnitSummary);
        }

        private void OnSelectionTabButtonPressed()
        {
            Debug.LogError("SelectionTabButonPressed");


            /*  Get the intention of the Unit selected. Process the selection   */
            if (!StationSelectorManager.Instance.TryGetUnitIndexOfSelectedStation(out UnitIndex unitIndex)) { return; }
            if (!Intention.UnitIntentionManager.Instance.TryGetIntention(unitIndex, out Intention.UnitIntention intention)) { return; }

            switch (intention.ResolutionState)
            {
                default:
                case UnitIntentionResolutionState.NONE:
                case UnitIntentionResolutionState.COMPLETED_INTENTION:
                    this.SelectionTabButton.gameObject.SetActive(false);
                    SetState(SummaryInspectionUIBehaviourStates.UnitSummary);
                    break;
                case UnitIntentionResolutionState.AWAITING_MOVE_SELECTION:
                    this.SelectionTabButton.gameObject.SetActive(true);
                    SetState(SummaryInspectionUIBehaviourStates.MoveSelection);
                    break;
                case UnitIntentionResolutionState.AWAITING_TARGET_SELECTION:
                    this.SelectionTabButton.gameObject.SetActive(true);
                    SetState(SummaryInspectionUIBehaviourStates.TargetSelection);
                    break;
            }

        }


        private void UnitIntentionManager_OnUnitIntentionChanged(UnitIndex index, Intention.UnitIntention intention)
        {
            /*  Get the selected station index, if the Unit that has it's intention change is the same unit who we are selecting, then proceed. */
            if (!StationSelectorManager.Instance.TryGetUnitIndexOfSelectedStation(out UnitIndex selectedUnitIndex)) {  return; }
            if (selectedUnitIndex.Index != index.Index) { return; }

            switch (intention.ResolutionState)
            {
                default:
                case UnitIntentionResolutionState.NONE:
                case UnitIntentionResolutionState.COMPLETED_INTENTION:
                    this.SelectionTabButton.gameObject.SetActive(false); 
                    SetState(SummaryInspectionUIBehaviourStates.UnitSummary);
                    break;
                case UnitIntentionResolutionState.AWAITING_MOVE_SELECTION:
                    this.SelectionTabButton.gameObject.SetActive(true);
                    SetState(SummaryInspectionUIBehaviourStates.MoveSelection);
                    break;
                case UnitIntentionResolutionState.AWAITING_TARGET_SELECTION:
                    this.SelectionTabButton.gameObject.SetActive(true);
                    SetState(SummaryInspectionUIBehaviourStates.TargetSelection);
                    break;
            }
        }

        private void StationSelectorManager_OnSelectionChange(StationIndex newSelectedStation, StationIndex? oldStation)
        {
            Debug.LogError("Summary UI OnSelectionChange invoked");

            /*  When we select a new Unit, check if it is needing User Input. If not, disable switching to the SelectionTab.    */
            if (!StationManager.Instance.TryGetUnitIndexOnStation(newSelectedStation, out UnitIndex unitIndex)) { return; }

            Debug.LogError("Unit index exists on station");

            if (UserInterfaceUserInput.Instance.GetSelectedUnit() == null || 
                    (UserInterfaceUserInput.Instance.GetSelectedUnit() != null && UserInterfaceUserInput.Instance.GetSelectedUnit().Value.Index != unitIndex.Index)
                ) 
            {  
                this.SelectionTabButton.gameObject.SetActive(false);
            }
            else
            {
                this.SelectionTabButton.gameObject.SetActive(true);
            }

            Debug.LogError("Updating the health display");

            this.healthDisplayUI.UpdateUnitIndex(unitIndex);

            Debug.LogError("Done updating the health display");

            SetState(SummaryInspectionUIBehaviourStates.UnitSummary);

            Debug.LogError("Summary UI OnSelectionChange done");
        }







        private void UpdateBehaviourState()
        {
            Debug.LogError("Updaing behviour state");

            if (StationManager.Instance == null) { return; }
            StationIndex selectedStationIndex = StationSelectorManager.Instance.GetSelectedStationIndex();
            if (!StationManager.Instance.TryGetUnitIndexOnStation(selectedStationIndex, out UnitIndex unitIndex)) { return; }


            Debug.LogError("Resetting behaviour state");

            ResetBehviourState();

            Debug.LogError($"Done resetting behaviour state. Visualising the current state: {this.currentState}");

            switch (this.currentState)
            {
                default:
                case SummaryInspectionUIBehaviourStates.UnitSummary:
                    VisualiseUnitSummary(unitIndex);
                    break;
                case SummaryInspectionUIBehaviourStates.MoveSelection:
                    VisualiseUnitMoveSelection(unitIndex);
                    break;
                case SummaryInspectionUIBehaviourStates.TargetSelection:
                    VisualiseUnitTargetSelection(unitIndex);
                    break;
            }
        }

        private void ResetBehviourState()
        {
            DisableRootObjects();
            DestroyInstanciatedUIElements();
        }

        private void DisableRootObjects()
        {
            if (this.SummaryRootGameObject == null || this.MoveSelectionRootGameObject == null || this.MainTargetRootGameObject == null) { return; }

            this.SummaryRootGameObject.SetActive(false);
            this.MoveSelectionRootGameObject.SetActive(false);
            this.MainTargetRootGameObject.SetActive(false);
        }

        private void DestroyInstanciatedUIElements()
        {
            if (this.InstanciatedMoveUIElements == null || this.InstanciatedTargetUIElements == null) { return; }

            /*  Destroy all instanciated moves and target widgets.  */
            foreach (InspectionMoveUIPrefab moveUI in this.InstanciatedMoveUIElements)
            {
                moveUI.OnButtonClicked -= OnMoveButtonClick;
                GameObject.Destroy(moveUI.gameObject);
            }

            foreach (InspectionTargetUIPrefab targetUI in this.InstanciatedTargetUIElements)
            {
                targetUI.OnButtonClicked -= OnTargetButtonClick;
                GameObject.Destroy(targetUI.gameObject);
            }

            this.InstanciatedMoveUIElements.Clear();
            this.InstanciatedTargetUIElements.Clear();
        }

        private void VisualiseUnitSummary(UnitIndex selectedUnitIndex)
        {
            if (this.SummaryTabButton == null || this.SelectionTabButton == null || this.UnitImage == null) { return; } 

            this.SummaryRootGameObject.SetActive(true);

            /*  Set the Summary button to disabled and selection to allowing switching. */
            this.SummaryTabButton.interactable      = false;
            this.SelectionTabButton.interactable    = true;

            if (!StationManager.Instance.TryGetUnitDataOfUnitIndex(selectedUnitIndex, out UnitData unitData)) { return; }

            /*  Set the Unit Icon to the correct icon with colour.  */
            this.UnitImage.sprite = unitData.sprite;
            this.UnitImage.color = unitData.color;

            /*  Update the Intention Text. Depending on the intention state of this Unit, it will have multiple things to display.  */
            SetSummaryIntention(selectedUnitIndex, unitData);
        }

        private void SetSummaryIntention(UnitIndex selectedUnitIndex, UnitData unitData)
        { 
            if (this.UnitIntentionText == null) { return; }

            /*  Retrieve the current intention state of the Unit.   */
            if (!Intention.UnitIntentionManager.Instance.TryGetIntention(selectedUnitIndex, out Intention.UnitIntention intention)) { return; }

            if (intention.ResolutionState != UnitIntentionResolutionState.COMPLETED_INTENTION)
            {
                this.UnitIntentionText.text = $"{unitData.name} is Twiddling their Metaphysical thumbs.";
            }
            else
            {
                this.UnitIntentionText.text = AttackResolution.CombatDamageUtility.GetUnitIntentionIntentionString(selectedUnitIndex, unitData, intention);
            }
        }
        private void VisualiseUnitMoveSelection(UnitIndex selectedUnitIndex)
        {
            if (this.MoveSelectionRootGameObject == null || this.SummaryTabButton == null || this.SelectionTabButton == null) { return; }

            /*  Enable the MoveSelection Root.  */
            this.MoveSelectionRootGameObject.SetActive(true);

            /*  Set the Summary button to disabled and selection to allowing switching. */
            this.SummaryTabButton.interactable = true;
            this.SelectionTabButton.interactable = false;

            CreateMoveUIElement(selectedUnitIndex);
        }


        private void VisualiseUnitTargetSelection(UnitIndex selectedUnitIndex)
        {
            this.MainTargetRootGameObject.SetActive(true);

            /*  Set the Summary button to disabled and selection to allowing switching. */
            this.SummaryTabButton.interactable = true;
            this.SelectionTabButton.interactable = false;


            /*  Get the currently selected Unit's intention to visualise it.    */
            if (!Intention.UnitIntentionManager.Instance.TryGetIntention(selectedUnitIndex, out Intention.UnitIntention intention)) { return; }
            TargettingSelectorInfo selectorInfo = StationManagerUtilities.FindAllPossibleTargettingStationIndexesOfTargettingType(selectedUnitIndex, MoveTarget.SingleEnemy);


            /*  Assign the Move reminder text with the selected move's description. */
            this.TargetSelectionMoveReminderText.text = string.Empty;
            this.TargetSelectionMoveReminderText.text = $"{intention.MoveSelection.GetMoveName()} — {CombatDamageUtility.GetMoveDescription(selectedUnitIndex, intention.MoveSelection)}";

            /*  If we do not require individual targets, we amalgamate all the options to 'All Allies' or 'Area'.   */
            if (!selectorInfo.DoesRequireTargettingSelectorSelection)
            {
                CreateTargetUIElement(selectorInfo.PossibleTargets, selectorInfo.TargettingDisplayText);
                return;
            }

            /*  If we do require individual targets, Create individual UI elements for each target unit.    */
            foreach (StationIndex possibleTargetStation in selectorInfo.PossibleTargets)
            {
                CreateTargetUIElement(possibleTargetStation);
            }
        }

        #region Move UI Methods

        private void CreateMoveUIElement(UnitIndex selectedUnitIndex)
        {
            /*  Get the UnitData of the selected Unit to determine which moves need to be made. */
            if (this.MoveUIPrefab == null || this.MoveSelectionContentTransform == null) { return; }
            if (!StationManager.Instance.TryGetUnitDataOfUnitIndex(selectedUnitIndex, out UnitData unitData)) { return; }

            foreach (IBattleMove move in unitData.moves)
            {
                GameObject instanciatedObj = GameObject.Instantiate(this.MoveUIPrefab, this.MoveSelectionContentTransform);
                if (instanciatedObj != null && instanciatedObj.gameObject.TryGetComponent(out InspectionMoveUIPrefab instanciatedMove))
                {
                    instanciatedMove.Initalise(selectedUnitIndex, move);
                    instanciatedMove.OnButtonClicked += OnMoveButtonClick;
                    this.InstanciatedMoveUIElements.Add(instanciatedMove);
                }
            }
        }

        private void OnMoveButtonClick(InspectionMoveUIPrefab buttonObject, bool isPressed)
        {
            /*  Unclick all buttons when this is clicked.   */
            foreach (InspectionMoveUIPrefab moveButtonData in this.InstanciatedMoveUIElements)
            {
                if (moveButtonData == buttonObject) { continue; }

                moveButtonData.IsButtonClicked = false;
            }

            /*  If the selectedUnitIndex is the one we are processing, then continue   */
            if (!StationSelectorManager.Instance.TryGetUnitIndexOfSelectedStation(out UnitIndex selectedUnitIndex)) { return; }

            if (UserInterfaceUserInput.Instance.GetSelectedUnit()?.Index != selectedUnitIndex.Index) { return; }

            UserInterfaceUserInput.Instance.OnMoveSelection(buttonObject.GetCorrelatingMove());
        }

        #endregion

        #region Target UI Methods

        private void CreateTargetUIElement(StationIndex targetedStationIndex)
        {
            if (this.InstanciatedTargetUIElements == null) { return; }

            GameObject instanciatedObject = GameObject.Instantiate(this.TargetUIPrefab, this.TargetSelectionContentTransform);
            if (instanciatedObject != null && instanciatedObject.TryGetComponent(out InspectionTargetUIPrefab instanciatedTargetUIData))
            {
                instanciatedTargetUIData.Initalise(targetedStationIndex);
                instanciatedTargetUIData.OnButtonClicked += OnTargetButtonClick;
                this.InstanciatedTargetUIElements.Add(instanciatedTargetUIData);
            }
        }

        private void CreateTargetUIElement(System.Collections.Generic.List<StationIndex> targetStations, string targetText)
        {
            if (this.InstanciatedTargetUIElements == null) { return; }
            GameObject instanciatedObject = GameObject.Instantiate(this.TargetUIPrefab, this.TargetSelectionContentTransform);
            if (instanciatedObject != null && instanciatedObject.TryGetComponent(out InspectionTargetUIPrefab instanciatedTargetUIData))
            {
                instanciatedTargetUIData.Initalise(targetStations, targetText);
                instanciatedTargetUIData.OnButtonClicked += OnTargetButtonClick;
                this.InstanciatedTargetUIElements.Add(instanciatedTargetUIData);
            }
        }

        private void OnTargetButtonClick(InspectionTargetUIPrefab buttonObject, bool isPressed)
        {
            /*  Unclick all buttons when this is clicked.   */
            foreach (InspectionTargetUIPrefab targetButtonData in this.InstanciatedTargetUIElements)
            {
                if (targetButtonData == buttonObject)
                {
                    continue;
                }

                targetButtonData.IsButtonClicked = false;
            }

            /*  If the selectedUnitIndex is the one we are processing, then continue   */
            if (!StationSelectorManager.Instance.TryGetUnitIndexOfSelectedStation(out UnitIndex selectedUnitIndex)) { return; }
            if (UserInterfaceUserInput.Instance.GetSelectedUnit()?.Index != selectedUnitIndex.Index) { return; }

            UserInterfaceUserInput.Instance.OnTargetSelection(buttonObject.GetCorrelatingTarget());
        }

        #endregion
    }
}