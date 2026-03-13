using UnityEngine;
using TurnBased;
using TurnBased.UI;
using System;
using System.Threading.Tasks;

public enum WindowType
{
    TurnOrderWindow,
    DialogueBox,
    Inspection
}
public static class UIWindowFactory
{

    private static GameObject CreateDialogueBoxGameObject(UICollection_SO uiPrefabData, RectTransform parent, Vector2 position)
    {
        /*  Create the Dialogue box from the Prefab and assign it's position.   */
        GameObject gO = GameObject.Instantiate(uiPrefabData.DialogueBoxPrefab, parent);
        UnityUIUtility.SetRectPosition(gO.GetComponent<RectTransform>(), position);

        return gO;
    }
    private static DialogueBoxWidgetPair? CreateDialogueBox(UICollection_SO uiPrefabData, RectTransform windowParent, RectTransform widgetParent, int index, Vector2 position, Vector2 size)
    {
        GameObject instanciatedObject = CreateDialogueBoxGameObject(uiPrefabData, windowParent, position);

        /*  Assign it's minimisable information and Size.    */
        if (instanciatedObject != null && instanciatedObject.TryGetComponent(out DialogueBoxBehaviour dBB))
        {
            dBB.ResizeDialogueBox(size);
            instanciatedObject.GetComponent<MinimisableUI>().SetMinimisableIndex(index);

            return new DialogueBoxWidgetPair() { DialogueBox = new DefaultDialogueBoxAttachment(dBB), TaskBarWidget = CreateTaskBarMinimisationWidget(uiPrefabData, widgetParent,  index) };
        }
        else
        {
            return null;
        }
    }

    private static GameObject CreateScrollableDialogueBoxGameObject(UICollection_SO uiPrefabData, RectTransform parent)
    {
        return GameObject.Instantiate(uiPrefabData.ScrollableContentPrefab, parent);
    }

    private static DialogueBoxWidgetPair? CreateScrollableDialogueBox(UICollection_SO uiPrefabData, RectTransform windowParent, RectTransform widgetParent, int index, Vector2 position, Vector2 size)
    {
        GameObject instanciatedObject = CreateDialogueBoxGameObject(uiPrefabData, windowParent, position);
        
        /*  Assign it's minimisable information and Size.    */
        if (instanciatedObject != null && instanciatedObject.TryGetComponent(out DialogueBoxBehaviour dBB))
        {
            dBB.ResizeDialogueBox(size);
            instanciatedObject.GetComponent<MinimisableUI>().SetMinimisableIndex(index);

            /*  Initalise the usual stuff for a scrollable DialogueBox  */
            /*  Create the scrollable prefab and attach it to the Content transform.    */
            // Find the Content child and instanciate the Scrollable Content prefab to it.
            GameObject instanciatedScrollable = CreateScrollableDialogueBoxGameObject(uiPrefabData, dBB.GetContentGameObjectRoot());

            if (instanciatedScrollable != null && instanciatedScrollable.TryGetComponent(out ScrollableContentPrefabData data))
            {
                return new DialogueBoxWidgetPair() { DialogueBox = new TurnBased.TurnOrderUIManager(dBB, uiPrefabData, data), TaskBarWidget = CreateTaskBarMinimisationWidget(uiPrefabData, widgetParent, index) };
            }
            else
                return null;
        }
        else
        {
            return null;
        }
    }

    private static UITaskBarMinimisationWidget CreateTaskBarMinimisationWidget(UICollection_SO uiPrefabData, RectTransform widgetParent, int index)
    {
        GameObject gO = GameObject.Instantiate(uiPrefabData.WindowMinimisationWidgetPrefab, widgetParent);
        if (gO.TryGetComponent(out UITaskBarMinimisationWidget minimisationWidget))
        {
            minimisationWidget.SetMinimisableIndex(index);
            return minimisationWidget;
        }
        return null;
    }


    public static DialogueBoxWidgetPair CreateWindow(WindowType windowType, UICollection_SO uiPrefabData, RectTransform windowParent, RectTransform widgetParent, int index, Vector2 position, Vector2 size)
    {
        DialogueBoxWidgetPair? pair = new();
        switch (windowType)
        {
            case WindowType.TurnOrderWindow:
                pair = CreateScrollableDialogueBox(uiPrefabData, windowParent, widgetParent, index, position, size);
                if(!pair.HasValue) 
                { 
                    Debug.LogError("ERROR: UNABLE TO CREATE WINDOW OF INDEX: " + index);
                }
                return pair.Value;

            case WindowType.DialogueBox:
                pair = CreateDialogueBox(uiPrefabData, windowParent, widgetParent, index, position, size);
                if (!pair.HasValue)
                {
                    Debug.LogError("ERROR: UNABLE TO CREATE WINDOW OF INDEX: " + index);
                }
                return pair.Value;

            case WindowType.Inspection:

                return new DialogueBoxWidgetPair();

            default:

                return new DialogueBoxWidgetPair();
        }
    }

    public static async Task MinimiseWindow(WindowData windowData, float expandShrinkTimer)
    {
        /*  Check to see if we are animating, if so, ABORT! */
        if (windowData.WindowAnimData.isAnimating) { return; }


        /*  Set the target state to the opposite that we are in. So if we are shrunk, we want to enlarge.   */
        windowData.WindowAnimData.ToggleAnimationState();


        /*  We want to check if we AREN'T animating, and that our target state is Enlarged. If so, then we want to save the size of the dialogue box.  */
        windowData.WindowAnimData.SavePositionAndSize
            (windowData.WindowPair.DialogueBox.GetDialogueBoxOwner().transform.position, windowData.WindowPair.DialogueBox.GetDialogueBoxOwner().GetDialogueBoxSize());

        /*  Play the animation as an asyncronous task. Only after ALL tasks are done, do we want to set isAnimating to false!   */
        await windowData.WindowAnimData.PlayEnlargeShrinkAnimation(windowData.WindowPair, expandShrinkTimer);


    }

    public static void DestroyWindow(WindowData windowData)
    {
        /*  Destroy the window and the widget.  */
        windowData.WindowPair.DialogueBox.Destroy();
        GameObject.Destroy(windowData.WindowPair.DialogueBox.GetDialogueBoxOwner().gameObject);
        GameObject.Destroy(windowData.WindowPair.TaskBarWidget.gameObject);
    }
}
