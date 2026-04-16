namespace TurnBased.Phases
{
    public class MainTurnManager
    {
        private static MainTurnManager instance;
        public static MainTurnManager Instance
        {
            get
            {
                return instance;
            }
            set
            {
                if (instance != null)
                {
                    instance = value;
                }
            }
        }

        private System.Collections.Generic.Dictionary<MAIN_TURN_STATE, UnitTurnSubPhase> MainPhaseStates = new();
        private MAIN_TURN_STATE currentState = new();

        public MainTurnManager() 
        {
            /*  Clear the previous Phases for this currentUnit and Initalise them.  */
            this.MainPhaseStates.Clear();

            /*  Add all phases to the dictionary.   */
            this.MainPhaseStates.Add(MAIN_TURN_STATE.IDLE,                      new UnitTurnPhase_Idle());
            this.MainPhaseStates.Add(MAIN_TURN_STATE.AWAITING_MOVE_SELECTION,   new UnitTurnPhase_MoveSelection());
            this.MainPhaseStates.Add(MAIN_TURN_STATE.AWAITING_TARGET_SELECTION, new UnitTurnPhase_TargetSelection());
            this.MainPhaseStates.Add(MAIN_TURN_STATE.READY_TO_EXECUTE_MOVE,     new UnitTurnPhase_ReadyToExecuteMove());
            this.MainPhaseStates.Add(MAIN_TURN_STATE.RESOLVE_ATTACK,            new UnitTurnPhase_ResolveAttack());
            this.MainPhaseStates.Add(MAIN_TURN_STATE.ATTACK_COMPLETE,           new UnitTurnPhase_AttackComplete());

            StationSelectorManager.OnSelectionChange += StationSelectorManager_OnSelectionChange;
        }

        ~MainTurnManager()
        {
            this.MainPhaseStates.Clear();
            StationSelectorManager.OnSelectionChange -= StationSelectorManager_OnSelectionChange;
        }


        private void StationSelectorManager_OnSelectionChange(StationIndex newSelectedStation, StationIndex? oldStation)
        {
            /*  Get the selected Unit's unit Index  */
            if(!StationManager.Instance.TryGetUnitIndexOnStation(newSelectedStation, out UnitIndex unitIndex)) { return; }

            InitaliseMainTurnManagerForUnit(unitIndex);
        }

        private void InitaliseMainTurnManagerForUnit(UnitIndex unitIndex)
        {
            SetActiveSubPhaseBasedOnUnitIntention(unitIndex);
        }

        public void EnterCurrentPhase()
        {
            this.MainPhaseStates[this.currentState]?.OnEnter();
        }

        public void UpdateCurrentPhase()
        {
            this.MainPhaseStates[this.currentState]?.Update();
        }
        public void ExitCurrentPhase()
        {
            this.MainPhaseStates[this.currentState]?.OnExit();
        }
        public void SwitchToNextSubPhase()
        {
            /*  This should assign our current state to the next state in sequence as defined in the Enum.  */
            MAIN_TURN_STATE nextState = (MAIN_TURN_STATE)((int)(this.currentState + 1) % System.Enum.GetValues(typeof(MAIN_TURN_STATE)).Length);

            SwitchSubPhase(nextState);
        }

        private void SetActiveSubPhaseBasedOnUnitIntention(UnitIndex newSelectedUnit)
        {
            /*  Retrieve the Unit Intention.    */
            if (!TurnBased.Intention.UnitIntentionManager.Instance.TryGetIntention(newSelectedUnit, out TurnBased.Intention.UnitIntention intention)) { return; }

            UnityEngine.Debug.Log("Retrieved intent: " + intention.ResolutionState);
            UnitIntentionResolutionState state = intention.ResolutionState;

            UnityEngine.Debug.Log("intent state is: " + state.ToString());


            switch (state)
            {
                case UnitIntentionResolutionState.AWAITING_MOVE_SELECTION:
                    SwitchSubPhase(MAIN_TURN_STATE.AWAITING_MOVE_SELECTION);
                    return;
                case UnitIntentionResolutionState.AWAITING_TARGET_SELECTION:
                    SwitchSubPhase(MAIN_TURN_STATE.AWAITING_TARGET_SELECTION);
                    return;

                case UnitIntentionResolutionState.COMPLETE:
                    SwitchSubPhase(MAIN_TURN_STATE.READY_TO_EXECUTE_MOVE);
                    return;

                case UnitIntentionResolutionState.NONE:
                default:
                    SwitchSubPhase(MAIN_TURN_STATE.IDLE);
                    return; 
            }

        }

        public void SwitchSubPhase(MAIN_TURN_STATE newState)
        {
            this.MainPhaseStates[this.currentState]?.OnExit();

            this.currentState = newState;

            SetSelectedCurrentUnitForSubPhase();

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