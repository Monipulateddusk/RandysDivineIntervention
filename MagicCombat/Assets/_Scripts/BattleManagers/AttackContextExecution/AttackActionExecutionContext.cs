namespace TurnBased.AttackResolution
{
    public class BaseRequest
    {
        public Intention.ResolvingSource ResolvingSource;
        public bool IsNegated;

        public BaseRequest(Intention.ResolvingSource resolvingSource)
        {
            this.ResolvingSource = resolvingSource;
            this.IsNegated = false;
        }
    }

    public class DamageRequest : BaseRequest
    {
        public UnitIndex TargetUnit;
        public Element Element;
        public int DamageAmount;
        public bool IsCrit;

        public DamageRequest(Intention.ResolvingSource resolvingSource, UnitIndex targetIndex, int damageValue) : base(resolvingSource)
        {
            this.TargetUnit = targetIndex;
            this.DamageAmount = damageValue;
            this.Element = Element.NULL;
            this.IsCrit = false;

        }
        public DamageRequest(Intention.ResolvingSource resolvingSource, UnitIndex targetIndex, int damageValue, Element element) : base(resolvingSource)
        {
            this.ResolvingSource = resolvingSource;
            this.TargetUnit = targetIndex;
            this.DamageAmount = damageValue;
            this.Element = element;
            this.IsCrit = false;
            this.IsNegated = false;
        }
    }

    public class HealRequest : BaseRequest
    {
        public UnitIndex TargetUnit;
        public int HealAmount;

        public HealRequest(Intention.ResolvingSource resolvingSource, UnitIndex targetIndex, int healValue) : base(resolvingSource)
        {
            this.ResolvingSource = resolvingSource;
            this.TargetUnit = targetIndex;
            this.HealAmount = healValue;
            this.IsNegated = false;
        }
    }
    public class ApplyStatusRequest : BaseRequest
    {
        public Status.BaseStatus ApplingStatus;
        public UnitIndex TargetUnit;

        public ApplyStatusRequest(Intention.ResolvingSource resolvingSource, UnitIndex targetIndex, Status.BaseStatus status) : base(resolvingSource)
        {
            this.ResolvingSource = resolvingSource;
            this.ApplingStatus = status;
            this.TargetUnit = targetIndex;
            this.IsNegated = false;
        }
    }

    public class RemoveStatusRequest : BaseRequest
    {
        public Status.BaseStatus RemovingStatus;
        public UnitIndex TargetUnit;

        public RemoveStatusRequest(Intention.ResolvingSource resolvingSource, UnitIndex targetIndex, Status.BaseStatus status) : base(resolvingSource)
        {
            this.ResolvingSource = resolvingSource;
            this.RemovingStatus = status;
            this.TargetUnit = targetIndex;
            this.IsNegated = false;
        }
    }

    public class ImbueElementRequest : BaseRequest
    {
        public UnitIndex TargetUnit;
        public Element ImbuedElementType;

        public ImbueElementRequest(Intention.ResolvingSource resolvingSource, UnitIndex targetIndex, Element imbuedElementType) : base(resolvingSource)
        {
            this.ResolvingSource = resolvingSource;
            this.TargetUnit= targetIndex;
            this.ImbuedElementType = imbuedElementType;
            this.IsNegated = false;
        }
    }

    public class AttackActionExecutionContext
    {
        public void HandleAttackEvent(AttackEvent ev)
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

                ImbueEnvironment(imbueElementRequest);
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

        private void ImbueEnvironment(ImbueElementRequest imbueElementRequest)
        {
            Elements.CombatEnvironmentController.Instance.AddEnvironmentalEffect(imbueElementRequest.ImbuedElementType, imbueElementRequest.TargetUnit);
        }

        private void AddStatus(ApplyStatusRequest applyStatusRequest)
        {
            UnityEngine.Debug.LogError("Adding Status within AttackActionContext");
            Status.BaseStatus statusAdded = Status.UnitStatusHandler.AddStatusForUnitIndex(applyStatusRequest.TargetUnit, applyStatusRequest.ApplingStatus);
            if (statusAdded == null) { return; }

            UnityEngine.Debug.LogError($"Added Status: {statusAdded.StatusName}");

            Status.CombatStatusController.AddStatusEffect(applyStatusRequest, statusAdded);
        }

        private void RemoveStatus(RemoveStatusRequest removeStatusRequest)
        {
            Status.BaseStatus statusRemoved = Status.UnitStatusHandler.RemoveStatusForUnitIndex(removeStatusRequest.TargetUnit, removeStatusRequest.RemovingStatus);
            if (statusRemoved == null) { return; }

            Status.CombatStatusController.RemoveStatusEffect(removeStatusRequest, statusRemoved);
        }
    }
}