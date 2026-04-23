using System.Linq;
using UnityEngine;

namespace TurnBased.Intention {
    public class CombatRoundUnitIntentionManager
    {
        private readonly Phases.CombatTurnOrchestrator orchestrator;

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

        public CombatRoundUnitIntentionManager(Phases.CombatTurnOrchestrator combatTurnOrchestrator)
        {
            this.orchestrator = combatTurnOrchestrator;
        }


        public void Awake()
        {
            StationSelectorManager.OnSelectionChange        += OnStationSelectionChange;

            MoveSelectionResolver.OnRequireUserInput        += SetIsAwaitingUserInput;
            TargetSelectionResolver.OnRequireUserInput      += SetIsAwaitingUserInput;
            MoveSelectionResolver.OnCompleteUserInput       += CompleteAwaitingUserInput;
            TargetSelectionResolver.OnCompleteUserInput     += CompleteAwaitingUserInput;
        }


        public void OnDestroy()
        {
            MoveSelectionResolver.OnRequireUserInput        -= SetIsAwaitingUserInput;
            TargetSelectionResolver.OnRequireUserInput      -= SetIsAwaitingUserInput;
            MoveSelectionResolver.OnCompleteUserInput       -= CompleteAwaitingUserInput;
            TargetSelectionResolver.OnCompleteUserInput     -= CompleteAwaitingUserInput;
        }

        private void OnStationSelectionChange(StationIndex newSelectedIndex, StationIndex? oldSelectedIndex)
        {
            /*  Get the selectedUnitIndex on the new selected station. If the new selected unit index exists in our resolution list, select that new Unit.  */
            if (!StationSelectorManager.Instance.TryGetUnitIndexOfSelectedStation(out UnitIndex selectedUnitIndex)) { return; }

            if (!DoesUnitIndexExistInResolvingList(selectedUnitIndex)) { return; }

            SelectNewUnit(selectedUnitIndex);
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
            this.ProcessingUnitIndexes = IntentionResolverUtility.GetAllAutonomousUnits();

            if (this.ProcessingUnitIndexes.Count > 0)
            {
                CurrentResolvingUnit = this.ProcessingUnitIndexes.FirstOrDefault();
            }

            UnityEngine.Debug.LogWarning($"Obtained non-player-driven intentions. Size of list is: {this.ProcessingUnitIndexes.Count}, currently resolving unit index is: {CurrentResolvingUnit.Value.Index}");
            ProcessIntentionOfResolvingUnit();
        }

        public void ObtainPlayerDrivenUnitIntentions()
        {
            this.ProcessingUnitIndexes = IntentionResolverUtility.GetAllPlayerDrivenUnits();

            UnityEngine.Debug.LogError($"Processing Player driven intentions count is: {this.ProcessingUnitIndexes.Count}");

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
                }
                else
                {
                    UnityEngine.Debug.LogWarning($"Result was false. OUGH");

                }             
            }
        }

        public void ProcessIntentionOfResolvingUnit()
        {
            UnityEngine.Debug.LogWarning($"Processing Intention of Unit index: {CurrentResolvingUnit.Value.Index}");

            /*  If the Current Resolving Unit is not within our resolving list, don't process it and get the next unit. */
            if (!IntentionResolverUtility.DoesListContainUnitIndex(CurrentResolvingUnit.Value, this.ProcessingUnitIndexes)) { GetNextUnitInList(CurrentResolvingUnit.Value); }

            UnityEngine.Debug.LogWarning($"Is contained in list");

            /*  Peek at the current Unit's intention state. Tell the Orchestrator to move into that state.  */
            if (!Intention.UnitIntentionManager.Instance.TryGetIntention(currentResolvingUnit.Value, out UnitIntention intention)) { GetNextUnitInList(CurrentResolvingUnit.Value); }

            UnityEngine.Debug.LogWarning($"Intent of Unit is: {intention.ResolutionState.ToString()}");

            switch (intention.ResolutionState)
            {
                case UnitIntentionResolutionState.AWAITING_MOVE_SELECTION:
                    UnityEngine.Debug.LogWarning($"Changing subphase to move selection");

                    this.orchestrator.ChangeSubPhase(SubPhaseState.AWAITING_MOVE_SELECTION, CurrentResolvingUnit.Value);
                    break;

                case UnitIntentionResolutionState.AWAITING_TARGET_SELECTION:
                    UnityEngine.Debug.LogWarning($"Changing subphase to target selection");

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
            UnityEngine.Debug.LogWarning($"Does unit exist in the list?");

            /*  If this UnitIndex exists in our processing list and is identical to the currently resolving unit, we want to remove this currently resolving unit from the processing list. */
            if (IntentionResolverUtility.DoesListContainUnitIndex(previousUnit, this.ProcessingUnitIndexes) && CurrentResolvingUnit.HasValue && CurrentResolvingUnit.Value.Index == previousUnit.Index)
            {
                if (!this.ProcessingUnitIndexes.Remove(previousUnit)) { throw new System.IndexOutOfRangeException("ERROR — INTENTION MANAGER: UNABLE TO REMOVE THE PREVIOUS UNIT WHEN SELECTING NEW UNIT! UNIT DOES NOT EXIST IN COLLECTION"); }
                else
                {
                    UnityEngine.Debug.LogWarning($"Removed previous unit: {previousUnit.Index}");
                }
            }

            UnityEngine.Debug.LogWarning($"Is there enough Units in the list?");

            /*  Retrieve the next new active unit if the container exists. If not, we are done. */
            if (this.ProcessingUnitIndexes.Count > 0)
            {
         
                CurrentResolvingUnit = this.ProcessingUnitIndexes.FirstOrDefault();

                UnityEngine.Debug.LogWarning($"New Unit selected with index: {currentResolvingUnit.Value.Index}. Old Index was: {previousUnit.Index}");

                return true;
            }


            UnityEngine.Debug.LogWarning($"List of the intention list is empty!!!");

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
            Debug.LogWarning("COMBAT ROUND MANAGER IS AWAITING USER INPUT!");

            IsAwaitingUserInput = true; 
            /*  Alert the UI    */
            TurnBased.UI.UserInterfaceUserInput.Instance.StartSelection(unitIndex);
        }

        public void CompleteAwaitingUserInput(UnitIndex unitIndex)
        {
            Debug.LogWarning("COMBAT ROUND MANAGER IS NO LONGER AWAITING USER INPUT!");

            IsAwaitingUserInput = false;
            /*  Alert the UI    */
            TurnBased.UI.UserInterfaceUserInput.Instance.StopSelection(unitIndex);
        }
    }
}