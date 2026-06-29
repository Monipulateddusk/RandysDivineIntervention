using System;

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

    public class RequestResolver
    {
        private RequestTaskCompletionManager completionManager;

        /// -=-=-=-=-=-=-=-=-=-=-
        /// DAMAGE REQUEST
        /// -=-=-=-=-=-=-=-=-=-=-

        private void EventHookSystem_OnDamageRequestResolved(RequestTaskCompletionManager requestCompletionManager, DamageRequest request)
        {
            EventHookSystem.OnDamageRequestResolved -= EventHookSystem_OnDamageRequestResolved;
            this.completionManager = requestCompletionManager;
            this.completionManager.AddAction();
        }

        public void DealDamage(EventHookSystem hookSystem, DamageRequest damageRequest)
        {
            EventHookSystem.OnDamageRequestResolved += EventHookSystem_OnDamageRequestResolved;
            hookSystem.InvokeDamageRequestResolved(damageRequest, ProcessDamageRequest);
            this.completionManager.OnActionComplete(damageRequest);
        }

        private void ProcessDamageRequest(BaseRequest request)
        {
            DamageRequest damageRequest = request as DamageRequest;
            Information.UnitInformationManager.Instance.DamageUnitByDamageAmount(damageRequest.TargetUnit, damageRequest.DamageAmount);
        }

        /// -=-=-=-=-=-=-=-=-=-=-
        /// HEAL REQUEST
        /// -=-=-=-=-=-=-=-=-=-=-

        private void EventHookSystem_OnHealRequestResolved(RequestTaskCompletionManager requestCompletionManager, HealRequest request)
        {
            EventHookSystem.OnHealRequestResolved -= EventHookSystem_OnHealRequestResolved;
            this.completionManager = requestCompletionManager;
            this.completionManager.AddAction();
        }

        public void HealDamage(EventHookSystem hookSystem, HealRequest healRequest)
        {
            EventHookSystem.OnHealRequestResolved += EventHookSystem_OnHealRequestResolved;
            hookSystem.InvokeHealRequestResolved(healRequest, ProcessHealRequest);
            this.completionManager.OnActionComplete(healRequest);
        }

        private void ProcessHealRequest(BaseRequest request)
        {
            HealRequest healRequest = request as HealRequest;
            Information.UnitInformationManager.Instance.HealUnitByHealAmount(healRequest.TargetUnit, healRequest.HealAmount);
        }

        /// -=-=-=-=-=-=-=-=-=-=-
        /// IMBUE ENVIRONMENT REQUEST
        /// -=-=-=-=-=-=-=-=-=-=-

        private void EventHookSystem_OnImbueElementRequestResolved(RequestTaskCompletionManager requestCompletionManager, ImbueElementRequest request)
        {
            EventHookSystem.OnImbueElementRequestResolved -= EventHookSystem_OnImbueElementRequestResolved;
            this.completionManager = requestCompletionManager;
            this.completionManager.AddAction();
        }

        public void ImbueEnvironment(EventHookSystem hookSystem, ImbueElementRequest imbueElementRequest)
        {
            EventHookSystem.OnImbueElementRequestResolved += EventHookSystem_OnImbueElementRequestResolved;
            hookSystem.InvokeImbueEnvironmentRequestResolved(imbueElementRequest, ProcessImbueEnvironmentRequest);
            this.completionManager.OnActionComplete(imbueElementRequest);
        }

        private void ProcessImbueEnvironmentRequest(BaseRequest request)
        {
            ImbueElementRequest imbueElementRequest = request as ImbueElementRequest;
            Elements.CombatEnvironmentController.Instance.AddEnvironmentalEffect(imbueElementRequest.ImbuedElementType, imbueElementRequest.TargetUnit);
        }

        /// -=-=-=-=-=-=-=-=-=-=-
        /// ADD STATUS REQUEST
        /// -=-=-=-=-=-=-=-=-=-=-



        private void EventHookSystem_OnApplyStatusRequestResolved(RequestTaskCompletionManager requestCompletionManager, ApplyStatusRequest request)
        {
            EventHookSystem.OnApplyStatusRequestResolved -= EventHookSystem_OnApplyStatusRequestResolved;
            this.completionManager = requestCompletionManager;
            this.completionManager.AddAction();
        }

        public void AddStatus(EventHookSystem hookSystem, ApplyStatusRequest applyStatusRequest)
        {
            EventHookSystem.OnApplyStatusRequestResolved += EventHookSystem_OnApplyStatusRequestResolved;
            hookSystem.InvokeApplyStatusRequestResolved(applyStatusRequest, ProcessAddStatusRequest);
            this.completionManager.OnActionComplete(applyStatusRequest);
        }
        private void ProcessAddStatusRequest(BaseRequest request)
        {
            ApplyStatusRequest applyStatusRequest = request as ApplyStatusRequest;
            UnityEngine.Debug.LogError("Adding Status within AttackActionContext");
            Status.BaseStatus statusAdded = Status.UnitStatusHandler.AddStatusForUnitIndex(applyStatusRequest.TargetUnit, applyStatusRequest.ApplingStatus);
            if (statusAdded == null) { return; }

            UnityEngine.Debug.LogError($"Added Status: {statusAdded.StatusName}");

            Status.CombatStatusHandler.Instance.AddStatusEffect(applyStatusRequest, statusAdded);
        }


        /// -=-=-=-=-=-=-=-=-=-=-
        /// REMOVE STATUS REQUEST
        /// -=-=-=-=-=-=-=-=-=-=-

        private void EventHookSystem_OnRemoveStatusRequestResolved(RequestTaskCompletionManager requestCompletionManager, RemoveStatusRequest request)
        {
            EventHookSystem.OnRemoveStatusRequestResolved -= EventHookSystem_OnRemoveStatusRequestResolved;
            this.completionManager = requestCompletionManager;
            this.completionManager.AddAction();
        }

        public void RemoveStatus(EventHookSystem hookSystem, RemoveStatusRequest removeStatusRequest)
        {
            EventHookSystem.OnRemoveStatusRequestResolved += EventHookSystem_OnRemoveStatusRequestResolved;
            hookSystem.InvokeRemoveStatusRequestResolved(removeStatusRequest, ProcessRemoveStatusRequest);
            this.completionManager.OnActionComplete(removeStatusRequest);
        }
        private void ProcessRemoveStatusRequest(BaseRequest request)
        {
            RemoveStatusRequest removeStatusRequest = request as RemoveStatusRequest;
            Status.BaseStatus statusRemoved = Status.UnitStatusHandler.RemoveStatusForUnitIndex(removeStatusRequest.TargetUnit, removeStatusRequest.RemovingStatus);
            if (statusRemoved == null) { return; }

            Status.CombatStatusHandler.Instance.RemoveStatusEffect(removeStatusRequest, statusRemoved);
        }
    }
}