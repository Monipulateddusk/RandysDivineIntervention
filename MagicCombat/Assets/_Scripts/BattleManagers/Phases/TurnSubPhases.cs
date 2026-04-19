using UnityEngine;

namespace TurnBased.Phases
{
    public abstract class UnitTurnSubPhase : Phase
    {
        protected UnitIndex currentUnitIndex;
        public UnitTurnSubPhase() : base()
        {
        }

        public void SetCurrentUnitIndex(UnitIndex unitIndex) { this.currentUnitIndex = unitIndex; }
    }

    public class UnitTurnPhase_Idle : UnitTurnSubPhase
    {
        public UnitTurnPhase_Idle() : base()
        {
        }

        public override void OnEnter()
        {
            UnityEngine.Debug.Log($"Entering IDLE! TRYING TO GET UNIT INTENTIONS!");

            /*  Retrieve the Unit Intention for this Unit to determine which phase of the Intention we are. */
            if (!Intention.UnitIntentionManager.Instance.TryGetIntention(currentUnitIndex, out Intention.UnitIntention unitIntention)) { UnityEngine.Debug.LogWarning($"UNABLE TO GET INTENTIONS!"); return;  }

            UnityEngine.Debug.Log($"Obtained UNIT INTENTIONS!");

            switch (unitIntention.ResolutionState)
            {
                case UnitIntentionResolutionState.NONE:
                    UnityEngine.Debug.Log("No state?");

                    return;
                case UnitIntentionResolutionState.AWAITING_MOVE_SELECTION:
                    UnityEngine.Debug.Log($"Selecting move for unitIndex: {currentUnitIndex.Index}");
                    Intention.IntentionResolver.Instance.SwitchSubPhase(MAIN_TURN_STATE.AWAITING_MOVE_SELECTION);
                    return;
                case UnitIntentionResolutionState.AWAITING_TARGET_SELECTION:
                    UnityEngine.Debug.Log($"Selecting target for unitIndex: {currentUnitIndex.Index}");
                    Intention.IntentionResolver.Instance.SwitchSubPhase(MAIN_TURN_STATE.AWAITING_TARGET_SELECTION);

                    return;
                case UnitIntentionResolutionState.COMPLETE:
                    UnityEngine.Debug.Log($"Complete? for unitIndex: {currentUnitIndex.Index}");
                    Intention.IntentionResolver.Instance.SwitchSubPhase(MAIN_TURN_STATE.READY_TO_EXECUTE_MOVE);
                    return;
            }
        }

        public override void OnExit()
        {

        }

        public override void Update()
        {
            
        }
    }

    public class UnitTurnPhase_MoveSelection : UnitTurnSubPhase
    {
        public UnitTurnPhase_MoveSelection() : base()
        {
        }


        public override void OnEnter()
        {
            Debug.Log($"Entering : MoveSelection for UnitIndex: {currentUnitIndex.Index}");

            Intention.MoveSelectionResolver.ProcessIntentionMoveSelection(this.currentUnitIndex);
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
        public UnitTurnPhase_TargetSelection() : base()
        {
        }

        public override void OnEnter()
        {
            Debug.Log($"Entering : TargetSelection for UnitIndex: {currentUnitIndex.Index}");
            Intention.TargetSelectionResolver.ProcessIntentionTargetSelection(this.currentUnitIndex);
        }

        public override void OnExit()
        {

        }

        public override void Update()
        {
            //MonoBehaviour.print("<color=pink>TargetSelection</color>");
         //   this.mainTurnManager.SwitchToNextSubPhase();
        }
    }


    public class UnitTurnPhase_ReadyToExecuteMove : UnitTurnSubPhase
    {
        public UnitTurnPhase_ReadyToExecuteMove() : base()
        {
        }

        public override void OnEnter()
        {
            Debug.Log($"Entering : READY_TO_EXECUTE_MOVE");

            if(Intention.IntentionResolver.Instance.GetQueueCount() > 0)
            {
                Intention.IntentionResolver.Instance.ProcessUnit(this.currentUnitIndex);

            }
            else
            {
                /*  
                 *  If we have no more Intentions to resolve, check to see if all intents have been filled out. If so, proceed to combat. 
                 *  If not, we are likely in the StartOfRound Phase and so we want to move onto the Unit Turn Phase to proceed with Player-Driven Input.    
                 */
                if (!Intention.UnitIntentionManager.Instance.AreUnitIntentionsDone)
                {
                    PhaseManager.Instance.ChangeToNextStateInOrder();
                }
                else
                {
                    Debug.Log("No one left to resolve. Switching to Resolve Attack");
                    Intention.IntentionResolver.Instance.SwitchSubPhase(MAIN_TURN_STATE.RESOLVE_ATTACK);
                }
            }         
        }

        public override void OnExit()
        {

        }

        public override void Update()
        {
           // throw new NotImplementedException();
        }
    }

    public class UnitTurnPhase_ResolveAttack : UnitTurnSubPhase
    {
        PhaseTaskCompletionManager resolveAttackResolutionCompletionManager;

        public UnitTurnPhase_ResolveAttack() : base()
        {

        }

        public override void OnEnter()
        {
            MonoBehaviour.print("<color=green>Entering in ResolveAttack</color>");

            this.resolveAttackResolutionCompletionManager = new(OnAttackResolutionPhaseComplete);

            this.resolveAttackResolutionCompletionManager.AddAction();
            AttackResolution.AttackResolutionManager.OnAllAttacksFullyResolved += this.resolveAttackResolutionCompletionManager.OnActionComplete;
            AttackResolution.AttackResolutionManager.Instance.StartCombatResolution();

        }

        public override void OnExit()
        {

        }

        public override void Update()
        {

        }


        private void OnAttackResolutionPhaseComplete()
        {
            /*  Once all attacks are done. Go to the Attack complete subphase for any triggers if implemented.  */
            Intention.IntentionResolver.Instance.SwitchSubPhase(MAIN_TURN_STATE.ATTACK_COMPLETE);
        }

    }

    public class UnitTurnPhase_AttackComplete : UnitTurnSubPhase
    {
        public UnitTurnPhase_AttackComplete() : base()
        {
        }

        public override void OnEnter()
        {
            throw new System.NotImplementedException();
        }

        public override void OnExit()
        {
            throw new System.NotImplementedException();
        }

        public override void Update()
        {
            throw new System.NotImplementedException();
        }
    }
}