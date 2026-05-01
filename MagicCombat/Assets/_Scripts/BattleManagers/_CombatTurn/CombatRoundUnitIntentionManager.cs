using System.Linq;
using UnityEngine;

namespace TurnBased.Intention {
    public class CombatRoundUnitIntentionManager
    {
        private Phases.CombatTurnOrchestrator orchestrator;

        /// =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=
        /// 
        /// If the Intentions are finished resolving. Happens Twice, once when we do Non-Player-Driven Intentions and when we do Player-Driven Intentions
        /// 
        /// =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=

        private bool intentionsComplete;
        public bool IsAllUnitIntentionsComplete
        {
            get
            {
                return intentionsComplete;
            }
            set
            {
                intentionsComplete = value;
            }
        }


        private static bool isAwaitingUserInput = false;
        public static bool IsAwaitingUserInput
        {
            get
            {
                return isAwaitingUserInput;
            }
            private set
            {
                isAwaitingUserInput = value;
            }
        }





        private static UnitIndex? currentResolvingUnit;
        public static UnitIndex? CurrentResolvingUnit
        {
            get
            {
                return currentResolvingUnit;
            }
            private set
            {
                currentResolvingUnit = value;
                OnResolvingUnitChange?.Invoke(currentResolvingUnit);
            }
        }

        public static event System.Action<UnitIndex?> OnResolvingUnitChange;
        public static event System.Action OnAllIntentionsResolved;

        private System.Collections.Generic.List<UnitIndex> ProcessingUnitIndexes;


        public void Awake(Phases.CombatTurnOrchestrator combatTurnOrchestrator)
        {
            UnityEngine.Debug.LogError($"Assigning the orchestrator to: {combatTurnOrchestrator}");
            this.orchestrator = combatTurnOrchestrator;

            this.ProcessingUnitIndexes = new();

            StationSelectorManager.OnSelectionChange        += OnStationSelectionChange;

            MoveSelectionResolver.OnRequireUserInput        += SetIsAwaitingUserInput;
            TargetSelectionResolver.OnRequireUserInput      += SetIsAwaitingUserInput;
            MoveSelectionResolver.OnCompleteUserInput       += CompleteAwaitingUserInput;
            TargetSelectionResolver.OnCompleteUserInput     += CompleteAwaitingUserInput;
        }


        public void OnDestroy()
        {
            UnityEngine.Debug.Log($"Nullifying the orchestrator");
            this.orchestrator = null;
            IsAwaitingUserInput = false;
            IsAllUnitIntentionsComplete = false;
            StationSelectorManager.OnSelectionChange -= OnStationSelectionChange;

            MoveSelectionResolver.OnRequireUserInput        -= SetIsAwaitingUserInput;
            TargetSelectionResolver.OnRequireUserInput      -= SetIsAwaitingUserInput;
            MoveSelectionResolver.OnCompleteUserInput       -= CompleteAwaitingUserInput;
            TargetSelectionResolver.OnCompleteUserInput     -= CompleteAwaitingUserInput;

            OnResolvingUnitChange = null;
            OnAllIntentionsResolved = null;
        }

        private void OnStationSelectionChange(StationIndex newSelectedIndex, StationIndex? oldSelectedIndex)
        {
            UnityEngine.Debug.Log($"CombatRoundUnitIntentionManager - OnStationSelectionChange  called !");

            /*  Get the selectedUnitIndex on the new selected station. If the new selected unit index exists in our resolution list, select that new Unit.  */
            if (!StationSelectorManager.Instance.TryGetUnitIndexOfSelectedStation(out UnitIndex selectedUnitIndex)) { return; }

            if (!DoesUnitIndexExistInResolvingList(selectedUnitIndex)) { return; }

            UnityEngine.Debug.Log($"Station selection change. Selected unit index does exist  in list!");

            SelectNewUnit(selectedUnitIndex);

            UnityEngine.Debug.Log($"Selected new unit!");
        }

        private void SelectNewUnit(UnitIndex unitIndex)
        {
            if (this.ProcessingUnitIndexes.Count > 0)
            {
                CurrentResolvingUnit = unitIndex;

                ProcessIntentionOfResolvingUnit();
            }
        }

        public void ObtainNonPlayerDrivenUnitIntentions()
        {
            Debug.LogWarning($"Getting autonomous. Orchestrator is: {this.orchestrator} ");

            this.ProcessingUnitIndexes = IntentionResolverUtility.GetAllAutonomousUnits();

            if (this.ProcessingUnitIndexes.Count > 0)
            {
                CurrentResolvingUnit = this.ProcessingUnitIndexes.FirstOrDefault();
            }

            Debug.LogWarning($"Autonomous count is: {this.ProcessingUnitIndexes.Count}. Orchestrator is: {this.orchestrator}  ");

            ProcessIntentionOfResolvingUnit();
        }

        public void ObtainPlayerDrivenUnitIntentions()
        {
            this.ProcessingUnitIndexes = IntentionResolverUtility.GetAllPlayerDrivenUnits();

            if (this.ProcessingUnitIndexes.Count > 0)
            {
                CurrentResolvingUnit = this.ProcessingUnitIndexes.FirstOrDefault();
            }
            ProcessIntentionOfResolvingUnit();
        }

        public void ProcessNextIntentionInSequence()
        {
            if (this.ProcessingUnitIndexes.Count > 0)
            {
                bool res = GetNextUnitInList(CurrentResolvingUnit.Value);

                if (res)
                {
                    ProcessIntentionOfResolvingUnit();
                    StationSelectorManager.Instance.SetSelectedStationIndex(CurrentResolvingUnit.Value);
                }
            }
        }

        public void ProcessIntentionOfResolvingUnit()
        {
            /*  If the Current Resolving Unit is not within our resolving list, don't process it and get the next unit. */
            if (!IntentionResolverUtility.DoesListContainUnitIndex(CurrentResolvingUnit.Value, this.ProcessingUnitIndexes)) { GetNextUnitInList(CurrentResolvingUnit.Value); }

            /*  Peek at the current Unit's intention state. Tell the Orchestrator to move into that state.  */
            if (!Intention.UnitIntentionManager.Instance.TryGetIntention(currentResolvingUnit.Value, out UnitIntention intention)) { GetNextUnitInList(CurrentResolvingUnit.Value); }

            Debug.LogWarning($"Intention res state is: {intention.ResolutionState}");

            switch (intention.ResolutionState)
            {
                case UnitIntentionResolutionState.NONE:
                case UnitIntentionResolutionState.AWAITING_MOVE_SELECTION:

                    Debug.LogWarning($"Awaiting move selection. Orchestrator is: {this.orchestrator}");

                    this.orchestrator.ChangeSubPhase(SubPhaseState.AWAITING_MOVE_SELECTION, CurrentResolvingUnit.Value);
                    break;

                case UnitIntentionResolutionState.AWAITING_TARGET_SELECTION:

                    this.orchestrator.ChangeSubPhase(SubPhaseState.AWAITING_TARGET_SELECTION, CurrentResolvingUnit.Value);
                    break;

                case UnitIntentionResolutionState.COMPLETED_INTENTION:
                    this.orchestrator.ChangeSubPhase(SubPhaseState.READY_TO_EXECUTE_MOVE, CurrentResolvingUnit.Value);
                    break;

                case UnitIntentionResolutionState.RESOLVED_MOVE:
                    break;

                default:

                    break;
            }
        }


        /// <summary>
        ///    
        /// </summary>
        /// <param name="previousUnit"></param>
        /// <returns>Returns true if there is a next unit. If there isn't a new unit to get in the list. Returns false. </returns>
        private bool GetNextUnitInList(UnitIndex previousUnit)
        {
            /*  If this UnitIndex exists in our processing list and is identical to the currently resolving unit, we want to remove this currently resolving unit from the processing list. */
            if (IntentionResolverUtility.DoesListContainUnitIndex(previousUnit, this.ProcessingUnitIndexes) && CurrentResolvingUnit.HasValue && CurrentResolvingUnit.Value.Index == previousUnit.Index)
            {
                if (!this.ProcessingUnitIndexes.Remove(previousUnit)) { throw new System.IndexOutOfRangeException("ERROR — INTENTION MANAGER: UNABLE TO REMOVE THE PREVIOUS UNIT WHEN SELECTING NEW UNIT! UNIT DOES NOT EXIST IN COLLECTION"); }
                else
                {
                    UnityEngine.Debug.LogWarning($"Removed previous unit: {previousUnit.Index}");
                }
            }


            Debug.LogWarning($"Processing indexes count is: {this.ProcessingUnitIndexes.Count}");

            /*  Retrieve the next new active unit if the container exists. If not, we are done. */
            if (this.ProcessingUnitIndexes.Count > 0)
            {
                CurrentResolvingUnit = this.ProcessingUnitIndexes.FirstOrDefault();
                return true;
            }

            Debug.LogWarning($"All intents resolved");

            /*  The list is empty, therefore we are done with our intentions.   */
            OnAllIntentionsResolved?.Invoke();
            return false;
        }

        private bool DoesUnitIndexExistInResolvingList(UnitIndex unitIndex)
        {
            if (this.ProcessingUnitIndexes == null) { return false; }

            foreach(UnitIndex index in this.ProcessingUnitIndexes)
            {
                if (index.Index == unitIndex.Index) {  return true; }
            }
            return false;
        }



        public void SetIsAwaitingUserInput(UnitIndex unitIndex)
        {
            IsAwaitingUserInput = true; 
            /*  Alert the UI    */
            TurnBased.UI.UserInterfaceUserInput.Instance.StartSelection(unitIndex);
        }

        public void CompleteAwaitingUserInput(UnitIndex unitIndex)
        {
            IsAwaitingUserInput = false;
            /*  Alert the UI    */
            TurnBased.UI.UserInterfaceUserInput.Instance.StopSelection(unitIndex);
        }
    }
}