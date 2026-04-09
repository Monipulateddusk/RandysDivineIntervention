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
            /*  If the turn order list is not empty, move past here.    */
            if (TurnOrder.TurnOrderManager.Instance.GetTurnOrderList().Count < 0)
            {
                this.PhaseManager.ChangeToNextStateInOrder();
                return;
            }

            /*  When we enter this phase, we want to create the Turn Order List awaiting any Tasks that need to be done from external classes.  */
            TurnOrder.TurnOrderManager.Instance.CreateTurnOrderList();

            /*  If the turn order list is less than 0 because there is not enough units to make a turn order with. Stop!!!! */
            if (TurnOrder.TurnOrderManager.Instance.GetTurnOrderList().Count >= 0)
            {
                return;
            }

            /*  After that, get the Intention of all Enemy Units to reveal that information to the Player.  */

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

    public abstract class UnitTurnSubPhase : Phase
    {
        protected UnitIndex currentUnitIndex;
        protected UnitTurnPhase UnitTurnPhase_main;
        public UnitTurnSubPhase(PhaseManager phaseManager, UnitTurnPhase unitTurnPhase_main, UnitIndex currentUnit) : base(phaseManager)
        {
            this.currentUnitIndex = currentUnit;
            this.UnitTurnPhase_main = unitTurnPhase_main;
        }
    }

    public class UnitTurnPhase_Idle : UnitTurnSubPhase
    {
        private float _enemyTurnTimer;
        public UnitTurnPhase_Idle(PhaseManager phaseManager, UnitTurnPhase UnitTurnPhase_main, UnitIndex currentUnit) : base(phaseManager, UnitTurnPhase_main, currentUnit)
        {
        }

        public override void OnEnter()
        {
            _enemyTurnTimer = 2.0f;
        }

        public override void OnExit()
        {

        }

        public override void Update()
        {
            //MonoBehaviour.print("<color=yellow>Idle for </color>" + ConcreteMediator.GetBattleUnitOfUnitIndex(currentUnitIndex).name);
            if (StationManager.Instance.GetUnitTeamOfIndex(currentUnitIndex) == UnitTeam.ALLY)
            {
                if (Input.GetKeyDown(KeyCode.Escape))
                {

                    UnitTurnPhase_main.SwitchToNextSubPhase();
                }

            }
            else if (StationManager.Instance.GetUnitTeamOfIndex(currentUnitIndex) == UnitTeam.ENEMY)
            {
                //Debug.LogWarning("EnemyTurnTimer is: " + _enemyTurnTimer);
                _enemyTurnTimer -= Time.deltaTime;
                if (_enemyTurnTimer <= 0)
                {
                    //MonoBehaviour.print("<color=yellow>Idle</color>");
                    //Debug.LogWarning("Processing Update in IDLE SubPhase for Unit named: " + ConcreteMediator.GetBattleUnitOfUnitIndex(currentUnitIndex).gameObject.name);
                    UnitTurnPhase_main.SwitchToNextSubPhase();
                }
            }
        }
    }

    public class UnitTurnPhase_MoveSelection : UnitTurnSubPhase
    {
        public UnitTurnPhase_MoveSelection(PhaseManager phaseManager, UnitTurnPhase UnitTurnPhase_main, UnitIndex currentUnit) : base(phaseManager, UnitTurnPhase_main, currentUnit)
        {
        }


        public override void OnEnter()
        {
            /*  Get the Current Unit's Move Selection Intention.    */
            //SceneData_UnitTurn sceneData = ConcreteMediator.GetCombatSceneDataForSourceUnitIndex(currentUnitIndex);
            //MoveSelectionData moveSelectionData = ConcreteMediator.GetBattleUnitOfUnitIndex(currentUnitIndex).GetMoveSelectorComponent().SelectMove(sceneData);

            //if (moveSelectionData.SelectedMove != null)
            //{

            //    MonoBehaviour.print("Unit of name: " + ConcreteMediator.GetBattleUnitOfUnitIndex(currentUnitIndex).name + " has chosen move: " + moveSelectionData.SelectedMove.ToString());
            //}
        }

        public override void OnExit()
        {

        }

        public override void Update()
        {
            //MonoBehaviour.print("<color=purple>MoveSelection</color>");
            UnitTurnPhase_main.SwitchToNextSubPhase();
        }
    }

    public class UnitTurnPhase_TargetSelection : UnitTurnSubPhase
    {
        public UnitTurnPhase_TargetSelection(PhaseManager phaseManager, UnitTurnPhase unitTurnPhase_main, UnitIndex currentUnit) : base(phaseManager, unitTurnPhase_main, currentUnit)
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
            //MonoBehaviour.print("<color=pink>TargetSelection</color>");
            UnitTurnPhase_main.SwitchToNextSubPhase();
        }
    }

    public class UnitTurnPhase_ResolveAttack : UnitTurnSubPhase
    {
        public UnitTurnPhase_ResolveAttack(PhaseManager phaseManager, UnitTurnPhase unitTurnPhase_main, UnitIndex currentUnit) : base(phaseManager, unitTurnPhase_main, currentUnit)
        {

        }

        public override void OnEnter()
        {
            //MonoBehaviour.print("<color=green>ResolveAttack</color>");
        }

        public override void OnExit()
        {

        }

        public override void Update()
        {
            if (Input.GetKey(KeyCode.Backspace))
            {
                UnitTurnPhase_main.SwitchToNextSubPhase();
            }
            else if (Input.GetKey(KeyCode.KeypadEnter))
            {
                this.PhaseManager.ChangeToNextStateInOrder();
            }


        }
    }

    public class UnitTurnPhase : Phase
    {
        private Dictionary<MAIN_TURN_STATE, Phase> MainPhaseStates = new();
        private MAIN_TURN_STATE currentState = new();

        UnitIndex currentUnitIndex;

        public UnitTurnPhase(PhaseManager phaseManager) : base(phaseManager)
        {
        }

        public override void OnEnter()
        {
            /*  When we enter this Phase, we want to process any Start-Of-Turn Status Effects.  */

            /*  Get the current Unit.   */
            currentUnitIndex = TurnOrder.TurnOrderManager.Instance.GetCurrentUnit(); 

            /*  Clear the previous Phases for this currentUnit and Initalise them.  */
            MainPhaseStates.Clear();

            MainPhaseStates.Add(MAIN_TURN_STATE.IDLE, new UnitTurnPhase_Idle(this.PhaseManager, this, currentUnitIndex));
            MainPhaseStates.Add(MAIN_TURN_STATE.MOVE_SELECTION, new UnitTurnPhase_MoveSelection(this.PhaseManager, this, currentUnitIndex));
            MainPhaseStates.Add(MAIN_TURN_STATE.TARGET_SELECTION, new UnitTurnPhase_TargetSelection(this.PhaseManager, this, currentUnitIndex));
            MainPhaseStates.Add(MAIN_TURN_STATE.RESOLVE_ATTACK, new UnitTurnPhase_ResolveAttack(this.PhaseManager, this, currentUnitIndex));

            SwitchSubPhase(MAIN_TURN_STATE.IDLE);
        }

        public override void OnExit()
        {
            MainPhaseStates[currentState]?.OnExit();

            /*  When we exit this Phase, we want to process any End-Of-Turn Status Effects.  */

        }

        public override void Update()
        {
            MainPhaseStates[currentState]?.Update();

            /*  Check if the Unit has an intention already planned. If so, execute it.  */


            /*  Otherwise, we want to create the intention by polling the Unit's Move Selection Component. */


            /*  Once a move is selected, get the Targetting data from that move and proceed to targetting Units for that move. As we are doing this in Update, it makes it easy for us to use a State-Machine to go backwards a step of this Phase. */

            /*  Once we have the move and Targetting data, proceed to processing the move. */

            /*  Once the Move is finished playing, check if the move ends the turn or not. If so, we want to tell the Mediator. If not, we want to just start again from Move selection. */


        }

        public void SwitchToNextSubPhase()
        {
            /*  This should assign our current state to the next state in sequence as defined in the Enum.  */
            MAIN_TURN_STATE nextState = (MAIN_TURN_STATE)((int)(currentState + 1) % Enum.GetValues(typeof(MAIN_TURN_STATE)).Length);

            SwitchSubPhase(nextState);
        }

        public void SwitchSubPhase(MAIN_TURN_STATE newState)
        {
            MainPhaseStates[currentState]?.OnExit();

            currentState = newState;

            MainPhaseStates[currentState]?.OnEnter();
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