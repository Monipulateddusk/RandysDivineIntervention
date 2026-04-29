using System.Collections.Generic;
using TurnBased.AttackResolution;
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

        private System.Collections.Generic.List<MoveUIPrefabData>   InstanciatedMoveUIElements = new();
        private System.Collections.Generic.List<TargetUIPrefabData> InstanciatedTargetUIElements = new();

        private SummaryInspectionUIBehaviourStates currentState = SummaryInspectionUIBehaviourStates.UnitSummary;

        private void Awake()
        {
            StationSelectorManager.OnSelectionChange                += StationSelectorManager_OnSelectionChange;

            Intention.UnitIntentionManager.OnUnitIntentionChanged   += UnitIntentionManager_OnUnitIntentionChanged;
        }

        private void OnDestroy()
        {
            StationSelectorManager.OnSelectionChange                -= StationSelectorManager_OnSelectionChange;

            Intention.UnitIntentionManager.OnUnitIntentionChanged   -= UnitIntentionManager_OnUnitIntentionChanged;

        }

        private void Start()
        {
            InitaliseSummarySelectionUI();
        }

        private void InitaliseSummarySelectionUI()
        {
            /*  Set the current selected State to be Summary so we go to the summary of the Unit we select onto.    */
            this.currentState = SummaryInspectionUIBehaviourStates.UnitSummary;

            /*  Initalise the Health UI with the current selected UnitIndex.    */
            StationIndex selectedStationIndex = StationSelectorManager.Instance.GetSelectedStationIndex();
            if (!StationManager.Instance.TryGetUnitIndexOnStation(selectedStationIndex, out UnitIndex unitIndex)) { return; }

            this.healthDisplayUI.Initalise(unitIndex);

            /*  Once everything is Initalised, Update the state so it is visualised.    */
            UpdateBehaviourState();
        }


        private void UpdateBehaviourState()
        {
            StationIndex selectedStationIndex = StationSelectorManager.Instance.GetSelectedStationIndex();
            if (!StationManager.Instance.TryGetUnitIndexOnStation(selectedStationIndex, out UnitIndex unitIndex)) { return; }

            switch (this.currentState)
            {
                default:
                case SummaryInspectionUIBehaviourStates.UnitSummary:
                    VisualiseUnitSummary(unitIndex);
                    break;
                case SummaryInspectionUIBehaviourStates.MoveSelection:

                    break;
                case SummaryInspectionUIBehaviourStates.TargetSelection:

                    break;
            }
        }

        private void VisualiseUnitSummary(UnitIndex selectedUnitIndex)
        {
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
            /*  Retrieve the current intention state of the Unit.   */
            if (!Intention.UnitIntentionManager.Instance.TryGetIntention(selectedUnitIndex, out Intention.UnitIntention intention)) { return; }

            if (intention.ResolutionState != UnitIntentionResolutionState.COMPLETED_INTENTION)
            {
                this.UnitIntentionText.text = $"{unitData.name} is Twiddling their Metaphysical thumbs.";
            }
            else
            {
                SetSummaryIntentionTextWithIntention(selectedUnitIndex, unitData, intention);
            }
        }

        private void SetSummaryIntentionTextWithIntention(UnitIndex selectedUnitIndex, UnitData unitData, Intention.UnitIntention intention)
        {         
            this.UnitIntentionText.text = CombatDamageUtility.GetUnitIntentionIntentionString(selectedUnitIndex, unitData, intention);

            return;
        }


        private void VisualiseSelectedUnitOnStart()
        {
            StationIndex currentlySelectedStation = StationSelectorManager.Instance.GetSelectedStationIndex();

            if (!StationManager.Instance.TryGetUnitIndexOnStation(currentlySelectedStation, out UnitIndex unitIndex)) { return; }

            if (!Intention.UnitIntentionManager.Instance.TryGetIntention(unitIndex, out var intention)) { return; }

            SetStateBasedOnUnitIntention(intention);
        }

        private void StationSelectorManager_OnSelectionChange(StationIndex selectedStationIndex, StationIndex? deselectedStationIndex)
        {

            if (!StationManager.Instance.TryGetUnitIndexOnStation(selectedStationIndex, out UnitIndex unitIndex)) { return; }

            if (!Intention.UnitIntentionManager.Instance.TryGetIntention(unitIndex, out var intention)) {  return; }

            SetStateBasedOnUnitIntention(intention);
        }

        private void UnitIntentionManager_OnUnitIntentionChanged(UnitIndex unitIndex, Intention.UnitIntention intentionOfTheUnitIndex)
        {
            Debug.LogError($"UnitIntentionChanged. Getting station index of the unit index");

            if (!StationManager.Instance.TryGetStationIndexOfIndex(unitIndex, out StationIndex stationIndexOfUnitIndex)) { return; }
            
            Debug.LogError($"Got station index of the unit index who's intent changed. Is it the same as the stationIndex?  ");
            Debug.LogError($"This intention changed unit index station index is: StationIndex: {stationIndexOfUnitIndex.Index} Selected station index is: {StationSelectorManager.Instance.GetSelectedStationIndex().Index}  ");
            /*  
             *  Only update this UI element if the intention belonged to the selected Unit. 
             *  Basically, if an enemy aren't selected changes it's intention, we don't want to do anything as we aren't displaying that unit.  
             */
            if (stationIndexOfUnitIndex.Index != StationSelectorManager.Instance.GetSelectedStationIndex().Index) { return; }

            Debug.LogError($"On Unit Selection Changed UI fired. ResolutionState: {intentionOfTheUnitIndex.ResolutionState.ToString()}");


            SetStateBasedOnUnitIntention(intentionOfTheUnitIndex);

            //SetStateIfAwaitingUserInput(unitIndex);
        }

        private void SetStateBasedOnUnitIntention(Intention.UnitIntention intentionOfTheUnitIndex)
        {
            switch (intentionOfTheUnitIndex.ResolutionState)
            {
                case UnitIntentionResolutionState.AWAITING_MOVE_SELECTION:       
                    SetState(SummaryInspectionUIBehaviourStates.MoveSelection);
                    break;

                case UnitIntentionResolutionState.AWAITING_TARGET_SELECTION:
                    SetState(SummaryInspectionUIBehaviourStates.TargetSelection);
                    break;

                default:
                case UnitIntentionResolutionState.NONE:
                case UnitIntentionResolutionState.COMPLETED_INTENTION:
                    SetState(SummaryInspectionUIBehaviourStates.UnitSummary);
                    break;
            }
        }

        private void SetState(SummaryInspectionUIBehaviourStates nextState)
        {
            this.currentState = nextState;
          //  UpdateState();
        }


        //    private void UpdateState()
        //    {
        //        DisableAllWindows();

        //        StationIndex selectedStationIndex = StationSelectorManager.Instance.GetSelectedStationIndex();
        //        if (!StationManager.Instance.TryGetUnitIndexOnStation(selectedStationIndex, out UnitIndex unitIndex)) { Debug.Log("Invalid selection change!"); return; }
        //        if (!StationManager.Instance.TryGetBattleUnitOfIndex(unitIndex, out BaseBattleUnit bBU)) { Debug.Log("Invalid selection change!"); return; }


        //        switch (this.currentState)
        //        {
        //            case SummaryInspectionUIBehaviourStates.MoveSelection:
        //                VisualiseForMoveSelection(unitIndex, bBU);
        //                break;

        //            case SummaryInspectionUIBehaviourStates.TargetSelection:
        //                VisualiseForTargetSelection(unitIndex, bBU);
        //                break;

        //            case SummaryInspectionUIBehaviourStates.UnitSummary:
        //            default:      
        //                VisualiseForIntention(unitIndex, bBU);
        //                break;

        //        }
        //    }

        //    private void VisualiseForMoveSelection(UnitIndex unitIndex, BaseBattleUnit bBU)
        //    {
        //        /*  Enable the Inspection for the Health and Status as well as the Unit's Moves.    */
        //        this.InspectionGameObject.SetActive(true);
        //        this.UnitImageHealthWrapperGameObject.SetActive(true);

        //        SetImage(bBU);
        //        SetHealthValues(unitIndex);
        //        SetMoves(bBU);
        //    }

        //    private void VisualiseForTargetSelection(UnitIndex unitIndex, BaseBattleUnit bBU)
        //    {
        //        Debug.LogWarning("Target Selection Enable");

        //        /*  Enable the Keep the inspection window open to show the selected Move but also show the possible targets for the move.   */
        //        this.InspectionGameObject.SetActive(true);
        //        this.TargetSelectionGameObject.SetActive(true);

        //        SetMoves(bBU);

        //        /*  Get the currently selected Unit's intention to visualise it.    */
        //        if (!Intention.UnitIntentionManager.Instance.TryGetIntention(unitIndex, out Intention.UnitIntention intention)) { return; }
        //        TargettingSelectorInfo selectorInfo = StationManagerUtilities.FindAllPossibleTargettingStationIndexesOfTargettingType(unitIndex, intention.MoveSelection.GetMoveTargetType());
        //        SetTargets(selectorInfo);
        //        SetMovesSelectedState(intention.MoveSelection);
        //    }

        //    private void VisualiseForIntention(UnitIndex unitIndex, BaseBattleUnit bBU)
        //    {
        //        if(!Intention.UnitIntentionManager.Instance.TryGetIntention(unitIndex, out Intention.UnitIntention intention)) { return; }

        //        /*  Enable the Inspection for the Health and Status as well as the Unit's Moves.    */
        //        this.InspectionGameObject.SetActive(true);
        //        this.UnitImageHealthWrapperGameObject.SetActive(true);

        //        SetImage(bBU);
        //        SetHealthValues(unitIndex);
        //    }

        //    private void SetImage(BaseBattleUnit battleUnit)
        //    {
        //        if (this.UnitImage == null) { return; }
        //        this.UnitImage.sprite = battleUnit.GetBaseUnit().sprite;
        //        this.UnitImage.color = battleUnit.GetBaseUnit().color;
        //    }

        //    private void SetHealthValues(UnitIndex unitIndex)
        //    {
        //        if(this.healthDisplayUI != null)
        //        {
        //            this.healthDisplayUI.Initalise(unitIndex);
        //        }
        //    }

        //    #region Move UI Methods

        //    private void SetMoves(BaseBattleUnit battleUnit)
        //    {
        //        DestroyMoveUIElements();

        //        foreach (IBattleMove moveAction in battleUnit.GetBaseUnit().moves)
        //        {
        //            CreateMoveUIElement(moveAction);
        //        }
        //    }

        //    private void SetMovesSelectedState(IBattleMove selectedMove)
        //    {
        //        Debug.LogWarning("Setting moves for selected move state");

        //        foreach (MoveUIPrefabData moveButtonData in this.InstanciatedMoveUIElements)
        //        {
        //            if (moveButtonData.GetCorrelatingMove() == selectedMove)
        //            {
        //                moveButtonData.LockButtonClickedStatus(true);
        //                continue;
        //            }
        //            moveButtonData.LockButtonClickedStatus(false); 
        //        }
        //    }

        //    private void CreateMoveUIElement(IBattleMove move)
        //    {
        //        if (this.InstanciatedMoveUIElements == null || this.PopupBufferGameObject == null || this.UnitImageHealthWrapperGameObject == null) { return; }

        //        GameObject instanciatedObject = GameObject.Instantiate(this.MoveUIPrefab, this.CommandWrapperGameObject.transform);
        //        if (instanciatedObject != null && instanciatedObject.TryGetComponent(out MoveUIPrefabData instanciatedMoveUIData))
        //        {
        //            instanciatedMoveUIData.Initalise(move);
        //            instanciatedMoveUIData.OnButtonClicked += OnMoveButtonClick;
        //            this.InstanciatedMoveUIElements.Add(instanciatedMoveUIData);
        //        }
        //    }

        //    private void DestroyMoveUIElements()
        //    {
        //        if (this.InstanciatedMoveUIElements == null) { return; }

        //        foreach (MoveUIPrefabData obj in this.InstanciatedMoveUIElements)
        //        {
        //            obj.OnButtonClicked -= OnMoveButtonClick;

        //            Destroy(obj.gameObject);
        //        }
        //        this.InstanciatedMoveUIElements.Clear();
        //    }

        //    private void OnMoveButtonClick(MoveUIPrefabData buttonObject, bool isPressed)
        //    {
        //        /*  Unclick all buttons when this is clicked.   */
        //        foreach (MoveUIPrefabData moveButtonData in this.InstanciatedMoveUIElements)
        //        {
        //            if (moveButtonData == buttonObject) { continue; }

        //            moveButtonData.IsButtonClicked = false;
        //        }

        //        /*  If the selectedUnitIndex is the one we are processing, then continue   */
        //        if (!StationSelectorManager.Instance.TryGetUnitIndexOfSelectedStation(out UnitIndex selectedUnitIndex)) { return; }

        //        Debug.LogError($"Move button clicked where selected unit is: {selectedUnitIndex.Index} " +
        //            $"and the current UI UserInput selected Unit is: {UserInterfaceUserInput.Instance.GetSelectedUnit()?.Index}");

        //        if (UserInterfaceUserInput.Instance.GetSelectedUnit()?.Index != selectedUnitIndex.Index) { return; }

        //        UserInterfaceUserInput.Instance.OnMoveSelection(buttonObject.GetCorrelatingMove());
        //    }

        //    #endregion

        //        #region Target UI Methods

        //    private void SetTargets(TargettingSelectorInfo targettingSelectorInfo)
        //    {
        //        DestroyTargetUIElements();

        //        /*  
        //         *  The Selector Info says if we need to select from among the targets or if they are amalgamated into one option.
        //         *  I.e. if the selected move can only target everyone on the field, there is no point making multiple Target buttons for each target if we are hitting all of them.
        //         */

        //        if (!targettingSelectorInfo.DoesRequireTargettingSelectorSelection)
        //        {
        //            CreateTargetUIElement(targettingSelectorInfo.PossibleTargets, targettingSelectorInfo.TargettingDisplayText);
        //            return;
        //        }

        //        foreach (StationIndex possibleTargetStation in targettingSelectorInfo.PossibleTargets)
        //        {
        //            CreateTargetUIElement(possibleTargetStation);
        //        }          
        //    }

        //    private void CreateTargetUIElement(StationIndex targetedStationIndex)
        //    {
        //        if (this.InstanciatedTargetUIElements == null || this.PopupBufferGameObject == null || this.UnitImageHealthWrapperGameObject == null) { return; }

        //        GameObject instanciatedObject = GameObject.Instantiate(this.TargetUIPrefab, this.TargetSelectionGameObject.transform);
        //        if (instanciatedObject != null && instanciatedObject.TryGetComponent(out TargetUIPrefabData instanciatedTargetUIData))
        //        {
        //            instanciatedTargetUIData.Initalise(targetedStationIndex);
        //            instanciatedTargetUIData.OnButtonClicked += OnTargetButtonClick;
        //            this.InstanciatedTargetUIElements.Add(instanciatedTargetUIData);
        //        }
        //    }

        //    private void CreateTargetUIElement(System.Collections.Generic.List<StationIndex> targetStations, string targetText)
        //    {
        //        if (this.InstanciatedTargetUIElements == null || this.PopupBufferGameObject == null || this.UnitImageHealthWrapperGameObject == null) { return; }
        //        GameObject instanciatedObject = GameObject.Instantiate(this.TargetSelectionGameObject, this.CommandWrapperGameObject.transform);
        //        if (instanciatedObject != null && instanciatedObject.TryGetComponent(out TargetUIPrefabData instanciatedTargetUIData))
        //        {
        //            instanciatedTargetUIData.Initalise(targetStations, targetText);
        //            instanciatedTargetUIData.OnButtonClicked += OnTargetButtonClick;
        //            this.InstanciatedTargetUIElements.Add(instanciatedTargetUIData);
        //        }
        //    }

        //    private void OnTargetButtonClick(TargetUIPrefabData buttonObject, bool isPressed)
        //    {
        //        /*  Unclick all buttons when this is clicked.   */
        //        foreach (TargetUIPrefabData targetButtonData in this.InstanciatedTargetUIElements)
        //        {
        //            if (targetButtonData == buttonObject)
        //            {
        //                continue;
        //            }

        //            targetButtonData.IsButtonClicked = false;
        //        }

        //        /*  If the selectedUnitIndex is the one we are processing, then continue   */
        //        if (!StationSelectorManager.Instance.TryGetUnitIndexOfSelectedStation(out UnitIndex selectedUnitIndex)) { return; }
        //        if (UserInterfaceUserInput.Instance.GetSelectedUnit()?.Index != selectedUnitIndex.Index) { return; }

        //        UserInterfaceUserInput.Instance.OnTargetSelection(buttonObject.GetCorrelatingTarget());
        //    }
        //    private void DestroyTargetUIElements()
        //    {
        //        if (this.InstanciatedTargetUIElements == null) { return; }

        //        foreach (TargetUIPrefabData obj in this.InstanciatedTargetUIElements)
        //        {
        //            obj.OnButtonClicked -= OnTargetButtonClick;
        //            Destroy(obj.gameObject);
        //        }
        //        this.InstanciatedTargetUIElements.Clear();
        //    }



        //    #endregion
    }
}