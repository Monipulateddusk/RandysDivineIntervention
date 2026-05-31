using TurnBased.AttackResolution;

namespace TurnBased.Status
{
    public abstract class BaseStatus
    {
        public string StatusName { get; private set; }
        public int StackSize { get; private set; }
        public bool CanStack { get; private set; }
        public CombatTurnOrchestrationPhase StatusTiming { get; private set; }
        public StatusType StatusType { get; private set; }
        public UnitTargetSelectorType StatusTargetType { get; }

        public BaseStatus(string statusName, bool canStack, CombatTurnOrchestrationPhase timing, StatusType statusType, UnitTargetSelectorType statusTargetType = UnitTargetSelectorType.HighestHP)
        {
            this.StatusName = statusName;
            this.StackSize = 1;
            this.StatusTiming = timing;
            this.CanStack = canStack;
            this.StatusType = statusType;
            this.StatusTargetType = statusTargetType;
        }

        public virtual AttackResolutionInfo OnStatusAdded(AttackResolution.ResolutionSceneData resolutionSceneData) { return new(); }
        public virtual AttackResolutionInfo OnStatusRemoved(AttackResolution.ResolutionSceneData resolutionSceneData) { return new(); }
        public virtual AttackResolutionInfo ProcessStatus(AttackResolution.ResolutionSceneData resolutionSceneData) { return new(); } 
        
        public virtual void ModifyIncomingDamageRequest(AttackResolution.DamageRequest damageRequest) { }
        public virtual void ModifyOutgoingDamageRequest(AttackResolution.DamageRequest damageRequest) { }
        public virtual void ModifyIncomingHealRequest(AttackResolution.HealRequest healRequest) { }
        public virtual void ModifyOutgoingHealRequest(AttackResolution.HealRequest healRequest) { }
        public virtual void ModifyIncomingApplyStatusRequest(AttackResolution.ApplyStatusRequest applyStatusRequest) { }
        public virtual void ModifyOutgoingApplyStatusRequest(AttackResolution.ApplyStatusRequest applyStatusRequest) { }
        public virtual void ModifyIncomingRemoveStatusRequest(AttackResolution.RemoveStatusRequest removeStatusRequest) { }
        public virtual void ModifyOutgoingRemoveStatusRequest(AttackResolution.RemoveStatusRequest removeStatusRequest) { }

        public virtual void IncrementStack() 
        {
            if (this.CanStack) 
            { 
                this.StackSize++;   
            }
        }

        /// <returns>True if the stack size is less than or equal to 0.</returns>
        public virtual bool DecrementStack()
        {
            this.StackSize--;

            if (this.StackSize <= 0)
            {
                return true;
            }
            return false;
        }
    }


    public class BurnStatus : BaseStatus
    {
        public BurnStatus() : base("Burn", canStack: false, CombatTurnOrchestrationPhase.StartOfRound, StatusType.Debuff)
        {
        }

        public override AttackResolutionInfo OnStatusAdded(ResolutionSceneData resolutionSceneData)
        {
            return new()
            {
                TargetDeclarationGroups = { new TargetDeclarationGroup(groupID: 0, MoveTarget.Self) },

                Steps =
                {
                    new AttackStep()
                    {
                        Actions =
                        {
                            new AttackResolution.DamageAttackAction(damageAmount: 4, 1, groupID: 0)
                        }
                    },
                }
            };
        }

        public override AttackResolutionInfo ProcessStatus(AttackResolution.ResolutionSceneData resolutionSceneData)
        {
            return new()
            {
                TargetDeclarationGroups = { new TargetDeclarationGroup(groupID: 0, MoveTarget.Self) },

                Steps =
                {
                    new AttackStep()
                    {
                        Actions =
                        {
                            new AttackResolution.DamageAttackAction(damageAmount: 1, 1, groupID: 0)
                        }
                    },
                }
            };
        }
        public override void ModifyOutgoingDamageRequest(AttackResolution.DamageRequest damageRequest)
        {
            damageRequest.DamageAmount /= 3;
        }
    }
    public class PoisonStatus : BaseStatus
    {
        public PoisonStatus() : base("Poison", canStack: true, CombatTurnOrchestrationPhase.StartOfRound, StatusType.Debuff)
        {
        }

        public override AttackResolutionInfo ProcessStatus(AttackResolution.ResolutionSceneData resolutionSceneData)
        {
            return new()
            {
                TargetDeclarationGroups = { new TargetDeclarationGroup(groupID: 0, MoveTarget.Self) },

                Steps =
                {
                    new AttackStep()
                    {
                        Actions =
                        {
                            new AttackResolution.DamageAttackAction(damageAmount: this.StackSize, 1, groupID: 0),
                            new AttackResolution.RemoveStatusAttackAction(this, groupID: 0)
                        }
                    },
                }
            };
        }
    }

    public static class StatusReferances
    {
        public readonly static System.Collections.Generic.List<TurnBased.Status.BaseStatus> StatusEffects = new()
        {
            { new PoisonStatus() }, { new BurnStatus() }            
        };
    }
}