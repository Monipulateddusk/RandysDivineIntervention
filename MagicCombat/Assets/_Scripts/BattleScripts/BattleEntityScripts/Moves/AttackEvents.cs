public abstract class AttackEvent
{
    public UnitIndex SourceUnitIndex { get; }

    public AttackEvent(UnitIndex sourceUnitIndex)
    {
        this.SourceUnitIndex = sourceUnitIndex;
    }
}


public class DamageEvent : AttackEvent
{
    public int Damage { get; }
    public DamageEvent(int damage, UnitIndex sourceUnitIndex) : base(sourceUnitIndex)
    {
        this.Damage = damage;
    }
}

public class ElementalDamageEvent : DamageEvent
{
    public Element Element { get; }
    public ElementalDamageEvent(Element element, int damage, UnitIndex sourceUnitIndex) : base(damage, sourceUnitIndex)
    {
        this.Element = element; 
    }   
}

public class HealEvent : AttackEvent
{
    public int HealAmount { get; }
    public HealEvent(int healAmount, UnitIndex sourceUnitIndex) : base(sourceUnitIndex)
    {
        this.HealAmount = healAmount;
    }
}

public class ImbueElementEvent : AttackEvent
{
    public Element ImbuedElement { get; }
    public ImbueElementEvent(Element imbuedElement, UnitIndex sourceUnitIndex) : base(sourceUnitIndex)
    {
        this.ImbuedElement = imbuedElement;
    }
}