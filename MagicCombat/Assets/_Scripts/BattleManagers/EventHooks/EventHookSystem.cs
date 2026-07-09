using TurnBased.AttackResolution;
using TurnBased.Phases;

namespace TurnBased
{
    public class EventHookSystem
    {
        private PhaseTaskCompletionManager phaseCompletionManager;
        private RequestTaskCompletionManager requestTaskCompletionManager;

        /// <summary>
        /// Allows the hooking of any effects, presentation or other when starting the Battle. Remember to Call the 'AddAction' Method before, and 'OnActionComplete' when the action is done!
        /// </summary>
        public static event System.Action<PhaseTaskCompletionManager> OnStartOfBattlePhase;

        /// <summary>
        /// Allows the hooking of any effects, presentation or other when starting the Round. Remember to Call the 'AddAction' Method before, and 'OnActionComplete' when the action is done!
        /// </summary>
        public static event System.Action<PhaseTaskCompletionManager> OnStartOfRoundPhase;

        /// <summary>
        /// Allows the hooking of any effects, presentation or other at the Start of the Player's Pre-Turn. Remember to Call the 'AddAction' Method before, and 'OnActionComplete' when the action is done!
        /// </summary>
        public static event System.Action<PhaseTaskCompletionManager> OnStartOfPrePlayerTurnPhase;

        /// <summary>
        /// Allows the hooking of any effects, presentation or other to the Start of the Player Turn. Remember to Call the 'AddAction' Method before, and 'OnActionComplete' when the action is done!
        /// </summary>
        public static event System.Action<PhaseTaskCompletionManager> OnStartOfPlayerTurnPhase;

        /// <summary>
        /// Allows the hooking of any effects, presentation or other when Resolving Turn Order. Remember to Call the 'AddAction' Method before, and 'OnActionComplete' when the action is done!
        /// </summary>
        public static event System.Action<PhaseTaskCompletionManager> OnResolvingTurnOrderPhase;

        /// <summary>
        /// Allows the hooking of any effects, presentation or other at the end of the Round. Remember to Call the 'AddAction' Method before, and 'OnActionComplete' when the action is done!
        /// </summary>
        public static event System.Action<PhaseTaskCompletionManager> OnEndOfRoundPhase;

        /// <summary>
        /// Allows the hooking of any effects, presentation or other at the end of the Battle. Remember to Call the 'AddAction' Method before, and 'OnActionComplete' when the action is done!
        /// </summary>
        public static event System.Action<PhaseTaskCompletionManager> OnEndOfBattlePhase;

        // -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

        /// <summary>
        /// Allows the hooking of any effects, presentation or other when a Unit is awaiting Move Selection. Remember to Call the 'AddAction' Method before, and 'OnActionComplete' when the action is done!
        /// </summary>
        public static event System.Action<PhaseTaskCompletionManager, UnitIndex> OnAwaitingSubPhaseUnitMoveSelection;

        /// <summary>
        /// Allows the hooking of any effects, presentation or other when a Unit is awaiting Target Selection. Remember to Call the 'AddAction' Method before, and 'OnActionComplete' when the action is done!
        /// </summary>
        public static event System.Action<PhaseTaskCompletionManager, UnitIndex> OnAwaitingSubPhaseUnitTargetSelection;

        /// <summary>
        /// Allows the hooking of any effects, presentation or other when a Unit is ready to execute their Move. Remember to Call the 'AddAction' Method before, and 'OnActionComplete' when the action is done!
        /// </summary>
        public static event System.Action<PhaseTaskCompletionManager, UnitIndex> OnSubPhaseUnitReadyToExecuteMove;

        /// <summary>
        /// Allows the hooking of any effects, presentation or other when a Unit is about to Resolve their move. Remember to Call the 'AddAction' Method before, and 'OnActionComplete' when the action is done!
        /// </summary>
        public static event System.Action<PhaseTaskCompletionManager, UnitIndex> OnSubPhaseUnitResolveMove;

        /// <summary>
        /// Allows the hooking of any effects, presentation or other when a Unit's Attack is complete. Remember to Call the 'AddAction' Method before, and 'OnActionComplete' when the action is done!
        /// </summary>
        public static event System.Action<PhaseTaskCompletionManager, UnitIndex> OnSubPhaseUnitAttackComplete;

        // -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

        /// <summary>
        /// Allows the hooking of any effects, presentation or other when a Unit's Battle Move occours during resolution. Remember to Call the 'AddAction' Method before, and 'OnActionComplete' when the action is done!
        /// 
        /// IBattleMove: Referance to the Move Processed.
        /// UnitIndex: UnitIndex of the source unit who performed the move.
        /// </summary>
        public static event System.Action<PhaseTaskCompletionManager, IBattleMove, UnitIndex> OnUnitBattleMoveProcessed;

        /// <summary>
        /// Allows the hooking of any effects, presentation or other when a Unit is inflicted damage during resolution. Remember to Call the 'AddAction' Method before, and 'OnActionComplete' when the action is done!
        /// </summary>
        public static event System.Action<RequestTaskCompletionManager, DamageRequest> OnDamageRequestResolved;

        /// <summary>
        /// Allows the hooking of any effects, presentation or other when a Unit is healed during resolution. Remember to Call the 'AddAction' Method before, and 'OnActionComplete' when the action is done!
        /// </summary>

        public static event System.Action<RequestTaskCompletionManager, HealRequest> OnHealRequestResolved;

        /// <summary>
        /// Allows the hooking of any effects, presentation or other when a Unit has a status applied to them during resolution. Remember to Call the 'AddAction' Method before, and 'OnActionComplete' when the action is done!
        /// </summary>

        public static event System.Action<RequestTaskCompletionManager, ApplyStatusRequest> OnApplyStatusRequestResolved;

        /// <summary>
        /// Allows the hooking of any effects, presentation or other when a Unit has a status removed from them during resolution. Remember to Call the 'AddAction' Method before, and 'OnActionComplete' when the action is done!
        /// </summary>

        public static event System.Action<RequestTaskCompletionManager, RemoveStatusRequest> OnRemoveStatusRequestResolved;

        /// <summary>
        /// Allows the hooking of any effects, presentation or other when an element is imbued to the environment during resolution. Remember to Call the 'AddAction' Method before, and 'OnActionComplete' when the action is done!
        /// </summary>

        public static event System.Action<RequestTaskCompletionManager, ImbueElementRequest> OnImbueElementRequestResolved;


        public void OnDestroy()
        {
            this.phaseCompletionManager = null;
            this.requestTaskCompletionManager = null;

            /*  
             *  Remove all listeners to events. Doesn't prevent memory leaks, all listeners still need to unsubscribe.  
             *  But it resets it for next time.
             */

            OnStartOfBattlePhase = null;
            OnStartOfRoundPhase = null;
            OnStartOfPrePlayerTurnPhase = null;
            OnStartOfPlayerTurnPhase = null;
            OnResolvingTurnOrderPhase = null;
            OnEndOfRoundPhase = null;
            OnEndOfBattlePhase = null;

            OnAwaitingSubPhaseUnitMoveSelection = null;
            OnAwaitingSubPhaseUnitTargetSelection = null;
            OnSubPhaseUnitReadyToExecuteMove = null;
            OnSubPhaseUnitResolveMove = null;
            OnSubPhaseUnitAttackComplete = null;

            OnUnitBattleMoveProcessed = null;
            OnDamageRequestResolved = null;
            OnHealRequestResolved = null;   
            OnApplyStatusRequestResolved = null;
            OnRemoveStatusRequestResolved = null;
            OnImbueElementRequestResolved = null;
        }

        public void InvokeStartOfBattle(System.Action onComplete)
        {
            this.phaseCompletionManager = new(onComplete);

            OnStartOfBattlePhase?.Invoke(this.phaseCompletionManager);
        }

        public void InvokeStartOfRound(System.Action onComplete)
        {
            this.phaseCompletionManager = new(onComplete);

            OnStartOfRoundPhase?.Invoke(this.phaseCompletionManager);
        }

        public void InvokeStartOfPrePlayerTurn(System.Action onComplete)
        {
            this.phaseCompletionManager = new(onComplete);

            OnStartOfPrePlayerTurnPhase?.Invoke(this.phaseCompletionManager);
        }

        public void InvokeStartOfPlayerTurn(System.Action onComplete)
        {
            this.phaseCompletionManager = new(onComplete);

            OnStartOfPlayerTurnPhase?.Invoke(this.phaseCompletionManager);
        }

        public void InvokeTurnOrderResolving(System.Action onComplete)
        {
            this.phaseCompletionManager = new(onComplete);

            OnResolvingTurnOrderPhase?.Invoke(this.phaseCompletionManager);
        }

        public void InvokeEndOfRound(System.Action onComplete)
        {
            this.phaseCompletionManager = new(onComplete);

            OnEndOfRoundPhase?.Invoke(this.phaseCompletionManager);
        }

        public void InvokeEndOfBattle(System.Action onComplete)
        {
            this.phaseCompletionManager = new(onComplete);

            OnEndOfBattlePhase?.Invoke(this.phaseCompletionManager);
        }


        public void InvokeAwaitingMoveSelection(UnitIndex unitIndex, System.Action onComplete)
        {
            this.phaseCompletionManager = new(onComplete);

            OnAwaitingSubPhaseUnitMoveSelection?.Invoke(this.phaseCompletionManager, unitIndex);
        }

        public void InvokeAwaitingTargetSelection(UnitIndex unitIndex, System.Action onComplete)
        {
            this.phaseCompletionManager = new(onComplete);

            OnAwaitingSubPhaseUnitTargetSelection?.Invoke(this.phaseCompletionManager, unitIndex);
        }

        public void InvokeReadyToExecuteMove(UnitIndex unitIndex, System.Action onComplete)
        {
            this.phaseCompletionManager = new(onComplete);

            OnSubPhaseUnitReadyToExecuteMove?.Invoke(this.phaseCompletionManager, unitIndex);
        }

        public void InvokeUnitResolveMove(UnitIndex unitIndex, System.Action onComplete)
        {
            this.phaseCompletionManager = new(onComplete);

            OnSubPhaseUnitResolveMove?.Invoke(this.phaseCompletionManager, unitIndex);
        }

        public void InvokeUnitAttackComplete(UnitIndex unitIndex, System.Action onComplete)
        {
            this.phaseCompletionManager = new(onComplete);

            OnSubPhaseUnitAttackComplete?.Invoke(this.phaseCompletionManager, unitIndex);
        }


        public void InvokeUnitBattleMoveProcessed(UnitIndex unitIndex, IBattleMove battleMove, System.Action onComplete)
        {
            this.phaseCompletionManager = new(onComplete);

            OnUnitBattleMoveProcessed?.Invoke(this.phaseCompletionManager, battleMove, unitIndex);
        }

        public void InvokeDamageRequestResolved(DamageRequest request, System.Action<BaseRequest> onComplete)
        {
            this.requestTaskCompletionManager = new(request, onComplete);

            OnDamageRequestResolved?.Invoke(this.requestTaskCompletionManager, request);
        }

        public void InvokeHealRequestResolved(HealRequest request, System.Action<BaseRequest> onComplete)
        {
            this.requestTaskCompletionManager = new(request, onComplete);

            OnHealRequestResolved?.Invoke(this.requestTaskCompletionManager, request);
        }

        public void InvokeApplyStatusRequestResolved(ApplyStatusRequest request, System.Action<BaseRequest> onComplete)
        {
            this.requestTaskCompletionManager = new(request, onComplete);

            OnApplyStatusRequestResolved?.Invoke(this.requestTaskCompletionManager, request);
        }

        public void InvokeRemoveStatusRequestResolved(RemoveStatusRequest request, System.Action<BaseRequest> onComplete)
        {
            this.requestTaskCompletionManager = new(request, onComplete);

            OnRemoveStatusRequestResolved?.Invoke(this.requestTaskCompletionManager, request);
        }

        public void InvokeImbueEnvironmentRequestResolved(ImbueElementRequest request, System.Action<BaseRequest> onComplete)
        {
            this.requestTaskCompletionManager = new(request, onComplete);

            OnImbueElementRequestResolved?.Invoke(this.requestTaskCompletionManager, request);
        }

    }
}