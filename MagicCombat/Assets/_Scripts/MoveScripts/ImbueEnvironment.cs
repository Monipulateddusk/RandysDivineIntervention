using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// An attack that imbues the environment with the user's element. Used for the joint attacks proc-ing
/// </summary>
[CreateAssetMenu(fileName = "ImbueEnvironment", menuName = "Moves/ImbueEnvironment")]
public class ImbueEnvrionment : BattleMoveAction
{
    public override AttackResolutionInfo DoMove(List<BaseUnit> usersInfo = null, BaseUnit userInfo = null, List<BaseUnit> targetsInfo = null, BaseUnit targetInfo = null)
    {
        resolutionInfo = new AttackResolutionInfo
        {
            moveName = "ImbueEnvironment"
        };
        resolutionInfo.Actions.Add(new AttackAction(AttackAction.ActionType.IMBUE_ENVIRONMENTS, elementEff: userInfo.element));
        return resolutionInfo;
    }
}