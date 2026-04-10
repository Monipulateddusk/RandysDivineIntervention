using System;
using UnityEngine;

namespace TurnBased.Phases
{
    public abstract class UnitTurnSubPhase : Phase
    {
        protected MainTurnManager mainTurnManager;
        protected UnitIndex currentUnitIndex;
        public UnitTurnSubPhase(PhaseManager phaseManager, MainTurnManager mTM) : base(phaseManager)
        {
            this.mainTurnManager = mTM;
        }

        public void SetCurrentUnitIndex(UnitIndex unitIndex) { this.currentUnitIndex = unitIndex; }
    }

    public class UnitTurnPhase_Idle : UnitTurnSubPhase
    {
        private float _enemyTurnTimer;
        public UnitTurnPhase_Idle(PhaseManager phaseManager, MainTurnManager mTM) : base(phaseManager, mTM)
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

                    this.mainTurnManager.SwitchToNextSubPhase();
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
                    this.mainTurnManager.SwitchToNextSubPhase();
                }
            }
        }
    }

    public class UnitTurnPhase_MoveSelection : UnitTurnSubPhase
    {
        public UnitTurnPhase_MoveSelection(PhaseManager phaseManager, MainTurnManager mTM) : base(phaseManager, mTM)
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

        }
    }

    public class UnitTurnPhase_TargetSelection : UnitTurnSubPhase
    {
        public UnitTurnPhase_TargetSelection(PhaseManager phaseManager, MainTurnManager mTM) : base(phaseManager, mTM)
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
            this.mainTurnManager.SwitchToNextSubPhase();
        }
    }


    public class UnitTurnPhase_ReadyToExecuteMove : UnitTurnSubPhase
    {
        public UnitTurnPhase_ReadyToExecuteMove(PhaseManager phaseManager, MainTurnManager mTM) : base(phaseManager, mTM)
        {
        }

        public override void OnEnter()
        {
            throw new NotImplementedException();
        }

        public override void OnExit()
        {
            throw new NotImplementedException();
        }

        public override void Update()
        {
            throw new NotImplementedException();
        }
    }

    public class UnitTurnPhase_ResolveAttack : UnitTurnSubPhase
    {
        public UnitTurnPhase_ResolveAttack(PhaseManager phaseManager, MainTurnManager mTM) : base(phaseManager, mTM)
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
                this.mainTurnManager.SwitchToNextSubPhase();
            }
            else if (Input.GetKey(KeyCode.KeypadEnter))
            {
                this.PhaseManager.ChangeToNextStateInOrder();
            }


        }
    }

    public class UnitTurnPhase_AttackComplete : UnitTurnSubPhase
    {
        public UnitTurnPhase_AttackComplete(PhaseManager phaseManager, MainTurnManager mTM) : base(phaseManager, mTM)
        {
        }

        public override void OnEnter()
        {
            throw new NotImplementedException();
        }

        public override void OnExit()
        {
            throw new NotImplementedException();
        }

        public override void Update()
        {
            throw new NotImplementedException();
        }
    }
}