namespace TurnBased.AttackResolution {
    public class GameStateVisualisation
    {
        private static GameStateVisualisation instance;
        public static GameStateVisualisation Instance
        {
            get
            {
                return instance;
            }
            set
            {
                if (instance == null)
                {
                    instance = value;
                }
            }
        }

        public void Awake()
        {
            instance = this;
        }

        public void ProcessGameStateVisualisation(GameState currentGameState)
        {
            if (currentGameState == GameState.Running) { return; }

            switch (currentGameState)
            {

                case GameState.PlayerLoss:
                    VisualisePlayerLoss();
                    break;

                case GameState.PlayerWin:
                    VisualisePlayerWin();
                    break;

                default:
                    break;

            }
        }

        private void VisualisePlayerWin()
        {
            /*  Tell the UI Manager to Visualise the Victory Screen.    */

        }

        private void VisualisePlayerLoss()
        {
            /*  Tell the UI Manager to Visualise the Losing Screen.    */

        }
    }
}