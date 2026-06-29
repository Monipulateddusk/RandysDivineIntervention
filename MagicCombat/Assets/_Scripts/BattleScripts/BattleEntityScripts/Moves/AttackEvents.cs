namespace TurnBased.AttackResolution
{
    public abstract class AttackEvent
    {
        public Intention.ResolvingSource Source { get; }
        public UnitIndex TargetUnitIndex { get; }

        public AttackEvent(Intention.ResolvingSource source, UnitIndex targetUnitIndex)
        {
            this.Source = source;
            this.TargetUnitIndex = targetUnitIndex;
        }

        public abstract void ExecuteAttackEvent(EventHookSystem hookSystem, RequestResolver resolver);
    }


    public class DamageEvent : AttackEvent
    {
        public int Damage { get; }
        public DamageEvent(int damage, Intention.ResolvingSource source, UnitIndex targetUnitIndex) : base(source, targetUnitIndex)
        {
            this.Damage = damage;
        }

        public override void ExecuteAttackEvent(EventHookSystem hookSystem, RequestResolver resolver)
        {
            DamageRequest damageRequest = new(this.Source, this.TargetUnitIndex, this.Damage);
            resolver.DealDamage(hookSystem, damageRequest);
        }
    }

    public class ElementalDamageEvent : DamageEvent
    {
        public Element Element { get; }
        public ElementalDamageEvent(Element element, int damage, Intention.ResolvingSource source, UnitIndex targetUnitIndex) : base(damage, source, targetUnitIndex)
        {
            this.Element = element;
        }
    }

    public class HealEvent : AttackEvent
    {
        public int HealAmount { get; }
        public HealEvent(int healAmount, Intention.ResolvingSource source, UnitIndex targetUnitIndex) : base(source, targetUnitIndex)
        {
            this.HealAmount = healAmount;
        }
        public override void ExecuteAttackEvent(EventHookSystem hookSystem, RequestResolver resolver)
        {
            HealRequest healRequest = new(this.Source, this.TargetUnitIndex, this.HealAmount);
            resolver.HealDamage(hookSystem, healRequest);
        }
    }

    public class ImbueElementEvent : AttackEvent
    {
        public Element ImbuedElement { get; }
        public ImbueElementEvent(Element imbuedElement, Intention.ResolvingSource source, UnitIndex targetUnitIndex) : base(source, targetUnitIndex)
        {
            this.ImbuedElement = imbuedElement;
        }
        public override void ExecuteAttackEvent(EventHookSystem hookSystem, RequestResolver resolver)
        {
            ImbueElementRequest imbueElementRequest = new(this.Source, this.TargetUnitIndex, this.ImbuedElement);
            resolver.ImbueEnvironment(hookSystem, imbueElementRequest);
        }
    }

    public class AddStatusEvent : AttackEvent
    {
        public Status.BaseStatus Status { get; }

        public AddStatusEvent(Status.BaseStatus addedStatus, Intention.ResolvingSource source, UnitIndex targetUnitIndex) : base(source, targetUnitIndex)
        {
            this.Status = addedStatus;  
        }

        public override void ExecuteAttackEvent(EventHookSystem hookSystem, RequestResolver resolver)
        {
            ApplyStatusRequest applyStatusRequest = new(this.Source, this.TargetUnitIndex, this.Status);
            resolver.AddStatus(hookSystem, applyStatusRequest);
        }
    }
    public class RemoveStatusEvent : AttackEvent
    {
        public Status.BaseStatus Status { get; }

        public RemoveStatusEvent(Status.BaseStatus addedStatus, Intention.ResolvingSource source, UnitIndex targetUnitIndex) : base(source, targetUnitIndex)
        {
            this.Status = addedStatus;
        }
        public override void ExecuteAttackEvent(EventHookSystem hookSystem, RequestResolver resolver)
        {
            RemoveStatusRequest removeStatusRequest = new(this.Source, this.TargetUnitIndex, this.Status);
            resolver.RemoveStatus(hookSystem, removeStatusRequest);
        }
    }
}