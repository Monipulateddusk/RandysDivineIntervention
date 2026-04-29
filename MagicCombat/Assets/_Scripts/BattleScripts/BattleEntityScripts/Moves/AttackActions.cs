namespace TurnBased
{
    public abstract class AttackAction
    {
        public MoveTarget AttackTarget { get; private set; }

        protected AttackAction(MoveTarget moveTarget)
        {
            this.AttackTarget = moveTarget;
        }

        public abstract System.Collections.Generic.List<AttackEvent> Execute(UnitIndex targetUnitIndex);
        public abstract string GetDescription();
    }

    public class DamageAttackAction : AttackAction
    {
        public int DamageAmount { get; }
        public int HitsAmount { get; }

        public DamageAttackAction(int damageAmount, int hitsAmount, MoveTarget moveTarget) : base(moveTarget)
        {
            this.DamageAmount = damageAmount;
            this.HitsAmount = hitsAmount;
        }

        public override string GetDescription()
        {
            return this.HitsAmount > 1 ? $"Deal {this.DamageAmount}x{this.HitsAmount} damage" : $"Deal {this.DamageAmount} damage";
        }

        public override System.Collections.Generic.List<AttackEvent> Execute(UnitIndex targetUnitIndex)
        {
            System.Collections.Generic.List<AttackEvent> events = new();

            for (int i = 0; i < this.HitsAmount; i++)
            {
                events.Add(new DamageEvent(this.DamageAmount, targetUnitIndex));
            }

            return events;
        }
    }

    public class ElementalDamageAttackAction : DamageAttackAction
    {
        public Element ElementEffect { get; private set; }

        public ElementalDamageAttackAction(Element element, int damageAmount, int hitsAmount, MoveTarget moveTarget) : base(damageAmount, hitsAmount, moveTarget)
        {
            this.ElementEffect = element;
        }

        public override string GetDescription()
        {
            return this.HitsAmount > 1 ? $"Deal {this.DamageAmount}x{this.HitsAmount} {DynamicElementalString()} damage" : $"Deal {this.DamageAmount} {DynamicElementalString()} damage";
        }

        private string DynamicElementalString()
        {
            return ElementEffect switch
            {
                Element.FIRE => $"<color=red>Fire</color>",
                Element.WATER => $"<color=blue>Water</color>",
                Element.ICE => $"<color=aqua>Ice</color>",
                Element.EARTH => $"<color=brown>Earth</color>",
                Element.LIGHT => $"<color=yellow>Light</color>",
                Element.DARKNESS => $"<color=grey>Dark</color>",
                _ => $"Non-Elemental",
            };
        }
        public override System.Collections.Generic.List<AttackEvent> Execute(UnitIndex targetUnitIndex)
        {
            System.Collections.Generic.List<AttackEvent> events = new();

            for (int i = 0; i < this.HitsAmount; i++)
            {
                events.Add(new ElementalDamageEvent(this.ElementEffect, this.DamageAmount, targetUnitIndex));
            }

            return events;
        }
    }

    public class HealingAttackAction : AttackAction
    {
        public int HealingAmount { get; }

        public HealingAttackAction(int healingAmount, MoveTarget moveTarget) : base(moveTarget)
        {
            this.HealingAmount = healingAmount;
        }

        public override string GetDescription()
        {
            return $"Heal for {HealingAmount}";
        }

        public override System.Collections.Generic.List<AttackEvent> Execute(UnitIndex targetUnitIndex)
        {
            return new() { new HealEvent(this.HealingAmount, targetUnitIndex) };
        }
    }

    public class ImbueEnvironmentAttackAction : AttackAction
    {
        public Element ElementEffect { get; private set; }

        public ImbueEnvironmentAttackAction(Element element, MoveTarget moveTarget) : base(moveTarget)
        {
            this.ElementEffect = element;
        }

        public override string GetDescription()
        {
            return $"Imbue the environment with {DynamicElementalString()} Energy.";
        }

        private string DynamicElementalString()
        {
            return ElementEffect switch
            {
                Element.FIRE => $"<color=red>Fire</color>",
                Element.WATER => $"<color=blue>Water</color>",
                Element.ICE => $"<color=aqua>Ice</color>",
                Element.EARTH => $"<color=brown>Earth</color>",
                Element.LIGHT => $"<color=yellow>Light</color>",
                Element.DARKNESS => $"<color=grey>Dark</color>",
                _ => $"Non-Elemental",
            };
        }

        public override System.Collections.Generic.List<AttackEvent> Execute(UnitIndex targetUnitIndex)
        {
            return new() { new ImbueElementEvent(this.ElementEffect, targetUnitIndex) };
        }
    }
}