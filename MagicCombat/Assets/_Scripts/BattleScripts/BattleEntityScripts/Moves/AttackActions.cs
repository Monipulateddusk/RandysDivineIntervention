namespace TurnBased.AttackResolution
{
    public abstract class AttackAction
    {
        public int TargetGroupID { get; }
        protected AttackAction(int groupID)
        {
            this.TargetGroupID = groupID;
        }

        public abstract System.Collections.Generic.List<AttackEvent> Execute(Intention.ResolvingSource resolvingSource, UnitIndex targetUnitIndex);
        public abstract string GetDescription();
    }

    public class DamageAttackAction : AttackAction
    {
        public int DamageAmount { get; }
        public int HitsAmount { get; }

        public DamageAttackAction(int damageAmount, int hitsAmount, int groupID) : base(groupID)
        {
            this.DamageAmount = damageAmount;
            this.HitsAmount = hitsAmount;
        }

        public override string GetDescription()
        {
            return this.HitsAmount > 1 ? $"Deal {this.DamageAmount}x{this.HitsAmount} damage" : $"Deal {this.DamageAmount} damage";
        }

        public override System.Collections.Generic.List<AttackEvent> Execute(Intention.ResolvingSource resolvingSource, UnitIndex targetUnitIndex)
        {
            System.Collections.Generic.List<AttackEvent> events = new();

            for (int i = 0; i < this.HitsAmount; i++)
            {
                events.Add(new DamageEvent(this.DamageAmount, resolvingSource, targetUnitIndex));
            }

            return events;
        }
    }

    public class ElementalDamageAttackAction : DamageAttackAction
    {
        public Element ElementEffect { get; private set; }

        public ElementalDamageAttackAction(Element element, int damageAmount, int hitsAmount, int groupID) : base(damageAmount, hitsAmount, groupID)
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
        public override System.Collections.Generic.List<AttackEvent> Execute(Intention.ResolvingSource resolvingSource, UnitIndex targetUnitIndex)
        {
            System.Collections.Generic.List<AttackEvent> events = new();

            for (int i = 0; i < this.HitsAmount; i++)
            {
                events.Add(new ElementalDamageEvent(this.ElementEffect, this.DamageAmount, resolvingSource, targetUnitIndex));
            }

            return events;
        }
    }

    public class HealingAttackAction : AttackAction
    {
        public int HealingAmount { get; }

        public HealingAttackAction(int healingAmount, int groupID) : base(groupID)
        {
            this.HealingAmount = healingAmount;
        }

        public override string GetDescription()
        {
            return $"Heal for {HealingAmount}";
        }

        public override System.Collections.Generic.List<AttackEvent> Execute(Intention.ResolvingSource resolvingSource, UnitIndex targetUnitIndex)
        {
            return new() { new HealEvent(this.HealingAmount, resolvingSource, targetUnitIndex) };
        }
    }

    public class ImbueEnvironmentAttackAction : AttackAction
    {
        public Element ElementEffect { get; private set; }

        public ImbueEnvironmentAttackAction(Element element, int groupID) : base(groupID)
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
                Element.FIRE => "<color=red>Fire</color>",
                Element.WATER => "<color=blue>Water</color>",
                Element.ICE => "<color=#00ffffff>Ice</color>",
                Element.EARTH => "<color=#803300>Earth</color>",
                Element.LIGHT => "<color=yellow>Light</color>",
                Element.DARKNESS => "<color=purple>Dark</color>",
                _ => $"Non-Elemental",
            };
        }

        public override System.Collections.Generic.List<AttackEvent> Execute(Intention.ResolvingSource resolvingSource, UnitIndex targetUnitIndex)
        {
            return new() { new ImbueElementEvent(this.ElementEffect, resolvingSource, targetUnitIndex) };
        }
    }

    public class InflictStatusAttackAction : AttackAction
    {
        public Status.BaseStatus StatusEffect { get; private set; }

        public InflictStatusAttackAction(Status.BaseStatus statusEffect, int groupID) : base(groupID)
        {
            this.StatusEffect = statusEffect;
        }

        public override string GetDescription()
        {
            return $"Inflict the target with {this.StatusEffect.StatusName}";
        }

        public override System.Collections.Generic.List<AttackEvent> Execute(Intention.ResolvingSource resolvingSource, UnitIndex targetUnitIndex)
        {
            return new() { new AddStatusEvent(this.StatusEffect, resolvingSource, targetUnitIndex) };
        }
    }
    public class RemoveStatusAttackAction : AttackAction
    {
        public Status.BaseStatus StatusEffect { get; private set; }

        public RemoveStatusAttackAction(Status.BaseStatus statusEffect, int groupID) : base(groupID)
        {
            this.StatusEffect = statusEffect;
        }

        public override string GetDescription()
        {
            return $"Remove the target of {this.StatusEffect.StatusName}";
        }

        public override System.Collections.Generic.List<AttackEvent> Execute(Intention.ResolvingSource resolvingSource, UnitIndex targetUnitIndex)
        {
            return new() { new RemoveStatusEvent(this.StatusEffect, resolvingSource, targetUnitIndex) };
        }
    }
}