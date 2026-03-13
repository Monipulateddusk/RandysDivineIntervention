using System.Collections.Generic;
using System.Threading.Tasks;
using TurnBased;
using UnityEngine;
using UnityEngine.Assertions;

public struct DialogueBoxWidgetPair
{
    public DialogueBoxAttachment DialogueBox;
    public UITaskBarMinimisationWidget TaskBarWidget;
}

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

        private void ToggleDialogueBoxVisibility(DialogueBoxWidgetPair pair)
        {
            pair.DialogueBox.GetDialogueBoxOwner().ApplyMinimised(this.IsEnabled);
        }

        private async Task EnlargeShrinkWindow(DialogueBoxWidgetPair pair, float duration)
        {
            /*  If our target state is Shrunk, then we want to expand. Otherwise, we are already enlarged, and our target is to shrink. */
            // Item 1 of Touple: Start  Size
            // Item 2 of Touple: Target Size
            (Vector2, Vector2) sizes        = this.TargetState == WindowAnimationState.Shrunk ? (this.Size, Vector2.zero) : (Vector2.zero, this.Size);
            (Vector2, Vector2) positions = this.TargetState == WindowAnimationState.Shrunk ? (this.Position, pair.TaskBarWidget.transform.position) : (pair.TaskBarWidget.transform.position, this.Position);

            float startTime = Time.time;
            while (Time.time < startTime + duration)
            {
                float t = (Time.time - startTime) / duration;
                pair.DialogueBox.GetDialogueBoxOwner().ResizeDialogueBox(new Vector2(Mathf.Lerp(sizes.Item1.x, sizes.Item2.x, t), Mathf.Lerp(sizes.Item1.y, sizes.Item2.y, t)));
                pair.DialogueBox.GetDialogueBoxOwner().transform.position = new Vector2(Mathf.Lerp(positions.Item1.x, positions.Item2.x, t), Mathf.Lerp(positions.Item1.y, positions.Item2.y, t));
                await Task.Yield();
            }
        }

        public async Task PlayEnlargeShrinkAnimation(DialogueBoxWidgetPair pair, float duration)
        {
            if (this.isAnimating) { return; }
            this.isAnimating = true;

            await EnlargeShrinkWindow(pair, duration);
            Debug.Log("Done enlarge Shrinking");

            ToggleEnabled();
            Debug.Log("Done disabling enabling");


            ToggleDialogueBoxVisibility(pair);

            Debug.Log("Done toggling visibility");

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

    public DialogueBoxWidgetPair WindowPair;

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

    private RectTransform WindowGridTransform;
    private RectTransform ScreenElementsTransform;

    private Dictionary<int, WindowData> WindowDataDict = new();

    public UITaskBarManager(RectTransform taskBarHomeBoxTransform, RectTransform windowGridTransform, RectTransform screenElementsTransform, UICollection_SO uiData)
    {
        this.TaskbarHomeBoxTransform = taskBarHomeBoxTransform;
        this.WindowGridTransform = windowGridTransform;
        this.ScreenElementsTransform = screenElementsTransform;
        this.UI_PrefabData = uiData;

        CreateWindow(WindowType.DialogueBox, 0, new Vector2(500, 500), new Vector2(300, 400));
        CreateWindow(WindowType.DialogueBox,1, new Vector2(1000, 500), new Vector2(300, 400));
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
            CreateWindow(WindowType.TurnOrderWindow, 3, new Vector2(700, 700), new Vector2(300, 400));
        }
        
    }

    public void CreateWindow(WindowType windowType, int index, Vector2 position, Vector2 size)
    {
        /*  Store the data of this Window.  */
        if (!WindowDataDict.ContainsKey(index))
        {
            DialogueBoxWidgetPair createdWindowPair = UIWindowFactory.CreateWindow(windowType, UI_PrefabData, ScreenElementsTransform, WindowGridTransform, index, position, size);

            WindowDataDict.Add(index, new WindowData()
            {
                WindowPair = createdWindowPair,

                WindowAnimData = new WindowData.WindowAnimationData(position, size)

            });
        }
    }

    public async Task OnMinimiseClicked(int index)
    {
        if (WindowDataDict.ContainsKey(index))
        {
            await UIWindowFactory.MinimiseWindow(WindowDataDict[index], EXPAND_SHRINK_TIMER);
        }
    }

    public void OnClosedClicked(int index)
    {
        if (WindowDataDict.ContainsKey(index))
        {
            UIWindowFactory.DestroyWindow(WindowDataDict[index]);

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
