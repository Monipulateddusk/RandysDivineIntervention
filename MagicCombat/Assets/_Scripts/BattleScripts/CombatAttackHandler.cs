using System.Linq;
using UnityEngine;

namespace TurnBased.AttackResolution
{
    /// <summary>
    /// This class will be globally accessable to units so they can feed information 
    /// </summary>
    public static class CombatAttackHandler
    {
        public async static System.Threading.Tasks.Task ProcessAttackStep(Intention.ResolvingState resolvingState)
        {
            if (resolvingState.ActionResolvingStates.Count <= 0)
            {
                Debug.LogError("COMBAT_ATTACK_HANDLER_ERROR: Unable to process ActionResolvingStates! List is Empty!");
                return;
            }

            foreach (Intention.AttackActionResolvingState actionResolvingState in resolvingState.ActionResolvingStates)
            {
                /*  Get the Declared Units in this Attack Action's Targetting Group.    */
                if (!Intention.UnitIntentionFactory.TryGetDeclaredTargetsForTargetGroup(resolvingState, actionResolvingState.Action.TargetGroupID, out System.Collections.Generic.List<StationIndex> declaredTargets)) { continue; }

                await ProcessAttackAction(resolvingState.ResolvingSource, actionResolvingState.Action, declaredTargets);

                actionResolvingState.IsResolved = true;
            }
        }


        public async static System.Threading.Tasks.Task ProcessAttackAction(Intention.ResolvingSource resolvingSource, AttackAction attackAction, System.Collections.Generic.List<StationIndex> declaredTargets)
        {
            if (!StationManagerUtilities.DoesStationIndexListContainExistantTarget(declaredTargets)) { return; }
            if (resolvingSource == null) { return; }

            System.Collections.Generic.Dictionary<UnitIndex, System.Collections.Generic.List<AttackEvent>> unitIndexTargetPerAttackEventDict = new();

            foreach (StationIndex targetStation in declaredTargets)
            {
                if (!StationManager.Instance.TryGetUnitIndexOnStation(targetStation, out UnitIndex unitIndexOnStation)) { Debug.LogWarning($"COMBAT ATTACK HANDLER — UNABLE TO RETRIEVE UNIT INDEX OF TARGET. TARGET STATION INDEX IS: {targetStation.Index}"); continue; }

                System.Collections.Generic.List<AttackEvent> attackEvents = attackAction.Execute(resolvingSource, unitIndexOnStation);

                /*  Execute the attack for this UnitIndex and store it. */
                if (!unitIndexTargetPerAttackEventDict.ContainsKey(unitIndexOnStation))
                {
                    unitIndexTargetPerAttackEventDict[unitIndexOnStation] = new System.Collections.Generic.List<AttackEvent>();
                }

                unitIndexTargetPerAttackEventDict[unitIndexOnStation].AddRange(attackEvents);
                
            }

            UnityEngine.Debug.LogWarning($"Converting to timeline");

            await ConvertAttackEventsToTimeline(unitIndexTargetPerAttackEventDict);
        }

        private async static System.Threading.Tasks.Task ConvertAttackEventsToTimeline(System.Collections.Generic.Dictionary<UnitIndex, System.Collections.Generic.List<AttackEvent>> unitIndexTargetPerAttackEventDict)
        {
            if (unitIndexTargetPerAttackEventDict.Count <= 0) { return; }

            /*  As we want each part of an action to hit all required units at the same time, a timeline is needed. 
             *  An example would be a shockwave. Instead of looping through the units and each one processes one after the other, we want all units to be damaged at the same time. 
             */
            System.Collections.Generic.List<System.Collections.Generic.List<AttackEvent>> attackEventTimeline = new();

            int maximumAttackEventSequenceLength = unitIndexTargetPerAttackEventDict.Values.Max(list => list.Count);

            /*  For each hit of the attack, create a time index where we sequence all targets to invoke at the same time index. (not sure how to explain this)  
                
                So if an attack has 3 parts (P¹1, P¹2, P¹3) hitting 3 different targets ((P²1, P²2, P²3), etc) we want each timeline tick to be represented like so:
                
                — P¹1, P²1, P³1
                — P¹2, P²2, P³2
                — P¹3, P²3, P³3
             
             */
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

            await Combat.AttackTimelineManager.ResolveCombatAttackTimeline(attackEventTimeline);
        }
    }
}
