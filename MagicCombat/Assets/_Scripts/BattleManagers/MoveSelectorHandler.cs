using System.Collections.Generic;

public  enum UnitMoveSelectorType { Random, Sequential, PlayerDriven}

public static class MoveSelectorHandler 
{
    private static readonly Dictionary<UnitMoveSelectorType, IMoveSelector> moveSelectors = new()
    {
        {UnitMoveSelectorType.Sequential,   new SequentialMoveSelector()    },
        {UnitMoveSelectorType.Random,       new RandomMoveSelector()        },
        {UnitMoveSelectorType.PlayerDriven, new PlayerDrivenMoveSelector()  },
    };

    public static IMoveSelector GetSelector(UnitMoveSelectorType type) => moveSelectors[type];
}
