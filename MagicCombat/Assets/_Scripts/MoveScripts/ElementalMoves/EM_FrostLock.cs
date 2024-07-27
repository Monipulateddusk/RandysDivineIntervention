using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Frost Lock", menuName = "ElementalMoves/Frost Lock")]
// This move is when the Earth element combines with an Ice element. It afflicts targets with the Frost Lock condition. Units cannot move (melee attackers skip their attack, ranged attackers can still attack)
public class EM_FrostLock: BattleMoveAction
{
    public override AttackResolutionInfo DoMove(List<BaseUnit> usersInfo = null, BaseUnit userInfo = null, List<BaseUnit> targetsInfo = null, BaseUnit targetInfo = null)
    {
        resolutionInfo = new AttackResolutionInfo
        {
            moveName = "Frost Lock"
        };

        // Afflict targets with frost lock

        resolutionInfo.Actions.Add(new AttackAction(AttackAction.ActionType.STATUS_EFFECT, staEffect: "Frost Lock"));
        return resolutionInfo;
    }
}
