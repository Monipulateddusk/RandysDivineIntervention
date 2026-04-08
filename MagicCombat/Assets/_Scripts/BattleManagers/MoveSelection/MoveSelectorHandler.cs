public enum UnitMoveSelectorType { Random, Sequential, PlayerDriven}

namespace TurnBased.MoveSelection
{
    public static class MoveSelectorHandler
    {
        private static readonly System.Collections.Generic.Dictionary<UnitMoveSelectorType, IMoveSelector> moveSelectors = new()
        {
        {UnitMoveSelectorType.Sequential,   new SequentialMoveSelector()    },
        {UnitMoveSelectorType.Random,       new RandomMoveSelector()        },
        {UnitMoveSelectorType.PlayerDriven, new PlayerDrivenMoveSelector()  },
        };

        public static bool TryGetSelector(UnitMoveSelectorType type, out IMoveSelector moveSelector)
        {
            if (moveSelectors.TryGetValue(type, out moveSelector))
            {
                return true;
            }
            return false;
        }

        public static bool IsSelectorTypeInstanciatable(UnitMoveSelectorType type)
        {
            return type switch
            {
                UnitMoveSelectorType.Sequential => true,
                UnitMoveSelectorType.Random => false,
                UnitMoveSelectorType.PlayerDriven => false,
                _ => false,
            };
        }
    }
}

