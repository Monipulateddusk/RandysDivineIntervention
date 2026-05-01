namespace TurnBased.Phases
{
    public class PhaseTaskCompletionManager
    {
        private uint phaseTaskCount;
        private System.Action onPhaseCompletion;

        public PhaseTaskCompletionManager(System.Action actionUponPhaseCompletion)
        {
            this.phaseTaskCount = 0;
            this.onPhaseCompletion = actionUponPhaseCompletion;
        }

        ~PhaseTaskCompletionManager()
        {
            phaseTaskCount = 0;
            onPhaseCompletion = null;
        }

        public void AddAction()
        {
            this.phaseTaskCount++;
        }

        public void OnActionComplete()
        {
            this.phaseTaskCount--;

            if (this.phaseTaskCount == 0)
            {
                this.onPhaseCompletion?.Invoke();
            }
        }
    }
}