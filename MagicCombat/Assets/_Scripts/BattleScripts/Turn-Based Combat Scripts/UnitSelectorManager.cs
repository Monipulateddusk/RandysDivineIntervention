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

    /// <summary>
    /// How this works: When we call to increment. 
    /// Get the current index that our current value is in  the list. Then we want to do a for loop for the end of the max length of the list. 
    /// If it has a value, then that is the  value  we go  with. If we reach the  end of  the loop, wrap back  around.
    /// If  we reach the first index again,  we stop
    /// </summary>
    /// <param name="newIndex"></param>
    private void IncrementIndex()
    {
        // Get the current index
        FindIndexInStationIndexesOfCurrentSelectedStationIndex(out int currentIndex);

        for(int i = currentIndex + 1; i < this.stationIndexes.Count; i++)
        {
            if (!this.stationIndexes[i].HasValue) {
                continue; }

            // If this index has a value (Isn't null), then we want to take that as the new selected index.
            SetCurrentStationIndex(i);
            return;
        }

        // If we have reached here, we have not found a new index, so we start at the start of the list.
        // If we reach our original currentIndex, then we clearly have no other options.  
        for(int i = 0; i < currentIndex; i++)
        {
            if (!this.stationIndexes[i].HasValue) 
            {
                continue; }

            // If this index has a value (Isn't null), then we want to take that as the new selected index.
            SetCurrentStationIndex(i);
            return;
        }
    }

    private void DecrementIndex()
    {
        // Get the current index
        FindIndexInStationIndexesOfCurrentSelectedStationIndex(out int currentIndex);

        for (int i = currentIndex - 1; i > 0; i--)
        {
            if(i < 0) {
                Debug.Log("Index is: " + i + " which is less than 0, breaking out the loop.");
                break; }
            if (!this.stationIndexes[i].HasValue)
            {
                continue;
            }
            

            // If this index has a value (Isn't null), then we want to take that as the new selected index.
            SetCurrentStationIndex(i);
            Debug.Log("Setting station index to: "+ i);

            return;
        }

        Debug.Log("Switching");

        // If we have reached here, we have not found a new index, so we start at the start of the list.
        // If we reach our original currentIndex, then we clearly have no other options.  
        for (int i = this.stationIndexes.Count -1; i > currentIndex; i--)
        {
            Debug.Log("i in the second loop is: " + i);
            if (!this.stationIndexes[i].HasValue)
            {
                continue;
            }

            // If this index has a value (Isn't null), then we want to take that as the new selected index.
            SetCurrentStationIndex(i);
            Debug.Log("Setting station index to: " + i);
            return;
        }
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
        bool input = false;
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            DecrementIndex();
            input = true;
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            IncrementIndex();
            input = true;
        }

        if (input)
        {
            FindIndexInStationIndexesOfCurrentSelectedStationIndex(out int index);
            Debug.LogWarning("Current index is: " + index);

            if (this.currentSelectedStationIndex.HasValue)
            {
                Debug.Log("Current Selected station is: " + currentSelectedStationIndex.Value.Index);
            }
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
