using System;
using TurnBased.AttackResolution;
using UnityEngine;

namespace TurnBased.Phases
{
    public abstract class SubPhase : Phase
    {
        protected UnitIndex currentUnitIndex;
        public SubPhase(EventHookSystem hookSystem) : base(hookSystem)
        {
        }

        ~SubPhase()
        {

        }

        public void SetCurrentUnitIndex(UnitIndex unitIndex) { this.currentUnitIndex = unitIndex; }
    }

    public class UnitTurnPhase_None : SubPhase
    {
        public UnitTurnPhase_None(EventHookSystem hookSystem, System.Action onSubPhaseComplete) : base(hookSystem)
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
        private readonly System.Action<UnitTurnPhase_MoveSelection> OnSubPhaseComplete;

        public UnitTurnPhase_MoveSelection(EventHookSystem hookSystem, System.Action<UnitTurnPhase_MoveSelection> onSubPhaseComplete) : base(hookSystem)
        {
            this.OnSubPhaseComplete = onSubPhaseComplete;
            EventHookSystem.OnAwaitingSubPhaseUnitMoveSelection += EventHookSystem_OnAwaitingSubPhaseUnitMoveSelection;
        }
        ~UnitTurnPhase_MoveSelection()
        {
            Debug.LogError("MoveSelection Deconstructor called");

            Intention.MoveSelectionResolver.OnMoveSelected -= OnMoveSelected;
        }

        private void EventHookSystem_OnAwaitingSubPhaseUnitMoveSelection(PhaseTaskCompletionManager completionManager, UnitIndex unitIndex)
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
            OnSubPhaseComplete(this);
        }


    }

    public class UnitTurnPhase_TargetSelection : SubPhase
    {
        private readonly System.Action<UnitTurnPhase_TargetSelection> OnSubPhaseComplete;
        public UnitTurnPhase_TargetSelection(EventHookSystem hookSystem, System.Action<UnitTurnPhase_TargetSelection> onSubPhaseComplete) : base(hookSystem)
        {
            this.OnSubPhaseComplete = onSubPhaseComplete;
            EventHookSystem.OnAwaitingSubPhaseUnitTargetSelection += EventHookSystem_OnAwaitingSubPhaseUnitTargetSelection;
        }
        ~UnitTurnPhase_TargetSelection()
        {
            EventHookSystem.OnAwaitingSubPhaseUnitTargetSelection -= EventHookSystem_OnAwaitingSubPhaseUnitTargetSelection;
            Intention.TargetSelectionResolver.OnTargetSelected -= OnPhaseComplete;
        }
        private void EventHookSystem_OnAwaitingSubPhaseUnitTargetSelection(PhaseTaskCompletionManager completionManager, UnitIndex unitIndex)
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
            OnSubPhaseComplete(this);
        }
    }


    public class UnitTurnPhase_ReadyToExecuteMove : SubPhase
    {
        private readonly System.Action<UnitTurnPhase_ReadyToExecuteMove> OnSubPhaseComplete;
        private bool isResolvingInstantMove;
        public UnitTurnPhase_ReadyToExecuteMove(EventHookSystem hookSystem, System.Action<UnitTurnPhase_ReadyToExecuteMove> onSubPhaseComplete) : base(hookSystem)
        {
            this.OnSubPhaseComplete = onSubPhaseComplete;   
            EventHookSystem.OnSubPhaseUnitReadyToExecuteMove += EventHookSystem_OnSubPhaseUnitReadyToExecuteMove;
        }

        ~UnitTurnPhase_ReadyToExecuteMove()
        {
            EventHookSystem.OnSubPhaseUnitReadyToExecuteMove -= EventHookSystem_OnSubPhaseUnitReadyToExecuteMove;
        }

        private void EventHookSystem_OnSubPhaseUnitReadyToExecuteMove(PhaseTaskCompletionManager completionManager, UnitIndex unitIndex)
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
            /*  Get the Resolving state from the Unit's Intention   */
            if (!Intention.UnitIntentionManager.Instance.TryGetIntention(this.currentUnitIndex, out Intention.UnitIntention intention)) { OnPhaseComplete(); }

            /*  If the intention timing isn't Instant, we want to move onto the next unit. If it is a move that is resolved now, then we process that here.     */
            if (intention.ResolvingState.ResolutionTiming != MoveResolutionTiming.Instant) { this.isResolvingInstantMove = false;  }
            else
            {
                this.isResolvingInstantMove = true;
            }
            OnPhaseComplete();
        }

        protected override void OnPhaseComplete()
        {
            this.OnSubPhaseComplete(this);
        }


        public bool GetIsResolvingInstantMove() => this.isResolvingInstantMove;
        public void ResetIsResolvingInstantMove() => this.isResolvingInstantMove = false;
    }

    public class UnitTurnPhase_ResolveAttack : SubPhase
    {
        private readonly System.Action<UnitTurnPhase_ResolveAttack> OnSubPhaseComplete;
        public UnitTurnPhase_ResolveAttack(EventHookSystem hookSystem, System.Action<UnitTurnPhase_ResolveAttack> onSubPhaseComplete) : base(hookSystem)
        {
            this.OnSubPhaseComplete = onSubPhaseComplete;
            EventHookSystem.OnSubPhaseUnitResolveMove += EventHookSystem_OnSubPhaseUnitResolveMove;
        }

        private void EventHookSystem_OnSubPhaseUnitResolveMove(PhaseTaskCompletionManager completionManager, UnitIndex unitIndex)
        {
            this.completionManager = completionManager;
            this.completionManager.AddAction();
 
        }

        ~UnitTurnPhase_ResolveAttack()
        {
            EventHookSystem.OnSubPhaseUnitResolveMove -= EventHookSystem_OnSubPhaseUnitResolveMove;
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
            this.OnSubPhaseComplete(this);
        }
    }

    public class UnitTurnPhase_AttackComplete : SubPhase
    {
        private readonly System.Action<UnitTurnPhase_AttackComplete> OnSubPhaseComplete;
        public UnitTurnPhase_AttackComplete(EventHookSystem hookSystem, System.Action<UnitTurnPhase_AttackComplete> onSubPhaseComplete) : base(hookSystem)
        {
            this.OnSubPhaseComplete = onSubPhaseComplete;
            EventHookSystem.OnSubPhaseUnitAttackComplete += EventHookSystem_OnSubPhaseUnitAttackComplete;
        }
        ~UnitTurnPhase_AttackComplete()
        {
            EventHookSystem.OnSubPhaseUnitAttackComplete -= EventHookSystem_OnSubPhaseUnitAttackComplete;
        }

        private void EventHookSystem_OnSubPhaseUnitAttackComplete(PhaseTaskCompletionManager completionManager, UnitIndex unitIndex)
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
            this.OnSubPhaseComplete(this);
        }
    }
}