using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// An attack that does the user's full damage stat to the target
/// </summary>
[CreateAssetMenu(fileName = "HeavyAttack", menuName = "Moves/HeavyAttack")]
public class HeavyAttack : BattleMoveAction
{
    public override AttackResolutionInfo DoMove(BaseUnit userInfo, BaseUnit targetInfo)
    {
        resolutionInfo = new AttackResolutionInfo
        {
            moveName = "HeavyAttack"
        };
        resolutionInfo.actions.Add(new AttackAction(AttackAction.ActionType.DAMAGE, userInfo.attack));
        return resolutionInfo;
    }
}