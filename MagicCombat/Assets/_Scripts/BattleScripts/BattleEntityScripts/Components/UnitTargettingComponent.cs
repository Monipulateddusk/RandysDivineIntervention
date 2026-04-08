using System.Collections.Generic;
using System.Linq;
using TurnBased;

public enum MoveTarget
{
    Self,
    SingleEnemy,
    SingleAlly,
    AllEnemies,
    AllAllies,
    Area
}


public struct UnitTargettingData
{
    public List<StationIndex?> Targets;
    public UnitTargettingData(List<StationIndex?> targets)
    {
        this.Targets = targets;
    }
}

public class BaseUnitTargettingComponent : BaseComponent
{
    protected UnitTargettingData returnData = new();

    public BaseUnitTargettingComponent(BaseBattleUnit battleUnit, UnitData unitData)
    {
        this.battleUnit = battleUnit;
        this.unitData = unitData;
    }

    public virtual UnitTargettingData SelectTargets(IBattleMoveAction moveData)
    {
        MoveTarget moveTargetType = moveData.GetMoveTargetType();

        switch (moveTargetType)
        {
            case MoveTarget.Self:
                return returnData = new()
                {
                   // Targets = new List<StationIndex?> { moveData }
                };
            case MoveTarget.SingleEnemy:
                return returnData = new()
                {
                    //Targets = new List<StationIndex?> { moveData.TargetStationIndexes.FirstOrDefault() }
                };
            case MoveTarget.SingleAlly:
                return returnData = new()
                {
                    //Targets = new List<StationIndex?> { moveData.AllyStationIndexes.FirstOrDefault() }
                };
            case MoveTarget.AllEnemies:
                return returnData = new()
                {
                   // Targets = moveData.TargetStationIndexes
                };
            case MoveTarget.AllAllies:
                return returnData = new()
                {
                   // Targets = moveData.AllyStationIndexes
                };
            case MoveTarget.Area:
                //List<StationIndex?> targets = new();
                //targets.AddRange(moveData.TargetStationIndexes);
                //targets.AddRange(moveData.AllyStationIndexes);

                return returnData = new()
                {
                    //Targets = targets
                };
            default:
                return new();
        }
    }
}
