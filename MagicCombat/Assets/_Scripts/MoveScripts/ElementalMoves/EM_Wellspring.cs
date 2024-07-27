using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Wellspring", menuName = "ElementalMoves/Wellspring")]
// This move is when the water element combines with an earth element. Allies are healed for 2 points of HP
public class EM_Wellspring : BattleMoveAction
{
    public override AttackResolutionInfo DoMove(List<BaseUnit> usersInfo = null, BaseUnit userInfo = null, List<BaseUnit> targetsInfo = null, BaseUnit targetInfo = null)
    {
        resolutionInfo = new AttackResolutionInfo
        {
            moveName = "Wellspring"
        };

        // All allies are healed for 2 HP

        resolutionInfo.Actions.Add(new AttackAction(AttackAction.ActionType.HEALING, value: 2));
        return resolutionInfo;
    }
}
