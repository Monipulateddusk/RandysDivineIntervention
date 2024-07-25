using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// An attack that does the user's full damage stat to the target
/// </summary>
[CreateAssetMenu(fileName = "HeavyAttack", menuName = "Moves/HeavyAttack")]
public class HeavyAttack : BattleMoveAction
{
    public override AttackResolutionInfo DoMove(List<BaseUnit> usersInfo = null, BaseUnit userInfo = null, List<BaseUnit> targetsInfo = null, BaseUnit targetInfo = null)
    {
        resolutionInfo = new AttackResolutionInfo
        {
            moveName = "HeavyAttack"
        };
        resolutionInfo.Actions.Add(new AttackAction(AttackAction.ActionType.DAMAGE, userInfo.attack));
        return resolutionInfo;
    }
}