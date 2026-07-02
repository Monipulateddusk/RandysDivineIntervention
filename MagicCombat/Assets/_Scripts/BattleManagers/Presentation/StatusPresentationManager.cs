namespace TurnBased.Presentation
{
    public class StatusPresentationManager
    {
        public System.Collections.Generic.Dictionary<TurnBased.Status.BaseStatus, ParticleSystemController> StatusEffects;
        private AttackResolution.RequestTaskCompletionManager _RequestCompleitonManager;
        private ParticlesCollection_SO _ParticlesData;

        public void Awake(ParticlesCollection_SO particlesData)
        {
            EventHookSystem.OnDamageRequestResolved += EventHookSystem_OnDamageRequestResolved;
            this._ParticlesData = particlesData;

            this.StatusEffects = new() {
                { new Status.PoisonStatus(),    particlesData.PoisonParticlePrefab  },
                { new Status.BurnStatus(),      particlesData.BurnParticlePrefab    },      
            };
        }

        public void OnDestroy()
        {
            EventHookSystem.OnDamageRequestResolved -= EventHookSystem_OnDamageRequestResolved;
        }

        private void EventHookSystem_OnDamageRequestResolved(AttackResolution.RequestTaskCompletionManager completionManager, AttackResolution.DamageRequest request)
        {
            this._RequestCompleitonManager = completionManager;

            /*  Determine what status made this damage request to visualise it. */
            if (request.ResolvingSource.Type == DamageOriginType.Status)
            {
                VisualiseStatusDamageRequest(request);
            }
        }

        private void VisualiseStatusDamageRequest(AttackResolution.DamageRequest request)
        {
            if (!StationManager.Instance.TryGetBattleUnitOfIndex(request.TargetUnit, out BaseBattleUnit battleUnit)) { return; }
            if (!TryGetParticleSystemOfStatusChildSubClass(request.ResolvingSource.SourceStatus, out ParticleSystemController particleSystemController)) { return; }

            UnityEngine.GameObject instanciatedParticleSystem = UnityEngine.GameObject.Instantiate(particleSystemController.gameObject, battleUnit.transform.position, UnityEngine.Quaternion.identity);
            instanciatedParticleSystem.GetComponent<ParticleSystemController>().PlayParticleSystem();


            /*  Destroy the Prefab after a second and a half.  */
            UnityEngine.GameObject.Destroy(instanciatedParticleSystem, 1.5f);
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