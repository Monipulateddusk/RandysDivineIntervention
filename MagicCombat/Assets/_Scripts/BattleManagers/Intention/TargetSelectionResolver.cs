namespace TurnBased.Intention
{
    public class TargetSelectionResolver
    {

        public static System.Action<UnitIndex> OnRequireUserInput;
        public static System.Action<UnitIndex> OnCompleteUserInput;
        public static System.Action<UnitIndex, System.Collections.Generic.List<StationIndex>> OnTargetSelected;

        public void OnDestroy()
        {
            OnRequireUserInput = null;
            OnCompleteUserInput = null;
            OnTargetSelected = null;
        }

        public bool ProcessTargetSelection(UnitIndex unitIndex)
        {
            /*  We need to process each target group seperately one after the other for this Move.  */
            if (!Intention.UnitIntentionManager.Instance.TryGetIntention(unitIndex, out UnitIntention intention)) { return false; }

            /*  Find the first unResolved Target Group. */
            for (int i = 0; i < intention.TargetGroupResolvingStates.Count; i++)
            {
                if (intention.TargetGroupResolvingStates[i].IsResolved) { continue; }
                intention.CurrentProcessingTargetGroupIndex = i;
            }

            return ProcessTargetGroupSelection(unitIndex);
        }

        public bool ProcessTargetGroupSelection(UnitIndex unitIndex)
        {
            UnityEngine.Debug.Log("Processing Target Group Selection!");

            /*  Determine if we need to invoke the Player's Input systems to resolve this. If so, halt processing until it is done! */
            if (!TargetSelection.UnitTargetSelectorManager.Instance.TryGetTargetSelector(unitIndex, out TargetSelection.ITargetSelector targetSelector)) { UnityEngine.Debug.Log("Target selector invalid?!"); return false; }

            /*  If this is player driven, then we need to select that Unit if it isn't already and await the player's move selection.   */
            if (targetSelector is TargetSelection.PlayerDrivenTargetSelector)
            {
                OnRequireUserInput?.Invoke(unitIndex);
                return true;
            }
            else
            {
                UnityEngine.Debug.Log("Processing Autonomous Target Selector! " + targetSelector.ToString());
                ProcessTargetSelector(unitIndex, targetSelector);
                return true;
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

            ProcessTargetSelector(unitIndex, targetSelector);
        }

        /// =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=

        private bool ProcessTargetSelector(UnitIndex unitIndex, TargetSelection.ITargetSelector targetSelector)
        {
            if (targetSelector == null) { UnityEngine.Debug.Log("Target selector is null?"); return false; }

            /*  Create the scene data for this unit.    */
            if (!StationManagerUtilities.TryCreateCombatSceneDataForUnitIndex(unitIndex, out UnitTurnStationIndexesSceneData sceneData)){ return false; }

            /*  Get the move data for the target selection. */
            if(!Intention.UnitIntentionManager.Instance.TryGetIntention(unitIndex, out UnitIntention intention)) {  return false; }

            if (!UnitIntentionFactory.TryGetMoveTargetOfCurrentTargetGroup(intention, out MoveTarget moveTargetType)) { return false; }

            System.Collections.Generic.List<StationIndex> selectedTargets = targetSelector.SelectTargets(sceneData, moveTargetType);

            if (!UnitIntentionFactory.TryAssignTargetsToCurrentProcessingTargetGroup(intention, selectedTargets))
            {
                /*  If there is still groups to be selected, process the next target group. */
                ProcessTargetGroupSelection(unitIndex);
            }
            else
            {
                /*  Notify CombatRoundIntentionManager that all TargetGroups has been selected by this UnitIndex. */
                OnTargetSelected?.Invoke(unitIndex, selectedTargets);
            }

            return true;
        }
    }
}