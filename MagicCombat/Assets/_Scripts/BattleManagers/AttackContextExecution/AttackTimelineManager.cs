
namespace TurnBased.Combat
{
    public static class AttackTimelineManager
    {
        public static async System.Threading.Tasks.Task ResolveCombatAttackTimeline(System.Collections.Generic.List<System.Collections.Generic.List<AttackEvent>> attackEventTimeline)
        {
            if (attackEventTimeline.Count <= 0) { return; }
            AttackActionExecutionContext executionContext = new();

            foreach (System.Collections.Generic.List<AttackEvent> listOfEvents in attackEventTimeline)
            {
                foreach (AttackEvent attackEvent in listOfEvents)
                {
                    UnityEngine.Debug.LogError("Processing Attack");
                    executionContext.HandleAttackEvent(attackEvent);
                }
                await System.Threading.Tasks.Task.Delay(500);
            }
        }
    }
}