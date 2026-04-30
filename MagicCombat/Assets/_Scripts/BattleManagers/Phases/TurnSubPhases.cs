using UnityEngine;

namespace TurnBased.Phases
{
    public abstract class SubPhase : Phase
    {
        protected System.Action<SubPhaseState> OnSubPhaseComplete; 
        protected UnitIndex currentUnitIndex;
        public SubPhase(System.Action<SubPhaseState> onSubPhaseComplete) : base()
        {
            this.OnSubPhaseComplete = onSubPhaseComplete;
        }

        public void SetCurrentUnitIndex(UnitIndex unitIndex) { this.currentUnitIndex = unitIndex; }
    }

    public class UnitTurnPhase_None : SubPhase
    {
        public UnitTurnPhase_None(System.Action<SubPhaseState> onSubPhaseComplete) : base(onSubPhaseComplete)
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
    }


    public class UnitTurnPhase_MoveSelection : SubPhase
    {

        public UnitTurnPhase_MoveSelection(System.Action<SubPhaseState> onSubPhaseComplete) : base(onSubPhaseComplete)
        {
        }


        public override void OnEnter()
        {
            Intention.MoveSelectionResolver.OnMoveSelected += OnMoveSelected;
            Intention.MoveSelectionResolver.ProcessIntentionMoveSelection(this.currentUnitIndex);
        }

        public override void OnExit()
        {
            Intention.MoveSelectionResolver.OnMoveSelected -= OnMoveSelected;
        }

        public override void Update()
        {

        }

        private void OnMoveSelected(UnitIndex selectedUnitIndex, IBattleMove selectedMove)
        {
            /*  Add this selected move to intentionManager. */
            Intention.UnitIntentionManager.Instance.SetMoveIntention(selectedUnitIndex, selectedMove);
            OnSubPhaseComplete(SubPhaseState.AWAITING_MOVE_SELECTION);
        }
    }

    public class UnitTurnPhase_TargetSelection : SubPhase
    {
        public UnitTurnPhase_TargetSelection(System.Action<SubPhaseState> onSubPhaseComplete) : base(onSubPhaseComplete)
        {
        }

        public override void OnEnter()
        {
            Intention.TargetSelectionResolver.OnTargetSelected += OnTargetSelected;
            Intention.TargetSelectionResolver.ProcessIntentionTargetSelection(this.currentUnitIndex);
        }

        public override void OnExit()
        {
            Intention.TargetSelectionResolver.OnTargetSelected -= OnTargetSelected;
        }

        public override void Update()
        {

        }

        private void OnTargetSelected(UnitIndex selectedUnitIndex, System.Collections.Generic.List<StationIndex> selectedTarget)
        {
            /*  Add this selected move to intentionManager. */
            Intention.UnitIntentionManager.Instance.SetTargetIntention(selectedUnitIndex, selectedTarget);

            OnSubPhaseComplete(SubPhaseState.AWAITING_TARGET_SELECTION);
        }

    }


    public class UnitTurnPhase_ReadyToExecuteMove : SubPhase
    {
        public UnitTurnPhase_ReadyToExecuteMove(System.Action<SubPhaseState> onSubPhaseComplete) : base(onSubPhaseComplete)
        {
        }

        public override void OnEnter()
        {
            this.OnSubPhaseComplete(SubPhaseState.READY_TO_EXECUTE_MOVE);
        }

        public override void OnExit()
        {

        }

        public override void Update()
        {
        }
    }

    public class UnitTurnPhase_ResolveAttack : SubPhase
    {
        PhaseTaskCompletionManager resolveAttackResolutionCompletionManager;

        public UnitTurnPhase_ResolveAttack(System.Action<SubPhaseState> onSubPhaseComplete) : base(onSubPhaseComplete)
        {

        }

        public override void OnEnter()
        {
            this.resolveAttackResolutionCompletionManager = new(OnAttackResolutionPhaseComplete);

            this.resolveAttackResolutionCompletionManager.AddAction();
            AttackResolution.AttackResolutionManager.OnAllAttacksFullyResolved += this.resolveAttackResolutionCompletionManager.OnActionComplete;
            AttackResolution.AttackResolutionManager.Instance.StartCombatResolution();

        }

        public override void OnExit()
        {
            AttackResolution.AttackResolutionManager.OnAllAttacksFullyResolved -= this.resolveAttackResolutionCompletionManager.OnActionComplete;
        }

        public override void Update()
        {

        }


        private void OnAttackResolutionPhaseComplete()
        {
            /*  Once all attacks are done. Go to the Attack complete subphase for any triggers if implemented.  */
            this.OnSubPhaseComplete(SubPhaseState.RESOLVE_ATTACK);
        }

    }

    public class UnitTurnPhase_AttackComplete : SubPhase
    {
        public UnitTurnPhase_AttackComplete(System.Action<SubPhaseState> onSubPhaseComplete) : base(onSubPhaseComplete)
        {
        }

        public override void OnEnter()
        {
            this.OnSubPhaseComplete(SubPhaseState.ATTACK_COMPLETE);
        }

        public override void OnExit()
        {

        }

        public override void Update()
        {

        }
    }
}