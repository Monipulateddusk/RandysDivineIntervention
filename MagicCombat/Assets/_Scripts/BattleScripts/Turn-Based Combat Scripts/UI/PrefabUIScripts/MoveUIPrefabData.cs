using UnityEngine;

namespace TurnBased.UI
{
    public class MoveUIPrefabData : MonoBehaviour
    {
        public event System.Action<MoveUIPrefabData, bool> OnButtonClicked;

        [SerializeField, Tooltip("Assign with the Button Component on 'MoveUIElement'")] private UnityEngine.UI.Button MoveUIElementClickableButton;
        [SerializeField, Tooltip("Assign with the TextMeshProUGUI component on 'MoveText'")] private TMPro.TextMeshProUGUI MoveUIElementText;

        [SerializeField, Tooltip("Assign with each Shadow on this GameObject, Drag the component itself into the fields.")]
        private UnityEngine.UI.Shadow whiteShadow, blackShadow;
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

        public void Initalise(IBattleMove move)
        {
            if (this.MoveUIElementText != null)
            {
                this.correlatingMove = move;    
                this.MoveUIElementText.text = move.GetMoveName();
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