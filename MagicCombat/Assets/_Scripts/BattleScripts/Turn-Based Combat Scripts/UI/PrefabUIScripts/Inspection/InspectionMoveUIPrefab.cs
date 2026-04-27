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
            TurnBased.AttackResolution.CombatDamageUtility.TryGetTotalValuesOfMoveFromSourceIndexToTarget(unitIndex, move, out AttackResolution.CombatDamageUtility.MoveValueAmounts totalValues);

            string descriptionStart = string.Empty;
            if (totalValues.DamageAmount > 0)
            {
                descriptionStart = $"Deals {totalValues.DamageAmount} Total Damage to {GetMoveTargetText(move)}.\n";
            }
            else if (totalValues.HealingAmount > 0 && totalValues.DamageAmount <= 0)
            {
                descriptionStart = $"Heals {GetMoveTargetText(move)} for {totalValues.DamageAmount} Health Points.\n";
            }

            return descriptionStart;    

        }

        private string GetMoveTargetText(IBattleMove move)
        {
            switch (move.GetMoveTargetType())
            {
                default:
                case MoveTarget.Self:
                    return "Itself";
                case MoveTarget.SingleEnemy:
                    return "a Single Enemy";
                case MoveTarget.SingleAlly:
                    return "a Single Ally";
                case MoveTarget.AllEnemies:
                    return "All Enemies";
                case MoveTarget.AllAllies:
                    return "All Allies";
                case MoveTarget.Area:
                    return "the Entire Area";
            }
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