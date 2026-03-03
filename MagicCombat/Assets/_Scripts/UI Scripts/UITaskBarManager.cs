using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

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

    private readonly Dictionary<int, DialogueBoxData> DialogueBoxDataDict = new();

    public UITaskBarManager(RectTransform taskBarHomeBoxTransform, RectTransform windowGridTransform, RectTransform screenElementsTransform, GameObject windowMinimisationPrefab, GameObject dialogueBoxPrefab)
    {
        this.TaskbarHomeBoxTransform = taskBarHomeBoxTransform;
        this.WindowGridTransform = windowGridTransform;
        this.ScreenElementsTransform = screenElementsTransform;
        this.WindowMinimisationPrefab = windowMinimisationPrefab;
        this.DialogueBoxPrefab = dialogueBoxPrefab;

        CreateWindow(0, new Vector2(500, 0), new Vector2(300, 400));
        CreateWindow(1, new Vector2(1000, 0), new Vector2(300, 400));
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Y))
        {
            _ = OnClickStartOS();
        }
    }

    public void CreateWindow(int index, Vector2 position, Vector2 size)
    {
        /*  When creating a window, we need to do two things:   */
        /*  1) Create the UI Game Object.   */
        GameObject gO = GameObject.Instantiate(this.DialogueBoxPrefab, position, Quaternion.identity, this.ScreenElementsTransform.transform);
        gO.GetComponent<DialogueBoxBehaviour>().ResizeDialogueBox(size);
        gO.GetComponent<MinimisableUI>().SetMinimisableIndex(index);

        /*  2) Create the TaskBar minimisation Widget.          */
        gO = GameObject.Instantiate(this.WindowMinimisationPrefab, this.ScreenElementsTransform.transform);
        gO.transform.SetParent(WindowGridTransform.transform);

        /*  Store the data of this Window.  */
        if(!DialogueBoxDataDict.ContainsKey(index))
        {
            DialogueBoxDataDict.Add(index, new DialogueBoxData() { Size = size, Position = position });
            Debug.Log(DialogueBoxDataDict.Count);
        }
    }

    public void OnMinimisedClicked(DialogueBoxBehaviour dialogueBoxBehaviour)
    {
        DialogueBoxData data = new()
        {
            Position = dialogueBoxBehaviour.transform.position,
            Size = dialogueBoxBehaviour.GetDialogueBoxSize(),
        };


    }

    public void OnClosedClicked(DialogueBoxBehaviour dialogueBoxBehaviour)
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
