using System;
using System.Collections.Generic;
using System.Linq;
using TurnBased.TargetSelection;

public readonly struct UnitIndex     
{  
    public int Index { get; }
    public UnitIndex(int index)
    {
        this.Index = index;
    }
}

[Serializable]public readonly struct StationIndex
{
    public int Index { get; }
    public StationIndex(int index)
    {
        this.Index = index;
    }
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
    public System.Collections.Generic.Dictionary<int, BaseBattleUnit> Units = new();
    public System.Collections.Generic.Dictionary<int, Station> Stations = new();

    public static int MAX_UNITS_PER_SIDE = 9;
    private int nextUnitId = 0, nextStationId = 0;

    public SceneUnitData()
    {
        this.nextUnitId = 0;
        this.Units = new();
        this.Stations = new();
    }

    ~SceneUnitData()
    {
        this.nextStationId = 0;
        
        this.Units.Clear();
        this.Units = null;

        this.Stations.Clear();
        this.Stations = null;
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
        StationIndex createdStationIndex = new(stationId);
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
        foreach(System.Collections.Generic.KeyValuePair<int, Station> stationKeyValuePair in this.Stations)
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
        UnitIndex createdUnitIndex = new(unitId);

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

        /*  If the removed unit was on a station, remove it from the station.   */
        if(GetStationOfUnitIndex(unitIndex, out Station stationRemovedUnitWasOn)){
            stationRemovedUnitWasOn.UnitOnStation = null;   
        }

        /*  Remove that Unit. Notify any Listeners. */
        this.Units.Remove(unitIndex.Index);



        return true;
    }

    /// <summary>
    /// Deploy a Unit from resurves.
    /// </summary>
    /// <param name="unitIndex"></param>
    /// <param name="stationIndex"></param>
    /// <returns></returns>
    public bool DeployUnit(UnitIndex unitIndex, StationIndex stationIndex, out UnitIndex? previousUnit)
    { 
        previousUnit = null;

        /*  Is the Unit already on the battlefield? I.e. Does a station already have that same UnitIndex supplied?  */
        if (IsUnitIndexOnBattlefield(unitIndex)) return false;

        /*  Is this a valid station ID? If not, stop!   */
        if (!IsStationIndexValid(stationIndex)) return false;

        /*  Confirm that the station and unit are on the same team! */
        Station station = this.Stations[stationIndex.Index];
        BaseBattleUnit unit = this.Units[unitIndex.Index];
        if (station.StationTeam != unit.GetTeam()) return false;

        /*  Return the previous Unit on the station if there was one, and assign the new unit onto the station. */
        previousUnit = station.UnitOnStation;
        station.UnitOnStation = unitIndex;
        return true;
    }
    public UnitIndex[] GetUnitsInReserve(UnitTeam team)
    {
        List<UnitIndex> reserveUnitIndexes = new();
        foreach (KeyValuePair<int, BaseBattleUnit> unitKeyValuePair in this.Units)
        {
            UnitIndex unitIndex = new(unitKeyValuePair.Key);

            // Only add the UnitIndexes if they aren't on the battlefield and on the same team as the one we request.
            if (!IsUnitIndexOnBattlefield(unitIndex) && unitKeyValuePair.Value.GetTeam() == team)
            {
                reserveUnitIndexes.Add(unitIndex);
            }
        }
        return reserveUnitIndexes.ToArray();
    }

    public bool TryGetUnitIndexesOfTeam(UnitTeam team, out List<UnitIndex> unitIndexesOnTeam)
    {
        // Loop through our Units. Look into them and determine which team they are on. Add those into a list and return the completed list.    
        unitIndexesOnTeam = new();
        
        foreach (KeyValuePair<int, BaseBattleUnit> unitKeyValuePair in this.Units)
        {
            int unitIndex               = unitKeyValuePair.Key;
            BaseBattleUnit battleUnit   = unitKeyValuePair.Value;

            if (battleUnit.GetTeam() == team)
            {
                unitIndexesOnTeam.Add(new UnitIndex(unitIndex));
            }
        }

        /*  Once retrieved, make sure it is populated, if not, we had insusficient Units.   */
        if(unitIndexesOnTeam.Count <= 0) { return false; }

        return true;
    }

    public bool TryGetTeamOfUnitIndex(UnitIndex unitIndex, out UnitTeam team)
    {
        team = UnitTeam.NULL;
        /*  Find if this UnitIndex is related to a Unit we have information on. If not, exit out.   */
        if (!this.Units.ContainsKey(unitIndex.Index)) { return false; }

        BaseBattleUnit unit = this.Units[unitIndex.Index];
        return unit.GetTeam() == team;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="stationIndex"></param>
    /// <param name="outUnitIndex"></param>
    /// <returns>True if there was a sucessful retrieval UnitIndex. Return false if the StationIndex is not Valid, or if there is no Unit on that station.  </returns>
    public bool TryGetUnitIndexOfStationIndex(StationIndex stationIndex, out UnitIndex outUnitIndex)
    {
        outUnitIndex = new();
        /*  Is this a valid station ID? If not, stop!   */
        if (!IsStationIndexValid(stationIndex)) { return false; }

        if (!DoesStationHaveUnit(stationIndex)){ return false; }

        outUnitIndex = this.Stations[stationIndex.Index].UnitOnStation.Value;
        return true;
    }

    public bool TryGetStationIndexOfUnitIndex(UnitIndex unitIndex, out StationIndex stationIndexOfUnitIndex)
    {
        stationIndexOfUnitIndex = default;
        foreach (KeyValuePair<int,Station> stationKeyValuePairs in this.Stations)
        {
            Station station = stationKeyValuePairs.Value;
            if (!station.UnitOnStation.HasValue) { continue; }
            if(station.UnitOnStation.Value.Index == unitIndex.Index)
            {
                stationIndexOfUnitIndex = station.StationIndex;
                return true;
            }
        }
        return false;
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
            int unitIndex = unitsKeyValuePair.Key;  
            BaseBattleUnit battleUnit = unitsKeyValuePair.Value;

            if(battleUnit.GetTeam() == team)
            {
                unitIndexesOnTeam.Add(new UnitIndex(unitIndex));
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

        UnityEngine.Debug.LogError($"Active units count is: {activeUnits.Count}");
        return activeUnits;
    }

    public List<UnitIndex> GetAllUnits()
    {
        List<UnitIndex> units = new();
        if(this.Units.Count < 0) { return new(); }

        return null;

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
            Station station = stationKeyValuePairs.Value;
            stationIndexes.Add(station.StationIndex);
        }
        return stationIndexes;
    }

    /// <summary>
    /// </summary>
    /// <returns>All populated Station Indexes (Station Indexes correlating to Stations with a Unit on them).</returns>
    public List<StationIndex> GetPopulatedStationIndexes()
    {
        List<StationIndex> stationIndexes = new();
        /*  Loop through all Stations, if a station has a Unit on it, add it to the List.   */
        foreach (KeyValuePair<int, Station> stationKeyValuePairs in this.Stations)
        {
            Station station = stationKeyValuePairs.Value;

            UnitIndex? unitIndexOnStation = station.UnitOnStation;
            if(unitIndexOnStation == null) { continue; }

            stationIndexes.Add(station.StationIndex);
        }
        return stationIndexes;
    }
    public UnitTeam GetUnitTeamOfUnitIndex(UnitIndex unitIndex)
    {
        if(!this.GetBattleUnitOfIndex(unitIndex, out BaseBattleUnit unit)) { return UnitTeam.NULL; }
        return unit.GetTeam();
    }

    public bool IsStationValid(Station station)
    {
        foreach (KeyValuePair<int, Station> stationKeyValuePair in this.Stations)
        {
            if(stationKeyValuePair.Value == station)
            {
                return true;
            }
        }
        return false;
    }
    public bool IsUnitValid(BaseBattleUnit unit)
    {
        foreach(KeyValuePair<int, BaseBattleUnit> unitKeyValuePair in this.Units)
        {
            if(unitKeyValuePair.Value == unit)
            {
                return true;
            }
        }
        return false;
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
    public bool GetBattleUnitOfIndex(UnitIndex index, out BaseBattleUnit unit)
    {
        unit = default;
        if(this.Units.TryGetValue(index.Index, out BaseBattleUnit retrievedUnit))
        {
            unit = retrievedUnit;
            return true;
        }
        return false;   
    }
    public bool GetStationOfStationIndex(StationIndex stationIndex, out Station stationOfStationIndex)
    {
        stationOfStationIndex = default;
        if (this.Stations.TryGetValue(stationIndex.Index, out Station station))
        {
            stationOfStationIndex = station;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Gets the Station that a Unit is on.  
    /// </summary>
    /// <param name="index"></param>
    /// <returns>The Station the Unit is on if Sucessful. Returns a null referance if it is invalid.    </returns>
    public bool GetStationOfUnitIndex(UnitIndex index, out Station stationOfUnitIndex)
    {
        stationOfUnitIndex = default;
        /*  Find the Station the UnitIndex is on.   */
        if(!TryGetStationIndexOfUnitIndex(index, out StationIndex stationIndex)) { return false; }

        bool valid = IsStationIndexValid(stationIndex);
        if (!valid) { return false; }

        /*  Get the Station.    */
        if(!GetStationOfStationIndex(stationIndex, out Station stationOfStationIndex)) { return false; }
        stationOfUnitIndex = stationOfStationIndex;
        return true;
    }
}

public class StationManager
{
    private static StationManager instance;
    public static StationManager Instance 
    {  
        get { return instance; }
        set
        {
            if (instance != null)
            {
                instance = value;
            }
        }
    }

    #region Events
    /// <summary>
    /// Invoked when a Unit is added. Normally at start of game.    
    /// [ UnitIndex: Unit Index of the Unit being added.  ]
    /// </summary>
    public static event Action<UnitIndex> OnAddUnit;

    /// <summary>
    /// Invoked when a Unit is Removed. When a unit is destroyed.   
    /// [ Unit Index: The UnitIndex this Unit was correlated to. ]
    /// [ Station Index: StationIndex of the Destroyed Unit. Could be null if they were removed in resurve. ]
    /// [ BaseBattleUnit: Main Script of the Unit, allows use of the GameObject. ]
    /// 
    /// IMPORTANT: As we remove the UnitIndex, StationIndex from arrays, you cannot use any method within SceneUnitData to retrieve further information on the Unit. 
    /// However, you can use the StationIndex to consult another class to retrieve the Station's Location in worldSpace.
    /// </summary>
    public static event Action<UnitIndex, StationIndex?, BaseBattleUnit> OnRemoveUnit;

    /// <summary>
    /// Invoked on Switching the stations of Units on the Same Team. 
    /// 
    /// [ Index 1: UnitIndex that is switching to the desired station. ]
    /// [ Index 2: UnitIndex that is being forced to the other Unit's Station.]
    /// </summary>
    public static event Action<UnitIndex?, UnitIndex?> OnSwapUnits;


    /// <summary>
    /// Invoked on failing Switching the stations of Units on the Same Team. 
    /// 
    /// [ StationIndex 1: StationIndex that would be switching to the desired station. ]
    /// [ StationIndex 2: StationIndex that would be forced to the other Unit's Station. ]
    /// [ UnitIndex 1: UnitIndex that would be switching to the desired station. ]
    /// [ UnitIndex 2: UnitIndex that would be forced to the other Unit's Station. ]
    /// </summary>
    public static event Action<StationIndex, StationIndex, UnitIndex, UnitIndex> OnFailSwapUnits;

    /// <summary>
    /// Invoked on Deploying from Resurves. 
    /// 
    /// [ Station 1: Station we are deploying the new unit onto. ]
    /// [ BaseBattleUnit 1: BaseBattleUnit of the unit that is going onto the station. ]
    /// [ BaseBattleUnit 2: BaseBattleUnit of the unit that is going into resurves. ]
    /// 
    /// [ StationIndex: StationIndex we are deploying the new unit onto. ]
    /// [ UnitIndex 1: UnitIndex of the unit that is going onto the station. ]
    /// [ UnitIndex 2: UnitIndex of the unit that is going into resurves. ]
    /// </summary>
    public static event Action<StationIndex, UnitIndex, UnitIndex?> OnDeployUnit;

    #endregion

    private SceneUnitData SceneUnitData = new();

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

    public void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        this.SceneUnitData = new();
        CreateStartingStations();
    }

    public void OnDestroy()
    {
        if (instance != null && instance == this)
        {
            instance = null;
        }

        OnAddUnit = null;
        OnRemoveUnit = null;
        OnSwapUnits = null;
        OnFailSwapUnits = null;
        OnDeployUnit = null;


        this.SceneUnitData = null;
    }

    public List<UnitIndex>      GetAllActiveUnits()                                                                         => this.SceneUnitData.GetAllActiveUnits();
    public List<UnitIndex>      GetUnitsOnTeam              (UnitTeam team)                                                 => this.SceneUnitData.GetUnitsOnTeam(team);
    public bool                 TryGetUnitIndexesOfTeam     (UnitTeam team, out List<UnitIndex> unitIndexesOnTeam)          => this.SceneUnitData.TryGetUnitIndexesOfTeam(team, out unitIndexesOnTeam);
    public bool                 TryGetTeamOfUnitIndex       (UnitIndex unitIndex, out UnitTeam team)                        => this.SceneUnitData.TryGetTeamOfUnitIndex(unitIndex, out team);
    public List<StationIndex>   GetStationsIndex()                                                                          => this.SceneUnitData.GetStationIndexes();
    public List<StationIndex>   GetPopulatedStationIndexes()                                                                => this.SceneUnitData.GetPopulatedStationIndexes();
    public bool                 TryGetUnitIndexOnStation    (StationIndex stationIndex, out UnitIndex unitIndexOnStation)   => this.SceneUnitData.TryGetUnitIndexOfStationIndex(stationIndex, out unitIndexOnStation);
    public bool                 TryGetStationIndexOfIndex   (UnitIndex index,           out StationIndex stationIndex)      => this.SceneUnitData.TryGetStationIndexOfUnitIndex(index, out stationIndex); 
    public bool                 TryGetBattleUnitOfIndex     (UnitIndex index,           out BaseBattleUnit unit)            => this.SceneUnitData.GetBattleUnitOfIndex(index, out unit);
    public bool                 TryGetStationOfUnitIndex    (UnitIndex index,           out Station stationOfUnitIndex)     => this.SceneUnitData.GetStationOfUnitIndex(index, out stationOfUnitIndex);
    public bool                 TryGetStationOfStationIndex(StationIndex stationIndex, out Station stationOfStationIndex)   => this.SceneUnitData.GetStationOfStationIndex(stationIndex, out stationOfStationIndex);
    public UnitTeam             GetUnitTeamOfIndex      (UnitIndex index)                                                   => this.SceneUnitData.GetUnitTeamOfUnitIndex(index);
    public bool                 IsStationValid          (Station station)                                                   => this.SceneUnitData.IsStationValid(station);
    public bool                 IsUnitValid             (BaseBattleUnit unit)                                               => this.SceneUnitData.IsUnitValid(unit);



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
        /*  Deploy the unit and cancel out of here if we weren't sucessful! */
        if (!this.SceneUnitData.DeployUnit(unitIndex, stationIndex, out UnitIndex? previousUnitIndex))  { return false; }

        OnDeployUnit?.Invoke(stationIndex, unitIndex, previousUnitIndex);

        return true;
    }
    public bool RemoveUnit(UnitIndex unitIndex)
    {
        if (!this.SceneUnitData.TryGetStationIndexOfUnitIndex(unitIndex, out StationIndex removedUnitStationIndex)) {  return false; }

        if (!this.SceneUnitData.GetBattleUnitOfIndex(unitIndex, out BaseBattleUnit removedUnit)) {  return false; }

        bool sucess = this.SceneUnitData.RemoveUnit(unitIndex);
        if (sucess) 
        {
            OnRemoveUnit?.Invoke(unitIndex, removedUnitStationIndex, removedUnit);
            return true;
        }
        return false;        
    }
    public bool SwitchUnitStations(UnitIndex unitIndexA, UnitIndex unitIndexB)
    {   
        /*  Get each of the stations of the selected Units. */
        if(!this.SceneUnitData.TryGetStationIndexOfUnitIndex(unitIndexA, out StationIndex stationIndexA)) {  return false; }
        if (!this.SceneUnitData.TryGetStationIndexOfUnitIndex(unitIndexB, out StationIndex stationIndexB)) { return false; }

        bool sucess = this.SceneUnitData.SwapUnitsOnStations(stationIndexA, stationIndexB);

        if (sucess) 
        {
            OnSwapUnits?.Invoke(unitIndexA, unitIndexB);
            return true;
        }
        else
        {
            OnFailSwapUnits?.Invoke(stationIndexA, stationIndexB, unitIndexA, unitIndexB);
            return false;
        }
    }
    public bool SwitchUnitStations(StationIndex stationIndexA, StationIndex stationIndexB)
    {
        /*  Get each of the stations of the selected Units returning false if they weren't valid.   */
        if(this.SceneUnitData.TryGetUnitIndexOfStationIndex(stationIndexA, out UnitIndex unitIndexA)) { return false; }
        if(this.SceneUnitData.TryGetUnitIndexOfStationIndex(stationIndexB, out UnitIndex unitIndexB)) { return false; }

        bool sucess = this.SceneUnitData.SwapUnitsOnStations(stationIndexA, stationIndexB);

        if (sucess)
        {
            OnSwapUnits?.Invoke(unitIndexA, unitIndexB);
            return true;
        }
        else
        {
            OnFailSwapUnits?.Invoke(stationIndexA, stationIndexB, unitIndexA, unitIndexB);
            return false;
        }
    }

    public List<StationIndex> GetStationIndexesFromUnitIndexes(List<UnitIndex> indexes)
    {
        List<StationIndex> stationIndexes = new();
        foreach (UnitIndex unitIndex in indexes)
        {
            if(!this.SceneUnitData.TryGetStationIndexOfUnitIndex(unitIndex, out StationIndex stationIndexOfUnitIndex)) { continue; }
            stationIndexes.Add(stationIndexOfUnitIndex);

        }
        return stationIndexes;
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
            UnitIndex battleUnitIndex = new(battleUnitKeyValuePairs.Key);
            BaseBattleUnit battleUnit = battleUnitKeyValuePairs.Value;

            /*  Get the team of the Unit, loop through the stations to find the first empty station on that team.   */
            UnitTeam battleUnitTeam = battleUnit.GetTeam();
            StationIndex? emptyStationIndexOnTeam = FindFirstEmptyStationIndexOnTeam(battleUnitTeam);
            if (emptyStationIndexOnTeam == null) { UnityEngine.Debug.LogWarning("WARNING — DEPLOY_UNITS_FOR_START_OF_BATTLE: UNABLE TO FIND EMPTY STATION ON TEAM. UNIT IS IN RESERVE!"); }
            else
            {
                /*  If we found a valid station, assign this BattleUnit's UnitIndex to this station!    */
                DeployUnit(battleUnitIndex, emptyStationIndexOnTeam.Value);
            }
        }
    }

    public bool TryGetBaseBattleUnitOnStation(StationIndex stationIndex, out BaseBattleUnit baseBattleUnit)
    {
        baseBattleUnit = default;

        if (!TryGetUnitIndexOnStation(stationIndex, out UnitIndex unitIndex)) { return false; }

        if (!TryGetBattleUnitOfIndex(unitIndex, out baseBattleUnit)) { return false; }

        return true;
    }

    public bool TryGetUnitDataOnStation(StationIndex stationIndex, out UnitData unitData)
    {
        unitData = default;

        if(!TryGetBaseBattleUnitOnStation(stationIndex, out BaseBattleUnit baseBattleUnit)) {  return false; }

        unitData = baseBattleUnit.GetBaseUnit();
        return unitData != null;
    }

    public bool TryGetUnitDataOfUnitIndex(UnitIndex unitIndex, out UnitData unitData)
    {
        unitData = default;

        if (!TryGetBattleUnitOfIndex(unitIndex, out BaseBattleUnit baseBattleUnit)) { return false; }

        unitData = baseBattleUnit.GetBaseUnit();
        return unitData != null;
    }

    public bool DoesUnitIndexExist(UnitIndex index)
    {
        if (this.SceneUnitData.Units.ContainsKey(index.Index)) { return true; }
        return false;
    }


    /// <summary>
    /// This is working as intended.
    /// </summary>
    /// <param name="unitTeam"></param>
    /// <returns></returns>
    private StationIndex? FindFirstEmptyStationIndexOnTeam(UnitTeam unitTeam)
    {
        foreach (KeyValuePair<int, Station> stationsKeyValuePairs in this.SceneUnitData.Stations)
        {
            Station station = stationsKeyValuePairs.Value;
            if (station == null) continue;

            if (station.StationTeam == unitTeam && station.UnitOnStation == null)
            {
                return station.StationIndex;
            }
        }
        return null;
    }
}


/// =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-
/// 
/// STATION MANAGER UTILITY METHODS
/// 
/// =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

public readonly struct TargettingSelectorInfo
{
    public List<StationIndex> PossibleTargets { get; }
    public bool DoesRequireTargettingSelectorSelection { get; }
    public string TargettingDisplayText { get; }

    public TargettingSelectorInfo(List<StationIndex> targets, bool doesRequireSelection, string targetDisplayText)
    {
        this.PossibleTargets = targets;
        this.DoesRequireTargettingSelectorSelection = doesRequireSelection;
        this.TargettingDisplayText = targetDisplayText; 
    }
}


public static class StationManagerUtilities
{
    public static bool GetBattleUnitOnStation(StationIndex selectedStationIndex, out BaseBattleUnit battleUnitOnStation)
    {
        /*  Default declaration if invalid. */
        battleUnitOnStation = default;

        /*  Get the UnitIndex on the station. If this is invalid, Abort!  */
        if (!StationManager.Instance.TryGetUnitIndexOnStation(selectedStationIndex, out UnitIndex unitIndex)){ return false; }

        /*  Send out the UnitIndex and BaseBattleUnit.  */
        if(!StationManager.Instance.TryGetBattleUnitOfIndex(unitIndex, out BaseBattleUnit battleUnit)) { return false; }

        battleUnitOnStation = battleUnit;
        return true;
    }

    public static bool TryCreateCombatSceneDataForUnitIndex(UnitIndex sourceUnitIndex, out UnitTurnStationIndexesSceneData sceneData)
    {
        sceneData = default;
        StationManager stationManager = StationManager.Instance;

        /*  Get a list of all UnitIndexes on the opposite team. */
        UnitTeam sourceTeam = stationManager.GetUnitTeamOfIndex(sourceUnitIndex);
        if (sourceTeam == UnitTeam.NULL) { return false; }

        UnitTeam oppositeTeam = GetOppositeTeamType(stationManager.GetUnitTeamOfIndex(sourceUnitIndex));
        if (sourceTeam == UnitTeam.NULL) { return false; }

        /*  Get a list of all UnitIndexes that are on the team. */
        if (!stationManager.TryGetUnitIndexesOfTeam(sourceTeam, out List<UnitIndex> allIUnitIndexesOnAlliedTeam))
        { return false; throw new InvalidOperationException("ERROR — STATION_HANDLER: INSUFFICIENT QUANTITY OF UNITS PRESENT INSIDE TEAMED INDEXES!");  }
        if (!stationManager.TryGetUnitIndexesOfTeam(oppositeTeam, out List<UnitIndex> unitIndexesOfOppositeTeam))
        { return false; throw new InvalidOperationException("ERROR — STATION_HANDLER: INSUFFICIENT QUANTITY OF UNITS PRESENT INSIDE TEAMED INDEXES!"); }

        /*  Confirm that the sourceUnitIndex is contained within UnitIndexesOnTeam. If not, something broke.    */
        if (!allIUnitIndexesOnAlliedTeam.Contains(sourceUnitIndex)) { return false; throw new InvalidOperationException("ERROR — STATION_HANDLER: SOURCE UNIT INDEX NOT PRESENT INSIDE TEAMED INDEXES!"); }

        /*  Confirm that we have a teamedIndex array of length greater than 0. If it is 0, we don't want to proceed.    */
        if (allIUnitIndexesOnAlliedTeam.Count <= 0) { return false; throw new InvalidOperationException("ERROR — STATION_HANDLER: TEAMED UNIT INDEX LIST IS LESS THAN 0!"); }

        /*  Remove the source Unit Index from the TeamedIndexes Array.  */
        List<UnitIndex> TeamedIndexes = GetUnitIndexCollectionRemovingSourceIndex(allIUnitIndexesOnAlliedTeam, sourceUnitIndex);

        /*  Try to convert the UnitIndex to the stationIndex for purposes of Scene Data.    */
        if (!stationManager.TryGetStationIndexOfIndex(sourceUnitIndex, out StationIndex sourceStationIndex)) { return false; throw new InvalidOperationException("ERROR — STATION_HANDLER: UNABLE TO RETRIEVE STATION INDEX OF UNIT INDEX!"); }

        /*  Convert UnitIndex of both allied and enemy lists to the StationIndex of that Unit. */
        List<StationIndex> allyStationIndexes = stationManager.GetStationIndexesFromUnitIndexes(TeamedIndexes);
        List<StationIndex> enemyStationIndexes = stationManager.GetStationIndexesFromUnitIndexes(unitIndexesOfOppositeTeam);



        // TO DO: PASS IN ENVIRONMENT DATA
        sceneData = new UnitTurnStationIndexesSceneData(sourceStationIndex, allyStationIndexes, enemyStationIndexes);
        return true;
    }

    public static bool TryCreateUnitDataSceneDataForUnitIndex(UnitIndex unitIndex, out UnitDataUnitTurnSceneData unitDataSceneData)
    {
        unitDataSceneData = default;

        UnitData sourceUnitData = null;
        System.Collections.Generic.List<UnitData> allyData  = new();
        System.Collections.Generic.List<UnitData> enemyData = new();

        UnityEngine.Debug.LogError($"Creating combat scene data for unit index");

        /*  Get the stationIndexes for each active unit in the scene to loop through them and retrieve their data.  */
        if (!TryCreateCombatSceneDataForUnitIndex(unitIndex, out UnitTurnStationIndexesSceneData sceneData)) { return false; }

        foreach (StationIndex allyStationIndex in sceneData.AllyStationIndexes) 
        {
            if (!StationManager.Instance.TryGetUnitDataOnStation(allyStationIndex, out UnitData unitData)) { continue; }
            allyData.Add(unitData);
        }

        foreach (StationIndex enemyStationIndex in sceneData.EnemyStationIndexes)
        {
            if (!StationManager.Instance.TryGetUnitDataOnStation(enemyStationIndex, out UnitData unitData)) { continue; }
            enemyData.Add(unitData);
        }

        if (!StationManager.Instance.TryGetUnitDataOnStation(sceneData.SourceStationIndex, out sourceUnitData)) { return false; throw new NullReferenceException("ERROR — STATION_HANDLER: SOURCE UNIT INDEX UNABLE TO GET UNIT DATA!"); }

        unitDataSceneData = new UnitDataUnitTurnSceneData(sourceUnitData, allyData, enemyData);
        return true;
    }

    public static bool TryCreateSceneDataForTeam(UnitTeam team, out UnitTurnStationIndexesSceneData stationIndexesSceneData)
    {
        stationIndexesSceneData = default;

        UnitTeam oppositeTeam = GetOppositeTeamType(team);

        System.Collections.Generic.List<UnitIndex> unitIndexesOnTeam = StationManager.Instance.GetUnitsOnTeam(team);
        System.Collections.Generic.List<UnitIndex> unitIndexesOnOppositeTeam = StationManager.Instance.GetUnitsOnTeam(oppositeTeam);

        System.Collections.Generic.List<StationIndex> teamedStationIndexes = new();
        System.Collections.Generic.List<StationIndex> oppositeTeamedStationIndexes = new();

        foreach (UnitIndex unitIndex in unitIndexesOnTeam)
        {
            if (!StationManager.Instance.TryGetStationIndexOfIndex(unitIndex, out StationIndex stationIndex)) { continue; }

            teamedStationIndexes.Add(stationIndex);
        }

        foreach (UnitIndex unitIndex in unitIndexesOnOppositeTeam)
        {
            if (!StationManager.Instance.TryGetStationIndexOfIndex(unitIndex, out StationIndex stationIndex)) { continue; }

            oppositeTeamedStationIndexes.Add(stationIndex);
        }

        if (teamedStationIndexes.Count <= 0 || oppositeTeamedStationIndexes.Count <= 0) { return false; }

        StationIndex sourceStationIndex = teamedStationIndexes.FirstOrDefault();

        stationIndexesSceneData = new UnitTurnStationIndexesSceneData(sourceStationIndex, teamedStationIndexes, oppositeTeamedStationIndexes);

        return true;
    }



    /// <returns>True if sucessful. SourceUnitData is Null as Elemental Moves don't have a source.  </returns>
    public static bool TryCreateUnitDataSceneDataForElementalMove(UnitTeam team, out UnitDataUnitTurnSceneData unitDataSceneData)
    {
        unitDataSceneData = default;

        System.Collections.Generic.List<UnitData> teamedData = new();
        System.Collections.Generic.List<UnitData> oppositeTeamData = new();

        UnitTeam oppositeTeam = GetOppositeTeamType(team);

        System.Collections.Generic.List<UnitIndex> unitIndexesOnTeam            = StationManager.Instance.GetUnitsOnTeam(team);
        System.Collections.Generic.List<UnitIndex> unitIndexesOnOppositeTeam    = StationManager.Instance.GetUnitsOnTeam(oppositeTeam);

        foreach (UnitIndex unitIndexOnTeam in unitIndexesOnTeam)
        {
            if (!StationManager.Instance.TryGetUnitDataOfUnitIndex(unitIndexOnTeam, out UnitData unitData)) { continue; }
            teamedData.Add(unitData);
        }

        foreach (UnitIndex unitIndexOnOppositeTeam in unitIndexesOnOppositeTeam)
        {
            if (!StationManager.Instance.TryGetUnitDataOfUnitIndex(unitIndexOnOppositeTeam, out UnitData unitData)) { continue; }
            oppositeTeamData.Add(unitData);
        }

        if (teamedData.Count <= 0 || oppositeTeamData.Count <= 0) { return false; }


        UnitData sourceUnitData = teamedData.FirstOrDefault();

        unitDataSceneData = new UnitDataUnitTurnSceneData(sourceUnitData, teamedData, oppositeTeamData);
        return true;
    }


    /// <summary>
    /// Allows the retrieval of a single List of StationIndexes to loop through for purposes of targetting selection 
    /// </summary>
    /// <param name="data"></param>
    /// <param name="moveTargetType"></param>
    /// <returns>
    /// A List of all possible Stations the Move can Target. If it can hit multiple Units, the bool will be set to true and the string will specify if it is all Units, all Allies, all enemies for purpose of text reasons.   
    /// [bool] isTargettingMultipleTargets: True if the move selects AllEnemies, AllAllies or Area
    /// [string] multipleTargetString: Populated with text that can be used in the creation of UI elements to describe the target.    /// 
    /// </returns>
    public static TargettingSelectorInfo FindAllPossibleTargettingStationIndexesOfTargettingType(UnitTurnStationIndexesSceneData data, MoveTarget moveTargetType)
    {
        List<StationIndex> possibleTargetStationIndexes = new();
        switch (moveTargetType)
        {
            case MoveTarget.Self:
                possibleTargetStationIndexes.Add(data.SourceStationIndex);
                return new TargettingSelectorInfo(possibleTargetStationIndexes, false, "Yourself");

            case MoveTarget.SingleAlly:
                possibleTargetStationIndexes.AddRange(data.AllyStationIndexes);
                return new TargettingSelectorInfo(possibleTargetStationIndexes, true, "One Ally");

            case MoveTarget.SingleEnemy:
                possibleTargetStationIndexes.AddRange(data.AllyStationIndexes);
                return new TargettingSelectorInfo(possibleTargetStationIndexes, true, "One Enemy");

            case MoveTarget.AllEnemies:
                possibleTargetStationIndexes.AddRange(data.EnemyStationIndexes);
                return new TargettingSelectorInfo(possibleTargetStationIndexes, false, "All Enemies");

            case MoveTarget.AllAllies:
                possibleTargetStationIndexes.AddRange(data.AllyStationIndexes);
                return new TargettingSelectorInfo(possibleTargetStationIndexes, false, "All Allies");

            case MoveTarget.Area:
                possibleTargetStationIndexes = TargetSelectorHandler.GetAllStationsOnField(data, includeSource: true);
                return new TargettingSelectorInfo(possibleTargetStationIndexes, false, "Everyone");

            default:
                possibleTargetStationIndexes.Add(data.SourceStationIndex);
                return new TargettingSelectorInfo(possibleTargetStationIndexes, false, "Uhh. Uhh...");

        }
    }

    public static TargettingSelectorInfo FindAllPossibleTargettingStationIndexesOfTargettingType(UnitIndex unitIndex, MoveTarget moveTargetType)
    {
        List<StationIndex> possibleTargetStationIndexes = new();
        if (!TryCreateCombatSceneDataForUnitIndex(unitIndex, out UnitTurnStationIndexesSceneData data)) { return new TargettingSelectorInfo(possibleTargetStationIndexes, false, "Uhh. Uhh..."); }

        switch (moveTargetType)
        {
            case MoveTarget.Self:
                possibleTargetStationIndexes.Add(data.SourceStationIndex);
                return new TargettingSelectorInfo(possibleTargetStationIndexes, false, "Yourself");

            case MoveTarget.SingleAlly:
                possibleTargetStationIndexes.AddRange(data.AllyStationIndexes);
                return new TargettingSelectorInfo(possibleTargetStationIndexes, true, "One Ally");

            case MoveTarget.SingleEnemy:
                possibleTargetStationIndexes.AddRange(data.EnemyStationIndexes);
                return new TargettingSelectorInfo(possibleTargetStationIndexes, true, "One Enemy");

            case MoveTarget.AllEnemies:
                possibleTargetStationIndexes.AddRange(data.EnemyStationIndexes);
                return new TargettingSelectorInfo(possibleTargetStationIndexes, false, "All Enemies");

            case MoveTarget.AllAllies:
                possibleTargetStationIndexes.AddRange(data.AllyStationIndexes);
                return new TargettingSelectorInfo(possibleTargetStationIndexes, false, "All Allies");

            case MoveTarget.Area:
                possibleTargetStationIndexes = TargetSelectorHandler.GetAllStationsOnField(data, includeSource: true);
                return new TargettingSelectorInfo(possibleTargetStationIndexes, false, "Everyone");

            default:
                possibleTargetStationIndexes.Add(data.SourceStationIndex);
                return new TargettingSelectorInfo(possibleTargetStationIndexes, false, "Uhh. Uhh...");

        }
    }

    public static UnitTeam GetOppositeTeamType(UnitTeam team)
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
    public static List<UnitIndex> GetUnitIndexCollectionRemovingSourceIndex(List<UnitIndex> unitIndexes, UnitIndex sourceIndex)
    {
        List<UnitIndex> indexCollection = new();
        foreach (UnitIndex unitIndex in unitIndexes)
        {
            if (unitIndex.Index != sourceIndex.Index)
            {
                indexCollection.Add(unitIndex);
            }
        }
        return indexCollection;
    }

    public static List<UnitData> GetUnitDataOfStationIndexes(List<StationIndex> stationIndexes)
    {
        List<UnitData> dataCollection = new();

        foreach (StationIndex stationIndex in stationIndexes)
        {
            if (!StationManager.Instance.TryGetUnitDataOnStation(stationIndex, out UnitData unitData)) { continue; }

            dataCollection.Add(unitData);
        }
        return dataCollection;
    }

    public static bool DoesStationIndexListContainExistantTarget(List<StationIndex> stationIndexes)
    {
        foreach (StationIndex index in stationIndexes)
        {
            if (!StationManager.Instance.TryGetUnitIndexOnStation(index, out UnitIndex unitIndexOnStation)) {  continue; }

            if (StationManager.Instance.DoesUnitIndexExist(unitIndexOnStation))
            {
                return true;
            }
        }
        return false;
    }
}