using System;
using UnityEngine;

public class BattlePresentationManager : MonoBehaviour
{
    [SerializeField] GameObject tempVisual;
    private void Awake()
    {
        StationManager.OnDeployUnit += StationManager_OnDeployUnit;
        StationSelectorManager.OnSelectionChange += StationSelectorManager_OnSelectionChange;
    }

    private void OnDestroy()
    {
        StationManager.OnDeployUnit -= StationManager_OnDeployUnit;
        StationSelectorManager.OnSelectionChange -= StationSelectorManager_OnSelectionChange;
    }

    private void StationManager_OnDeployUnit(StationIndex stationIndex, UnitIndex deployUnitIndex, UnitIndex? recallUnitIndex)
    { 
        Debug.Log($"Looking up station index: {stationIndex.Index}");

        /*  Get the station  and the BaseBattleUnit */
        Station station = StationManager.Instance.GetStationOfStationIndex(stationIndex);
        bool valid = StationManager.Instance.IsStationValid(station);
        if (!valid) { return; }

        BaseBattleUnit battleUnit = StationManager.Instance.GetBattleUnitOfIndex(deployUnitIndex);
        valid = StationManager.Instance.IsUnitValid(battleUnit);
        if (!valid) { return; }

        battleUnit.transform.position = station.Position; 

    }

    private void StationSelectorManager_OnSelectionChange(StationIndex selectedStationIndex, StationIndex deselectedStationIndex)
    {
        StationManagerUtilities.GetUnitIndexAndBattleUnitOnStation(selectedStationIndex, out UnitIndex unitIndexOnStation, out BaseBattleUnit battleUnitOnStation);
    }

}
