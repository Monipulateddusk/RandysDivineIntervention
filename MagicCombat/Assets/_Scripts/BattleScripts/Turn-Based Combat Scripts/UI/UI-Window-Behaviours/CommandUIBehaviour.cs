using System.Linq;
using TurnBased.Intention;
using UnityEngine;
using UnityEngine.UI;

namespace TurnBased.UI
{
    public class CommandUIBehaviour : MonoBehaviour
    {
        [Header("Inspector Variables")]
        [SerializeField] private UnityEngine.UI.Image UnitImage;
        [SerializeField, Tooltip("Assign with the 'CommandWrapper' Object in Command")]             GameObject CommandWrapperGameObject;
        [SerializeField, Tooltip("Assign with the 'Inspection' Object in InspectionSubWindow")]     GameObject InspectionGameObject;
        [SerializeField, Tooltip("Assign with the 'PopupBuffer' Object in InspectionSubWindow")]    GameObject PopupBufferGameObject;
        [SerializeField, Tooltip("Assign with the 'UnitImageHealthWrapper' Object in VIew")]        GameObject UnitImageHealthWrapperGameObject;
        [SerializeField, Tooltip("Assign with the 'TargetSelection' Object in VIew")]               GameObject TargetSelectionGameObject;
        [SerializeField, Tooltip("Assign with the 'UnitHealthBuffer' object with the 'UnitHealthDisplayUI' script")] private UnitHealthDisplayUI healthDisplayUI;

        [Header("Prefabs")]
        [SerializeField] private GameObject MoveUIPrefab;
        [SerializeField] private GameObject TargetUIPrefab;
        [SerializeField] private GameObject IntentionTextPrefab;

        private System.Collections.Generic.List<MoveUIPrefabData>   InstanciatedMoveUIElements = new();
        private System.Collections.Generic.List<TargetUIPrefabData> InstanciatedTargetUIElements = new();
        private System.Collections.Generic.List<GameObject>         InstanciatedIntentionTextElements = new();

        private CommandUIBehaviourStates currentState = CommandUIBehaviourStates.UnitIntention;

        private void Awake()
        {
            //StationSelectorManager.OnSelectionChange                += StationSelectorManager_OnSelectionChange;

            //Intention.UnitIntentionManager.OnUnitIntentionChanged   += UnitIntentionManager_OnUnitIntentionChanged;
        }

        private void OnDestroy()
        {
            //StationSelectorManager.OnSelectionChange                -= StationSelectorManager_OnSelectionChange;

            //Intention.UnitIntentionManager.OnUnitIntentionChanged   -= UnitIntentionManager_OnUnitIntentionChanged;

        }

        private void Start()
        {
            //VisualiseSelectedUnitOnStart();
        }

        private void VisualiseSelectedUnitOnStart()
        {
            StationIndex currentlySelectedStation = StationSelectorManager.Instance.GetSelectedStationIndex();

            if (!StationManager.Instance.TryGetUnitIndexOnStation(currentlySelectedStation, out UnitIndex unitIndex)) { return; }

            if (!UnitIntentionManager.Instance.TryGetIntention(unitIndex, out var intention)) { return; }

            SetStateBasedOnUnitIntention(intention);
        }

        private void StationSelectorManager_OnSelectionChange(StationIndex selectedStationIndex, StationIndex? deselectedStationIndex)
        {
            Debug.LogError($"StationSelectorManager_OnSelectionChange");
            if (!StationManager.Instance.TryGetUnitIndexOnStation(selectedStationIndex, out UnitIndex unitIndex)) { return; }

            if (!UnitIntentionManager.Instance.TryGetIntention(unitIndex, out var intention)) {  return; }

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

            Debug.LogError($"On Unit Selection Changed UI fired. ResolutionState: {intentionOfTheUnitIndex.IntentionResolutionState.ToString()}");


            SetStateBasedOnUnitIntention(intentionOfTheUnitIndex);
        }

        private void SetStateBasedOnUnitIntention(Intention.UnitIntention intentionOfTheUnitIndex)
        {
            switch (intentionOfTheUnitIndex.IntentionResolutionState)
            {
                case UnitIntentionResolutionState.AWAITING_MOVE_SELECTION:       
                    SetState(CommandUIBehaviourStates.MoveSelection);
                    break;

                case UnitIntentionResolutionState.AWAITING_TARGET_SELECTION:
                    SetState(CommandUIBehaviourStates.TargetSelection);
                    break;

                default:
                case UnitIntentionResolutionState.NONE:
                case UnitIntentionResolutionState.COMPLETED_INTENTION:
                    SetState(CommandUIBehaviourStates.UnitIntention);
                    break;
            }
        }

        private void SetState(CommandUIBehaviourStates nextState)
        {
            this.currentState = nextState;
            UpdateState();
        }


        private void UpdateState()
        {
            DisableAllWindows();

            StationIndex selectedStationIndex = StationSelectorManager.Instance.GetSelectedStationIndex();
            if (!StationManager.Instance.TryGetUnitIndexOnStation(selectedStationIndex, out UnitIndex unitIndex)) { Debug.Log("Invalid selection change!"); return; }
            if (!StationManager.Instance.TryGetBattleUnitOfIndex(unitIndex, out BaseBattleUnit bBU)) { Debug.Log("Invalid selection change!"); return; }


            switch (this.currentState)
            {
                case CommandUIBehaviourStates.MoveSelection:
                    VisualiseForMoveSelection(unitIndex, bBU);
                    break;

                case CommandUIBehaviourStates.TargetSelection:
                    VisualiseForTargetSelection(unitIndex, bBU);
                    break;

                case CommandUIBehaviourStates.UnitIntention:
                default:      
                    VisualiseForIntention(unitIndex, bBU);
                    break;

            }
        }

        private void VisualiseForMoveSelection(UnitIndex unitIndex, BaseBattleUnit bBU)
        {
            /*  Enable the Inspection for the Health and Status as well as the Unit's Moves.    */
            this.InspectionGameObject.SetActive(true);
            this.UnitImageHealthWrapperGameObject.SetActive(true);

            SetImage(bBU);
            SetHealthValues(unitIndex);
            SetMoves(bBU);
        }
        private void VisualiseForUnitEndOfTurn(UnitIndex unitIndex, BaseBattleUnit bBU)
        {
            Debug.LogWarning("End Turn Enable");

            /*  Enable the Popup Window to show the End of Turn.    */
            this.PopupBufferGameObject.SetActive(true);

        }

        private void VisualiseForTargetSelection(UnitIndex unitIndex, BaseBattleUnit bBU)
        {
            Debug.LogWarning("Target Selection Enable");

            /*  Enable the Keep the inspection window open to show the selected Move but also show the possible targets for the move.   */
            this.InspectionGameObject.SetActive(true);
            this.TargetSelectionGameObject.SetActive(true);

            SetMoves(bBU);

            /*  Get the currently selected Unit's intention to visualise it.    */
            if (!Intention.UnitIntentionManager.Instance.TryGetIntention(unitIndex, out Intention.UnitIntention intention)) { return; }
            TargettingSelectorInfo selectorInfo = StationManagerUtilities.FindAllPossibleTargettingStationIndexesOfTargettingType(unitIndex, MoveTarget.SingleEnemy);
            SetTargets(selectorInfo);
            SetMovesSelectedState(intention.MoveSelection);
        }

        private void VisualiseForIntention(UnitIndex unitIndex, BaseBattleUnit bBU)
        {
            if(!Intention.UnitIntentionManager.Instance.TryGetIntention(unitIndex, out Intention.UnitIntention intention)) { return; }

            /*  Enable the Inspection for the Health and Status as well as the Unit's Moves.    */
            this.InspectionGameObject.SetActive(true);
            this.UnitImageHealthWrapperGameObject.SetActive(true);

            SetImage(bBU);
            SetHealthValues(unitIndex);
            SetIntentionTextElement(bBU, intention);
        }

        private void DestroyIntentionTextUIElements()
        {
            foreach(GameObject obj in this.InstanciatedIntentionTextElements)
            {
                Destroy(obj);
            }
            this.InstanciatedIntentionTextElements.Clear();
        }
        private void SetIntentionTextElement(BaseBattleUnit battleUnit, Intention.UnitIntention unitIntention)
        {
            DestroyIntentionTextUIElements();
            DestroyMoveUIElements();

            GameObject instanciatedTextElement = GameObject.Instantiate(this.IntentionTextPrefab, this.CommandWrapperGameObject.transform);

            /*  Set the intention Text. */
            if (instanciatedTextElement != null && instanciatedTextElement.TryGetComponent(out TMPro.TextMeshProUGUI textMeshPro))
            {
                textMeshPro.text = GetIntentionText(battleUnit, unitIntention);
                this.InstanciatedIntentionTextElements.Add(instanciatedTextElement);
            }
        }

        private string GetIntentionText(BaseBattleUnit battleUnit, Intention.UnitIntention unitIntention)
        {
            //if (!StationManager.Instance.TryGetUnitDataOnStation(unitIntention.DeclaredTargetGroups.FirstOrDefault().Value.FirstOrDefault(), out UnitData targetUnitData)) { return string.Empty; }
            //if (unitIntention.ResolutionState == UnitIntentionResolutionState.COMPLETED_INTENTION)
            //{
            //    return $"{battleUnit.GetBaseUnit().name} is intending to attack {targetUnitData.name} with a {unitIntention.MoveSelection.GetMoveName()}";
            //}
            //else
            //{
                return string.Empty;
            //}
        }

        private void SetImage(BaseBattleUnit battleUnit)
        {
            if (this.UnitImage == null) { return; }
            this.UnitImage.sprite = battleUnit.GetBaseUnit().sprite;
            this.UnitImage.color = battleUnit.GetBaseUnit().color;
        }

        private void SetHealthValues(UnitIndex unitIndex)
        {
            if(this.healthDisplayUI != null)
            {
                this.healthDisplayUI.Initalise(unitIndex);
            }
        }

        #region Move UI Methods

        private void SetMoves(BaseBattleUnit battleUnit)
        {
            DestroyIntentionTextUIElements();
            DestroyMoveUIElements();

            foreach (IBattleMove moveAction in battleUnit.GetBaseUnit().moves)
            {
                CreateMoveUIElement(moveAction);
            }
        }

        private void SetMovesSelectedState(IBattleMove selectedMove)
        {
            Debug.LogWarning("Setting moves for selected move state");

            foreach (MoveUIPrefabData moveButtonData in this.InstanciatedMoveUIElements)
            {
                if (moveButtonData.GetCorrelatingMove() == selectedMove)
                {
                    moveButtonData.LockButtonClickedStatus(true);
                    continue;
                }
                moveButtonData.LockButtonClickedStatus(false); 
            }
        }

        private void CreateMoveUIElement(IBattleMove move)
        {
            if (this.InstanciatedMoveUIElements == null || this.PopupBufferGameObject == null || this.UnitImageHealthWrapperGameObject == null) { return; }

            GameObject instanciatedObject = GameObject.Instantiate(this.MoveUIPrefab, this.CommandWrapperGameObject.transform);
            if (instanciatedObject != null && instanciatedObject.TryGetComponent(out MoveUIPrefabData instanciatedMoveUIData))
            {
                instanciatedMoveUIData.Initalise(move);
                instanciatedMoveUIData.OnButtonClicked += OnMoveButtonClick;
                this.InstanciatedMoveUIElements.Add(instanciatedMoveUIData);
            }
        }

        private void DestroyMoveUIElements()
        {
            if (this.InstanciatedMoveUIElements == null) { return; }

            foreach (MoveUIPrefabData obj in this.InstanciatedMoveUIElements)
            {
                obj.OnButtonClicked -= OnMoveButtonClick;

                Destroy(obj.gameObject);
            }
            this.InstanciatedMoveUIElements.Clear();
        }

        private void OnMoveButtonClick(MoveUIPrefabData buttonObject, bool isPressed)
        {
            /*  Unclick all buttons when this is clicked.   */
            foreach (MoveUIPrefabData moveButtonData in this.InstanciatedMoveUIElements)
            {
                if (moveButtonData == buttonObject) { continue; }

                moveButtonData.IsButtonClicked = false;
            }

            /*  If the selectedUnitIndex is the one we are processing, then continue   */
            if (!StationSelectorManager.Instance.TryGetUnitIndexOfSelectedStation(out UnitIndex selectedUnitIndex)) { return; }

            Debug.LogError($"Move button clicked where selected unit is: {selectedUnitIndex.Index} " +
                $"and the current UI UserInput selected Unit is: {UserInterfaceUserInput.Instance.GetSelectedUnit()?.Index}");
            
            if (UserInterfaceUserInput.Instance.GetSelectedUnit()?.Index != selectedUnitIndex.Index) { return; }

            UserInterfaceUserInput.Instance.OnMoveSelection(buttonObject.GetCorrelatingMove());
        }

        #endregion

            #region Target UI Methods

        private void SetTargets(TargettingSelectorInfo targettingSelectorInfo)
        {
            DestroyTargetUIElements();

            /*  
             *  The Selector Info says if we need to select from among the targets or if they are amalgamated into one option.
             *  I.e. if the selected move can only target everyone on the field, there is no point making multiple Target buttons for each target if we are hitting all of them.
             */

            if (!targettingSelectorInfo.DoesRequireTargettingSelectorSelection)
            {
                CreateTargetUIElement(targettingSelectorInfo.PossibleTargets, targettingSelectorInfo.TargettingDisplayText);
                return;
            }

            foreach (StationIndex possibleTargetStation in targettingSelectorInfo.PossibleTargets)
            {
                CreateTargetUIElement(possibleTargetStation);
            }          
        }

        private void CreateTargetUIElement(StationIndex targetedStationIndex)
        {
            if (this.InstanciatedTargetUIElements == null || this.PopupBufferGameObject == null || this.UnitImageHealthWrapperGameObject == null) { return; }

            GameObject instanciatedObject = GameObject.Instantiate(this.TargetUIPrefab, this.TargetSelectionGameObject.transform);
            if (instanciatedObject != null && instanciatedObject.TryGetComponent(out TargetUIPrefabData instanciatedTargetUIData))
            {
                instanciatedTargetUIData.Initalise(targetedStationIndex);
                instanciatedTargetUIData.OnButtonClicked += OnTargetButtonClick;
                this.InstanciatedTargetUIElements.Add(instanciatedTargetUIData);
            }
        }

        private void CreateTargetUIElement(System.Collections.Generic.List<StationIndex> targetStations, string targetText)
        {
            if (this.InstanciatedTargetUIElements == null || this.PopupBufferGameObject == null || this.UnitImageHealthWrapperGameObject == null) { return; }
            GameObject instanciatedObject = GameObject.Instantiate(this.TargetSelectionGameObject, this.CommandWrapperGameObject.transform);
            if (instanciatedObject != null && instanciatedObject.TryGetComponent(out TargetUIPrefabData instanciatedTargetUIData))
            {
                instanciatedTargetUIData.Initalise(targetStations, targetText);
                instanciatedTargetUIData.OnButtonClicked += OnTargetButtonClick;
                this.InstanciatedTargetUIElements.Add(instanciatedTargetUIData);
            }
        }

        private void OnTargetButtonClick(TargetUIPrefabData buttonObject, bool isPressed)
        {
            /*  Unclick all buttons when this is clicked.   */
            foreach (TargetUIPrefabData targetButtonData in this.InstanciatedTargetUIElements)
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
        private void DestroyTargetUIElements()
        {
            if (this.InstanciatedTargetUIElements == null) { return; }

            foreach (TargetUIPrefabData obj in this.InstanciatedTargetUIElements)
            {
                obj.OnButtonClicked -= OnTargetButtonClick;
                Destroy(obj.gameObject);
            }
            this.InstanciatedTargetUIElements.Clear();
        }



        #endregion
        void DisableAllWindows()
        {
            if (this.InspectionGameObject == null || this.PopupBufferGameObject == null || this.UnitImageHealthWrapperGameObject == null || this.TargetSelectionGameObject == null) { return; }

            this.InspectionGameObject.SetActive(false);
            this.PopupBufferGameObject.SetActive(false);
            this.UnitImageHealthWrapperGameObject.SetActive(false);
            this.TargetSelectionGameObject.SetActive(false);
        }
    }
}