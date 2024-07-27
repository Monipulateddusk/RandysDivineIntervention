using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Hail Cloak", menuName = "ElementalMoves/Hail Cloak")]
// This move is when the water element combines with an ice element. Allies are granted the status condition: Hail Cloak. (50% chance to not take damage) guarantees damage every other hit
public class EM_HailCloak: BattleMoveAction
{
    public override AttackResolutionInfo DoMove(List<BaseUnit> usersInfo = null, BaseUnit userInfo = null, List<BaseUnit> targetsInfo = null, BaseUnit targetInfo = null)
    {
        resolutionInfo = new AttackResolutionInfo
        {
            moveName = "Hail Cloak"
        };
        
        // All allies are granted Hailcloak

        resolutionInfo.Actions.Add(new AttackAction(AttackAction.ActionType.STATUS_EFFECT, staEffect: "Hail Cloak"));
        return resolutionInfo;
    }
}
