using UnityEngine;

/// <summary>
/// An attack that imbues the environment with the user's element. Used for the joint attacks proc-ing
/// </summary>
[CreateAssetMenu(fileName = "ImbueEnvironment", menuName = "Moves/ImbueEnvironment")]
public class ImbueEnvrionment : BattleMoveAction
{
    public override AttackResolutionInfo DoMove(BaseUnit userInfo, BaseUnit targetInfo)
    {
        resolutionInfo = new AttackResolutionInfo
        {
            moveName = "ImbueEnvironment"
        };
        resolutionInfo.actions.Add(new AttackAction(AttackAction.ActionType.IMBUE_ENVIRONMENTS, elementEff: userInfo.element));
        return resolutionInfo;
    }
}