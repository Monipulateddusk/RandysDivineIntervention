using System.Collections.Generic;
using System.Threading.Tasks;
using TurnBased;
using TurnBased.UI;
using UnityEngine;

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
            bool doesMove = pair.DialogueBox.GetDialogueBoxOwner().DoesMoveMinimised();
            while (Time.time < startTime + duration)
            {
                float t = (Time.time - startTime) / duration;
                pair.DialogueBox.GetDialogueBoxOwner().Resize(new Vector2(Mathf.Lerp(sizes.Item1.x, sizes.Item2.x, t), Mathf.Lerp(sizes.Item1.y, sizes.Item2.y, t)));

                if (doesMove)
                {
                    pair.DialogueBox.GetDialogueBoxOwner().transform.position = new Vector2(Mathf.Lerp(positions.Item1.x, positions.Item2.x, t), Mathf.Lerp(positions.Item1.y, positions.Item2.y, t));
                }
                await Task.Yield();
            }
        }

        public async Task PlayEnlargeShrinkAnimation(DialogueBoxWidgetPair pair, float duration)
        {
            if (this.isAnimating) { return; }
            this.isAnimating = true;

            await EnlargeShrinkWindow(pair, duration);

            ToggleEnabled();

            ToggleDialogueBoxVisibility(pair);

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
    private RectTransform TaskbarHomeBoxPivotTransform;
    private RectTransform TaskbarButtonTransform;
    const float EXPAND_SHRINK_TIMER = 0.1f;
    const int MAX_WINDOW_COUNT = 20;

    /*  Minimised Window Variables and Referances   */
    private readonly UICollection_SO UI_PrefabData;

    private readonly RectTransform WindowGridTransform;
    private readonly RectTransform ScreenElementsTransform;

    private Dictionary<int, WindowData> WindowDataDict = new();

    public UITaskBarManager(RectTransform taskBarHomeBoxPivotTransform, RectTransform taskbarStartButtonTransform, RectTransform windowGridTransform, RectTransform screenElementsTransform, UICollection_SO uiData)
    {
        this.TaskbarHomeBoxPivotTransform = taskBarHomeBoxPivotTransform;
        this.TaskbarButtonTransform = taskbarStartButtonTransform;
        this.WindowGridTransform = windowGridTransform;
        this.ScreenElementsTransform = screenElementsTransform;
        this.UI_PrefabData = uiData;

        AddWindow(UIWindowFactoryWindowType.HomeStart, Vector2.zero, Vector2.zero);
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            foreach (var item in WindowDataDict.Values)
            {
                Debug.Log(item.WindowAnimData.IsEnabled);
            }
        }        
    }

    public void AddWindow(UIWindowFactoryWindowType UIWindowFactoryWindowType, Vector2 position, Vector2 size)
    {
        for (int i = 0; i < MAX_WINDOW_COUNT; i++)
        {
            if (WindowDataDict.ContainsKey(i)) { continue; }

            CreateWindow(UIWindowFactoryWindowType, i, position, size);
            return;
        }

    }

    private void CreateWindow(UIWindowFactoryWindowType UIWindowFactoryWindowType, int index, Vector2 position, Vector2 size)
    {
        /*  Store the data of this Window.  */
        if (!WindowDataDict.ContainsKey(index))
        {
            DialogueBoxWidgetPair createdWindowPair;
            if (UIWindowFactoryWindowType == UIWindowFactoryWindowType.HomeStart)
            {
                createdWindowPair = UIWindowFactory.CreateWindow(UIWindowFactoryWindowType, UI_PrefabData, TaskbarHomeBoxPivotTransform, TaskbarButtonTransform, index, position, size);
            }
            else
            {
                createdWindowPair = UIWindowFactory.CreateWindow(UIWindowFactoryWindowType, UI_PrefabData, ScreenElementsTransform, WindowGridTransform, index, position, size);
            }

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
}
