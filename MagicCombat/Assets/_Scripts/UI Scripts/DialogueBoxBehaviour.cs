using System;
using TMPro;
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
    private DialogueBoxState currentDialogueBoxState;
    private Vector3 MouseDragStartPosition;
    private const float MIN_WIDTH = 300, MIN_HEIGHT = 150, TITLE_BAR_HEIGHT = 50;

    //  Components 
    private UnityEngine.UI.LayoutElement LayoutElement;
    private UnityEngine.BoxCollider2D BoxCollider;

    //  Header Button Referances.   
    private UnityEngine.BoxCollider2D minimiseCollider, closeCollider;
    private ButtonSelection currentButtonSelection;

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
        /* Ignoring Positive and Negative values, determine how far the Position is from the Box's centre in world Space. */
        float distanceFromBoxCentreY = Mathf.Abs(positionY - this.transform.position.y); 
        
        /* Using that distance, compare that distance from the half size of the BoxCollider. */ 
        
        float distanceFromVerticalEdge = Mathf.Abs(distanceFromBoxCentreY - (this.BoxCollider.size.y * 0.5f)); 
        float horizontalBounds = Mathf.Abs(this.transform.position.x + (this.BoxCollider.size.x * 0.5f) + this.BoxCollider.edgeRadius); 
        
        /* Is this point within the threshold for the edge radius? */ 
        return (distanceFromVerticalEdge <= this.BoxCollider.edgeRadius) && Mathf.Abs(positionX) <= horizontalBounds;

    }

    bool IsPositionInsideHorizontalEdgeBounds(float positionX, float positionY)
    {
        /* Ignoring Positive and Negative values, determine how far the Position is from the Box's centre in world Space. */
        float distanceFromBoxCentreX = Mathf.Abs(positionX - this.transform.position.x);

        /* Using that distance, compare that distance from the half size of the BoxCollider. */

        float distanceFromHorizontalEdge = Mathf.Abs(distanceFromBoxCentreX - (this.BoxCollider.size.x * 0.5f));
        float verticalBounds = Mathf.Abs(this.transform.position.y + (this.BoxCollider.size.y * 0.5f) + this.BoxCollider.edgeRadius);

        /* Is this point within the threshold for the edge radius? */
        return (distanceFromHorizontalEdge <= this.BoxCollider.edgeRadius) && Mathf.Abs(positionY) <= verticalBounds;

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

    void ProcessResize()
    {
        if (currentDialogueBoxState == DialogueBoxState.HorizontalResize)
        {
            float mouseDistanceFromCentreX = Mathf.Abs(Input.mousePosition.x - this.transform.position.x);
            ResizeDialogueBox(new Vector2(mouseDistanceFromCentreX * 2, this.BoxCollider.size.y));
        }
        else if (currentDialogueBoxState == DialogueBoxState.VerticalResize)
        {
            float mouseDistanceFromCentreY = Mathf.Abs(Input.mousePosition.y - this.transform.position.y);
            ResizeDialogueBox(new Vector2(this.BoxCollider.size.x, mouseDistanceFromCentreY * 2));
        }
        else if (currentDialogueBoxState == DialogueBoxState.BothAxisResize)
        {
            float mouseDistanceFromCentreX = Mathf.Abs(Input.mousePosition.x - this.transform.position.x);
            float mouseDistanceFromCentreY = Mathf.Abs(Input.mousePosition.y - this.transform.position.y);
            ResizeDialogueBox(new Vector2(mouseDistanceFromCentreX * 2, mouseDistanceFromCentreY * 2));
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
                UserInterfaceManager.Instance.GetTaskBarManager().OnMinimisedClicked(this);
            }
            else if(currentButtonSelection == ButtonSelection.Close)
            {
                UserInterfaceManager.Instance.GetTaskBarManager().OnClosedClicked(this);

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
            ProcessResize();
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


