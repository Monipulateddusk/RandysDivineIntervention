using UnityEngine;

public interface IUISelectable
{
    public void OnSelect(Vector2 mousePos);
    public void OnDeselect(Vector2 mousePos);
    public void OnDrag(Vector2 mousePos);
    public void OnHover(Vector2 mousePos);
    public CursorManager.CursorIcons GetCurrentMouseStateSuggestion();
}

public class MinimisableUI : MonoBehaviour
{
    private int minimisableIndex;
    bool isMinimised;
    public void SetMinimisableIndex(int index) {  this.minimisableIndex = index; }
    protected void SetIsMinimised(bool minimised) { this.isMinimised = minimised;}
    public bool GetIsMinimised() => isMinimised;
    public int GetMinimisableIndex() { return this.minimisableIndex; }
    

    protected Vector2 GetMousePositionWithinRect(RectTransform rectTransform, Vector2 mousePos)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, mousePos, null, out Vector2 localMousePos);
        return localMousePos;
    }
}

public class DialogueBoxBehaviour : MinimisableUI, IUISelectable
{
    enum DialogueBoxState
    {
        Idle = 0,
        HorizontalResize = 1,
        VerticalResize = 2,

        BothAxisResize = HorizontalResize | VerticalResize,
        DragMoving = 4,
    }
    enum ButtonSelection 
    {
        None = 0,
        Minimise = 1,
        Close = 2,
    }

    [Header("Serialised Variables. Assign in inspector!")]
    [SerializeField] private UnityEngine.UI.LayoutElement LayoutElement;
    [SerializeField] private UnityEngine.BoxCollider2D BoxCollider;
    [SerializeField] private CanvasGroup CanvasGroup;
    [SerializeField, Tooltip("Assign with the 'Content' GameObject")] private UnityEngine.RectTransform ContentGameObjectRoot;
    [SerializeField] private UnityEngine.RectTransform headerBufferTransform, minimiseButtonTransform, closeButtonTransform;

    [Header("Debugging")]
    
    private ButtonSelection currentButtonSelection;
    private DialogueBoxState currentDialogueBoxState;

    //  Box Move and Resize.        
    private Vector3 MouseDragStartPosition;

    #region Resize Functionality
    void ProcessIfCursorIsWithinEdgeBounds(Vector2 mousePos)
    {
        bool isPointWithinHorizontalEdge = IsPositionInsideHorizontalEdgeBounds(mousePos.x, mousePos.y);
        bool isPointWithinVerticalEdge = IsPositionInsideVerticalEdgeBounds(mousePos.x, mousePos.y);

        AssignResizeOperation(isPointWithinHorizontalEdge, isPointWithinVerticalEdge);
    }

    bool IsPositionInsideVerticalEdgeBounds(float positionX, float positionY)
    {
        /*  Get the bounds  */
        Bounds boxBounds = this.BoxCollider.bounds;

        bool withinTopEdge          = positionY >= (boxBounds.max.y - this.BoxCollider.edgeRadius) && positionY <= (boxBounds.max.y);
        bool withinBottomEdge       = positionY >= (boxBounds.min.y - this.BoxCollider.edgeRadius) && positionY <= (boxBounds.min.y);

        bool withinHorizontalBounds = positionX >= (boxBounds.min.x - this.BoxCollider.edgeRadius) && positionX <= (boxBounds.max.x + this.BoxCollider.edgeRadius);

        return (withinTopEdge || withinBottomEdge) && withinHorizontalBounds;
    }

    bool IsPositionInsideHorizontalEdgeBounds(float positionX, float positionY)
    {
        /*  Get the bounds  */
        Bounds boxBounds = this.BoxCollider.bounds;

        bool withinLeftEdge     = positionX >= (boxBounds.min.x - this.BoxCollider.edgeRadius) && positionX <= (boxBounds.min.x + this.BoxCollider.edgeRadius);
        bool withinRightEdge    = positionX >= (boxBounds.max.x - this.BoxCollider.edgeRadius) && positionX <= (boxBounds.max.x + this.BoxCollider.edgeRadius);

        bool withinVerticalBounds = positionY <= (boxBounds.max.y + this.BoxCollider.edgeRadius) && positionY >= (boxBounds.min.y - this.BoxCollider.edgeRadius);

        return (withinLeftEdge || withinRightEdge) && withinVerticalBounds;
    }

    void AssignResizeOperation(bool isWithinHorizonalEdge, bool isWithinVerticalEdge)
    {
        if (isWithinHorizonalEdge && !isWithinVerticalEdge)
        {
            currentDialogueBoxState = DialogueBoxState.HorizontalResize;

        }
        else if(!isWithinHorizonalEdge && isWithinVerticalEdge)
        {
            currentDialogueBoxState = DialogueBoxState.VerticalResize;

        }
        else if(isWithinHorizonalEdge && isWithinVerticalEdge)
        {
            currentDialogueBoxState = DialogueBoxState.BothAxisResize;

        }
        else
        {
            currentDialogueBoxState = DialogueBoxState.Idle;
        }
    }
    
    public void ResizeDialogueBox(Vector2 newSize)
    {
        if(newSize.x <= this.LayoutElement.minWidth) { newSize.x = this.LayoutElement.minWidth; }
        if(newSize.y <= this.LayoutElement.minHeight) { newSize.y = this.LayoutElement.minHeight; }

        this.LayoutElement.preferredHeight = newSize.y;
        this.LayoutElement.preferredWidth = newSize.x;

        this.BoxCollider.size = newSize;
    }

    void ProcessResize(Vector2 mousePos)
    {
        RectTransform mainWindowRect = GetComponent<RectTransform>();

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            mainWindowRect,
            mousePos,
            null,
            out Vector2 localMousePos
        );

        float mouseDistanceFromCentreX = Mathf.Abs(localMousePos.x) * 2;
        float mouseDistanceFromCentreY = Mathf.Abs(localMousePos.y) * 2;

        if (currentDialogueBoxState == DialogueBoxState.HorizontalResize)
        {
            ResizeDialogueBox(new Vector2(mouseDistanceFromCentreX, this.BoxCollider.size.y));
        }
        else if (currentDialogueBoxState == DialogueBoxState.VerticalResize)
        {
            ResizeDialogueBox(new Vector2(this.BoxCollider.size.x, mouseDistanceFromCentreY));
        }
        else if (currentDialogueBoxState == DialogueBoxState.BothAxisResize)
        {
            ResizeDialogueBox(new Vector2(mouseDistanceFromCentreX, mouseDistanceFromCentreY));
        }
        else
        {
            currentDialogueBoxState = DialogueBoxState.Idle;
        }
    }

    #endregion

    #region Header Button Functionality

    private void ProcessIfCursorIsWithinTitleBar(Vector2 mousePos)
    {
        /*  Get the mouse position inside each UI element. Yes, this is horribly inefficient. However, counterpoint: */
        Vector2 mousePositionInsideTitleBarRect = GetMousePositionWithinRect(this.headerBufferTransform, mousePos);
        Vector2 mousePositionInsideMinimiseButtonRect = GetMousePositionWithinRect(minimiseButtonTransform, mousePos);
        Vector2 mousePositionInsideCloseButtonRect = GetMousePositionWithinRect(closeButtonTransform, mousePos);

        if (this.headerBufferTransform.rect.Contains(mousePositionInsideTitleBarRect))
        {
            if (this.minimiseButtonTransform.rect.Contains(mousePositionInsideMinimiseButtonRect))
            {
                currentButtonSelection = ButtonSelection.Minimise;
                currentDialogueBoxState = DialogueBoxState.Idle;
            }
            else if (this.closeButtonTransform.rect.Contains(mousePositionInsideCloseButtonRect))
            {
                currentButtonSelection = ButtonSelection.Close;
                currentDialogueBoxState = DialogueBoxState.Idle;
            }
            else
            {
                currentButtonSelection = ButtonSelection.None;
                currentDialogueBoxState = DialogueBoxState.DragMoving;
            }
        }
    }

    #endregion

    #region Drag Move Functionality

    private void ProcessDragMove()
    {
        this.transform.position = Input.mousePosition - MouseDragStartPosition;
    }


    #endregion

    public void ApplyMinimised(bool isEnabled)
    {
        // Based if we are minimised, we want to disable colliders and set the CanvasGroup's settings accordingly.
        if (isEnabled)
        {
            SetIsMinimised(false);
            this.CanvasGroup.alpha = 1;
            this.CanvasGroup.interactable = true;
            this.CanvasGroup.blocksRaycasts = true;
        }
        else
        {
            SetIsMinimised(true);
            // Set Canvas group settings
            this.CanvasGroup.alpha = 0;
            this.CanvasGroup.interactable = false;
            this.CanvasGroup.blocksRaycasts = false;

        }
    }

    public void OnSelect(Vector2 mousePos)
    {
        if (currentDialogueBoxState == DialogueBoxState.DragMoving)
        {
            MouseDragStartPosition = (Vector3)mousePos - this.transform.position;
        }
        else if (currentDialogueBoxState == DialogueBoxState.Idle)
        {

            if (currentButtonSelection == ButtonSelection.Minimise)
            {
                _ = UserInterfaceManager.Instance.GetTaskBarManager().OnMinimiseClicked(this.GetMinimisableIndex());
            }
            else if (currentButtonSelection == ButtonSelection.Close)
            {
                UserInterfaceManager.Instance.GetTaskBarManager().OnClosedClicked(this.GetMinimisableIndex());

            }
        }
    }

    public void OnDeselect(Vector2 mousePos)
    {
        currentDialogueBoxState = DialogueBoxState.Idle; 
    }

    public void OnDrag(Vector2 mousePos)
    {
        if (GetIsMinimised()) { return; }

        if (currentDialogueBoxState != DialogueBoxState.Idle && currentDialogueBoxState != DialogueBoxState.DragMoving)
        {
            ProcessResize(mousePos);
        }
        else if(currentDialogueBoxState == DialogueBoxState.DragMoving)
        {
            ProcessDragMove();
        }

    }

    public void OnHover(Vector2 mousePos)
    {
        if (GetIsMinimised()) { return; }

        ProcessIfCursorIsWithinEdgeBounds(mousePos);

        ProcessIfCursorIsWithinTitleBar(mousePos);


    }

    public CursorManager.CursorIcons GetCurrentMouseStateSuggestion()
    {
        switch (currentDialogueBoxState)
        {
            case DialogueBoxState.Idle:
                return CursorManager.CursorIcons.Cursor;
            case DialogueBoxState.HorizontalResize:
                return CursorManager.CursorIcons.HorizResize;
            case DialogueBoxState.VerticalResize:
                return CursorManager.CursorIcons.VerticResize;
            case DialogueBoxState.BothAxisResize:
                return CursorManager.CursorIcons.DiagResize;
            case DialogueBoxState.DragMoving:
                return CursorManager.CursorIcons.Move;
            default:
                return CursorManager.CursorIcons.Cursor;

        }
    }

    public Vector2 GetDialogueBoxSize()
    {
        return new Vector2(this.LayoutElement.preferredWidth, this.LayoutElement.preferredHeight);
    }

    public RectTransform GetContentGameObjectRoot() => ContentGameObjectRoot;
}


