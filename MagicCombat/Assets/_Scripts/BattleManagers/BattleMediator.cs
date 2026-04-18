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

        private readonly MoveSelection.MoveSelectorManager      MoveSelectorManager         = new();
        private readonly TargetSelection.TargetSelectorManager  TargetSelectorManager       = new();
        private readonly TurnOrder.TurnOrderManager             TurnOrderManager            = new();
        private readonly Phases.PhaseManager                    PhaseManager                = new();
        private readonly Intention.IntentionResolver            IntentionResolver           = new();
        private readonly Intention.UnitIntentionManager         UnitIntentionManager        = new();
        private readonly Intention.IntentionVisualiserManager   IntentionVisualiserManager  = new();
        private readonly AttackResolution.AttackResolutionManager AttackResolutionManager   = new();


        [SerializeField] TMPro.TextMeshProUGUI tempUnitResolutionChangeVisualiser;
        [SerializeField] GameObject textPrefab;

        void CreateUnit(GameObject objectWithUnitComponent, int stationIndexValue, UnitTeam unitTeam)
        {
            BaseBattleUnit spawnedUnit = Instantiate(objectWithUnitComponent).GetComponent<BaseBattleUnit>();
            spawnedUnit.SetTeam(unitTeam);

            /*  Initalise the Unit Slot.    */
            this.StationHandler.CreateUnit(spawnedUnit);
        }
        public void CreateCombatEncounter()
        {
            /*  
                Instanciate Enemies from Resources for now, we will do it differently later. 
                After that, compare the enemy Index with the slots. If the Index exceeds the amount of slots, the Unit spawned is in resurve.
            */
            int i = 0;
            foreach (GameObject obj in Resources.LoadAll("TempPrefabs/Enemies").Cast<GameObject>())
            {
                CreateUnit(obj, i, UnitTeam.ENEMY);
                i++;
            }


            // Instanciate active allies
            foreach (GameObject obj in Resources.LoadAll("TempPrefabs/Players").Cast<GameObject>())
            {
                CreateUnit(obj, i, UnitTeam.ALLY);
                i++;
            }
        }

        private void Awake()
        {
            instance = this;
            this.TurnOrderManager.Awake();
            this.IntentionResolver.Awake();
            this.UnitIntentionManager.Awake();
            this.StationHandler.Awake();
            this.StationSelectorManager.Awake();
            this.MoveSelectorManager.Awake();
            this.TargetSelectorManager.Awake();
            this.PhaseManager.Awake();
            this.IntentionVisualiserManager.Awake();
            this.AttackResolutionManager.Awake();


            Phases.MainTurnManager.OnUnitIntentionResolutionStateChange += IntentionResolver_OnUnitIntentionResolutionStateChange;
        }

        private void OnDestroy()
        {
            Phases.MainTurnManager.OnUnitIntentionResolutionStateChange -= IntentionResolver_OnUnitIntentionResolutionStateChange;
        }

        private void IntentionResolver_OnUnitIntentionResolutionStateChange(UnitIndex unitIndex, UnitIntentionResolutionState state)
        {
            if (tempUnitResolutionChangeVisualiser != null)
            {
                if (!StationManager.Instance.TryGetUnitDataOfUnitIndex(unitIndex, out UnitData unitData)) { return; }

                this.tempUnitResolutionChangeVisualiser.text = $"UnitIndex: {unitIndex.Index} named: {unitData.name} is in this Resolution State: {state}";
            }
        }

        private void Start()
        {
            this.IntentionVisualiserManager.SetTextPrefab(textPrefab);

            CreateCombatEncounter();
            this.StationHandler.DeployUnitsForStartOfBattle();
            this.StationSelectorManager.Start();

            this.PhaseManager.Initialise();
        }

        private void Update()
        {
            this.PhaseManager.UpdatePhases();
        }



    }
}