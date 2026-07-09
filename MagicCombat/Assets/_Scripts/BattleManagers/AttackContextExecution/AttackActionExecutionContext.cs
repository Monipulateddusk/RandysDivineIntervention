namespace TurnBased.AttackResolution
{
    public class BaseRequest
    {
        public Intention.ResolvingSource ResolvingSource;
        public UnitIndex TargetUnit;
        public bool IsNegated;
        public bool IsResolved { get; protected set; }


        public BaseRequest(Intention.ResolvingSource resolvingSource)
        {
            this.ResolvingSource = resolvingSource;
            this.IsNegated = false;
        }
        public BaseRequest(Intention.ResolvingSource resolvingSource, bool isNegated)
        {
            this.ResolvingSource = resolvingSource;
            this.IsNegated = isNegated;
        }
    }

    public class DamageRequest : BaseRequest
    {
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

        public DamageRequest(DamageRequest resolvedDamageRequest) : base(resolvedDamageRequest.ResolvingSource, resolvedDamageRequest.IsNegated)
        {
            this.Element = resolvedDamageRequest.Element;
            this.DamageAmount = resolvedDamageRequest.DamageAmount;
            this.IsCrit = resolvedDamageRequest.IsCrit;

            this.IsResolved = true;
        }
    }

    public class HealRequest : BaseRequest
    {
        public int HealAmount;

        public HealRequest(Intention.ResolvingSource resolvingSource, UnitIndex targetIndex, int healValue) : base(resolvingSource)
        {
            this.ResolvingSource = resolvingSource;
            this.TargetUnit = targetIndex;
            this.HealAmount = healValue;
            this.IsNegated = false;
        }

        public HealRequest(HealRequest resolvedHealRequest) : base(resolvedHealRequest.ResolvingSource, resolvedHealRequest.IsNegated)
        {
            this.HealAmount = resolvedHealRequest.HealAmount;

            this.IsResolved = true;
        }
    }
    public class ApplyStatusRequest : BaseRequest
    {
        public Status.BaseStatus ApplingStatus;

        public ApplyStatusRequest(Intention.ResolvingSource resolvingSource, UnitIndex targetIndex, Status.BaseStatus status) : base(resolvingSource)
        {
            this.ResolvingSource = resolvingSource;
            this.ApplingStatus = status;
            this.TargetUnit = targetIndex;
            this.IsNegated = false;
        }

        public ApplyStatusRequest(ApplyStatusRequest resolvedApplyStatusRequest) : base(resolvedApplyStatusRequest.ResolvingSource, resolvedApplyStatusRequest.IsNegated)
        {
            this.ApplingStatus = resolvedApplyStatusRequest.ApplingStatus;

            this.IsResolved = true;
        }
    }

    public class RemoveStatusRequest : BaseRequest
    {
        public Status.BaseStatus RemovingStatus;

        public RemoveStatusRequest(Intention.ResolvingSource resolvingSource, UnitIndex targetIndex, Status.BaseStatus status) : base(resolvingSource)
        {
            this.ResolvingSource = resolvingSource;
            this.RemovingStatus = status;
            this.TargetUnit = targetIndex;
            this.IsNegated = false;
        }

        public RemoveStatusRequest(RemoveStatusRequest resolvedRemoveStatusRequest) : base(resolvedRemoveStatusRequest.ResolvingSource, resolvedRemoveStatusRequest.IsNegated)
        {
            this.RemovingStatus = resolvedRemoveStatusRequest.RemovingStatus;

            this.IsResolved = true;
        }
    }

    public class ImbueElementRequest : BaseRequest
    {
        public Element ImbuedElementType;

        public ImbueElementRequest(Intention.ResolvingSource resolvingSource, UnitIndex targetIndex, Element imbuedElementType) : base(resolvingSource)
        {
            this.ResolvingSource = resolvingSource;
            this.TargetUnit= targetIndex;
            this.ImbuedElementType = imbuedElementType;
            this.IsNegated = false;
        }
        public ImbueElementRequest(ImbueElementRequest resolvedImbueElementRequest) : base(resolvedImbueElementRequest.ResolvingSource, resolvedImbueElementRequest.IsNegated)
        {
            this.ImbuedElementType = resolvedImbueElementRequest.ImbuedElementType;

            this.IsResolved = true;
        }
    }

    public class RequestResolver
    {
        private RequestTaskCompletionManager completionManager;
        private System.Action OnRequestResolverComplete;

        public void Awake(System.Action onCompleteResolving)
        {
            this.OnRequestResolverComplete = onCompleteResolving;
        }

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
            this.completionManager.OnActionComplete();
        }

        private void ProcessDamageRequest(BaseRequest request)
        {
            DamageRequest damageRequest = request as DamageRequest;
            Information.UnitInformationManager.Instance.DamageUnitByDamageAmount(damageRequest.TargetUnit, damageRequest.DamageAmount);
            this.OnRequestResolverComplete?.Invoke();
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
            this.completionManager.OnActionComplete();
        }

        private void ProcessHealRequest(BaseRequest request)
        {
            HealRequest healRequest = request as HealRequest;
            Information.UnitInformationManager.Instance.HealUnitByHealAmount(healRequest.TargetUnit, healRequest.HealAmount);
            this.OnRequestResolverComplete?.Invoke();
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
            this.completionManager.OnActionComplete();
        }

        private void ProcessImbueEnvironmentRequest(BaseRequest request)
        {
            ImbueElementRequest imbueElementRequest = request as ImbueElementRequest;
            Elements.CombatEnvironmentController.Instance.AddEnvironmentalEffect(imbueElementRequest.ImbuedElementType, imbueElementRequest.TargetUnit);
            this.OnRequestResolverComplete?.Invoke();
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
            this.completionManager.OnActionComplete();
        }
        private void ProcessAddStatusRequest(BaseRequest request)
        {
            ApplyStatusRequest applyStatusRequest = request as ApplyStatusRequest;
            UnityEngine.Debug.LogError("Adding Status within AttackActionContext");
            Status.BaseStatus statusAdded = Status.UnitStatusHandler.AddStatusForUnitIndex(applyStatusRequest.TargetUnit, applyStatusRequest.ApplingStatus);
            if (statusAdded == null) { return; }

            UnityEngine.Debug.LogError($"Added Status: {statusAdded.StatusName}");

            Status.CombatStatusHandler.Instance.AddStatusEffect(applyStatusRequest, statusAdded);
            this.OnRequestResolverComplete?.Invoke();
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
            this.completionManager.OnActionComplete();
        }
        private void ProcessRemoveStatusRequest(BaseRequest request)
        {
            RemoveStatusRequest removeStatusRequest = request as RemoveStatusRequest;
            Status.BaseStatus statusRemoved = Status.UnitStatusHandler.RemoveStatusForUnitIndex(removeStatusRequest.TargetUnit, removeStatusRequest.RemovingStatus);
            if (statusRemoved == null) { return; }

            Status.CombatStatusHandler.Instance.RemoveStatusEffect(removeStatusRequest, statusRemoved);
            this.OnRequestResolverComplete?.Invoke();
        }
    }
}