using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Volcano", menuName = "ElementalMoves/Volcano")]
// This move is when the fire element combines with an earth element. It deals medium earth damage and inflicts targets with burning status dealing 1 damage per turn 
public class EM_Volcano : BattleMoveAction
{
    public override AttackResolutionInfo DoMove(List<BaseUnit> usersInfo = null, BaseUnit userInfo = null, List<BaseUnit> targetsInfo = null, BaseUnit targetInfo = null)
    {
        resolutionInfo = new AttackResolutionInfo
        {
            moveName = "Volcano"
        };

        int totalAttackValue = 0;
        foreach (var unit in usersInfo)
        {
            if (unit.element == Element.EARTH)
            {
                totalAttackValue = +unit.attack;
            }
        }

        // Give a multiplier to the attack to make it slightly better than the sum of it's parts
        totalAttackValue = (int)(totalAttackValue * 1.25f);

        // Afflicting status: Burned to enemies
        resolutionInfo.Actions.Add(new AttackAction(AttackAction.ActionType.DAMAGE, value: totalAttackValue, elementEff: Element.EARTH, staEffect: "Burned"));
        return resolutionInfo;
    }
}
