using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UITaskBarMinimisationWidget : MinimisableUI, IUISelectable
{
    public CursorIcons GetCurrentMouseStateSuggestion()
    {
        return CursorIcons.Cursor;
    }

    public void OnDeselect(Vector2 mousePos)
    {
        
    }

    public void OnDrag(Vector2 mousePos)
    {
        
    }

    private bool IsMouseInsideRect(Vector2 mousePos)
    {
        /*  Get the mouse position inside each UI element. Yes, this is horribly inefficient. However, counterpoint: */
        Vector2 mousePositionInsideTitleBarRect = GetMousePositionWithinRect((RectTransform)this.transform, mousePos);
        if((this.transform as RectTransform).rect.Contains(mousePositionInsideTitleBarRect))
        {
            return true;
        }
        return false;
    }

    public void OnHover(Vector2 mousePos)
    {
    }

    public async System.Threading.Tasks.Task OnSelect(Vector2 mousePos)
    {
        if (IsMouseInsideRect(mousePos))
        {
            await UserInterfaceManager.Instance.GetTaskBarManager().OnMinimiseClicked(this.GetMinimisableIndex());
        }
    }
}
