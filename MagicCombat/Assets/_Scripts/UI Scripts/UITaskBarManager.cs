using System.Threading.Tasks;
using UnityEngine;

public class UITaskBarManager
{
    RectTransform TaskbarHomeBoxTransform;

    const float MAX_TASKBAR_HOME_HEIGHT = 140;
    const float EXPAND_SHRINK_TIMER = 0.1f;
    bool isExpandingShrinking = false;

    public UITaskBarManager(RectTransform taskBarHomeBoxTransform)
    {
        this.TaskbarHomeBoxTransform = taskBarHomeBoxTransform;
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Y))
        {
            _ = OnClickStartOS();
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
