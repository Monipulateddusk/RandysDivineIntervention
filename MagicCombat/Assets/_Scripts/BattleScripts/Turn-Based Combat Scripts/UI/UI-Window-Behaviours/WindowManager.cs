using UnityEngine;

public class WindowManager : MonoBehaviour
{
    public void OnTurnOrderButtonClicked()
    {
        UserInterfaceManager.Instance.GetTaskBarManager().AddWindow(UIWindowFactoryWindowType.TurnOrderWindow, new Vector2(600, 200), new Vector2(300, 400));
    }
    public void OnInspectUnitButtonClicked()
    {
        UserInterfaceManager.Instance.GetTaskBarManager().AddWindow(UIWindowFactoryWindowType.Inspection, new Vector2(600, 200), new Vector2(300, 400));
    }
    public void OnCameraControllerButtonClicked()
    {
        UserInterfaceManager.Instance.GetTaskBarManager().AddWindow(UIWindowFactoryWindowType.Selector, new Vector2(600, 200), new Vector2(300, 400));
    }
}
