namespace TurnBased.Presentation
{
    public class StatusPresentationManager
    {
        private ParticleSystemManager particleSystemManager;
        private System.Collections.Generic.Dictionary<TurnBased.Status.BaseStatus, ParticleSystemController> StatusEffects;
        private ParticlesCollection_SO _ParticlesData;


        public void Awake(ParticleSystemManager particleManager, ParticlesCollection_SO particlesData)
        {
            EventHookSystem.OnDamageRequestResolved         += EventHookSystem_OnDamageRequestResolved;
            EventHookSystem.OnApplyStatusRequestResolved    += EventHookSystem_OnApplyStatusRequestResolved; 

            this._ParticlesData = particlesData;
            this.particleSystemManager = particleManager;

            this.StatusEffects = new() {
                { new Status.PoisonStatus(),    particlesData.PoisonParticlePrefab  },
                { new Status.BurnStatus(),      particlesData.ApplyBurnParticleSystem    },      
            };
        }

        public void OnDestroy()
        {
            EventHookSystem.OnDamageRequestResolved -= EventHookSystem_OnDamageRequestResolved;
            EventHookSystem.OnApplyStatusRequestResolved -= EventHookSystem_OnApplyStatusRequestResolved;
        }

        private void EventHookSystem_OnDamageRequestResolved(AttackResolution.RequestTaskCompletionManager completionManager, AttackResolution.DamageRequest request)
        {
            /*  Determine what status made this damage request to visualise it. */
            if (request.ResolvingSource.Type == DamageOriginType.Status)
            {
                if (!StationManager.Instance.TryGetBattleUnitOfIndex(request.TargetUnit, out BaseBattleUnit battleUnit)) { return; }
                _ = VisualiseStatusRequest(completionManager, request, battleUnit);
            }
        }

        private void EventHookSystem_OnApplyStatusRequestResolved(AttackResolution.RequestTaskCompletionManager completionManager, AttackResolution.ApplyStatusRequest request)
        {
            if (!StationManager.Instance.TryGetBattleUnitOfIndex(request.TargetUnit, out BaseBattleUnit battleUnit)) { return; }

            _ = VisualiseStatusRequest(completionManager, request, battleUnit);
        }

        private void CompleteRequest(AttackResolution.RequestTaskCompletionManager completionManager)
        {
            completionManager.OnActionComplete();
        }

        private async System.Threading.Tasks.Task VisualiseStatusRequest(AttackResolution.RequestTaskCompletionManager completionManager, AttackResolution.DamageRequest request, BaseBattleUnit battleUnit)
        {
            if (!TryGetParticleSystemOfStatusChildSubClass(request.ResolvingSource.SourceStatus, out ParticleSystemController particleSystemController)) { return; }
            if (particleSystemController == null) { return; }

            await this.particleSystemManager.SpawnParticleSystem(completionManager, particleSystemController, battleUnit.transform.position);
        }

        private async System.Threading.Tasks.Task VisualiseStatusRequest(AttackResolution.RequestTaskCompletionManager completionManager, AttackResolution.ApplyStatusRequest request, BaseBattleUnit battleUnit)
        {
            if (!TryGetParticleSystemOfStatusChildSubClass(request.ApplingStatus, out ParticleSystemController particleSystemController)) { return; }
            if (particleSystemController == null) { return; }

            await this.particleSystemManager.SpawnParticleSystem(completionManager, particleSystemController, battleUnit.transform.position);
        }

        private async System.Threading.Tasks.Task SpawnParticleSystemOfStatus(AttackResolution.RequestTaskCompletionManager completionManager, ParticleSystemController particleSystemController, BaseBattleUnit battleUnit)
        {
            completionManager.AddAction();

            UnityEngine.GameObject instanciatedParticleSystem = UnityEngine.GameObject.Instantiate(particleSystemController.gameObject, battleUnit.transform.position, UnityEngine.Quaternion.identity);
            instanciatedParticleSystem.GetComponent<ParticleSystemController>().PlayParticleSystem();

            await System.Threading.Tasks.Task.Delay(800);

            /*  Destroy the Prefab after a second and a half.  */
            UnityEngine.GameObject.Destroy(instanciatedParticleSystem);
            CompleteRequest(completionManager);
        }

        private bool TryGetParticleSystemOfStatusChildSubClass(Status.BaseStatus baseStatusOfRequest, out ParticleSystemController particleSystemPrefab)
        {
            particleSystemPrefab = default;

            foreach (System.Collections.Generic.KeyValuePair<Status.BaseStatus, ParticleSystemController> statusKeyValuePair in this.StatusEffects)
            {
                Status.BaseStatus status = statusKeyValuePair.Key;
                ParticleSystemController particleSystem = statusKeyValuePair.Value;

                if (baseStatusOfRequest.GetType() == status.GetType())
                {
                    particleSystemPrefab = particleSystem;
                    return true;
                }
            }

           return false;
        }
    }
}