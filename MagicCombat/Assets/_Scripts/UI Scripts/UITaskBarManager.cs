using System.Threading.Tasks;
using UnityEngine;

public class UITaskBarManager
{
    RectTransform TaskbarHomeBoxTransform;

    const float MAX_TASKBAR_HOME_HEIGHT = -140;
    const float EXPAND_SHRINK_TIMER = 2.0f;

    public UITaskBarManager(RectTransform taskBarHomeBoxTransform)
    {
        this.TaskbarHomeBoxTransform = taskBarHomeBoxTransform;
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Y))
        {
            OnClickStartOS();
        }
    }


    public void OnClickStartOS()
    {
        if (TaskbarHomeBoxTransform != null)
        {
            float targetHeight = this.TaskbarHomeBoxTransform.GetTop() == MAX_TASKBAR_HOME_HEIGHT ? 0 : MAX_TASKBAR_HOME_HEIGHT;
            _ = ExpandShrinkHomeBox(targetHeight);
        }
    }


    async Task ExpandShrinkHomeBox(float targetHeight)
    {
        if (TaskbarHomeBoxTransform != null)
        {
            float endTime = Time.deltaTime + EXPAND_SHRINK_TIMER;
            while (Time.deltaTime < endTime) 
            {
                Debug.Log("sadad");
                //this.TaskbarHomeBoxTransform.SetTop(this.TaskbarHomeBoxTransform.GetTop() - (targetHeight * Time.deltaTime));
                await Task.Yield();
            }
        }
        
    }
}
