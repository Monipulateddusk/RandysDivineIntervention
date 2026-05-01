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

        private System.Collections.Generic.Dictionary<CombatTurnOrchestrationPhase, Phase> PhaseDictionary = new();
        private Phase CurrentPhase;

        public void Awake(Intention.CombatRoundUnitIntentionManager cRUIM, System.Action<CombatTurnOrchestrationPhase> onMainPhaseComplete)
        {
            if (instance == null)
            {
                instance = this;
            }

            this.PhaseDictionary = new()
            {
                {CombatTurnOrchestrationPhase.StartOfBattle,    new BeginBattlePhase                (cRUIM, onMainPhaseComplete) },
                {CombatTurnOrchestrationPhase.StartOfRound,     new BeginRoundPhase                 (cRUIM, onMainPhaseComplete) },
                {CombatTurnOrchestrationPhase.PrePlayerTurn,    new PreTurnPhase                    (cRUIM, onMainPhaseComplete) },
                {CombatTurnOrchestrationPhase.PlayerTurn,       new UnitTurnPhase                   (cRUIM, onMainPhaseComplete) },
                {CombatTurnOrchestrationPhase.TurnOrderRes,     new TurnOrderCombatResolutionPhase  (cRUIM, onMainPhaseComplete) },
                {CombatTurnOrchestrationPhase.EndOfRound,       new EndRoundPhase                   (cRUIM, onMainPhaseComplete) },
                {CombatTurnOrchestrationPhase.EndOfBattle,      new EndOfBattlePhase                (cRUIM, onMainPhaseComplete) },
            };
        }

        public void OnDestroy()
        {
            if (instance != null && instance == this)
            {
                UnityEngine.Debug.LogError("Destroying PhaseManager");
                instance = null;
            }

            int phaseCount = this.PhaseDictionary.Count;
            for (int i = 0; i < phaseCount; i++)
            {
                this.PhaseDictionary[(CombatTurnOrchestrationPhase)i] = null;
            }

            this.PhaseDictionary = null;
        }
        public void Update()
        {
            this.CurrentPhase?.Update();
        }

        public void ChangeState(CombatTurnOrchestrationPhase newPhaseType)
        {
            UnityEngine.Debug.LogWarning($"Exiting {this.CurrentPhase}, entering {newPhaseType}");
            this.CurrentPhase?.OnExit();

            CombatTurnOrchestrator.CurrentOrchestrationPhase = newPhaseType;
            this.CurrentPhase = PhaseDictionary[CombatTurnOrchestrator.CurrentOrchestrationPhase];

            this.CurrentPhase?.OnEnter();
        }

        public void ChangeToNextStateInOrder()
        {
            switch (CombatTurnOrchestrator.CurrentOrchestrationPhase)
            {
                case CombatTurnOrchestrationPhase.StartOfBattle:
                    ChangeState(CombatTurnOrchestrationPhase.StartOfRound);
                    break;
                case CombatTurnOrchestrationPhase.StartOfRound:
                    ChangeState(CombatTurnOrchestrationPhase.PrePlayerTurn);
                    break;
                case CombatTurnOrchestrationPhase.PrePlayerTurn:
                    ChangeState(CombatTurnOrchestrationPhase.PlayerTurn);
                    break;
                case CombatTurnOrchestrationPhase.PlayerTurn:
                    ChangeState(CombatTurnOrchestrationPhase.EndOfRound);
                    break;
                case CombatTurnOrchestrationPhase.EndOfRound:
                    ChangeState(CombatTurnOrchestrationPhase.StartOfRound);
                    break;
            }
        }


    }
}
