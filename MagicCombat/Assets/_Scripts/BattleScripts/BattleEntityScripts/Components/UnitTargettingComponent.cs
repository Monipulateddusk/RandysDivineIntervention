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


public class UnitTargettingData
{

    public List<BaseBattleUnit> Targets;
    public UnitTargettingData()
    {
        Targets = new();
    }
    public UnitTargettingData(List<BaseBattleUnit> targets)
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
        returnData.Targets.Clear();
        switch (moveData.SelectedMove.GetMoveTargetType())
        {
            case MoveTarget.Self:
                returnData.Targets.Add(moveData.SourceUnit);
                break;
            case MoveTarget.SingleEnemy:
                if (moveData.Targets.Count > 0)
                {
                    returnData.Targets.Add(moveData.Targets[0]);
                }
                break;
            case MoveTarget.SingleAlly:
                foreach (BaseBattleUnit target in moveData.Allies)
                {
                    if (target != moveData.SourceUnit)
                    {
                        returnData.Targets.Add(target);
                        break;
                    }
                }
                if(returnData.Targets.Count <= 0 && moveData.Allies.Count > 0)
                {
                    returnData.Targets.Add(moveData.Allies[0]);
                }

                break;
            case MoveTarget.AllEnemies:
                returnData.Targets = new List<BaseBattleUnit>(moveData.Targets);
                break;
            case MoveTarget.AllAllies:
                returnData.Targets = new List<BaseBattleUnit>(moveData.Allies);
                break;
            case MoveTarget.Area:
                returnData.Targets = new List<BaseBattleUnit>(moveData.Allies);
                returnData.Targets.AddRange(moveData.Targets);
                break;   
            default:
                if (moveData.Targets.Count > 0)
                {
                    returnData.Targets.Add(moveData.Targets[0]);
                }
                break;
        }

        return returnData;
    }
}
