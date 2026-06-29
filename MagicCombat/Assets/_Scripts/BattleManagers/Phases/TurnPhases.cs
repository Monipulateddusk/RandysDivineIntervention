using UnityEngine;

namespace TurnBased.Phases
{
    public abstract class Phase
    {
        protected EventHookSystem hookSystem;
        protected PhaseTaskCompletionManager completionManager;
        public Phase(EventHookSystem hookSystem)
        {
            this.hookSystem = hookSystem;
        }

        ~Phase()
        {
            this.hookSystem = null;
            this.completionManager = null;
        }
        public abstract void OnEnter();
        public abstract void Update();
        public abstract void OnExit();
        protected abstract void OnEventsComplete();
        protected abstract void OnPhaseComplete();   
    }

    public abstract class MainPhase : Phase
    {
        protected Intention.CombatRoundUnitIntentionManager RoundUnitIntentionManager;

        protected System.Action<CombatTurnOrchestrationPhase> OnMainPhaseComplete;
        public MainPhase(Intention.CombatRoundUnitIntentionManager cRUIM, EventHookSystem hookSystem, System.Action<CombatTurnOrchestrationPhase> onMainPhaseComplete) : base(hookSystem)
        {
            this.OnMainPhaseComplete = onMainPhaseComplete;
            this.RoundUnitIntentionManager = cRUIM;
        }
        ~MainPhase()
        {
            this.OnMainPhaseComplete = null;
            this.RoundUnitIntentionManager = null;
            this.hookSystem = null;
        }
    }

    public class BeginBattlePhase : MainPhase
    {
        public BeginBattlePhase(Intention.CombatRoundUnitIntentionManager cRUIM, EventHookSystem hookSystem, System.Action<CombatTurnOrchestrationPhase> onMainPhaseComplete) : base(cRUIM, hookSystem, onMainPhaseComplete)
        {
            EventHookSystem.OnStartOfBattlePhase += EventHookSystem_OnStartOfBattlePhase;
        }

        ~BeginBattlePhase()
        {
            EventHookSystem.OnStartOfBattlePhase -= EventHookSystem_OnStartOfBattlePhase;
        }

        private void EventHookSystem_OnStartOfBattlePhase(PhaseTaskCompletionManager completionManager)
        {
            this.completionManager = completionManager;

            this.completionManager.AddAction();
        }

        public override void OnEnter()
        {
            this.hookSystem.InvokeStartOfBattle(OnEventsComplete);

            this.completionManager.OnActionComplete();
        }

        public override void OnExit()
        {

        }

        public override void Update()
        {
   
        }
        protected override void OnEventsComplete()
        {
            OnPhaseComplete();
        }

        protected override void OnPhaseComplete()
        {
            this.OnMainPhaseComplete(CombatTurnOrchestrationPhase.StartOfBattle);
        }
    }


    public class BeginRoundPhase : MainPhase
    {
        public BeginRoundPhase(Intention.CombatRoundUnitIntentionManager cRUIM, EventHookSystem hookSystem, System.Action<CombatTurnOrchestrationPhase> onMainPhaseComplete) : base(cRUIM, hookSystem, onMainPhaseComplete)
        {
            EventHookSystem.OnStartOfRoundPhase += EventHookSystem_OnStartOfRoundPhase;
        }

        ~BeginRoundPhase()
        {
            EventHookSystem.OnStartOfRoundPhase -= EventHookSystem_OnStartOfRoundPhase;
        }

        private void EventHookSystem_OnStartOfRoundPhase(PhaseTaskCompletionManager completionManager)
        {
            this.completionManager = completionManager;
            this.completionManager.AddAction();
        }

        public override void OnEnter()
        {
            /*  Set the StationSelector to be locked so the player cannot select units while processing initial intentions. */
            StationSelectorManager.Instance.SetSelectorStateLocked();

            this.hookSystem.InvokeStartOfRound(OnEventsComplete);
            this.completionManager.OnActionComplete();
        }

        public override void OnExit()
        {
            /*  Reenable selection when we are done processing all Start-of-Round effects and events.   */
            StationSelectorManager.Instance.SetSelectorStateUnlocked();

            this.completionManager = null;
            Debug.LogError($"Leaving Begin Round Phase!");
        }

        public override void Update()
        {

        }

        protected override void OnEventsComplete()
        {
            Intention.UnitIntentionManager.Instance.ResetAllActiveUnitIntentions(); 
            InitaliseTurnOrderForTheRound();
            SubscribeEventsForStartOfRound();
        }

        protected override void OnPhaseComplete()
        {
            Intention.CombatRoundUnitIntentionManager.OnAllIntentionsResolved -= OnPhaseComplete;

            /*  Once everything is done, we want to move onto the next Phase.   */
            this.OnMainPhaseComplete(CombatTurnOrchestrationPhase.StartOfRound);
        }

        private void InitaliseTurnOrderForTheRound()
        {
            TurnOrderCreationState turnOrderCreationState = AttackResolution.AttackResolutionManager.Instance.TryCreateNewTurnOrderList(out System.Collections.Generic.List<UnitIndex> newTurnOrderList);

            Debug.LogWarning($"TurnOrder initalisation! State is {turnOrderCreationState}");

            if (turnOrderCreationState == TurnOrderCreationState.InsufficentUnits) { throw new System.Exception("ERROR — START OF ROUND: INSUSFICIENT UNIT COUNT!"); }
            else if (turnOrderCreationState == TurnOrderCreationState.OldTurnOrderList)
            {
                PhaseManager.Instance.ChangeToNextStateInOrder();
                return;
            }
        }

        private void SubscribeEventsForStartOfRound()
        {
            Debug.LogWarning($"Adding events for start of round.");
            this.completionManager.AddAction();
            Intention.CombatRoundUnitIntentionManager.OnAllIntentionsResolved += OnPhaseComplete;

            Debug.LogWarning($"Starting Processing non-player driven unit intentions.");


            this.RoundUnitIntentionManager.ObtainNonPlayerDrivenUnitIntentions();
        }


    }

    public class PreTurnPhase : MainPhase
    {

        public PreTurnPhase(Intention.CombatRoundUnitIntentionManager cRUIM, EventHookSystem hookSystem, System.Action<CombatTurnOrchestrationPhase> onMainPhaseComplete) : base(cRUIM, hookSystem, onMainPhaseComplete)
        {
            EventHookSystem.OnStartOfPrePlayerTurnPhase += EventHookSystem_OnStartOfPrePlayerTurnPhase;
        }

        private void EventHookSystem_OnStartOfPrePlayerTurnPhase(PhaseTaskCompletionManager completionManager)
        {
            this.completionManager = completionManager;
            this.completionManager.AddAction();
        }

        public override void OnEnter()
        {
            MonoBehaviour.print("<color=green>Entering in PreTurnPhase</color>");
            /*  When we enter this Phase, we want to process any Pre-Start-Of-Turn Status Effects.  */

            this.hookSystem.InvokeStartOfPrePlayerTurn(OnEventsComplete);

            this.completionManager.OnActionComplete();
        }

        public override void OnExit()
        {

        }

        public override void Update()
        {

        }

        protected override void OnPhaseComplete()
        {
            this.OnMainPhaseComplete(CombatTurnOrchestrationPhase.PrePlayerTurn);
        }

        protected override void OnEventsComplete()
        {
            OnPhaseComplete();
        }

        ~PreTurnPhase()
        {
            EventHookSystem.OnStartOfPrePlayerTurnPhase -= EventHookSystem_OnStartOfPrePlayerTurnPhase;
            this.OnMainPhaseComplete = null;
            this.RoundUnitIntentionManager = null;
        }
    }

    public class UnitTurnPhase : MainPhase
    {
        public UnitTurnPhase(Intention.CombatRoundUnitIntentionManager cRUIM, EventHookSystem hookSystem, System.Action<CombatTurnOrchestrationPhase> onMainPhaseComplete) : base(cRUIM, hookSystem, onMainPhaseComplete)
        {
            EventHookSystem.OnStartOfPlayerTurnPhase += EventHookSystem_OnStartOfPlayerTurn;
        }
        ~UnitTurnPhase()
        {
            EventHookSystem.OnStartOfPlayerTurnPhase -= EventHookSystem_OnStartOfPlayerTurn;
        }

        private void EventHookSystem_OnStartOfPlayerTurn(PhaseTaskCompletionManager completionManager)
        {
            this.completionManager = completionManager;
            this.completionManager.AddAction();

        }

        public override void OnEnter()
        {
            /*  Notify the Intention Resolver to begin processing Player Driven Intentions. */
            this.hookSystem.InvokeStartOfPlayerTurn(OnEventsComplete);
            this.completionManager.OnActionComplete();
        }

        public override void OnExit()
        {
            /*  When we exit this Phase, we want to process any End-Of-Turn Status Effects.  */

        }

        public override void Update()
        {
        }
        protected override void OnEventsComplete()
        {
            StartPlayerDrivenIntentions();
        }

        /// <summary>
        /// Called upon entering this Phase. Processes the Player Driven Intentions of Units. 
        /// 
        /// Once done, Processes combat within the Subphase.
        /// </summary>
        private void StartPlayerDrivenIntentions()
        {
            Intention.CombatRoundUnitIntentionManager.OnAllIntentionsResolved += OnPhaseComplete;
            this.RoundUnitIntentionManager.ObtainPlayerDrivenUnitIntentions();
        }
        protected override void OnPhaseComplete()
        {
            Intention.CombatRoundUnitIntentionManager.OnAllIntentionsResolved -= OnPhaseComplete;

            /*  Within the MainTurnManager on the Intention Resolver, start the combat allowing each unit to process each of their attacks. */
            this.OnMainPhaseComplete(CombatTurnOrchestrationPhase.PlayerTurn);
        }

    }

    public class TurnOrderCombatResolutionPhase : MainPhase
    {
        public TurnOrderCombatResolutionPhase(Intention.CombatRoundUnitIntentionManager cRUIM, EventHookSystem hookSystem, System.Action<CombatTurnOrchestrationPhase> onMainPhaseComplete) : base(cRUIM, hookSystem, onMainPhaseComplete)
        {
            EventHookSystem.OnResolvingTurnOrderPhase += EventHookSystem_OnResolvingTurnOrder;
        }

        ~TurnOrderCombatResolutionPhase()
        {
            EventHookSystem.OnResolvingTurnOrderPhase -= EventHookSystem_OnResolvingTurnOrder;
        }

        private void EventHookSystem_OnResolvingTurnOrder(PhaseTaskCompletionManager completionManager)
        {
            this.completionManager = completionManager;
            this.completionManager.AddAction();
        }

        public override void OnEnter()
        {
            /*  The Combat Round Intention Manager should be the one to put us into the resolve attack sub phase.   */
            this.hookSystem.InvokeTurnOrderResolving(OnEventsComplete);
            this.completionManager.OnActionComplete();
        }

        public override void OnExit()
        {

        }

        public override void Update()
        {
         
        }
        protected override void OnEventsComplete()
        {
            UnityEngine.Debug.LogError($"WE ARE STARTING THE TURN ORDER RESOLUTION PHASE! WEE WOO!");

            AttackResolution.AttackResolutionManager.Instance.StartCombatResolution(OnPhaseComplete);
        }

        protected override void OnPhaseComplete()
        {
            UnityEngine.Debug.LogError($"WE ARE LEAVING THE TURN ORDER RESOLUTION PHASE! WEE WOO!");

            this.OnMainPhaseComplete(CombatTurnOrchestrationPhase.TurnOrderRes);
        }
    }



    public class EndRoundPhase : MainPhase
    {
        public EndRoundPhase(Intention.CombatRoundUnitIntentionManager cRUIM, EventHookSystem hookSystem, System.Action<CombatTurnOrchestrationPhase> onMainPhaseComplete) : base(cRUIM, hookSystem, onMainPhaseComplete)
        {
            EventHookSystem.OnEndOfRoundPhase += EventHookSystem_OnEndOfRound;
        }
        ~EndRoundPhase()
        {
            EventHookSystem.OnEndOfRoundPhase -= EventHookSystem_OnEndOfRound;
        }

        private void EventHookSystem_OnEndOfRound(PhaseTaskCompletionManager completionManager)
        {
            this.completionManager = completionManager;
            this.completionManager.AddAction();
        }

        public override void OnEnter()
        {
            UnityEngine.Debug.LogWarning($"Ending end of round.   ");
            this.hookSystem.InvokeEndOfRound(OnEventsComplete);
            this.completionManager.OnActionComplete();
        }

        public override void OnExit()
        {
      
        }

        public override void Update()
        {

        }
        protected override void OnEventsComplete()
        {
            OnPhaseComplete();
        }

        protected override void OnPhaseComplete()
        {
            this.OnMainPhaseComplete(CombatTurnOrchestrationPhase.EndOfRound);
        }
    }

    public class EndOfBattlePhase : MainPhase
    {
        public EndOfBattlePhase(Intention.CombatRoundUnitIntentionManager cRUIM, EventHookSystem hookSystem, System.Action<CombatTurnOrchestrationPhase> onMainPhaseComplete) : base(cRUIM, hookSystem, onMainPhaseComplete)
        {
            EventHookSystem.OnEndOfBattlePhase += EventHookSystem_OnEndOfBattle;
        }
        ~EndOfBattlePhase()
        {
            EventHookSystem.OnEndOfBattlePhase -= EventHookSystem_OnEndOfBattle;
        }

        private void EventHookSystem_OnEndOfBattle(PhaseTaskCompletionManager completionManager)
        {
            this.completionManager = completionManager;
            this.completionManager.AddAction();
        }

        public override void OnEnter()
        {
            this.hookSystem.InvokeEndOfBattle(OnEventsComplete);
            this.completionManager.OnActionComplete();
        }

        public override void OnExit()
        {
  
        }

        public override void Update()
        {

        }
        protected override void OnEventsComplete()
        {
            OnPhaseComplete();
        }

        protected override void OnPhaseComplete()
        {
            UnityEngine.Debug.Log("END OF BATTLE!");
        }
    }
}