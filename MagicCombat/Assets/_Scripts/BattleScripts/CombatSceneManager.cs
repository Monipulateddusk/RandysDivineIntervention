using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum EnvironmentalEffect
{
    IMBUE_FIRE, IMBUE_WATER, IMBUE_ICE, IMBUE_EARTH, IMBUE_LIGHT, IMBUE_DARKNESS,
}

/// <summary>
/// Passed to each AI so they can correctly use the information (if needed to target players and understand the current turn effects)
/// </summary>
public class CombatSceneData
{
    public List<EnvironmentalEffect> environmentalEffects;
    public GameObject playerGO1, playerGO2;
    public GameObject enemyGO1, enemyGO2, enemyGO3, enemyGO4;
}



public class CombatSceneManager : MonoBehaviour
{
    public enum BattleState {PLAYER_1_TURN, PLAYER_2_TURN, ENEMY_1_TURN, ENEMY_2_TURN, ENEMY_3_TURN, ENEMY_4_TURN, START_BATTLE, WON, LOST };
    [SerializeField] List<GameObject> enemyGameObjects = new List<GameObject>();
    [SerializeField] List<GameObject> playerGameObjects = new List<GameObject>();

    CombatSceneData combatSceneData = new CombatSceneData();

    [SerializeField] Transform[] playerBattleStations;
    [SerializeField] Transform[] enemyBattleStations;

    [SerializeField] BattleState battleState;


    // UI
    [Header("UI Variables")]
    [SerializeField]TextMeshProUGUI turnText;



    private void Start()
    {
        battleState = BattleState.START_BATTLE;
        HandleCombat();

    }

    private void SetupCombat()
    {
        // Instanciate enemies
        int i = 0;
        foreach (GameObject obj in Resources.LoadAll("TempPrefabs/"))
        {
            enemyGameObjects.Add(Instantiate(obj, enemyBattleStations[i]));
            enemyGameObjects[i].AddComponent<Health>();
            i++;
        }


        // Instanciate active allies



        // Once set up is done, proceed to the player's turn
        battleState = BattleState.PLAYER_1_TURN;
        UpdateTurnUI();
    }

    void UpdateTurnUI()
    {
        turnText.text = battleState.ToString();
    }


    private void HandleCombat()
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
        HandleCombat();
    }

    private void HandlePlayer1Turn()
    {
        Debug.Log("This is the start of Player 1's turn");
    }

    private void HandlePlayer2Turn()
    {
        Debug.Log("This is the start of Player 2's turn");
    }

    private void HandleEnemy1Turn()
    {
        Debug.Log("This is the start of Enemy 1's turn");
    }

    private void HandleEnemy2Turn()
    {
        Debug.Log("This is the start of Enemy 2's turn");
    }

    private void HandleEnemy3Turn()
    {
        Debug.Log("This is the start of Enemy 3's turn");
    }
    private void HandleEnemy4Turn()
    {
        Debug.Log("This is the start of Enemy 4's turn");
    }

}
