using UnityEngine;

namespace TurnBased.UI
{
    public class TargetUIPrefabData : MonoBehaviour
    {
        public event System.Action<TargetUIPrefabData, bool> OnButtonClicked;

        [SerializeField, Tooltip("Assign with the Button Component on 'MoveUIElement'")]        private UnityEngine.UI.Button TargetUIElementClickableButton;
        [SerializeField, Tooltip("Assign with the TextMeshProUGUI component on 'MoveText'")]    private TMPro.TextMeshProUGUI TargetUIElementText;

        [SerializeField, Tooltip("Assign with each Shadow on this GameObject, Drag the component itself into the fields.")]
        private UnityEngine.UI.Shadow whiteShadow, blackShadow;

        private System.Collections.Generic.List<StationIndex> correlatingTarget;

        private bool isButtonClicked = false;
        public bool IsButtonClicked
        {
            get { return this.isButtonClicked; }
            set
            {
                this.isButtonClicked = value;

                /*  Visual feedback for the button being selected or deselected.    */
                OnPressed(IsButtonClicked);

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
            if (this.blackShadow == null || this.whiteShadow == null) { return; }

            if (pressed)
            {
                this.whiteShadow.effectColor = UserInterfaceUtility.COLOR_SLATEGRAY;
                this.blackShadow.effectColor = UserInterfaceUtility.COLOR_BLUEVIOLET;
            }
            else
            {
                this.whiteShadow.effectColor = Color.white;
                this.blackShadow.effectColor = Color.black;
            }
        }

        public void Initalise(StationIndex targetStationIndex)
        {
            if (this.TargetUIElementText != null)
            {
                /*  Get the Name of the target on the station.  */
                if(!StationManager.Instance.TryGetUnitDataOnStation(targetStationIndex, out UnitData unitData))
                {
                    this.TargetUIElementText.text = string.Empty;
                    return;
                }
                
                this.TargetUIElementText.text = unitData.name;
                this.correlatingTarget = new() { targetStationIndex };
            }
        }

        public void Initalise(System.Collections.Generic.List<StationIndex> targettedStations, string uiText)
        {
            if (this.TargetUIElementText != null)
            {
                this.TargetUIElementText.text = uiText;
                this.correlatingTarget = targettedStations;
            }
        }
        private void OnButtonPressed()
        {
            this.IsButtonClicked = !this.IsButtonClicked;
        }
        public System.Collections.Generic.List<StationIndex> GetCorrelatingTarget() => this.correlatingTarget;
    }
}
