namespace TurnBased.UI
{
    public class UserInterfaceUserInput
    {
        private static UserInterfaceUserInput instance;
        public static UserInterfaceUserInput Instance
        {
            get
            {
                try
                {
                    return instance;
                }
                catch (System.Exception e)
                {
                    UnityEngine.Debug.LogError(e.ToString());
                    return null;
                }
            }
        }

        /// <summary>
        /// Invoked when we are awaiting user input, allows all subscribed classes to investigate the current intention for the Unit requesting user input.
        /// </summary>
        public static event System.Action<UnitIndex>    OnAwaitingUserInput;
        public static event System.Action<UnitIndex>    OnStopAwaitingUserInput;

        private UnitIndex? selectedUnit;


        public void Initalise()
        {
            /*  Initalise the Singleton.    */
            instance = this;
        }

        public void StartSelection(UnitIndex unitIndex)
        {
            this.selectedUnit = unitIndex;
            OnAwaitingUserInput?.Invoke(unitIndex);
        }

        public void StopSelection(UnitIndex unitIndex)
        {
            this.selectedUnit = null;
            OnStopAwaitingUserInput?.Invoke(unitIndex);
        }

        public void OnMoveSelection(IBattleMove selectedMove)
        {
            if (this.selectedUnit.HasValue)
            {
                Intention.MoveSelectionResolver.OnPlayerDrivenSelection(this.selectedUnit.Value, selectedMove);
            }
        }

        public void OnTargetSelection(System.Collections.Generic.List<StationIndex> selectedTarget)
        {
            UnityEngine.Debug.LogWarning("Called OnTargetSelection");
            if (this.selectedUnit.HasValue)
            {
                UnityEngine.Debug.LogWarning("Done target selection in UI");
                Intention.TargetSelectionResolver.OnPlayerDrivenSelection(this.selectedUnit.Value, selectedTarget);
            }
        }

        public bool IsAwaitingUserInput() => this.selectedUnit != null;

    }
}