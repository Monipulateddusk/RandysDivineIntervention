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
            public int AttackActionTypeInstanceDamageCount      { get; }
            public int AttackActionTypeInstanceHealingCount     { get; }
            public int AttackActionTypeInstanceStatusCount      { get; }
            public int AttackActionTypeInstanceImbuementCount   { get; }

            public float DamageValueAmount { get; }
            public float HealingValueAmount { get; }

            public MoveValueAmounts(int damageInstanceCount, int healingInstanceCount, int statusInstanceCount, int imbuementInstanceCount, float damage, float healing)
            {
                this.AttackActionTypeInstanceDamageCount = damageInstanceCount;
                this.AttackActionTypeInstanceHealingCount = healingInstanceCount;
                this.AttackActionTypeInstanceStatusCount = statusInstanceCount;
                this.AttackActionTypeInstanceImbuementCount = imbuementInstanceCount;


                this.DamageValueAmount = damage;
                this.HealingValueAmount = healing;
            }
        }

        public static bool TryGetTotalValuesOfMoveFromSourceIndexToTarget(UnitIndex sourceUnitIndex, IBattleMove sourceBattleMove, out MoveValueAmounts valueAmounts)
        {
            int damageAttackActionInstanceTotal = 0;
            int healingAttackActionInstanceTotal = 0;
            int statusAttackActionInstanceTotal = 0;
            int imbueAttackActionInstanceTotal = 0;


            float damageValueTotal = 0;
            float healingValueTotal = 0;

            valueAmounts = default;
            if (!IntentionCombatResolver.TryGetUnitDataForCombatResolution(sourceUnitIndex, out IntentionCombatResolver.UnitDataForCombatResolution combatResData)) { return false; }

            AttackResolutionInfo resolutionInfo = sourceBattleMove.ExecuteMove(combatResData.SourceUnitData, combatResData.AllyUnitData, combatResData.TargetUnitData);

            foreach (AttackStep step in resolutionInfo.Steps)
            {
                foreach (AttackAction action in step.Actions)
                {
                    switch (action.Type)
                    {
                        case AttackActionType.DAMAGE:
                            damageAttackActionInstanceTotal++;
                            damageValueTotal += action.Value;
                            break;
                        case AttackActionType.HEALING:
                            healingAttackActionInstanceTotal++;
                            healingValueTotal += action.Value;
                            break;
                        case AttackActionType.STATUS_EFFECT:
                            statusAttackActionInstanceTotal++;

                            break;
                        case AttackActionType.IMBUE_ENVIRONMENTS:
                            imbueAttackActionInstanceTotal++;

                            break;
                    }              
                }
            }

            valueAmounts = new(damageAttackActionInstanceTotal, healingAttackActionInstanceTotal, statusAttackActionInstanceTotal, imbueAttackActionInstanceTotal, damageValueTotal, healingValueTotal);

            return true;
        }

        public static void GetAttackActionTypeFromMoveValueAmounts(MoveValueAmounts valueAmounts, out AttackActionType type, out float majorityValue)
        {
            type = AttackActionType.DAMAGE;
            majorityValue = 0;

            /*  Check what value has the majority between Damage and Health. Depending on that, we report that value back. Makes UI visualisation much easier.  */
            if (valueAmounts.DamageValueAmount > valueAmounts.HealingValueAmount)
            {
                type = AttackActionType.DAMAGE;
                majorityValue = valueAmounts.DamageValueAmount;
            }
            else if (valueAmounts.DamageValueAmount < valueAmounts.HealingValueAmount)
            {
                type = AttackActionType.HEALING;
                majorityValue = valueAmounts.HealingValueAmount;
            }
            else
            {
                type = AttackActionType.IMBUE_ENVIRONMENTS;
                majorityValue = 0;
            }
        }

    }
}