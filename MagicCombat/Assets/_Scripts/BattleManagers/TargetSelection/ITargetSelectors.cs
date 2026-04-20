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
            MoveTarget moveTargetType = selectedMove.GetMoveTargetType();
            TargettingSelectorInfo selectorInfo = StationManagerUtilities.FindAllPossibleTargettingStationIndexesOfTargettingType(data, moveTargetType);
            StationIndex selectedStationIndex;
            int wrappedIndex;



            /*  If the this MoveTargetType is any of: Self, Area, AllEnemies, AllAllies. Then we don't need to figure out which of the stations we have available specifically is the target.   */
            if (!selectorInfo.DoesRequireTargettingSelectorSelection)
            {
                return selectorInfo.PossibleTargets;
            }
            /*  However, for SingleAlly or SingleEnemy, we need to pick from the all possible options who specifically we are targetting.   */
            else
            {
                /// Single Enemy
                if (moveTargetType == MoveTarget.SingleEnemy)
                {
                    WrapSelectionIndex(data.EnemyStationIndexes, curSelectionIndex, out wrappedIndex);

                    /*  After we get the selected station index, increment it for next time.    */
                    selectedStationIndex = data.EnemyStationIndexes[wrappedIndex];
                    curSelectionIndex++;

                    return new() { selectedStationIndex };
                }
                /// Single Ally
                else
                {
                    WrapSelectionIndex(data.AllyStationIndexes, curSelectionIndex, out wrappedIndex);

                    /*  After we get the selected station index, increment it for next time.    */
                    selectedStationIndex = data.AllyStationIndexes[wrappedIndex];
                    curSelectionIndex++;

                    return new() { selectedStationIndex };
                }
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
            TargettingSelectorInfo selectorInfo = StationManagerUtilities.FindAllPossibleTargettingStationIndexesOfTargettingType(data, moveTargetType);


            /*  If the this MoveTargetType is any of: Self, Area, AllEnemies, AllAllies. Then we don't need to figure out which of the stations we have available specifically is the target.   */
            if (!selectorInfo.DoesRequireTargettingSelectorSelection)
            {
                return selectorInfo.PossibleTargets;
            }
            /*  However, for SingleAlly or SingleEnemy, we need to pick from the all possible options who specifically we are targetting.   */
            else
            {
                /// Single Enemy
                if (moveTargetType == MoveTarget.SingleEnemy)
                {
                    return new() { GetRandomStationIndexFromList(data.EnemyStationIndexes) };
                }
                /// Single Ally
                else
                {

                    return new() { GetRandomStationIndexFromList(data.AllyStationIndexes) };
                }
            }
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
            TargettingSelectorInfo selectorInfo = StationManagerUtilities.FindAllPossibleTargettingStationIndexesOfTargettingType(data, moveTargetType);


            /*  If the this MoveTargetType is any of: Self, Area, AllEnemies, AllAllies. Then we don't need to figure out which of the stations we have available specifically is the target.   */
            if (!selectorInfo.DoesRequireTargettingSelectorSelection)
            {
                return selectorInfo.PossibleTargets;
            }
            /*  However, for SingleAlly or SingleEnemy, we need to pick from the all possible options who specifically we are targetting.   */
            else
            {
                /// Single Enemy
                if (moveTargetType == MoveTarget.SingleEnemy)
                {
                    /*  Confirm that the selected index the player chose is within the list's size. If not, abort!  */
                    if (this.SelectedTargetIndex > data.EnemyStationIndexes.Count - 1) { return null; }

                    return new() { data.EnemyStationIndexes[this.SelectedTargetIndex] };
                }
                /// Single Ally
                else
                {
                    if (this.SelectedTargetIndex > data.AllyStationIndexes.Count - 1) { return null; }

                    return new() { data.AllyStationIndexes[this.SelectedTargetIndex] };
                }
            }
        }



    }

}