namespace TurnBased.LoaderUnloader
{
    public class LevelLoaderManager : UnityEngine.MonoBehaviour
    {
        private static LevelLoaderManager instance;
        public static LevelLoaderManager Instance
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

        public static event System.Action<BaseBattleUnit> OnCreateUnit;
        private System.Collections.Generic.List<LevelData> AllLevelData;
        [UnityEngine.SerializeField] private LevelData currentLevelData;
        private int currentLevelIndex;


        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(this.gameObject);
            }
            else if (instance != null && instance != this)
            {
                Destroy(this.gameObject);
            }
        }

        private void OnDestroy()
        {
            if (instance != null && instance == this)
            {
                instance = null;
            }
        }


        /// <summary>
        /// Only switch to the level selection screen if we are in the main scene
        /// </summary>
        public void SwitchToLevelSelection()
        {
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex == UnityEngine.SceneManagement.SceneManager.GetSceneByName("MainScene").buildIndex)
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("LevelSelect");
            }
        }

        /// <summary>
        /// Only switch to the MainScene screen if we are in the level selection screen
        /// </summary>
        public void SwitchToMainSceneSelection(System.Collections.Generic.List<LevelData> levelData, int selectedLevelIndex)
        {
            if (levelData.Count <= 0) { return; } 

            this.AllLevelData = levelData;
            this.currentLevelIndex = selectedLevelIndex;

            if (this.AllLevelData == null || this.AllLevelData.Count <= 0) { UnityEngine.SceneManagement.SceneManager.LoadScene("MainScene"); }

            this.currentLevelData = this.AllLevelData[this.currentLevelIndex];

            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex == UnityEngine.SceneManagement.SceneManager.GetSceneByName("LevelSelect").buildIndex)
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("MainScene");
            }
        }

        public void ReloadCurrentScene()
        {
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex != UnityEngine.SceneManagement.SceneManager.GetSceneByName("MainScene").buildIndex) { return; }

            UnityEngine.SceneManagement.SceneManager.LoadScene("MainScene");
        }

        public void LoadNextLevel()
        {
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex != UnityEngine.SceneManagement.SceneManager.GetSceneByName("MainScene").buildIndex) { return; }

            this.currentLevelIndex++;

            if (this.currentLevelIndex > this.AllLevelData.Count)
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("LevelSelect");                
            }
            else
            {
                this.currentLevelData = this.AllLevelData[this.currentLevelIndex];
                UnityEngine.SceneManagement.SceneManager.LoadScene("MainScene");
            }
        }


        public void CreateCombatEncounter()
        {
            if (this.currentLevelData == null) { return; }
            /*  
                Instanciate Enemies from Resources for now, we will do it differently later. 
                After that, compare the enemy Index with the slots. If the Index exceeds the amount of slots, the Unit spawned is in resurve.
            */
            foreach (UnitData data in this.currentLevelData.AllyUnitsInLevel)
            {
                CreateUnit(data, UnitTeam.ALLY);
            }
            foreach (UnitData data in this.currentLevelData.EnemyUnitsInLevel)
            {
                CreateUnit(data, UnitTeam.ENEMY);
            }
        }



        void CreateUnit(UnitData unitData, UnitTeam unitTeam)
        {
            UnityEngine.GameObject instanciatedGameObject = new UnityEngine.GameObject(unitData.name);
            instanciatedGameObject.transform.localScale = new UnityEngine.Vector3(0.5f, 0.5f, 1);

            BaseBattleUnit instanciatedBaseBattleUnit = instanciatedGameObject.AddComponent<BaseBattleUnit>();
            instanciatedBaseBattleUnit.Initialise(unitData, unitTeam);

            instanciatedBaseBattleUnit.gameObject.transform.localScale = new UnityEngine.Vector3(0.5f, 0.5f, 1);

            OnCreateUnit?.Invoke(instanciatedBaseBattleUnit);
        }

        public void SetLevelData(LevelData levelData)
        {
            this.currentLevelData = levelData;
        }

    }
}