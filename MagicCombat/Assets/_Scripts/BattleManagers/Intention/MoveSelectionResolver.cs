namespace TurnBased.Intention
{
    public static class MoveSelectionResolver 
    {
        public static bool ProcessIntentionMoveSelection(UnitIndex unitIndex)
        {
            UnityEngine.Debug.Log("Try get move selector!");

            /*  Determine if we need to invoke the Player's Input systems to resolve this. If so, halt processing until it is done! */
            if (!MoveSelection.MoveSelectorManager.Instance.TryGetMoveSelector(unitIndex, out MoveSelection.IMoveSelector moveSelector)) { UnityEngine.Debug.Log("Move selector invalid?!"); return false; }



            /*  If this is player driven, then we need to select that Unit if it isn't already and await the player's move selection.   */
            if (moveSelector is MoveSelection.PlayerDrivenMoveSelector)
            {
                /*  Alert the UI    */
                TurnBased.UI.UserInterfaceUserInput.Instance.StartSelection(unitIndex);
                return true;
            }
            else
            {
                UnityEngine.Debug.Log("Move Selector! " + moveSelector.ToString());
                ProcessMoveSelector(unitIndex, moveSelector);
                return true;
            }
        }

        public static void ProcessMoveSelector(UnitIndex unitIndex, MoveSelection.IMoveSelector moveSelector)
        {
            if (moveSelector == null) { UnityEngine.Debug.Log("Move selector is null?"); return; }

            /*  Create the scene data for this unit.    */
            SceneData_UnitTurn sceneData = StationManagerUtilities.CreateCombatSceneDataForUnitIndex(unitIndex);
            IBattleMove selectedMove = moveSelector.SelectMove(sceneData);



            /*  Add this selected move to intentionManager. */
            UnitIntentionManager.Instance.SetMoveIntention(unitIndex, selectedMove);

            UnityEngine.Debug.Log("Invoking OnMoveSelectionComplete");

            IntentionResolver.Instance.ContinueProcessingSelectedUnitIntention(unitIndex);

        }

        public static void OnPlayerDrivenSelection(UnitIndex unitIndex, IBattleMove selectedMove)
        {
            if (!MoveSelection.MoveSelectorManager.Instance.TryGetMoveSelector(unitIndex, out MoveSelection.IMoveSelector moveSelector)) { return; }

            if (moveSelector is MoveSelection.PlayerDrivenMoveSelector)
            {
                (moveSelector as MoveSelection.PlayerDrivenMoveSelector).SelectedMove = selectedMove; 
            }

            ProcessMoveSelector(unitIndex, moveSelector);
        }

    }
}