using System.Collections.Generic;
using System.Linq;
using TurnBased.TurnOrder;
using UnityEngine;

namespace TurnBased
{
    public struct CombatReturnData
    {
        public CombatReturnData(IBattleMove battleMoveAction, UnitTeam source, BaseBattleUnit unitSource, List<BaseBattleUnit> users, List<BaseBattleUnit> targets, bool requiresMovement = false)
        {
            this.battleMoveAction = battleMoveAction;
            this.battleElementalMoveAction = null;

            this.teamSource = source;
            this.sourceUnit = unitSource;
            this.users = users;
            this.targets = targets;

            this.requiresMovement = requiresMovement;
        }
        public CombatReturnData(IElementalMoveAction elementalBattleMoveAction, UnitTeam source, BaseBattleUnit unitSource, List<BaseBattleUnit> users, List<BaseBattleUnit> targets, bool requiresMovement = false)
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
        public List<BaseBattleUnit> users;
        public List<BaseBattleUnit> targets;
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
        private StationManager StationHandler;

        private readonly MoveSelection.MoveSelectorManager      moveSelectorManager     = new();
        private readonly TargetSelection.TargetSelectorManager  targetSelectorManager   = new();
        private readonly TurnOrder.TurnOrderManager             turnOrderManager        = new();
        private readonly Phases.PhaseManager                    phaseManager            = new();


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
            this.moveSelectorManager.Initalise();
            this.targetSelectorManager.Initalise();
            this.turnOrderManager.Initalise();
            this.phaseManager.Initialise();
        }

        private void Start()
        {
            this.StationHandler = new StationManager();

            CreateCombatEncounter();
            this.StationHandler.DeployUnitsForStartOfBattle();         
        }

        private void Update()
        {
            this.phaseManager.UpdatePhases();

            if (Input.GetKeyDown(KeyCode.H))
            {
                this.StationHandler.RemoveUnit(TurnOrderManager.Instance.GetCurrentUnit());
            }
        }
    }
}