using System;
using System.Collections.Generic;
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
    }

    /// <summary>
    /// Find the index in the list where our currently selected station is stored. Allows us to select the next station over.
    /// Returns 0 if the index is somehow not found.    
    /// </summary>
    /// <returns></returns>
    private int FindIndexInStationIndexesOfCurrentSelectedStationIndex()
    {
        for (int i = 0; i < this.stationIndexes.Count; i++)
        {
            if (this.stationIndexes[i]?.Index == this.currentSelectedStationIndex?.Index)
            {
                return i;
            }
        }
        return 0;
    }
    private int WrapIndexWithinListBounds(int index)
    {
        if(index > this.stationIndexes.Count - 1)
        {
            return 0;
        }
        else if(index < 0)
        {
            return (this.stationIndexes.Count - 1);
        }
        else { return index; }
    }

    private void SetCurrentStationIndex(int index)
    {
        if (index > 0)
        {
            this.currentSelectedStationIndex = this.stationIndexes[index];
        }
    }

    private void SelectStationLeft()
    {
        if (this.stationIndexes == null && this.stationIndexes.Count != 0) { return; }

        int index = FindIndexInStationIndexesOfCurrentSelectedStationIndex();
        index--;
        int wrappedIndex = WrapIndexWithinListBounds(index);

        SetCurrentStationIndex(wrappedIndex);
    }
    private void SelectStationRight()
    {
        if (this.stationIndexes == null && this.stationIndexes.Count != 0) { return; }

        int index = FindIndexInStationIndexesOfCurrentSelectedStationIndex();
        index++;

        int wrappedIndex = WrapIndexWithinListBounds(index);

        SetCurrentStationIndex(wrappedIndex);
    }

    private void Update()
    {
        bool input = false;
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            SelectStationLeft();
            input = true;
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            SelectStationRight();
            input = true;
        }
        if (input)
        {
            if (currentSelectedStationIndex != null)
            {
                Debug.Log("Current Station count is: " + this.stationIndexes.Count  );
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
