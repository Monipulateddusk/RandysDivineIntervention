using UnityEngine;

namespace TurnBased.Intention
{
    public class UnitIntention
    {
        public IBattleMove MoveSelection { get; set; }
        public UnitIntentionResolutionState ResolutionState { get; set; }
        public System.Collections.Generic.List<AttackActionResolvingState> ActionResolvingStates { get; }

        public System.Collections.Generic.List<TargetGroupResolvingState> TargetGroupResolvingStates { get; }

        public int CurrentProcessingTargetGroupIndex;

        public UnitIntention()
        {
            this.MoveSelection = null;
            this.ResolutionState = UnitIntentionResolutionState.NONE;
            this.ActionResolvingStates = new();
            this.TargetGroupResolvingStates = new();
            this.CurrentProcessingTargetGroupIndex = 0;
        }
    }

    public class AttackActionResolvingState
    {
        public AttackAction Action { get; }
        public IBattleMove SourceMove { get; }
        public MoveResolutionTiming Timing { get; }

        public bool IsResolved;

        public AttackActionResolvingState(AttackAction attackAction, IBattleMove move, MoveResolutionTiming timing)
        {
            this.Action = attackAction;
            this.SourceMove = move;
            this.Timing = timing;

            this.IsResolved = false;
        }

    }

    public class TargetGroupResolvingState
    {
        public int GroupID { get; }
        public MoveTarget MoveTarget { get; }
        public UnitTargetSelectorType SelectorType { get; }
        public System.Collections.Generic.List<StationIndex> DeclaredTargets { get; private set; }
        public bool IsResolved { get; private set; }
        public bool AwaitingTargetInput;
        public TargetGroupResolvingState(int groupID, MoveTarget moveTarget, UnitTargetSelectorType selectorType)
        {
            this.GroupID = groupID;
            this.MoveTarget = moveTarget;
            this.SelectorType = selectorType;
            this.DeclaredTargets = new();
            this.IsResolved = false;
            this.AwaitingTargetInput = false;
        }

        public void AssignTargets(System.Collections.Generic.List<StationIndex> targets)
        {
            this.DeclaredTargets = targets;
            this.IsResolved = true;
        }
    }


    public class UnitIntentionManager
    {
        private static UnitIntentionManager instance;
        public static UnitIntentionManager Instance
        {
            get
            {
                try
                {
                    return instance;
                }
                catch (System.Exception e)
                {
                    Debug.LogError(e.ToString());
                    return null;
                }
            }
        }

        private System.Collections.Generic.Dictionary<int, UnitIntention> intentionDictionary = new();

        public static event System.Action<UnitIndex> OnUnitIntentionAdded;
        /// <summary>
        /// Invoked when a unit's intention changes due to move selection, target selection, switching out
        /// </summary>
        public static event System.Action<UnitIndex, UnitIntention> OnUnitIntentionChanged;
        public static event System.Action<UnitIndex> OnUnitIntentionRemoved;


        public void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }

            this.intentionDictionary = new();

            StationManager.OnAddUnit += AddUnitIndexToDictionary;
            StationManager.OnRemoveUnit += RemoveUnitIndexFromDictionary;
        }



        public void OnDestroy()
        {
            if (instance != null && instance == this)
            {
                instance = null;
            }

            this.intentionDictionary.Clear();

            StationManager.OnAddUnit -= AddUnitIndexToDictionary;
            StationManager.OnRemoveUnit -= RemoveUnitIndexFromDictionary;

            OnUnitIntentionAdded = null;
            OnUnitIntentionChanged = null;
            OnUnitIntentionRemoved = null;
        }

        public void AddUnitIndexToDictionary(UnitIndex unitIndex)
        {
            if (this.intentionDictionary.ContainsKey(unitIndex.Index)) { return; }

            intentionDictionary.Add(unitIndex.Index, new());
            OnUnitIntentionAdded?.Invoke(unitIndex);
        }

        private void RemoveUnitIndexFromDictionary(UnitIndex unitIndex, StationIndex? arg2, BaseBattleUnit arg3)
        {
            UnityEngine.Debug.LogError("Starting to remove intention from UnitIntentionManager");

            RemoveIntention(unitIndex);

            UnityEngine.Debug.LogError("Removed unit index from UnitIntentionManager");
        }

        public void RemoveIntention(UnitIndex unitIndex)
        {
            if (!this.intentionDictionary.ContainsKey(unitIndex.Index)) { return; }

            intentionDictionary.Remove(unitIndex.Index);
            OnUnitIntentionRemoved?.Invoke(unitIndex);
        }

        public void SetIntention(UnitIndex unitIndex, UnitIntention intention)
        {
            if (!intentionDictionary.ContainsKey(unitIndex.Index)) { return; }

            intentionDictionary[unitIndex.Index] = intention;
           
            OnUnitIntentionChanged?.Invoke(unitIndex, intention);
        }

        public void SetReadyForMoveIntention(UnitIndex unitIndex)
        {
            if (!intentionDictionary.ContainsKey(unitIndex.Index)) { return; }

            SetIntention(unitIndex, UnitIntentionFactory.CreateIntentionAwaitingMove());
        }

        public void SetMoveIntention(UnitIndex unitIndex, IBattleMove battleMove)
        {
            if (!intentionDictionary.ContainsKey(unitIndex.Index)) { return; }

            SetIntention(unitIndex, UnitIntentionFactory.CreateIntentionFromMove(unitIndex, battleMove));
        }

        public void ClearIntention(UnitIndex unitIndex)
        {
            if (!intentionDictionary.ContainsKey(unitIndex.Index)) { return; }

            SetIntention(unitIndex, new UnitIntention());
        }

        public bool TryGetIntention(UnitIndex unitIndex, out UnitIntention intention)
        {
            intention = default;
            if (!intentionDictionary.ContainsKey(unitIndex.Index)) { return false; }

            intention = intentionDictionary[unitIndex.Index];
            return true;
        }
    }


    public static class UnitIntentionFactory
    {
        public static UnitIntention CreateIntentionAwaitingMove()
        {
            return new UnitIntention()
            {
                ResolutionState = UnitIntentionResolutionState.AWAITING_MOVE_SELECTION
            };
        }

        public static UnitIntention CreateIntentionFromMove(UnitIndex unitIndex, IBattleMove move)
        {
            UnitIntention intention = new()
            {
                MoveSelection = move,
                ResolutionState = UnitIntentionResolutionState.AWAITING_TARGET_SELECTION
            };

            if (!BuildAttackActionResolvingStatesFromMove(unitIndex, intention, move)) { return intention; }

            return intention;
        }


        public static bool BuildAttackActionResolvingStatesFromMove(UnitIndex unitIndex, UnitIntention unitIntention, IBattleMove move)
        {
            if (move == null) { return false; }

            if (!StationManagerUtilities.TryCreateUnitDataSceneDataForUnitIndex(unitIndex, out UnitDataUnitTurnSceneData SceneData)) { return false; }
            if (!StationManager.Instance.TryGetUnitDataOfUnitIndex(unitIndex, out UnitData unitData)) {  return false; }  

            AttackResolutionInfo info = move.ExecuteMove(SceneData.SourceUnitData, SceneData.AllyUnitData, SceneData.EnemyUnitData);

            CreateTargetGroupResolvingStatesForIntention(unitIntention, info, unitData);

            CreateActionResolvingStatesForIntention(unitIntention, info, move);
            return true;
        }


        private static void CreateTargetGroupResolvingStatesForIntention(UnitIntention intention, AttackResolutionInfo resolutionInfo, UnitData unitData)
        {
            intention.TargetGroupResolvingStates.Clear();
            foreach (TargetDeclarationGroup declarationGroup in resolutionInfo.TargetDeclarationGroups)
            {
                TargetGroupResolvingState targetGroupResolvingState = new(declarationGroup.TargetGroupID, declarationGroup.GroupMoveTargetType, unitData.targetSelectorType);
                intention.TargetGroupResolvingStates.Add(targetGroupResolvingState);
            }
        }

        private static void CreateActionResolvingStatesForIntention(UnitIntention intention, AttackResolutionInfo resolutionInfo, IBattleMove move)
        {
            for (int stepIndex = 0; stepIndex < resolutionInfo.Steps.Count; stepIndex++)
            {
                AttackStep step = resolutionInfo.Steps[stepIndex];

                for (int actionIndex = 0; actionIndex < step.Actions.Count; actionIndex++)
                {
                    AttackAction action = step.Actions[actionIndex];

                    AttackActionResolvingState resolvingState = new(action, move, move.GetResolutionTiming());
                    intention.ActionResolvingStates.Add(resolvingState);
                }
            }
        }


        /// <returns>True if all Target Groups are Resolved.</returns>
        public static bool TryAssignTargetsToCurrentProcessingTargetGroup(UnitIntention intention, System.Collections.Generic.List<StationIndex> targets)
        {
            intention.TargetGroupResolvingStates[intention.CurrentProcessingTargetGroupIndex].AssignTargets(targets);

            /*  Determine if all resolving states are resolved. */
            foreach (TargetGroupResolvingState targetGroupResolvingState in intention.TargetGroupResolvingStates)
            {
                if (!targetGroupResolvingState.IsResolved) { return false; }
            }

            intention.ResolutionState = UnitIntentionResolutionState.COMPLETED_INTENTION;
            return true;
        }

        public static bool TryGetMoveTargetOfCurrentTargetGroup(UnitIntention intention, out MoveTarget moveTarget)
        {
            moveTarget = default;
            if (intention.CurrentProcessingTargetGroupIndex < 0 || intention.CurrentProcessingTargetGroupIndex >= intention.TargetGroupResolvingStates.Count) { return false; }

            moveTarget = intention.TargetGroupResolvingStates[intention.CurrentProcessingTargetGroupIndex].MoveTarget;
            return true;
        }
        public static bool TryGetDeclaredTargetsForTargetGroup(UnitIntention intention, int targetGroupIndex, out System.Collections.Generic.List<StationIndex> declaredTargets)
        {
            declaredTargets = default;
            foreach (TargetGroupResolvingState targetGroup in intention.TargetGroupResolvingStates)
            {
                if (targetGroup.GroupID == targetGroupIndex)
                {
                    declaredTargets = targetGroup.DeclaredTargets;
                    return true;
                }
            }
            return false;
        }
    }
}