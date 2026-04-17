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
        }

        private void SubscribeEventsForStartOfRound()
        {
            this.completionManager = new(OnBeginRoundComplete);

            this.completionManager.AddAction();
            Intention.IntentionResolver.OnAllIntentionsProcessed += this.completionManager.OnActionComplete;

            /*  After that, get the Intention of all Enemy Units to reveal that information to the Player.  */
            Intention.IntentionResolver.Instance.DetermineNonPlayerDrivenUnitIntentions();

        }

    }

    public class PreTurnPhase : Phase
    {
        PhaseTaskCompletionManager playerDrivenIntentionsCompletionManager;
        public PreTurnPhase() : base()
        {
        }

        public override void OnEnter()
        {
            MonoBehaviour.print("<color=green>Entering in PreTurnPhase</color>");

            StartPlayerDrivenIntentions();



            /*  Check the intentions when we enter. If everyone has chosen their intentions, proceed to combat. */

            /*  Pop out the next Unit in turn order, move on to the Unit Turn Phase after this. */
            //UnitIndex? unit = TurnOrder.TurnOrderManager.Instance.PopNextUnitInTurnOrder();
            //if (unit != null)
            //{

            //    PhaseManager.Instance.ChangeToNextStateInOrder();
            //}
            ///*  If there is no Unit available in the Turn order, we are at the end of the Turn order and then we want to start the Round anew.  */
            //else
            //{
            //    PhaseManager.Instance.ChangeState(PHASE_TYPES.END_ROUND);
            //}
        }

        public override void OnExit()
        {

        }

        public override void Update()
        {

        }

        private void StartPlayerDrivenIntentions()
        {
            this.playerDrivenIntentionsCompletionManager = new(OnPlayerDrivenIntentionsComplete);

            this.playerDrivenIntentionsCompletionManager.AddAction();
            Intention.IntentionResolver.OnAllIntentionsProcessed += this.playerDrivenIntentionsCompletionManager.OnActionComplete;

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
            }
        }


    }

    public class UnitTurnPhase : Phase
    {
        UnitIndex currentUnitIndex;

        public UnitTurnPhase() : base()
        {
        }

        public override void OnEnter()
        {
            /*  When we enter this Phase, we want to process any Start-Of-Turn Status Effects.  */

            /*  Get the current Unit.   */
            currentUnitIndex = TurnOrder.TurnOrderManager.Instance.GetCurrentUnit();

            /*  INITIALISE THE MAIN TURN MANAGER!!!!!!  */

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