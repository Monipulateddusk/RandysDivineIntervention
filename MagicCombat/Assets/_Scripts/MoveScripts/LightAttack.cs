using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// An attack that does 1 damage 2 times and then 1/3 of damage stat of user
/// </summary>
[CreateAssetMenu(fileName = "LightAttack", menuName = "Moves/LightAttack")]
public class LightAttack : BattleMoveAction
{
    public override AttackResolutionInfo DoMove(List<BaseUnit> usersInfo = null, BaseUnit userInfo = null, List<BaseUnit> targetsInfo = null, BaseUnit targetInfo = null)
    {
        resolutionInfo = new AttackResolutionInfo
        {
            moveName = "LightAttack"
        };
        int damage = userInfo.attack / 3;
        resolutionInfo.Actions.Add(new AttackAction(AttackAction.ActionType.DAMAGE, 1));
        resolutionInfo.Actions.Add(new AttackAction(AttackAction.ActionType.DAMAGE, 1));
        resolutionInfo.Actions.Add(new AttackAction(AttackAction.ActionType.DAMAGE, damage));

        return resolutionInfo;
    }
}
