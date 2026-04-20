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

    public class BeginRoundPhase : Phase
    {
        PhaseTaskCompletionManager completionManager;

        public BeginRoundPhase() : base()
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
            Intention.IntentionResolver.OnAllIntentionsProcessed -= this.completionManager.OnActionComplete;

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
            Intention.IntentionResolver.OnAllIntentionsProcessed += this.completionManager.OnActionComplete;
            Intention.IntentionResolver.Instance.ResetIntentionResolver();

            /*  After that, get the Intention of all Enemy Units to reveal that information to the Player.  */
            if (!Intention.IntentionResolver.Instance.DetermineNonPlayerDrivenUnitIntentions())
            {
                OnBeginRoundComplete();
            }

        }

    }

    public class PreTurnPhase : Phase
    {

        public PreTurnPhase() : base()
        {
        }

        public override void OnEnter()
        {
            MonoBehaviour.print("<color=green>Entering in PreTurnPhase</color>");
            /*  When we enter this Phase, we want to process any Pre-Start-Of-Turn Status Effects.  */


            /*  Once done, proceed to the main phase.   */
            PhaseManager.Instance.ChangeToNextStateInOrder();
        }

        public override void OnExit()
        {

        }

        public override void Update()
        {

        }




    }

    public class UnitTurnPhase : Phase
    {
        PhaseTaskCompletionManager playerDrivenIntentionsCompletionManager;

        public UnitTurnPhase() : base()
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

            /*  Check if the Unit has an intention already planned. If so, execute it.  */


            /*  Otherwise, we want to create the intention by polling the Unit's Move Selection Component. */


            /*  Once a move is selected, get the Targetting data from that move and proceed to targetting Units for that move. As we are doing this in Update, it makes it easy for us to use a State-Machine to go backwards a step of this Phase. */

            /*  Once we have the move and Targetting data, proceed to processing the move. */

            /*  Once the Move is finished playing, check if the move ends the turn or not. If so, we want to tell the Mediator. If not, we want to just start again from Move selection. */


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
            Intention.IntentionResolver.OnAllIntentionsProcessed += this.playerDrivenIntentionsCompletionManager.OnActionComplete;
            Intention.IntentionResolver.Instance.ResetIntentionResolver();

            /*  When we enter here, try and resolve all Player Driven Intentions. If it was unsucessful, then we just want to proceed to evaluating if all the unit intentions are done.    */
            if (!Intention.IntentionResolver.Instance.DeterminePlayerDrivenUnitIntentions())
            {
                OnPlayerDrivenIntentionsComplete();
            }
        }

        private void OnPlayerDrivenIntentionsComplete()
        {
            Intention.IntentionResolver.OnAllIntentionsProcessed -= this.playerDrivenIntentionsCompletionManager.OnActionComplete;

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
            }
        }
    }

    public class EndRoundPhase : Phase
    {
        public EndRoundPhase() : base()
        {
        }

        public override void OnEnter()
        {
            /*  Check if the Turn-Order List is empty. If not, we don't want to be here.    */
            if (TurnOrder.TurnOrderManager.Instance.GetTurnOrderList().Count > 0)
            {
                PhaseManager.Instance.ChangeToNextStateInOrder();
                return;
            }
            /*  If we are supposed to be here. Process any end of round effects. Start the Round anew. */
            else
            {
                PhaseManager.Instance.ChangeState(PHASE_TYPES.START_ROUND);
            }

        }

        public override void OnExit()
        {

        }

        public override void Update()
        {

        }
    }

}