using System;
using System.Collections.Generic;
using System.Linq;
using TurnBased;
using UnityEngine;

public class UnitSelectorManager : MonoBehaviour
{
    List<StationIndex?> stationIndexes = new();
    StationIndex? currentSelectedStationIndex;
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
        if (instance != null && instance != this)
        {
            DestroyImmediate(this.gameObject);
        }
        instance = this;
    }

    private void Start()
    {
        Initalise(BattleMediator.Instance.GetStations());
    }

    public void Initalise(List<StationIndex?> stations)
    {
        this.stationIndexes = stations;
        this.currentSelectedStationIndex = this.stationIndexes.FirstOrDefault();
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
            if (this.stationIndexes[i]?.Index == this.currentSelectedStationIndex?.Index)
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
            SetCurrentStationIndex(i);
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
            SetCurrentStationIndex(i);
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

    private void SetCurrentStationIndex(int index)
    {
        if (index >= 0)
        {
            this.currentSelectedStationIndex = this.stationIndexes[index];
        }
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
        if (this.currentSelectedStationIndex != null)
        {
            return BattleMediator.Instance.GetUnitIndexOnStation(this.currentSelectedStationIndex.Value);
        }
        return null;
    }
    public StationIndex? GetCurrentSelectedStation() => this.currentSelectedStationIndex;
}
