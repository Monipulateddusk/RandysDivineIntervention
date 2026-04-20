using UnityEngine;

public class BattlePresentationManager : MonoBehaviour
{
    private static BattlePresentationManager instance;
    public  static BattlePresentationManager Instance
    {
        get 
        { 
            return instance;
        }
        set
        {
            if (instance == null)
            {
                instance = value;
            }
        }
    }

    [SerializeField] GameObject tempVisual;
    const float MOVEMENT_DURATION = 3.0f;

    private void Awake()
    {
        Instance = this;

        StationManager.OnDeployUnit                 += StationManager_OnDeployUnit;
        StationSelectorManager.OnSelectionChange    += StationSelectorManager_OnSelectionChange;
    }

    private void Start()
    {
        StationSelectorManager_OnSelectionChange(StationSelectorManager.Instance.GetSelectedStationIndex(), null);
    }

    private void OnDestroy()
    {
        if (Instance != null && Instance == this)
        {
            Instance = null;
        }
        StationManager.OnDeployUnit                 -= StationManager_OnDeployUnit;
        StationSelectorManager.OnSelectionChange    -= StationSelectorManager_OnSelectionChange;
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
        StationManagerUtilities.GetBattleUnitOnStation(selectedStationIndex, out BaseBattleUnit battleUnitOnStation);

        if (tempVisual != null)
        {
            tempVisual.transform.position = battleUnitOnStation.transform.position;
        }
    }

    public async System.Threading.Tasks.Task MoveUnitToTarget(StationIndex stationOfTheSourceUnit, StationIndex targetStation)
    {
        /*  Get the baseBattleUnit of the source and Target */
        if(!StationManager.Instance.TryGetBaseBattleUnitOnStation(stationOfTheSourceUnit,   out BaseBattleUnit sourceUnit)) { return; }
        if(!StationManager.Instance.TryGetBaseBattleUnitOnStation(targetStation,            out BaseBattleUnit targetUnit)) { return; }

        float startTime = Time.time;

        Quaternion sourceLookRotation =  Quaternion.LookRotation((targetUnit.transform.position - sourceUnit.transform.position).normalized);

        while (Time.time < startTime + MOVEMENT_DURATION)
        {
            Vector3 currentSourcePosition   = sourceUnit.transform.position; 
            Vector3 currentTargetPosition   = targetUnit.transform.position;
            float t = (Time.time - startTime) / MOVEMENT_DURATION;

            /*  Next incremental rotation and position values.  */
            Vector3 pos = new(
                Mathf.Lerp(currentSourcePosition.x, currentTargetPosition.x, t),
                Mathf.Lerp(currentSourcePosition.y, currentTargetPosition.y, t),
                Mathf.Lerp(currentSourcePosition.z, currentTargetPosition.z, t)
                            );

            sourceUnit.transform.SetPositionAndRotation(pos, sourceLookRotation);

            await System.Threading.Tasks.Task.Yield();
        }
    }

    public async System.Threading.Tasks.Task MoveUnitToStation(StationIndex stationOfTheSourceUnit)
    {
        /*  Get the baseBattleUnit of the source and Target */
        if (!StationManager.Instance.TryGetBaseBattleUnitOnStation(stationOfTheSourceUnit,  out BaseBattleUnit sourceUnit)) { return; }
        if (!StationManager.Instance.TryGetStationOfStationIndex(stationOfTheSourceUnit,    out Station sourceStation)) { return; }

        float startTime = Time.time;

        Quaternion sourceLookRotation = Quaternion.identity;

        while (Time.time < startTime + MOVEMENT_DURATION)
        {
            Vector3 currentSourcePosition = sourceUnit.transform.position;
            float t = (Time.time - startTime) / MOVEMENT_DURATION;

            /*  Next incremental rotation and position values.  */
            Vector3 pos = new(
                Mathf.Lerp(currentSourcePosition.x, sourceStation.Position.x, t),
                Mathf.Lerp(currentSourcePosition.y, sourceStation.Position.y, t),
                Mathf.Lerp(currentSourcePosition.z, sourceStation.Position.z, t)
                            );

            sourceUnit.transform.SetPositionAndRotation(pos, sourceLookRotation);

            await System.Threading.Tasks.Task.Yield();
        }
    }

}
