namespace TurnBased.Intention
{
    public class MoveSelectionResolver 
    {
        private static MoveSelectionResolver instance;
        public static MoveSelectionResolver Instance
        {
            get
            {
                return instance;                
            }
        }


        public static System.Action<UnitIndex> OnRequireUserInput;
        public static System.Action<UnitIndex> OnCompleteUserInput;
        public static System.Action<UnitIndex, IBattleMove> OnMoveSelected;

        public void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
        }

        public void OnDestroy()
        {
            if (instance != null && instance == this)
            {
                instance = null;
            }

            OnRequireUserInput = null;
            OnCompleteUserInput = null;
            OnMoveSelected = null;
        }
        public bool ProcessIntentionMoveSelection(UnitIndex unitIndex)
        {
            UnityEngine.Debug.LogWarning("Try get move selector!");

            /*  Determine if we need to invoke the Player's Input systems to resolve this. If so, halt processing until it is done! */
            if (!MoveSelection.MoveSelectorManager.Instance.TryGetMoveSelector(unitIndex, out MoveSelection.IMoveSelector moveSelector)) { UnityEngine.Debug.Log("Move selector invalid?!"); return false; }



            /*  If this is player driven, then we need to select that Unit if it isn't already and await the player's move selection.   */
            if (moveSelector is MoveSelection.PlayerDrivenMoveSelector)
            {
                UnityEngine.Debug.LogWarning($"Move Selector {moveSelector.ToString()} requires user input for UnitIndex: {unitIndex.Index}!");

                OnRequireUserInput?.Invoke(unitIndex);
                return true;
            }
            else
            {
                UnityEngine.Debug.LogWarning("Processing Autonomous Move Selector! " + moveSelector.ToString());
                ProcessMoveSelector(unitIndex, moveSelector);
                return true;
            }
        }

        /// =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=
        /// 
        /// When A Player Selects a Move Via UI
        /// 
        /// =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=

        public void OnPlayerDrivenSelection(UnitIndex unitIndex, IBattleMove selectedMove)
        {
            if (!MoveSelection.MoveSelectorManager.Instance.TryGetMoveSelector(unitIndex, out MoveSelection.IMoveSelector moveSelector)) { return; }

            if (moveSelector is MoveSelection.PlayerDrivenMoveSelector)
            {
                (moveSelector as MoveSelection.PlayerDrivenMoveSelector).SelectedMove = selectedMove; 
            }

            UnityEngine.Debug.LogError("Before OnCompleteUserInput event");

            OnCompleteUserInput?.Invoke(unitIndex);
            UnityEngine.Debug.LogError("After OnCompleteUserInput event");

            ProcessMoveSelector(unitIndex, moveSelector);
        }

        /// =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=



        public void ProcessMoveSelector(UnitIndex unitIndex, MoveSelection.IMoveSelector moveSelector)
        {
            if (moveSelector == null) { UnityEngine.Debug.Log("Move selector is null?"); return; }

            /*  Create the scene data for this unit.    */
            if (!StationManagerUtilities.TryCreateCombatSceneDataForUnitIndex(unitIndex, out SceneData_UnitTurn sceneData)) {  return; }
            IBattleMove selectedMove = moveSelector.SelectMove(sceneData);

            /*  Notify CombatRoundIntentionManager that a move has been selected by this UnitIndex. */
            OnMoveSelected?.Invoke(unitIndex, selectedMove);
        }
    }
}