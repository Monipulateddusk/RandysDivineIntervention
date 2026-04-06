using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;

public struct UnitIndex     
{  
    public int Index;   
}

[Serializable]public struct StationIndex
{
    public int Index;
}

public class Station
{
    public StationIndex         StationIndex;       
    public UnitIndex?           UnitOnStation;
    public UnitTeam             StationTeam;    
    public UnityEngine.Vector3  Position;
    public bool                 IsTemporary;
}

public class SceneUnitData
{
    public Dictionary<int,  BaseBattleUnit> Units = new();           
    public Dictionary<int,  Station>        Stations = new();

    public static int MAX_UNITS_PER_SIDE = 9;
    private int nextUnitId = 0, nextStationId = 0;

    public SceneUnitData()
    {
        this.nextUnitId = 0;
        this.Units      = new();
        this.Stations   = new();
    }

    /// <summary>
    /// There will be two versions of this method. 1st is, do we know the Unit that will be on this station? I.e. will there be a Unit on this station on Station Creation. Sometimes no, sometimes yes.    
    /// If the game first is loading, then yes, we will supply a Unit that will sit on this station.
    /// </summary>
    /// <returns></returns>
    public StationIndex? CreateStation(UnityEngine.Vector3 stationPosition, UnitTeam stationTeam, bool isTemp)
    {
        /*  Only add the station if it is not a duplicate station position. */
        if (IsDuplicateStationPosition(stationPosition)) { return null;}

        int stationId = this.nextStationId;
        StationIndex createdStationIndex = new() { Index = stationId };
        this.Stations.Add(stationId,
            new()
            {
                StationIndex = createdStationIndex,
                UnitOnStation = null,
                Position = stationPosition,
                IsTemporary = isTemp,
                StationTeam = stationTeam,
            }
        );

        IncrementStationID();
        return createdStationIndex;
    }

    public bool RemoveStation(StationIndex stationIndex)
    {
        if (this.Stations.ContainsKey(stationIndex.Index))
        {
            this.Stations.Remove(stationIndex.Index);
            return true;
        }
        return false;
    }

    private bool IsDuplicateStationPosition(UnityEngine.Vector3 stationPosition)
    {
        foreach(KeyValuePair<int, Station> stationKeyValuePair in this.Stations)
        {
            Station station = stationKeyValuePair.Value;    

            if(station.Position == stationPosition)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// When adding a Unit to the combat Scene. We aren't defining station here. All units start in resurve until placed.
    /// </summary>
    /// <param name="baseBattleUnit"></param>
    /// <param name="unitTeam"></param>
    /// <param name="station"></param>
    /// <returns>True if the Unit was sucessfully added to the combat Scene.    </returns>
    public UnitIndex? CreateUnit(BaseBattleUnit baseBattleUnit)
    {
        /*  Is this a valid Unit? If not, fail creation.    */
        if(baseBattleUnit == null) { return null; }

        /*  Find the next index to use for the Dictionary.  */
        int unitId = this.nextUnitId;
        this.IncrementUnitID();

        /*  Add this Unit to the Dictionary. */
        this.Units.Add(unitId, baseBattleUnit);

        /*  Once Sucessful, return the UnitIndex and notify any listeners to the OnAddUnitEvent.    */
        UnitIndex createdUnitIndex = new() { Index = unitId };

        return createdUnitIndex;
    }

    /// <summary>
    /// Swaps one unit on the battlefield with another unit on the battlefield on the same team.
    /// </summary>
    /// <param name="stationIndexA"></param>
    /// <param name="stationIndexB"></param>
    /// <returns>True if the swap with sucessful. Additionally, OnSwapUnits will be Invoked on sucessful switch, whereas OnFailSwapUnits is invoked if we fail. </returns>
    public bool SwapUnitsOnStations(StationIndex stationIndexA, StationIndex stationIndexB)
    {
        /*  Check to see if these are valid station indexes!    */
        if (!this.IsStationIndexValid(stationIndexA)) {  return false; }
        if (!this.IsStationIndexValid(stationIndexB)) {  return false; }

        /*  Are the selected stations on the same team?     */
        if (this.Stations[stationIndexA.Index].StationTeam != this.Stations[stationIndexB.Index].StationTeam) { return false; }

        /*  Get the stations and swap their Units on them.  */
        UnitIndex? unitOnStationA = this.Stations[stationIndexA.Index].UnitOnStation;
        UnitIndex? unitOnStationB = this.Stations[stationIndexB.Index].UnitOnStation;

        // If either Station don't have a Unit on them. Then we want to notify anyone subscribed to the event and not switch.
        if(!unitOnStationA.HasValue || !unitOnStationB.HasValue)
        {
            return false;
        }

        /*  Swap the Units to each other's Stations.    */
        this.Stations[stationIndexA.Index].UnitOnStation = unitOnStationB;
        this.Stations[stationIndexB.Index].UnitOnStation = unitOnStationA;

        return true;
    }

    public bool RemoveUnit(UnitIndex unitIndex)
    {
        /*  Find if this UnitIndex is related to a Unit we have information on. If not, exit out.   */
        if (!this.Units.ContainsKey(unitIndex.Index)) { return false;   }

        /*  Before we remove, gain a referance to the BaseBattleUnit we are destroying to pass to the event.    */
        BaseBattleUnit removedUnit = this.Units[unitIndex.Index];

        /*  Remove that Unit. Notify any Listeners. How do we want to handle notifying the station? Do we simply loop through every station, find the one we are on (if at all) and pass that along to the event?   */
        this.Units.Remove(unitIndex.Index);

        return true;
    }

    /// <summary>
    /// Deploy a Unit from resurves.
    /// </summary>
    /// <param name="unitIndex"></param>
    /// <param name="stationIndex"></param>
    /// <returns></returns>
    public bool DeployUnit(UnitIndex unitIndex, StationIndex stationIndex)
    {
        /*  Is the Unit already on the battlefield? I.e. Does a station already have that same UnitIndex supplied?  */
        if (IsUnitIndexOnBattlefield(unitIndex)) { return false;}

        /*  Is this a valid station ID? If not, stop!   */
        if (!IsStationIndexValid(stationIndex)) { return false; }


        /*  Is the Unit Index we are deploying onto this station on the same team as the station? 
         *  I.e. The Unit and the Station need to be on the Ally team in order for this to work. We can't have an enemy on an ally's Station
         */
        if (this.Stations[stationIndex.Index].StationTeam != this.Units[unitIndex.Index].GetTeam()) {  return false; }

        // Is there a Unit already on that StationIndex? If so, we want to recall that Unit to resurves
        UnitIndex? unitPreviouslyOnStation = this.Stations[stationIndex.Index].UnitOnStation;

        // Replace this previous Unit with the one we want to Deploy.          
        this.Stations[stationIndex.Index].UnitOnStation = unitIndex;

        return true;
    }

    public UnitIndex[] GetUnitsInReserve(UnitTeam team)
    {
        List<UnitIndex> reserveUnitIndexes = new();
        foreach (KeyValuePair<int, BaseBattleUnit> unitKeyValuePair in this.Units)
        {
            UnitIndex unitIndex = new() { Index = unitKeyValuePair.Key };

            // Only add the UnitIndexes if they aren't on the battlefield and on the same team as the one we request.
            if (!IsUnitIndexOnBattlefield(unitIndex) && unitKeyValuePair.Value.GetTeam() == team)
            {
                reserveUnitIndexes.Add(unitIndex);
            }
        }
        return reserveUnitIndexes.ToArray();
    }

    public List<UnitIndex> GetUnitIndexesOfTeam(UnitTeam team)
    {
        // Loop through our Units. Look into them and determine which team they are on. Add those into a list and return the completed list.    
        List<UnitIndex> unitIndexesOnTeam = new();
        foreach (KeyValuePair<int, BaseBattleUnit> unitKeyValuePair in this.Units)
        {
            BaseBattleUnit battleUnit = unitKeyValuePair.Value;

            if (battleUnit.GetTeam() == team)
            {
                unitIndexesOnTeam.Add(new UnitIndex() { Index = unitKeyValuePair.Key });
            }
        }

        return unitIndexesOnTeam;
    }

    public UnitIndex? GetUnitIndexOfStationIndex(StationIndex stationIndex)
    {
        /*  Is this a valid station ID? If not, stop!   */
        if (!IsStationIndexValid(stationIndex)) { return null; }

        return this.Stations[stationIndex.Index].UnitOnStation;
    }

    public StationIndex? GetStationIndexOfUnitIndex(UnitIndex unitIndex)
    {
        foreach (KeyValuePair<int,Station> stationKeyValuePairs in this.Stations)
        {
            Station station = stationKeyValuePairs.Value;
            if (!station.UnitOnStation.HasValue) { continue; }
            if(station.UnitOnStation.Value.Index != unitIndex.Index)
            {
                return station.StationIndex;
            }
        }
        return null;
    }

    /// <summary>
    /// Get a collection of all Units on a team both on the field and not.
    /// </summary>
    /// <param name="team"></param>
    /// <returns></returns>
    public List<UnitIndex> GetUnitsOnTeam(UnitTeam team)
    {
        List<UnitIndex> unitIndexesOnTeam = new();
        foreach (KeyValuePair<int, BaseBattleUnit> unitsKeyValuePair in this.Units)
        {
            BaseBattleUnit battleUnit = unitsKeyValuePair.Value;

            if(battleUnit.GetTeam() == team)
            {
                unitIndexesOnTeam.Add(new UnitIndex() { Index = unitsKeyValuePair.Key });
            }
        }

        return unitIndexesOnTeam;
    }

    public List<UnitIndex> GetAllActiveUnits()
    {
        // Loop through our stations. All the ones without a NULL UnitOnStation have a Unit on them. 
        List<UnitIndex> activeUnits = new();
        if (this.Stations.Count < 0) { return new(); }
        foreach(KeyValuePair<int, Station> stationKeyValuePairs in this.Stations)
        {
            Station station = stationKeyValuePairs.Value;

            if (station.UnitOnStation.HasValue)
            {
                activeUnits.Add(station.UnitOnStation.Value);
            }
        }
        return activeUnits;
    }

    /// <summary>
    /// Gets all active units on the defined Team
    /// </summary>
    /// <param name="team"></param>
    /// <returns></returns>
    public List<UnitIndex> GetAllActiveUnits(UnitTeam team)
    {
        // Loop through our stations. All the ones without a NULL UnitOnStation have a Unit on them. 
        List<UnitIndex> activeUnits = new();
        foreach (KeyValuePair<int, Station> stationKeyValuePairs in this.Stations)
        {
            Station station = stationKeyValuePairs.Value;

            if (station.UnitOnStation.HasValue && station.StationTeam == team)
            {
                activeUnits.Add(station.UnitOnStation.Value);
            }
        }
        return activeUnits;
    }

    public List<StationIndex> GetStationIndexes()
    {
        List<StationIndex> stationIndexes = new();
        foreach (KeyValuePair<int, Station> stationKeyValuePairs in this.Stations)
        {
            int stationIndexInt = stationKeyValuePairs.Key;
            stationIndexes.Add(
                new StationIndex (){ Index = stationIndexInt }
            );
        }
        return stationIndexes;
    }
    public UnitTeam GetUnitTeamOfUnitIndex(UnitIndex unitIndex)
    {
        BaseBattleUnit unit = this.GetBattleUnitOfIndex(unitIndex);
        return unit.GetTeam();
    }

    private bool IsUnitIndexOnBattlefield(UnitIndex unitIndex)
    {
        foreach (KeyValuePair<int, Station> stationsKeyValuePair in this.Stations)
        {
            Station station = stationsKeyValuePair.Value;
            if (!DoesStationHaveUnit(station)) { continue; }

            if (station.UnitOnStation.Value.Index == unitIndex.Index)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Station Indexes need to be a value between 0 and whatever the current size of the Stations list is. Ordinarally, between 0-17 but temporary stations exist. 
    /// </summary>
    /// <returns></returns>
    private bool IsStationIndexValid(StationIndex stationIndex)
    {
        // Ensure that the station index is less than the size of the stations count and not less than 0
        return stationIndex.Index >= 0 && stationIndex.Index < this.Stations.Count;
    }

    private bool DoesStationHaveUnit(StationIndex stationIndex) => this.Stations[stationIndex.Index].UnitOnStation.HasValue;
    private bool DoesStationHaveUnit(Station station) => station.UnitOnStation.HasValue;
    private void IncrementUnitID() { this.nextUnitId++; }
    private void IncrementStationID() { this.nextStationId++; }

    /*  
     *  We will need to look into this one eventually. As we return the BaseBattleUnit class, we can change the information on the fly which is not advised. 
     *  Eventually, we'd want to make it so only certain code places can actually change the BaseBattleUnit. 
    */
    public BaseBattleUnit GetBattleUnitOfIndex(UnitIndex index) => this.Units[index.Index];
    public Station GetStationOfStationIndex(StationIndex stationIndex)
    {
        if (this.Stations.TryGetValue(stationIndex.Index, out Station station))
        {
            return station;
        }
        return null;
    }
}

public class StationManager
{
    #region Events
    /// <summary>
    /// Invoked when a Unit is added. Normally at start of game.    
    /// UnitIndex: Unit Index of the Unit being added.  
    /// </summary>
    public static event Action<UnitIndex> OnAddUnit;

    /// <summary>
    /// Invoked when a Unit is Removed. When a unit is destroyed.   
    /// Station Index: StationIndex of the Destroyed Unit. Could be null if they were removed in resurve.
    /// BaseBattleUnit: Main Script of the Unit, allows use of the GameObject.
    /// 
    /// IMPORTANT: As we remove the UnitIndex, StationIndex from arrays, you cannot use any method within SceneUnitData to retrieve further information on the Unit. 
    /// However, you can use the StationIndex to consult another class to retrieve the Station's Location in worldSpace.
    /// </summary>
    public static event Action<StationIndex?, BaseBattleUnit> OnRemoveUnit;

    /// <summary>
    /// Invoked on Switching the stations of Units on the Same Team. 
    /// 
    /// Index 1: UnitIndex that is switching to the desired station. 
    /// Index 2: UnitIndex that is being forced to the other Unit's Station.
    /// </summary>
    public static event Action<UnitIndex?, UnitIndex?> OnSwapUnits;


    /// <summary>
    /// Invoked on failing Switching the stations of Units on the Same Team. 
    /// 
    /// StationIndex 1: StationIndex that would be switching to the desired station. 
    /// StationIndex 2: StationIndex that would be forced to the other Unit's Station.
    /// UnitIndex 1: UnitIndex that would be switching to the desired station. 
    /// UnitIndex 2: UnitIndex that would be forced to the other Unit's Station.
    /// </summary>
    public static event Action<StationIndex, StationIndex, UnitIndex, UnitIndex> OnFailSwapUnits;

    /// <summary>
    /// Invoked on Deploying from Resurves. 
    /// 
    /// UnitIndex 1: UnitIndex we are deploying. 
    /// UnitIndex 2: UnitIndex of the unit that is going to resurves. Null if there was nothing on that Deployment station.
    /// </summary>
    public static event Action<UnitIndex, UnitIndex?> OnDeployUnit;

    #endregion

    private readonly SceneUnitData SceneUnitData;

    readonly List<UnityEngine.Vector3> ALLY_STATION_LOCATIONS = new(){
        new(0,      0,  1),         new(-3,         0,      1),         new(3,      0,      1),
        new(1.5f,   0,  3),         new(-1.5f,      0,      3),         new(4.5f,   0,      3),
        new(1.5f,   0, -1),         new(-1.5f,      0,     -1),         new(4.5f,   0,     -1),
    };
    readonly List<UnityEngine.Vector3> ENEMY_STATION_LOCATIONS = new(){
        new(1.5f,   0, -6),         new(-1.5f,      0,     -6),         new(4.5f,   0,     -6),
        new(0,      0, -8),         new(-3,         0,     -8),         new(3,      0,     -8),
        new(0,      0, -4),         new(-3,         0,     -4),         new(3,      0,     -4),
    };
    private const int MAX_STATIONS_PER_SIDE = 9;

    public StationManager()
    {
        this.SceneUnitData = new();
        CreateStartingStations();
    }

    public List<UnitIndex>      GetAllActiveUnits()                                                 => this.SceneUnitData.GetAllActiveUnits();
    public List<UnitIndex>      GetUnitsOnTeam          (UnitTeam team)                             => this.SceneUnitData.GetUnitsOnTeam(team);
    public List<UnitIndex>      GetUnitIndexesOfTeam    (UnitTeam team)                             => this.SceneUnitData.GetUnitIndexesOfTeam(team);
    public List<StationIndex>   GetStationsIndex()                                                  => this.SceneUnitData.GetStationIndexes();
    public UnitIndex?           GetUnitIndexOnStation   (StationIndex stationIndex)                 => this.SceneUnitData.GetUnitIndexOfStationIndex(stationIndex);
    public StationIndex?        GetStationOfIndex       (UnitIndex index)                           => this.SceneUnitData.GetStationIndexOfUnitIndex(index); 
    public BaseBattleUnit       GetBattleUnitOfIndex    (UnitIndex index)                           => this.SceneUnitData.GetBattleUnitOfIndex(index);
    public Station              GetStationOfStationIndex(StationIndex stationIndex)                 => this.SceneUnitData.GetStationOfStationIndex(stationIndex);
    public UnitTeam             GetUnitTeamOfIndex      (UnitIndex index)                           => this.SceneUnitData.GetUnitTeamOfUnitIndex(index); 
    public bool                 IsStationEmpty          (StationIndex stationIndex)                 => !this.SceneUnitData.GetUnitIndexOfStationIndex(stationIndex).HasValue;



    public UnitIndex? CreateUnit(BaseBattleUnit unit)
    {
        UnitIndex? createdUnitIndex = this.SceneUnitData.CreateUnit(unit);
        if (createdUnitIndex.HasValue)
        {
            OnAddUnit?.Invoke(createdUnitIndex.Value);
            return createdUnitIndex;
        }
        else
        {
            return null;
        }
    }
    public bool DeployUnit(UnitIndex unitIndex, StationIndex stationIndex)
    {
        Station station     = this.SceneUnitData.GetStationOfStationIndex(stationIndex);
        BaseBattleUnit unit = this.SceneUnitData.GetBattleUnitOfIndex(unitIndex);

        /*  When deploying a unit, check that the baseBattleUnit and the station match Teams.   */
        if(unit.GetTeam() != station.StationTeam) { return false; }

        /*  If so, send the Unit on the Station to resurve and replace it with the new Unit.    */
        UnitIndex? previousUnitIndex = station.UnitOnStation;
        station.UnitOnStation = unitIndex;  

        /*  Notify any listeners that the deployment took place.    */
        OnDeployUnit?.Invoke(unitIndex, previousUnitIndex);

        return true;
    }
    public bool RemoveUnit(UnitIndex unitIndex)
    {
        StationIndex? removedUnitStationIndex   = this.SceneUnitData.GetStationIndexOfUnitIndex(unitIndex);
        BaseBattleUnit removedUnit              = this.SceneUnitData.GetBattleUnitOfIndex(unitIndex); 

        bool sucess = this.SceneUnitData.RemoveUnit(unitIndex);
        if (sucess) 
        {
            OnRemoveUnit?.Invoke(removedUnitStationIndex, removedUnit);
            return true;
        }
        else
        {
            return false;
        }
    }
    public bool SwitchUnitStations(UnitIndex unitIndexA, UnitIndex unitIndexB)
    {   
        /*  Get each of the stations of the selected Units. */
        StationIndex? stationIndexA = this.SceneUnitData.GetStationIndexOfUnitIndex(unitIndexA);
        StationIndex? stationIndexB = this.SceneUnitData.GetStationIndexOfUnitIndex(unitIndexB);

        /*  If either of the stations is invalid, then we don't even want to notify that we failed swapping as one of the stations is invalid.  */
        if (stationIndexA == null || stationIndexB == null) {   return false;   }

        bool sucess = this.SceneUnitData.SwapUnitsOnStations(stationIndexA.Value, stationIndexB.Value);

        if (sucess) 
        {
            OnSwapUnits?.Invoke(unitIndexA, unitIndexB);
            return true;
        }
        else
        {
            OnFailSwapUnits?.Invoke(stationIndexA.Value, stationIndexB.Value, unitIndexA, unitIndexB);
            return false;
        }
    }
    public bool SwitchUnitStations(StationIndex stationIndexA, StationIndex stationIndexB)
    {
        /*  Get each of the stations of the selected Units. */
        UnitIndex? unitIndexA = this.SceneUnitData.GetUnitIndexOfStationIndex(stationIndexA);
        UnitIndex? unitIndexB = this.SceneUnitData.GetUnitIndexOfStationIndex(stationIndexB);

        /*  If either of the stations is invalid, then we don't even want to notify that we failed swapping as one of the stations is invalid.  */
        if (unitIndexA == null || unitIndexA == null) { return false; }

        bool sucess = this.SceneUnitData.SwapUnitsOnStations(stationIndexA, stationIndexB);

        if (sucess)
        {
            OnSwapUnits?.Invoke(unitIndexA, unitIndexB);
            return true;
        }
        else
        {
            OnFailSwapUnits?.Invoke(stationIndexA, stationIndexB, unitIndexA.Value, unitIndexB.Value);
            return false;
        }
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

    private List<UnitIndex> GetUnitIndexCollectionRemovingSourceIndex(List<UnitIndex> unitIndexes, UnitIndex sourceIndex)
    {
        List<UnitIndex> indexCollection = new();
        foreach (UnitIndex unitIndex in unitIndexes)
        {
            if(unitIndex.Index != sourceIndex.Index)
            {
                indexCollection.Add(unitIndex);
            }
        }
        return indexCollection;
    }

    public List<StationIndex?> GetStationIndexesFromUnitIndexes(List<UnitIndex> indexes)
    {
        List<StationIndex?> stationIndexes = new();
        foreach (UnitIndex unitIndex in indexes)
        {
            stationIndexes.Add(this.SceneUnitData.GetStationIndexOfUnitIndex(unitIndex));

        }
        return stationIndexes;
    }

    public SceneData_UnitTurn CreateCombatSceneDataForUnitIndex(UnitIndex sourceUnitIndex)
    {
        /*  Get a list of all UnitIndexes on the opposite team. */
        UnitTeam sourceTeam = this.SceneUnitData.GetUnitTeamOfUnitIndex(sourceUnitIndex);
        UnitTeam oppositeTeam = GetOppositeTeamType(this.SceneUnitData.GetUnitTeamOfUnitIndex(sourceUnitIndex));

        /*  Get a list of all UnitIndexes that are on the team. */
        List<UnitIndex> allIUnitIndexesOnAlliedTeam = this.SceneUnitData.GetUnitIndexesOfTeam(sourceTeam);
        List<UnitIndex> unitIndexesOfOppositeTeam   = this.SceneUnitData.GetUnitIndexesOfTeam(oppositeTeam);

        /*  Confirm that the sourceUnitIndex is contained within UnitIndexesOnTeam. If not, something broke.    */
        if (!allIUnitIndexesOnAlliedTeam.Contains(sourceUnitIndex)) { throw new InvalidOperationException("ERROR — STATION_HANDLER: SOURCE UNIT INDEX NOT PRESENT INSIDE TEAMED INDEXES!"); }

        /*  Confirm that we have a teamedIndex array of length greater than 0. If it is 0, we don't want to proceed.    */
        if (allIUnitIndexesOnAlliedTeam.Count <= 0) { throw new InvalidOperationException("ERROR — STATION_HANDLER: TEAMED UNIT INDEX LIST IS LESS THAN 0!"); }

        /*  Remove the source Unit Index from the TeamedIndexes Array.  */
        List<UnitIndex> TeamedIndexes = GetUnitIndexCollectionRemovingSourceIndex(allIUnitIndexesOnAlliedTeam, sourceUnitIndex);

        /*  Convert UnitIndex of both allied and enemy lists to the StationIndex of that Unit. */
        List<StationIndex?> allyStationIndexes = GetStationIndexesFromUnitIndexes(TeamedIndexes);
        List<StationIndex?> enemyStationIndexes = GetStationIndexesFromUnitIndexes(unitIndexesOfOppositeTeam);

        // TO DO: PASS IN ENVIRONMENT DATA
        return new SceneData_UnitTurn(sourceUnitIndex, allyStationIndexes, enemyStationIndexes);
    }
    
    /// <summary>
    /// Called on Construction of the StationHandler. Will create each Station with corresponding index.    
    /// </summary>
    private void CreateStartingStations()
    {
        /*  Do Ally Stations turns.     */
        for (int i = 0; i < MAX_STATIONS_PER_SIDE; i++)
        {
            StationIndex? resultantStationIndex = this.SceneUnitData.CreateStation(this.ALLY_STATION_LOCATIONS[i], UnitTeam.ALLY, isTemp: false);

            if (resultantStationIndex == null){ UnityEngine.Debug.LogWarning("ERROR — CREATE_STARTING_STATIONS: INVALID OR DUPLICATE STATION LOCATION!");   }
        }

        /*  Do Enemy Stations turns.    */
        for (int i = 0; i < MAX_STATIONS_PER_SIDE; i++)
        {
            StationIndex? resultantStationIndex = this.SceneUnitData.CreateStation(this.ENEMY_STATION_LOCATIONS[i], UnitTeam.ENEMY, isTemp: false);

            if (resultantStationIndex == null) { UnityEngine.Debug.LogWarning("ERROR — CREATE_STARTING_STATIONS: INVALID OR DUPLICATE STATION LOCATION!"); }
        }
    }
    
    /// <summary>
    /// This takes all created units and assigns them their station index if they are null. We could consider adding a keyword or something to a Unit to determine if it stays in resurve until later.  
    /// </summary>
    public void DeployUnitsForStartOfBattle()
    {
        /*  Deploy the Units so that the first units in the list go onto the first empty station on their team. */
        foreach (KeyValuePair<int, BaseBattleUnit> battleUnitKeyValuePairs in this.SceneUnitData.Units)
        {
            UnitIndex battleUnitIndex = new() { Index = battleUnitKeyValuePairs.Key };
            BaseBattleUnit battleUnit = battleUnitKeyValuePairs.Value;

            /*  Get the team of the Unit, loop through the stations to find the first empty station on that team.   */
            UnitTeam battleUnitTeam = battleUnit.GetTeam();
            Station emptyStationOnTeam = FindFirstEmptyStationOnTeam(battleUnitTeam);
            if (emptyStationOnTeam == null) { UnityEngine.Debug.LogWarning("WARNING — DEPLOY_UNITS_FOR_START_OF_BATTLE: UNABLE TO FIND EMPTY STATION ON TEAM. UNIT IS IN RESERVE!"); }
            else
            {
                /*  If we found a valid station, assign this BattleUnit's UnitIndex to this station!    */
                emptyStationOnTeam.UnitOnStation = battleUnitIndex;
            }
        }
    }

    private Station FindFirstEmptyStationOnTeam(UnitTeam unitTeam)
    {
        foreach (KeyValuePair<int, Station> stationsKeyValuePairs in this.SceneUnitData.Stations)
        {
            Station station = stationsKeyValuePairs.Value;
            if(station == null) continue;

            if(station.StationTeam == unitTeam && station.UnitOnStation == null) return station;
        }
        return null;
    }
}
