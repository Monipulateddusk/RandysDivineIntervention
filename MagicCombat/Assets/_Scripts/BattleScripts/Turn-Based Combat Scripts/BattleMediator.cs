using System;
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

    public struct CombatSceneData
    {
        public List<ImbuedEnvironmentElement> environmentalEffects;
        public List<UnitSlot> targets;
        public List<UnitSlot> allies;
        public UnitSlot sourceUnit;

        public CombatSceneData(UnitSlot source, List<UnitSlot> allies, List<UnitSlot> targets, List<ImbuedEnvironmentElement> environmentalEffects)
        {
            this.environmentalEffects = environmentalEffects;
            this.targets = targets;
            this.allies = allies;
            this.sourceUnit = source;
        }
    }
    [Serializable]
    public struct UnitSlot
    {
        [SerializeField] public int? Index;
        [SerializeField] public UnitTeam Team;
        [SerializeField] public BaseBattleUnit Unit;

        public UnitSlot(int? index, UnitTeam unitTeam, BaseBattleUnit unit)
        {
            this.Index = index;
            this.Team = unitTeam;
            this.Unit = unit;
        }
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
        // public static event System.Action OnAttack;

        public enum PHASE_TYPES {START_ROUND, PRE_UNIT_TURN, UNIT_TURN, END_ROUND};
        
        Dictionary<PHASE_TYPES, Phase> PhaseDictionary;
        [SerializeField] private PHASE_TYPES CurrentPhaseType;
        private Phase CurrentPhase;

        private int playerFieldSlots = 0;
        private int enemyFieldSlots = 0;

        private List<UnitSlot> Units = new();
        [SerializeField] private List<UnitSlot> TurnOrderList = new();

        private UnitSlot? currentUnit;

        void CreateUnit(GameObject objectWithUnitComponent, int index, int slotsCount, UnitTeam unitTeam)
        {
            BaseBattleUnit spawnedUnit = Instantiate(objectWithUnitComponent).GetComponent<BaseBattleUnit>();
            //  enemyUnit.SetMediator(this);

            /*  Initalise the Unit Slot.    */
            UnitSlot slot = new(
                (index < slotsCount) ? index : null, 
                unitTeam,
                spawnedUnit);

            Units.Add(slot);
        }
        public void CreateCombatEncounter(int playerSlots, int enemySlots)
        {
            /*  
                Instanciate Enemies from Resources for now, we will do it differently later. 
                After that, compare the enemy Index with the slots. If the Index exceeds the amount of slots, the Unit spawned is in resurve.
            */
            int i = 0;
            foreach (GameObject obj in Resources.LoadAll("TempPrefabs/Enemies").Cast<GameObject>())
            {
                CreateUnit(obj, i, enemySlots, UnitTeam.ENEMY);
                i++;
            }


            // Instanciate active allies
            i = 0;
            foreach (GameObject obj in Resources.LoadAll("TempPrefabs/Players").Cast<GameObject>())
            {
                CreateUnit(obj, i, playerSlots, UnitTeam.ALLY);
                i++;
            }
        }

        public void CreateTurnOrderList()
        {
            TurnOrderList = GetAllActiveUnits();

            /*  Sort the List so that slowest Units are processed last. */
            TurnOrderList = TurnOrderList.OrderByDescending(unitSlot => unitSlot.Unit.GetBaseUnit().speed).ToList();
        }

        public UnitSlot? PopNextUnitInTurnOrder()
        {
            currentUnit = TurnOrderList.FirstOrDefault();
            TurnOrderList.RemoveAt(0);
            return currentUnit;
        }

        public CombatSceneData CreateCombatSceneDataForUnitSlot(UnitSlot sourceUnit)
        {
            /*  Get the Unit's Team and therefore the opposite team.    */
            UnitTeam allyTeam = sourceUnit.Team;

            List<UnitSlot> allies = new(), targets = new();
            foreach(UnitSlot unit in Units)
            {
                /*  Don't process the UnitSlot who called us. */
                if (unit.Unit == sourceUnit.Unit) { continue; }

                if(unit.Team == allyTeam)
                {
                    allies.Add(unit);
                }
                else
                {
                    targets.Add(unit);
                }

            }

            // TO DO: PASS IN ENVIRONMENT DATA
            return new CombatSceneData(sourceUnit, allies, targets, new());
        }

        private void Awake()
        {
            playerFieldSlots = 2;
            enemyFieldSlots = 4;
            CreateCombatEncounter(playerFieldSlots, enemyFieldSlots);

            PhaseDictionary = new()
            {
                {PHASE_TYPES.START_ROUND, new BeginRoundPhase(concreteMediator: this) },
                {PHASE_TYPES.PRE_UNIT_TURN, new PreTurnPhase(concreteMediator: this) },
                {PHASE_TYPES.UNIT_TURN, new UnitTurnPhase(concreteMediator : this) },
                {PHASE_TYPES.END_ROUND, new EndRoundPhase(concreteMediator : this) }
            };

            
            CurrentPhaseType = PHASE_TYPES.START_ROUND;
            ChangeState(CurrentPhaseType);
        }

        private void Update()
        {
            CurrentPhase?.Update();
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
            static string CurrentPhaseDebug(string currentPhaseTypeText)
            {
                switch (currentPhaseTypeText)
                {
                    case "BeginRoundPhase":
                        return "<color=orange>BeginRoundPhase</color>";
                    case "PreTurnPhase":
                        return "<color=yellow>PreTurnPhase</color>";
                    case "UnitTurnPhase":
                        return "<color=blue>UnitTurnPhase</color>";
                    case "EndRoundPhase":
                        return "<color=purple>EndRoundPhase</color>";
                    default:
                        return "<color=orange>BeginRoundPhase</color>";


                }
            }

            //print("<color=red>Exiting</color> Phase: " + CurrentPhaseDebug(CurrentPhase?.ToString()));
            CurrentPhase?.OnExit();

            CurrentPhaseType = newPhaseType;
            CurrentPhase = PhaseDictionary[newPhaseType];

            //print(" <color=green>Entering</color> Phase: " + CurrentPhaseDebug(CurrentPhase?.ToString()));
            CurrentPhase?.OnEnter();
        }

        public void NotifyConcreteMediator(BaseBattleUnit sender, CombatEvent ev)
        {
            throw new System.NotImplementedException();
        }

        #region Getter Methods
        public int GetPlayerSlotsCount() { return playerFieldSlots; }
        public int GetEnemySlotsCount() { return enemyFieldSlots; }
        public UnitSlot? GetCurrentUnit() => currentUnit;

        public List<UnitSlot> GetTurnOrderList() { return TurnOrderList; }

        public List<UnitSlot> GetUnits() { return Units; }

        public List<UnitSlot> GetAllUnitsOfTeam(UnitTeam team)
        {
            List<UnitSlot> teamedUnits = new();
            foreach (UnitSlot unit in Units)
            {
                if(unit.Team == team)
                {
                    teamedUnits.Add(unit);
                }
            }
            return teamedUnits;
        }

        public List<UnitSlot> GetAllActiveUnits()
        {
            List<UnitSlot> activeUnits = new();
            foreach (UnitSlot unit in Units)
            {
                if(unit.Index != null)
                {
                    activeUnits.Add(unit);
                }
            }
            return activeUnits;
        }

        public int? GetIndexOfBattleUnit(BaseBattleUnit unit)
        {
            foreach(UnitSlot slot in Units)
            {
                if(slot.Unit == unit)
                {
                    return slot.Index;
                }
            }
            return null;
        }

        #endregion
    }
}