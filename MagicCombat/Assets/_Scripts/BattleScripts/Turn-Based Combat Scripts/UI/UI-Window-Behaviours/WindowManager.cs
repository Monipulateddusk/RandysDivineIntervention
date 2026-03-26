using UnityEngine;

public class WindowManager : MonoBehaviour
{
    public void OnTurnOrderButtonClicked()
    {
        UserInterfaceManager.Instance.GetTaskBarManager().AddWindow(WindowType.TurnOrderWindow, new Vector2(600, 200), new Vector2(300, 400));
    }
    public void OnInspectUnitButtonClicked()
    {
        UserInterfaceManager.Instance.GetTaskBarManager().AddWindow(WindowType.Inspection, new Vector2(600, 200), new Vector2(300, 400));
    }
    public void OnCameraControllerButtonClicked()
    {
        UserInterfaceManager.Instance.GetTaskBarManager().AddWindow(WindowType.DialogueBox, new Vector2(600, 200), new Vector2(300, 400));
    }
}
