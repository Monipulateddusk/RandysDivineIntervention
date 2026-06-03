using TurnBased.Phases;

namespace TurnBased
{
    public class EventHookSystem
    {
        private PhaseTaskCompletionManager phaseCompletionManager;

        /// <summary>
        /// Allows the hooking of any effects, presentation or other when starting the Battle. Remember to Call the 'AddAction' Method before, and 'OnActionComplete' when the action is done!
        /// </summary>
        public static event System.Action<PhaseTaskCompletionManager> OnStartOfBattle;

        /// <summary>
        /// Allows the hooking of any effects, presentation or other when starting the Round. Remember to Call the 'AddAction' Method before, and 'OnActionComplete' when the action is done!
        /// </summary>
        public static event System.Action<PhaseTaskCompletionManager> OnStartOfRound;

        /// <summary>
        /// Allows the hooking of any effects, presentation or other at the Start of the Player's Pre-Turn. Remember to Call the 'AddAction' Method before, and 'OnActionComplete' when the action is done!
        /// </summary>
        public static event System.Action<PhaseTaskCompletionManager> OnStartOfPrePlayerTurn;

        /// <summary>
        /// Allows the hooking of any effects, presentation or other to the Start of the Player Turn. Remember to Call the 'AddAction' Method before, and 'OnActionComplete' when the action is done!
        /// </summary>
        public static event System.Action<PhaseTaskCompletionManager> OnStartOfPlayerTurn;

        /// <summary>
        /// Allows the hooking of any effects, presentation or other when Resolving Turn Order. Remember to Call the 'AddAction' Method before, and 'OnActionComplete' when the action is done!
        /// </summary>
        public static event System.Action<PhaseTaskCompletionManager> OnResolvingTurnOrder;

        /// <summary>
        /// Allows the hooking of any effects, presentation or other at the end of the Round. Remember to Call the 'AddAction' Method before, and 'OnActionComplete' when the action is done!
        /// </summary>
        public static event System.Action<PhaseTaskCompletionManager> OnEndOfRound;

        /// <summary>
        /// Allows the hooking of any effects, presentation or other at the end of the Battle. Remember to Call the 'AddAction' Method before, and 'OnActionComplete' when the action is done!
        /// </summary>
        public static event System.Action<PhaseTaskCompletionManager> OnEndOfBattle;

        // -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

        /// <summary>
        /// Allows the hooking of any effects, presentation or other when a Unit is awaiting Move Selection. Remember to Call the 'AddAction' Method before, and 'OnActionComplete' when the action is done!
        /// </summary>
        public static event System.Action<PhaseTaskCompletionManager, UnitIndex> OnAwaitingUnitMoveSelection;

        /// <summary>
        /// Allows the hooking of any effects, presentation or other when a Unit is awaiting Target Selection. Remember to Call the 'AddAction' Method before, and 'OnActionComplete' when the action is done!
        /// </summary>
        public static event System.Action<PhaseTaskCompletionManager, UnitIndex> OnAwaitingUnitTargetSelection;

        /// <summary>
        /// Allows the hooking of any effects, presentation or other when a Unit is ready to execute their Move. Remember to Call the 'AddAction' Method before, and 'OnActionComplete' when the action is done!
        /// </summary>
        public static event System.Action<PhaseTaskCompletionManager, UnitIndex> OnUnitReadyToExecuteMove;

        /// <summary>
        /// Allows the hooking of any effects, presentation or other when a Unit is about to Resolve their move. Remember to Call the 'AddAction' Method before, and 'OnActionComplete' when the action is done!
        /// </summary>
        public static event System.Action<PhaseTaskCompletionManager, UnitIndex> OnUnitResolveMove;

        /// <summary>
        /// Allows the hooking of any effects, presentation or other when a Unit's Attack is complete. Remember to Call the 'AddAction' Method before, and 'OnActionComplete' when the action is done!
        /// </summary>
        public static event System.Action<PhaseTaskCompletionManager, UnitIndex> OnUnitAttackComplete;


        public void OnDestroy()
        {
            this.phaseCompletionManager = null;

            /*  
             *  Remove all listeners to events. Doesn't prevent memory leaks, all listeners still need to unsubscribe.  
             *  But it resets it for next time.
             */

            OnStartOfBattle = null;
            OnStartOfRound = null;
            OnStartOfPrePlayerTurn = null;
            OnStartOfPlayerTurn = null;
            OnResolvingTurnOrder = null;
            OnEndOfRound = null;
            OnEndOfBattle = null;

            OnAwaitingUnitMoveSelection = null;
            OnAwaitingUnitTargetSelection = null;
            OnUnitReadyToExecuteMove = null;
            OnUnitResolveMove = null;
            OnUnitAttackComplete = null;
        }

        public void InvokeStartOfBattle(System.Action onComplete)
        {
            this.phaseCompletionManager = new(onComplete);

            OnStartOfBattle?.Invoke(this.phaseCompletionManager);
        }

        public void InvokeStartOfRound(System.Action onComplete)
        {
            this.phaseCompletionManager = new(onComplete);

            OnStartOfRound?.Invoke(this.phaseCompletionManager);
        }

        public void InvokeStartOfPrePlayerTurn(System.Action onComplete)
        {
            this.phaseCompletionManager = new(onComplete);

            OnStartOfPrePlayerTurn?.Invoke(this.phaseCompletionManager);
        }

        public void InvokeStartOfPlayerTurn(System.Action onComplete)
        {
            this.phaseCompletionManager = new(onComplete);

            OnStartOfPlayerTurn?.Invoke(this.phaseCompletionManager);
        }

        public void InvokeTurnOrderResolving(System.Action onComplete)
        {
            this.phaseCompletionManager = new(onComplete);

            OnResolvingTurnOrder?.Invoke(this.phaseCompletionManager);
        }

        public void InvokeEndOfRound(System.Action onComplete)
        {
            this.phaseCompletionManager = new(onComplete);

            OnEndOfRound?.Invoke(this.phaseCompletionManager);
        }

        public void InvokeEndOfBattle(System.Action onComplete)
        {
            this.phaseCompletionManager = new(onComplete);

            OnEndOfBattle?.Invoke(this.phaseCompletionManager);
        }


        public void InvokeAwaitingMoveSelection(UnitIndex unitIndex, System.Action onComplete)
        {
            this.phaseCompletionManager = new(onComplete);

            OnAwaitingUnitMoveSelection?.Invoke(this.phaseCompletionManager, unitIndex);
        }

        public void InvokeAwaitingTargetSelection(UnitIndex unitIndex, System.Action onComplete)
        {
            this.phaseCompletionManager = new(onComplete);

            OnAwaitingUnitTargetSelection?.Invoke(this.phaseCompletionManager, unitIndex);
        }

        public void InvokeReadyToExecuteMove(UnitIndex unitIndex, System.Action onComplete)
        {
            this.phaseCompletionManager = new(onComplete);

            OnUnitReadyToExecuteMove?.Invoke(this.phaseCompletionManager, unitIndex);
        }

        public void InvokeUnitResolveMove(UnitIndex unitIndex, System.Action onComplete)
        {
            this.phaseCompletionManager = new(onComplete);

            OnUnitResolveMove?.Invoke(this.phaseCompletionManager, unitIndex);
        }

        public void InvokeUnitAttackComplete(UnitIndex unitIndex, System.Action onComplete)
        {
            this.phaseCompletionManager = new(onComplete);

            OnUnitAttackComplete?.Invoke(this.phaseCompletionManager, unitIndex);
        }
    }
}