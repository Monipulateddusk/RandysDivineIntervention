using TurnBased.AttackResolution;
using UnityEngine;

namespace TurnBased.UI
{
    public class InspectionMoveUIPrefab : MonoBehaviour
    {
        public event System.Action<InspectionMoveUIPrefab, bool> OnButtonClicked;

        [SerializeField, Tooltip("Assign with the Button Component on 'MoveUIElement'")]                            private UnityEngine.UI.Button MoveUIElementClickableButton;
        [SerializeField, Tooltip("Assign with the TextMeshProUGUI component child to: 'MoveName'")]                 private TMPro.TextMeshProUGUI MoveNameUIElementText;
        [SerializeField, Tooltip("Assign with the TextMeshProUGUI component child to: 'MoveDamageDescription'")]    private TMPro.TextMeshProUGUI MoveDescriptionUIElementText;

        private IBattleMove correlatingMove;


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
            if (this.MoveUIElementClickableButton != null)
            {
                this.MoveUIElementClickableButton.onClick.AddListener(OnButtonPressed);
            }
        }
        private void OnDestroy()
        {
            if (this.MoveUIElementClickableButton != null)
            {
                this.MoveUIElementClickableButton.onClick.RemoveAllListeners();
            }
            this.OnButtonClicked = null;
        }

        private void OnPressed(bool pressed)
        {
            
        }

        public void OnPointerEnter()
        {
          //  UnityEngine.Debug.LogError("Pointer Enter");
        }

        public void OnPointerExit()
        {
          //  UnityEngine.Debug.LogError("Pointer Exit");

        }
        public void Initalise(UnitIndex unitIndex, IBattleMove move)
        {
            if (this.MoveNameUIElementText != null && this.MoveDescriptionUIElementText != null)
            {
                this.correlatingMove                        = move;
                this.MoveNameUIElementText.text             = move.GetMoveName();
                this.MoveDescriptionUIElementText.text      = GetMoveDescription(unitIndex, move);
            }
        }

        private string GetMoveDescription(UnitIndex unitIndex, IBattleMove move)
        {
            return CombatDamageUtility.GetMoveDescription(unitIndex, move);
        }


        private void OnButtonPressed()
        {
            this.IsButtonClicked = !this.IsButtonClicked;
        }

        public void LockButtonClickedStatus(bool buttonClickedStatus)
        {
            this.MoveUIElementClickableButton.enabled = false;
            SetClickedStatus(buttonClickedStatus);
        }

        public void UnlockButtonClickedStatus(bool buttonClickedStatus)
        {
            this.MoveUIElementClickableButton.enabled = true;
            SetClickedStatus(buttonClickedStatus);
        }

        private void SetClickedStatus(bool buttonClickedStatus)
        {
            this.isButtonClicked = buttonClickedStatus;
            OnPressed(this.isButtonClicked);
        }

        public IBattleMove GetCorrelatingMove() => this.correlatingMove;
    }
}