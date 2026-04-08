using System;
using System.Collections.Generic;
using TurnBased;
using UnityEngine;

public class CommandUIBehaviour : MonoBehaviour
{
    [Header("Inspector Variables")]
    [SerializeField] private UnityEngine.UI.Image UnitImage;
    [SerializeField, Tooltip("Assign with the 'Health' Object in HealthBG")] private RectTransform HealthRectTransform;
    [SerializeField, Tooltip("Assign with the 'HealthValueString' Object in HealthBG")] TMPro.TextMeshProUGUI HealthText;

    [SerializeField, Tooltip("Assign with the 'CommandWrapper' Object in Command")] GameObject CommandWrapperGameObject;
    [SerializeField, Tooltip("Assign with the 'Inspection' Object in InspectionSubWindow")] GameObject InspectionGameObject;
    [SerializeField, Tooltip("Assign with the 'PopupBuffer' Object in InspectionSubWindow")] GameObject PopupBufferGameObject;
    [SerializeField, Tooltip("Assign with the 'UnitImageHealthWrapper' Object in VIew")] GameObject UnitImageHealthWrapperGameObject;
    [SerializeField, Tooltip("Assign with the 'TargetSelection' Object in VIew")] GameObject TargetSelectionGameObject;


    [Header("Prefabs")]
    [SerializeField] private GameObject MoveUIPrefab;

    private List<MoveUIPrefabData> InstanciatedMoveUIElements = new();

    enum CommandUIBehaviourStates { Default = 0, TargetSelection = 1, UnitEndTurn = 2};
    private CommandUIBehaviourStates currentState = CommandUIBehaviourStates.Default;

    private void Awake()
    {
        StationSelectorManager.OnSelectionChange += StationSelectorManager_OnSelectionChange;
    }
    private void OnDestroy()
    {
        StationSelectorManager.OnSelectionChange -= StationSelectorManager_OnSelectionChange;
    }

    private void StationSelectorManager_OnSelectionChange(StationIndex selectedStationIndex, StationIndex? deselectedStationIndex)
    {
        if(!StationManager.Instance.TryGetUnitIndexOnStation(selectedStationIndex, out UnitIndex unitIndex)) { Debug.Log("Invalid selection change!"); return; }

        if (!StationManager.Instance.TryGetBattleUnitOfIndex(unitIndex, out BaseBattleUnit bBU)) { Debug.Log("Invalid selection change!"); return; }

        SetImage(bBU);
        SetHealthValues(bBU);
        SetMoves(bBU);
    }

    private void Start()
    {

        StationSelectorManager_OnSelectionChange(StationSelectorManager.Instance.GetSelectedStationIndex(), null);
        SetState(CommandUIBehaviourStates.Default);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Keypad0))
        {
            int stateValue = (int)currentState;
            stateValue++;
            Debug.LogWarning(stateValue);
            if (stateValue > (int)CommandUIBehaviourStates.UnitEndTurn) { stateValue = 0; }

            SetState((CommandUIBehaviourStates)stateValue);
         
        }
    }



    private float GetValueNormalisation(float minimum, float maximum, float current)
    {
        return (current - minimum) / (maximum - minimum);
    }
    private void SetImage(BaseBattleUnit battleUnit)
    {
        if (this.UnitImage == null) { return; }
        this.UnitImage.sprite = battleUnit.GetBaseUnit().sprite;
        this.UnitImage.color = battleUnit.GetBaseUnit().color;
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
    private void CreateMoveUIElement(string moveName)
    {
        if(this.InstanciatedMoveUIElements == null || this.PopupBufferGameObject == null || this.UnitImageHealthWrapperGameObject == null) { return; }

        GameObject instanciatedObject = GameObject.Instantiate(this.MoveUIPrefab, this.CommandWrapperGameObject.transform);
        if (instanciatedObject != null && instanciatedObject.TryGetComponent(out MoveUIPrefabData instanciatedMoveUIData))
        {
            instanciatedMoveUIData.Initalise(moveName);
            instanciatedMoveUIData.OnButtonClicked += OnMoveButtonClick;
            this.InstanciatedMoveUIElements.Add(instanciatedMoveUIData);
        }
    }

    private void OnMoveButtonClick(MoveUIPrefabData buttonObject, bool isPressed)
    {
        foreach(MoveUIPrefabData moveButtonData in this.InstanciatedMoveUIElements)
        {
            if(moveButtonData == buttonObject) { continue; }

            moveButtonData.IsButtonClicked = false;
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

    private void SetMoves(BaseBattleUnit battleUnit)
    {
        DestroyMoveUIElements();

        foreach(IBattleMove moveAction in battleUnit.GetBaseUnit().moves)
        {
            CreateMoveUIElement(moveAction.GetMoveName());
        }
    }


    void DisableAllWindows()
    {
        if(this.InspectionGameObject == null || this.PopupBufferGameObject == null || this.UnitImageHealthWrapperGameObject == null || this.TargetSelectionGameObject == null) { return; }
        
        this.InspectionGameObject.SetActive(false);
        this.PopupBufferGameObject.SetActive(false);
        this.UnitImageHealthWrapperGameObject.SetActive(false);
        this.TargetSelectionGameObject.SetActive(false);
    }

    private void SetState(CommandUIBehaviourStates nextState)
    {
        this.currentState = nextState;
        UpdateState();
    }

    private void UpdateState()
    {
        DisableAllWindows();
        switch (this.currentState)
        {
            case CommandUIBehaviourStates.UnitEndTurn:
                Debug.LogWarning("End Turn Enable");

                this.PopupBufferGameObject.SetActive(true);
                break;
            case CommandUIBehaviourStates.TargetSelection:
                Debug.LogWarning("Target Selection Enable");

                this.InspectionGameObject.SetActive(true);
                this.TargetSelectionGameObject.SetActive(true);
                break;
            case CommandUIBehaviourStates.Default:
                Debug.LogWarning("Default Enable");


                this.InspectionGameObject.SetActive(true);
                this.UnitImageHealthWrapperGameObject.SetActive(true);
                break;

            default:
                Debug.LogWarning("Default Enable");


                this.InspectionGameObject.SetActive(true);
                this.UnitImageHealthWrapperGameObject.SetActive(true);
                break;

        }
    }
}
