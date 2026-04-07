using System;
using UnityEngine;

public class BattlePresentationManager : MonoBehaviour
{
    [SerializeField] GameObject tempVisual;
    private void Awake()
    {
        StationManager.OnDeployUnit                 += StationManager_OnDeployUnit;
        StationSelectorManager.OnSelectionChange    += StationSelectorManager_OnSelectionChange;
    }

    private void OnDestroy()
    {
        StationManager.OnDeployUnit                 -= StationManager_OnDeployUnit;
        StationSelectorManager.OnSelectionChange    -= StationSelectorManager_OnSelectionChange;
    }

    private void StationManager_OnDeployUnit(Station station, BaseBattleUnit deployedUnit, BaseBattleUnit recalledUnit)
    {
        /*  Confirm that the station and the Deployed Unit are valid.   */
        bool validStation = StationManager.Instance.IsStationValid(station);
        if (!validStation) { return; }

        bool validUnit = StationManager.Instance.IsUnitValid(deployedUnit);
        if (!validUnit) { return; }

        /*  Assign the Unit to it's station position.   */
        deployedUnit.transform.position = station.Position;
    }

    private void StationSelectorManager_OnSelectionChange(StationIndex selectedStationIndex, StationIndex deselectedStationIndex)
    {
        StationManagerUtilities.GetUnitIndexAndBattleUnitOnStation(selectedStationIndex, out UnitIndex unitIndexOnStation, out BaseBattleUnit battleUnitOnStation);
    }

}
