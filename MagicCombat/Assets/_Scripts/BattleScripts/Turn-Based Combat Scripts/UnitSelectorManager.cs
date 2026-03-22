using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using TurnBased;
using Unity.VisualScripting;
using UnityEngine;

public class UnitSelectorManager : MonoBehaviour
{
    public struct StationLocationData { public Vector2 Location; public StationIndex? StationIndex; public UnitTeam Team; }
    public static event Action<StationLocationData> OnSelectionChange;

    [SerializeField] private List<StationLocationData> stationLocationData = new();

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
                if(stationLocationData != null)
                {
                    OnSelectionChange?.Invoke(stationLocationData.Value);
                    Debug.Log("New current Station index is: " + this.selectedStationIndex.Value.Index);
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
        SceneUnitData.OnAddUnit += SceneUnitData_OnAddUnit;
        SceneUnitData.OnRemoveUnit += SceneUnitData_OnRemoveUnit;
    }

    private void OnDestroy()
    {
        /*  UnSubscribe to the events for Unit Data   */
        SceneUnitData.OnAddUnit -= SceneUnitData_OnAddUnit;
        SceneUnitData.OnRemoveUnit -= SceneUnitData_OnRemoveUnit;
    }

    private void SceneUnitData_OnAddUnit(UnitIndex unitIndex)
    {
        /*  Get the StationIndex of this Unit   */
        StationIndex? stationIndex = BattleMediator.Instance.GetStationIndexOfUnitIndex(unitIndex);
        if(stationIndex != null)
        {
            AddStationToList(stationIndex);
        }
    }
    private void SceneUnitData_OnRemoveUnit(UnitIndex unitIndex, StationIndex? stationIndex, BaseBattleUnit bBU)
    {
        RemoveStationFromList(stationIndex);
        
    }

    private void Start()
    {
        Initalise(BattleMediator.Instance.GetStations());
    }

    List<Vector2> GetStationLocationsInUseOnTeam(UnitTeam team)
    {
        List<Vector2> locationsInUse = new();
        foreach(StationLocationData data in this.stationLocationData)
        {
            if(data.Team == team)
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
        if(index == null) { return false; }
        
        UnitIndex? unitIndex = BattleMediator.Instance.GetUnitIndexOnStation(index.Value);
        if(unitIndex == null) { return false; ; }

        // Look into the index, what team is it on?
        UnitTeam team = BattleMediator.Instance.GetUnitTeamOfUnitIndex(unitIndex.Value);

        /*  Get the next location not in use for that team.     */
        List<Vector2> locations = GetStationLocationsInUseOnTeam(team);
        Vector2 nextLocation = GetNextLocationOnTeamFromLocationsInUse(team, locations);

        // After everything, there is a possibility there is no more locations on that team. If so, return false.   
        if(nextLocation == null)
        {
            Debug.Log("There is no more Locations to use on Team: " + team.ToString());
            return false;
        }

        stationLocationData.Add(new()
        {
            StationIndex = index,
            Location = nextLocation,
            Team = team,
        });

        /*  For testing, set the game object positions of the basebattleunit of that index to be the location.  */
        BaseBattleUnit bBU = BattleMediator.Instance.GetBattleUnitOfUnitIndex(unitIndex.Value);
        bBU.gameObject.transform.position = new(nextLocation.x, 0, nextLocation.y);

        return true;
    }

    bool RemoveStationFromList(StationIndex? index)
    {
        if ( index == null) { return false; }

        /*  Find the Station Location in use for this Index.    */
        for (int i = 0; i < this.stationLocationData.Count; i++)
        {
            if (this.stationLocationData[i].StationIndex.Value.Index == index.Value.Index)
            {
                this.stationLocationData.RemoveAt(i);

                return true;
            }
        }
        return false;
    }


    public void Initalise(List<StationIndex?> stations)
    {
        this.stationIndexes = stations;

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

    private void IncrementIndex()
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

    private void DecrementIndex()
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


    private StationLocationData? GetCurrentStationIndexStationLocationData()
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

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            DecrementIndex();
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            IncrementIndex();
        }
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
