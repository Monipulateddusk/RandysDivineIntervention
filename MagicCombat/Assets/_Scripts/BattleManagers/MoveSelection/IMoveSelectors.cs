namespace TurnBased.MoveSelection
{
    public interface IMoveSelector
    {
        public abstract IBattleMove SelectMove(UnitTurnStationIndexesSceneData data);
    }

    public class RandomMoveSelector : IMoveSelector
    {
        public IBattleMove SelectMove(UnitTurnStationIndexesSceneData data)
        {
            /*  Try get the Unit from the source Index. */
            if (!SelectorUtility.TryGetBattleUnitOnStation(data.SourceStationIndex, out BaseBattleUnit unit)) { return null; }

            /*  Check to see if the moves List is populated, if not, return null. */
            if (!SelectorUtility.IsMoveListPopulated(unit)) { return null; }

            System.Collections.Generic.List<IBattleMove> unitMoves = unit.GetBaseUnit().moves;

            /*  Get a random index. As Random.Range when using Ints is Exclusive, if the number of moves was 3, we'd get a value of 0-2.    */
            int randomIndex = UnityEngine.Random.Range(0, unitMoves.Count);

            /*  Retrieve the selected move. */
            return unitMoves[randomIndex];
        }
    }

    public class SequentialMoveSelector : IMoveSelector
    {
        int curMoveIndex = 0;

        public IBattleMove SelectMove(UnitTurnStationIndexesSceneData data)
        {
            /*  Try get the Unit from the source Index. */
            if (!SelectorUtility.TryGetBattleUnitOnStation(data.SourceStationIndex, out BaseBattleUnit unit)) { return null; }

            /*  Check to see if the moves List is populated, if not, return null. */
            if (!SelectorUtility.IsMoveListPopulated(unit)) { return null; }

            System.Collections.Generic.List<IBattleMove> unitMoves = unit.GetBaseUnit().moves;

            /*  Check to see if the index is valid, if not, wrap back to the start. */
            if (curMoveIndex > unitMoves.Count - 1)
            {
                curMoveIndex = 0;
            }

            /*  Get the move and increment the index.*/
            IBattleMove selectedMove = unitMoves[curMoveIndex];
            curMoveIndex++;
            return selectedMove;
        }
    }

    /// <summary>
    /// This selector will be assigned to via the UI or other means for the player. 
    /// External classes will assign to 'selectedMoveIndex' so that when we call 'SelectMove' during runtime, it will select the move selected via the UI.
    /// </summary>
    public class PlayerDrivenMoveSelector : IMoveSelector
    {
        public IBattleMove SelectedMove { private get; set; }
        public IBattleMove SelectMove(UnitTurnStationIndexesSceneData data)
        {
            /*  Try get the Unit from the source Index. */
            if (!SelectorUtility.TryGetBattleUnitOnStation(data.SourceStationIndex, out BaseBattleUnit unit)) { return null; }

            /*  Check to see if the moves List is populated, if not, return null. */
            if (!SelectorUtility.IsMoveListPopulated(unit)) { return null; }

            /*  Check to see if the selected index is not exceeding the length of the move List. If so, get a dud move. */
            if (!DoesSelectedMoveExistInUnitMoves(unit.GetBaseUnit())) {  return null; }

            return this.SelectedMove;
        }

        private bool DoesSelectedMoveExistInUnitMoves(UnitData unitData)
        {
            return unitData.moves.Contains(this.SelectedMove);
        }
    }
}


public static class SelectorUtility
{
    public static bool IsMoveListPopulated(BaseBattleUnit unit)
    {
        if(unit.GetBaseUnit() != null && unit.GetBaseUnit().moves.Count > 0)
        {
            return true;
        } 
        return false;
    }

    public static bool TryGetBattleUnitOnStation(StationIndex sourceStationIndex, out BaseBattleUnit unit)
    {
        if(!StationManager.Instance.TryGetBaseBattleUnitOnStation(sourceStationIndex, out unit)) 
        { 
            return false; 
        }

        return true;

    }
}