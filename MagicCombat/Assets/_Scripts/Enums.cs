public enum UnitTeam 
{ 
    NULL = 0,
    ALLY = 1, 
    ENEMY = 2 
};

public enum StatusEffect
{
    NULL,
}

public enum Element
{
    NULL, FIRE, WATER, ICE, EARTH, LIGHT, DARKNESS,
}

/// =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-
/// 
/// COMBAT RESOLVING ENUMERATIONS
/// 
/// =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-


public enum AttackActionType 
{ 
    DAMAGE,
    HEALING, 
    STATUS_EFFECT, 
    IMBUE_ENVIRONMENTS 
}

public enum MoveTarget
{
    Self,
    SingleEnemy,
    SingleAlly,
    AllEnemies,
    AllAllies,
    Area
}

public enum UnitMoveSelectorType 
{ 
    Random, 
    Sequential, 
    PlayerDriven 
}

public enum UnitTargetSelectorType
{
    Random,
    Sequential,
    PlayerDriven
}

/// =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-
/// 
/// UI ENUMERATIONS
/// 
/// =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

public enum CommandUIBehaviourStates 
{ 
    Default = 0, 
    TargetSelection = 1, 
    UnitEndTurn = 2 
};


public enum SelectionUIBehaviourState 
{ 
    Camera = 0, 
    Units = 1 
}

public enum CursorIcons 
{ 
    Cursor, 
    Help, 
    VerticResize, 
    HorizResize, 
    DiagResize, 
    Text, 
    Grid, 
    Move, 
    END 
}

public enum DialogueBoxState
{
    Idle = 0,
    HorizontalResize = 1,
    VerticalResize = 2,

    BothAxisResize = HorizontalResize | VerticalResize,
    DragMoving = 4,
}
public enum DialogueBoxButtonSelectionState
{
    None = 0,
    Minimise = 1,
    Close = 2,
}

public enum WindowAnimationState 
{ 
    Shrunk, 
    Enlarged 
}

public enum UIWindowFactoryWindowType
{
    HomeStart,
    TurnOrderWindow,
    Options,
    DialogueBox,
    Inspection,
    WindowManager,
    Selector
}


/// =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-
/// 
/// PHASE ENUMERATIONS
/// 
/// =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

public enum MAIN_TURN_STATE 
{ 
    IDLE = 0, 
    AWAITING_MOVE_SELECTION = 1, 
    AWAITING_TARGET_SELECTION = 2, 
    READY_TO_EXECUTE_MOVE = 3,
    RESOLVE_ATTACK = 4,
    ATTACK_COMPLETE = 5,
}

public enum PHASE_TYPES
{
    START_ROUND,
    PRE_UNIT_TURN,
    UNIT_TURN,
    END_ROUND
};

public enum UnitIntentionResolutionState
{
    NONE = 0,
    AWAITING_MOVE_SELECTION = 1,
    AWAITING_TARGET_SELECTION = 2,
    COMPLETE = 3,   
}

/// =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-
/// 
/// CAMERA ENUMERATIONS
/// 
/// =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

enum CameraAnimType 
{ 
    Fade, 
    Shift, 
    Static, 
    Shader 
}
