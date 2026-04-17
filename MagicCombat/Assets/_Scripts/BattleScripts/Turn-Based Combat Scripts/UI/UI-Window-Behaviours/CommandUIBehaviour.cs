using System.Linq;
using TMPro;
using UnityEngine;

namespace TurnBased.UI
{
    public class CommandUIBehaviour : MonoBehaviour
    {
        [Header("Inspector Variables")]
        [SerializeField] private UnityEngine.UI.Image UnitImage;
        [SerializeField, Tooltip("Assign with the 'Health' Object in HealthBG")] private RectTransform HealthRectTransform;
        [SerializeField, Tooltip("Assign with the 'HealthValueString' Object in HealthBG")] TMPro.TextMeshProUGUI HealthText;

        [SerializeField, Tooltip("Assign with the 'CommandWrapper' Object in Command")] GameObject CommandWrapperGameObject;
        [SerializeField, Tooltip("Assign with the 'Inspection' Object in InspectionSubWindow")] GameObject InspectionGameObject;
        [SerializeField, Tooltip("Assign with the 'PopupBuffer' Object in InspectionSubWindow")] GameObject PopupBufferGameObject;
        [SerializeField, Tooltip("Assign with the 'UnitImageHealthWrapper' Object in VIew")] GameObject UnitImageHealthWrapperGameObject;
        [SerializeField, Tooltip("Assign with the 'TargetSelection' Object in VIew")] GameObject TargetSelectionGameObject;


        [Header("Prefabs")]
        [SerializeField] private GameObject MoveUIPrefab;
        [SerializeField] private GameObject TargetUIPrefab;
        [SerializeField] private GameObject IntentionTextPrefab;

        private System.Collections.Generic.List<MoveUIPrefabData>   InstanciatedMoveUIElements = new();
        private System.Collections.Generic.List<TargetUIPrefabData> InstanciatedTargetUIElements = new();
        private System.Collections.Generic.List<GameObject>         InstanciatedIntentionTextElements = new();

        private CommandUIBehaviourStates currentState = CommandUIBehaviourStates.MoveSelection;

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
            OnUnitIntentionChanged();
        }

        private void StationSelectorManager_OnSelectionChange(StationIndex selectedStationIndex, StationIndex? deselectedStationIndex)
        {
            OnUnitIntentionChanged();
        }

        private void OnUnitIntentionChanged()
        {
            if (!StationManager.Instance.TryGetUnitIndexOnStation(StationSelectorManager.Instance.GetSelectedStationIndex(), out UnitIndex unitIndex)) { return; }

            if (!Intention.UnitIntentionManager.Instance.TryGetIntention(unitIndex, out Intention.UnitIntention intention)) { return; }

            UnitIntentionManager_OnUnitIntentionChanged(unitIndex, intention);
        }

        private void UnitIntentionManager_OnUnitIntentionChanged(UnitIndex unitIndex, Intention.UnitIntention intentionOfTheUnitIndex)
        {
            if (!StationManager.Instance.TryGetStationIndexOfIndex(unitIndex, out StationIndex stationIndexOfUnitIndex)) { return; }

            /*  
             *  Only update this UI element if the intention belonged to the selected Unit. 
             *  Basically, if an enemy aren't selected changes it's intention, we don't want to do anything as we aren't displaying that unit.  
             */
            if (stationIndexOfUnitIndex.Index != StationSelectorManager.Instance.GetSelectedStationIndex().Index) { return; }

            SetStateBasedOnUnitIntention(intentionOfTheUnitIndex);
        }

        private void SetStateBasedOnUnitIntention(Intention.UnitIntention intentionOfTheUnitIndex)
        {
            switch (intentionOfTheUnitIndex.ResolutionState)
            {
                case UnitIntentionResolutionState.AWAITING_MOVE_SELECTION:       
                    SetState(CommandUIBehaviourStates.MoveSelection);
                    break;

                case UnitIntentionResolutionState.AWAITING_TARGET_SELECTION:
                    SetState(CommandUIBehaviourStates.TargetSelection);
                    break;

                default:
                case UnitIntentionResolutionState.NONE:
                case UnitIntentionResolutionState.COMPLETE:
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
            SetHealthValues(bBU);
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
            TargettingSelectorInfo selectorInfo = StationManagerUtilities.FindAllPossibleTargettingStationIndexesOfTargettingType(unitIndex, intention.MoveSelection.GetMoveTargetType());
            SetTargets(selectorInfo);
            SetMovesSelectedState(intention.MoveSelection);
        }

        private void VisualiseForIntention(UnitIndex unitIndex, BaseBattleUnit bBU)
        {
            if(!Intention.UnitIntentionManager.Instance.TryGetIntention(unitIndex, out Intention.UnitIntention intention)) { return; }

            Debug.LogWarning("Default Enable");

            /*  Enable the Inspection for the Health and Status as well as the Unit's Moves.    */
            this.InspectionGameObject.SetActive(true);
            this.UnitImageHealthWrapperGameObject.SetActive(true);

            SetImage(bBU);
            SetHealthValues(bBU);
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
            if (instanciatedTextElement != null && instanciatedTextElement.TryGetComponent(out TextMeshProUGUI textMeshPro))
            {
                textMeshPro.text = GetIntentionText(battleUnit, unitIntention);
                this.InstanciatedIntentionTextElements.Add(instanciatedTextElement);
            }
        }

        private string GetIntentionText(BaseBattleUnit battleUnit, Intention.UnitIntention unitIntention)
        {
            if (!StationManager.Instance.TryGetUnitDataOnStation(unitIntention.TargetIndexList.FirstOrDefault(), out UnitData targetUnitData)) { return string.Empty; }
            return $"{battleUnit.GetBaseUnit().name} is intending to attack {targetUnitData.name} with a {unitIntention.MoveSelection.GetMoveName()}";
        }

        private void SetImage(BaseBattleUnit battleUnit)
        {
            if (this.UnitImage == null) { return; }
            this.UnitImage.sprite = battleUnit.GetBaseUnit().sprite;
            this.UnitImage.color = battleUnit.GetBaseUnit().color;
        }

        private void SetHealthValues(BaseBattleUnit battleUnit)
        {
            if (this.HealthRectTransform != null && this.HealthText != null)
            {
                int currentHealth = battleUnit.GetHealthComponent().GetHealth();
                int maximumHealth = battleUnit.GetBaseUnit().maxHP;

                float value = UserInterfaceUtility.GetValueNormalisation(minimum: 0, maximum: maximumHealth, current: currentHealth);

                this.HealthRectTransform.localScale = new(value, 1);
                this.HealthText.text = currentHealth.ToString() + "/" + maximumHealth.ToString();
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

        private void OnMoveButtonClick(MoveUIPrefabData buttonObject, bool isPressed)
        {
            foreach (MoveUIPrefabData moveButtonData in this.InstanciatedMoveUIElements)
            {
                if (moveButtonData == buttonObject) { continue; }

                moveButtonData.IsButtonClicked = false;

                UserInterfaceUserInput.Instance.OnMoveSelection();
                return;
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
                CreateTargetUIElement(targettingSelectorInfo.TargettingDisplayText);
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

        private void CreateTargetUIElement(string targetText)
        {
            if (this.InstanciatedTargetUIElements == null || this.PopupBufferGameObject == null || this.UnitImageHealthWrapperGameObject == null) { return; }
            GameObject instanciatedObject = GameObject.Instantiate(this.TargetSelectionGameObject, this.CommandWrapperGameObject.transform);
            if (instanciatedObject != null && instanciatedObject.TryGetComponent(out TargetUIPrefabData instanciatedTargetUIData))
            {
                instanciatedTargetUIData.Initalise(targetText);
                instanciatedTargetUIData.OnButtonClicked += OnTargetButtonClick;
                this.InstanciatedTargetUIElements.Add(instanciatedTargetUIData);
            }
        }

        private void OnTargetButtonClick(TargetUIPrefabData buttonObject, bool isPressed)
        {
            foreach (TargetUIPrefabData targetButtonData in this.InstanciatedTargetUIElements)
            {
                if (targetButtonData == buttonObject) { continue; }

                targetButtonData.IsButtonClicked = false;

                UserInterfaceUserInput.Instance.OnTargetSelection();
                return;
            }
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