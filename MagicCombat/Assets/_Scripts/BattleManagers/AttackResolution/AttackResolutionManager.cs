using System.Collections.Generic;
using TurnBased.Combat;
using TurnBased.UI;

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
        public static string GetMoveDescription(UnitIndex unitIndex, IBattleMove selectedMove)
        {
            /*  Siliently Execute the selected move to retrieve the AttackAction descriptions.  */
            UnitData_SceneData_UnitTurn unitDataSceneData = StationManagerUtilities.CreateUnitDataSceneDataForUnitIndex(unitIndex);
            AttackResolutionInfo resolutionInfo = selectedMove.ExecuteMove(unitDataSceneData.SourceUnitData, unitDataSceneData.AllyUnitData, unitDataSceneData.EnemyUnitData);


            /*  Get a full list of all actions so we can format the string properly.    */
            List<AttackAction> actions = new();
            foreach (AttackStep step in resolutionInfo.Steps)
            {
                foreach (AttackAction action in step.Actions)
                {
                    actions.Add(action);
                }
            }
            /*  If something went wrong, complete the string.   */
            if (actions.Count <= 0) { return $"Do nothing."; }


            string intentionText = string.Empty;
            for (int i = 0; i < actions.Count; i++)
            {
                if (i != 0)
                {
                    intentionText += (i == actions.Count - 1) ? " then, " : ", ";
                }

                intentionText += $"{actions[i].GetDescription()} to {GetTargettingString(actions[i])}";
            }
            return intentionText;
        }

        private static string GetTargettingString(AttackAction action)
        {
            switch (action.AttackTarget)
            {
                default:
                case MoveTarget.Self:
                    return "itself";
                case MoveTarget.SingleAlly:
                    return "a Single Ally";
                case MoveTarget.SingleEnemy:
                    return "a Single Enemy";
                case MoveTarget.AllEnemies:
                    return "All Enemies";
                case MoveTarget.AllAllies:
                    return "All Allies";
                case MoveTarget.Area:
                    return "the Area";
            }
        }

        public static string GetUnitIntentionIntentionString(UnitIndex unitIndex, UnitData unitData, Intention.UnitIntention intention)
        {
            /*  Siliently Execute the selected move to retrieve the AttackAction descriptions.  */
            UnitData_SceneData_UnitTurn unitDataSceneData = StationManagerUtilities.CreateUnitDataSceneDataForUnitIndex(unitIndex);
            AttackResolutionInfo resolutionInfo = intention.MoveSelection.ExecuteMove(unitDataSceneData.SourceUnitData, unitDataSceneData.AllyUnitData, unitDataSceneData.EnemyUnitData);

            /*  Determine who the attack is going to.   There is a limitation here, each attack action can go to multiple targets. So we would need to fix this up to account for different targets for each attack action.   */
            if (!UserInterfaceUtility.TryGetIntentionTargetText(intention, out string unitName)) { return $"{unitData.name} is intending to do nothing."; }

            /*  Get a full list of all actions so we can format the string properly.    */
            List<AttackAction> actions = new();
            foreach (AttackStep step in resolutionInfo.Steps)
            {
                foreach (AttackAction action in step.Actions)
                {
                    actions.Add(action);
                }
            }

            /*  If something went wrong, complete the string.   */
            if (actions.Count <= 0) { return $"{unitData.name} is intending to do nothing."; }


            string intentionText = $"{unitData.name} is intending to ";
            for (int i = 0; i < actions.Count; i++)
            {
                if (i != 0) 
                { 
                    intentionText += (i == actions.Count - 1) ? " then, " : ", "; 
                }
           
                intentionText += $"{actions[i].GetDescription()} to {unitName}";
            }

            return intentionText;
        }
    }
}