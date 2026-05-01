using System.Linq;
using System.Threading.Tasks;
using TurnBased.Combat;
using UnityEngine;

namespace TurnBased.AttackResolution
{
    /// <summary>
    /// This class will be globally accessable to units so they can feed information 
    /// </summary>
    public static class CombatAttackHandler
    {
        public async static System.Threading.Tasks.Task ProcessAttackStep(AttackResolutionInfo currentAttackInfo, TurnBased.Intention.UnitIntention intentInfo)
        {
            if (currentAttackInfo.Steps.Count < 0)
            {
                Debug.LogError("COMBAT_ATTACK_HANDLER_ERROR: Unable to process Attack Step! Steps List is Empty!");
                return;
            }

            AttackStep processedStep = null;
            foreach (AttackStep step in currentAttackInfo.Steps)
            {
                foreach (AttackAction action in step.Actions)
                {
                    UnityEngine.Debug.LogWarning($"Processing attack Action");

                    await ProcessAttackAction(intentInfo, action);
                }

                processedStep = step;
                break;
            }
            // Remove the processed step from the Steps List
            currentAttackInfo.Steps.Remove(processedStep);
        }


        public async static System.Threading.Tasks.Task ProcessAttackAction(Intention.UnitIntention intentInfo, AttackAction attackAction, CombatEnvironmentController environmentController = null)
        {
            System.Collections.Generic.Dictionary<UnitIndex, System.Collections.Generic.List<AttackEvent>> unitIndexTargetPerAttackEventDict = new();

            foreach (StationIndex targetStation in intentInfo.TargetIndexList)
            {
                if (!StationManager.Instance.TryGetUnitIndexOnStation(targetStation, out UnitIndex unitIndexOnStation)) { Debug.LogWarning($"COMBAT ATTACK HANDLER — UNABLE TO RETRIEVE UNIT INDEX OF TARGET. TARGET STATION INDEX IS: {targetStation.Index}"); continue; }

                UnityEngine.Debug.LogWarning($"Processing attack Action for target station: {targetStation.Index}");

                System.Collections.Generic.List<AttackEvent> attackEvents = attackAction.Execute(unitIndexOnStation);

                UnityEngine.Debug.LogWarning($"Got attack events");

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

            UnityEngine.Debug.LogWarning($"Is the dict empty?");

            if (unitIndexTargetPerAttackEventDict.Count <= 0) { return; }

            UnityEngine.Debug.LogWarning($"dict is not empty");

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

            await AttackTimelineManager.ResolveCombatAttackTimeline(attackEventTimeline);
        }
    }
}
