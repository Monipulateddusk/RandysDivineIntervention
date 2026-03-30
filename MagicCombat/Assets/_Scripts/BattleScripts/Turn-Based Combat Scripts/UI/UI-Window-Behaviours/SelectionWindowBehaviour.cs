using TurnBased;
using UnityEngine;

public class SelectionWindowBehaviour : MonoBehaviour
{
    enum SelectionWindowState { Camera = 0, Units = 1}

    [Header("Inspector Variables")]
    [SerializeField, Tooltip("Supply this field with 'CameraTab' in Tabs")] UnityEngine.UI.Button   CameraTabButton;
    [SerializeField, Tooltip("Supply this field with 'UnitTab' in Tabs")]   UnityEngine.UI.Button   UnitTabButton;

    [SerializeField, Tooltip("Supply this field with 'ButtonBG' in LeftButton") ]   UnityEngine.UI.Button   LeftButton;
    [SerializeField, Tooltip("Supply this field with 'ButtonBG' in RightButton")]   UnityEngine.UI.Button   RightButton;
    [SerializeField, Tooltip("Supply this field with 'TextElement' in Window")]     TMPro.TextMeshProUGUI   TextElement;

    SelectionWindowState currentSelectionState;

    private void Awake()
    {
        this.CameraTabButton.onClick.AddListener(OnCameraTabClick);
        this.UnitTabButton.onClick.AddListener(OnUnitTabClick);

        this.LeftButton.onClick.AddListener(LeftButtonClickEvent);
        this.RightButton.onClick.AddListener(RightButtonClickEvent);
    }

    private void OnDestroy()
    {
        this.CameraTabButton.onClick.RemoveAllListeners();
        this.UnitTabButton.onClick.RemoveAllListeners();

        this.LeftButton.onClick.RemoveAllListeners();
        this.RightButton.onClick.RemoveAllListeners();
    }

    private void Start()
    {
        SetState(SelectionWindowState.Camera);
    }
    void SetState(SelectionWindowState newState)
    {
        currentSelectionState = newState;
        UpdateState();
    }
    
    void UpdateState()
    {
        switch (currentSelectionState)
        {
            case SelectionWindowState.Camera:
                this.CameraTabButton.interactable   = false;
                this.UnitTabButton.interactable     = true;
                break;

            case SelectionWindowState.Units:
                this.CameraTabButton.interactable   = true;
                this.UnitTabButton.interactable     = false;
                break;

            default:

                break;  
        }

        SetTextElementText();
    }

    private void SetTextElementText()
    {
        string stateText  = "Camera";
        string numberText = CameraController.Instance.CurrentCameraIndex.ToString();

        /*  Override the strings if we are in Unit Selection.   */
        if (currentSelectionState == SelectionWindowState.Units)
        {
            stateText   = "Unit";
            numberText  = "Unit"; // Default in the event our retrieval of data fails.
            UnitIndex? unitIndex = UnitSelectorManager.Instance.GetSelectedStationUnit();
            if(unitIndex != null) {
                UnitData unitData = BattleMediator.Instance.GetBattleUnitOfUnitIndex(unitIndex.Value).GetBaseUnit();
                if (unitData == null) { return; }

                numberText = unitData.name;
            } 
        }

        string text = "Current " + stateText + " Selected: " + numberText;

        this.TextElement.text = text;
    }


    void OnCameraTabClick()     { SetState(SelectionWindowState.Camera);    }
    void OnUnitTabClick()       { SetState(SelectionWindowState.Units);     }
    
    void DisableButtons()
    {
        this.RightButton.interactable = false;
        this.LeftButton.interactable = false;
    }
    void EnableButtons()
    {
        this.RightButton.interactable = true;
        this.LeftButton.interactable = true;
    }
    void LeftButtonClickEvent()
    {
        _ = OnLeftButtonClick();
    }

    async System.Threading.Tasks.Task OnLeftButtonClick()
    {
        DisableButtons();
        switch (currentSelectionState)
        {
            case SelectionWindowState.Units:
                UnitSelectorManager.Instance.DecrementIndex();
                break;
            case SelectionWindowState.Camera:
                await CameraController.Instance.DecrementCameraIndex();
                break;
            default:
                break;
        }
        EnableButtons();
        UpdateState();
    }
    void RightButtonClickEvent()
    {
        _ = OnRightButtonClick();
    }

    async System.Threading.Tasks.Task OnRightButtonClick()
    {
        DisableButtons(); 
        switch (currentSelectionState)
        {
            case SelectionWindowState.Units:
                UnitSelectorManager.Instance.IncrementIndex();
                break;
            case SelectionWindowState.Camera:
                await CameraController.Instance.IncrementCameraIndex();
                break;
            default:
                break;
        }
        EnableButtons();
        UpdateState();
    }
}
