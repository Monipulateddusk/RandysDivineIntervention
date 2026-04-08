using TurnBased;
using UnityEngine;

public class SelectionWindowBehaviour : MonoBehaviour
{
    [Header("Inspector Variables")]
    [SerializeField, Tooltip("Supply this field with 'CameraTab' in Tabs")] UnityEngine.UI.Button   CameraTabButton;
    [SerializeField, Tooltip("Supply this field with 'UnitTab' in Tabs")]   UnityEngine.UI.Button   UnitTabButton;

    [SerializeField, Tooltip("Supply this field with 'ButtonBG' in LeftButton") ]   UnityEngine.UI.Button   LeftButton;
    [SerializeField, Tooltip("Supply this field with 'ButtonBG' in RightButton")]   UnityEngine.UI.Button   RightButton;
    [SerializeField, Tooltip("Supply this field with 'TextElement' in Window")]     TMPro.TextMeshProUGUI   TextElement;

    SelectionUIBehaviourState currentSelectionState;

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
        SetState(SelectionUIBehaviourState.Camera);
    }
    void SetState(SelectionUIBehaviourState newState)
    {
        currentSelectionState = newState;
        UpdateState();
    }
    
    void UpdateState()
    {
        switch (currentSelectionState)
        {
            case SelectionUIBehaviourState.Camera:
                this.CameraTabButton.interactable   = false;
                this.UnitTabButton.interactable     = true;
                break;

            case SelectionUIBehaviourState.Units:
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
        if(this.currentSelectionState == SelectionUIBehaviourState.Units)
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
        if(!StationManager.Instance.TryGetUnitDataOnStation(StationSelectorManager.Instance.GetSelectedStationIndex(), out UnitData unitData)) { return; }

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

    void OnCameraTabClick()     { SetState(SelectionUIBehaviourState.Camera);    }
    void OnUnitTabClick()       { SetState(SelectionUIBehaviourState.Units);     }
    
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
            case SelectionUIBehaviourState.Units:
                StationSelectorManager.Instance.DecrementIndex();
                break;
            case SelectionUIBehaviourState.Camera:
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
            case SelectionUIBehaviourState.Units:
                StationSelectorManager.Instance.IncrementIndex();
                break;
            case SelectionUIBehaviourState.Camera:
                await CameraController.Instance.IncrementCameraIndex();
                break;
            default:
                break;
        }
        EnableButtons();
        UpdateState();
    }
}
