namespace TurnBased.Phases
{
    public class PhaseTaskCompletionManager
    {
        private int phaseTaskCount;
        private System.Action OnPhaseCompletion;

        public PhaseTaskCompletionManager(System.Action actionUponPhaseCompletion)
        {
            this.phaseTaskCount = 0;
            this.OnPhaseCompletion = actionUponPhaseCompletion;
        }

        ~PhaseTaskCompletionManager()
        {
            this.phaseTaskCount = 0;
            this.OnPhaseCompletion = null;
        }

        public void AddAction()
        {
            this.phaseTaskCount++;
        }

        public void OnActionComplete()
        {
            this.phaseTaskCount--;

            if (this.phaseTaskCount <= 0)
            {
                this.OnPhaseCompletion?.Invoke();
            }
        }
    }
}

namespace TurnBased.AttackResolution
{
    public class RequestTaskCompletionManager
    {
        private int requestTaskCount;
        private System.Action<BaseRequest> OnRequestTaskCompletion;

        public RequestTaskCompletionManager(System.Action<BaseRequest> onRequestTaskCompletion)
        {
            this.requestTaskCount = 0;
            this.OnRequestTaskCompletion = onRequestTaskCompletion;
        }
        ~RequestTaskCompletionManager()
        {
            this.requestTaskCount = 0;
            this.OnRequestTaskCompletion = null;
        }
        public void AddAction()
        {
            this.requestTaskCount++;
        }

        public void OnActionComplete(BaseRequest baseRequest)
        {
            this.requestTaskCount--;

            if (this.requestTaskCount <= 0)
            {
                this.OnRequestTaskCompletion?.Invoke(baseRequest);
            }
        }
    }
}