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
            System.Collections.Generic.List<AttackResolution.AttackEvent> attackEventList)

        {
            int textIndex = UnityEngine.Random.Range(0, 3);

            string[] text = { "BANG!", "POW!", "ZAM!" };

            UnityEngine.Debug.LogError($"<color=red>{text[textIndex]}</color>");


        }
    }
}