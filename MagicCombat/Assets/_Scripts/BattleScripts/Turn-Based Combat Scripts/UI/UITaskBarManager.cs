using System.Collections.Generic;
using System.Threading.Tasks;
using TurnBased.UI;
using Unity.Content;
using UnityEngine;

public struct WindowData
{
    public class WindowAnimationData
    {
        public Vector2 Position, Size;
        public enum WindowAnimationState { Shrunk, Enlarged }
        public WindowAnimationState TargetState;

        public bool IsEnabled, isAnimating;

        public WindowAnimationData(Vector2 position, Vector2 size)
        {
            this.Position = position;
            this.Size = size;

            this.IsEnabled = true;
            this.isAnimating = false;
            this.TargetState = WindowAnimationState.Enlarged;
        }

        private void ToggleEnabled()
        {
            if (this.TargetState == WindowAnimationState.Enlarged) { this.IsEnabled = true; }
            else { this.IsEnabled = false; }
            
        }

        private void ToggleDialogueBoxVisibility(DialogueBoxBehaviour dialogueBox)
        {
            dialogueBox.ApplyMinimised(this.IsEnabled);
        }

        private async Task EnlargeShrinkWindow(DialogueBoxBehaviour dialogueBox, UITaskBarMinimisationWidget taskbarWidget, float duration)
        {
            /*  If our target state is Shrunk, then we want to expand. Otherwise, we are already enlarged, and our target is to shrink. */
            // Item 1 of Touple: Start  Size
            // Item 2 of Touple: Target Size
            (Vector2, Vector2) sizes        = this.TargetState == WindowAnimationState.Shrunk ? (this.Size, Vector2.zero) : (Vector2.zero, this.Size);
            (Vector2, Vector2) positions = this.TargetState == WindowAnimationState.Shrunk ? (this.Position, taskbarWidget.transform.position) : (taskbarWidget.transform.position, this.Position);

            float startTime = Time.time;
            while (Time.time < startTime + duration)
            {
                float t = (Time.time - startTime) / duration;
                dialogueBox.ResizeDialogueBox(new Vector2(Mathf.Lerp(sizes.Item1.x, sizes.Item2.x, t), Mathf.Lerp(sizes.Item1.y, sizes.Item2.y, t)));
                dialogueBox.transform.position = new Vector2(Mathf.Lerp(positions.Item1.x, positions.Item2.x, t), Mathf.Lerp(positions.Item1.y, positions.Item2.y, t));
                await Task.Yield();
            }
        }

        public async Task PlayEnlargeShrinkAnimation(DialogueBoxBehaviour dialogueBox, UITaskBarMinimisationWidget taskbarWidget, float duration)
        {
            if (this.isAnimating) { return; }
            this.isAnimating = true;

            await EnlargeShrinkWindow(dialogueBox, taskbarWidget, duration);      

            ToggleEnabled();
            ToggleDialogueBoxVisibility(dialogueBox);
            this.isAnimating = false;
        }

        public void ToggleAnimationState()
        {
            if(this.TargetState == WindowAnimationState.Enlarged) { this.TargetState = WindowAnimationState.Shrunk; }
            else { this.TargetState = WindowAnimationState.Enlarged; }
        }

        public void SavePositionAndSize(Vector2 position,  Vector2 size)
        {
            /*  Only if we are not animating and our target state is shrinking do we want to save the position and size.    */
            if (!this.isAnimating && this.TargetState == WindowAnimationState.Shrunk)
            {
                this.Size = size;
                this.Position = position;
            }
        }
    }


    public DialogueBoxBehaviour DialogueBox;
    public UITaskBarMinimisationWidget TaskBarWidget;
    public WindowAnimationData WindowAnimData;
}

public class UITaskBarManager
{
    /*  Task Bar OS Home Variables and Referances.  */
    private RectTransform TaskbarHomeBoxTransform;
    const float MAX_TASKBAR_HOME_HEIGHT = 140;
    const float EXPAND_SHRINK_TIMER = 0.1f;
    bool isExpandingShrinking = false;

    /*  Minimised Window Variables and Referances   */
    UICollection_SO UI_PrefabData;

    private readonly RectTransform WindowGridTransform;
    private readonly RectTransform ScreenElementsTransform;

    private Dictionary<int, WindowData> WindowDataDict = new();

    private TurnBased.TurnOrderUIManager TurnOrderManagerUI;
    public UITaskBarManager(RectTransform taskBarHomeBoxTransform, RectTransform windowGridTransform, RectTransform screenElementsTransform, UICollection_SO uiData)
    {
        this.TaskbarHomeBoxTransform = taskBarHomeBoxTransform;
        this.WindowGridTransform = windowGridTransform;
        this.ScreenElementsTransform = screenElementsTransform;
        this.UI_PrefabData = uiData;

        CreateWindow(0, new Vector2(500, 500), new Vector2(300, 400));
        CreateWindow(1, new Vector2(1000, 500), new Vector2(300, 400));
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Y))
        {
            _ = OnClickStartOS();
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            foreach (var item in WindowDataDict.Values)
            {
                Debug.Log(item.WindowAnimData.IsEnabled);
            }
        }
        if (Input.GetKeyDown(KeyCode.Z))
        {
            CreateTurnOrderUIWindow();
        }
        
    }

    private void CreateTurnOrderUIWindow()
    {
        // Testing script. Get the 0-index of the windowDataDict and set one of the dialogue boxes to be the turn-order manager.
        if (WindowDataDict.ContainsKey(0))
        {
            // Find the Content child and instanciate the Scrollable Content prefab to it.
            RectTransform contentTransform = WindowDataDict[0].DialogueBox.GetContentGameObjectRoot();
            RectTransform scrollableRoot = (RectTransform)(GameObject.Instantiate(UI_PrefabData.ScrollableContentPrefab, contentTransform)).transform;
            
            if(scrollableRoot != null && scrollableRoot.gameObject.TryGetComponent(out ScrollableContentPrefabData data))
            {
                TurnOrderManagerUI = new(this.UI_PrefabData, data);
            }
        }
    }
    public void CreateWindow(int index, Vector2 position, Vector2 size)
    {
        /*  Store the data of this Window.  */
        if (!WindowDataDict.ContainsKey(index))
        {
            DialogueBoxBehaviour createdDialogueBox = CreateDialogueBoxWindow(index, position, size);
            UITaskBarMinimisationWidget taskBarWidget = CreateTaskBarMinimisationWidget(index);

            WindowDataDict.Add(index, new WindowData()
            {
                DialogueBox = createdDialogueBox,
                TaskBarWidget = taskBarWidget,

                WindowAnimData = new WindowData.WindowAnimationData(position, size)

            });

        }
    }

    DialogueBoxBehaviour CreateDialogueBoxWindow(int index, Vector2 position, Vector2 size)
    {
        GameObject gO = GameObject.Instantiate(this.UI_PrefabData.DialogueBoxPrefab, this.ScreenElementsTransform.transform);

        UnityUIUtility.SetRectPosition(gO.GetComponent<RectTransform>(), position);

        if (gO != null && gO.TryGetComponent(out DialogueBoxBehaviour dBB))
        {
            dBB.ResizeDialogueBox(size);
            gO.GetComponent<MinimisableUI>().SetMinimisableIndex(index);
            return dBB;
        }
        return null;
    }

    UITaskBarMinimisationWidget CreateTaskBarMinimisationWidget(int index)
    {
        GameObject gO = GameObject.Instantiate(this.UI_PrefabData.WindowMinimisationWidgetPrefab, this.ScreenElementsTransform.transform);
        gO.transform.SetParent(WindowGridTransform.transform);
        if(gO.TryGetComponent(out UITaskBarMinimisationWidget minimisationWidget))
        {
            minimisationWidget.SetMinimisableIndex(index);
            return minimisationWidget;
        }
        return null;
    }



    public async Task OnMinimiseClicked(int index)
    {
        if (WindowDataDict.ContainsKey(index))
        {
            /*  Get the window data and corresponding animation data for this window. Set the window to be disabled.    */
            WindowData data = WindowDataDict[index];

            /*  Check to see if we are animating, if so, ABORT! */
            if (data.WindowAnimData.isAnimating) { return; }

            /*  Set the target state to the opposite that we are in. So if we are shrunk, we want to enlarge.   */
            data.WindowAnimData.ToggleAnimationState();

            /*  We want to check if we AREN'T animating, and that our target state is Enlarged. If so, then we want to save the size of the dialogue box.  */
            data.WindowAnimData.SavePositionAndSize(data.DialogueBox.transform.position, data.DialogueBox.GetDialogueBoxSize());

            /*  Play the animation as an asyncronous task. Only after ALL tasks are done, do we want to set isAnimating to false!   */
            await data.WindowAnimData.PlayEnlargeShrinkAnimation(data.DialogueBox, data.TaskBarWidget, EXPAND_SHRINK_TIMER);
        }
    }

    public void OnClosedClicked(int index)
    {
        if (WindowDataDict.ContainsKey(index))
        {
            /*  Destroy the window and the widget.  */
            GameObject.Destroy(WindowDataDict[index].DialogueBox.gameObject);
            GameObject.Destroy(WindowDataDict[index].TaskBarWidget.gameObject);

            /*  Clear the dictionary entry. */
            WindowDataDict.Remove(index);
        }
    }

    public async Task OnClickStartOS()
    {
        if (TaskbarHomeBoxTransform != null && !isExpandingShrinking)
        {
            (float, float) homeBoxStartEnd = GetHomeBoxExpandShrinkParameters();
            await ExpandShrinkHomeBox(homeBoxStartEnd.Item1, homeBoxStartEnd.Item2);
        }
    }
    private (float, float) GetHomeBoxExpandShrinkParameters()
    {
        if (this.TaskbarHomeBoxTransform.GetTop() > 0)
        {
            return (-MAX_TASKBAR_HOME_HEIGHT, 0);
        }
        else
        {
            return (0, -MAX_TASKBAR_HOME_HEIGHT);
        }
    }

    async Task ExpandShrinkHomeBox(float startHeight, float targetHeight)
    {
        isExpandingShrinking = true;
        float startTime = Time.time;
        while (Time.time < startTime + EXPAND_SHRINK_TIMER)
        {
            float t = (Time.time - startTime) / EXPAND_SHRINK_TIMER;
            this.TaskbarHomeBoxTransform.SetTop(Mathf.Lerp(startHeight, targetHeight, t));
            await Task.Yield();
        }
    
        this.TaskbarHomeBoxTransform.SetTop(targetHeight);
        isExpandingShrinking = false;
    }
}
