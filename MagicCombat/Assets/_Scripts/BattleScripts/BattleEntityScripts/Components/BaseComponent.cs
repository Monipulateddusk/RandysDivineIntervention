using UnityEngine;

public abstract class BaseComponent 
{
    /*  Referance to the Parent Object  */
    protected BaseBattleUnit battleUnit;
    protected UnitData unitData;

    public BaseComponent()
    {
        battleUnit = null;
        unitData = null;
    }

    public BaseComponent(BaseBattleUnit battleUnit, UnitData unitData)
    {
        this.battleUnit = battleUnit;
        this.unitData = unitData;
    }
}