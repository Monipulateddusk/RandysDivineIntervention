using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

    public void SetMinimisableIndex(int index) {  this.minimisableIndex = index; }
    public int GetMinimisableIndex() { return this.minimisableIndex; }
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
    //  Box Move and Resize.        
    [SerializeField] private DialogueBoxState currentDialogueBoxState;
    private Vector3 MouseDragStartPosition;
    private const float MIN_WIDTH = 300, MIN_HEIGHT = 150, TITLE_BAR_HEIGHT = 50;

    //  Components 
    private UnityEngine.UI.LayoutElement LayoutElement;
    private UnityEngine.BoxCollider2D BoxCollider;

    //  Header Button Referances.   
    private UnityEngine.BoxCollider2D minimiseCollider, closeCollider;
    [SerializeField] private ButtonSelection currentButtonSelection;

    public event Action OnMinimise;

    private void Awake()
    {
        this.BoxCollider = GetComponent<BoxCollider2D>();
        this.LayoutElement = GetComponent<UnityEngine.UI.LayoutElement>();

        /*  Get the colliders of the Buttons in the Header. IMPORTANT: The selectable component is Smoke and Mirrors. It just changes the colour shade. */
        Transform buttonsParentTransform = this.transform.Find("Header").Find("HeaderBuffer").Find("Buttons");
        this.minimiseCollider   = buttonsParentTransform.Find("MinimiseBG").GetComponent<BoxCollider2D>();
        this.closeCollider      = buttonsParentTransform.Find("CloseBG").GetComponent<BoxCollider2D>();
    }

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
        if(newSize.x <= MIN_WIDTH) { newSize.x = MIN_WIDTH; }
        if(newSize.y <= MIN_HEIGHT) { newSize.y = MIN_HEIGHT; }

        this.LayoutElement.preferredHeight = newSize.y;
        this.LayoutElement.preferredWidth = newSize.x;

        this.BoxCollider.size = newSize;
    }

    private Vector3 WorldSpaceToScreenSpace(Vector3 inPos) => Camera.main.WorldToScreenPoint(new Vector3(inPos.x, inPos.y, 0));

    void ProcessResize(Vector2 mousePos)
    {
        RectTransform rect = GetComponent<RectTransform>();

        Vector2 localMousePos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rect,
            mousePos,
            null,
            out localMousePos
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
    private bool IsPositionWithinTopTile(float positionX, float positionY)
    {
        /*  Get the bounds  */
        Bounds boxBounds = this.BoxCollider.bounds;

        bool withinX = positionX <= (boxBounds.max.x) && positionX >= boxBounds.min.x;
        bool withinY = positionY <= boxBounds.max.y && (positionY >= boxBounds.max.y - TITLE_BAR_HEIGHT);

        return withinX && withinY;
    }

    private bool IsPositionWithinBoxBounds(float positionX, float positionY, Bounds boxBounds)
    {
        bool withinX = positionX <= (boxBounds.max.x) && positionX >= boxBounds.min.x;
        bool withinY = positionY <= boxBounds.max.y && (positionY >= boxBounds.min.y);
        return withinX && withinY;
    }

    private void ProcessIfCursorIsWithinTitleBar(Vector2 mousePos)
    {
        if (currentDialogueBoxState == DialogueBoxState.Idle && IsPositionWithinTopTile(mousePos.x, mousePos.y))
        {
            /*  Check to see if the mouse position is within either of the Button Boxes. If so, we aren't drag moving.  */
            if(IsPositionWithinBoxBounds(mousePos.x, mousePos.y, minimiseCollider.bounds)) 
            {
                currentButtonSelection = ButtonSelection.Minimise;
                currentDialogueBoxState = DialogueBoxState.Idle;
            }
            else if(IsPositionWithinBoxBounds(mousePos.x, mousePos.y, closeCollider.bounds))
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

    public void OnSelect(Vector2 mousePos)
    {
        if (currentDialogueBoxState == DialogueBoxState.DragMoving)
        {
            MouseDragStartPosition = Input.mousePosition - this.transform.position;
        }
        else if(currentDialogueBoxState == DialogueBoxState.Idle)
        {
            if(currentButtonSelection == ButtonSelection.Minimise)
            {
                UserInterfaceManager.Instance.GetTaskBarManager().OnMinimiseClicked(this.GetMinimisableIndex());
            }
            else if(currentButtonSelection == ButtonSelection.Close)
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

    public void OnMinimiseHappen()
    {
        throw new NotImplementedException();
    }
}


