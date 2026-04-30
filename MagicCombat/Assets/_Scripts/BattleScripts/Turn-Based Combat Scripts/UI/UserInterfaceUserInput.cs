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

        private UnitIndex? selectedUnit;


        public void Initalise()
        {
            /*  Initalise the Singleton.    */
            instance = this;
        }

        public void StartSelection(UnitIndex unitIndex)
        {
            this.selectedUnit = unitIndex;

            UnityEngine.Debug.LogError($"Start UI selection for UnitIndex: {unitIndex.Index}");
        }

        public void StopSelection(UnitIndex unitIndex)
        {
            this.selectedUnit = null;
            UnityEngine.Debug.LogError($"Stop UI selection for UnitIndex: {unitIndex.Index}");

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
            if (this.selectedUnit.HasValue)
            {
                Intention.TargetSelectionResolver.OnPlayerDrivenSelection(this.selectedUnit.Value, selectedTarget);
            }
        }

        public UnitIndex? GetSelectedUnit() => this.selectedUnit;
    }
}