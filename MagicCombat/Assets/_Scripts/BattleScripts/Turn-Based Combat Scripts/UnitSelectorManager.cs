using System;
using System.Collections.Generic;
using System.Linq;
using TurnBased;
using UnityEngine;

public class StationSelectorManager : MonoBehaviour
{
    /// <summary>
    /// Invoked when we change the selected Station. 
    /// [ StationIndex 1: New Selected Station Index ]
    /// [ StationIndex 2: De-Selected Station Index  ]
    /// </summary>
    public static event Action<StationIndex, StationIndex?> OnSelectionChange;

    [SerializeField] List<StationIndex> stationIndexes = new();
    [SerializeField] StationIndex selectedStationIndex;

    private static StationSelectorManager instance;
    public static StationSelectorManager Instance
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
        InitaliseSingleton();
    }

    private void InitaliseSingleton()
    {        
        /*  Initalise the Singleton.    */
        if (instance != null && instance != this)
        {
            DestroyImmediate(this);
        }
        instance = this;

    }




    private void Start()
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

        List<StationIndex> allyStationIndexes = new();
        List<StationIndex> enemyStationIndexes = new();

        /*  Loop through each StationIndex pulled from the StationManager, sort them by the team they are on.   */
        foreach (StationIndex stationIndex in this.stationIndexes)
        {
            if(!StationManager.Instance.GetStationOfStationIndex(stationIndex, out Station stationOfStationIndex)){ continue; }
            UnitTeam stationTeam = stationOfStationIndex.StationTeam;

            if (stationTeam == UnitTeam.ALLY) { allyStationIndexes.Add(stationIndex); }
            else { enemyStationIndexes.Add(stationIndex); }
        }

        /*  Once we sorted based on Team, sort the stations by numerical order. */
        List<StationIndex> sortedAllyStationIndexes = allyStationIndexes.OrderBy(i => i.Index).ToList();
        List<StationIndex> sortedEnemStationIndexes = enemyStationIndexes.OrderBy(i => i.Index).ToList();

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
        /*  Find where this index is in the Stations list.  */
        if (!FindCurrentIndexInStationIndexesList(out int currentIndex)) { return; }
        currentIndex++;

        /*  Wrap the index between 0 and the length of the Populated Station List.  */
        if (!WrapIndex(currentIndex, out int? wrappedIndex)) {  return; }

        /*  Invoke the event because we have switched. Pass the old index and the new one!  */
        StationIndex oldStationIndex = this.selectedStationIndex;
        this.selectedStationIndex = this.stationIndexes[wrappedIndex.Value];
        OnSelectionChange?.Invoke(this.selectedStationIndex, oldStationIndex);
    }
    public void DecrementIndex()
    {
        /*  Find where this index is in the Stations list.  */
        if (!FindCurrentIndexInStationIndexesList(out int currentIndex)) { return; }
        currentIndex--;

        /*  Wrap the index between 0 and the length of the Populated Station List.  */
        if (!WrapIndex(currentIndex, out int? wrappedIndex)) { return; }

        /*  Invoke the event because we have switched. Pass the old index and the new one!  */
        StationIndex oldStationIndex = this.selectedStationIndex;
        this.selectedStationIndex = this.stationIndexes[wrappedIndex.Value];
        OnSelectionChange?.Invoke(this.selectedStationIndex, oldStationIndex);
    }

    #endregion

    public StationIndex GetSelectedStationUnitIndex()
    {
        return this.selectedStationIndex;
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