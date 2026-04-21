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
/// STATION SELECTION ENUMERATIONS
/// 
/// =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

public enum StationSelectionState
{
    Unlocked = 0,
    Locked = 1
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


public enum CombatTurnOrchestrationPhase
{
    StartOfBattle   = 0,
    StartOfRound    = 1,
    PrePlayerTurn   = 2,
    PlayerTurn      = 3,
    EndOfRound      = 4,
}


/// =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-
/// 
/// UI ENUMERATIONS
/// 
/// =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

public enum CommandUIBehaviourStates 
{
    UnitIntention = 0,
    MoveSelection = 1, 
    TargetSelection = 2, 
    UnitEndTurn = 3,
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

public enum SubPhaseState 
{
    NONE                        = 0,
    AWAITING_MOVE_SELECTION     = 1, 
    AWAITING_TARGET_SELECTION   = 2, 
    READY_TO_EXECUTE_MOVE       = 3,
    RESOLVE_ATTACK              = 4,
    ATTACK_COMPLETE             = 5,
}

public enum PHASE_TYPES
{
    START_BATTLE,
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
    COMPLETED_INTENTION = 3,   
    RESOLVED_MOVE = 4,
}

public enum MoveResolutionTiming
{
    Instant,
    TurnOrderSequence
}


public enum TurnOrderCreationState
{
    InsufficentUnits = 0,
    NewTurnOrderList = 1,
    OldTurnOrderList = 2,
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
