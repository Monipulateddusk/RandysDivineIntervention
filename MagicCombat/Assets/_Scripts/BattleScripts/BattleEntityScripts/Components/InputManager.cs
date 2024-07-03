using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : BaseCombatComponent
{
    [SerializeField]List<BattleMoveAction> battleMoves = new List<BattleMoveAction>();

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

    /// <summary>
    /// As this will be a random input manager (used for lower tier enemies and to test things) we will be making use of randomisers to select moves and targets
    /// </summary>
    public CombatReturnData Combat(CombatSceneData data)
    { 
        // Select a random move to perform
        int rIndex = Random.Range(0, battleMoves.Count);

        BattleMoveAction selectedMove = battleMoves[rIndex];

        // Use the param of the function to select between targets
        rIndex = Random.Range(0, data.possibleTargets.Count);
        BaseBattleUnit target = data.possibleTargets[rIndex];

        return new CombatReturnData(selectedMove, bBU, target);
    }


}
