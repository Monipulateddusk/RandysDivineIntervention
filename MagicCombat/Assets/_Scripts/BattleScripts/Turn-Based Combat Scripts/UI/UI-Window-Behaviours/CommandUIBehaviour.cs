using System.Collections.Generic;
using TurnBased;
using UnityEngine;

public class CommandUIBehaviour : MonoBehaviour
{
    [Header("Inspector Variables")]
    [SerializeField] private UnityEngine.UI.Image UnitImage;
    [SerializeField, Tooltip("Assign with the 'Health' Object in HealthBG")] private RectTransform HealthRectTransform;
    [SerializeField, Tooltip("Assign with the 'HealthValueString' Object in HealthBG")] TMPro.TextMeshProUGUI HealthText;
    [SerializeField, Tooltip("Assign with the 'CommandWrapper' Object in Command")] RectTransform CommandWrapperTransform;

    [Header("Prefabs")]
    [SerializeField] private GameObject MoveUIPrefab;

    private List<MoveUIPrefabData> InstanciatedMoveUIElements = new();

    private void Awake()
    {
        UnitSelectorManager.OnSelectionChange += UnitSelectorManager_OnSelectionChange;
    }

    private void Start()
    {
        if (UnitSelectorManager.Instance.GetCurrentStationIndexStationLocationData().HasValue)
        {
            UnitSelectorManager_OnSelectionChange(UnitSelectorManager.Instance.GetCurrentStationIndexStationLocationData().Value);
        }
    }

    private void OnDestroy()
    {
        UnitSelectorManager.OnSelectionChange -= UnitSelectorManager_OnSelectionChange;
    }

    private float GetValueNormalisation(float minimum, float maximum, float current)
    {
        return (current - minimum) / (maximum - minimum);
    }
    private void SetHealthValues(BaseBattleUnit battleUnit)
    {
        if(this.HealthRectTransform != null && this.HealthText != null)
        {
            int currentHealth = battleUnit.GetHealthComponent().GetHealth();
            int maximumHealth = battleUnit.GetBaseUnit().maxHP;

            float value = GetValueNormalisation(minimum: 0, maximum: maximumHealth, current: currentHealth);

            this.HealthRectTransform.localScale = new(value, 1);
            this.HealthText.text = currentHealth.ToString() + "/" + maximumHealth.ToString();
        }
    }
    private void DestroyMoveUIElements()
    {
        if (this.InstanciatedMoveUIElements == null) { return; }

        foreach (MoveUIPrefabData obj in this.InstanciatedMoveUIElements)
        {
            Destroy(obj.gameObject);
        }
        this.InstanciatedMoveUIElements.Clear();
    }
    private void CreateMoveUIElement(string moveName)
    {
        if(this.InstanciatedMoveUIElements == null || this.MoveUIPrefab == null || this.CommandWrapperTransform == null) { return; }

        GameObject instanciatedObject = GameObject.Instantiate(this.MoveUIPrefab, this.CommandWrapperTransform.transform);
        if (instanciatedObject != null && instanciatedObject.TryGetComponent(out MoveUIPrefabData instanciatedMoveUIData))
        {
            instanciatedMoveUIData.Initalise(moveName);
            this.InstanciatedMoveUIElements.Add(instanciatedMoveUIData);
        }
    }

    private void SetMoves(BaseBattleUnit battleUnit)
    {
        DestroyMoveUIElements();

        foreach(IBattleMoveAction moveAction in battleUnit.GetBaseUnit().moves)
        {
            CreateMoveUIElement(moveAction.GetMoveName());
        }
    }

    private void UnitSelectorManager_OnSelectionChange(UnitSelectorManager.StationLocationData stationData)
    {
        if(stationData.StationIndex == null) { return; }

        UnitIndex? unitIndex = BattleMediator.Instance.GetUnitIndexOnStation(stationData.StationIndex.Value);
        if(unitIndex == null) { return; }

        BaseBattleUnit bBU = BattleMediator.Instance.GetBattleUnitOfUnitIndex(unitIndex.Value);

        if(this.UnitImage == null) { return; }
        this.UnitImage.sprite = bBU.GetBaseUnit().sprite;
        this.UnitImage.color = bBU.GetBaseUnit().color;

        SetHealthValues(bBU);
        SetMoves(bBU);
    }
}
