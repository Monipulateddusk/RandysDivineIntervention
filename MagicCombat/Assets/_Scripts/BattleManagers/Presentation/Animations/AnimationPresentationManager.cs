namespace TurnBased.Presentation
{
    public class AnimationPresentationManager
    {
        public void Awake()
        {
            AttackResolution.AttackEventResolver.OnResolveAttackEventSequenceSection += AttackEventResolver_OnResolveAttackEventSequenceSection;
        }

        public void OnDestroy()
        {
            AttackResolution.AttackEventResolver.OnResolveAttackEventSequenceSection -= AttackEventResolver_OnResolveAttackEventSequenceSection;
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
            if (IsStartingAnimation(resolvingStateIndex, timelineIndex))
            {
                UnityEngine.Debug.LogError($"<color=green>Starting Animation: Count is: {resolvingStatesCount} and Index is: {resolvingStateIndex}</color>");
            }


            int textIndex = UnityEngine.Random.Range(0, 3);

            string[] text = { "BANG!", "POW!", "ZAM!" };

            UnityEngine.Debug.LogError($"<color=red>{text[textIndex]}</color>");

            AnimationUnitManager.Instance.PlayAnimationForUnit(source.SourceUnitIndex, "LightAttack", out float duration);
            await AnimationDelay(completionManager, (int)duration);

            if (IsEndingAnimation(resolvingStatesCount, resolvingStateIndex, timelineCount, timelineIndex))
            {
                UnityEngine.Debug.LogError($"<color=purple>Ending Animation: Count is: {resolvingStatesCount} and Index is: {resolvingStateIndex}</color>");
            }
        }


        private bool IsStartingAnimation(int resolvingStateIndex, int timelineIndex) => resolvingStateIndex == 0 && timelineIndex == 0;
        
        private bool IsEndingAnimation(int resolvingStatesCount, int resolvingStateIndex, int timelineCount, int timelineIndex) => ( ( resolvingStatesCount - 1 ) <= resolvingStateIndex ) && ( ( timelineCount - 1 ) <= timelineIndex );




        private async System.Threading.Tasks.Task AnimationDelay(AttackResolution.ResolvingStatePhaseCompletionManager completionManager, int duration)
        {
            completionManager.AddAction();
            await System.Threading.Tasks.Task.Delay(duration * 1000);

            completionManager.OnActionComplete();
        }
    }
}