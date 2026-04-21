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
            Debug.LogWarning($"Entering : MoveSelection for UnitIndex: {this.currentUnitIndex.Index}");

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
            UnityEngine.Debug.LogWarning($"Move was selected: {selectedMove.GetMoveName()}. Setting move intention for  selectedUnitIndex: {selectedUnitIndex.Index}");

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
            Debug.Log($"Entering : TargetSelection for UnitIndex: {this.currentUnitIndex.Index}");
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
            UnityEngine.Debug.LogWarning($"Target was selected. Amount of targets: {selectedTarget.Count}. Setting target intention for  selectedUnitIndex: {selectedUnitIndex.Index}");

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
            Debug.LogWarning($"Entering : READY_TO_EXECUTE_MOVE");

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
            MonoBehaviour.print("<color=white>OnAttackResolutionPhaseComplete</color>");

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