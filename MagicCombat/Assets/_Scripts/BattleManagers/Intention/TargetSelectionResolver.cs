namespace TurnBased.Intention
{
    public class TargetSelectionResolver
    {

        public static System.Action<UnitIndex> OnRequireUserInput;
        public static System.Action<UnitIndex> OnCompleteUserInput;
        public static System.Action OnTargetSelected;

        private UnitIndex? currentResolvingUnit;

        public void OnDestroy()
        {
            this.currentResolvingUnit = null;

            OnRequireUserInput = null;
            OnCompleteUserInput = null;
            OnTargetSelected = null;
        }

        public bool ProcessTargetSelection(UnitIndex unitIndex)
        {
            this.currentResolvingUnit = unitIndex;
            if (!this.currentResolvingUnit.HasValue) { return false; }

            /*  We need to process each target group seperately one after the other for this Move.  */
            if (!Intention.UnitIntentionManager.Instance.TryGetIntention(this.currentResolvingUnit.Value, out UnitIntention intention)) { return false; }

            /*  Find the first unResolved Target Group. */
            ResolvingState unitResolvingState = intention.ResolvingState;
            for (int i = 0; i < unitResolvingState.TargetGroupResolvingStates.Count; i++)
            {
                if (unitResolvingState.TargetGroupResolvingStates[i].IsResolved) { continue; }
                intention.CurrentProcessingTargetGroupIndex = i;
            }

            /*  Determine if we need to invoke the Player's Input systems to resolve this. If so, halt processing until it is done! */
            if (!TargetSelection.UnitTargetSelectorManager.Instance.TryGetTargetSelector(unitIndex, out TargetSelection.ITargetSelector targetSelector)) { UnityEngine.Debug.Log("Target selector invalid?!"); return false; }

            return ProcessUnitTargetGroupSelection(unitResolvingState, targetSelector);
        }
        public bool ProcessEnvironmentTargetSelection(UnitTurnStationIndexesSceneData stationIndexesSceneData, ResolvingState elementalMoveResolvingState, TargetSelection.ITargetSelector targetSelector)
        {
            int targetGroupResolvingIndex = 0;
            /*  Find the first unResolved Target Group. */
            for (int i = 0; i < elementalMoveResolvingState.TargetGroupResolvingStates.Count; i++)
            {
                if (elementalMoveResolvingState.TargetGroupResolvingStates[i].IsResolved) { continue; }
                targetGroupResolvingIndex = i;
            }

            return ProcessNonUnitTargetSelector(stationIndexesSceneData, elementalMoveResolvingState, targetGroupResolvingIndex, targetSelector);
        }

        private bool ProcessUnitTargetGroupSelection(ResolvingState resolvingState, TargetSelection.ITargetSelector targetSelector)
        {
            /*  If this is player driven, then we need to select that Unit if it isn't already and await the player's move selection.   */
            if (targetSelector is TargetSelection.PlayerDrivenTargetSelector)
            {
                OnRequireUserInput?.Invoke(this.currentResolvingUnit.Value);
                return true;
            }
            else
            {
                return ProcessUnitTargetSelector(targetSelector);
            }
        }

        /// =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=
        /// 
        /// When A Player Selects a Target Via UI
        /// 
        /// =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=

        public void OnPlayerDrivenSelection(UnitIndex unitIndex, System.Collections.Generic.List<StationIndex> selectedTarget)
        {
            if (!TargetSelection.UnitTargetSelectorManager.Instance.TryGetTargetSelector(unitIndex, out TargetSelection.ITargetSelector targetSelector)) { return; }

            if (targetSelector is TargetSelection.PlayerDrivenTargetSelector)
            {
                (targetSelector as TargetSelection.PlayerDrivenTargetSelector).SelectedTarget = selectedTarget;
            }

            OnCompleteUserInput?.Invoke(unitIndex);

            ProcessUnitTargetSelector(targetSelector);
        }


        // =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=



        /// <summary>
        /// Process the current Resolving Unit's target selector
        /// </summary>
        /// <param name="targetSelector"></param>
        /// <returns></returns>
        private bool ProcessUnitTargetSelector(TargetSelection.ITargetSelector targetSelector)
        {
            if (targetSelector == null) { return false; }
            if (!this.currentResolvingUnit.HasValue) { return false; }

            /*  Create the scene data for this unit.    */
            if (!StationManagerUtilities.TryCreateCombatSceneDataForUnitIndex(this.currentResolvingUnit.Value, out UnitTurnStationIndexesSceneData sceneData)){ return false; }

            /*  Get the move data for the target selection. */
            if(!Intention.UnitIntentionManager.Instance.TryGetIntention(this.currentResolvingUnit.Value, out UnitIntention intention)) {  return false; }

            if (!UnitIntentionFactory.TryGetMoveTargetOfCurrentTargetGroup(intention, out MoveTarget moveTargetType)) { return false; }

            System.Collections.Generic.List<StationIndex> selectedTargets = targetSelector.SelectTargets(sceneData, moveTargetType);

            if (!UnitIntentionFactory.AssignTargetsToCurrentProcessingTargetGroup(intention, selectedTargets))
            {
                /*  If there is still groups to be selected, process the next target group. */
                ProcessTargetSelection(this.currentResolvingUnit.Value);
            }
            else
            {
                this.currentResolvingUnit = null;
                /*  Notify CombatRoundIntentionManager that all TargetGroups has been selected by this UnitIndex. */
                OnTargetSelected?.Invoke();
            }

            return true;
        }

        private bool ProcessNonUnitTargetSelector(UnitTurnStationIndexesSceneData stationIndexesSceneData, ResolvingState elementalMoveResolvingState, int currentTargetGroupResolvingIndex, TargetSelection.ITargetSelector targetSelector)
        {
            if (targetSelector == null) { return false; }

            if (!UnitIntentionFactory.TryGetMoveTargetOfCurrentTargetGroup(elementalMoveResolvingState, currentTargetGroupResolvingIndex, out MoveTarget moveTargetType)) { return false; }

            System.Collections.Generic.List<StationIndex> selectedTargets = targetSelector.SelectTargets(stationIndexesSceneData, moveTargetType);

            if (!UnitIntentionFactory.AssignTargetsToCurrentProcessingTargetGroup(elementalMoveResolvingState, currentTargetGroupResolvingIndex, selectedTargets))
            {
                /*  If there is still groups to be selected, process the next target group. */
                ProcessEnvironmentTargetSelection(stationIndexesSceneData, elementalMoveResolvingState, targetSelector);
            }
            else
            {
                /*  Notify CombatRoundIntentionManager that all TargetGroups has been selected by this UnitIndex. */
                OnTargetSelected?.Invoke();
            }

            return true;
        }

    }
}