using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;

[Serializable] public struct UnitIndex     {  public int Index;   }
[Serializable] public struct StationIndex  {  public int Index;   }

public struct SoA_SceneUnitData
{
    /*  These arrays will be linked per Unit. 
     *  So Unit 1 will be Element 0 of Units.
     *  Their Team will be stored at Element 0 of Teams
     *  Their Station will be stored at Element 0 of Stations.  
     *  Stations can be Nullable do determine if an Unit is on the Field or not.
    */
    public BaseBattleUnit[] Units;
    public UnitTeam[] Teams;
    public StationIndex?[] Stations;

    public int AllyStationSlots { get; private set; }
    public int EnemyStationSlots { get; private set; }

    public SoA_SceneUnitData(BaseBattleUnit[] units, UnitTeam[] teams, StationIndex?[] stations, int allySlots, int enemySlots)
    {
        this.Units = units;
        this.Teams = teams;
        this.Stations = stations;
        this.AllyStationSlots = allySlots;
        this.EnemyStationSlots = enemySlots;
    }
}

public class SceneUnitData
{
    public static int MAX_UNITS_PER_SIDE = 9;
    private SoA_SceneUnitData data;

    /// <summary>
    /// Invoked when a Unit is added. Normally at start of game.    
    /// UnitIndex: Unit Index of the Unit being added.  
    /// </summary>
    public static event Action<UnitIndex> OnAddUnit;

    /// <summary>
    /// Invoked when a Unit is Removed. When a unit is destroyed.   
    /// Unit Index: UnitIndex of the Destroyed Unit
    /// Station Index: StationIndex that the Unit was on. Could be Null if it was destroyed off field.  
    /// BaseBattleUnit: Main Script of the Unit, allows use of the GameObject.
    /// 
    /// IMPORTANT: As we remove the UnitIndex, StationIndex from arrays, you cannot use any method within SceneUnitData to retrieve further information on the Unit. 
    /// However, you can use the StationIndex to consult another class to retrieve the Station's Location in worldSpace.
    /// </summary>
    public static event Action<UnitIndex, StationIndex?, BaseBattleUnit> OnRemoveUnit;

    /// <summary>
    /// Invoked on Switching the stations of Units on the Same Team. 
    /// 
    /// Index 1: UnitIndex that is switching to the desired station. 
    /// Index 2: UnitIndex that is being forced to the other Unit's Station.
    /// </summary>
    public static event Action<UnitIndex, UnitIndex> OnSwitchUnit;

    /// <summary>
    /// Invoked on Deploying from Resurves. 
    /// 
    /// UnitIndex 1: UnitIndex we are deploying. 
    /// UnitIndex 2: UnitIndex of the unit that is going to resurves.
    /// </summary>
    public static event Action<UnitIndex, UnitIndex> OnDeployUnit;


    public SceneUnitData(int allyStationSlots, int enemyStationSlots)
    {
        if(allyStationSlots > MAX_UNITS_PER_SIDE) { throw new InvalidOperationException("ERROR — SCENEUNITDATA_SOA: CANNOT ASSIGN 'ALLY STATION SLOTS' TO A NUMBER GREATER THAN 'MAX_UNITS_PER_SIDE'"); }
        if (enemyStationSlots > MAX_UNITS_PER_SIDE) { throw new InvalidOperationException("ERROR — SCENEUNITDATA_SOA: CANNOT ASSIGN 'ENEMY STATION SLOTS' TO A NUMBER GREATER THAN 'MAX_UNITS_PER_SIDE'"); }

        this.data = new(new BaseBattleUnit[MAX_UNITS_PER_SIDE * 2], new UnitTeam[MAX_UNITS_PER_SIDE * 2], new StationIndex?[MAX_UNITS_PER_SIDE * 2], allyStationSlots, enemyStationSlots);
    }

    private int GetTeamCount(UnitTeam sampleTeam)
    {
        UnitTeam[] sampleTeamArr = this.data.Teams.Where(unitTeam => unitTeam == sampleTeam).ToArray();
        return sampleTeamArr.Length;
    }

    public bool AddUnit(BaseBattleUnit baseBattleUnit, UnitTeam unitTeam, StationIndex? station)
    {
        if(baseBattleUnit == null) { return false; }

        /*  Check if there is enough slots on the Team for the new Unit. If not, get out of here.   */
        if (GetTeamCount(unitTeam) >= MAX_UNITS_PER_SIDE) { return false; }

        for(int i = 0; i < (MAX_UNITS_PER_SIDE * 2); i++)
        {
            /*  If there is an empty Unit in this index, assign everything to this index.   */
            if (this.data.Units[i] == null)
            {
                this.data.Units[i] = baseBattleUnit;
                this.data.Teams[i] = unitTeam;
                this.data.Stations[i] = station;

                if (station != null)
                {
                    OnAddUnit?.Invoke(new UnitIndex() { Index = station.Value.Index});
                }

                return true;
            }
        }

        /*  If there wasn't an available slot available somehow, return false.*/
        return false;
    }

    public bool RemoveUnit(UnitIndex unitIndex)
    {
        /*  Check that there is a valid Unit and it is on an appropriate team.  */
        if (this.data.Units[unitIndex.Index]    == null) { return false; }
        if (this.data.Teams[unitIndex.Index]    == UnitTeam.NULL) { return false; }
        
        /*  Remove the Unit from the Teams Array.   */
        this.data.Teams[unitIndex.Index] = UnitTeam.NULL;

        /*  Remove the Unit from the Units Array but store temporary referance to the BattleUnit for the event.   */
        BaseBattleUnit battleUnit = this.data.Units[unitIndex.Index];
        this.data.Units[unitIndex.Index] = null;

        /*  Remove the Unit from the Stations Array but store temporary referance to the Station for the event.   */
        StationIndex? stationIndex = this.data.Stations[unitIndex.Index];
        this.data.Stations[unitIndex.Index] = null;

        OnRemoveUnit?.Invoke(unitIndex, stationIndex, battleUnit);

        return false;
    }

    /// <summary>
    /// Switch one Unit's Station with one on it's own team. If the indexes are not on the same team, this will fail.
    /// </summary>
    /// <param name="unitIndexA"></param>
    /// <param name="unitIndexB"></param>
    /// <returns></returns>
    public bool SwitchUnitStations(UnitIndex unitIndexA, UnitIndex unitIndexB)
    {
        /*  Confirm both Units are on the same team.    */
        UnitTeam teamUnitA = this.data.Teams[unitIndexA.Index];
        UnitTeam teamUnitB = this.data.Teams[unitIndexB.Index];
        
        if(teamUnitA != teamUnitB) {  return false; }

        /*  Get the station of each Unit, assign each station to the other Unit's station.  */
        StationIndex? stationUnitA = this.data.Stations[unitIndexA.Index];
        StationIndex? stationUnitB = this.data.Stations[unitIndexB.Index];

        this.data.Stations[unitIndexA.Index] = stationUnitB;
        this.data.Stations[unitIndexB.Index] = stationUnitA;

        OnSwitchUnit?.Invoke(unitIndexA, unitIndexB);

        return true;
    }

    /// <summary>
    /// Deploy a Unit from resurves. Will fail if the Unit is already on the field and if the Station Index doesn't belong to the Deployed Unit's Team.
    /// </summary>
    /// <param name="unitIndex"></param>
    /// <param name="stationIndex"></param>
    /// <returns></returns>
    public bool DeployUnit(UnitIndex unitIndex, StationIndex stationIndex)
    {
        /*  Get the current station of the Unit, if it is not NULL, then they are already on the field and therefore do not deployment. */
        StationIndex? deployedUnitStation = this.data.Stations[unitIndex.Index];
        if (deployedUnitStation?.Index != unitIndex.Index) { return false; }


        /*  Check that the Unit Index is on the same team as the UnitIndex found using the Station Index. If not, fail deployment.  */
        UnitIndex switchingUnitIndex = GetUnitIndexOfStationIndex(stationIndex);
        if(switchingUnitIndex.Index == -1) { return false; }


        UnitTeam teamDeployUnit = this.data.Teams[unitIndex.Index];
        UnitTeam teamSwitchUnit = this.data.Teams[switchingUnitIndex.Index];
        if (teamDeployUnit != teamSwitchUnit) { return false; }


        /*  Assign each Unit to the other's Station.    */
        StationIndex? stationUnitA = this.data.Stations[unitIndex.Index];
        StationIndex? stationUnitB = this.data.Stations[switchingUnitIndex.Index];


        this.data.Stations[unitIndex.Index] = stationUnitB;
        this.data.Stations[switchingUnitIndex.Index] = stationUnitA;

        OnDeployUnit?.Invoke(unitIndex, switchingUnitIndex);

        return true;
    }

    public UnitIndex[] GetUnitIndexesOfTeam(UnitTeam team)
    {
        List<UnitIndex> indexes = new();
        for (int i = 0; i < this.data.Teams.Length; i++)
        {
            if(this.data.Teams[i] == team)
            {
                indexes.Add(new UnitIndex() { Index = i});
            }
        }

        return indexes.ToArray();
    }

    public UnitIndex GetUnitIndexOfStationIndex(StationIndex? stationIndex)
    {
        UnitIndex unitIndex;
        for (int i = 0; i < this.data.Stations.Length; i++)
        {
            if (this.data.Stations[i]?.Index == stationIndex?.Index)
            {
                unitIndex = new()
                {
                    Index = i
                };
                return unitIndex;
            }
        }

        unitIndex = new()
        {
            Index = -1
        };
        return unitIndex;
    }

    public UnitIndex[] GetActiveUnitsOfTeam(UnitTeam team)
    {
        List<UnitIndex> indexes = new();
        UnitIndex[] unitIndexes = GetUnitIndexesOfTeam(team);

        for (int i = 0;i < unitIndexes.Length; i++)
        {
            /*  Get the Station Index of this UnitIndex. If it isn't NULL, it is on the field and so add it to our Indexes List.    */
            if (this.data.Stations[unitIndexes[i].Index] != null)
            {
                indexes.Add(unitIndexes[i]);
            }
        }
        return indexes.ToArray();
    }

    /*  
     *  We will need to look into this one eventually. As we return the BaseBattleUnit class, we can change the information on the fly which is not advised. 
     *  Eventually, we'd want to make it so only certain code places can actually change the BaseBattleUnit. 
    */
    public BaseBattleUnit GetBattleUnitOfIndex(UnitIndex index) => this.data.Units[index.Index];
    public UnitTeam GetUnitTeamOfIndex(UnitIndex index) => this.data.Teams[index.Index];
    public StationIndex? GetUnitStationOfIndex(UnitIndex index) => this.data.Stations[index.Index];
    public StationIndex?[] GetStations() => this.data.Stations;

    public int GetAllyStationSlots() {  return this.data.AllyStationSlots;}
    public int GetEnemyStationSlots() {  return this.data.EnemyStationSlots; }
}

public struct SceneData_UnitTurn
{
    public UnitIndex SourceUnitIndex;
    public List<StationIndex?> AllyStationIndexes;
    public List<StationIndex?> EnemyStationIndexes;

    public SceneData_UnitTurn(UnitIndex source, List<StationIndex?> allyStationIndexes, List<StationIndex?> enemyStationIndexes)
    {
        this.SourceUnitIndex = source;
        this.AllyStationIndexes = allyStationIndexes;
        this.EnemyStationIndexes = enemyStationIndexes;
    }
}

public class StationHandler
{
    private readonly SceneUnitData SceneUnitData;

    public StationHandler(int allyStationSlots, int enemyStationSlots)
    {
        this.SceneUnitData = new(allyStationSlots, enemyStationSlots);
    }

    public List<UnitIndex> GetAllActiveUnits()
    {
        UnitIndex[] allyUnitIndexes = this.SceneUnitData.GetActiveUnitsOfTeam(UnitTeam.ALLY);
        UnitIndex[] enemyUnitIndexes = this.SceneUnitData.GetActiveUnitsOfTeam(UnitTeam.ENEMY);

        List<UnitIndex> activeUnits = new();
        activeUnits.AddRange(allyUnitIndexes);
        activeUnits.AddRange(enemyUnitIndexes);

        return activeUnits;
    }

    public bool AddUnit(BaseBattleUnit unit, UnitTeam team, StationIndex? station)
    {
        return SceneUnitData.AddUnit(unit, team, station);
    }
    public bool RemoveUnit(UnitIndex unitIndex)
    {
        return SceneUnitData.RemoveUnit(unitIndex);
    }
    public bool SwitchUnitStations(UnitIndex unitIndexA, UnitIndex unitIndexB)
    {
        return SceneUnitData.SwitchUnitStations(unitIndexA, unitIndexB);
    }
    public bool DeployUnit(UnitIndex unitIndex, StationIndex stationIndex)
    {
        return SceneUnitData.DeployUnit(unitIndex, stationIndex);
    }

    private UnitTeam GetOppositeTeamType(UnitTeam team)
    {
        switch (team)
        {
            case UnitTeam.ALLY:
                return UnitTeam.ENEMY;
            case UnitTeam.ENEMY:
                return UnitTeam.ALLY;
            default:
                return UnitTeam.ALLY;

        }
    }

    private UnitIndex[] GetUnitIndexCollectionRemovingSourceIndex(UnitIndex[] unitIndexes, UnitIndex sourceIndex)
    {
        UnitIndex[] indexCollection = new UnitIndex[unitIndexes.Length];
        for (int i = 0; i < unitIndexes.Length; i++)
        {
            if (unitIndexes[i].Index != sourceIndex.Index)
            {
                indexCollection[i] = new UnitIndex() { Index = i };
            }
        }
        return indexCollection;
    }

    public List<StationIndex?> GetStationIndexesFromUnitIndexes(UnitIndex[] indexes)
    {
        List<StationIndex?> stationIndexes = new();
        for (int i = 0; i < indexes.Length; i++)
        {
            stationIndexes.Add(this.SceneUnitData.GetUnitStationOfIndex(indexes[i]));
        }

        return stationIndexes;
    }

    public SceneData_UnitTurn CreateCombatSceneDataForUnitIndex(UnitIndex sourceUnitIndex)
    {
        /*  Get a list of all UnitIndexes on the opposite team. */
        UnitTeam oppositeTeam = GetOppositeTeamType(this.SceneUnitData.GetUnitTeamOfIndex(sourceUnitIndex));

        /*  Get a list of all UnitIndexes that are on the team. */
        UnitIndex[] allIUnitIndexesOnAlliedTeam = this.SceneUnitData.GetUnitIndexesOfTeam(this.SceneUnitData.GetUnitTeamOfIndex(sourceUnitIndex));

        UnitIndex[] unitIndexesOfOppositeTeam = this.SceneUnitData.GetUnitIndexesOfTeam(oppositeTeam);

        /*  Confirm that the sourceUnitIndex is contained within UnitIndexesOnTeam. If not, something broke.    */
        if (!allIUnitIndexesOnAlliedTeam.Contains(sourceUnitIndex)) { throw new InvalidOperationException("ERROR — STATION_HANDLER: SOURCE UNIT INDEX NOT PRESENT INSIDE TEAMED INDEXES!"); }

        /*  Confirm that we have a teamedIndex array of length greater than 0. If it is 0, we don't want to proceed.    */
        if (allIUnitIndexesOnAlliedTeam.Length <= 0) { throw new InvalidOperationException("ERROR — STATION_HANDLER: TEAMED UNIT INDEX LIST IS LESS THAN 0!"); }

        /*  Remove the source Unit Index from the TeamedIndexes Array.  */
        UnitIndex[] TeamedIndexes = GetUnitIndexCollectionRemovingSourceIndex(allIUnitIndexesOnAlliedTeam, sourceUnitIndex);

        /*  Convert UnitIndex of both allied and enemy lists to the StationIndex of that Unit. */
        List<StationIndex?> allyStationIndexes = GetStationIndexesFromUnitIndexes(TeamedIndexes);
        List<StationIndex?> enemyStationIndexes = GetStationIndexesFromUnitIndexes(unitIndexesOfOppositeTeam);

        // TO DO: PASS IN ENVIRONMENT DATA
        return new SceneData_UnitTurn(sourceUnitIndex, allyStationIndexes, enemyStationIndexes);
    }

    public UnitIndex? GetUnitIndexOnStation(StationIndex stationIndex) { return this.SceneUnitData.GetUnitIndexOfStationIndex(stationIndex); }
    public UnitIndex[] GetUnitIndexesOfTeam(UnitTeam team) { return this.SceneUnitData.GetUnitIndexesOfTeam(team); }
    public BaseBattleUnit GetBattleUnitOfIndex(UnitIndex index) {   return this.SceneUnitData.GetBattleUnitOfIndex(index); }
    public UnitTeam GetUnitTeamOfIndex(UnitIndex index) {   return this.SceneUnitData.GetUnitTeamOfIndex(index); }

    public StationIndex? GetStationOfIndex(UnitIndex index) { return this.SceneUnitData.GetUnitStationOfIndex(index); }
    public StationIndex?[] GetStations() => this.SceneUnitData.GetStations();

    public int GetAllyStationSlots() { return this.SceneUnitData.GetAllyStationSlots(); }
    public int GetEnemyStationSlots() { return this.SceneUnitData.GetEnemyStationSlots(); }
}
