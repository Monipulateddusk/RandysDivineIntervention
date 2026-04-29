using UnityEngine;

namespace TurnBased.AttackResolution
{
    /// <summary>
    /// This class will be globally accessable to units so they can feed information 
    /// </summary>
    public static class CombatAttackHandler
    {
        public static void ProcessAttackStep(AttackResolutionInfo currentAttackInfo, TurnBased.Intention.UnitIntention intentInfo)
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
                    ProcessAttackAction(intentInfo, action);
                }

                processedStep = step;
                break;
            }
            // Remove the processed step from the Steps List
            currentAttackInfo.Steps.Remove(processedStep);
        }


        public static void ProcessAttackAction(Intention.UnitIntention intentInfo, AttackAction attackAction, CombatEnvironmentController environmentController = null)
        {
            foreach (StationIndex targetStation in intentInfo.TargetIndexList)
            {
                if (!StationManager.Instance.TryGetUnitIndexOnStation(targetStation, out UnitIndex unitIndexOnStation)) { Debug.LogWarning("COMBAT ATTACK HANDLER — UNABLE TO RETRIEVE UNIT INDEX OF TARGET"); continue; }

                AttackActionExecutionContext executionContext = new();

                attackAction.Execute(unitIndexOnStation, executionContext);
            }
        }
    }
}
