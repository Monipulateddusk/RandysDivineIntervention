using UnityEngine;
using TurnBased;

/// <summary>
/// This class will be globally accessable to units so they can feed information 
/// </summary>
public static class CombatAttackHandler
{
    public static void ProcessAttackStep(CombatEnvironmentController environmentController, AttackResolutionInfo currentAttackInfo, TurnBased.Intention.UnitIntention intentInfo)
    {
        //if (currentAttackInfo.Steps.Count < 0)
        //{
        //    Debug.LogError("COMBAT_ATTACK_HANDLER_ERROR: Unable to process Attack Step! Steps List is Empty!");
        //    return;
        //}

        //AttackStep processedStep = null;
        //foreach (AttackStep step in currentAttackInfo.Steps)
        //{
        //    foreach (AttackAction action in step.Actions)
        //    {
        //        ProcessAttackAction(environmentController, currentAttackInfo, intentInfo, action);
        //    }

        //    processedStep = step;
        //    break;
        //}
        //// Remove the processed step from the Steps List
        //currentAttackInfo.Steps.Remove(processedStep);
    }


    public static void ProcessAttackAction(CombatEnvironmentController environmentController, AttackResolutionInfo currentAttackInfo, TurnBased.Intention.UnitIntention intentInfo, AttackAction attackAction)
    {
        //foreach (BaseBattleUnit t in intentInfo.TargetSelection.Targets)
        //{
        //    switch (attackAction.Type)
        //    {
        //        case AttackActionType.DAMAGE:
        //            t.Damage(attackAction.Value);

        //            break;
        //        case AttackActionType.HEALING:
        //            t.Heal(attackAction.Value);
        //            break;

        //        // Call the CombatEnvironmentHandler to keep track of the environment condition
        //        case AttackActionType.IMBUE_ENVIRONMENTS:
        //            environmentController.AddEnvironmentalEffect(attackAction.ElementEffect, intentInfo.MoveSelection.SourceTeam, intentInfo.MoveSelection.Allies, intentInfo.MoveSelection.Targets);
        //            break;


        //        // Call the status handler passing in the attack action information so that class knows what targets are going to get what status
        //        case AttackActionType.STATUS_EFFECT:

        //            break;
        //    }
        //}
    }
    
}
