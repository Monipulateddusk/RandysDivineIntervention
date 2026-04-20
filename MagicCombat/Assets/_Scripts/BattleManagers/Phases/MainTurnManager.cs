namespace TurnBased.Phases
{
    public class MainTurnManager
    {
        private readonly System.Collections.Generic.Dictionary<MAIN_TURN_STATE, UnitTurnSubPhase> MainPhaseStates = new();
        private MAIN_TURN_STATE currentState = new();

        public MainTurnManager() 
        {
            /*  Clear the previous Phases for this currentUnit and Initalise them.  */
            this.MainPhaseStates.Clear();

            /*  Add all phases to the dictionary.   */
            this.MainPhaseStates.Add(MAIN_TURN_STATE.AWAITING_MOVE_SELECTION,   new UnitTurnPhase_MoveSelection());
            this.MainPhaseStates.Add(MAIN_TURN_STATE.AWAITING_TARGET_SELECTION, new UnitTurnPhase_TargetSelection());
            this.MainPhaseStates.Add(MAIN_TURN_STATE.READY_TO_EXECUTE_MOVE,     new UnitTurnPhase_ReadyToExecuteMove());
            this.MainPhaseStates.Add(MAIN_TURN_STATE.RESOLVE_ATTACK,            new UnitTurnPhase_ResolveAttack());
            this.MainPhaseStates.Add(MAIN_TURN_STATE.ATTACK_COMPLETE,           new UnitTurnPhase_AttackComplete());
        }

        ~MainTurnManager()
        {
            this.MainPhaseStates.Clear();
        }

        public void SwitchSubPhase(MAIN_TURN_STATE newState)
        {
            this.MainPhaseStates[this.currentState]?.OnExit();

            this.currentState = newState;

            SetSelectedCurrentUnitForSubPhase();
            UnityEngine.Debug.LogWarning($"Switching to Phase: {newState} ");

            this.MainPhaseStates[this.currentState]?.OnEnter();
        }

        private void SetSelectedCurrentUnitForSubPhase()
        {
            /*  Retrieve the selected station.  */
            StationIndex selectedStation = StationSelectorManager.Instance.GetSelectedStationIndex();

            /*  Try and get the Unit index on that station to pass to the subphase we are entering. */
            if(!StationManager.Instance.TryGetUnitIndexOnStation(selectedStation, out UnitIndex unitIndexOnStation)) { return; }

            UnityEngine.Debug.Log($"Selecting current unit for subphase as: {unitIndexOnStation.Index}");

            this.MainPhaseStates[this.currentState]?.SetCurrentUnitIndex(unitIndexOnStation);
        }
    }
}