namespace TurnBased.GameState
{
    public class GameStateManager
    {
        public static event System.Action<MetaGameState> OnGameStateUpdated;

        private static GameStateManager instance;   
        public static GameStateManager Instance
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
        private MetaGameState curGameState;
        public MetaGameState CurrentGameState
        {
            get
            {
                return curGameState;
            }
            private set
            {
                this.curGameState = value;
            }
        }

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
        }

        public void DetermineGameState()
        {
            UnityEngine.Debug.LogWarning($"Determining game state");

            /*  If there are less than 0 Units on the Player's team, the player lost. Check this before checking enemy count.   */
            if (StationManager.Instance.GetUnitsOnTeam(UnitTeam.ALLY).Count <= 0)
            {
                this.CurrentGameState = MetaGameState.PlayerLoss;
            }
            else if (StationManager.Instance.GetUnitsOnTeam(UnitTeam.ENEMY).Count <= 0)
            {
                this.CurrentGameState = MetaGameState.PlayerWin;
            }
            else
            {
                this.CurrentGameState = MetaGameState.Running;
            }

            UnityEngine.Debug.LogWarning($"Game state is: {this.CurrentGameState}");

            OnGameStateUpdated?.Invoke(this.CurrentGameState); 
        }

    }
}