using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Same order as 'Element' enum in BaseUnit Class for easy integer conversion between the two
/// </summary>
public enum EnvironmentalEffect
{
    NULL, IMBUE_FIRE, IMBUE_WATER, IMBUE_ICE, IMBUE_EARTH, IMBUE_LIGHT, IMBUE_DARKNESS,
}

/// <summary>
/// Passed to each AI so they can correctly use the information (if needed to target players and understand the current turn effects)
/// </summary>
public class CombatSceneData
{
    public List<EnvironmentalEffect> environmentalEffects;
    public List<BaseBattleUnit> possibleTargets;
}

/// <summary>
/// This will be passed back to the CombatSceneManager class to resolve the selected move to a specific target. 
/// </summary>
public class CombatReturnData
{
    public CombatReturnData(BattleMoveAction battleMoveAction, BaseBattleUnit user, BaseBattleUnit target)
    {
        this.battleMoveAction = battleMoveAction;
        this.target = target;
        this.user = user;
    }

    public BattleMoveAction battleMoveAction;
    public BaseBattleUnit target, user;
}

public class CombatSceneManager : MonoBehaviour
{
    public enum BattleState {PLAYER_1_TURN, PLAYER_2_TURN, ENEMY_1_TURN, ENEMY_2_TURN, ENEMY_3_TURN, ENEMY_4_TURN, START_BATTLE, WON, LOST };
    [SerializeField] List<BaseBattleUnit> enemyUnits = new List<BaseBattleUnit>();
    [SerializeField] List<BaseBattleUnit> playerUnits = new List<BaseBattleUnit>();

    CombatSceneData combatSceneData = new CombatSceneData();

    [SerializeField] Transform[] playerBattleStations;
    [SerializeField] Transform[] enemyBattleStations;

    [SerializeField] BattleState battleState;

    [SerializeField] GameObject arrowGO;
    // UI
    [Header("UI Variables")]
    [SerializeField]TextMeshProUGUI turnText;



    private void Start()
    {
        battleState = BattleState.START_BATTLE;
        HandleCombatTurns();

    }

    private void SetupCombat()
    {
        // Instanciate enemies. Extracting and saving their BaseBattleUnit Component
        int i = 0;
        foreach (GameObject obj in Resources.LoadAll("TempPrefabs/Enemies"))
        {
            enemyUnits.Add(Instantiate(obj, enemyBattleStations[i]).GetComponent<BaseBattleUnit>());
            i++;
        }


        // Instanciate active allies
        i = 0;
        foreach (GameObject obj in Resources.LoadAll("TempPrefabs/Players"))
        {
            playerUnits.Add(Instantiate(obj, playerBattleStations[i]).GetComponent<BaseBattleUnit>());
            i++;
        }


        // Once set up is done, proceed to the player's turn
        battleState = BattleState.PLAYER_1_TURN;
        UpdateTurnUI();
        HandleCombatTurns();
    }

    void UpdateTurnUI()
    {
        turnText.text = battleState.ToString();
    }
    IEnumerator MoveArrowToTurnObject(Transform target)
    {
        arrowGO.transform.parent = target.transform;
        yield return new WaitForSeconds(.1f);
        arrowGO.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

    }

    private void HandleCombatTurns()
    {
        switch (battleState)
        {
            case BattleState.START_BATTLE:
                SetupCombat();
                break;
            case BattleState.PLAYER_1_TURN:
                HandlePlayer1Turn();
                break;
            case BattleState.PLAYER_2_TURN:
                HandlePlayer2Turn();
                break;
            case BattleState.ENEMY_1_TURN:
                HandleEnemy1Turn();
                break;
            case BattleState.ENEMY_2_TURN:
                HandleEnemy2Turn();
                break;
            case BattleState.ENEMY_3_TURN:
                HandleEnemy3Turn();
                break;
            case BattleState.ENEMY_4_TURN:
                HandleEnemy4Turn();
                break;
        }
    }

    private void ResolveCombat(CombatReturnData data)
    {
        AttackResolutionInfo attackResolutionInfo = data.battleMoveAction.DoMove(data.user.GetBaseUnit(), data.target.GetBaseUnit());

        // If there are multiple actions to handle, handle them seperatly
        foreach (AttackAction action in attackResolutionInfo.actions)
        {
            switch (action.Type)
            {
                case AttackAction.ActionType.DAMAGE:
                   // Debug.LogWarning("Dealing Damage to " + data.target + " by: " + action.Value);

                    data.target.Damage(action.Value);

                    break;
                case AttackAction.ActionType.HEALING:
                    break;
            }
   
        }
    }

    /// <summary>
    /// Is the way to loop through the turns in the correct order.
    /// </summary>
    public void EndTurn()
    {
        // Convert the enum to int and check if the value is the 4th enemy's turn, if so, set it back to the player 1's turn.
        // If not, increment the battlestate
        int turnNumber = (int)battleState;
        
        if( turnNumber == (int)BattleState.ENEMY_4_TURN)
        {
            battleState = 0;
        }
        else
        {
            battleState++;
        }
        UpdateTurnUI();
        HandleCombatTurns();
    }

    /// <summary>
    /// Turns will be handled by sending information about the scene to each battle entity. This will be handled just once and thus the function doesn't need to be an IEnumerator with a while loop
    /// I then need to be able to handle information back from each input manager(?). Once that info is back, then we can invoke correct functions for losing/gaining health as well as call correct
    /// anims in function. Basically, I need another function that may be public or called upon event firing that relays all important info about a move.
    /// </summary>
    private void HandlePlayer1Turn()
    {
        Debug.Log("This is the start of Player 1's turn");
        StartCoroutine(MoveArrowToTurnObject(playerBattleStations[0]));

        // Fill out the combat data with the required info that this unit would require. I.e. Possible targets would only be the enemy units
        combatSceneData.possibleTargets = enemyUnits;

        // Call the Combat function from the InputManager class and send data about the scene to it
        if(playerUnits[0] != null)
        {
            CombatReturnData cRD = playerUnits[0].GetInputManagerComponent().Combat(combatSceneData);
            ResolveCombat(cRD);
        }
     

    }

    private void HandlePlayer2Turn()
    {
        Debug.Log("This is the start of Player 2's turn");
        StartCoroutine(MoveArrowToTurnObject(playerBattleStations[1]));
    }

    private void HandleEnemy1Turn()
    {
        Debug.Log("This is the start of Enemy 1's turn");
        StartCoroutine(MoveArrowToTurnObject(enemyBattleStations[0]));
    }

    private void HandleEnemy2Turn()
    {
        Debug.Log("This is the start of Enemy 2's turn");
        StartCoroutine(MoveArrowToTurnObject(enemyBattleStations[1]));
    }

    private void HandleEnemy3Turn()
    {
        Debug.Log("This is the start of Enemy 3's turn");
        StartCoroutine(MoveArrowToTurnObject(enemyBattleStations[2]));
    }
    private void HandleEnemy4Turn()
    {
        Debug.Log("This is the start of Enemy 4's turn");
        StartCoroutine(MoveArrowToTurnObject(enemyBattleStations[3]));
    }

}
