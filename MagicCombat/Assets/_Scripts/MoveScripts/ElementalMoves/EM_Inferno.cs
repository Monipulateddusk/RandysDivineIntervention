using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "Inferno", menuName = "ElementalMoves/Inferno")]
// This move is when the fire element combines with another fire element. It deals high fire damage to all targets based on all fire user's magic power and level 
public class EM_Inferno : BattleMoveAction
{
    public override AttackResolutionInfo DoMove(List<BaseUnit> usersInfo = null, BaseUnit userInfo = null, List<BaseUnit> targetsInfo = null, BaseUnit targetInfo = null)
    {
        resolutionInfo = new AttackResolutionInfo
        {
            moveName = "Inferno"
        };

        int totalAttackValue = 0;
        foreach (var unit in usersInfo)
        {
            if(unit.element == Element.FIRE)
            {
                totalAttackValue = +unit.attack;
            }
        }

        // Give a multiplier to the attack to make it better than the sum of it's parts
        totalAttackValue = (int)(totalAttackValue * 1.5f);

        resolutionInfo.Actions.Add(new AttackAction(AttackAction.ActionType.DAMAGE, value: totalAttackValue, elementEff: Element.FIRE));
        return resolutionInfo;
    }
}
