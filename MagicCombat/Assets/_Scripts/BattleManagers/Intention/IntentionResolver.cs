namespace TurnBased.Intention
{
    public class IntentionResolverManager
    {
        private MoveSelectionResolver MoveSelectionResolver = new();
        private TargetSelectionResolver TargetSelectionResolver = new();

        public void Awake()
        {
            this.MoveSelectionResolver = new();
            this.TargetSelectionResolver = new();

            this.MoveSelectionResolver.Awake();
            this.TargetSelectionResolver.Awake();
        }

        public void OnDestroy()
        {
            this.MoveSelectionResolver.OnDestroy();
            this.TargetSelectionResolver.OnDestroy();
        }

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