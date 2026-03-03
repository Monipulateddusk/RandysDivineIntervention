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

public class DialogueBoxBehaviour : MonoBehaviour, IUISelectable
{
    enum DialogueBoxState
    {
        Idle = 0,
        HorizontalResize = 1,
        VerticalResize = 2,

        BothAxisResize = HorizontalResize | VerticalResize,
        DragMoving = 4,
    }
    UnityEngine.UI.LayoutElement LayoutElement;
    UnityEngine.BoxCollider2D BoxCollider;
    [SerializeField] TextMeshProUGUI textMeshProUGUI;

    [SerializeField] DialogueBoxState currentDialogueBoxState;
    Vector3 MouseDragStartPosition;
    const float MIN_WIDTH = 300, MIN_HEIGHT = 150, TITLE_BAR_HEIGHT = 50, HEADER_BUTTON_WIDTH = 110;

    private void Awake()
    {
        this.BoxCollider = GetComponent<BoxCollider2D>();
        this.LayoutElement = GetComponent<UnityEngine.UI.LayoutElement>();
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
    
    void ResizeDialogueBox(Vector2 newSize)
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

    #region Drag Move Functionality

    private void ProcessIfCursorIsWithinTitleBar(Vector2 mousePos)
    {
        if (currentDialogueBoxState == DialogueBoxState.Idle && IsPositionWithinTopTile(mousePos.x, mousePos.y))
        {         
            currentDialogueBoxState = DialogueBoxState.DragMoving;
        }
    }


    private bool IsPositionWithinTopTile(float positionX, float positionY)
    {
        /*  Get the bounds  */
        Bounds boxBounds = this.BoxCollider.bounds;

        bool withinX = positionX <= (boxBounds.max.x - HEADER_BUTTON_WIDTH) && positionX >= boxBounds.min.x;
        bool withinY = positionY <= boxBounds.max.y && (positionY >= boxBounds.max.y - TITLE_BAR_HEIGHT);

        return withinX && withinY;
    }

    private void ProcessDragMove()
    {
        this.transform.position = Input.mousePosition - MouseDragStartPosition;
        textMeshProUGUI.text = "XPos: " + this.transform.position.x + "YPos" + this.transform.position.y;
    }


    #endregion

    public void OnSelect(Vector2 mousePos)
    {



        if (currentDialogueBoxState == DialogueBoxState.DragMoving)
        {
            MouseDragStartPosition = Input.mousePosition - this.transform.position;
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
}


