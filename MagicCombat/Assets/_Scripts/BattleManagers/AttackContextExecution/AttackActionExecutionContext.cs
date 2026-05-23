using TurnBased.Status;

namespace TurnBased.AttackResolution
{
    public class DamageRequest
    {
        public Intention.ResolvingSource ResolvingSource;
        public UnitIndex TargetUnit;
        public Element Element;
        public int DamageAmount;
        public bool IsCrit;
        public bool IsNegated;

        public DamageRequest(Intention.ResolvingSource resolvingSource, UnitIndex targetIndex, int damageValue)
        {
            this.ResolvingSource = resolvingSource;
            this.TargetUnit = targetIndex;
            this.DamageAmount = damageValue;
            this.Element = Element.NULL;
            this.IsCrit = false;
            this.IsNegated = false;
        }
        public DamageRequest(Intention.ResolvingSource resolvingSource, UnitIndex targetIndex, int damageValue, Element element)
        {
            this.ResolvingSource = resolvingSource;
            this.TargetUnit = targetIndex;
            this.DamageAmount = damageValue;
            this.Element = element;
            this.IsCrit = false;
            this.IsNegated = false;
        }
    }

    public class HealRequest
    {
        public Intention.ResolvingSource ResolvingSource;
        public UnitIndex TargetUnit;
        public int HealAmount;
        public bool IsNegated;

        public HealRequest(Intention.ResolvingSource resolvingSource, UnitIndex targetIndex, int healValue)
        {
            this.ResolvingSource = resolvingSource;
            this.TargetUnit = targetIndex;
            this.HealAmount = healValue;
            this.IsNegated = false;
        }
    }
    public class ApplyStatusRequest
    {
        public Intention.ResolvingSource ResolvingSource;
        public Status.BaseStatus ApplingStatus;
        public UnitIndex TargetUnit;
        public bool IsNegated;

        public ApplyStatusRequest(Intention.ResolvingSource resolvingSource, UnitIndex targetIndex, Status.BaseStatus status)
        {
            this.ResolvingSource = resolvingSource;
            this.ApplingStatus = status;
            this.TargetUnit = targetIndex;
            this.IsNegated = false;
        }
    }

    public class RemoveStatusRequest
    {
        public Intention.ResolvingSource ResolvingSource;
        public Status.BaseStatus RemovingStatus;
        public UnitIndex TargetUnit;
        public bool IsNegated;

        public RemoveStatusRequest(Intention.ResolvingSource resolvingSource, UnitIndex targetIndex, Status.BaseStatus status)
        {
            this.ResolvingSource = resolvingSource;
            this.RemovingStatus = status;
            this.TargetUnit = targetIndex;
            this.IsNegated = false;
        }
    }

    public class ImbueElementRequest
    {
        public Intention.ResolvingSource ResolvingSource;
        public UnitIndex TargetUnit;
        public Element ImbuedElementType;
        public bool IsNegated;

        public ImbueElementRequest(Intention.ResolvingSource resolvingSource, UnitIndex targetIndex, Element imbuedElementType)
        {
            this.ResolvingSource = resolvingSource;
            this.TargetUnit= targetIndex;
            this.ImbuedElementType = imbuedElementType;
            this.IsNegated = false;
        }
    }

    public class AttackActionExecutionContext
    {
        public async System.Threading.Tasks.Task HandleAttackEvent(AttackEvent ev)
        {
            if (ev == null) { return; }

            if (ev is DamageEvent damageEvent)
            {
                DamageRequest damageRequest = new(damageEvent.Source, damageEvent.TargetUnitIndex, damageEvent.Damage);
                DealDamage(damageRequest);
            }
            else if (ev is ElementalDamageEvent elementalDamageEvent)
            {
                DamageRequest damageRequest = new(elementalDamageEvent.Source, elementalDamageEvent.TargetUnitIndex, elementalDamageEvent.Damage, elementalDamageEvent.Element);

                DealDamage(damageRequest);
            }
            else if (ev is HealEvent healEvent)
            {
                HealRequest healRequest = new(healEvent.Source, healEvent.TargetUnitIndex, healEvent.HealAmount);

                HealDamage(healRequest);
            }
            else if (ev is ImbueElementEvent imbueElementEvent)
            {
                ImbueElementRequest imbueElementRequest = new(imbueElementEvent.Source, imbueElementEvent.TargetUnitIndex, imbueElementEvent.ImbuedElement);

                await ImbueEnvironment(imbueElementRequest);
            }
            else if (ev is AddStatusEvent addStatusEvent)
            {
                ApplyStatusRequest applyStatusRequest = new(addStatusEvent.Source, addStatusEvent.TargetUnitIndex, addStatusEvent.Status);

                AddStatus(applyStatusRequest);
            }
            else if (ev is RemoveStatusEvent removeStatusEvent)
            {
                RemoveStatusRequest removeStatusRequest = new(removeStatusEvent.Source, removeStatusEvent.TargetUnitIndex, removeStatusEvent.Status);

                RemoveStatus(removeStatusRequest);
            }
            else
            {
                UnityEngine.Debug.LogError("ERROR — AttackActionExecutionContext: INVALID ATTACK EVENT");
                return;
            }
        }


        private void DealDamage(DamageRequest damageRequest)
        {
            Health.UnitHealthManager.Instance.DamageUnitByDamageAmount(damageRequest.TargetUnit, damageRequest.DamageAmount);
        }

        public void HealDamage(HealRequest healRequest)
        {
            Health.UnitHealthManager.Instance.HealUnitByHealAmount(healRequest.TargetUnit, healRequest.HealAmount);
        }

        private async System.Threading.Tasks.Task ImbueEnvironment(ImbueElementRequest imbueElementRequest)
        {
            UnitTeam team = StationManager.Instance.GetUnitTeamOfIndex(imbueElementRequest.TargetUnit);
            await Elements.CombatEnvironmentController.Instance.AddEnvironmentalEffect(imbueElementRequest.ImbuedElementType, team);
        }

        private void AddStatus(ApplyStatusRequest applyStatusRequest)
        {
            UnitStatusHandler.RemoveStatusForUnitIndex(applyStatusRequest.TargetUnit, applyStatusRequest.ApplingStatus);
        }

        private void RemoveStatus(RemoveStatusRequest removeStatusRequest)
        {
            UnitStatusHandler.RemoveStatusForUnitIndex(removeStatusRequest.TargetUnit, removeStatusRequest.RemovingStatus);
        }
    }
}