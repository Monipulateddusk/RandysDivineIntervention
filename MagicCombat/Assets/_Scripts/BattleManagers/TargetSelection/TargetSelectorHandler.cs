namespace TurnBased.TargetSelection
{
    public static class TargetSelectorHandler
    {
        private static readonly System.Collections.Generic.Dictionary<UnitTargetSelectorType, ITargetSelector> targetSelectors = new()
        {
            {UnitTargetSelectorType.Sequential,     new SequentialTargetSelector()      },
            {UnitTargetSelectorType.Random,         new RandomTargetSelector()          },
            {UnitTargetSelectorType.PlayerDriven,   new PlayerDrivenTargetSelector()    },
            {UnitTargetSelectorType.HighestHP,      new HighestHPTargetSelector()       },
        };

        public static bool TryGetSelector(UnitTargetSelectorType type, out ITargetSelector moveSelector)
        {
            if (targetSelectors.TryGetValue(type, out moveSelector))
            {
                return true;
            }
            return false;
        }

        public static bool IsSelectorTypeInstanciatable(UnitTargetSelectorType type)
        {
            return type switch
            {
                UnitTargetSelectorType.Sequential => true,
                UnitTargetSelectorType.Random => false,
                UnitTargetSelectorType.PlayerDriven => false,
                UnitTargetSelectorType.HighestHP => false,
                _ => false,
            };
        }
        public static System.Collections.Generic.List<StationIndex> GetAllStationsOnField(SceneData_UnitTurn data, bool includeSource = true)
        {
            System.Collections.Generic.List<StationIndex> allStations = new();

            if (includeSource) { allStations.Add(data.SourceStationIndex); }

            allStations.AddRange(data.AllyStationIndexes);
            allStations.AddRange(data.EnemyStationIndexes);

            return allStations;
        }
    }
}

