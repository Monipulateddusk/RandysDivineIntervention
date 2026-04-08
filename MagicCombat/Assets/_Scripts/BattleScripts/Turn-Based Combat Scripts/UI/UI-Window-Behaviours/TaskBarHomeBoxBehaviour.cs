using UnityEngine;

public class TaskBarHomeBoxBehaviour : SizableWindowBaseBehaviour
{
    [SerializeField] Vector2 sizeHeightWidth = Vector2.zero;
    RectTransform rect;
    private void OnValidate()
    {
        rect = GetComponent<RectTransform>();
        sizeHeightWidth = rect.sizeDelta;
    }
    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        sizeHeightWidth = rect.sizeDelta;
    }

    public override void ApplyMinimised(bool isEnabled)
    {
        if (isEnabled)
        {
            rect.sizeDelta = sizeHeightWidth;
        }
        else
        {
            rect.sizeDelta = Vector2.zero;

        }
    }

    public override Vector2 GetDialogueBoxSize()
    {
        return sizeHeightWidth;
    }

    public override void Resize(Vector2 newSize)
    {
        rect.sizeDelta = newSize;
    }

    public override bool DoesMoveMinimised() => false;

    public void OnWindowManagerClicked()
    {
        Debug.Log("Opening Window");
        UserInterfaceManager.Instance.GetTaskBarManager().AddWindow(UIWindowFactoryWindowType.WindowManager, new Vector2(600, 200), new Vector2(300, 400));
    }

    public void OnOptionsClick()
    {
        Debug.Log("Opening Options");
        UserInterfaceManager.Instance.GetTaskBarManager().AddWindow(UIWindowFactoryWindowType.Options, new Vector2(600, 200), new Vector2(300, 400));
    }

    public void OnShutDownClick()
    {
        Debug.Log("Shutting down");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;    
#endif
    }

    public override void SetDialogueBoxName(string newText)
    {
        throw new System.NotImplementedException();
    }
}
