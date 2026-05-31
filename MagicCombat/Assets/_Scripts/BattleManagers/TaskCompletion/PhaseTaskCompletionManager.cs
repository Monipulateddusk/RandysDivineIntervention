namespace TurnBased.Phases
{
    public class PhaseTaskCompletionManager
    {
        private uint phaseTaskCount;
        private System.Action OnPhaseCompletion;

        public PhaseTaskCompletionManager(System.Action actionUponPhaseCompletion)
        {
            this.phaseTaskCount = 0;
            this.OnPhaseCompletion = actionUponPhaseCompletion;
        }

        ~PhaseTaskCompletionManager()
        {
            phaseTaskCount = 0;
            OnPhaseCompletion = null;
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
                this.OnPhaseCompletion?.Invoke();
            }
        }
    }
}