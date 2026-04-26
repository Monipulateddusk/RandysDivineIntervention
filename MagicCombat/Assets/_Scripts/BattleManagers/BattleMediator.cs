using System.Linq;
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
        private readonly StationManager                         StationHandler              = new();
        private readonly StationSelectorManager                 StationSelectorManager      = new();

        private readonly Phases.CombatTurnOrchestrator          CombatTurnOrchestrator      = new();
        private readonly LoaderUnloader.UnitDeathHandler        UnitDeathHandler            = new();
        private readonly GameState.GameStateManager             GameStateManager            = new();

        private readonly MoveSelection.MoveSelectorManager      MoveSelectorManager         = new();
        private readonly TargetSelection.TargetSelectorManager  TargetSelectorManager       = new();
        private readonly TurnOrder.TurnOrderManager             TurnOrderManager            = new();
        private readonly Intention.UnitIntentionManager         UnitIntentionManager        = new();
        private readonly Intention.IntentionVisualiserManager   IntentionVisualiserManager  = new();
        private readonly AttackResolution.AttackResolutionManager AttackResolutionManager   = new();
        private readonly Health.UnitHealthManager               UnitHealthManager = new();

        [SerializeField] GameObject textPrefab;

        private void Awake()
        {
            instance = this;

            this.UnitDeathHandler.Awake();
            this.TurnOrderManager.Awake();
            this.UnitIntentionManager.Awake();
            this.StationHandler.Awake();
            this.StationSelectorManager.Awake();
            this.MoveSelectorManager.Awake();
            this.TargetSelectorManager.Awake();
            this.IntentionVisualiserManager.Awake();
            this.AttackResolutionManager.Awake();
            this.UnitHealthManager.Awake();

            this.CombatTurnOrchestrator.Awake();
            this.GameStateManager.Awake();

            LoaderUnloader.LevelLoaderManager.OnCreateUnit += LevelLoaderManager_OnCreateUnit;
        }

        private void LevelLoaderManager_OnCreateUnit(BaseBattleUnit instanciatedUnit)
        {
            this.StationHandler.CreateUnit(instanciatedUnit);
        }

        private void OnDestroy()
        {
            this.UnitIntentionManager.OnDestroy();
            this.MoveSelectorManager.OnDestroy();
            this.TargetSelectorManager.OnDestroy();
            this.IntentionVisualiserManager.OnDestroy();
            this.UnitHealthManager.OnDestroy();

            this.GameStateManager.OnDestroy();
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