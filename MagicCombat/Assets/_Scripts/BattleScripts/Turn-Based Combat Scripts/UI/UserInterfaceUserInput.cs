using System.Diagnostics;

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
        }

        public void StopSelection()
        {
            this.selectedUnit = null;
        }

        public void OnMoveSelection(IBattleMove selectedMove)
        {
            if (this.selectedUnit.HasValue)
            {
                Intention.MoveSelectionResolver.OnPlayerDrivenSelection(this.selectedUnit.Value, selectedMove);
            }
        }

        public void OnTargetSelection()
        {
            UnityEngine.Debug.LogWarning("Called OnTargetSelection");
            if (this.selectedUnit.HasValue)
            {
                UnityEngine.Debug.LogWarning("Done target selection in UI");
                Intention.TargetSelectionResolver.OnPlayerDrivenSelection(this.selectedUnit.Value);
            }
        }
    }
}