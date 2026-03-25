using UnityEngine;

public class MoveUIPrefabData : MonoBehaviour
{
    [SerializeField, Tooltip("Assign with the Button Component on 'MoveUIElement'")] private UnityEngine.UI.Button MoveUIElementClickableButton;
    [SerializeField, Tooltip("Assign with the TextMeshProUGUI component on 'MoveText'")] private TMPro.TextMeshProUGUI MoveUIElementText;

    [SerializeField, Tooltip("Assign with each Shadow on this GameObject, Drag the component itself into the fields.")] private UnityEngine.UI.Shadow whiteShadow, blackShadow;
    Color COLOR_SLATEGRAY = new() { r = 0.3551086f, b = 0.5660378f, g = 0.5036566f, a = 1f };
    Color COLOR_BLUEVIOLET = new() { r = 0.25f, b = 1f, g = 0.3299609f, a = 1f};

    private bool isButtonClicked = false;
    public event System.Action<MoveUIPrefabData, bool> OnButtonClicked;
    public bool IsButtonClicked {  get { return this.isButtonClicked; } set
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
        if (this.MoveUIElementClickableButton != null)
        {
            this.MoveUIElementClickableButton.onClick.AddListener(OnButtonPressed);
        }
    }
    private void OnDestroy()
    {
        this.MoveUIElementClickableButton.onClick.RemoveAllListeners();
        this.OnButtonClicked = null;
    }

    private void OnPressed(bool pressed)
    {
        if(this.blackShadow == null || this.whiteShadow == null) { return; }    

        if (pressed)
        {
            this.whiteShadow.effectColor = COLOR_SLATEGRAY;
            this.blackShadow.effectColor = COLOR_BLUEVIOLET;
        }
        else
        {
            this.whiteShadow.effectColor = Color.white;
            this.blackShadow.effectColor = Color.black;
        }
    }

    public void Initalise(string moveText)
    {
        if (this.MoveUIElementText != null)
        {
            this.MoveUIElementText.text = moveText;
        }
    }
    private void OnButtonPressed()
    {
        this.IsButtonClicked = !this.IsButtonClicked;
    }

}
