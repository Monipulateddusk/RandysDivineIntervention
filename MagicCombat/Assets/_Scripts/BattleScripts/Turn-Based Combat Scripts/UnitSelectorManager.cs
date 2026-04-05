using System;
using System.Collections.Generic;
using System.Linq;
using TurnBased;
using UnityEngine;

public class UnitSelectorManager : MonoBehaviour
{
    public struct StationLocationData { public Vector2 Location; public StationIndex? StationIndex; public UnitTeam Team; }
    public static event Action<StationLocationData> OnSelectionChange;

    [SerializeField] private List<StationLocationData> stationLocationData = new();
    public static event Action<StationLocationData> OnAddStationLocationData, OnRemoveStationLocationData;

    static readonly List<Vector2> ALLY_STATION_LOCATIONS = new(){
        new(0, 1),      new(-3, 1),         new(3, 1),
        new(1.5f, 3),   new(-1.5f,3),       new(4.5f,3),
        new(1.5f,-1),   new(-1.5f,-1),      new(4.5f,-1),
    };
    static readonly List<Vector2> ENEMY_STATION_LOCATIONS = new(){
        new(1.5f,-6),   new(-1.5f,-6),      new(4.5f,-6),
        new(0,-8),      new(-3,-8),         new(3,-8),
        new(0,-4),      new(-3,-4),         new(3,-4),
    };

    List<StationIndex?> stationIndexes = new();

    private StationIndex? selectedStationIndex;
    public StationIndex? CurrentSelectedStationIndex { get { return selectedStationIndex; }
        set
        {
            /*  Only set the value if it isn't null and has a StationIndex (Which is a masked UnitIndex) of greater than 0. */
            if (value.HasValue && value.Value.Index >= 0)
            {
                this.selectedStationIndex = this.stationIndexes[value.Value.Index];

                /*  Notify anyone listening that the selection changed. */
                StationLocationData? stationLocationData = GetCurrentStationIndexStationLocationData();
                if (stationLocationData != null)
                {
                    OnSelectionChange?.Invoke(stationLocationData.Value);
                }
            }
        }
    }


    private static UnitSelectorManager instance;
    public static UnitSelectorManager Instance
    {
        get
        {
            try
            {
                return instance;
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
                return null;
            }
        }
    }

    private void Awake()
    {
        /*  Initalise the Singleton.    */
        if (instance != null && instance != this)
        {
            DestroyImmediate(this.gameObject);
        }
        instance = this;

        /*  Subscribe to the events for Unit Data   */
        StationManager.OnAddUnit += SceneUnitData_OnAddUnit;
        StationManager.OnRemoveUnit += SceneUnitData_OnRemoveUnit;
    }

    private void OnDestroy()
    {
        /*  UnSubscribe to the events for Unit Data   */
        StationManager.OnAddUnit -= SceneUnitData_OnAddUnit;
        StationManager.OnRemoveUnit -= SceneUnitData_OnRemoveUnit;
    }

    private void SceneUnitData_OnAddUnit(UnitIndex unitIndex)
    {
        /*  Get the StationIndex of this Unit   */
        StationIndex? stationIndex = BattleMediator.Instance.GetStationIndexOfUnitIndex(unitIndex);
        if (stationIndex != null)
        {
            AddStationToList(stationIndex);
        }
    }
    private void SceneUnitData_OnRemoveUnit(StationIndex? stationIndex, BaseBattleUnit unit)
    {
        RemoveStationFromList(stationIndex);

    }

    private void Start()
    {
        Initalise();
    }

    List<Vector2> GetStationLocationsInUseOnTeam(UnitTeam team)
    {
        List<Vector2> locationsInUse = new();
        foreach (StationLocationData data in this.stationLocationData)
        {
            if (data.Team == team)
            {
                locationsInUse.Add(data.Location);
            }
        }
        return locationsInUse;
    }

    Vector2 GetNextLocationOnTeamFromLocationsInUse(UnitTeam team, List<Vector2> locationsInUse)
    {
        List<Vector2> allLocationsOnTeam = team == UnitTeam.ALLY ? ALLY_STATION_LOCATIONS : ENEMY_STATION_LOCATIONS;

        /*  Get the locations on the team (the constant vector 2s). Loop through them and remove each vector2 currently in use from the copied allLocationsOnTeam list. */
        /*  The result is a new Vector2 list which we can take the first or default value to get the next new position from our allLocationsOnTeam list.    */
        foreach (Vector2 location in locationsInUse)
        {
            allLocationsOnTeam.Remove(location);
        }

        return allLocationsOnTeam.FirstOrDefault();
    }
    bool AddStationToList(StationIndex? index)
    {
        if (index == null) { return false; }

        UnitIndex? unitIndex = BattleMediator.Instance.GetUnitIndexOnStation(index.Value);
        if (unitIndex == null) { return false; ; }

        // Look into the index, what team is it on?
        UnitTeam team = BattleMediator.Instance.GetUnitTeamOfUnitIndex(unitIndex.Value);

        /*  Get the next location not in use for that team.     */
        List<Vector2> locations = GetStationLocationsInUseOnTeam(team);
        Vector2 nextLocation = GetNextLocationOnTeamFromLocationsInUse(team, locations);

        // After everything, there is a possibility there is no more locations on that team. If so, return false.   
        if (nextLocation == null)
        {
            Debug.Log("There is no more Locations to use on Team: " + team.ToString());
            return false;
        }

        StationLocationData data = new()
        {
            StationIndex = index,
            Location = nextLocation,
            Team = team,
        };

        stationLocationData.Add(data);

        OnAddStationLocationData?.Invoke(data);

        UpdateStationList();

        return true;
    }

    bool RemoveStationFromList(StationIndex? index)
    {
        if (index == null) { return false; }

        /*  Find the Station Location in use for this Index.    */
        for (int i = 0; i < this.stationLocationData.Count; i++)
        {
            if (this.stationLocationData[i].StationIndex.Value.Index == index.Value.Index)
            {
                StationLocationData data = this.stationLocationData[i];
                this.stationLocationData.RemoveAt(i);

                OnRemoveStationLocationData?.Invoke(data);

                UpdateStationList();

                return true;
            }
        }
        return false;
    }


    public void Initalise()
    {
        UpdateStationList();

        this.CurrentSelectedStationIndex = this.stationLocationData.FirstOrDefault().StationIndex;
    }

    /// <summary>
    /// Find the index in the list where our currently selected station is stored. Allows us to select the next station over.
    /// Returns 0 if the index is somehow not found.    
    /// </summary>
    /// <returns></returns>
    private void FindIndexInStationIndexesOfCurrentSelectedStationIndex(out int value)
    {
        value = 0;
        for (int i = 0; i < this.stationIndexes.Count; i++)
        {
            if (this.stationIndexes[i]?.Index == this.CurrentSelectedStationIndex?.Index)
            {
                value = i;
                break;
            }
        }
    }

    public void IncrementIndex()
    {
        // Get the current index
        FindIndexInStationIndexesOfCurrentSelectedStationIndex(out int currentIndex);
        bool reattempted = false;
        int start = currentIndex + 1, end = this.stationIndexes.Count;

    reattempted:

        for (int i = start; i < end; i++)
        {
            if (i > this.stationIndexes.Count)
            {
                Debug.Log("Index is: " + i + " which is greater than the size of the list, breaking out the loop.");
                break;
            }
            if (!this.stationIndexes[i].HasValue) {
                continue; }

            // If this index has a value (Isn't null), then we want to take that as the new selected index.
            this.CurrentSelectedStationIndex = this.stationIndexes[i];
            return;
        }

        // If we have reached here, we have not found a new index, so we start at the start of the list.
        // If we reach our original currentIndex, then we clearly have no other options.  
        if (!reattempted)
        {
            reattempted = true;
            start = 0;
            end = currentIndex;
            goto reattempted;
        }
        /*  If we wrapped back around, then exit out so we aren't creating an infinite loop.    */
        else { return; }
    }

    public void DecrementIndex()
    {
        // Get the current index
        FindIndexInStationIndexesOfCurrentSelectedStationIndex(out int currentIndex);
        bool reattempted = false;
        int start = currentIndex - 1, end = -1;

    reattempted:

        for (int i = start; i > end; i--)
        {
            if (i < 0)
            {
                Debug.Log("Index is: " + i + " which is less than 0, breaking out the loop.");
                break;
            }
            if (!this.stationIndexes[i].HasValue)
            {
                continue;
            }

            // If this index has a value (Isn't null), then we want to take that as the new selected index.
            this.CurrentSelectedStationIndex = this.stationIndexes[i];
            return;
        }

        // If we have reached here, we have not found a new index, so we start at the start of the list.
        // If we reach our original currentIndex, then we clearly have no other options.  
        if (!reattempted)
        {
            reattempted = true;
            start = this.stationIndexes.Count - 1;
            end = currentIndex;
            goto reattempted;
        }
        /*  If we wrapped back around, then exit out so we aren't creating an infinite loop.    */
        else { return; }
    }

    private void UpdateStationList()
    {
        this.stationIndexes = BattleMediator.Instance.GetStations();
    }

    public StationLocationData? GetCurrentStationIndexStationLocationData()
    {
        if (!this.CurrentSelectedStationIndex.HasValue) { return null; }

        foreach(StationLocationData data in this.stationLocationData)
        {
            if(data.StationIndex.HasValue && data.StationIndex.Value.Index == this.CurrentSelectedStationIndex.Value.Index)
            {
                return data;
            }
        }
        return null;
    }

    public UnitIndex? GetSelectedStationUnit()
    {
        if (this.CurrentSelectedStationIndex != null)
        {
            return BattleMediator.Instance.GetUnitIndexOnStation(this.CurrentSelectedStationIndex.Value);
        }
        return null;
    }
}

public class StationIntentionManager
{
    private static StationIntentionManager instance;
    public static StationIntentionManager Instance {  
        get { return instance; } 
        set 
        {
            if (instance == null)
            {
                instance = value;
            }
        }
    }    
    Dictionary<StationIndex, UnitIntention> StationIntentionPairs = new();

    public StationIntentionManager()
    {
        Instance = this;
    }

    public bool RemoveStationIndexAndIntention(StationIndex index)
    {
        if (this.StationIntentionPairs.ContainsKey(index))
        {
            return this.StationIntentionPairs.Remove(index);
        }
        return false;
    }

    public UnitIntention? GetIntentionOfStationIndex(StationIndex index)
    {
        if (this.StationIntentionPairs.ContainsKey(index))
        {
            return this.StationIntentionPairs[index];    
        }
        return null;
    }

    public UnitIntention? SetIntention(StationIndex index, UnitIntention value)
    {
        if (this.StationIntentionPairs.ContainsKey(index))
        {
            this.StationIntentionPairs[index] = value;
            return this.StationIntentionPairs[index];
        }
        else
        {
            this.StationIntentionPairs.Add(index, value);
            return this.StationIntentionPairs[index];
        }
    }

    /// <summary>
    /// Set the targets of the existing move selection. This maintains the Invarient. If, the pairs does not exist however, return null entirely.
    /// </summary>
    /// <param name="index"></param>
    /// <param name="targets"></param>
    /// <returns></returns>
    public UnitIntention? SetIntention(StationIndex index, List<int?> targets)
    {
        if (this.StationIntentionPairs.ContainsKey(index))
        {
            // Get the intention stored here. If MoveSelection has a value, continue 
            UnitIntention intention = this.StationIntentionPairs[index];
            if(!intention.MoveSelection.HasValue) {return null;}

            this.StationIntentionPairs[index] = new(intention.MoveSelection.Value, targets);
            return this.StationIntentionPairs[index];
        }
        return null;
    }

    /// <summary>
    /// When we set pass in MoveSelectionData, we want to null target selection. 
    /// So you can't do something wierd like target yourself with a multi-hit attack because the previous target was yourself.  
    /// </summary>
    /// <param name="index"></param>
    /// <param name="moveData"></param>
    /// <returns></returns>
    public UnitIntention? SetIntention(StationIndex index, MoveSelectionData moveData)
    {
        if (this.StationIntentionPairs.ContainsKey(index))
        {
            this.StationIntentionPairs[index] = new(moveData, null);
            return this.StationIntentionPairs[index];
        }
        else
        {
            this.StationIntentionPairs.Add(index, new(moveData, null));
            return this.StationIntentionPairs[index];
        }
    }
}