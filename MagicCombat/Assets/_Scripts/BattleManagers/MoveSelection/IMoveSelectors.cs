namespace TurnBased.MoveSelection
{
    public interface IMoveSelector
    {
        public abstract IBattleMoveAction SelectMove(BaseBattleUnit unit, SceneData_UnitTurn data);
    }

    public class RandomMoveSelector : IMoveSelector
    {
        public IBattleMoveAction SelectMove(BaseBattleUnit unit, SceneData_UnitTurn data)
        {
            /*  Check to see if the moves List is populated, if not, return a dud move. */
            if (!MoveSelectorUtility.IsMoveListPopulated(unit)) { return new HeavyAttack(); }

            /*  Get a random index. As Random.Range when using Ints is Exclusive, if the number of moves was 3, we'd get a value of 0-2.    */
            int randomIndex = UnityEngine.Random.Range(0, unit.GetBaseUnit().moves.Count);

            /*  Retrieve the selected move. */
            return unit.GetBaseUnit().moves[randomIndex];
        }
    }

    public class SequentialMoveSelector : IMoveSelector
    {
        int curMoveIndex = 0;

        public IBattleMoveAction SelectMove(BaseBattleUnit unit, SceneData_UnitTurn data)
        {
            /*  Check to see if the moves List is populated, if not, return a dud move. */
            if(!MoveSelectorUtility.IsMoveListPopulated(unit)) { return new HeavyAttack(); }

            /*  Check to see if the index is valid, if not, wrap back to the start. */
            if (curMoveIndex > unit.GetBaseUnit().moves.Count)
            {
                curMoveIndex = 0;
            }

            /*  Get the move and increment the index.*/
            IBattleMoveAction selectedMove = unit.GetBaseUnit().moves[curMoveIndex];
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
        public int SelectedMoveIndex { private get; set; }
        public IBattleMoveAction SelectMove(BaseBattleUnit unit, SceneData_UnitTurn data)
        {
            /*  Check to see if the moves List is populated, if not, return a dud move. */
            if (!MoveSelectorUtility.IsMoveListPopulated(unit)) { return new HeavyAttack(); }

            /*  Check to see if the selected index is not exceeding the length of the move List. If so, get a dud move. */
            if (SelectedMoveIndex > unit.GetBaseUnit().moves.Count)
            {
                return new HeavyAttack();
            }

            return unit.GetBaseUnit().moves[SelectedMoveIndex];
        }
    }
}


public static class MoveSelectorUtility
{
    public static bool IsMoveListPopulated(BaseBattleUnit unit)
    {
        if(unit.GetBaseUnit() != null && unit.GetBaseUnit().moves.Count > 0)
        {
            return true;
        } 
        return false;
    }
}