
namespace TurnBased.Combat
{
    public static class AttackTimelineManager
    {
        public static async System.Threading.Tasks.Task ResolveCombatAttackTimeline(System.Collections.Generic.List<System.Collections.Generic.List<AttackResolution.AttackEvent>> attackEventTimeline)
        {
            if (attackEventTimeline.Count <= 0) { return; }
            AttackResolution.AttackActionExecutionContext executionContext = new();

            foreach (System.Collections.Generic.List<AttackResolution.AttackEvent> listOfEvents in attackEventTimeline)
            {
                foreach (AttackResolution.AttackEvent attackEvent in listOfEvents)
                {
                    UnityEngine.Debug.LogError("Processing Attack");
                    await executionContext.HandleAttackEvent(attackEvent);
                }
                await System.Threading.Tasks.Task.Delay(500);
            }
        }
    }
}