using UnityEngine;

namespace TurnBased
{
    public struct CombatReturnData
    {
        public CombatReturnData(IBattleMove battleMoveAction, UnitTeam source, BaseBattleUnit unitSource, System.Collections.Generic.List<BaseBattleUnit> users, System.Collections.Generic.List<BaseBattleUnit> targets, bool requiresMovement = false)
        {
            this.battleMoveAction = battleMoveAction;
            this.battleElementalMoveAction = null;

            this.teamSource = source;
            this.sourceUnit = unitSource;
            this.users = users;
            this.targets = targets;

            this.requiresMovement = requiresMovement;
        }
        public CombatReturnData(IElementalMoveAction elementalBattleMoveAction, UnitTeam source, BaseBattleUnit unitSource, System.Collections.Generic.List<BaseBattleUnit> users, System.Collections.Generic.List<BaseBattleUnit> targets, bool requiresMovement = false)
        {
            this.battleMoveAction = null;
            this.battleElementalMoveAction = elementalBattleMoveAction;

            this.teamSource = source;
            this.sourceUnit = unitSource;
            this.users = users;
            this.targets = targets;

            this.requiresMovement = requiresMovement;
        }
        public BaseBattleUnit sourceUnit;
        public System.Collections.Generic.List<BaseBattleUnit> users;
        public System.Collections.Generic.List<BaseBattleUnit> targets;
        public UnitTeam teamSource;
        public IBattleMove battleMoveAction;
        public IElementalMoveAction battleElementalMoveAction;
        public bool requiresMovement;
    }



    public class BattleMediator : MonoBehaviour
    {
        private static BattleMediator instance;
        public static BattleMediator Instance
        {
            get
            {
                if (instance == null)
                {
                    Debug.Log("BattleMediator is NULL");
                }
                return instance;
            }

        }
        private StationManager                          StationHandler              = new();
        private StationSelectorManager                  StationSelectorManager      = new();
        private Elements.CombatEnvironmentController    CombatEnvironmentController = new();

        private Phases.CombatTurnOrchestrator           CombatTurnOrchestrator      = new();
        private LoaderUnloader.UnitDeathHandler         UnitDeathHandler            = new();
        private GameState.GameStateManager              GameStateManager            = new();

        private Intention.IntentionVisualiserManager    IntentionVisualiserManager = new();


        [SerializeField] GameObject textPrefab;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }

            this.StationHandler = new();
            this.StationSelectorManager = new();
            this.CombatEnvironmentController = new();
            this.CombatTurnOrchestrator = new();
            this.UnitDeathHandler = new();
            this.GameStateManager = new();
            this.IntentionVisualiserManager = new();


            this.StationHandler.Awake();
            this.StationSelectorManager.Awake();
            this.CombatEnvironmentController.Awake();

            this.CombatTurnOrchestrator.Awake();
            this.UnitDeathHandler.Awake();
            this.GameStateManager.Awake();

            this.IntentionVisualiserManager.Awake();

            LoaderUnloader.LevelLoaderManager.OnCreateUnit += LevelLoaderManager_OnCreateUnit;
        }

        private void LevelLoaderManager_OnCreateUnit(BaseBattleUnit instanciatedUnit)
        {
            this.StationHandler.CreateUnit(instanciatedUnit);
        }

        private void OnDestroy()
        {
            if (instance != null && instance == this)
            {
                instance = null;
            }

            this.IntentionVisualiserManager.OnDestroy();  

            this.GameStateManager.OnDestroy();
            this.UnitDeathHandler.OnDestroy();
            this.CombatTurnOrchestrator.OnDestroy();

            this.CombatEnvironmentController.OnDestroy();
            this.StationSelectorManager.OnDestroy();
            this.StationHandler.OnDestroy();

            LoaderUnloader.LevelLoaderManager.OnCreateUnit -= LevelLoaderManager_OnCreateUnit;
        }

        private void Start()
        {
            LoaderUnloader.LevelLoaderManager.Instance.CreateCombatEncounter();  



            this.IntentionVisualiserManager.SetTextPrefab(textPrefab);



            this.StationHandler.DeployUnitsForStartOfBattle();
            this.StationSelectorManager.Start();

            this.CombatTurnOrchestrator.Start();

        }

        private void Update()
        {
            this.CombatTurnOrchestrator.Update();
        }
    }
}