using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class will be globally accessable to units so they can feed information 
/// </summary>
public class CombatAttackHandler : MonoBehaviour
{
    private static CombatAttackHandler _instance;
    public static CombatAttackHandler Instance
    {
        get
        {
            if (_instance == null)
            {
                Debug.LogError("CombatAttackHandler is null");
            }
            return _instance;
        }
    }

    List<BaseBattleUnit> users, targets;

    private void Awake()
    {
        // Declare the instance
        _instance = this;
    }
    /// <summary>
    /// Is called by units when doing moves but also by the environment moves handler
    /// </summary>
    /// <param name="attackAction"></param>
    public void ProcessAttack(CombatReturnData attackInfo)
    {
        AttackResolutionInfo info = attackInfo.battleMoveAction.resolutionInfo;
        AttackAction attackAction = info.Actions[0];
        foreach (BaseBattleUnit t in attackInfo.targets)
        {
            switch (attackAction.Type)
            {
                case AttackAction.ActionType.DAMAGE:
                    //Debug.LogWarning("Dealing Damage to " + combatReturnData.target + " by: " + attackAction.Value);
                    t.Damage(attackAction.Value);

                    break;
                case AttackAction.ActionType.HEALING:
                    t.Heal(attackAction.Value);
                    break;

                    // Call the CombatEnvironmentController to keep track of the environment condition
                case AttackAction.ActionType.IMBUE_ENVIRONMENTS:

                    break;


                    // Call the status handler passing in the attack action information so that class knows what targets are going to get what status
                case AttackAction.ActionType.STATUS_EFFECT:

                    break;
            }
        }
        // Remove the move when it is done. If we have a multi-part move, with different attack values, this is how it should be done
        info.Actions.Remove(attackAction);
    }


}
