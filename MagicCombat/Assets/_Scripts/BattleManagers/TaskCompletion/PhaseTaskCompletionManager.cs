using TurnBased.Intention;

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
        private BaseRequest request;


        public RequestTaskCompletionManager(BaseRequest request, System.Action<BaseRequest> onRequestTaskCompletion)
        {
            this.requestTaskCount = 0;
            this.request = request;
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

        public void OnActionComplete()
        {
            this.requestTaskCount--;

            if (this.requestTaskCount <= 0)
            {
                this.OnRequestTaskCompletion?.Invoke(this.request);
            }
        }
    }

    public class ResolvingStatePhaseCompletionManager
    {
        private int requestTaskCount;
        private System.Action OnResolvingStateCompletion;

        public ResolvingStatePhaseCompletionManager(System.Action onRequestTaskCompletion)
        {
            this.requestTaskCount = 0;
            this.OnResolvingStateCompletion = onRequestTaskCompletion;
        }
        ~ResolvingStatePhaseCompletionManager()
        {
            this.requestTaskCount = 0;
            this.OnResolvingStateCompletion = null;
        }
        public void AddAction()
        {
            this.requestTaskCount++;
        }

        public void OnActionComplete()
        {
            this.requestTaskCount--;

            if (this.requestTaskCount <= 0)
            {
                this.OnResolvingStateCompletion?.Invoke();
            }
        }
    }
}