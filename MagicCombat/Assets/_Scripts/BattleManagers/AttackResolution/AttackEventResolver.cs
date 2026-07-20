
namespace TurnBased.AttackResolution
{
    public class AttackEventResolver
    {

        /// <summary>
        /// CompletionManager 
        /// | Referance of Resolving ResolvingSource
        /// | Referance of Resolving AttackAction
        /// | List of Attack Events to all affected units for this Attack Event Sequence
        /// | The amount of Entries in the resolving ActionResolvingStates
        /// | The current index to the resolving ActionResolvingStates. 
        /// | The amount of Entries in the Resolving Timeline
        /// | The current Index to the Resolving Timeline
        /// /-\ Remember, the count is not 0-index starting. Therefore, there could be 1 entry in the resolving ActionResolvingStates List and we would be index 0. 
        /// </summary>
        public static event System.Action<ResolvingStatePhaseCompletionManager, Intention.ResolvingSource, AttackAction, System.Collections.Generic.List<AttackResolution.AttackEvent>, int, int, int, int> OnResolveAttackEventSequenceSection;

        private Intention.ResolvingSource currentResolvingSource;
        private AttackAction currentAttackAction;


        private EventHookSystem _EventHookSystem;
        private AttackResolution.RequestResolver _RequestResolver;
        private System.Action _OnEventSequenceComplete;
        private System.Collections.Generic.List<System.Collections.Generic.List<AttackResolution.AttackEvent>> _AttackEventTimeline;
        private int _AttackEventTimelineIndex;
        private int _AttackEventCount;
        private int _ResolvingStatesCount;
        private int _ResolvingStateIndex;


        public void Awake(EventHookSystem hookSystem, System.Action onEventSequenceComplete)
        {
            this._EventHookSystem = hookSystem;
            this._RequestResolver = new AttackResolution.RequestResolver();
            this._RequestResolver.Awake(OnRequestResolverComplete);

            this._OnEventSequenceComplete = onEventSequenceComplete;
        }

        public void OnDestroy()
        {
            OnResolveAttackEventSequenceSection = null;
        }

        public void StartResolvingCombatAttackTimeline(Intention.ResolvingSource resolvingSource, AttackAction attackAction, System.Collections.Generic.List<System.Collections.Generic.List<AttackResolution.AttackEvent>> attackEventTimeline, 
            int resolvingStateCount, int resolvingStateIndex)
        {
            /*  If it is empty, proceed to the next Attack Action Resolving State   */
            if (attackEventTimeline.Count <= 0 || this._EventHookSystem == null || this._RequestResolver == null) { return; }

            this._AttackEventTimeline = attackEventTimeline;
            this._AttackEventTimelineIndex = 0;
            this._AttackEventCount = 0;

            this._ResolvingStatesCount = resolvingStateCount;
            this._ResolvingStateIndex = resolvingStateIndex;

            this.currentAttackAction = attackAction;
            this.currentResolvingSource = resolvingSource;

            GetAttackEventSequenceForProcessing();
        }

        private void GetAttackEventSequenceForProcessing()
        {
            int TIMELINE_COUNT = this._AttackEventTimeline.Count;
            if (TIMELINE_COUNT > this._AttackEventTimelineIndex)
            {
                ResolvingStatePhaseCompletionManager completionManager = new(ResolveAttackEventSequenceSection);
                completionManager.AddAction();
                OnResolveAttackEventSequenceSection?.Invoke(completionManager, this.currentResolvingSource, this.currentAttackAction, this._AttackEventTimeline[this._AttackEventTimelineIndex], this._ResolvingStatesCount, this._ResolvingStateIndex, TIMELINE_COUNT, this._AttackEventTimelineIndex);
                completionManager.OnActionComplete();
            }
            // Attack Event Sequence is complete, proceeding to the next Resolving State
            else
            {
                this._OnEventSequenceComplete?.Invoke();
            }
        }

        private void ResolveAttackEventSequenceSection()
        {
            ProcessAttackEventSequence(this._AttackEventTimeline[this._AttackEventTimelineIndex]);
        }



        private void ProcessAttackEventSequence(System.Collections.Generic.List<AttackResolution.AttackEvent> attackEvents)
        {
            this._AttackEventCount = attackEvents.Count;

            foreach (AttackResolution.AttackEvent attackEvent in attackEvents)
            {
                if (attackEvent == null) { continue; }


                attackEvent.ExecuteAttackEvent(this._EventHookSystem, this._RequestResolver);
                UnityEngine.Debug.LogError($"RESOLVING ATTACK EVENT OF TYPE: {attackEvent.Source.Type}");

            }
        }

        /// <summary>
        /// When an attack event is finished resolving whatever it is and especially after animations and particles, proceed to the next part
        /// </summary>
        private void OnRequestResolverComplete()
        {
            this._AttackEventCount--;
            if (this._AttackEventCount <= 0)
            {
                this._AttackEventTimelineIndex++;
                GetAttackEventSequenceForProcessing();
            }
        }

    }
}