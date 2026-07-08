namespace TurnBased.Presentation
{
    public class ParticleSystemManager
    {
        private StatusPresentationManager _StatusPresentationManager;
        private ParticlesCollection_SO _ParticlesData;



        public void Awake(ParticlesCollection_SO data)
        {
            this._ParticlesData = data;

            EventHookSystem.OnDamageRequestResolved += EventHookSystem_OnDamageRequestResolved;

            this._StatusPresentationManager = new();
            this._StatusPresentationManager.Awake(data);
        }

        private void EventHookSystem_OnDamageRequestResolved(AttackResolution.RequestTaskCompletionManager completionManager, AttackResolution.DamageRequest damageRequest)
        {
            if (damageRequest.ResolvingSource.Type == DamageOriginType.UnitMove)
            {
                if (!StationManager.Instance.TryGetBattleUnitOfIndex(damageRequest.TargetUnit, out BaseBattleUnit battleUnit)) { return; }

                completionManager.AddAction();
                _ = SpawnParticleSystem(completionManager, damageRequest, this._ParticlesData.CollisionParticlePrefab, battleUnit);
            }
        }

        public void OnDestroy()
        {
            this._StatusPresentationManager.OnDestroy();
            this._StatusPresentationManager = null;
        }


        private async System.Threading.Tasks.Task SpawnParticleSystem(AttackResolution.RequestTaskCompletionManager completionManager, AttackResolution.DamageRequest damageRequest, ParticleSystemController particleSystemController, BaseBattleUnit battleUnit)
        {
            UnityEngine.GameObject instanciatedParticleSystem = UnityEngine.GameObject.Instantiate(particleSystemController.gameObject, battleUnit.transform.position, UnityEngine.Quaternion.identity);
            instanciatedParticleSystem.GetComponent<ParticleSystemController>().PlayParticleSystem();

            await System.Threading.Tasks.Task.Delay(800);

            /*  Destroy the Prefab after a second and a half.  */
            UnityEngine.GameObject.Destroy(instanciatedParticleSystem);

            completionManager.OnActionComplete(damageRequest);
        }

    }
}