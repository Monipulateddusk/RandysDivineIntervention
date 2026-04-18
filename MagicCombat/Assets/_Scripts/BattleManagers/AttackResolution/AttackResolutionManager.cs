using System.Diagnostics;
using System.Threading.Tasks;

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
            await ProcessUnitIndexIntent(this.unitIndexToProcess);

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

        private async System.Threading.Tasks.Task ProcessUnitIndexIntent(UnitIndex unitIndex)
        {
            if (!Intention.UnitIntentionManager.Instance.TryGetIntention(unitIndex, out Intention.UnitIntention intention)) { UnityEngine.Debug.LogError("ERROR — ATTACK RESOLUTION MANAGER: UNABLE TO RETRIEVE INTENTION OF UNIT_INDEX!"); return; }

            UnityEngine.Debug.LogWarning($"Trying to exectute Move named: {intention.MoveSelection.GetMoveName()}! ");

            /*  Process the move based on the information for that UnitIndex.   */
            AttackResolutionInfo resolutionInfo = intention.MoveSelection.ExecuteMove();

            UnityEngine.Debug.LogWarning($"Executing Move: {intention.MoveSelection.GetMoveName()}! ");

            if (resolutionInfo == null) { UnityEngine.Debug.LogError("ERROR — ATTACK RESOLUTION MANAGER: RESOLUTION INFO OF MOVE IS NULL!"); }

            UnityEngine.Debug.LogWarning($"Resolution info is not null. ");

            for (int i = 0; i < resolutionInfo.Steps.Count; i++){
                CombatAttackHandler.ProcessAttackStep(resolutionInfo, intention);
                UnityEngine.Debug.LogWarning($"Processed attack step for UnitIndex: {unitIndex.Index}");
                await Task.Delay(3000);
            }      
        }

        private bool IsProcessingIntentContinuing() => TurnOrder.TurnOrderManager.Instance.GetTurnOrderList().Count > 0;


    }
}