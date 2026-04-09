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

        private UnitIndex selectedUnit;


        public void Initalise()
        {
            /*  Initalise the Singleton.    */
            instance = this;
        }

        public void StartMoveSelection(UnitIndex unitIndex)
        {
            this.selectedUnit = unitIndex;
        }

        public void OnMoveSelection()
        {
            Intention.MoveSelectionResolver.OnPlayerDrivenSelection(selectedUnit);
        }
    }
}