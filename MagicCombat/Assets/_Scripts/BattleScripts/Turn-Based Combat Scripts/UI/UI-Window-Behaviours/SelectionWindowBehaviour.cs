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
        if(this.currentSelectionState == SelectionWindowState.Units)
        {
            SetTextElementUnits();
        }
        else
        {
           SetTextElementCamera();
        }
    }

    private void SetTextElementUnits()
    {
        /*  Default declaration if values are invalid when we retrieve them.    */
        SetTextElementText("Current Unit Selected: Unit");

        /*  Retrieve the selected Unit from the StationSelectorManager. Convert the Selected Station Index to UnitIndex.    */
        StationIndex? selectedStationIndex = StationSelectorManager.Instance.GetSelectedStationUnitIndex();
        if (!selectedStationIndex.HasValue)
        {
            return;
        }
        
        /*  If the Station is valid, retrieve the Unit on the station.  */
        if(!StationManager.Instance.GetUnitIndexOnStation(selectedStationIndex.Value, out UnitIndex selectedUnitIndex)) { return; }
        
        if(!StationManager.Instance.GetBattleUnitOfIndex(selectedUnitIndex, out BaseBattleUnit bBU)) { return; }
        UnitData unitData = bBU.GetBaseUnit();
        if (unitData == null) { return; }
        SetTextElementText("Current Unit Selected: " + unitData.name);
    }
    private void SetTextElementCamera()
    {
        string numberText = CameraController.Instance.CurrentCameraIndex.ToString();

        string text = "Current Camera Selected: " + numberText;

        this.TextElement.text = text;
    }

    void SetTextElementText(string text)
    {
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
                StationSelectorManager.Instance.DecrementIndex();
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
                StationSelectorManager.Instance.IncrementIndex();
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
