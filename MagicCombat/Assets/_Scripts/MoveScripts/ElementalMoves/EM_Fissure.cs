using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Fissure", menuName = "ElementalMoves/Fissure")]
// This move is when the earth element combines with another earth element. It deals high earth damage to all targets based on all earth user's magic power and level 
public class EM_Fissure : BattleMoveAction
{
    public override AttackResolutionInfo DoMove(List<BaseUnit> usersInfo = null, BaseUnit userInfo = null, List<BaseUnit> targetsInfo = null, BaseUnit targetInfo = null)
    {
        resolutionInfo = new AttackResolutionInfo
        {
            moveName = "Fissure"
        };

        int totalAttackValue = 0;
        foreach (var unit in usersInfo)
        {
            if (unit.element == Element.EARTH)
            {
                totalAttackValue = +unit.attack;
            }
        }

        // Give a multiplier to the attack to make it better than the sum of it's parts
        totalAttackValue = (int)(totalAttackValue * 1.5f);

        resolutionInfo.Actions.Add(new AttackAction(AttackAction.ActionType.DAMAGE, value: totalAttackValue, elementEff: Element.EARTH));
        return resolutionInfo;
    }
}
