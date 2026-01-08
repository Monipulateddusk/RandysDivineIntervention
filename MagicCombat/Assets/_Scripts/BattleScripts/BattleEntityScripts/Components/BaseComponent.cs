using UnityEngine;

public abstract class BaseComponent 
{
    /*  Referance to the Parent Object  */
    protected BaseBattleUnit battleUnit;
    protected BaseUnit unitData;

    public BaseComponent()
    {
        battleUnit = null;
        unitData = null;
    }

    public BaseComponent(BaseBattleUnit battleUnit, BaseUnit unitData)
    {
        this.battleUnit = battleUnit;
        this.unitData = unitData;
    }
}