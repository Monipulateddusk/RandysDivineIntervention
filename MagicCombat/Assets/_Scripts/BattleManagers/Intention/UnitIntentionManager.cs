using UnityEngine;

namespace TurnBased.Intention
{
    public class ResolvingSource
    {
        public DamageOriginType Type { get; }
        public UnitIndex SourceUnitIndex { get; }
        public IBattleMove SourceUnitMove { get; }  
        public IElementalMove SourceElementalMove { get; }
        public Status.BaseStatus SourceStatus { get; }



        public ResolvingSource(IBattleMove unitMove, UnitIndex sourceUnit)
        {
            this.SourceUnitIndex = sourceUnit;
            this.SourceUnitMove = unitMove;
            this.Type = DamageOriginType.UnitMove;
            this.SourceElementalMove = null;
            this.SourceStatus = null;
        }
        public ResolvingSource(IElementalMove elementalMove, UnitIndex sourceUnit)
        {
            this.SourceElementalMove = elementalMove;
            this.SourceUnitIndex = sourceUnit;
            this.Type = DamageOriginType.Environment;
            this.SourceStatus = null;
            this.SourceUnitMove = null;
        }
        public ResolvingSource(Status.BaseStatus sourceStatus, UnitIndex sourceUnit)
        {
            this.SourceStatus = sourceStatus;
            this.SourceUnitIndex = sourceUnit;
            this.Type = DamageOriginType.Status;
            this.SourceElementalMove = null;
            this.SourceUnitMove = null;
        }
    }

    public class AttackActionResolvingState
    {
        public AttackResolution.AttackAction Action { get; }

        public ResolvingSource ResolvingSource { get; set; }

        public MoveResolutionTiming Timing { get; }

        public bool IsResolved;

        public AttackActionResolvingState(AttackResolution.AttackAction attackAction, ResolvingSource resolvingSource, MoveResolutionTiming timing)
        {
            this.Action = attackAction;
            this.ResolvingSource = resolvingSource;
            this.Timing = timing;

            this.IsResolved = false;
        }
    }

    public class TargetGroupResolvingState
    {
        public int GroupID { get; }
        public MoveTarget MoveTarget { get; }
        public System.Collections.Generic.List<StationIndex> DeclaredTargets { get; private set; }
        public bool IsResolved { get; private set; }
        public bool AwaitingTargetInput;
        public TargetGroupResolvingState(int groupID, MoveTarget moveTarget)
        {
            this.GroupID = groupID;
            this.MoveTarget = moveTarget;
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

    public class ResolvingState
    {
        public ResolvingSource ResolvingSource { get; }
        public System.Collections.Generic.List<AttackActionResolvingState> ActionResolvingStates { get; set; }

        public System.Collections.Generic.List<TargetGroupResolvingState> TargetGroupResolvingStates { get; set; }
        public UnitIntentionResolutionState IntentionResolutionState { get; set; }
        public int CurrentProcessingTargetGroupIndex { get; set; }  

        public ResolvingState()
        {
            this.IntentionResolutionState = UnitIntentionResolutionState.AWAITING_MOVE_SELECTION;
            this.ResolvingSource = null;
            this.ActionResolvingStates = new();
            this.TargetGroupResolvingStates = new();
            this.CurrentProcessingTargetGroupIndex = 0;
        }
        public ResolvingState(ResolvingSource resolvingSource)
        {
            this.IntentionResolutionState = UnitIntentionResolutionState.AWAITING_TARGET_SELECTION;
            this.ResolvingSource = resolvingSource;
            this.ActionResolvingStates = new();
            this.TargetGroupResolvingStates = new();
            this.CurrentProcessingTargetGroupIndex = 0;
        }
    }

    public class UnitIntention
    {
        public System.Collections.Generic.List<ResolvingState> ResolvingStates { get; private set; }
        public int CurrentResolvingStateIndex { get; private set; }
        public int MaximumResolvingStates { get; private set; } 


        public UnitIntention()
        {
            this.ResolvingStates = new() { new()  };
            this.MaximumResolvingStates = 2;
            this.CurrentResolvingStateIndex = 0;
        }

        public void CreateNewResolvingStateInList()
        {
            if (this.ResolvingStates[this.CurrentResolvingStateIndex].IntentionResolutionState != UnitIntentionResolutionState.AWAITING_MOVE_SELECTION)
            {
                this.ResolvingStates.Add(new());
                this.CurrentResolvingStateIndex++;
            }
        }

        public void AssignMoveToCurrentResolvingState(IBattleMove move, UnitIndex unitIndex)
        {
            ResolvingSource resolvingSource = new(move, unitIndex);
            this.ResolvingStates[this.CurrentResolvingStateIndex] = new(resolvingSource);
        }

        public ResolvingState GetCurrentResolvingState()
        {
            return this.ResolvingStates[this.CurrentResolvingStateIndex];
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

            this.intentionDictionary.Add(unitIndex.Index, new());
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

            this.intentionDictionary.Remove(unitIndex.Index);
            OnUnitIntentionRemoved?.Invoke(unitIndex);
        }

        public void SetIntention(UnitIndex unitIndex, UnitIntention intention)
        {
            if (!this.intentionDictionary.ContainsKey(unitIndex.Index)) { return; }

            this.intentionDictionary[unitIndex.Index] = intention;
           
            OnUnitIntentionChanged?.Invoke(unitIndex, intention);
        }

        public void SetReadyForMoveIntention(UnitIndex unitIndex)
        {
            if (!this.intentionDictionary.ContainsKey(unitIndex.Index)) { return; }

            this.intentionDictionary[unitIndex.Index].CreateNewResolvingStateInList();
        }

        public void SetMoveIntention(UnitIndex unitIndex, IBattleMove battleMove)
        {
            if (!this.intentionDictionary.ContainsKey(unitIndex.Index)) { return; }

            SetIntention(unitIndex, UnitIntentionFactory.CreateIntentionFromMoveForUnit(unitIndex, battleMove));
        }

        public void ClearIntention(UnitIndex unitIndex)
        {
            if (!intentionDictionary.ContainsKey(unitIndex.Index)) { return; }

            SetIntention(unitIndex, new UnitIntention());
        }

        public bool TryGetIntention(UnitIndex unitIndex, out UnitIntention intention)
        {
            intention = default;
            if (!this.intentionDictionary.ContainsKey(unitIndex.Index)) { return false; }

            intention = this.intentionDictionary[unitIndex.Index];
            return true;
        }
    }


    public static class UnitIntentionFactory
    {
        public static UnitIntention CreateIntentionFromMoveForUnit(UnitIndex unitIndex, IBattleMove move)
        {
            UnitIntention intention = new();
            intention.AssignMoveToCurrentResolvingState(move, unitIndex);

            if (!StationManagerUtilities.TryCreateUnitDataSceneDataForUnitIndex(unitIndex, out TurnBased.AttackResolution.ResolutionSceneData resolutionSceneData)) { UnityEngine.Debug.LogError("ERROR — UnitIntentionFactory: UNABLE TO SUCESSFULLY CREATE INTENTION"); return null; }
            AttackResolutionInfo moveResolutionInfo = move.ExecuteMove(resolutionSceneData);

            if (!BuildAttackActionResolvingState(unitIndex, moveResolutionInfo, intention.GetCurrentResolvingState(), move.GetResolutionTiming())) { UnityEngine.Debug.LogError("ERROR — UnitIntentionFactory: UNABLE TO SUCESSFULLY CREATE INTENTION"); return intention; }

            return intention;
        }

        public static bool BuildAttackActionResolvingState(UnitIndex unitIndex, AttackResolutionInfo info, ResolvingState resolvingState, MoveResolutionTiming timing = MoveResolutionTiming.Instant)
        {
            if (resolvingState == null) { return false; }


            if (!StationManager.Instance.TryGetUnitDataOfUnitIndex(unitIndex, out UnitData unitData)) { return false; }

            CreateTargetGroupResolvingStates(resolvingState, info);

            CreateActionResolvingStates(resolvingState, info, resolvingState.ResolvingSource, timing);
            return true;
        }


        private static void CreateTargetGroupResolvingStates(ResolvingState resolvingState, AttackResolutionInfo resolutionInfo)
        {
            UnityEngine.Debug.LogError($"Clearing target group resolving states for resolving state.    ");

            resolvingState.TargetGroupResolvingStates.Clear();
            foreach (TargetDeclarationGroup declarationGroup in resolutionInfo.TargetDeclarationGroups)
            {
                UnityEngine.Debug.LogError($"Foreach iteration.    ");

                TargetGroupResolvingState targetGroupResolvingState = new(declarationGroup.TargetGroupID, declarationGroup.GroupMoveTargetType);

                UnityEngine.Debug.LogError($"Adding {targetGroupResolvingState} to resolving states.    ");

                resolvingState.TargetGroupResolvingStates.Add(targetGroupResolvingState);
            }

            UnityEngine.Debug.LogError($"Done Creating target group resolving states    ");
        }

        private static void CreateActionResolvingStates(ResolvingState resolvingState, AttackResolutionInfo resolutionInfo, ResolvingSource resolvingSource, MoveResolutionTiming timing)
        {
            for (int stepIndex = 0; stepIndex < resolutionInfo.Steps.Count; stepIndex++)
            {
                AttackStep step = resolutionInfo.Steps[stepIndex];

                for (int actionIndex = 0; actionIndex < step.Actions.Count; actionIndex++)
                {
                    AttackResolution.AttackAction action = step.Actions[actionIndex];

                    AttackActionResolvingState attackActionResolvingState = new(action, resolvingSource, timing);
                    resolvingState.ActionResolvingStates.Add(attackActionResolvingState);
                }
            }
        }


        /// <returns>True if all Target Groups are Resolved.</returns>
        public static bool AssignTargetsToCurrentProcessingTargetGroup(ResolvingState resolvingState, UnitTurnStationIndexesSceneData sceneData, MoveTarget moveTargetType, TargetSelection.ITargetSelector targetSelector)
        {
            if (resolvingState.CurrentProcessingTargetGroupIndex < 0 || resolvingState.CurrentProcessingTargetGroupIndex >= resolvingState.TargetGroupResolvingStates.Count) { return false; }
            if (targetSelector == null) { return false; }

            System.Collections.Generic.List<StationIndex> targets = targetSelector.SelectTargets(sceneData, moveTargetType);
            resolvingState.TargetGroupResolvingStates[resolvingState.CurrentProcessingTargetGroupIndex].AssignTargets(targets);

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
            if (intention.GetCurrentResolvingState().CurrentProcessingTargetGroupIndex < 0 || intention.GetCurrentResolvingState().CurrentProcessingTargetGroupIndex >= intention.GetCurrentResolvingState().TargetGroupResolvingStates.Count) { return false; }

            moveTarget = intention.GetCurrentResolvingState().TargetGroupResolvingStates[intention.GetCurrentResolvingState().CurrentProcessingTargetGroupIndex].MoveTarget;
            return true;
        }

        public static bool TryGetMoveTargetOfCurrentTargetGroup(ResolvingState resolvingState, out MoveTarget moveTarget)
        {
            moveTarget = default;
            if (resolvingState.CurrentProcessingTargetGroupIndex < 0 || resolvingState.CurrentProcessingTargetGroupIndex >= resolvingState.TargetGroupResolvingStates.Count) { return false; }

            moveTarget = resolvingState.TargetGroupResolvingStates[resolvingState.CurrentProcessingTargetGroupIndex].MoveTarget;
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