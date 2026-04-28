using UnityEngine;

namespace TurnBased.UI
{
    public class InspectionTargetUIPrefab : MonoBehaviour
    {
        public event System.Action<InspectionTargetUIPrefab, bool> OnButtonClicked;

        [SerializeField, Tooltip("Assign with the Button Component on 'TargetSelectionObject'")]            private UnityEngine.UI.Button   TargetUIElementClickableButton;
        [SerializeField, Tooltip("Assign with the TextMeshProUGUI component child to: 'TargetName'")]       private TMPro.TextMeshProUGUI   TargetNameUIElementText;
        [SerializeField, Tooltip("Assign with the UnitHealthDisplayUI Component on 'UnitHealthBuffer'")]    private UnitHealthDisplayUI     UnitHealthDisplayUI;

        private System.Collections.Generic.List<StationIndex> correlatingTarget;


        private bool isButtonClicked = false;

        public bool IsButtonClicked
        {
            get { return this.isButtonClicked; }
            set
            {
                this.isButtonClicked = value;

                /*  Visual feedback for the button being selected or deselected.    */
                OnPressed(isButtonClicked);

                if (this.isButtonClicked)
                {
                    OnButtonClicked?.Invoke(this, this.isButtonClicked);
                }
            }
        }

        private void Awake()
        {
            if (this.TargetUIElementClickableButton != null)
            {
                this.TargetUIElementClickableButton.onClick.AddListener(OnButtonPressed);
            }
        }
        private void OnDestroy()
        {
            if (this.TargetUIElementClickableButton != null)
            {
                this.TargetUIElementClickableButton.onClick.RemoveAllListeners();
            }
            this.OnButtonClicked = null;
        }

        private void OnPressed(bool pressed)
        {
            
        }

        public void OnPointerEnter()
        {
            UnityEngine.Debug.LogError("Pointer Enter");
        }

        public void OnPointerExit()
        {
            UnityEngine.Debug.LogError("Pointer Exit");

        }

        public void Initalise(StationIndex targetStationIndex)
        {
            if (this.TargetNameUIElementText != null && this.UnitHealthDisplayUI != null)
            {
                /*  Get the Name of the target on the station.  */
                if (!StationManager.Instance.TryGetUnitDataOnStation(targetStationIndex, out UnitData unitData))
                {
                    this.TargetNameUIElementText.text = string.Empty;
                    this.UnitHealthDisplayUI.gameObject.SetActive(false);
                    return;
                }

                /*  Get the UnitIndex of the Unit on the station for the purposes of initalising the health UI  */
                if (!StationManager.Instance.TryGetUnitIndexOnStation(targetStationIndex, out UnitIndex unitIndexOnStation))
                {
                    this.TargetNameUIElementText.text = string.Empty;
                    this.UnitHealthDisplayUI.gameObject.SetActive(false);
                    return;
                }

                this.TargetNameUIElementText.text = unitData.name;
                this.correlatingTarget = new() { targetStationIndex };

                this.UnitHealthDisplayUI.gameObject.SetActive(true);
                this.UnitHealthDisplayUI.Initalise(unitIndexOnStation);
            }
        }

        public void Initalise(System.Collections.Generic.List<StationIndex> targettedStations, string uiText)
        {
            if (this.TargetNameUIElementText != null && this.UnitHealthDisplayUI != null)
            {
                this.TargetNameUIElementText.text = uiText;
                this.correlatingTarget = targettedStations;
                this.UnitHealthDisplayUI.gameObject.SetActive(false);
            }
        }

        private void OnButtonPressed()
        {
            this.IsButtonClicked = !this.IsButtonClicked;
        }

        public void LockButtonClickedStatus(bool buttonClickedStatus)
        {
            this.TargetUIElementClickableButton.enabled = false;
            SetClickedStatus(buttonClickedStatus);
        }

        public void UnlockButtonClickedStatus(bool buttonClickedStatus)
        {
            this.TargetUIElementClickableButton.enabled = true;
            SetClickedStatus(buttonClickedStatus);
        }

        private void SetClickedStatus(bool buttonClickedStatus)
        {
            this.isButtonClicked = buttonClickedStatus;
            OnPressed(this.isButtonClicked);
        }

        public System.Collections.Generic.List<StationIndex> GetCorrelatingTarget() => this.correlatingTarget;
    }
}