using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    [SerializeField]List<BattleMoveAction> battleMoves = new List<BattleMoveAction>();
    BaseBattleUnit unitSelf;

    /// <summary>
    /// Creates the component on the gameObject and returns the created input manager for the invoking class.
    /// </summary>
    /// <param name="gameObject"></param>
    /// <param name="unit"></param>
    /// <returns></returns>
    /// 
    /// Desired this 'Factory' method as OpenAI calls it as I needed a way to declare the base battle unit before awake occoured, however, we can likely now use this system in my other components
    public static InputManager CreateInstance(GameObject gameObject, BaseBattleUnit unit)
    {
        InputManager iM = gameObject.AddComponent<InputManager>();

        iM.Initialize(unit);      

        return iM;
    }

    void Initialize(BaseBattleUnit unit)
    {
        unitSelf = unit;
        unitSelf.OnUnitCreated += SetUpInputManager;
    }
    private void Awake()
    {
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

        return new CombatReturnData(selectedMove, unitSelf, target);
    }

    public void SetBaseBattleUnit(BaseBattleUnit comp) { this.unitSelf = comp; }
}
