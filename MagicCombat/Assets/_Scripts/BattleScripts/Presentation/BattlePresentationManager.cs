using UnityEngine;

public class BattlePresentationManager : MonoBehaviour
{
    [SerializeField]GameObject tempVisual;
    private void Awake()
    {
        UnitSelectorManager.OnSelectionChange           += UnitSelectorManager_OnSelectionChange;
        UnitSelectorManager.OnAddStationLocationData    += UnitSelectorManager_OnAddStationLocationData;
        UnitSelectorManager.OnRemoveStationLocationData += UnitSelectorManager_OnRemoveStationLocationData;
    }


    private void OnDestroy()
    {
        UnitSelectorManager.OnSelectionChange -= UnitSelectorManager_OnSelectionChange;
    }

    private void GetUnitIndexAndBattleUnitOnStation(UnitSelectorManager.StationLocationData locationData, out UnitIndex unitIndexOnStation, out BaseBattleUnit battleUnitOnStation)
    {
        if (locationData.StationIndex == null) { unitIndexOnStation = default; battleUnitOnStation = null; return; }

        UnitIndex? unitIndex = TurnBased.BattleMediator.Instance.GetUnitIndexOnStation(locationData.StationIndex.Value);
        if (unitIndex == null) { unitIndexOnStation = default; battleUnitOnStation = null; return; }

        BaseBattleUnit battleUnit = TurnBased.BattleMediator.Instance.GetBattleUnitOfUnitIndex(unitIndex.Value);

        unitIndexOnStation = unitIndex.Value;
        battleUnitOnStation = battleUnit;
        return;
    }

    private void PositionUnitOnStation(UnitSelectorManager.StationLocationData locationData, BaseBattleUnit battleUnitOnStation)
    {
        if(battleUnitOnStation == null) { return; }
        battleUnitOnStation.transform.position = new() { x = locationData.Location.x, y = 0, z = locationData.Location.y };
    }

    private void UnitSelectorManager_OnAddStationLocationData(UnitSelectorManager.StationLocationData locationData)
    {
        GetUnitIndexAndBattleUnitOnStation(locationData, out UnitIndex unitIndex, out BaseBattleUnit battleUnitOnStation);
        PositionUnitOnStation(locationData, battleUnitOnStation);
    }

    private void UnitSelectorManager_OnSelectionChange(UnitSelectorManager.StationLocationData locationData)
    {
        if (tempVisual != null)
        {
            tempVisual.transform.position = new(locationData.Location.x, 0, locationData.Location.y);
        }

    }

    private void UnitSelectorManager_OnRemoveStationLocationData(UnitSelectorManager.StationLocationData locationData)
    {
    }
}
