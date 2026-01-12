using System;
using TurnBased;


/// <summary>
/// This class will be globally accessable to units so they can feed information 
/// </summary>
public static class CombatAttackHandler
{
    public static void ProcessAttack(CombatEnvironmentController environmentController, AttackResolutionInfo currentAttackInfo, CombatReturnData attackInfo)
    {

        AttackAction attackAction = currentAttackInfo.Actions[0];
        foreach (BaseBattleUnit t in attackInfo.targets)
        {
            switch (attackAction.Type)
            {
                case AttackAction.ActionType.DAMAGE:
                    t.Damage(attackAction.Value);

                    break;
                case AttackAction.ActionType.HEALING:
                    t.Heal(attackAction.Value);
                    break;

                    // Call the CombatEnvironmentHandler to keep track of the environment condition
                case AttackAction.ActionType.IMBUE_ENVIRONMENTS:
                    environmentController.AddEnvironmentalEffect(attackAction.ElementEffect, attackInfo.teamSource, attackInfo.users, attackInfo.targets);
                    break;


                    // Call the status handler passing in the attack action information so that class knows what targets are going to get what status
                case AttackAction.ActionType.STATUS_EFFECT:

                    break;
            }
        }
        // Remove the move when it is done. If we have a multi-part move, with different attack values, this is how it should be done
        currentAttackInfo.Actions.Remove(attackAction);
    }


}
