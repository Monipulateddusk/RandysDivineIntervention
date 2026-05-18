namespace TurnBased.Intention
{
    public class IntentionResolverManager
    {
        private static IntentionResolverManager instance;
        public static IntentionResolverManager Instance
        {
            get
            {
                return instance;
            }
        }

        private MoveSelectionResolver MoveSelectionResolver = new();
        private TargetSelectionResolver TargetSelectionResolver = new();


        public void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }

            this.MoveSelectionResolver = new();
            this.TargetSelectionResolver = new();
        }

        public void OnDestroy()
        {
            if (instance != null && instance == this)
            {
                instance = null;
            }

            this.MoveSelectionResolver.OnDestroy();
            this.TargetSelectionResolver.OnDestroy();

        }

        #region Move Selection

        public void ProcessMoveSelection(UnitIndex unitIndex)
        {
            if (this.MoveSelectionResolver == null) { return; }

            this.MoveSelectionResolver.ProcessIntentionMoveSelection(unitIndex);
        }


        public void OnPlayerDrivenMoveSelection(UnitIndex unitIndex, IBattleMove selectedMove)
        {
            if (this.MoveSelectionResolver == null) { return; }

            this.MoveSelectionResolver.OnPlayerDrivenSelection(unitIndex, selectedMove);
        }

        #endregion

        #region Target Selection

        public void ProcessUnitIntentionTargetSelection(UnitIndex unitIndex)
        {
            if (this.TargetSelectionResolver == null) { return; }

            this.TargetSelectionResolver.ProcessTargetSelection(unitIndex);
        }

        public void ProcessResolvingStateTargetSelection(UnitTurnStationIndexesSceneData stationIndexesSceneData, ResolvingState elementalMoveResolvingState, TargetSelection.ITargetSelector targetSelector)
        {
            if (this.TargetSelectionResolver == null) { return; }

            this.TargetSelectionResolver.ProcessEnvironmentTargetSelection(stationIndexesSceneData, elementalMoveResolvingState, targetSelector);
        }

        public void OnPlayerDrivenTargetSelection(UnitIndex unitIndex, System.Collections.Generic.List<StationIndex> targets)
        {
            if (this.TargetSelectionResolver == null) { return; }

            this.TargetSelectionResolver.OnPlayerDrivenSelection(unitIndex, targets);
        }

        #endregion

    }

    public static class IntentionResolverUtility
    {
        public static System.Collections.Generic.List<UnitIndex> GetAllAutonomousUnits()
        {
            UnityEngine.Debug.LogError($"Getting autonomous units");

            System.Collections.Generic.List<UnitIndex> autonomousUnits = new();
            foreach (UnitIndex unitIndex in StationManager.Instance.GetAllActiveUnits())
            {
                UnityEngine.Debug.LogError($"Getting move selector of unit index: {unitIndex.Index}");

                if (!MoveSelection.MoveSelectorManager.Instance.TryGetMoveSelector(unitIndex, out MoveSelection.IMoveSelector moveSelector)) { continue; }

                UnityEngine.Debug.LogError($"Got move selector.");

                if (moveSelector is not MoveSelection.PlayerDrivenMoveSelector)
                {
                    UnityEngine.Debug.LogError($"Is not a player driven selector");

                    autonomousUnits.Add(unitIndex);
                }
            }
            return autonomousUnits;
        }

        public static System.Collections.Generic.List<UnitIndex> GetAllPlayerDrivenUnits()
        {
            System.Collections.Generic.List<UnitIndex> playerDrivenUnits = new();
            foreach (UnitIndex unitIndex in StationManager.Instance.GetAllActiveUnits())
            {
                if (!MoveSelection.MoveSelectorManager.Instance.TryGetMoveSelector(unitIndex, out MoveSelection.IMoveSelector moveSelector)) { continue; }

                if (moveSelector is MoveSelection.PlayerDrivenMoveSelector)
                {
                    playerDrivenUnits.Add(unitIndex);
                }
            }
            return playerDrivenUnits;
        }

        public static bool DoesListContainUnitIndex(UnitIndex unitIndex, System.Collections.Generic.List<UnitIndex> listOfUnitIndexes)
        {
            int index = unitIndex.Index;

            foreach (UnitIndex uIndex in listOfUnitIndexes)
            {
                if (uIndex.Index == index)
                {
                    return true;
                }
            }
            return false;
        }
    }
}