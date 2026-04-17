namespace TurnBased.Phases
{
    public class PhaseManager
    {
        private static PhaseManager instance;
        public static PhaseManager Instance
        {
            get
            {
                return instance;
            }
            set
            {
                if(instance != null)
                {
                    instance = value;
                }
            }
        }

        System.Collections.Generic.Dictionary<PHASE_TYPES, Phase> PhaseDictionary = new();
        private PHASE_TYPES CurrentPhaseType;
        private Phase CurrentPhase;

        public void Awake()
        {
            instance = this;
        }

        public void Initialise()
        {
            this.PhaseDictionary = new()
            {
                {PHASE_TYPES.START_ROUND,   new BeginRoundPhase () },
                {PHASE_TYPES.PRE_UNIT_TURN, new PreTurnPhase    () },
                {PHASE_TYPES.UNIT_TURN,     new UnitTurnPhase   () },
                {PHASE_TYPES.END_ROUND,     new EndRoundPhase   () }
            };
            ChangeState(PHASE_TYPES.START_ROUND);
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
