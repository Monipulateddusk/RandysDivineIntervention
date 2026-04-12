namespace TurnBased.Intention
{
    public static class TargetSelectionResolver
    {
        public static event System.Action<UnitIndex> OnTargetSelectionComplete;
        public static bool ProcessIntentionTargetSelection(UnitIndex unitIndex)
        {
            UnityEngine.Debug.Log("Try get Target selector!");

            /*  Determine if we need to invoke the Player's Input systems to resolve this. If so, halt processing until it is done! */
            if (!TargetSelection.TargetSelectorManager.Instance.TryGetTargetSelector(unitIndex, out TargetSelection.ITargetSelector targetSelector)) { UnityEngine.Debug.Log("Target selector invalid?!"); return false; }



            /*  If this is player driven, then we need to select that Unit if it isn't already and await the player's move selection.   */
            if (targetSelector is TargetSelection.PlayerDrivenTargetSelector)
            {
                /*  Select this Unit in our Station selector and tell the UI that we are awaiting calls.    */
                if (!StationManager.Instance.TryGetStationIndexOfIndex(unitIndex, out StationIndex stationIndexOfUnitIndex)) { return false; }
                StationSelectorManager.Instance.SetSelectedStationIndex(stationIndexOfUnitIndex);

                /*  Alert the UI    */
                TurnBased.UI.UserInterfaceUserInput.Instance.StartSelection(unitIndex);
                return true;
            }
            else
            {
                UnityEngine.Debug.Log("Move Selector! " + targetSelector.ToString());
                ProcessTargetSelector(unitIndex, targetSelector);
                return true;
            }
        }

        public static bool ProcessTargetSelector(UnitIndex unitIndex, TargetSelection.ITargetSelector targetSelector)
        {
            if (targetSelector == null) { UnityEngine.Debug.Log("Target selector is null?"); return false; }

            /*  Create the scene data for this unit.    */
            SceneData_UnitTurn sceneData = StationManagerUtilities.CreateCombatSceneDataForUnitIndex(unitIndex);

            /*  Get the move data for the target selection. */
            if(!Intention.UnitIntentionManager.Instance.TryGetIntention(unitIndex, out UnitIntention intention)) {  return false; } 

            System.Collections.Generic.List<StationIndex> selectedTargets = targetSelector.SelectTargets(sceneData, intention.MoveSelection);


            /*  Add this selected move to intentionManager. */
            UnitIntentionManager.Instance.SetTargetIntention(unitIndex, selectedTargets);

            UnityEngine.Debug.Log("Invoking OnTargetSelectionComplete");

            OnTargetSelectionComplete?.Invoke(unitIndex);
            return true;
        }
        public static bool OnPlayerDrivenSelection(UnitIndex unitIndex)
        {
            if (!TargetSelection.TargetSelectorManager.Instance.TryGetTargetSelector(unitIndex, out TargetSelection.ITargetSelector targetSelector)){ return false; }

            ProcessTargetSelector(unitIndex, targetSelector);

            OnTargetSelectionComplete?.Invoke(unitIndex);

            return true;
        }

    }
}