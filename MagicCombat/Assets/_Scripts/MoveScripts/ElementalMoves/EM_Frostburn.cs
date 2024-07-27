using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Frostburn", menuName = "ElementalMoves/Frostburn")]
// This move is when the fire element combines with an ice element. It afflicts the target with two status; frostburn and slippery
public class EM_Frostburn : BattleMoveAction
{
    public override AttackResolutionInfo DoMove(List<BaseUnit> usersInfo = null, BaseUnit userInfo = null, List<BaseUnit> targetsInfo = null, BaseUnit targetInfo = null)
    {
        resolutionInfo = new AttackResolutionInfo
        {
            moveName = "Frostburn"
        };

        // Afflicting status: Frostburn to enemies & Slippery
        resolutionInfo.Actions.Add(new AttackAction(AttackAction.ActionType.DAMAGE, elementEff: Element.WATER, staEffect: "Frostburn"));
        return resolutionInfo;
    }
}