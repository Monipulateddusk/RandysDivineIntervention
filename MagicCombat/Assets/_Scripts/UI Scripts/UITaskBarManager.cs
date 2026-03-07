using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

public struct WindowData
{
    public DialogueBoxBehaviour DialogueBox;
    public UITaskBarMinimisationWidget TaskBarWidget;

    public Vector2 Position, Size;

    public bool IsEnabled;
}

public class UITaskBarManager
{
    /*  Task Bar OS Home Variables and Referances.  */
    private RectTransform TaskbarHomeBoxTransform;
    const float MAX_TASKBAR_HOME_HEIGHT = 140;
    const float EXPAND_SHRINK_TIMER = 0.1f;
    bool isExpandingShrinking = false;

    /*  Minimised Window Variables and Referances   */
    private readonly GameObject WindowMinimisationPrefab;
    private readonly GameObject DialogueBoxPrefab;

    private readonly RectTransform WindowGridTransform;
    private readonly RectTransform ScreenElementsTransform;

    private Dictionary<int, WindowData> WindowDataDict = new();

    public UITaskBarManager(RectTransform taskBarHomeBoxTransform, RectTransform windowGridTransform, RectTransform screenElementsTransform, GameObject windowMinimisationPrefab, GameObject dialogueBoxPrefab)
    {
        this.TaskbarHomeBoxTransform = taskBarHomeBoxTransform;
        this.WindowGridTransform = windowGridTransform;
        this.ScreenElementsTransform = screenElementsTransform;
        this.WindowMinimisationPrefab = windowMinimisationPrefab;
        this.DialogueBoxPrefab = dialogueBoxPrefab;

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
                Debug.Log(item.IsEnabled);
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
                IsEnabled = true,
                Position = position,
                Size = size
            });

        }
    }

    DialogueBoxBehaviour CreateDialogueBoxWindow(int index, Vector2 position, Vector2 size)
    {
        GameObject gO = GameObject.Instantiate(this.DialogueBoxPrefab, this.ScreenElementsTransform.transform);

        RectTransform gORect = gO.GetComponent<RectTransform>();
        gORect.anchoredPosition = position;

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
        GameObject gO = GameObject.Instantiate(this.WindowMinimisationPrefab, this.ScreenElementsTransform.transform);
        gO.transform.SetParent(WindowGridTransform.transform);
        if(gO.TryGetComponent(out UITaskBarMinimisationWidget minimisationWidget))
        {
            minimisationWidget.SetMinimisableIndex(index);
            return minimisationWidget;
        }
        return null;
    }

    public void OnMinimiseClicked(int index)
    {
        if (WindowDataDict.ContainsKey(index))
        {
            WindowData data = WindowDataDict[index];
            data.IsEnabled = !data.IsEnabled;
            data.DialogueBox.ApplyMinimised(data.IsEnabled);

            WindowDataDict[index] = data;
        }
    }

    public void OnMinimisedTaskbarClicked(int index)
    {
        if (WindowDataDict.ContainsKey(index))
        {
            WindowData data = WindowDataDict[index];
            data.IsEnabled = !data.IsEnabled;
            data.DialogueBox.ApplyMinimised(data.IsEnabled);

            WindowDataDict[index] = data;
        }

    }

    public void OnClosedClicked(int index)
    {

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
