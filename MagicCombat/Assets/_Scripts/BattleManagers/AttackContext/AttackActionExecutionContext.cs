namespace TurnBased
{
    public class AttackActionExecutionContext
    {
        public void DealDamage(UnitIndex targetUnitIndex, int damageAmount)
        {
            Health.UnitHealthManager.Instance.DamageUnitByDamageAmount(targetUnitIndex, damageAmount);
        }

        public void HealDamage(UnitIndex targetUnitIndex, int healingAmount)
        {
            Health.UnitHealthManager.Instance.HealUnitByHealAmount(targetUnitIndex, healingAmount);
        }

        public void ImbueEnvironment(UnitIndex targetUnitIndex, Element imbuedElement)
        {

        }
    }
}