using System;
using System.Collections;
using System.Collections.Generic;
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
    public GameObject enemyGO1, enemyGO2, enemyGO3;
}



public class CombatSceneManager : MonoBehaviour
{
    public enum BattleState { START_BATTLE, PLAYER_1_TURN, PLAYER_2_TURN, ENEMY_1_TURN, ENEMY_2_TURN, ENEMY_3_TURN, WON, LOST };

    [SerializeField] List<GameObject> enemyGameObjects = new List<GameObject>();
    [SerializeField] List<GameObject> playerGameObjects = new List<GameObject>();

    CombatSceneData combatSceneData = new CombatSceneData();

    [SerializeField] Transform[] playerBattleStations;
    [SerializeField] Transform[] enemyBattleStations;

    [SerializeField] BattleState battleState;


    private void Start()
    {
        battleState = BattleState.START_BATTLE;
        SetupCombat();

    }

    private void SetupCombat()
    {
        // Instanciate enemies

        GameObject prefab = Resources.Load("TempPrefabs/BlueEnemyGO") as GameObject;
        enemyGameObjects.Add(Instantiate(prefab, enemyBattleStations[0]));
        prefab = Resources.Load("TempPrefabs/EnemyGO") as GameObject;
        enemyGameObjects.Add(Instantiate(prefab, enemyBattleStations[1]));
        prefab = Resources.Load("TempPrefabs/RedEnemyGO") as GameObject;
        enemyGameObjects.Add(Instantiate(prefab, enemyBattleStations[2]));


        // Instanciate active allies


    }
}
