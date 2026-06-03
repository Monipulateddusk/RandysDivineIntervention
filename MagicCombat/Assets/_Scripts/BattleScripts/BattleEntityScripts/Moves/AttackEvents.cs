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
    }


    public class DamageEvent : AttackEvent
    {
        public int Damage { get; }
        public DamageEvent(int damage, Intention.ResolvingSource source, UnitIndex targetUnitIndex) : base(source, targetUnitIndex)
        {
            this.Damage = damage;
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
    }

    public class ImbueElementEvent : AttackEvent
    {
        public Element ImbuedElement { get; }
        public ImbueElementEvent(Element imbuedElement, Intention.ResolvingSource source, UnitIndex targetUnitIndex) : base(source, targetUnitIndex)
        {
            this.ImbuedElement = imbuedElement;
        }
    }

    public class AddStatusEvent : AttackEvent
    {
        public Status.BaseStatus Status { get; }

        public AddStatusEvent(Status.BaseStatus addedStatus, Intention.ResolvingSource source, UnitIndex targetUnitIndex) : base(source, targetUnitIndex)
        {
            this.Status = addedStatus;  
        }
    }
    public class RemoveStatusEvent : AttackEvent
    {
        public Status.BaseStatus Status { get; }

        public RemoveStatusEvent(Status.BaseStatus addedStatus, Intention.ResolvingSource source, UnitIndex targetUnitIndex) : base(source, targetUnitIndex)
        {
            this.Status = addedStatus;
        }
    }
}