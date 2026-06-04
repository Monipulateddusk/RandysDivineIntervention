namespace TurnBased.Combat
{
    public class TurnOrderCombatHandler 
    {
        public static event System.Action OnTurnOrderAttacksFullyResolved;

        private static TurnOrderCombatHandler instance;
        public static TurnOrderCombatHandler Instance
        {
            get
            {
                return instance;
            }
            set
            {
                if (instance == null)
                {
                    instance = value;
                }
            }
        }

        private UnitIndex currentUnitIndexResolving;

        public void Awake()
        {
            IntentionCombatResolver.OnResolvingStatesComplete += OnResolvingStatesComplete;
            if (instance == null)
            {
                instance = this;
            }
        }

        public void OnDestroy()
        {
            IntentionCombatResolver.OnResolvingStatesComplete -= OnResolvingStatesComplete;
            if (instance != null && instance == this)
            {
                instance = null;
            }
            OnTurnOrderAttacksFullyResolved = null;
        }

        public void StartTurnOrderCombat()
        {
            UnityEngine.Debug.LogWarning($"Starting combat resolution!");
            ContinueNextUnit();
        }

        private void ContinueNextUnit()
        {
            UnitIndex? turnOrderNextUnit = TurnOrder.TurnOrderManager.Instance.PopNextUnitInTurnOrder();

            UnityEngine.Debug.LogWarning($"Next turn order unit is: " + turnOrderNextUnit);

            if (!turnOrderNextUnit.HasValue) { UnityEngine.Debug.LogError("ERROR — ATTACK RESOLUTION MANAGER: CANNOT PROCESS NEXT UNIT IN TURN ORDER THAT DOESN'T EXIST!"); return; }
            this.currentUnitIndexResolving = turnOrderNextUnit.Value;


            ProcessNextUnitInTurnOrder();
        }

        private void ProcessNextUnitInTurnOrder()
        {
            if (!Intention.UnitIntentionManager.Instance.TryGetIntention(this.currentUnitIndexResolving, out Intention.UnitIntention intention)) { return; }

            UnityEngine.Debug.LogWarning($"Processing next unit in turn order. Current unit index resolving is: {this.currentUnitIndexResolving.Index} and resolving state unit index is: {intention.GetCurrentResolvingState().ResolvingSource.SourceUnitIndex.Index}");

            AttackResolution.CombatResolvingRequest request = Combat.IntentionCombatResolverUtility.AddToCombatResolverBack(intention.GetCurrentResolvingState());

            request.OnRequestComplete += WhenRequestComplete; 
        }

        private void WhenRequestComplete(AttackResolution.CombatResolvingRequest request)
        {
            request.OnRequestComplete -= WhenRequestComplete;

            TurnOrder.TurnOrderManager.Instance.ResetCurrentUnit();

            UnityEngine.Debug.LogWarning($"Done processing next unit in turn order");

            if (IsProcessingIntentContinuing())
            {
                UnityEngine.Debug.LogWarning($"Intentions continuing!");

                ContinueNextUnit();
            }
            else
            {
                UnityEngine.Debug.LogError($"ALL ATTACKS DONE!!! ");
            }
        }

        private void OnResolvingStatesComplete()
        {
            OnTurnOrderAttacksFullyResolved?.Invoke();
        }


        private bool IsProcessingIntentContinuing() => TurnOrder.TurnOrderManager.Instance.GetTurnOrderList().Count > 0;
    }



    
}