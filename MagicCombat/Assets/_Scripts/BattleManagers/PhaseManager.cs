namespace TurnBased.Phases
{
    public class PhaseManager
    {
        System.Collections.Generic.Dictionary<PHASE_TYPES, Phase> PhaseDictionary = new();
        private PHASE_TYPES CurrentPhaseType;
        private Phase CurrentPhase;

        public PhaseManager() 
        {
            Initialise();
        }

        public void Initialise()
        {
            this.PhaseDictionary = new()
            {
                {PHASE_TYPES.START_ROUND,   new BeginRoundPhase (this) },
                {PHASE_TYPES.PRE_UNIT_TURN, new PreTurnPhase    (this) },
                {PHASE_TYPES.UNIT_TURN,     new UnitTurnPhase   (this) },
                {PHASE_TYPES.END_ROUND,     new EndRoundPhase   (this) }
            };
        }

        public void UpdatePhases()
        {
            this.CurrentPhase?.Update();
        }

        public void ChangeState(PHASE_TYPES newPhaseType)
        {
            this.CurrentPhase?.OnExit();

            this.CurrentPhaseType = newPhaseType;
            this.CurrentPhase = PhaseDictionary[newPhaseType];

            this.CurrentPhase?.OnEnter();
        }

        public void ChangeToNextStateInOrder()
        {
            // Get which state we are in, decide which state is next
            int index = (int)this.CurrentPhaseType;

            index++;

            if (index > this.PhaseDictionary.Count - 1)
            {
                index = 0;
            }

            ChangeState((PHASE_TYPES)index);
        }


    }
}
