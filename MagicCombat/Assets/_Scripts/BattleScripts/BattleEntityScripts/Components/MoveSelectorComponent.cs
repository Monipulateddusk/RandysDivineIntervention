using System.Collections.Generic;
using TurnBased;
using UnityEngine;


public struct MoveSelectionData
{
    public IBattleMoveAction SelectedMove { get; }

    public StationIndex SourceStationIndex { get; }
    public List<StationIndex?> AllyStationIndexes { get; }
    public List<StationIndex?> TargetStationIndexes { get; }

    public MoveSelectionData(IBattleMoveAction selectedMove, StationIndex sourceStationIndex,  List<StationIndex?> allyStationIndexes, List<StationIndex?> targetStationIndexes)
    {
        this.SelectedMove = selectedMove;
        this.SourceStationIndex = sourceStationIndex;
        this.AllyStationIndexes = allyStationIndexes;
        this.TargetStationIndexes = targetStationIndexes;
    }
}

public interface IMoveSelector
{
    public abstract MoveSelectionData SelectMove(BaseBattleUnit unit, SceneData_UnitTurn data);
}

public abstract class BaseMoveSelector : IMoveSelector
{
    public abstract MoveSelectionData SelectMove(BaseBattleUnit unit, SceneData_UnitTurn data);
}

public class RandomMoveSelector : BaseMoveSelector
{
    public override MoveSelectionData SelectMove(BaseBattleUnit unit, SceneData_UnitTurn data)
    {
        // Select a random move to perform
        //int rIndex = Random.Range(0, unit.battleMoves.Count);

        //IBattleMoveAction selectedMove = battleMoves[rIndex];

        //if (StationManager.Instance.TryGetStationIndexOfIndex(data.SourceUnitIndex, out StationIndex sourceStation)) { return new(); }
        //List<StationIndex?> allyStationIndexes = data.AllyStationIndexes;
        //List<StationIndex?> enemyStationIndexes = data.EnemyStationIndexes;


        //return new(selectedMove, sourceStation, allyStationIndexes, enemyStationIndexes);

        return new();
    }
}

public class SequentialMoveSelector : BaseMoveSelector
{
    int curMoveIndex = 0;

    public override MoveSelectionData SelectMove(BaseBattleUnit unit, SceneData_UnitTurn data)
    {
        //// If the index exceeds the count on the list, set it to the start of the list (0).
        //// This is the main logic to allow for each move to be used in order of the declaration on the scriptable object
        //if (curMoveIndex + 1 > battleMoves.Count)
        //{
        //    curMoveIndex = 0;
        //}

        //IBattleMoveAction selectedMove = battleMoves[curMoveIndex];
        //if (!StationManager.Instance.TryGetStationIndexOfIndex(data.SourceUnitIndex, out StationIndex sourceStation)) { return new(); }
        //List<StationIndex?> allyStationIndexes = data.AllyStationIndexes;
        //List<StationIndex?> enemyStationIndexes = data.EnemyStationIndexes;

        //// Increment the index after everything is decided
        //curMoveIndex++;

        //return new(selectedMove, sourceStation, allyStationIndexes, enemyStationIndexes);
        return new();
    }
}

public class PlayerDrivenMoveSelector : BaseMoveSelector
{
    public override MoveSelectionData SelectMove(BaseBattleUnit unit, SceneData_UnitTurn data)
    {
        return new();
    }
}