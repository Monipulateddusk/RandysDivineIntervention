using UnityEngine;

public class MoveUIPrefabData : MonoBehaviour
{
    [SerializeField] private UnityEngine.UI.Button MoveUIElementClickableButton;
    [SerializeField] private TMPro.TextMeshProUGUI MoveUIElementText;

    private void Awake()
    {
        if (this.MoveUIElementClickableButton != null)
        {
            this.MoveUIElementClickableButton.onClick.AddListener(OnButtonClick);
        }
    }

    public void Initalise(string moveText)
    {
        if (this.MoveUIElementText != null)
        {
            this.MoveUIElementText.text = moveText;
        }
    }
    private void OnButtonClick()
    {
        Debug.Log("sadasd");
    }
}
