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
        protected System.Action<CombatTurnOrchestrationPhase> OnMainPhaseComplete;
        public MainPhase(System.Action<CombatTurnOrchestrationPhase> onMainPhaseComplete) : base()
        {
            this.OnMainPhaseComplete = onMainPhaseComplete;
        }
    }

    public class BeginBattlePhase : MainPhase
    {
        public BeginBattlePhase(System.Action<CombatTurnOrchestrationPhase> onMainPhaseComplete) : base(onMainPhaseComplete)
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
    }


    public class BeginRoundPhase : MainPhase
    {
        PhaseTaskCompletionManager completionManager;

        public BeginRoundPhase(System.Action<CombatTurnOrchestrationPhase> onMainPhaseComplete) : base(onMainPhaseComplete)
        {
        }

        public override void OnEnter()
        {
            Debug.Log("Entering begin round phase");

            /*  Set the StationSelector to be locked so the player cannot select units while processing initial intentions. */
            StationSelectorManager.Instance.SetSelectorStateLocked();

            InitaliseTurnOrderForTheRound();

            SubscribeEventsForStartOfRound();


        }

        public override void OnExit()
        {
            /*  Reenable selection when we are done processing all Start-of-Round effects and events.   */
            StationSelectorManager.Instance.SetSelectorStateUnlocked();
        }

        public override void Update()
        {

        }

        private void OnBeginRoundComplete()
        {
          //  Intention.IntentionResolver.OnAllIntentionsProcessed -= this.completionManager.OnActionComplete;

            /*  Once everything is done, we want to move onto the next Phase.   */
            PhaseManager.Instance.ChangeToNextStateInOrder();
        }

        private void InitaliseTurnOrderForTheRound()
        {
            TurnOrderCreationState turnOrderCreationState = TurnOrder.TurnOrderManager.Instance.TryCreateNewTurnOrderList(out System.Collections.Generic.List<UnitIndex> newTurnOrderList);

            if (turnOrderCreationState == TurnOrderCreationState.InsufficentUnits) { throw new System.Exception("ERROR — START OF ROUND: INSUSFICIENT UNIT COUNT!"); }
            else if (turnOrderCreationState == TurnOrderCreationState.OldTurnOrderList)
            {
                PhaseManager.Instance.ChangeToNextStateInOrder();
                return;
            }
            TurnOrder.TurnOrderManager.Instance.PopNextUnitInTurnOrder();
        }

        private void SubscribeEventsForStartOfRound()
        {
            this.completionManager = new(OnBeginRoundComplete);

            this.completionManager.AddAction();
            //Intention.IntentionResolver.OnAllIntentionsProcessed += this.completionManager.OnActionComplete;
            //Intention.IntentionResolver.Instance.ResetIntentionResolver();

            /*  After that, get the Intention of all Enemy Units to reveal that information to the Player.  */
            //if (!Intention.IntentionResolver.Instance.DetermineNonPlayerDrivenUnitIntentions())
            //{
            //    OnBeginRoundComplete();
            //}

        }

    }

    public class PreTurnPhase : MainPhase
    {

        public PreTurnPhase(System.Action<CombatTurnOrchestrationPhase> onMainPhaseComplete) : base(onMainPhaseComplete)
        {
        }

        public override void OnEnter()
        {
            MonoBehaviour.print("<color=green>Entering in PreTurnPhase</color>");
            /*  When we enter this Phase, we want to process any Pre-Start-Of-Turn Status Effects.  */



        }

        public override void OnExit()
        {

        }

        public override void Update()
        {

        }




    }

    public class UnitTurnPhase : MainPhase
    {
        PhaseTaskCompletionManager playerDrivenIntentionsCompletionManager;

        public UnitTurnPhase(System.Action<CombatTurnOrchestrationPhase> onMainPhaseComplete) : base(onMainPhaseComplete)
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

            //this.playerDrivenIntentionsCompletionManager.AddAction();
            //Intention.IntentionResolver.OnAllIntentionsProcessed += this.playerDrivenIntentionsCompletionManager.OnActionComplete;
            //Intention.IntentionResolver.Instance.ResetIntentionResolver();

            ///*  When we enter here, try and resolve all Player Driven Intentions. If it was unsucessful, then we just want to proceed to evaluating if all the unit intentions are done.    */
            //if (!Intention.IntentionResolver.Instance.DeterminePlayerDrivenUnitIntentions())
            //{
            //    OnPlayerDrivenIntentionsComplete();
            //}
        }

        private void OnPlayerDrivenIntentionsComplete()
        {
          //  Intention.IntentionResolver.OnAllIntentionsProcessed -= this.playerDrivenIntentionsCompletionManager.OnActionComplete;

            MonoBehaviour.print("<color=blue>DONE INTENTIONS</color>");

            /*  When player driven intentions are done, we want to evaluate if we are entering resolving comabat.   */
            ProcessCombat();
        }

        private void ProcessCombat()
        {
            if (Intention.UnitIntentionManager.Instance.AreUnitIntentionsDone)
            {
                Debug.Log("INTENTIONS ARE DONE!!!! POGGIES!!!");
                /*  Within the MainTurnManager on the Intention Resolver, start the combat allowing each unit to process each of their attacks. */

                this.OnMainPhaseComplete(CombatTurnOrchestrationPhase.PlayerTurn);

                return;
                //PhaseManager.Instance.ChangeSubPhase(SubPhaseState.RESOLVE_ATTACK);
            }
        }
    }

    public class EndRoundPhase : MainPhase
    {
        public EndRoundPhase(System.Action<CombatTurnOrchestrationPhase> onMainPhaseComplete) : base(onMainPhaseComplete)
        {
        }

        public override void OnEnter()
        {
            /*  Clear intents   */
            UnitIntentionManager.Instance.ClearAllUnitIntentions();
         //   Intention.IntentionResolver.Instance.ResetIntentionResolver();

            this.OnMainPhaseComplete(CombatTurnOrchestrationPhase.EndOfRound);     

        }

        public override void OnExit()
        {

        }

        public override void Update()
        {

        }
    }

}