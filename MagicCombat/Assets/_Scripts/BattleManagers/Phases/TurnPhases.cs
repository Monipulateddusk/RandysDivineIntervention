using UnityEngine;
using System.Collections.Generic;
using System;

namespace TurnBased.Phases
{
    public abstract class Phase
    {
        public PhaseManager PhaseManager;

        public Phase(PhaseManager phaseManager)
        {
            this.PhaseManager = phaseManager;
        }
        public abstract void OnEnter();
        public abstract void Update();
        public abstract void OnExit();
    }

    public class BeginRoundPhase : Phase
    {
        public BeginRoundPhase(PhaseManager phaseManager) : base(phaseManager)
        {
        }

        public override void OnEnter()
        {
            Debug.Log("Entering begin round phase");


            /*  If the turn order list is not empty, move past here.    */
            if (TurnOrder.TurnOrderManager.Instance.GetTurnOrderList().Count < 0)
            {
                this.PhaseManager.ChangeToNextStateInOrder();
                return;
            }

            /*  When we enter this phase, we want to create the Turn Order List awaiting any Tasks that need to be done from external classes.  */
            List<UnitIndex> createdTurnOrder = TurnOrder.TurnOrderManager.Instance.CreateTurnOrderList();

            /*  If the turn order list is less than 0 because there is not enough units to make a turn order with. Stop!!!! */
            if (createdTurnOrder.Count <= 0)
            {
                return;
            }

            /*  After that, get the Intention of all Enemy Units to reveal that information to the Player.  */
            
            if(Intention.IntentionResolver.Instance == null) { Debug.Log("Intention resolver is null BOZO!"); return; }
            Intention.IntentionResolver.Instance.DetermineEnemyUnitIntentions();


            /*  Once everything is done, we want to move onto the next Phase.   */
            this.PhaseManager.ChangeToNextStateInOrder();
        }

        public override void OnExit()
        {

        }

        public override void Update()
        {

        }
    }

    public class PreTurnPhase : Phase
    {
        public PreTurnPhase(PhaseManager phaseManager) : base(phaseManager)
        {
        }

        public override void OnEnter()
        {
            /*  Pop out the next Unit in turn order, move on to the Unit Turn Phase after this. */
            UnitIndex? unit = TurnOrder.TurnOrderManager.Instance.PopNextUnitInTurnOrder();
            if (unit != null)
            {

                this.PhaseManager.ChangeToNextStateInOrder();
            }
            /*  If there is no Unit available in the Turn order, we are at the end of the Turn order and then we want to start the Round anew.  */
            else
            {
                this.PhaseManager.ChangeState(PHASE_TYPES.END_ROUND);
            }
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
        MainTurnManager mainTurnManager;
        UnitIndex currentUnitIndex;

        public UnitTurnPhase(PhaseManager phaseManager) : base(phaseManager)
        {
            this.mainTurnManager = new(phaseManager);
        }

        public override void OnEnter()
        {
            /*  When we enter this Phase, we want to process any Start-Of-Turn Status Effects.  */

            /*  Get the current Unit.   */
            currentUnitIndex = TurnOrder.TurnOrderManager.Instance.GetCurrentUnit();

            /*  INITIALISE THE MAIN TURN MANAGER!!!!!!  */
            mainTurnManager.EnterCurrentPhase();

        }

        public override void OnExit()
        {
            this.mainTurnManager.ExitCurrentPhase();

            /*  When we exit this Phase, we want to process any End-Of-Turn Status Effects.  */

        }

        public override void Update()
        {
            this.mainTurnManager.UpdateCurrentPhase();

            /*  Check if the Unit has an intention already planned. If so, execute it.  */


            /*  Otherwise, we want to create the intention by polling the Unit's Move Selection Component. */


            /*  Once a move is selected, get the Targetting data from that move and proceed to targetting Units for that move. As we are doing this in Update, it makes it easy for us to use a State-Machine to go backwards a step of this Phase. */

            /*  Once we have the move and Targetting data, proceed to processing the move. */

            /*  Once the Move is finished playing, check if the move ends the turn or not. If so, we want to tell the Mediator. If not, we want to just start again from Move selection. */


        }




    }

    public class EndRoundPhase : Phase
    {
        public EndRoundPhase(PhaseManager phaseManager) : base(phaseManager)
        {
        }

        public override void OnEnter()
        {
            /*  Check if the Turn-Order List is empty. If not, we don't want to be here.    */
            if (TurnOrder.TurnOrderManager.Instance.GetTurnOrderList().Count > 0)
            {
                this.PhaseManager.ChangeToNextStateInOrder();
                return;
            }
            /*  If we are supposed to be here. Process any end of round effects. Start the Round anew. */
            else
            {
                this.PhaseManager.ChangeState(PHASE_TYPES.START_ROUND);
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