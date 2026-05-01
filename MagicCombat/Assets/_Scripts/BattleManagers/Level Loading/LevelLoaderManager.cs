using UnityEngine;
using UnityEngine.SceneManagement;

namespace TurnBased.LoaderUnloader
{
    public class LevelLoaderManager : MonoBehaviour
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
        [SerializeField] private LevelData currentLevelData;
        private int currentLevelIndex;


        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(this.gameObject);
            }
        }

        private void OnDestroy()
        {
            if (instance != null && instance == this)
            {
                instance = null;
            }
        }

        //private void Update()
        //{
        //    HandleScreenShot();
        //}

        //private void HandleScreenShot()
        //{
        //    if (Input.GetKeyDown(KeyCode.Escape))
        //    {
        //        string path = System.IO.Path.Combine(Application.dataPath, "screenshot.png");

        //        Debug.LogError("Printing screenshot at path: " + path);

        //        ScreenCapture.CaptureScreenshot(path);
        //    }
        //}


        /// <summary>
        /// Only switch to the level selection screen if we are in the main scene
        /// </summary>
        public void SwitchToLevelSelection()
        {
            if (SceneManager.GetActiveScene().buildIndex == SceneManager.GetSceneByName("MainScene").buildIndex)
            {
                SceneManager.LoadScene("LevelSelect");
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

            this.currentLevelData = this.AllLevelData[this.currentLevelIndex];

            if (SceneManager.GetActiveScene().buildIndex == SceneManager.GetSceneByName("LevelSelect").buildIndex)
            {
                SceneManager.LoadScene("MainScene");
            }
        }

        public void ReloadCurrentScene()
        {
            if (SceneManager.GetActiveScene().buildIndex != SceneManager.GetSceneByName("MainScene").buildIndex) { return; }

            SceneManager.LoadScene("MainScene");
        }

        public void LoadNextLevel()
        {
            if (SceneManager.GetActiveScene().buildIndex != SceneManager.GetSceneByName("MainScene").buildIndex) { return; }

            this.currentLevelIndex++;
            this.currentLevelData = this.AllLevelData[this.currentLevelIndex];

            if (this.currentLevelIndex > this.AllLevelData.Count - 1)
            {
                SceneManager.LoadScene("LevelSelect");                
            }
            else
            {
                SceneManager.LoadScene("MainScene");
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
            GameObject instanciatedGameObject = new GameObject(unitData.name);
            instanciatedGameObject.transform.localScale = new Vector3(0.5f, 0.5f, 1);

            BaseBattleUnit instanciatedBaseBattleUnit = instanciatedGameObject.AddComponent<BaseBattleUnit>();
            instanciatedBaseBattleUnit.Initialise(unitData);
            instanciatedBaseBattleUnit.SetTeam(unitTeam);

            instanciatedBaseBattleUnit.gameObject.transform.localScale = new Vector3(0.5f, 0.5f, 1);

            OnCreateUnit?.Invoke(instanciatedBaseBattleUnit);
        }

        public void SetLevelData(LevelData levelData)
        {
            this.currentLevelData = levelData;
        }

    }
}