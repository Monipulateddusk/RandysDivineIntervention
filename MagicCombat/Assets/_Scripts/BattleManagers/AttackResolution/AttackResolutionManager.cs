using TurnBased.Combat;

namespace TurnBased.AttackResolution
{
    public class AttackResolutionManager
    {
        public static event System.Action OnAllAttacksFullyResolved;
        private UnitIndex unitIndexToProcess;


        private static AttackResolutionManager instance;
        public static AttackResolutionManager Instance
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

        public void Awake()
        {
            instance = this;
        }

        public void OnDestroy()
        {
            if (instance != null && instance == this)
            {
                instance = null;
            }
        }

        /// <summary>
        /// When called, goes through the TurnOrder Queue to process each Unit's intentions.
        /// </summary>
        public void StartCombatResolution()
        {
            UnityEngine.Debug.LogWarning($"Starting combat resolution!");
            ContinueNextUnit();
        }

        private void ContinueNextUnit()
        {
            UnitIndex? turnOrderNextUnit = TurnOrder.TurnOrderManager.Instance.PopNextUnitInTurnOrder();

            UnityEngine.Debug.LogWarning($"Next turn order unit is: "+ turnOrderNextUnit);

            if (!turnOrderNextUnit.HasValue) { UnityEngine.Debug.LogError("ERROR — ATTACK RESOLUTION MANAGER: CANNOT PROCESS NEXT UNIT IN TURN ORDER THAT DOESN'T EXIST!"); return; }
            this.unitIndexToProcess = turnOrderNextUnit.Value;


            _ = ProcessNextUnitInTurnOrder();
        }

        private async System.Threading.Tasks.Task ProcessNextUnitInTurnOrder()
        {
            UnityEngine.Debug.LogWarning($"Processing next unit in turn order");
            await IntentionCombatResolver.ProcessAttack(this.unitIndexToProcess);

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
                OnAllAttacksFullyResolved?.Invoke();
            }
        }

        private bool IsProcessingIntentContinuing() => TurnOrder.TurnOrderManager.Instance.GetTurnOrderList().Count > 0;


    }


    public static class CombatDamageUtility
    {
        public class MoveValueAmounts
        {
            public float HealingAmount { get; }
            public float DamageAmount { get; }

            public MoveValueAmounts(float damage, float healing)
            {
                this.DamageAmount = damage;
                this.HealingAmount = healing;
            }
        }

        public static bool TryGetTotalValuesOfMoveFromSourceIndexToTarget(UnitIndex sourceUnitIndex, IBattleMove sourceBattleMove, out MoveValueAmounts valueAmounts)
        {
            float damageTotal = 0;
            float healingTotal = 0;

            valueAmounts = default;
            if (!IntentionCombatResolver.TryGetUnitDataForCombatResolution(sourceUnitIndex, out IntentionCombatResolver.UnitDataForCombatResolution combatResData)) { return false; }

            AttackResolutionInfo resolutionInfo = sourceBattleMove.ExecuteMove(combatResData.SourceUnitData, combatResData.AllyUnitData, combatResData.TargetUnitData);

            foreach (AttackStep step in resolutionInfo.Steps)
            {
                foreach (AttackAction action in step.Actions)
                {
                    if (action.Type == AttackActionType.DAMAGE)
                    {
                        damageTotal += action.Value;
                    }
                    else if (action.Type == AttackActionType.HEALING)
                    {
                        healingTotal += action.Value;
                    }                  
                }
            }

            valueAmounts = new(damageTotal, healingTotal);

            return true;
        }
    }
}