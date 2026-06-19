using System;
using TurnBased.AttackResolution;
using UnityEngine;

namespace TurnBased.Phases
{
    public abstract class SubPhase : Phase
    {
        protected System.Action<SubPhaseState> OnSubPhaseComplete;
        protected UnitIndex currentUnitIndex;
        public SubPhase(EventHookSystem hookSystem, System.Action<SubPhaseState> onSubPhaseComplete) : base(hookSystem)
        {
            this.OnSubPhaseComplete = onSubPhaseComplete;
        }

        ~SubPhase()
        {
            OnSubPhaseComplete = null;
        }

        public void SetCurrentUnitIndex(UnitIndex unitIndex) { this.currentUnitIndex = unitIndex; }
    }

    public class UnitTurnPhase_None : SubPhase
    {
        public UnitTurnPhase_None(EventHookSystem hookSystem, System.Action<SubPhaseState> onSubPhaseComplete) : base(hookSystem, onSubPhaseComplete)
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

        protected override void OnEventsComplete()
        {
     
        }

        protected override void OnPhaseComplete()
        {
     
        }
    }


    public class UnitTurnPhase_MoveSelection : SubPhase
    {
        public UnitTurnPhase_MoveSelection(EventHookSystem hookSystem, System.Action<SubPhaseState> onSubPhaseComplete) : base(hookSystem, onSubPhaseComplete)
        {
            EventHookSystem.OnAwaitingUnitMoveSelection += EventHookSystem_OnAwaitingUnitMoveSelection;
        }
        ~UnitTurnPhase_MoveSelection()
        {
            Debug.LogError("MoveSelection Deconstructor called");

            Intention.MoveSelectionResolver.OnMoveSelected -= OnMoveSelected;
        }

        private void EventHookSystem_OnAwaitingUnitMoveSelection(PhaseTaskCompletionManager completionManager, UnitIndex unitIndex)
        {
            this.completionManager = completionManager;
            this.completionManager.AddAction();
            UnityEngine.Debug.LogError("Added action to move selection!");
        }


        public override void OnEnter()
        {
            Debug.LogWarning($"Ready for move intention");

            this.hookSystem.InvokeAwaitingMoveSelection(this.currentUnitIndex, OnEventsComplete);

            if (!StationManager.Instance.TryGetUnitDataOfUnitIndex(this.currentUnitIndex, out var unitData)) { OnPhaseComplete(); }

            UnityEngine.Debug.LogError($"Processing move selection for: {unitData.name}");

            this.completionManager.OnActionComplete();
        }

        public override void OnExit()
        {
            Intention.MoveSelectionResolver.OnMoveSelected -= OnMoveSelected;
        }

        public override void Update()
        {

        }

        protected override void OnEventsComplete()
        {
            UnityEngine.Debug.LogError("Events complete in move selection!");


            Intention.MoveSelectionResolver.OnMoveSelected += OnMoveSelected;

            Intention.UnitIntentionManager.Instance.SetReadyForMoveIntention(this.currentUnitIndex);
            Intention.IntentionResolverManager.Instance.ProcessMoveSelection(this.currentUnitIndex);
        }

        private void OnMoveSelected(UnitIndex selectedUnitIndex, IBattleMove selectedMove)
        {
            Intention.MoveSelectionResolver.OnMoveSelected -= OnMoveSelected;
            
            if (!StationManager.Instance.TryGetUnitDataOfUnitIndex(selectedUnitIndex, out var unitData)) { OnPhaseComplete(); }
            
            UnityEngine.Debug.LogError($"{unitData.name} Selected move: {selectedMove.GetMoveName()}!");




            /*  Add this selected move to intentionManager. */
            Intention.UnitIntentionManager.Instance.SetMoveIntention(selectedUnitIndex, selectedMove);

            OnPhaseComplete();
        }


        protected override void OnPhaseComplete()
        {
            OnSubPhaseComplete(SubPhaseState.AWAITING_MOVE_SELECTION);
        }


    }

    public class UnitTurnPhase_TargetSelection : SubPhase
    {
        public UnitTurnPhase_TargetSelection(EventHookSystem hookSystem, System.Action<SubPhaseState> onSubPhaseComplete) : base(hookSystem, onSubPhaseComplete)
        {
            EventHookSystem.OnAwaitingUnitTargetSelection += EventHookSystem_OnAwaitingUnitTargetSelection;
        }
        ~UnitTurnPhase_TargetSelection()
        {
            EventHookSystem.OnAwaitingUnitTargetSelection -= EventHookSystem_OnAwaitingUnitTargetSelection;
            Intention.TargetSelectionResolver.OnTargetSelected -= OnPhaseComplete;
        }
        private void EventHookSystem_OnAwaitingUnitTargetSelection(PhaseTaskCompletionManager completionManager, UnitIndex unitIndex)
        {
            this.completionManager = completionManager;
            this.completionManager.AddAction();
        }

        public override void OnEnter()
        {
            this.hookSystem.InvokeAwaitingTargetSelection(this.currentUnitIndex, OnEventsComplete);
            this.completionManager.OnActionComplete();
        }

        public override void OnExit()
        {

        }

        public override void Update()
        {

        }
        protected override void OnEventsComplete()
        {
            Intention.TargetSelectionResolver.OnTargetSelected += OnPhaseComplete;
            Intention.IntentionResolverManager.Instance.ProcessUnitIntentionTargetSelection(this.currentUnitIndex);
        }

        protected override void OnPhaseComplete()
        {
            Intention.TargetSelectionResolver.OnTargetSelected -= OnPhaseComplete;
            OnSubPhaseComplete(SubPhaseState.AWAITING_TARGET_SELECTION);
        }
    }


    public class UnitTurnPhase_ReadyToExecuteMove : SubPhase
    {
        public UnitTurnPhase_ReadyToExecuteMove(EventHookSystem hookSystem, System.Action<SubPhaseState> onSubPhaseComplete) : base(hookSystem, onSubPhaseComplete)
        {
            EventHookSystem.OnUnitReadyToExecuteMove += EventHookSystem_OnUnitReadyToExecuteMove;
        }

        ~UnitTurnPhase_ReadyToExecuteMove()
        {
            EventHookSystem.OnUnitReadyToExecuteMove -= EventHookSystem_OnUnitReadyToExecuteMove;
        }

        private void EventHookSystem_OnUnitReadyToExecuteMove(PhaseTaskCompletionManager completionManager, UnitIndex unitIndex)
        {
            this.completionManager = completionManager;
            this.completionManager.AddAction();
        }

        public override void OnEnter()
        {
            this.hookSystem.InvokeReadyToExecuteMove(this.currentUnitIndex, OnEventsComplete);
            this.completionManager.OnActionComplete();
        }

        public override void OnExit()
        {

        }

        public override void Update()
        {
        }

        protected override void OnEventsComplete()
        {
            OnPhaseComplete();
        }

        protected override void OnPhaseComplete()
        {
            this.OnSubPhaseComplete(SubPhaseState.READY_TO_EXECUTE_MOVE);
        }
    }

    public class UnitTurnPhase_ResolveAttack : SubPhase
    {
        public UnitTurnPhase_ResolveAttack(EventHookSystem hookSystem, System.Action<SubPhaseState> onSubPhaseComplete) : base(hookSystem, onSubPhaseComplete)
        {
            EventHookSystem.OnUnitResolveMove += EventHookSystem_OnUnitResolveMove;
        }

        private void EventHookSystem_OnUnitResolveMove(PhaseTaskCompletionManager completionManager, UnitIndex unitIndex)
        {
            this.completionManager = completionManager;
            this.completionManager.AddAction();
 
        }

        ~UnitTurnPhase_ResolveAttack()
        {
            EventHookSystem.OnUnitResolveMove -= EventHookSystem_OnUnitResolveMove;
        }

        public override void OnEnter()
        {
            this.hookSystem.InvokeUnitResolveMove(this.currentUnitIndex, OnEventsComplete);
            this.completionManager.OnActionComplete();
        }

        public override void OnExit()
        {

        }

        public override void Update()
        {

        }
        protected override void OnEventsComplete()
        {
            /*  Get the Resolving state from the Unit's Intention   */
            if (!Intention.UnitIntentionManager.Instance.TryGetIntention(this.currentUnitIndex, out Intention.UnitIntention intention)) { OnPhaseComplete(); }
            AttackResolution.CombatResolvingRequest request = Combat.IntentionCombatResolverUtility.AddToCombatResolverBack(intention.ResolvingState);

            request.OnRequestComplete += WhenRequestCompleted;


            OnPhaseComplete();
        }

        private void WhenRequestCompleted(CombatResolvingRequest request)
        {
            request.OnRequestComplete -= WhenRequestCompleted;

            UnityEngine.Debug.LogError($"REQUEST DONE IN SUB PHASE. MOVING ONTO THE ATTACK COMPLETE PHASE");

            OnPhaseComplete();
        }

        protected override void OnPhaseComplete()
        {
            /*  Once all attacks are done. Go to the Attack complete subphase for any triggers if implemented.  */
            this.OnSubPhaseComplete(SubPhaseState.RESOLVE_ATTACK);
        }
    }

    public class UnitTurnPhase_AttackComplete : SubPhase
    {
        public UnitTurnPhase_AttackComplete(EventHookSystem hookSystem, System.Action<SubPhaseState> onSubPhaseComplete) : base(hookSystem, onSubPhaseComplete)
        {
            EventHookSystem.OnUnitAttackComplete += EventHookSystem_OnUnitAttackComplete;
        }
        ~UnitTurnPhase_AttackComplete()
        {
            EventHookSystem.OnUnitAttackComplete -= EventHookSystem_OnUnitAttackComplete;
        }

        private void EventHookSystem_OnUnitAttackComplete(PhaseTaskCompletionManager completionManager, UnitIndex unitIndex)
        {
            this.completionManager = completionManager;
            this.completionManager.AddAction();
        }

        public override void OnEnter()
        {
            this.hookSystem.InvokeUnitAttackComplete(this.currentUnitIndex, OnEventsComplete);
            this.completionManager.OnActionComplete();
        }

        public override void OnExit()
        {

        }

        public override void Update()
        {

        }
        protected override void OnEventsComplete()
        {
            OnPhaseComplete();
        }

        protected override void OnPhaseComplete()
        {
            this.OnSubPhaseComplete(SubPhaseState.ATTACK_COMPLETE);
        }
    }
}