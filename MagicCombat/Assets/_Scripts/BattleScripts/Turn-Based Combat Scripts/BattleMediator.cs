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

    #region Combat Events

    public class CombatEvent { }

    public class CombatAttackEvent : CombatEvent
    {
        public AttackResolutionInfo AttackInfo { get; set; }
        public UnitIntention IntentData { get; set; }

        public CombatAttackEvent(AttackResolutionInfo attackInfo, UnitIntention intentData)
        {
            AttackInfo = attackInfo;
            IntentData = intentData;
        }
    }
    #endregion
    public interface ICombatMediator
    {
        void NotifyConcreteMediator(BaseBattleUnit sender, CombatEvent ev);
    }

    public class BattleMediator : MonoBehaviour, ICombatMediator
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
        private StationHandler StationHandler;
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
            //  enemyUnit.SetMediator(this);

            /*  Initalise the Unit Slot.    */
            this.StationHandler.AddUnit(spawnedUnit, unitTeam, new StationIndex() { Index = stationIndexValue });
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

        public void CreateTurnOrderList()
        {
            UnitIndexTurnOrderList = this.StationHandler.GetAllActiveUnits();

            /*  Sort the List so that slowest Units are processed last. */
            UnitIndexTurnOrderList.Sort((g1, g2) => this.StationHandler.GetBattleUnitOfIndex(g1).GetBaseUnit().speed.CompareTo(this.StationHandler.GetBattleUnitOfIndex(g2).GetBaseUnit().speed));
            UnitIndexTurnOrderList.Reverse();

            OnUpdateTurnOrder?.Invoke(UnitIndexTurnOrderList);
        }

        public UnitIndex? PopNextUnitInTurnOrder()
        {
            currentUnit = UnitIndexTurnOrderList.FirstOrDefault();
            UnitIndexTurnOrderList.RemoveAt(0);
            OnUpdateTurnOrder?.Invoke(UnitIndexTurnOrderList);
            return currentUnit;
        }

        public SceneData_UnitTurn GetCombatSceneDataForSourceUnitIndex(UnitIndex sourceUnitIndex)
        {
            return this.StationHandler.CreateCombatSceneDataForUnitIndex(sourceUnitIndex);
        }

        private void Awake()
        {
            instance = this;
        }

        private void Start()
        {
            int playerFieldSlots = 2, enemyFieldSlots = 4;
            this.StationHandler = new StationHandler(playerFieldSlots, enemyFieldSlots);

            CreateCombatEncounter();



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

        public void NotifyConcreteMediator(BaseBattleUnit sender, CombatEvent ev)
        {
            throw new System.NotImplementedException();
        }

        #region Getter Methods
        public BaseBattleUnit GetBattleUnitOfUnitIndex(UnitIndex index) { return this.StationHandler.GetBattleUnitOfIndex(index); }
        public UnitTeam GetUnitTeamOfUnitIndex(UnitIndex index) { return this.StationHandler.GetUnitTeamOfIndex(index); }
        public StationIndex? GetStationIndexOfUnitIndex(UnitIndex index) { return this.StationHandler.GetStationOfIndex(index); }

        public int GetPlayerSlotsCount() { return this.StationHandler.GetAllyStationSlots(); }
        public int GetEnemySlotsCount() { return this.StationHandler.GetEnemyStationSlots(); }
        public UnitIndex? GetCurrentUnit() => currentUnit;

        public List<UnitIndex> GetTurnOrderList() { return this.UnitIndexTurnOrderList; }

        public List<UnitIndex> GetAllUnitsOfTeam(UnitTeam team){ return this.StationHandler.GetUnitIndexesOfTeam(team).ToList();   }
        public List<StationIndex?> GetStations() => this.StationHandler.GetStations().ToList();
        public UnitIndex? GetUnitIndexOnStation(StationIndex stationIndex) { return this.StationHandler.GetUnitIndexOnStation(stationIndex); }

        #endregion
    }
}