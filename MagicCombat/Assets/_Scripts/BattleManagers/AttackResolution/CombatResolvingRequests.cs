namespace TurnBased.AttackResolution {

    public class CombatResolvingRequest
    {
        public event System.Action<CombatResolvingRequest> OnRequestComplete;
        public Intention.ResolvingState ResolvingState { get; }

        public CombatResolvingRequest(Intention.ResolvingState resolvingState)
        {
            this.ResolvingState = resolvingState;
        }

        public void CompleteRequest()
        {
            OnRequestComplete?.Invoke(this);
        }
    }
}