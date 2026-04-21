namespace TurnBased.Phases
{
    public class SubPhaseManager
    {
        private readonly System.Collections.Generic.Dictionary<SubPhaseState, SubPhase> MainPhaseStates = new();
        private SubPhaseState currentState = SubPhaseState.NONE;
        public SubPhaseState CurrentSubPhaseState
        {
            get
            {
                return currentState;
            }
            set
            {
                currentState = value;
            }
        }

        public void Awake(System.Action<SubPhaseState> OnSubPhaseComplete) 
        {
            /*  Clear the previous Phases for this currentUnit and Initalise them.  */
            this.MainPhaseStates.Clear();

            /*  Add all phases to the dictionary.   */
            this.MainPhaseStates.Add(SubPhaseState.NONE,                        new UnitTurnPhase_None(OnSubPhaseComplete));
            this.MainPhaseStates.Add(SubPhaseState.AWAITING_MOVE_SELECTION,     new UnitTurnPhase_MoveSelection(OnSubPhaseComplete));
            this.MainPhaseStates.Add(SubPhaseState.AWAITING_TARGET_SELECTION,   new UnitTurnPhase_TargetSelection(OnSubPhaseComplete));
            this.MainPhaseStates.Add(SubPhaseState.READY_TO_EXECUTE_MOVE,       new UnitTurnPhase_ReadyToExecuteMove(OnSubPhaseComplete));
            this.MainPhaseStates.Add(SubPhaseState.RESOLVE_ATTACK,              new UnitTurnPhase_ResolveAttack(OnSubPhaseComplete));
            this.MainPhaseStates.Add(SubPhaseState.ATTACK_COMPLETE,             new UnitTurnPhase_AttackComplete(OnSubPhaseComplete));
        }

        public void OnDestroy()
        {
            this.MainPhaseStates.Clear();
        }

        public void SwitchSubPhase(SubPhaseState newState, UnitIndex selectedUnitIndex)
        {
            UnityEngine.Debug.LogWarning($"Switching to Phase: {newState} ");

            this.MainPhaseStates[this.CurrentSubPhaseState]?.OnExit();

            this.CurrentSubPhaseState = newState;
            this.MainPhaseStates[this.CurrentSubPhaseState]?.SetCurrentUnitIndex(selectedUnitIndex);
 

            this.MainPhaseStates[this.CurrentSubPhaseState]?.OnEnter();
        }

        public void Update()
        {
            this.MainPhaseStates[this.CurrentSubPhaseState]?.Update();
        }

        public void ResetCurrentPhase()
        {
            this.CurrentSubPhaseState = SubPhaseState.NONE;
        }
    }
}