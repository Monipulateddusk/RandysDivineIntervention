namespace TurnBased.AttackResolution
{
    public class AttackEventResolver
    {
        private EventHookSystem _EventHookSystem;
        private AttackResolution.RequestResolver _RequestResolver;
        private System.Action _OnEventSequenceComplete;
        private System.Collections.Generic.List<System.Collections.Generic.List<AttackResolution.AttackEvent>> _AttackEventTimeline;
        private int _AttackEventTimelineIndex;
        private int _AttackEventCount;

        public void Awake(EventHookSystem hookSystem, System.Action onEventSequenceComplete)
        {
            this._EventHookSystem = hookSystem;
            this._RequestResolver = new AttackResolution.RequestResolver();
            this._RequestResolver.Awake(OnRequestResolverComplete);

            this._OnEventSequenceComplete = onEventSequenceComplete;
        }

        public void StartResolvingCombatAttackTimeline(System.Collections.Generic.List<System.Collections.Generic.List<AttackResolution.AttackEvent>> attackEventTimeline)
        {
            /*  If it is empty, proceed to the next Attack Action Resolving State   */
            if (attackEventTimeline.Count <= 0 || this._EventHookSystem == null || this._RequestResolver == null) { return; }

            this._AttackEventTimeline = attackEventTimeline;
            this._AttackEventTimelineIndex = 0;
            this._AttackEventCount = 0;

            GetAttackEventSequenceForProcessing();
        }

        private void GetAttackEventSequenceForProcessing()
        {
            if (this._AttackEventTimeline.Count > this._AttackEventTimelineIndex)
            {
                ProcessAttackEventSequence(this._AttackEventTimeline[this._AttackEventTimelineIndex]);
            }
            // Attack Event Sequence is complete, proceeding to the next Resolving State
            else
            {
                this._OnEventSequenceComplete?.Invoke();
            }
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