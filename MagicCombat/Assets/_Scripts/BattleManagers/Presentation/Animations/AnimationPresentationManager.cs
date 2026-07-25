namespace TurnBased.Presentation
{
    public class AnimationPresentationManager
    {
        private ParticleSystemManager particleSystemManager;
        private ParticlesCollection_SO particlesCollectionData;

        public void Awake(ParticleSystemManager pSM, ParticlesCollection_SO particleData)
        {
            this.particleSystemManager = pSM;   
            this.particlesCollectionData = particleData;

            AttackResolution.AttackEventResolver.OnResolveAttackEventSequenceSection += AttackEventResolver_OnResolveAttackEventSequenceSection;
            StationSelectorManager.OnSelectionChange += StationSelectorManager_OnSelectionChange;
        }


        public void OnDestroy()
        {
            AttackResolution.AttackEventResolver.OnResolveAttackEventSequenceSection -= AttackEventResolver_OnResolveAttackEventSequenceSection;
            StationSelectorManager.OnSelectionChange -= StationSelectorManager_OnSelectionChange;
        }

        private void StationSelectorManager_OnSelectionChange(StationIndex selectedStation, StationIndex? previousSelectedStation)
        {
            if (previousSelectedStation != null) { SetUnitAnimatorSelectedStatus(previousSelectedStation.Value, false); }
            SetUnitAnimatorSelectedStatus(selectedStation, true);
        }

        private void SetUnitAnimatorSelectedStatus(StationIndex stationIndex, bool isSelected)
        {
            if (!StationManager.Instance.TryGetUnitIndexOnStation(stationIndex, out UnitIndex unitIndex)) { return; }
            AnimationUnitManager.Instance.SetBooleanFlagForUnitAnimator(unitIndex, "IsSelected", isSelected);
        }

        private void AttackEventResolver_OnResolveAttackEventSequenceSection(
            AttackResolution.ResolvingStatePhaseCompletionManager completionManager, 
            Intention.ResolvingSource source, 
            AttackResolution.AttackAction action,
            System.Collections.Generic.List<AttackResolution.AttackEvent> attackEventList,
            int resolvingStatesCount,
            int resolvingStateIndex,
            int timelineCount,
            int timelineIndex)

        {
            /*  If we are animating a UnitMove, we want to confirm if we are the start or the end.  */
            if (source.Type == DamageOriginType.UnitMove)
            {
                _ = ProcessUnitAnimations(completionManager, source, action, attackEventList, resolvingStatesCount, resolvingStateIndex, timelineCount, timelineIndex);
            }
        }

        private async System.Threading.Tasks.Task ProcessUnitAnimations(
            AttackResolution.ResolvingStatePhaseCompletionManager completionManager,
            Intention.ResolvingSource source,
            AttackResolution.AttackAction action,
            System.Collections.Generic.List<AttackResolution.AttackEvent> attackEventList,
            int resolvingStatesCount,
            int resolvingStateIndex,
            int timelineCount,
            int timelineIndex)
        {
            completionManager.AddAction();

            if (IsStartingAnimation(resolvingStateIndex, timelineIndex))
            {
                AnimationUnitManager.Instance.SetBooleanFlagForUnitAnimator(source.SourceUnitIndex, "IsAttacking", true);
                PlayAnimationBasedOnMoveType(source);


                await AwaitAnimationDuration(source);
            }

            await AwaitTriggerFlagDuration(source);
            await AwaitParticleEffectsForTargets(source, attackEventList);

            if (IsEndingAnimation(resolvingStatesCount, resolvingStateIndex, timelineCount, timelineIndex))
            {
                AnimationUnitManager.Instance.SetBooleanFlagForUnitAnimator(source.SourceUnitIndex, "IsAttacking", false);

                await AwaitAnimationDuration(source);
            }

            completionManager.OnActionComplete();
        }

        private void PlayAnimationBasedOnMoveType(Intention.ResolvingSource source)
        {
            MoveAnimationType animationType = source.SourceUnitMove.GetAnimationType();

            switch (animationType)
            {
                case MoveAnimationType.LightAttack:
                    AnimationUnitManager.Instance.PlayAnimationForUnit(source.SourceUnitIndex, "LightAttack");
                    return;
                case MoveAnimationType.HeavyAttack:
                    AnimationUnitManager.Instance.PlayAnimationForUnit(source.SourceUnitIndex, "HeavyAttack");
                    return;
                case MoveAnimationType.Imbuement:
                    AnimationUnitManager.Instance.PlayAnimationForUnit(source.SourceUnitIndex, "Imbuement");
                    return;
                default:
                    AnimationUnitManager.Instance.PlayAnimationForUnit(source.SourceUnitIndex, "LightAttack");
                    return;
            }



        }

        private async System.Threading.Tasks.Task AwaitAnimationDuration(Intention.ResolvingSource source)
        {
            await System.Threading.Tasks.Task.Yield();

            AnimationUnitManager.Instance.GetAnimationDurationForUnitAnimator(source.SourceUnitIndex, out float animationDuration);

            await System.Threading.Tasks.Task.Delay((int)(animationDuration * 1000));
        }

        private async System.Threading.Tasks.Task AwaitTriggerFlagDuration(Intention.ResolvingSource source)
        {
            AnimationUnitManager.Instance.SetTriggerFlagForUnitAnimator(source.SourceUnitIndex, "TriggerAction");
            await System.Threading.Tasks.Task.Yield();
            AnimationUnitManager.Instance.GetDurationToNextAnimationEvent(source.SourceUnitIndex, out float triggerFlagDuration);
            await System.Threading.Tasks.Task.Delay((int)(triggerFlagDuration * 1000));
        }

        private async System.Threading.Tasks.Task AwaitParticleEffectsForTargets(Intention.ResolvingSource source, System.Collections.Generic.List<AttackResolution.AttackEvent> attackEventList)
        {
            /*  For each target in the attackEventList, spawn a particle and wait until the last one has resolved.  */
            foreach (AttackResolution.AttackEvent ev in attackEventList)
            {
                if (ev.TargetUnitIndex.Index == source.SourceUnitIndex.Index) { continue; }
                if (!StationManager.Instance.TryGetBattleUnitOfIndex(ev.TargetUnitIndex, out BaseBattleUnit battleUnit)) { continue; }

                _ = this.particleSystemManager.SpawnParticleSystem(this.particlesCollectionData.CollisionParticlePrefab, battleUnit.transform.position);
            }

            await System.Threading.Tasks.Task.Delay(800);
        }


        private bool IsStartingAnimation(int resolvingStateIndex, int timelineIndex) => resolvingStateIndex == 0 && timelineIndex == 0;
        
        private bool IsEndingAnimation(int resolvingStatesCount, int resolvingStateIndex, int timelineCount, int timelineIndex) => ( ( resolvingStatesCount - 1 ) <= resolvingStateIndex ) && ( ( timelineCount - 1 ) <= timelineIndex );



    }
}