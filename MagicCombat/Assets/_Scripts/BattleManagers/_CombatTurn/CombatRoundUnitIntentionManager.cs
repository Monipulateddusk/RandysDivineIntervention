using System.Linq;
using UnityEngine;

namespace TurnBased.Intention {
    public class CombatRoundUnitIntentionManager
    {
        private static CombatRoundUnitIntentionManager instance;
        public static CombatRoundUnitIntentionManager Instance
        {
            get
            {
                try
                {
                    return instance;
                }
                catch (System.Exception e)
                {
                    UnityEngine.Debug.LogError(e.ToString());
                    return null;
                }
            }
        }

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
            Intention.MoveSelectionResolver.OnRequireUserInput      += SetIsAwaitingUserInput;
            Intention.TargetSelectionResolver.OnRequireUserInput    += SetIsAwaitingUserInput;
            Intention.MoveSelectionResolver.OnCompleteUserInput     += CompleteAwaitingUserInput;
        }

        public void OnDestroy()
        {
            Intention.MoveSelectionResolver.OnRequireUserInput      -= SetIsAwaitingUserInput;
            Intention.TargetSelectionResolver.OnRequireUserInput    -= SetIsAwaitingUserInput;
            Intention.MoveSelectionResolver.OnCompleteUserInput     -= CompleteAwaitingUserInput;
        }

        public void ObtainNonPlayerDrivenUnitIntentions()
        {
            this.ProcessingUnitIndexes = IntentionResolverUtility.GetAllAutonomousUnits();

            if (this.ProcessingUnitIndexes.Count > 0)
            {
                CurrentResolvingUnit = this.ProcessingUnitIndexes.FirstOrDefault();
            }
            ProcessIntentionOfResolvingUnit();
        }

        public void ObtainPlayerDrivenUnitIntentions()
        {
            this.ProcessingUnitIndexes = IntentionResolverUtility.GetAllPlayerDrivenUnits();
        }

        public void ProcessNextIntentionInSequence()
        {

            if (this.ProcessingUnitIndexes.Count > 0)
            {
                GetNextUnitInList(CurrentResolvingUnit.Value);
            }
        }

        private void ProcessIntentionOfResolvingUnit()
        {
            /*  If the Current Resolving Unit is not within our resolving list, don't process it and get the next unit. */
            if (!IntentionResolverUtility.DoesListContainUnitIndex(CurrentResolvingUnit.Value, this.ProcessingUnitIndexes)) { GetNextUnitInList(CurrentResolvingUnit.Value); }

            /*  Peek at the current Unit's intention state. Tell the Orchestrator to move into that state.  */
            if (!Intention.UnitIntentionManager.Instance.TryGetIntention(currentResolvingUnit.Value, out UnitIntention intention)) { GetNextUnitInList(CurrentResolvingUnit.Value); }

            switch (intention.ResolutionState)
            {
                case UnitIntentionResolutionState.AWAITING_MOVE_SELECTION:
                    this.orchestrator.ChangeSubPhase(SubPhaseState.AWAITING_TARGET_SELECTION, CurrentResolvingUnit.Value);
                    break;

                case UnitIntentionResolutionState.AWAITING_TARGET_SELECTION:
                    this.orchestrator.ChangeSubPhase(SubPhaseState.READY_TO_EXECUTE_MOVE, CurrentResolvingUnit.Value);
                    break;

                case UnitIntentionResolutionState.COMPLETED_INTENTION:
                    this.orchestrator.ChangeSubPhase(SubPhaseState.RESOLVE_ATTACK, CurrentResolvingUnit.Value);
                    break;

                case UnitIntentionResolutionState.RESOLVED_MOVE:
                    break;

                default:

                    break;
            }
        }


        private void GetNextUnitInList(UnitIndex previousUnit)
        {
            if (IntentionResolverUtility.DoesListContainUnitIndex(previousUnit, this.ProcessingUnitIndexes) && CurrentResolvingUnit.HasValue && CurrentResolvingUnit.Value.Index == previousUnit.Index)
            {
                if (!this.ProcessingUnitIndexes.Remove(previousUnit)) { UnityEngine.Debug.LogWarning($"ERROR — INTENTION MANAGER: UNABLE TO REMOVE THE PREVIOUS UNIT WHEN SELECTING NEW UNIT! UNIT DOES NOT EXIST IN COLLECTION"); }
            }


            /*  Retrieve the next new active unit if the container exists. If not, we are done. */
            if (this.ProcessingUnitIndexes.Count > 0)
            {
                MonoBehaviour.print("<color=orange>Selecting new Unit in list</color>");
                CurrentResolvingUnit = this.ProcessingUnitIndexes.FirstOrDefault();
                return;
            }

            /*  The list is empty, therefore we are done with our intentions.   */
            HandleIsDoneIntentions();
        }


        private void HandleIsDoneIntentions()
        {
            if (this.ProcessingUnitIndexes.Count == 0)
            {
                UnityEngine.Debug.LogWarning("ALL UNITS PROCESSED!");

                OnAllIntentionsResolved?.Invoke();

                UnitIntentionManager.Instance.PrintOutAllIntents();
            }
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

        }
    }
}