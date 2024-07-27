using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Tsunami", menuName = "ElementalMoves/Tsunami")]
// This move is when the water element combines with another water element. It deals high water damage to target
public class EM_Tsunami : BattleMoveAction
{
    public override AttackResolutionInfo DoMove(List<BaseUnit> usersInfo = null, BaseUnit userInfo = null, List<BaseUnit> targetsInfo = null, BaseUnit targetInfo = null)
    {
        resolutionInfo = new AttackResolutionInfo
        {
            moveName = "Tsunami"
        };

        int totalAttackValue = 0;
        foreach (var unit in usersInfo)
        {
            if (unit.element == Element.WATER)
            {
                totalAttackValue = +unit.attack;
            }
        }

        // Give a multiplier to the attack to make it better than the sum of it's parts
        totalAttackValue = (int)(totalAttackValue * 1.5f);

        resolutionInfo.Actions.Add(new AttackAction(AttackAction.ActionType.DAMAGE, value: totalAttackValue, elementEff: Element.WATER));
        return resolutionInfo;
    }
}
