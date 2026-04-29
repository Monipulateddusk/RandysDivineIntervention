using System.Collections.Generic;

namespace TurnBased
{
    #region Attack Data Classes


    public abstract class AttackAction
    {
        public MoveTarget AttackTarget { get; private set; }

        protected AttackAction(MoveTarget moveTarget)
        {
            this.AttackTarget = moveTarget;
        }

        public abstract void Execute(UnitIndex targetUnitIndex, AttackActionExecutionContext context);
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

        public override void Execute(UnitIndex targetUnitIndex, AttackActionExecutionContext context)
        {
            for (int i = 0; i < this.HitsAmount; i++)
            {
                context.DealDamage(targetUnitIndex, this.DamageAmount);
            }
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
        public override void Execute(UnitIndex targetUnitIndex, AttackActionExecutionContext context)
        {
            for (int i = 0; i < this.HitsAmount; i++)
            {
                context.DealDamage(targetUnitIndex, this.DamageAmount);
            }  
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

        public override void Execute(UnitIndex targetUnitIndex, AttackActionExecutionContext context)
        {
            context.HealDamage(targetUnitIndex, this.HealingAmount);
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

        public override void Execute(UnitIndex targetUnitIndex, AttackActionExecutionContext context)
        {
            context.ImbueEnvironment(targetUnitIndex, this.ElementEffect);
        }
    }


    public class AttackStep
    {
        public List<AttackAction> Actions = new();

        public AttackStep()
        {
            Actions = new();
        }

        public AttackStep(List<AttackAction> actions)
        {
            this.Actions = actions; 
        }
    }


    public class AttackResolutionInfo
    {
        public List<AttackStep> Steps { get; private set; }

        public AttackResolutionInfo()
        {
            Steps = new List<AttackStep>();
        }
    }
    #endregion

    public interface IBattleMove
    {
        public abstract AttackResolutionInfo ExecuteMove(UnitData userInfo = null, List<UnitData> usersInfo = null, List<UnitData> targetsInfo = null);
        public MoveTarget GetMoveTargetType();
        public int GetMaxTargets();
        public bool DoesSourceUnitMove();
        public string GetMoveName();
    }

    /// <summary>
    /// An attack that does the user's full damage stat to the target
    /// </summary>
    public class HeavyAttack : IBattleMove
    {
        public AttackResolutionInfo ExecuteMove(UnitData userInfo = null, List<UnitData> usersInfo = null, List<UnitData> targetsInfo = null)
        {
            AttackResolutionInfo resolutionInfo = new()
            {
                Steps =
                {
                    new AttackStep()
                    {
                        Actions =
                        {
                            new ElementalDamageAttackAction(userInfo.element, userInfo.attack, 1, MoveTarget.SingleEnemy),
                        }
                    },
                }
            };
            return resolutionInfo;
        }

        public int GetMaxTargets() => 1;
        public MoveTarget GetMoveTargetType() => MoveTarget.SingleEnemy;
        public bool DoesSourceUnitMove() => true;

        public string GetMoveName() => "HeavyAttack";
    }

    /// <summary>
    /// An attack that does 1 damage 2 times and then 1/3 of damage stat of user
    /// </summary>
    public class LightAttack : IBattleMove
    {
        public AttackResolutionInfo ExecuteMove(UnitData userInfo = null, List<UnitData> usersInfo = null, List<UnitData> targetsInfo = null)
        {
            int damage = userInfo.attack / 3;
            AttackResolutionInfo resolutionInfo = new()
            { 
                Steps =
                {
                    new AttackStep()
                    {
                        Actions =
                        {
                            new ElementalDamageAttackAction(userInfo.element, damageAmount: 1, 2, MoveTarget.SingleEnemy)         
                        }
                    },
                    new AttackStep()
                    {
                        Actions =
                        {
                            new ElementalDamageAttackAction(userInfo.element, damage, 1, MoveTarget.SingleEnemy)
                        }
                    },
                }

            };
            return resolutionInfo;
        }

        public int GetMaxTargets() => 1;
        public MoveTarget GetMoveTargetType() => MoveTarget.SingleEnemy;
        public bool DoesSourceUnitMove() => true;

        public string GetMoveName() => "LightAttack";
    }

    /// <summary>
    /// An attack that imbues the environment with the user's element. Used for the joint attacks proc-ing
    /// </summary>
    public class ImbueEnvrionment : IBattleMove
    {
        public AttackResolutionInfo ExecuteMove(UnitData userInfo = null, List<UnitData> usersInfo = null, List<UnitData> targetsInfo = null)
        {
            AttackResolutionInfo resolutionInfo = new()
            {
                Steps =
                {
                    new AttackStep()
                    {
                        Actions =
                        {
                            new ImbueEnvironmentAttackAction(userInfo.element, MoveTarget.Area)
                        },
                    }
                }

            };
            return resolutionInfo;
        }

        public int GetMaxTargets() => 0;
        public MoveTarget GetMoveTargetType() => MoveTarget.Area;
        public bool DoesSourceUnitMove() => false;
        public string GetMoveName() => "ImbueEnvironment";
    }
    
}