namespace TurnBased.Intention
{
    public static class TargetSelectionResolver
    {
        public static System.Action<UnitIndex> OnRequireUserInput;
        public static System.Action<UnitIndex> OnCompleteUserInput;
        public static System.Action<UnitIndex, System.Collections.Generic.List<StationIndex>> OnTargetSelected;

        public static bool ProcessIntentionTargetSelection(UnitIndex unitIndex)
        {
            UnityEngine.Debug.Log("Try get Target selector!");

            /*  Determine if we need to invoke the Player's Input systems to resolve this. If so, halt processing until it is done! */
            if (!TargetSelection.TargetSelectorManager.Instance.TryGetTargetSelector(unitIndex, out TargetSelection.ITargetSelector targetSelector)) { UnityEngine.Debug.Log("Target selector invalid?!"); return false; }



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
        /// When A Player Selects a Move Via UI
        /// 
        /// =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=

        public static void OnPlayerDrivenSelection(UnitIndex unitIndex, System.Collections.Generic.List<StationIndex> selectedTarget)
        {
            if (!TargetSelection.TargetSelectorManager.Instance.TryGetTargetSelector(unitIndex, out TargetSelection.ITargetSelector targetSelector)) { return; }

            if (targetSelector is TargetSelection.PlayerDrivenTargetSelector)
            {
                (targetSelector as TargetSelection.PlayerDrivenTargetSelector).SelectedTarget = selectedTarget;
            }

            OnCompleteUserInput?.Invoke(unitIndex);

            ProcessTargetSelector(unitIndex, targetSelector);
        }

        /// =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=

        public static bool ProcessTargetSelector(UnitIndex unitIndex, TargetSelection.ITargetSelector targetSelector)
        {
            if (targetSelector == null) { UnityEngine.Debug.Log("Target selector is null?"); return false; }

            /*  Create the scene data for this unit.    */
            SceneData_UnitTurn sceneData = StationManagerUtilities.CreateCombatSceneDataForUnitIndex(unitIndex);

            /*  Get the move data for the target selection. */
            if(!Intention.UnitIntentionManager.Instance.TryGetIntention(unitIndex, out UnitIntention intention)) {  return false; } 

            System.Collections.Generic.List<StationIndex> selectedTargets = targetSelector.SelectTargets(sceneData, intention.MoveSelection);

            /*  Notify CombatRoundIntentionManager that a Target has been selected by this UnitIndex. */
            OnTargetSelected?.Invoke(unitIndex, selectedTargets);
            return true;
        }
    }
}