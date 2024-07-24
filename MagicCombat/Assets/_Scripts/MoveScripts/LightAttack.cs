using UnityEngine;


/// <summary>
/// An attack that does 1 damage 2 times and then 1/3 of damage stat of user
/// </summary>
[CreateAssetMenu(fileName = "LightAttack", menuName = "Moves/LightAttack")]
public class LightAttack : BattleMoveAction
{
    public override AttackResolutionInfo DoMove(BaseUnit userInfo, BaseUnit targetInfo)
    {
        resolutionInfo = new AttackResolutionInfo
        {
            moveName = "LightAttack"
        };
        int damage = userInfo.attack / 3;
        resolutionInfo.actions.Add(new AttackAction(AttackAction.ActionType.DAMAGE, 1));
        resolutionInfo.actions.Add(new AttackAction(AttackAction.ActionType.DAMAGE, 1));
        resolutionInfo.actions.Add(new AttackAction(AttackAction.ActionType.DAMAGE, damage));

        return resolutionInfo;
    }
}
