namespace TurnBased
{
    public class AttackActionExecutionContext
    {
        public async System.Threading.Tasks.Task HandleAttackEvent(AttackEvent ev)
        {
            if (ev == null) { return; }

            if (ev is DamageEvent damageEvent)
            {
                DealDamage(damageEvent.TargetUnitIndex, damageEvent.Damage);
            }
            else if (ev is ElementalDamageEvent elementalDamageEvent)
            {
                DealDamage(elementalDamageEvent.TargetUnitIndex, elementalDamageEvent.Damage);
            }
            else if (ev is HealEvent healEvent)
            {
                HealDamage(healEvent.TargetUnitIndex, healEvent.HealAmount);
            }
            else if (ev is ImbueElementEvent imbueElementEvent)
            {
                await ImbueEnvironment(imbueElementEvent.TargetUnitIndex, imbueElementEvent.ImbuedElement);
            }
            else
            {
                UnityEngine.Debug.LogError("ERROR — AttackActionExecutionContext: INVALID ATTACK EVENT");
                return;
            }
        }


        private void DealDamage(UnitIndex targetUnitIndex, int damageAmount)
        {
            Health.UnitHealthManager.Instance.DamageUnitByDamageAmount(targetUnitIndex, damageAmount);
        }

        public void HealDamage(UnitIndex targetUnitIndex, int healingAmount)
        {
            Health.UnitHealthManager.Instance.HealUnitByHealAmount(targetUnitIndex, healingAmount);
        }

        private async System.Threading.Tasks.Task ImbueEnvironment(UnitIndex targetUnitIndex, Element imbuedElement)
        {
            UnitTeam team = StationManager.Instance.GetUnitTeamOfIndex(targetUnitIndex);
            await Elements.CombatEnvironmentController.Instance.AddEnvironmentalEffect(imbuedElement, team);
        }
    }
}