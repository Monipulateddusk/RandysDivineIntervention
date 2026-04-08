namespace TurnBased.TargetSelection
{
    using System.Collections.Generic;

    public enum MoveTarget
    {
        Self,
        SingleEnemy,
        SingleAlly,
        AllEnemies,
        AllAllies,
        Area
    }

    public interface ITargetSelector
    {
        public abstract List<StationIndex?> SelectTargets(IBattleMove selectedMove);
    }

    public class SequentialTargetSelector : ITargetSelector
    {
        List<StationIndex?> ITargetSelector.SelectTargets(IBattleMove selectedMove)
        {
            MoveTarget moveTargetType = selectedMove.GetMoveTargetType();

            switch (moveTargetType)
            {
                case MoveTarget.Self:
                    return new();
                case MoveTarget.SingleEnemy:
                    break;
                case MoveTarget.SingleAlly:
                    break;
                case MoveTarget.AllEnemies:
                    break;
                case MoveTarget.AllAllies:
                    break;
                case MoveTarget.Area:
                    break;
                default:
                    break;
            }
            return new();
        }
    }
}