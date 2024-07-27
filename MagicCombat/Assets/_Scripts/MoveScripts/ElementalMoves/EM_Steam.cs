using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Steam", menuName = "ElementalMoves/Steam")]
// This move is when the fire element combines with a water element. It deals medium water damage and afflicts targets with burning (deals 1 hp per turn)
public class EM_Steam: BattleMoveAction
{
    public override AttackResolutionInfo DoMove(List<BaseUnit> usersInfo = null, BaseUnit userInfo = null, List<BaseUnit> targetsInfo = null, BaseUnit targetInfo = null)
    {
        resolutionInfo = new AttackResolutionInfo
        {
            moveName = "Steam"
        };

        int totalAttackValue = 0;
        foreach (var unit in usersInfo)
        {
            if (unit.element == Element.WATER)
            {
                totalAttackValue = +unit.attack;
            }
        }

        // Give a multiplier to the attack to make it slightly better than the sum of it's parts
        totalAttackValue = (int)(totalAttackValue * 1.25f);

        // Afflicting status: Burned to enemies
        resolutionInfo.Actions.Add(new AttackAction(AttackAction.ActionType.DAMAGE, value: totalAttackValue, elementEff: Element.WATER, staEffect: "Burned"));
        return resolutionInfo;
    }
}
