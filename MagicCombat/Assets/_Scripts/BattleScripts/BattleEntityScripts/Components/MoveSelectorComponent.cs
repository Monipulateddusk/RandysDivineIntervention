using System.Collections.Generic;
using TurnBased;
using UnityEngine;


public struct MoveSelectionData
{
    public IBattleMoveAction SelectedMove;

    public StationIndex SourceStationIndex;
    public List<StationIndex?> AllyStationIndexes;
    public List<StationIndex?> TargetStationIndexes;
}

public abstract class BaseMoveSelectorComponent : BaseComponent
{

    protected List<IBattleMoveAction> battleMoves = new();

    public BaseMoveSelectorComponent()
    {
        battleUnit = null;
        unitData = null;
        battleMoves = null;
    }

    public BaseMoveSelectorComponent(BaseBattleUnit battleUnit, UnitData unitData)
    {
        this.battleUnit = battleUnit;
        this.unitData = unitData;

        /*  Set up the List of the Moves the Unit is capable of     */
        this.battleMoves = unitData.moves;
    }

    public abstract MoveSelectionData SelectMove(SceneData_UnitTurn data);

    public List<IBattleMoveAction> GetBattleMoves() => battleMoves; 
}

public class RandomMoveSelectorComponent : BaseMoveSelectorComponent
{
    public RandomMoveSelectorComponent(BaseBattleUnit battleUnit, UnitData unitData)
    {
        this.battleUnit = battleUnit;
        this.unitData = unitData;

        /*  Set up the List of the Moves the Unit is capable of     */
        this.battleMoves = unitData.moves;
    }

    /// <summary>
    /// As this will be a random input manager (used for lower tier enemies and to test things) we will be making use of randomisers to select moves and targets
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public override MoveSelectionData SelectMove(SceneData_UnitTurn data)
    {
        // Select a random move to perform
        int rIndex = Random.Range(0, battleMoves.Count);

        IBattleMoveAction selectedMove = battleMoves[rIndex];
        StationIndex? sourceStation = BattleMediator.Instance.GetStationIndexOfUnitIndex(data.SourceUnitIndex);
        List<StationIndex?> allyStationIndexes = data.AllyStationIndexes;
        List<StationIndex?> enemyStationIndexes = data.EnemyStationIndexes;


        return new() { SourceStationIndex = sourceStation.Value, SelectedMove = selectedMove, AllyStationIndexes = allyStationIndexes, TargetStationIndexes = enemyStationIndexes };
    }
}

public class SequentialMoveSelectorComponent : BaseMoveSelectorComponent
{
    int curMoveIndex = 0;

    public SequentialMoveSelectorComponent(BaseBattleUnit battleUnit, UnitData unitData)
    {
        this.battleUnit = battleUnit;
        this.unitData = unitData;

        /*  Set up the List of the Moves the Unit is capable of     */
        this.battleMoves = unitData.moves;
    }

    /// <summary>
    /// This is a Sequential input manager. Therefore, moves will be selected in the order they are stored in the list.
    /// This information would be conveyed to designers so they are aware how to order the moves to their liking
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public override MoveSelectionData SelectMove(SceneData_UnitTurn data)
    {
        // If the index exceeds the count on the list, set it to the start of the list (0).
        // This is the main logic to allow for each move to be used in order of the declaration on the scriptable object
        if (curMoveIndex + 1 > battleMoves.Count)
        {
            curMoveIndex = 0;
        }

        IBattleMoveAction selectedMove = battleMoves[curMoveIndex];
        StationIndex? sourceStation = BattleMediator.Instance.GetStationIndexOfUnitIndex(data.SourceUnitIndex);
        List<StationIndex?> allyStationIndexes = data.AllyStationIndexes;
        List<StationIndex?> enemyStationIndexes = data.EnemyStationIndexes;

        // Increment the index after everything is decided
        curMoveIndex++;

        return new() { SourceStationIndex = sourceStation.Value, SelectedMove = selectedMove, AllyStationIndexes = allyStationIndexes, TargetStationIndexes = enemyStationIndexes };
    }
}