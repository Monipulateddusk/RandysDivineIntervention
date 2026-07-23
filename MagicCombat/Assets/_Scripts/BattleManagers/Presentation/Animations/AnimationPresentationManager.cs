using static Unity.VisualScripting.Member;

namespace TurnBased.Presentation
{
    public class AnimationPresentationManager
    {
        public void Awake()
        {
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
                AnimationUnitManager.Instance.PlayAnimationForUnit(source.SourceUnitIndex, "LightAttack");

                await System.Threading.Tasks.Task.Yield();


                AnimationUnitManager.Instance.GetAnimationDurationForUnitAnimator(source.SourceUnitIndex, out float lightAttackIdleDuration);


                await System.Threading.Tasks.Task.Delay((int)(lightAttackIdleDuration * 1000));
            }

            AnimationUnitManager.Instance.SetTriggerFlagForUnitAnimator(source.SourceUnitIndex, "TriggerAction");
            await System.Threading.Tasks.Task.Yield();
            AnimationUnitManager.Instance.GetDurationToNextAnimationEvent(source.SourceUnitIndex, out float lightAttackSwingDuration);


            await System.Threading.Tasks.Task.Delay((int)(lightAttackSwingDuration * 1000));
            completionManager.OnActionComplete();

            if (IsEndingAnimation(resolvingStatesCount, resolvingStateIndex, timelineCount, timelineIndex))
            {
                AnimationUnitManager.Instance.SetBooleanFlagForUnitAnimator(source.SourceUnitIndex, "IsAttacking", false);

                await System.Threading.Tasks.Task.Yield();
                AnimationUnitManager.Instance.GetAnimationDurationForUnitAnimator(source.SourceUnitIndex, out float lightAttackEndDuration);

                await System.Threading.Tasks.Task.Delay((int)(lightAttackEndDuration * 1000));
            }


        }


        private bool IsStartingAnimation(int resolvingStateIndex, int timelineIndex) => resolvingStateIndex == 0 && timelineIndex == 0;
        
        private bool IsEndingAnimation(int resolvingStatesCount, int resolvingStateIndex, int timelineCount, int timelineIndex) => ( ( resolvingStatesCount - 1 ) <= resolvingStateIndex ) && ( ( timelineCount - 1 ) <= timelineIndex );



    }
}