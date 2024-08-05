using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

/// <summary>
/// Passed to each AI so they can correctly use the information (if needed to target players and understand the current turn effects)
/// </summary>
public class CombatSceneData
{
    public List<EnvironmentalElement> environmentalEffects;
    public List<BaseBattleUnit> possibleTargets;
}

/// <summary>
/// This will be passed back to the CombatSceneManager class to resolve the selected move to a specific target. 
/// </summary>
public class CombatReturnData
{
    public CombatReturnData() { }
    public CombatReturnData(BattleMoveAction battleMoveAction, List<BaseBattleUnit> u, List<BaseBattleUnit> t)
    {
        this.battleMoveAction = battleMoveAction;
        targets = t;
        users = u;
    }
    public CombatReturnData(BattleMoveAction battleMoveAction, BaseBattleUnit u, BaseBattleUnit t)
    {
        this.battleMoveAction = battleMoveAction;
        targets.Add(t);
        users.Add(u);
    }

    public BattleMoveAction battleMoveAction;
    public List<BaseBattleUnit> targets = new List<BaseBattleUnit>();
    public List<BaseBattleUnit> users = new List<BaseBattleUnit>();

    public void AddEntryToList(BaseBattleUnit entry, List<BaseBattleUnit> list) { list.Add(entry); }
}

public class CombatSceneManager : MonoBehaviour
{
    public enum BattleState { PRECOMBAT, PLAYER_1_TURN, PLAYER_2_TURN, ENEMY_1_TURN, ENEMY_2_TURN, ENEMY_3_TURN, ENEMY_4_TURN, START_BATTLE, WON, LOST };
    [SerializeField] List<BaseBattleUnit> enemyUnits = new();
    [SerializeField] List<BaseBattleUnit> playerUnits = new();

    readonly CombatSceneData combatSceneData = new();

    [SerializeField] Transform[] playerBattleStations;
    [SerializeField] Transform[] enemyBattleStations;

    [SerializeField] BattleState battleState;

    [SerializeField] GameObject arrowGO;
    // UI
    [Header("UI Variables")]
    [SerializeField] TextMeshProUGUI turnText;



    private void Start()
    {
        battleState = BattleState.START_BATTLE;
        HandleCombatTurns();

    }

    private void SetupCombat()
    {
        // Instanciate enemies. Extracting and saving their BaseBattleUnit Component
        int i = 0;
        foreach (GameObject obj in Resources.LoadAll("TempPrefabs/Enemies").Cast<GameObject>())
        {
            enemyUnits.Add(Instantiate(obj, enemyBattleStations[i]).GetComponent<BaseBattleUnit>());
            i++;
        }


        // Instanciate active allies
        i = 0;
        foreach (GameObject obj in Resources.LoadAll("TempPrefabs/Players").Cast<GameObject>())
        {
            playerUnits.Add(Instantiate(obj, playerBattleStations[i]).GetComponent<BaseBattleUnit>());
            i++;
        }


        // Once set up is done, proceed to the next phase
        battleState  = BattleState.PRECOMBAT;
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
            case BattleState.PRECOMBAT:

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
        // We are declaring the 0 index of the list as in this implementation, the list will never have more entries inside it
        AttackResolutionInfo attackResolutionInfo = data.battleMoveAction.DoMove(userInfo: data.users[0].GetBaseUnit(), targetInfo: data.targets[0].GetBaseUnit());

        // Call the combat component for the user passing in info on the target. 
        data.users[0].GetCombatComponent().StartCombat(attackResolutionInfo, data);
        data.users[0].GetCombatComponent().OnEndCombat += EndTurn;
    }

    /// <summary>
    /// Is the way to loop through the turns in the correct order. This will be the function that will be subscribed to the finishing of movement for the units
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
    /// Turns will be handled by sending information about the scene to each battle entity, recieving info from the combat about what attack and to what unit after which then resolving that combat. 
    /// 
    /// TO DO: Delays to invoke animation, movement around scene, death anims, etc
    /// </summary>
    void ProcessCombatForUnit(BaseBattleUnit user, List<BaseBattleUnit> targets)
    {
        // Fill out the combat data with the required info that this unit would require. I.e. Possible targets for ally units would only be the enemy units
        combatSceneData.possibleTargets = targets;

        // Call the Combat function from the InputManager class and send data about the scene to it
        if (user != null)
        {
            CombatReturnData data = user.GetInputManagerComponent().Combat(combatSceneData);
            ResolveCombat(data);
        }
    }


    private void HandlePlayer1Turn()
    {
        //Debug.Log("This is the start of Player 1's turn");

        if (playerUnits.Count >= 1)
        {
            StartCoroutine(MoveArrowToTurnObject(playerBattleStations[0]));

            ProcessCombatForUnit(playerUnits[0], enemyUnits);
        }
    }

    private void HandlePlayer2Turn()
    {
        //Debug.Log("This is the start of Player 2's turn");

        if (playerUnits.Count >= 2)
        {
            StartCoroutine(MoveArrowToTurnObject(playerBattleStations[1]));

            ProcessCombatForUnit(playerUnits[1], enemyUnits);
        }
    }

    private void HandleEnemy1Turn()
    {
        //Debug.Log("This is the start of Enemy 1's turn");

        if (enemyUnits.Count >= 1)
        {
            StartCoroutine(MoveArrowToTurnObject(enemyBattleStations[0]));

            ProcessCombatForUnit(enemyUnits[0], playerUnits);
        }
    }

    private void HandleEnemy2Turn()
    {
        //Debug.Log("This is the start of Enemy 2's turn");

        if (enemyUnits.Count >= 2)
        {
            StartCoroutine(MoveArrowToTurnObject(enemyBattleStations[1]));

            ProcessCombatForUnit(enemyUnits[1], playerUnits);
        }
    }

    private void HandleEnemy3Turn()
    {
        //Debug.Log("This is the start of Enemy 3's turn");

        if (enemyUnits.Count >= 3)
        {
            StartCoroutine(MoveArrowToTurnObject(enemyBattleStations[2]));

            ProcessCombatForUnit(enemyUnits[2], playerUnits);
        }
    }
    private void HandleEnemy4Turn()
    {
        //Debug.Log("This is the start of Enemy 4's turn");


        if (enemyUnits.Count >= 4)
        {
            StartCoroutine(MoveArrowToTurnObject(enemyBattleStations[3]));

            ProcessCombatForUnit(enemyUnits[3], playerUnits);
        }
        
    }

}
