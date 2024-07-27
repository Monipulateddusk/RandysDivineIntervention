using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Ice Age", menuName = "ElementalMoves/Ice Age")]
// This move is when the ice element combines with another ice element. It deals high ice damage to all targets based on all ice user's magic power and level 
public class EM_IceAge: BattleMoveAction
{
    public override AttackResolutionInfo DoMove(List<BaseUnit> usersInfo = null, BaseUnit userInfo = null, List<BaseUnit> targetsInfo = null, BaseUnit targetInfo = null)
    {
        resolutionInfo = new AttackResolutionInfo
        {
            moveName = "Ice Age"
        };

        int totalAttackValue = 0;
        foreach (var unit in usersInfo)
        {
            if (unit.element == Element.ICE)
            {
                totalAttackValue = +unit.attack;
            }
        }

        // Give a multiplier to the attack to make it better than the sum of it's parts
        totalAttackValue = (int)(totalAttackValue * 1.5f);

        resolutionInfo.Actions.Add(new AttackAction(AttackAction.ActionType.DAMAGE, value: totalAttackValue, elementEff: Element.ICE));
        return resolutionInfo;
    }
}
