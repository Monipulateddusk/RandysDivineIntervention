using System.Collections;
using System.Collections.Generic;
using TurnBased;
using UnityEngine;

public abstract class BaseInputManagerComponent : BaseCombatComponent
{
    [HideInInspector] public List<IBattleMoveAction> battleMoves = new();

    public override void Init(BaseBattleUnit bBU)
    {
        base.Init(bBU);
        bBU.OnUnitCreated += SetUpInputManager;
    }

    void SetUpInputManager(BaseUnit unitData)
    {
        // Ensure the list is clear and insert the moves the unit can use into the local list for use within the class
        battleMoves.Clear();
        battleMoves = unitData.moves;
    }

    public abstract CombatReturnData Combat(CombatSceneData data);

}

public class RandomInputManagerComponent : BaseInputManagerComponent
{
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

        return new CombatReturnData(selectedMove, bBU, target);
    }
}

public class SequentialInputManagerComponent : BaseInputManagerComponent
{
    int curMoveIndex = 0;

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

        return new CombatReturnData(selectedMove, bBU, target);
    }
}