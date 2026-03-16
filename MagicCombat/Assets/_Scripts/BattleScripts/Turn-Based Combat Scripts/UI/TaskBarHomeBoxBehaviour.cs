using UnityEngine;

public class TaskBarHomeBoxBehaviour : SizableWindowBaseBehaviour
{
    RectTransform rect;
    private void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    public override void ApplyMinimised(bool isEnabled)
    {
        if (isEnabled)
        {
            rect.sizeDelta = new Vector2(300, 140);
        }
        else
        {
            rect.sizeDelta = Vector2.zero;

        }
    }

    public override Vector2 GetDialogueBoxSize()
    {
        return new Vector2(300, 140);
    }

    public override void Resize(Vector2 newSize)
    {
        rect.sizeDelta = newSize;
    }
}
