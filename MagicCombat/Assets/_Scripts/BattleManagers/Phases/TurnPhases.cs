using System;
using TurnBased.Health;
using TurnBased.Intention;
using UnityEngine;

namespace TurnBased.Phases
{
    public abstract class Phase
    {
        public Phase()
        {
        }
        public abstract void OnEnter();
        public abstract void Update();
        public abstract void OnExit();
    }

    public abstract class MainPhase : Phase
    {
        protected Intention.CombatRoundUnitIntentionManager RoundUnitIntentionManager;

        protected System.Action<CombatTurnOrchestrationPhase> OnMainPhaseComplete;
        public MainPhase(Intention.CombatRoundUnitIntentionManager cRUIM, System.Action<CombatTurnOrchestrationPhase> onMainPhaseComplete) : base()
        {
            this.OnMainPhaseComplete = onMainPhaseComplete;
            this.RoundUnitIntentionManager = cRUIM;
        }
        ~MainPhase()
        {
            this.OnMainPhaseComplete = null;
            this.RoundUnitIntentionManager = null;
        }
    }

    public class BeginBattlePhase : MainPhase
    {
        public BeginBattlePhase(Intention.CombatRoundUnitIntentionManager cRUIM, System.Action<CombatTurnOrchestrationPhase> onMainPhaseComplete) : base(cRUIM, onMainPhaseComplete)
        {
        }

        public override void OnEnter()
        {
            this.OnMainPhaseComplete(CombatTurnOrchestrationPhase.StartOfBattle);
        }

        public override void OnExit()
        {

        }

        public override void Update()
        {
   
        }

        ~BeginBattlePhase()
        {
            this.OnMainPhaseComplete = null;
            this.RoundUnitIntentionManager = null;
        }
    }


    public class BeginRoundPhase : MainPhase
    {
        PhaseTaskCompletionManager completionManager;

        public BeginRoundPhase(Intention.CombatRoundUnitIntentionManager cRUIM, System.Action<CombatTurnOrchestrationPhase> onMainPhaseComplete) : base(cRUIM, onMainPhaseComplete)
        {
        }

        public override void OnEnter()
        {
            Debug.LogWarning("Entering begin round phase");

            /*  Set the StationSelector to be locked so the player cannot select units while processing initial intentions. */
            StationSelectorManager.Instance.SetSelectorStateLocked();

            InitaliseTurnOrderForTheRound();

            SubscribeEventsForStartOfRound();


        }

        public override void OnExit()
        {
            /*  Reenable selection when we are done processing all Start-of-Round effects and events.   */
            StationSelectorManager.Instance.SetSelectorStateUnlocked();

            Debug.LogError($"Leaving Begin Round Phase!");
        }

        public override void Update()
        {

        }

        private void InitaliseTurnOrderForTheRound()
        {
            TurnOrderCreationState turnOrderCreationState = TurnOrder.TurnOrderManager.Instance.TryCreateNewTurnOrderList(out System.Collections.Generic.List<UnitIndex> newTurnOrderList);

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

            this.completionManager = new(OnBeginRoundComplete);

            Debug.LogWarning($"Created completion manager.");

            this.completionManager.AddAction();

            Debug.LogWarning($"Added action.");


            Intention.CombatRoundUnitIntentionManager.OnAllIntentionsResolved += this.completionManager.OnActionComplete;

            Debug.LogWarning($"Starting Processing non-player driven unit intentions.");


            this.RoundUnitIntentionManager.ObtainNonPlayerDrivenUnitIntentions();
        }


        private void OnBeginRoundComplete()
        {
            Intention.CombatRoundUnitIntentionManager.OnAllIntentionsResolved -= this.completionManager.OnActionComplete;

            /*  Once everything is done, we want to move onto the next Phase.   */
            this.OnMainPhaseComplete(CombatTurnOrchestrationPhase.StartOfRound); 
        }
        ~BeginRoundPhase()
        {
            this.OnMainPhaseComplete = null;
            this.RoundUnitIntentionManager = null;
        }
    }

    public class PreTurnPhase : MainPhase
    {

        public PreTurnPhase(Intention.CombatRoundUnitIntentionManager cRUIM, System.Action<CombatTurnOrchestrationPhase> onMainPhaseComplete) : base(cRUIM, onMainPhaseComplete)
        {
        }

        public override void OnEnter()
        {
            MonoBehaviour.print("<color=green>Entering in PreTurnPhase</color>");
            /*  When we enter this Phase, we want to process any Pre-Start-Of-Turn Status Effects.  */

            this.OnMainPhaseComplete(CombatTurnOrchestrationPhase.PrePlayerTurn);


        }

        public override void OnExit()
        {

        }

        public override void Update()
        {

        }

        ~PreTurnPhase()
        {
            this.OnMainPhaseComplete = null;
            this.RoundUnitIntentionManager = null;
        }


    }

    public class UnitTurnPhase : MainPhase
    {
        PhaseTaskCompletionManager playerDrivenIntentionsCompletionManager;

        public UnitTurnPhase(Intention.CombatRoundUnitIntentionManager cRUIM, System.Action<CombatTurnOrchestrationPhase> onMainPhaseComplete) : base(cRUIM, onMainPhaseComplete)
        {
        }

        public override void OnEnter()
        {
            /*  Notify the Intention Resolver to begin processing Player Driven Intentions. */
            StartPlayerDrivenIntentions();

        }

        public override void OnExit()
        {
            /*  When we exit this Phase, we want to process any End-Of-Turn Status Effects.  */

        }

        public override void Update()
        {
        }


        /// <summary>
        /// Called upon entering this Phase. Processes the Player Driven Intentions of Units. 
        /// 
        /// Once done, Processes combat within the Subphase.
        /// </summary>
        private void StartPlayerDrivenIntentions()
        {
            this.playerDrivenIntentionsCompletionManager = new(OnPlayerDrivenIntentionsComplete);

            this.playerDrivenIntentionsCompletionManager.AddAction();
            Intention.CombatRoundUnitIntentionManager.OnAllIntentionsResolved += this.playerDrivenIntentionsCompletionManager.OnActionComplete;
            this.RoundUnitIntentionManager.ObtainPlayerDrivenUnitIntentions();
        }

        private void OnPlayerDrivenIntentionsComplete()
        {
            Intention.CombatRoundUnitIntentionManager.OnAllIntentionsResolved -= this.playerDrivenIntentionsCompletionManager.OnActionComplete;

            Debug.LogWarning("DONE PLAYER INTENTIONS");

            /*  When player driven intentions are done, we want to evaluate if we are entering resolving comabat.   */
            ProcessCombat();
        }

        private void ProcessCombat()
        {
            Debug.LogWarning("INTENTIONS ARE DONE!!!! POGGIES!!!");

            /*  Within the MainTurnManager on the Intention Resolver, start the combat allowing each unit to process each of their attacks. */
            this.OnMainPhaseComplete(CombatTurnOrchestrationPhase.PlayerTurn);
            return;
        }
        ~UnitTurnPhase()
        {
            this.OnMainPhaseComplete = null;
            this.RoundUnitIntentionManager = null;
        }
    }

    public class TurnOrderCombatResolutionPhase : MainPhase
    {
        PhaseTaskCompletionManager turnOrderCombatCompletionManager;

        public TurnOrderCombatResolutionPhase(CombatRoundUnitIntentionManager cRUIM, Action<CombatTurnOrchestrationPhase> onMainPhaseComplete) : base(cRUIM, onMainPhaseComplete)
        {
        }

        public override void OnEnter()
        {
            StationSelectorManager.Instance.SetSelectorStateLocked();

            /*  The Combat Round Intention Manager should be the one to put us into the resolve attack sub phase.   */
            this.turnOrderCombatCompletionManager = new(OnTurnOrderCombatFullyResolving);
            this.turnOrderCombatCompletionManager.AddAction();

            Combat.TurnOrderCombatHandler.OnTurnOrderAttacksFullyResolved += this.turnOrderCombatCompletionManager.OnActionComplete;

            Combat.TurnOrderCombatHandler.Instance.StartTurnOrderCombat();
        }

        public override void OnExit()
        {
            Combat.TurnOrderCombatHandler.OnTurnOrderAttacksFullyResolved -= this.turnOrderCombatCompletionManager.OnActionComplete;
        }

        public override void Update()
        {
         
        }

        private void OnTurnOrderCombatFullyResolving()
        {
            this.OnMainPhaseComplete(CombatTurnOrchestrationPhase.TurnOrderRes);
        }
        ~TurnOrderCombatResolutionPhase()
        {
            this.OnMainPhaseComplete = null;
            this.RoundUnitIntentionManager = null;
        }
    }



    public class EndRoundPhase : MainPhase
    {
        public EndRoundPhase(Intention.CombatRoundUnitIntentionManager cRUIM, System.Action<CombatTurnOrchestrationPhase> onMainPhaseComplete) : base(cRUIM, onMainPhaseComplete)
        {
        }

        public override void OnEnter()
        {
            UnityEngine.Debug.LogWarning($"Entering end of round phase. ");


            UnityEngine.Debug.LogWarning($"Ending end of round.   ");
            this.OnMainPhaseComplete(CombatTurnOrchestrationPhase.EndOfRound);     

        }

        public override void OnExit()
        {
            StationSelectorManager.Instance.SetSelectorStateUnlocked();
        }

        public override void Update()
        {

        }

        ~EndRoundPhase()
        {
            this.OnMainPhaseComplete = null;
            this.RoundUnitIntentionManager = null;
        }
    }

    public class EndOfBattlePhase : MainPhase
    {
        public EndOfBattlePhase(CombatRoundUnitIntentionManager cRUIM, Action<CombatTurnOrchestrationPhase> onMainPhaseComplete) : base(cRUIM, onMainPhaseComplete)
        {
        }

        public override void OnEnter()
        {
            
        }

        public override void OnExit()
        {
  
        }

        public override void Update()
        {

        }
        ~EndOfBattlePhase()
        {
            this.OnMainPhaseComplete = null;
            this.RoundUnitIntentionManager = null;
        }
    }

}