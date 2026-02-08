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
    public List<UnitSlot> Targets;
    public UnitTargettingData(List<UnitSlot> targets)
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

    public virtual UnitTargettingData SelectTargets(MoveSelectionData moveData)
    {
        MoveTarget moveTargetType = moveData.SelectedMove.GetMoveTargetType();

        switch (moveTargetType)
        {
            case MoveTarget.Self:
                return returnData = new()
                {
                    Targets = new List<UnitSlot> { moveData.SourceUnit }
                };
            case MoveTarget.SingleEnemy:
                return returnData = new()
                {
                    Targets = new List<UnitSlot> { moveData.Targets.FirstOrDefault() }
                };
            case MoveTarget.SingleAlly:
                return returnData = new()
                {
                    Targets = new List<UnitSlot> { moveData.Allies.FirstOrDefault() }
                };
            case MoveTarget.AllEnemies:
                return returnData = new()
                {
                    Targets = moveData.Targets
                };
            case MoveTarget.AllAllies:
                return returnData = new()
                {
                    Targets = moveData.Allies
                };
            case MoveTarget.Area:
                List<UnitSlot> targets = new();
                targets.AddRange(moveData.Targets);
                targets.AddRange(moveData.Allies);

                return returnData = new()
                {
                    Targets = targets
                };
            default:
                return new();
        }
    }
}
