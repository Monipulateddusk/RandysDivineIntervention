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
        [SerializeField] private LevelData currentLevelData;


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
        public void SwitchToMainSceneSelection()
        {
            if (SceneManager.GetActiveScene().buildIndex == SceneManager.GetSceneByName("LevelSelect").buildIndex)
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