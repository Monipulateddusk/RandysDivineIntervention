namespace TurnBased.Phases
{
    public class SubPhaseManager
    {
        private System.Collections.Generic.Dictionary<SubPhaseState, SubPhase> MainPhaseStates = new();
        private SubPhaseState currentState;
        private UnitIndex? selectedIndex;
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

        public void Awake(EventHookSystem hookSystem, System.Action<SubPhaseState> OnSubPhaseComplete) 
        {
            /*  Clear the previous Phases for this currentUnit and Initalise them.  */
            this.MainPhaseStates = new();
            this.MainPhaseStates.Clear();

            /*  Add all phases to the dictionary.   */
            this.MainPhaseStates.Add(SubPhaseState.NONE,                        new UnitTurnPhase_None(hookSystem, OnSubPhaseComplete));
            this.MainPhaseStates.Add(SubPhaseState.AWAITING_MOVE_SELECTION,     new UnitTurnPhase_MoveSelection(hookSystem, OnSubPhaseComplete));
            this.MainPhaseStates.Add(SubPhaseState.AWAITING_TARGET_SELECTION,   new UnitTurnPhase_TargetSelection(hookSystem, OnSubPhaseComplete));
            this.MainPhaseStates.Add(SubPhaseState.READY_TO_EXECUTE_MOVE,       new UnitTurnPhase_ReadyToExecuteMove(hookSystem, OnSubPhaseComplete));
            this.MainPhaseStates.Add(SubPhaseState.RESOLVE_ATTACK,              new UnitTurnPhase_ResolveAttack(hookSystem, OnSubPhaseComplete));
            this.MainPhaseStates.Add(SubPhaseState.ATTACK_COMPLETE,             new UnitTurnPhase_AttackComplete(hookSystem, OnSubPhaseComplete));

            UnityEngine.Debug.LogError("CREATED ALL SUBPHASES");
            UnityEngine.Debug.LogWarning($"Size of the list is: {this.MainPhaseStates.Count}");

            this.currentState = SubPhaseState.NONE;
        }

        public void OnDestroy()
        {
            UnityEngine.Debug.LogError("DESTROYING PHASES");

            int count = this.MainPhaseStates.Count;
            for(int i = 0; i < count; i++) 
            {
                this.MainPhaseStates[(SubPhaseState)i] = null;
            }

            this.MainPhaseStates.Clear();
        }

        public void SwitchSubPhase(SubPhaseState newState, UnitIndex selectedUnitIndex)
        {
            this.MainPhaseStates[this.CurrentSubPhaseState]?.OnExit();

            this.CurrentSubPhaseState = newState;
            this.selectedIndex = selectedUnitIndex;
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
            this.selectedIndex = null;
        }

        public UnitIndex? GetSelectedIndex() => this.selectedIndex;
    }
}