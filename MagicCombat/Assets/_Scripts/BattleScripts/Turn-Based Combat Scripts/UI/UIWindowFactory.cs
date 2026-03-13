using System.Drawing;
using System;
using UnityEngine;
using UnityEngine.UIElements;

public enum WindowType
{
    TurnOrderWindow,
    DialogueBox,
    Inspection
}
public static class UIWindowFactory
{
    private static MinimisableUI CreateDialogueBox(UICollection_SO uiPrefabData, ref RectTransform parent, int index, Vector2 position, Vector2 size)
    {
        /*  Create the Dialogue box from the Prefab and assign it's position.   */
        GameObject gO = GameObject.Instantiate(uiPrefabData.DialogueBoxPrefab, parent);
        UnityUIUtility.SetRectPosition(gO.GetComponent<RectTransform>(), position);
        
        /*  Assign it's minimisable information and Size.    */
        if (gO != null && gO.TryGetComponent(out DialogueBoxBehaviour dBB))
        {
            dBB.ResizeDialogueBox(size);
            gO.GetComponent<MinimisableUI>().SetMinimisableIndex(index);
            return dBB;
        }
        else
        {
            return null;
        }
    }

    public static MinimisableUI CreateWindow(WindowType windowType, UICollection_SO uiPrefabData, ref RectTransform parent, int index, Vector2 position, Vector2 size)
    {
        switch (windowType)
        {
            case WindowType.TurnOrderWindow:

                return null;
            case WindowType.DialogueBox:
                return CreateDialogueBox(uiPrefabData, ref parent, index, position, size);
            case WindowType.Inspection:
                return null;
            default:

                return null;
        }
    }
}
