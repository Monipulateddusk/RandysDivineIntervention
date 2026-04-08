namespace TurnBased.TargetSelection
{
    public interface ITargetSelector
    {
        public abstract System.Collections.Generic.List<StationIndex> SelectTargets(SceneData_UnitTurn data, IBattleMove selectedMove);
    }

    public class SequentialTargetSelector : ITargetSelector
    {
        // Yes, small bug is that if the unit selects to target their allies, it will increment their selection index, and then if they switch to enemies, it will carry over their selection index.
        int curSelectionIndex = 0;

        System.Collections.Generic.List<StationIndex> ITargetSelector.SelectTargets(SceneData_UnitTurn data, IBattleMove selectedMove)
        {
            StationIndex selectedStationIndex;
            int wrappedIndex;

            MoveTarget moveTargetType = selectedMove.GetMoveTargetType();
            switch (moveTargetType)
            {
                case MoveTarget.Self:
                    return new() { data.SourceStationIndex };

                case MoveTarget.SingleEnemy:

                    WrapSelectionIndex(data.EnemyStationIndexes, curSelectionIndex, out wrappedIndex);

                    /*  After we get the selected station index, increment it for next time.    */
                    selectedStationIndex = data.EnemyStationIndexes[wrappedIndex];
                    curSelectionIndex++;

                    return new() { selectedStationIndex };

                case MoveTarget.SingleAlly:

                    WrapSelectionIndex(data.AllyStationIndexes, curSelectionIndex, out wrappedIndex);

                    /*  After we get the selected station index, increment it for next time.    */
                    selectedStationIndex = data.AllyStationIndexes[wrappedIndex];
                    curSelectionIndex++;

                    return new() { selectedStationIndex };

                case MoveTarget.AllEnemies:
                    return data.EnemyStationIndexes;

                case MoveTarget.AllAllies:
                    return data.AllyStationIndexes;

                case MoveTarget.Area:
                    return TargetSelectorHandler.GetAllStationsOnField(data, includeSource: true);

                default:
                    return new() { data.SourceStationIndex };
            }
        }

        private void WrapSelectionIndex(System.Collections.Generic.List<StationIndex> targettingStations, int selectionIndex, out int wrappedIndex)
        {
            wrappedIndex = selectionIndex;
            /*  Check to see if the index is valid, if not, wrap back to the start. */
            if (selectionIndex > targettingStations.Count - 1)
            {
                wrappedIndex = 0;
            }
        }    
    }

    public class RandomTargetSelector : ITargetSelector
    {
        public System.Collections.Generic.List<StationIndex> SelectTargets(SceneData_UnitTurn data, IBattleMove selectedMove)
        {
            MoveTarget moveTargetType = selectedMove.GetMoveTargetType();
            return moveTargetType switch
            {
                MoveTarget.Self => new() { data.SourceStationIndex },

                MoveTarget.SingleEnemy => new() { GetRandomStationIndexFromList(data.EnemyStationIndexes) },

                MoveTarget.SingleAlly => new() { GetRandomStationIndexFromList(data.AllyStationIndexes) },

                MoveTarget.AllEnemies => data.EnemyStationIndexes,

                MoveTarget.AllAllies => data.AllyStationIndexes,

                MoveTarget.Area => TargetSelectorHandler.GetAllStationsOnField(data, includeSource: true),

                _ => new() { data.SourceStationIndex },
            };
        }

        private StationIndex GetRandomStationIndexFromList(System.Collections.Generic.List<StationIndex> stationIndexes)
        {
            int randomIndex = UnityEngine.Random.Range(0, stationIndexes.Count);
            return stationIndexes[randomIndex];
        }
    }

    public class PlayerDrivenTargetSelector : ITargetSelector
    {
        public int SelectedTargetIndex { private get; set; }
        public System.Collections.Generic.List<StationIndex> SelectTargets(SceneData_UnitTurn data, IBattleMove selectedMove)
        {
            MoveTarget moveTargetType = selectedMove.GetMoveTargetType();

            switch (moveTargetType)
            {
                case MoveTarget.SingleEnemy:
                    /*  Confirm that the selected index the player chose is within the list's size. If not, abort!  */
                    if (this.SelectedTargetIndex > data.EnemyStationIndexes.Count - 1) { return null; }

                    return new() { data.EnemyStationIndexes[this.SelectedTargetIndex] };

                case MoveTarget.SingleAlly:
                    if (this.SelectedTargetIndex > data.AllyStationIndexes.Count - 1) { return null; }

                    return new() { data.AllyStationIndexes[this.SelectedTargetIndex] };

                case MoveTarget.Self:
                    return new() { data.SourceStationIndex };

                case MoveTarget.AllEnemies:
                    return data.EnemyStationIndexes;

                case MoveTarget.AllAllies:
                    return data.AllyStationIndexes;

                case MoveTarget.Area:
                    return TargetSelectorHandler.GetAllStationsOnField(data, includeSource: true);

                default:
                    return new() { data.SourceStationIndex };
            }
        }
    }

}