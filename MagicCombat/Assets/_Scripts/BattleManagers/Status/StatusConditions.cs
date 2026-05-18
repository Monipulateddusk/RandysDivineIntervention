using TurnBased.AttackResolution;

namespace TurnBased.Status
{
    public abstract class BaseStatus
    {
        public int StackSize { get; private set; }
        public bool CanStack { get; private set; }
        public StatusResolutionTiming StatusTiming { get; private set; }
        public StatusType StatusType { get; private set; }

        public BaseStatus(bool canStack, StatusResolutionTiming timing, StatusType statusType)
        {
            this.StackSize = 1;
            this.StatusTiming = timing;
            this.CanStack = canStack;
            this.StatusType = statusType;
        }

        public virtual System.Collections.Generic.List<AttackResolution.AttackEvent> ProcessStatus(AttackResolution.ResolutionSceneData resolutionSceneData) { return new(); }
        public virtual void ModifyIncomingDamageRequest(AttackResolution.DamageRequest damageRequest) { }
        public virtual void ModifyOutgoingDamageRequest(AttackResolution.DamageRequest damageRequest) { }
        public virtual void ModifyIncomingHealRequest(AttackResolution.HealRequest healRequest) { }
        public virtual void ModifyOutgoingHealRequest(AttackResolution.HealRequest healRequest) { }
        public virtual void ModifyIncomingApplyStatusRequest(AttackResolution.ApplyStatusRequest applyStatusRequest) { }
        public virtual void ModifyOutgoingApplyStatusRequest(AttackResolution.ApplyStatusRequest applyStatusRequest) { }

        public void AddStack()
        {
            if (this.CanStack)
            {
                this.StackSize++;
            }
        }
        public void UpdateStatus()
        {

        }
    }

    public class BurnStatus : BaseStatus
    {
        public BurnStatus() : base(canStack: false, StatusResolutionTiming.StartOfRound, StatusType.Debuff)
        {
        }

        public override System.Collections.Generic.List<AttackResolution.AttackEvent> ProcessStatus(AttackResolution.ResolutionSceneData resolutionSceneData)
        {
            Intention.ResolvingSource resolvingSource = new(this);
            return new()
            {
                { new AttackResolution.DamageEvent(1, resolvingSource, resolutionSceneData.OwnerUnitInformation.UnitIndex) }
            };
        }
        public override void ModifyOutgoingDamageRequest(DamageRequest damageRequest)
        {
            damageRequest.DamageAmount /= 3;
        }
    }
    public class PoisonStatus : BaseStatus
    {
        public PoisonStatus() : base(canStack: true, StatusResolutionTiming.StartOfRound, StatusType.Debuff)
        {
        }

        public override System.Collections.Generic.List<AttackResolution.AttackEvent> ProcessStatus(AttackResolution.ResolutionSceneData resolutionSceneData)
        {
            Intention.ResolvingSource resolvingSource = new(this);
            return new()
            {
                { new AttackResolution.DamageEvent(this.StackSize, resolvingSource, resolutionSceneData.OwnerUnitInformation.UnitIndex) }
            };
        }
    }
}