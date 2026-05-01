public abstract class AttackEvent
{
    public UnitIndex TargetUnitIndex { get; }

    public AttackEvent(UnitIndex targetUnitIndex)
    {
        this.TargetUnitIndex = targetUnitIndex;
    }
}


public class DamageEvent : AttackEvent
{
    public int Damage { get; }
    public DamageEvent(int damage, UnitIndex targetUnitIndex) : base(targetUnitIndex)
    {
        this.Damage = damage;
    }
}

public class ElementalDamageEvent : DamageEvent
{
    public Element Element { get; }
    public ElementalDamageEvent(Element element, int damage, UnitIndex targetUnitIndex) : base(damage, targetUnitIndex)
    {
        this.Element = element; 
    }   
}

public class HealEvent : AttackEvent
{
    public int HealAmount { get; }
    public HealEvent(int healAmount, UnitIndex targetUnitIndex) : base(targetUnitIndex)
    {
        this.HealAmount = healAmount;
    }
}

public class ImbueElementEvent : AttackEvent
{
    public Element ImbuedElement { get; }
    public ImbueElementEvent(Element imbuedElement, UnitIndex targetUnitIndex) : base(targetUnitIndex)
    {
        this.ImbuedElement = imbuedElement;
    }
}