
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
            Combat.IntentionCombatResolver.OnResolvingStatesComplete += OnResolvingStatesComplete;
            if (Instance == null)
            {
                instance = this;
            }
        }

        public void OnDestroy()
        {
            Combat.IntentionCombatResolver.OnResolvingStatesComplete -= OnResolvingStatesComplete;
            if (instance != null && instance == this)
            {
                instance = null;
            }
            OnAllAttacksFullyResolved = null;

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

            if (!turnOrderNextUnit.HasValue) { UnityEngine.Debug.LogError("ERROR — ATTACK RESOLUTION MANAGER: CANNOT PROCESS NEXT UNIT IN TURN ORDER THAT DOESN'T EXIST!"); return; }
            this.unitIndexToProcess = turnOrderNextUnit.Value;

            ProcessNextUnitInTurnOrder();
        }

        private void ProcessNextUnitInTurnOrder()
        {
            UnityEngine.Debug.LogWarning($"Processing next unit in turn order");
            /*  Get the Resolving state from the Unit's Intention   */
            if (!Intention.UnitIntentionManager.Instance.TryGetIntention(this.unitIndexToProcess, out Intention.UnitIntention intention)) { return; }
            AttackResolution.CombatResolvingRequest request = Combat.IntentionCombatResolverUtility.AddToCombatResolverBack(intention.ResolvingState);

            request.OnRequestComplete -= WhenRequestCompleted;
        }

        private void WhenRequestCompleted(AttackResolution.CombatResolvingRequest request)
        {
            request.OnRequestComplete -= WhenRequestCompleted;

            /*  Once the request is processed, reset the current unit and move on.  */
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
            OnAllAttacksFullyResolved?.Invoke();
        }

        private bool IsProcessingIntentContinuing() => TurnOrder.TurnOrderManager.Instance.GetTurnOrderList().Count > 0;


    }


    public static class CombatDamageUtility
    {
        public static string GetMoveDescription(UnitIndex unitIndex, IBattleMove selectedMove)
        {
            /*  Siliently Execute the selected move to retrieve the AttackAction descriptions.  */
            if (!StationManagerUtilities.TryCreateUnitDataSceneDataForUnitIndex(unitIndex, out TurnBased.AttackResolution.ResolutionSceneData resolutionSceneData)) { return $"Do nothing."; }
            AttackResolutionInfo resolutionInfo = selectedMove.ExecuteMove(resolutionSceneData);

            /*  Get the target groups of the move.  */
            System.Collections.Generic.Dictionary<int, MoveTarget> targetGroupToMoveTargetDictionary = GetMoveTargetTargetGroupDictionaryFromMoveResolutionInfo(resolutionInfo);

            /*  Get a full list of all actions so we can format the string properly.    */
            System.Collections.Generic.List<AttackAction> actions = GetAttackActionsOfExecutedMove(resolutionInfo);

            /*  If something went wrong, complete the string.   */
            if (actions.Count <= 0) { return $"Do nothing."; }


            string intentionText = string.Empty;
            for (int i = 0; i < actions.Count; i++)
            {
                if (i != 0)
                {
                    intentionText += (i == actions.Count - 1) ? " then, " : ", ";
                }

                MoveTarget moveTarget = targetGroupToMoveTargetDictionary[actions[i].TargetGroupID];
                intentionText += $"{actions[i].GetDescription()} to {GetTargettingStringFromMoveTarget(moveTarget)}";
            }
            return intentionText;
        }


        /// <returns>A populated dictionary of Target Group to Move Target if valid. Returns null if Targets are not declared. </returns>
        public static System.Collections.Generic.Dictionary<int, MoveTarget> GetMoveTargetTargetGroupDictionaryFromMoveResolutionInfo(AttackResolutionInfo attackResolutionInfo)
        {
            if (attackResolutionInfo == null || attackResolutionInfo.TargetDeclarationGroups.Count == 0) { return null; }
            System.Collections.Generic.Dictionary<int, MoveTarget> targetGroupToMoveTargetDictionary = new();

            foreach (TargetDeclarationGroup group in attackResolutionInfo.TargetDeclarationGroups)
            {
                targetGroupToMoveTargetDictionary.Add(group.TargetGroupID, group.GroupMoveTargetType);
            }
            return targetGroupToMoveTargetDictionary;
        }

        private static System.Collections.Generic.List<AttackAction> GetAttackActionsOfExecutedMove(AttackResolutionInfo resolutionInfo)
        {
            System.Collections.Generic.List<AttackAction> actions = new();
            foreach (AttackStep step in resolutionInfo.Steps)
            {
                foreach (AttackAction action in step.Actions)
                {
                    actions.Add(action);
                }
            }
            return actions;
        }

        public static string GetElementalMoveIntentionString(UnitTeam team, IElementalMove elementalMove)
        {
            /*  Siliently Execute the selected move to retrieve the AttackAction descriptions.  */
            if (!StationManagerUtilities.TryCreateUnitDataSceneDataForElementalMove(team, out TurnBased.AttackResolution.ResolutionSceneData resolutionSceneData)) { return $"{elementalMove.GetMoveName()} is going to do nothing."; }

            AttackResolutionInfo resolutionInfo = elementalMove.ExecuteElementalMove(resolutionSceneData);

            /*  Get the target groups of the move.  */
            System.Collections.Generic.Dictionary<int, MoveTarget> targetGroupToMoveTargetDictionary = GetMoveTargetTargetGroupDictionaryFromMoveResolutionInfo(resolutionInfo);

            /*  Get a full list of all actions so we can format the string properly.    */
            System.Collections.Generic.List<AttackAction> actions = GetAttackActionsOfExecutedMove(resolutionInfo);

            /*  If something went wrong, complete the string.   */
            if (actions.Count <= 0) { return $"Elemental Move is going to do nothing."; }

            string intentionText = $"";
            for (int i = 0; i < actions.Count; i++)
            {
                if (i != 0)
                {
                    intentionText += (i == actions.Count - 1) ? " then, " : ", ";
                }

                MoveTarget moveTarget = targetGroupToMoveTargetDictionary[actions[i].TargetGroupID];
                intentionText += $"{actions[i].GetDescription()} to {GetTargettingStringFromMoveTarget(moveTarget)}";
            }

            return intentionText;
        }

        private static string GetTargettingStringFromMoveTarget(MoveTarget moveTarget)
        {
            switch (moveTarget)
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
            UnityEngine.Debug.LogError($"Created unit data for unit index");

            if (!Intention.UnitIntentionManager.Instance.TryGetIntention(unitIndex, out Intention.UnitIntention unitIntention)) { return $"{unitData.name} is intending to do nothing."; }

            System.Collections.Generic.Dictionary<int, MoveTarget> groupIDMoveTargetDict = CreateMoveTargetGroupIdentifierDictionary(unitIntention);

            string intentionText = $"{unitData.name} is going to ";
            for (int i = 0; i < unitIntention.ResolvingState.ActionResolvingStates.Count; i++)
            {
                if (i != 0)
                {
                    intentionText += (i == unitIntention.ResolvingState.ActionResolvingStates.Count - 1) ? " then, " : ", ";
                }

                /*  Get Target Group Text   */
                int groupID = unitIntention.ResolvingState.ActionResolvingStates[i].Action.TargetGroupID;
                if (!TryGetTargetTextOfGroupID(intention, groupIDMoveTargetDict, groupID, out string targetText)) { targetText = "a target"; }

                intentionText += $"{unitIntention.ResolvingState.ActionResolvingStates[i].Action.GetDescription()} to {targetText}";
            }

            return intentionText;
        }


        public static bool TryGetMoveTargetOfCurrentTargetGroup(Intention.UnitIntention unitIntention, out MoveTarget moveTarget)
        {
            moveTarget = MoveTarget.SingleEnemy;
            System.Collections.Generic.Dictionary<int, MoveTarget> groupIDMoveTargetDict = CreateMoveTargetGroupIdentifierDictionary(unitIntention);

            if (unitIntention.CurrentProcessingTargetGroupIndex < 0 || unitIntention.CurrentProcessingTargetGroupIndex >= unitIntention.ResolvingState.TargetGroupResolvingStates.Count) {  return false; }

            moveTarget = unitIntention.ResolvingState.TargetGroupResolvingStates[unitIntention.CurrentProcessingTargetGroupIndex].MoveTarget;
            return true;
        }

        private static System.Collections.Generic.Dictionary<int, MoveTarget> CreateMoveTargetGroupIdentifierDictionary(Intention.UnitIntention unitIntention)
        {
            System.Collections.Generic.Dictionary<int, MoveTarget> GroupIDMoveTargetDict = new();

            foreach (Intention.TargetGroupResolvingState groupState in unitIntention.ResolvingState.TargetGroupResolvingStates)
            {
                GroupIDMoveTargetDict.Add(groupState.GroupID, groupState.MoveTarget);
            }

            return GroupIDMoveTargetDict;   
        }

        private static bool TryGetTargetTextOfGroupID(Intention.UnitIntention intention, System.Collections.Generic.Dictionary<int, MoveTarget> groupIDMoveTargetDict, int groupId, out string targetText)
        {
            targetText = default;

            if (!groupIDMoveTargetDict.TryGetValue(groupId, out MoveTarget target)) { return false; }

            if (!TryGetDeclaredTargetsOfGroupID(intention, groupId, out System.Collections.Generic.List<StationIndex> selectedTargets)) {  return false; }

            if (!UI.UserInterfaceUtility.TryGetIntentionTargetText(selectedTargets, target, out targetText)) { return false; }

            return true;
        }

        private static bool TryGetDeclaredTargetsOfGroupID(Intention.UnitIntention intention, int groupID, out System.Collections.Generic.List<StationIndex> selectedTargets)
        {
            selectedTargets = default;
            foreach (Intention.TargetGroupResolvingState state in intention.ResolvingState.TargetGroupResolvingStates)
            {
                if (state.GroupID == groupID)
                {
                    selectedTargets = state.DeclaredTargets;
                    return true;
                }
            }
            return false;
        }
    }
}