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
    private void Start()
    {
        StationSelectorManager_OnSelectionChange(StationSelectorManager.Instance.GetSelectedStationIndex(), null);
    }

    private void OnDestroy()
    {
        StationManager.OnDeployUnit                 -= StationManager_OnDeployUnit;
        StationSelectorManager.OnSelectionChange -= StationSelectorManager_OnSelectionChange;
    }

    private void StationManager_OnDeployUnit(StationIndex stationIndex, UnitIndex deployUnitIndex, UnitIndex? recallUnitIndex)
    {
        /*  Get the station  and the BaseBattleUnit */
        if (!StationManager.Instance.TryGetStationOfStationIndex(stationIndex, out Station stationOfStationIndex)){ return; }

        if(!StationManager.Instance.TryGetBattleUnitOfIndex(deployUnitIndex, out BaseBattleUnit battleUnit)) {  return; }

        battleUnit.transform.position = stationOfStationIndex.Position;
    }

    private void StationSelectorManager_OnSelectionChange(StationIndex selectedStationIndex, StationIndex? deselectedStationIndex)
    {
        StationManagerUtilities.GetUnitIndexAndBattleUnitOnStation(selectedStationIndex, out UnitIndex unitIndexOnStation, out BaseBattleUnit battleUnitOnStation);

        if (tempVisual != null)
        {
            tempVisual.transform.position = battleUnitOnStation.transform.position;
        }
    }
}
