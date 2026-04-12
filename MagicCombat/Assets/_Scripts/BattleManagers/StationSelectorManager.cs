using System.Linq;
using UnityEngine;


public class StationSelectorManager
{
    /// <summary>
    /// Invoked when we change the selected Station. 
    /// [ StationIndex 1: New Selected Station Index ]
    /// [ StationIndex 2: De-Selected Station Index  ]
    /// </summary>
    public static event System.Action<StationIndex, StationIndex?> OnSelectionChange;
    public static event System.Action<StationSelectionState> OnSelectionStateChange;

    System.Collections.Generic.List<StationIndex> stationIndexes = new();
    StationIndex selectedStationIndex;
    StationSelectionState selectionState;

    private static StationSelectorManager instance;
    public static StationSelectorManager Instance
    {
        get
        {
            try
            {
                return instance;
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError(e.ToString());
                return null;
            }
        }
    }

    public void Awake()
    {
        InitaliseSingleton();
    }

    private void InitaliseSingleton()
    {        
        /*  Initalise the Singleton.    */
        if (instance == null)
        {
            instance = this;
        }
    }




    public void Start()
    {
        UpdateStationIndexes();
    }


    #region Creation Of Station Indexes
    /// <summary>
    /// Pulls the StationIndexes from the StationHandler, then sorts the Indexes into Team and Numerical Order.
    /// </summary>
    private void UpdateStationIndexes()
    {
        GetStationIndexesFromStationHandler();
        SortStationIndexesForTeams();
    }

    private void GetStationIndexesFromStationHandler()
    {
        if (StationManager.Instance == null) { return; }

        this.stationIndexes = StationManager.Instance.GetPopulatedStationIndexes();
    }

    /// <summary>
    /// Sorts the StationIndexes into both Teams.
    /// Additionally, sorts StationIndexes based on numerical order. 
    /// </summary>
    private void SortStationIndexesForTeams()
    {
        if (StationManager.Instance == null) { return; }

        System.Collections.Generic.List<StationIndex> allyStationIndexes = new();
        System.Collections.Generic.List<StationIndex> enemyStationIndexes = new();

        /*  Loop through each StationIndex pulled from the StationManager, sort them by the team they are on.   */
        foreach (StationIndex stationIndex in this.stationIndexes)
        {
            if(!StationManager.Instance.TryGetStationOfStationIndex(stationIndex, out Station stationOfStationIndex)){ continue; }
            UnitTeam stationTeam = stationOfStationIndex.StationTeam;

            if (stationTeam == UnitTeam.ALLY) { allyStationIndexes.Add(stationIndex); }
            else { enemyStationIndexes.Add(stationIndex); }
        }

        /*  Once we sorted based on Team, sort the stations by numerical order. */
        System.Collections.Generic.List<StationIndex> sortedAllyStationIndexes = allyStationIndexes.OrderBy(i => i.Index).ToList();
        System.Collections.Generic.List<StationIndex> sortedEnemStationIndexes = enemyStationIndexes.OrderBy(i => i.Index).ToList();

        /*  Recombine the indexes into our StationIndexes.  */
        this.stationIndexes = new();
        this.stationIndexes.AddRange(sortedAllyStationIndexes);
        this.stationIndexes.AddRange(sortedEnemStationIndexes);
    }

    #endregion

    #region Increment Decrement Functionality

    /// <summary>
    /// Finds where our currently selectedStationIndex is in the StationIndexesList for purposes of Incrementing and Decrementing.
    /// </summary>
    /// <returns>Returns an index greater than or equal to 0 if sucessful. If not, Returns -1!  </returns>
    private bool FindCurrentIndexInStationIndexesList(out int metaIndex)
    {
        metaIndex = -1;
        for (int i = 0; i < this.stationIndexes.Count; i++)
        {
            if (this.stationIndexes[i].Index == this.selectedStationIndex.Index)
            {
                metaIndex = i;
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Wraps the index to be within the size of the StationIndexes list. If the list is too small to be wrapped, returns null.
    /// </summary>
    /// <param name="index"></param>
    /// <param name="wrappedIndex"></param>
    private bool WrapIndex(int index, out int? wrappedIndex)
    {
        if(this.stationIndexes.Count == 0) {  wrappedIndex = null; return false; }

        if(index > this.stationIndexes.Count - 1)
        {
            wrappedIndex = 0;
            return true;
        }
        else if (index < 0)
        {
            wrappedIndex = this.stationIndexes.Count - 1;
            return true;
        }
        else
        {
            wrappedIndex = index;   
            return true;
        }
    }

    public void IncrementIndex()
    {
        if(this.selectionState == StationSelectionState.Locked) { return; }

        /*  Find where this index is in the Stations list.  */
        if (!FindCurrentIndexInStationIndexesList(out int currentIndex)) { return; }
        currentIndex++;

        /*  Wrap the index between 0 and the length of the Populated Station List.  */
        if (!WrapIndex(currentIndex, out int? wrappedIndex)) {  return; }

        /*  Invoke the event because we have switched. Pass the old index and the new one!  */
        SetSelectedStationIndex(new StationIndex(wrappedIndex.Value));
    }
    public void DecrementIndex()
    {
        if (this.selectionState == StationSelectionState.Locked) { return; }

        /*  Find where this index is in the Stations list.  */
        if (!FindCurrentIndexInStationIndexesList(out int currentIndex)) { return; }
        currentIndex--;

        /*  Wrap the index between 0 and the length of the Populated Station List.  */
        if (!WrapIndex(currentIndex, out int? wrappedIndex)) { return; }

        /*  Invoke the event because we have switched. Pass the old index and the new one!  */
        SetSelectedStationIndex(new StationIndex(wrappedIndex.Value));
    }

    #endregion

    public StationIndex GetSelectedStationIndex()
    {
        return this.selectedStationIndex;
    }

    public void SetSelectedStationIndex(StationIndex newStationIndex)
    {
        Debug.LogWarning($"Setting new StationIndex to: {newStationIndex.Index}. Size of collection is: {this.stationIndexes.Count}");
        StationIndex oldStationIndex = this.selectedStationIndex;

        /*  Check to see if this new station index is contained within our StationIndexes. */
        if (!this.stationIndexes.Contains(newStationIndex)) { return; }

        this.selectedStationIndex = newStationIndex;
        OnSelectionChange?.Invoke(this.selectedStationIndex, oldStationIndex);
    }

    public void SetSelectorStateLocked()
    {
        this.selectionState = StationSelectionState.Locked;
        OnSelectionStateChange?.Invoke(this.selectionState);
    }
    public void SetSelectorStateUnlocked()
    {
        this.selectionState = StationSelectionState.Unlocked;
        OnSelectionStateChange?.Invoke(this.selectionState);
    }
    public StationSelectionState GetStationSelectionState() => this.selectionState;
}