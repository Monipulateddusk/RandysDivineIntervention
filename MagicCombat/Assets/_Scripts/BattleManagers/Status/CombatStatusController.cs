namespace TurnBased.Status
{
    public static class CombatStatusController
    {
        public static void AddStatusEffect(AttackResolution.ApplyStatusRequest request, BaseStatus statusAdded)
        {
            //  -=-=-=-=-=-=-=-=-
            //  Resolve the OnStatusAdded method to retieve all of the resolution information
            //  -=-=-=-=-=-=-=-=-
            if (!StationManagerUtilities.TryCreateUnitDataSceneDataForUnitIndex(request.TargetUnit, out TurnBased.AttackResolution.ResolutionSceneData resolutionSceneData)) { return; }
            if (!StationManagerUtilities.TryCreateCombatSceneDataForUnitIndex(request.TargetUnit, out UnitTurnStationIndexesSceneData stationIndexesSceneData)) { return; }

            AttackResolutionInfo onStatusAddedInfo = statusAdded.OnStatusAdded(resolutionSceneData);

            //  -=-=-=-=-=-=-=-=-
            //  Process the Target Selection of the Adding Status Events
            //  -=-=-=-=-=-=-=-=-
            Intention.ResolvingSource resolvingSource = new(statusAdded, request.TargetUnit);
            Intention.ResolvingState statusResolvingState = new(resolvingSource);

            if (!Intention.UnitIntentionFactory.BuildAttackActionResolvingState(request.TargetUnit, onStatusAddedInfo, statusResolvingState)) { return; }
            if (!GeneralPurposeTargettingManager.Instance.TryGetTargetSelector(statusAdded.StatusName, out TargetSelection.ITargetSelector targetSelector)) {  return; }    

            Intention.IntentionResolverManager.Instance.ProcessResolvingStateTargetSelection(stationIndexesSceneData, statusResolvingState, targetSelector);

            //  -=-=-=-=-=-=-=-=-
            //  Add Resolving state as a request to the resolver
            //  -=-=-=-=-=-=-=-=-
            AttackResolution.CombatResolvingRequest resolvingRequest = Combat.IntentionCombatResolverUtility.AddToCombatResolverFront(statusResolvingState);
        }
        public static void RemoveStatusEffect(AttackResolution.RemoveStatusRequest request, BaseStatus statusRemoved)
        {
            //  -=-=-=-=-=-=-=-=-
            //  Resolve the OnStatusAdded method to retieve all of the resolution information
            //  -=-=-=-=-=-=-=-=-
            if (!StationManagerUtilities.TryCreateUnitDataSceneDataForUnitIndex(request.TargetUnit, out TurnBased.AttackResolution.ResolutionSceneData resolutionSceneData)) { return; }
            if (!StationManagerUtilities.TryCreateCombatSceneDataForUnitIndex(request.TargetUnit, out UnitTurnStationIndexesSceneData stationIndexesSceneData)) { return; }

            AttackResolutionInfo onStatusAddedInfo = statusRemoved.OnStatusRemoved(resolutionSceneData);

            //  -=-=-=-=-=-=-=-=-
            //  Process the Target Selection of the Adding Status Events
            //  -=-=-=-=-=-=-=-=-
            Intention.ResolvingSource resolvingSource = new(statusRemoved, request.TargetUnit);
            Intention.ResolvingState statusResolvingState = new(resolvingSource);

            if (!Intention.UnitIntentionFactory.BuildAttackActionResolvingState(request.TargetUnit, onStatusAddedInfo, statusResolvingState)) { return; }
            if (!GeneralPurposeTargettingManager.Instance.TryGetTargetSelector(statusRemoved.StatusName, out TargetSelection.ITargetSelector targetSelector)) { return; }

            Intention.IntentionResolverManager.Instance.ProcessResolvingStateTargetSelection(stationIndexesSceneData, statusResolvingState, targetSelector);

            //  -=-=-=-=-=-=-=-=-
            //  Add Resolving state as a request to the resolver
            //  -=-=-=-=-=-=-=-=-
            AttackResolution.CombatResolvingRequest resolvingRequest = Combat.IntentionCombatResolverUtility.AddToCombatResolverFront(statusResolvingState);
        }
    }
}