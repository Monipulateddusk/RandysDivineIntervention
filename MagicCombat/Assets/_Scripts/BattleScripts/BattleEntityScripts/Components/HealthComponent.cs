using UnityEngine;

public class HealthComponent : BaseComponent
{

    HealthBarController hBC;
    int health;


    public HealthComponent()
    {
        battleUnit = null;
        hBC = null;
        health = 0;
    }

    public HealthComponent(BaseBattleUnit battleUnit, UnitData unitData)
    {
        this.battleUnit = battleUnit;
        this.unitData = unitData;
        this.battleUnit.OnAlterHealth += HandleHealthChanges;

        /*  Instanciate and Attach the Health Bar to the Unit   */
        GameObject healthUIChild = (GameObject)Resources.Load("UI/HealthBarUI");
        hBC = GameObject.Instantiate(healthUIChild.transform).GetComponent<HealthBarController>();
        hBC.transform.SetParent(this.battleUnit.transform, worldPositionStays: false);

        /*  Set up the Health Bar Component     */
        health = unitData.maxHP;
        hBC.UpdateUI(health, unitData.maxHP);
    }

    ~HealthComponent()
    {
        hBC = null;
    }


    /// <summary>
    /// This handles changes in damage. The bool if it is false handles damage to the value whereas if it is true, it handles healing said value
    /// </summary>
    /// <param name="value"></param>
    /// <param name="isHealing"></param>
    void HandleHealthChanges(int value, bool isHealing)
    {

        /*  Handle health Loss or Gain. Clamp between 0 and MaxHealth   */
        int newHealthValue = (health += value * (isHealing ? 1 : -1)  );
        health = Mathf.Clamp(newHealthValue, 0, unitData.maxHP);

        /*  Update the Health Bar to show new HP    */
        hBC.UpdateUI(health, unitData.maxHP);
    }
}
