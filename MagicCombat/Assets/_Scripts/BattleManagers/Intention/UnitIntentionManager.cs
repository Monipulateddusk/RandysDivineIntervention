using UnityEngine;

namespace TurnBased.Intention
{
    public class ResolvingSource
    {
        public DamageOriginType Type { get; }
        public UnitIndex? SourceUnitIndex { get; }
        public IElementalMove SourceElementalMove { get; }
        public BaseStatus SourceStatus { get; }



        public ResolvingSource(UnitIndex sourceUnit)
        {
            this.SourceUnitIndex = sourceUnit;
            this.Type = DamageOriginType.Unit;
            this.SourceStatus = null;
        }
        public ResolvingSource(IElementalMove elementalMove)
        {
            this.SourceElementalMove = elementalMove;
            this.Type = DamageOriginType.Environment;
            this.SourceStatus = null;
            this.SourceUnitIndex = null;
        }
        public ResolvingSource(BaseStatus sourceStatus)
        {
            this.SourceStatus = sourceStatus;
            this.Type = DamageOriginType.Status;
            this.SourceUnitIndex = null;
        }
    }
    public class ResolvingState
    {
        public ResolvingSource ResolvingSource { get; }
        public System.Collections.Generic.List<AttackActionResolvingState> ActionResolvingStates { get; set; }

        public System.Collections.Generic.List<TargetGroupResolvingState> TargetGroupResolvingStates { get; set; }

        public ResolvingState(ResolvingSource resolvingSource)
        {
            this.ResolvingSource = resolvingSource;
            this.ActionResolvingStates = new();
            this.TargetGroupResolvingStates = new();     
        }
    }

    public class UnitIntention
    {
        public IBattleMove MoveSelection { get; set; }
        public UnitIntentionResolutionState IntentionResolutionState { get; set; }
        public ResolvingState ResolvingState { get; set; }

        public int CurrentProcessingTargetGroupIndex;

        public UnitIntention(UnitIndex unitIndex)
        {
            this.MoveSelection = null;
            this.IntentionResolutionState = UnitIntentionResolutionState.NONE;
            this.CurrentProcessingTargetGroupIndex = 0;

            ResolvingSource resolvingSource = new(unitIndex);
            this.ResolvingState = new(resolvingSource);     
        }
    }

    public class AttackActionResolvingState
    {
        public AttackResolution.AttackAction Action { get; }
        /// <summary>
        /// A referance to the source Unit Move. Can be Null if the Attack Action did not originate from a Unit Move.
        /// </summary>
        public IBattleMove SourceMove { get; }
        /// <summary>
        /// A referance to the source Elemental Move. Can be Null if the Attack Action did not originate from an Elemental Move.
        /// </summary>
        public IElementalMove SourceElementalMove { get; }
        public MoveResolutionTiming Timing { get; }

        public bool IsResolved;

        public AttackActionResolvingState(AttackResolution.AttackAction attackAction, IBattleMove unitMove, MoveResolutionTiming timing)
        {
            this.Action = attackAction;
            this.SourceMove = unitMove;
            this.Timing = timing;

            this.SourceElementalMove = null;
            this.IsResolved = false;
        }
        public AttackActionResolvingState(AttackResolution.AttackAction attackAction, IElementalMove elementalMove, MoveResolutionTiming timing)
        {
            this.Action = attackAction;
            this.SourceElementalMove = elementalMove;
            this.Timing = timing;

            this.SourceMove = null;
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

            intentionDictionary.Add(unitIndex.Index, new(unitIndex));
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

            SetIntention(unitIndex, UnitIntentionFactory.CreateIntentionAwaitingMove(unitIndex));
        }

        public void SetMoveIntention(UnitIndex unitIndex, IBattleMove battleMove)
        {
            if (!intentionDictionary.ContainsKey(unitIndex.Index)) { return; }

            SetIntention(unitIndex, UnitIntentionFactory.CreateIntentionFromMove(unitIndex, battleMove));
        }

        public void ClearIntention(UnitIndex unitIndex)
        {
            if (!intentionDictionary.ContainsKey(unitIndex.Index)) { return; }

            SetIntention(unitIndex, new UnitIntention(unitIndex));
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
        public static UnitIntention CreateIntentionAwaitingMove(UnitIndex unitIndex)
        {
            return new UnitIntention(unitIndex)
            {
                IntentionResolutionState = UnitIntentionResolutionState.AWAITING_MOVE_SELECTION
            };
        }

        public static UnitIntention CreateIntentionFromMove(UnitIndex unitIndex, IBattleMove move)
        {
            UnitIntention intention = new(unitIndex)
            {
                MoveSelection = move,
                IntentionResolutionState = UnitIntentionResolutionState.AWAITING_TARGET_SELECTION
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

            CreateTargetGroupResolvingStatesForIntention(unitIntention.ResolvingState, info, unitData.targetSelectorType);

            CreateActionResolvingStatesForIntention(unitIntention.ResolvingState, info, move);
            return true;
        }

        public static bool BuildAttackActionResolvingStatesFromElementalMove(UnitDataUnitTurnSceneData sceneData, ResolvingState resolvingState, IElementalMove elementalMove)
        {
            if (elementalMove == null) { return false; }

            UnityEngine.Debug.LogError($"Move is not null");

            /*  Process the elemental move and determine the targeting groups and action resolving states for it.   */
            AttackResolutionInfo info = elementalMove.ExecuteElementalMove(sceneData.AllyUnitData, sceneData.EnemyUnitData);

            UnityEngine.Debug.LogError($"Executed elemental move");

            CreateTargetGroupResolvingStatesForIntention(resolvingState, info, elementalMove.GetTargetSelectorType());

            UnityEngine.Debug.LogError($"Declared target group resolving state for elem move");

            CreateActionResolvingStatesForIntention(resolvingState, info, elementalMove);

            UnityEngine.Debug.LogError($"created action resolving state");



            return true;
        }


        private static void CreateTargetGroupResolvingStatesForIntention(ResolvingState resolvingState, AttackResolutionInfo resolutionInfo, UnitTargetSelectorType selectorType)
        {
            UnityEngine.Debug.LogError($"Clearing target group resolving states for resolving state.    ");

            resolvingState.TargetGroupResolvingStates.Clear();
            foreach (TargetDeclarationGroup declarationGroup in resolutionInfo.TargetDeclarationGroups)
            {
                UnityEngine.Debug.LogError($"Foreach iteration.    ");

                TargetGroupResolvingState targetGroupResolvingState = new(declarationGroup.TargetGroupID, declarationGroup.GroupMoveTargetType, selectorType);

                UnityEngine.Debug.LogError($"Adding {targetGroupResolvingState} to resolving states.    ");

                resolvingState.TargetGroupResolvingStates.Add(targetGroupResolvingState);
            }

            UnityEngine.Debug.LogError($"Done Creating target group resolving states    ");
        }

        private static void CreateActionResolvingStatesForIntention(ResolvingState resolvingState, AttackResolutionInfo resolutionInfo, IBattleMove unitMove)
        {
            for (int stepIndex = 0; stepIndex < resolutionInfo.Steps.Count; stepIndex++)
            {
                AttackStep step = resolutionInfo.Steps[stepIndex];

                for (int actionIndex = 0; actionIndex < step.Actions.Count; actionIndex++)
                {
                    AttackResolution.AttackAction action = step.Actions[actionIndex];

                    AttackActionResolvingState attackActionResolvingState = new(action, unitMove, unitMove.GetResolutionTiming());
                    resolvingState.ActionResolvingStates.Add(attackActionResolvingState);
                }
            }
        }

        private static void CreateActionResolvingStatesForIntention(ResolvingState resolvingState, AttackResolutionInfo resolutionInfo, IElementalMove elementalMove)
        {
            for (int stepIndex = 0; stepIndex < resolutionInfo.Steps.Count; stepIndex++)
            {
                AttackStep step = resolutionInfo.Steps[stepIndex];

                for (int actionIndex = 0; actionIndex < step.Actions.Count; actionIndex++)
                {
                    AttackResolution.AttackAction action = step.Actions[actionIndex];

                    AttackActionResolvingState attackActionResolvingState = new(action, elementalMove, elementalMove.GetResolutionTiming());
                    resolvingState.ActionResolvingStates.Add(attackActionResolvingState);
                }
            }
        }


        /// <returns>True if all Target Groups are Resolved.</returns>
        public static bool AssignTargetsToCurrentProcessingTargetGroup(UnitIntention intention, System.Collections.Generic.List<StationIndex> targets)
        {
            if (intention.CurrentProcessingTargetGroupIndex < 0 || intention.CurrentProcessingTargetGroupIndex >= intention.ResolvingState.TargetGroupResolvingStates.Count) { return false; }

            intention.ResolvingState.TargetGroupResolvingStates[intention.CurrentProcessingTargetGroupIndex].AssignTargets(targets);

            /*  Determine if all resolving states are resolved. */
            foreach (TargetGroupResolvingState targetGroupResolvingState in intention.ResolvingState.TargetGroupResolvingStates)
            {
                if (!targetGroupResolvingState.IsResolved) { return false; }
            }

            intention.IntentionResolutionState = UnitIntentionResolutionState.COMPLETED_INTENTION;
            return true;
        }

        public static bool AssignTargetsToCurrentProcessingTargetGroup(ResolvingState resolvingState, int currentProcessingTargetGroupIndex, System.Collections.Generic.List<StationIndex> targets)
        {
            if (currentProcessingTargetGroupIndex < 0 ||  currentProcessingTargetGroupIndex >= resolvingState.TargetGroupResolvingStates.Count) { return false; }

            resolvingState.TargetGroupResolvingStates[currentProcessingTargetGroupIndex].AssignTargets(targets);

            /*  Determine if all resolving states are resolved. */
            foreach (TargetGroupResolvingState targetGroupResolvingState in resolvingState.TargetGroupResolvingStates)
            {
                if (!targetGroupResolvingState.IsResolved) { return false; }
            }

            return true;
        }

        public static bool TryGetMoveTargetOfCurrentTargetGroup(UnitIntention intention, out MoveTarget moveTarget)
        {
            moveTarget = default;
            if (intention.CurrentProcessingTargetGroupIndex < 0 || intention.CurrentProcessingTargetGroupIndex >= intention.ResolvingState.TargetGroupResolvingStates.Count) { return false; }

            moveTarget = intention.ResolvingState.TargetGroupResolvingStates[intention.CurrentProcessingTargetGroupIndex].MoveTarget;
            return true;
        }

        public static bool TryGetMoveTargetOfCurrentTargetGroup(ResolvingState resolvingState, int currentProcessingTargetGroupIndex, out MoveTarget moveTarget)
        {
            moveTarget = default;
            if (currentProcessingTargetGroupIndex < 0 || currentProcessingTargetGroupIndex >= resolvingState.TargetGroupResolvingStates.Count) { return false; }

            moveTarget = resolvingState.TargetGroupResolvingStates[currentProcessingTargetGroupIndex].MoveTarget;
            return true;
        }

        public static bool TryGetDeclaredTargetsForTargetGroup(Intention.ResolvingState resolvingState, int targetGroupIndex, out System.Collections.Generic.List<StationIndex> declaredTargets)
        {
            declaredTargets = default;
            foreach (TargetGroupResolvingState targetGroup in resolvingState.TargetGroupResolvingStates)
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