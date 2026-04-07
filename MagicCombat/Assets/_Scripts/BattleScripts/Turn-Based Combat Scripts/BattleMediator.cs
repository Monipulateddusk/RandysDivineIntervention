using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TurnBased
{
    public struct CombatReturnData
    {
        public CombatReturnData(IBattleMoveAction battleMoveAction, UnitTeam source, BaseBattleUnit unitSource, List<BaseBattleUnit> users, List<BaseBattleUnit> targets, bool requiresMovement = false)
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
        public IBattleMoveAction battleMoveAction;
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
        [SerializeField] private List<UnitIndex> UnitIndexTurnOrderList = new();
        [SerializeField] private UnitIndex currentUnit;


        public enum PHASE_TYPES {START_ROUND, PRE_UNIT_TURN, UNIT_TURN, END_ROUND};
        
        Dictionary<PHASE_TYPES, Phase> PhaseDictionary;
        [SerializeField] private PHASE_TYPES CurrentPhaseType;
        private Phase CurrentPhase;

         public static event System.Action<List<UnitIndex>> OnUpdateTurnOrder;



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

        public List<UnitIndex> CreateTurnOrderList()
        {
            this.UnitIndexTurnOrderList = this.StationHandler.GetAllActiveUnits();

            /*  Sort the List so that slowest Units are processed last. */
            this.UnitIndexTurnOrderList.Sort((g1, g2) =>
            {
                this.StationHandler.GetBattleUnitOfIndex(g1, out BaseBattleUnit unit1);
                this.StationHandler.GetBattleUnitOfIndex(g2, out BaseBattleUnit unit2);

                return unit1.GetBaseUnit().speed.CompareTo(unit2.GetBaseUnit().speed);
            });
     
            this.UnitIndexTurnOrderList.Reverse();

            OnUpdateTurnOrder?.Invoke(UnitIndexTurnOrderList);
            return this.UnitIndexTurnOrderList;
        }

        public UnitIndex? PopNextUnitInTurnOrder()
        {
            if(!(this.UnitIndexTurnOrderList.Count > 0)){ return null;  }

            currentUnit = UnitIndexTurnOrderList.FirstOrDefault();
            UnitIndexTurnOrderList.RemoveAt(0);
            OnUpdateTurnOrder?.Invoke(UnitIndexTurnOrderList);
            return currentUnit;     
   
        }

        //public SceneData_UnitTurn GetCombatSceneDataForSourceUnitIndex(UnitIndex sourceUnitIndex)
        //{
        //    return this.StationHandler.CreateCombatSceneDataForUnitIndex(sourceUnitIndex);
        //}

        private void Awake()
        {
            instance = this;
        }

        private void Start()
        {
            this.StationHandler = new StationManager();

            CreateCombatEncounter();
            this.StationHandler.DeployUnitsForStartOfBattle();


            PhaseDictionary = new()
            {
                {PHASE_TYPES.START_ROUND,   new BeginRoundPhase (concreteMediator : this) },
                {PHASE_TYPES.PRE_UNIT_TURN, new PreTurnPhase    (concreteMediator : this) },
                {PHASE_TYPES.UNIT_TURN,     new UnitTurnPhase   (concreteMediator : this) },
                {PHASE_TYPES.END_ROUND,     new EndRoundPhase   (concreteMediator : this) }
            };

            
            CurrentPhaseType = PHASE_TYPES.START_ROUND;
            ChangeState(CurrentPhaseType);
        }

        private void Update()
        {
            CurrentPhase?.Update();

            if (Input.GetKeyDown(KeyCode.H))
            {
                this.StationHandler.RemoveUnit(currentUnit);
            }
        }

        public void ChangeToNextStateInOrder()
        {
            // Get which state we are in, decide which state is next
            int index = (int)CurrentPhaseType;

            index++;

            if (index > PhaseDictionary.Count -1)
            {
                index = 0;
            }

            ChangeState((PHASE_TYPES)index);
        }

        public void ChangeState(PHASE_TYPES newPhaseType)
        {
            CurrentPhase?.OnExit();

            CurrentPhaseType = newPhaseType;
            CurrentPhase = PhaseDictionary[newPhaseType];

            CurrentPhase?.OnEnter();
        }

        #region Getter Methods
        public List<UnitIndex>      GetTurnOrderList()                                                                          =>  this.UnitIndexTurnOrderList;
        public UnitIndex?           GetCurrentUnit()                                                                            =>  this.currentUnit;
        #endregion
    }
}