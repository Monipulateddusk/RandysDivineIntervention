namespace TurnBased.AttackResolution
{
    /// <summary>
    /// This class will be globally accessable to units so they can feed information 
    /// </summary>
    public class CombatAttackHandler
    {
        private EventHookSystem _EventHookSystem;
        private AttackEventResolver attackEventResolver;
        private Intention.ResolvingState currentResolvingState;
        private System.Action _OnActionResolvingStatesComplete;

        private int ResolvingStateIndex;

        public void Awake(EventHookSystem hookSystem)
        {
            this._EventHookSystem = hookSystem;
            this.attackEventResolver = new();
            this.attackEventResolver.Awake(hookSystem, ProcessNextResolvingState);
        }

        public void OnDestroy()
        {
            this.attackEventResolver.OnDestroy();
            this.attackEventResolver = null;
        }

        public void StartProcessingResolvingState(Intention.ResolvingState resolvingState, System.Action onActionResolvingStatesComplete)
        {
            if (resolvingState.ActionResolvingStates.Count <= 0)
            {
                UnityEngine.Debug.LogError("COMBAT_ATTACK_HANDLER_ERROR: Unable to process ActionResolvingStates! List is Empty!");
                ProcessCurrentResolvingState();
            }

            this.currentResolvingState = resolvingState;
            this._OnActionResolvingStatesComplete = onActionResolvingStatesComplete;
            this.ResolvingStateIndex = 0;
            ProcessCurrentResolvingState();
        }
        
        private void ProcessNextResolvingState()
        {
            /*  Resolve the previous resolving state and mark it as such.   */
            this.currentResolvingState.ActionResolvingStates[this.ResolvingStateIndex].IsResolved = true;

            this.ResolvingStateIndex++;
            ProcessCurrentResolvingState();
        }
        
        private void ProcessCurrentResolvingState()
        {
            if (this.currentResolvingState.ActionResolvingStates.Count > this.ResolvingStateIndex)
            {
                Intention.AttackActionResolvingState currentAttackActionResolvingState = this.currentResolvingState.ActionResolvingStates[this.ResolvingStateIndex];
                if (!Intention.UnitIntentionFactory.TryGetDeclaredTargetsForTargetGroup(this.currentResolvingState, currentAttackActionResolvingState.Action.TargetGroupID, out System.Collections.Generic.List<StationIndex> declaredTargets)) { ProcessNextResolvingState(); }
                ProcessAttackAction(this.currentResolvingState.ResolvingSource, currentAttackActionResolvingState.Action, declaredTargets);
            }
            else
            {
                this._OnActionResolvingStatesComplete?.Invoke();
            }
        }


        private void ProcessAttackAction(Intention.ResolvingSource resolvingSource, AttackAction attackAction, System.Collections.Generic.List<StationIndex> declaredTargets)
        {
            if (!StationManagerUtilities.DoesStationIndexListContainExistantTarget(declaredTargets)) { return; }
            if (resolvingSource == null) { return; }

            System.Collections.Generic.Dictionary<UnitIndex, System.Collections.Generic.List<AttackEvent>> unitIndexTargetPerAttackEventDict = new();

            foreach (StationIndex targetStation in declaredTargets)
            {
                if (!StationManager.Instance.TryGetUnitIndexOnStation(targetStation, out UnitIndex unitIndexOnStation)) { UnityEngine.Debug.LogWarning($"COMBAT ATTACK HANDLER — UNABLE TO RETRIEVE UNIT INDEX OF TARGET. TARGET STATION INDEX IS: {targetStation.Index}"); continue; }

                System.Collections.Generic.List<AttackEvent> attackEvents = attackAction.Execute(resolvingSource, unitIndexOnStation);

                /*  Execute the attack for this UnitIndex and store it. */
                if (!unitIndexTargetPerAttackEventDict.ContainsKey(unitIndexOnStation))
                {
                    unitIndexTargetPerAttackEventDict[unitIndexOnStation] = new System.Collections.Generic.List<AttackEvent>();
                }

                unitIndexTargetPerAttackEventDict[unitIndexOnStation].AddRange(attackEvents);
                
            }

            UnityEngine.Debug.LogWarning($"Converting to timeline");

            ConvertAttackEventsToTimeline(resolvingSource, attackAction, unitIndexTargetPerAttackEventDict);
        }

        private void ConvertAttackEventsToTimeline(Intention.ResolvingSource resolvingSource, AttackAction attackAction, System.Collections.Generic.Dictionary<UnitIndex, System.Collections.Generic.List<AttackEvent>> unitIndexTargetPerAttackEventDict)
        {
            if (unitIndexTargetPerAttackEventDict.Count <= 0) { return; }

            int maximumAttackEventSequenceLength = 0;
            foreach (System.Collections.Generic.List<AttackEvent> list in unitIndexTargetPerAttackEventDict.Values)
            {
                if (list.Count > maximumAttackEventSequenceLength)
                {
                    maximumAttackEventSequenceLength = list.Count;
                }
            }

            /*  As we want each part of an action to hit all required units at the same time, a timeline is needed. 
             *  An example would be a shockwave. Instead of looping through the units and each one processes one after the other, we want all units to be damaged at the same time. 
             */
            System.Collections.Generic.List<System.Collections.Generic.List<AttackEvent>> attackEventTimeline = new();

            for (int i = 0; i < maximumAttackEventSequenceLength; i++)
            {
                System.Collections.Generic.List<AttackEvent> timelineTick = new();

                foreach (System.Collections.Generic.KeyValuePair<UnitIndex, System.Collections.Generic.List<AttackEvent>> kvp in unitIndexTargetPerAttackEventDict)
                {
                    /*  Does this hit exist for this time index? 
                     *  Important if a multi-hit move misses one unit in sequece. Such as a move that hits a unit once, another unit twice and yet another unit thrice.    
                     */
                    if (i < kvp.Value.Count)
                    {
                        timelineTick.Add(kvp.Value[i]);
                    }

                }

                attackEventTimeline.Add(timelineTick);
            }

            UnityEngine.Debug.LogWarning($"Processing Timeline");

            this.attackEventResolver.StartResolvingCombatAttackTimeline(resolvingSource, attackAction, attackEventTimeline);
        } 
    }
}
