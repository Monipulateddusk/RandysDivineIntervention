using System.Collections;
using System.Collections.Generic;
using TurnBased;
using UnityEngine;

public abstract class BaseInputManagerComponent : BaseComponent
{

    public List<IBattleMoveAction> battleMoves = new();

    public BaseInputManagerComponent()
    {
        battleUnit = null;
        unitData = null;
        battleMoves = null;
    }

    public BaseInputManagerComponent(BaseBattleUnit battleUnit, BaseUnit unitData)
    {
        this.battleUnit = battleUnit;
        this.unitData = unitData;

        /*  Set up the List of the Moves the Unit is capable of     */
        this.battleMoves = new();
        this.battleMoves = unitData.moves;
    }

    public abstract CombatReturnData Combat(CombatSceneData data);

}

public class RandomInputManagerComponent : BaseInputManagerComponent
{
    RandomInputManagerComponent()
    {
        battleUnit = null;
        unitData = null;
        battleMoves = null;
    }

    public RandomInputManagerComponent(BaseBattleUnit battleUnit, BaseUnit unitData)
    {
        this.battleUnit = battleUnit;
        this.unitData = unitData;

        /*  Set up the List of the Moves the Unit is capable of     */
        this.battleMoves = new();
        this.battleMoves = unitData.moves;
    }

    /// <summary>
    /// As this will be a random input manager (used for lower tier enemies and to test things) we will be making use of randomisers to select moves and targets
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public override CombatReturnData Combat(CombatSceneData data)
    {
        // Select a random move to perform
        int rIndex = Random.Range(0, battleMoves.Count);

        IBattleMoveAction selectedMove = battleMoves[rIndex];

        // Use the param of the function to select between targets
        rIndex = Random.Range(0, data.possibleTargets.Count);
        BaseBattleUnit target = data.possibleTargets[rIndex];

        return new CombatReturnData(selectedMove, battleUnit, target);
    }
}

public class SequentialInputManagerComponent : BaseInputManagerComponent
{
    int curMoveIndex = 0;

    SequentialInputManagerComponent()
    {
        battleUnit = null;
        unitData = null;
        battleMoves = null;
    }

    public SequentialInputManagerComponent(BaseBattleUnit battleUnit, BaseUnit unitData)
    {
        this.battleUnit = battleUnit;
        this.unitData = unitData;

        /*  Set up the List of the Moves the Unit is capable of     */
        this.battleMoves = new();
        this.battleMoves = unitData.moves;
    }

    /// <summary>
    /// This is a Sequential input manager. Therefore, moves will be selected in the order they are stored in the list.
    /// This information would be conveyed to designers so they are aware how to order the moves to their liking
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public override CombatReturnData Combat(CombatSceneData data)
    {
        // If the index exceeds the count on the list, set it to the start of the list (0).
        // This is the main logic to allow for each move to be used in order of the declaration on the scriptable object
        if (curMoveIndex + 1 > battleMoves.Count)
        {
            curMoveIndex = 0;
        }

        IBattleMoveAction selectedMove = battleMoves[curMoveIndex];

        // Increment the index after everything is decided
        curMoveIndex++;

        // We will still randomly gen a target from the possible targets
        // Use the param of the function to select between targets
        int rIndex = Random.Range(0, data.possibleTargets.Count);
        BaseBattleUnit target = data.possibleTargets[rIndex];

        return new CombatReturnData(selectedMove, battleUnit, target);
    }
}