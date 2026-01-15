using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using TurnBased;
using UnityEngine;

/// <summary>
/// Passed to each AI so they can correctly use the information (if needed to target players and understand the current turn effects)
/// </summary>
public class CombatSceneData
{
    public List<Element> environmentalEffects;
    public List<BaseBattleUnit> targets;
    public List<BaseBattleUnit> allies;

    public CombatSceneData()
    {
        environmentalEffects = new List<Element>();
        targets = new List<BaseBattleUnit>();
        allies = new List<BaseBattleUnit>();
    }

    public CombatSceneData(List<Element> environmentalEffects, List<BaseBattleUnit> targets, List<BaseBattleUnit> allies)
    {
        this.environmentalEffects = environmentalEffects;
        this.targets = targets;
        this.allies = allies;
    }
}

/// <summary>
/// This will be passed back to the CombatSceneManager class to resolve the selected move to a specific target. 
/// </summary>
public class CombatReturnData
{
    public CombatReturnData() { }
    public CombatReturnData(IBattleMoveAction battleMoveAction, UnitTeam source, List<BaseBattleUnit> u, List<BaseBattleUnit> t)
    {
        this.battleMoveAction = battleMoveAction;
        this.battleElementalMoveAction = null;
        this.teamSource = source;
        targets = t;
        users = u;
    }
    public CombatReturnData(IElementalMoveAction battleMoveAction, UnitTeam source, List<BaseBattleUnit> u, List<BaseBattleUnit> t)
    {
        this.battleMoveAction = null;
        this.battleElementalMoveAction = battleMoveAction;
        this.teamSource = source;
        targets = t;
        users = u;
    }

    public UnitTeam teamSource;
    public IBattleMoveAction battleMoveAction;
    public IElementalMoveAction battleElementalMoveAction;
    public List<BaseBattleUnit> targets = new();
    public List<BaseBattleUnit> users = new();

    public void AddEntryToList(BaseBattleUnit entry, List<BaseBattleUnit> list) { list.Add(entry); }
}

public class CombatSceneManager : MonoBehaviour
{
    private static CombatSceneManager instance;

    public static CombatSceneManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new GameObject("CombatSceneManager").AddComponent<CombatSceneManager>();
            }
            return instance;
        }
    }

    public enum BattleState { PRECOMBAT, PLAYER_1_TURN, PLAYER_2_TURN, ENEMY_1_TURN, ENEMY_2_TURN, ENEMY_3_TURN, ENEMY_4_TURN, START_BATTLE, WON, LOST };
    [SerializeField] List<BaseBattleUnit> enemyUnits = new();
    [SerializeField] List<BaseBattleUnit> playerUnits = new();

    readonly CombatSceneData combatSceneData = new CombatSceneData();
    CombatEnvironmentController combatEnvironmentController = new();

    [SerializeField] Transform[] playerBattleStations;
    [SerializeField] Transform[] enemyBattleStations;

    [SerializeField] BattleState battleState;

    [SerializeField] GameObject arrowGO;

    [Header("UI Variables")]
    [SerializeField] TextMeshProUGUI turnText;

    CombatMediator combatMediator;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            DestroyImmediate(this.gameObject);
        }
    }

    private void Start()
    {
        combatMediator = new CombatMediator(combatEnvironmentController);


        battleState = BattleState.START_BATTLE;
        HandleCombatTurns();

    }

    private void SetupCombat()
    {
        // Instanciate enemies. Extracting and saving their BaseBattleUnit Component
        int i = 0;
        foreach (GameObject obj in Resources.LoadAll("TempPrefabs/Enemies").Cast<GameObject>())
        {
            BaseBattleUnit enemyUnit = Instantiate(obj, enemyBattleStations[i]).GetComponent<BaseBattleUnit>();
            enemyUnit.SetMediator(combatMediator);
            enemyUnit.SetTeam(UnitTeam.ENEMY);
            enemyUnits.Add(enemyUnit);
            i++;
        }


        // Instanciate active allies
        i = 0;
        foreach (GameObject obj in Resources.LoadAll("TempPrefabs/Players").Cast<GameObject>())
        {
            BaseBattleUnit allyUnit = Instantiate(obj, playerBattleStations[i]).GetComponent<BaseBattleUnit>();
            allyUnit.SetMediator(combatMediator);
            allyUnit.SetTeam(UnitTeam.ALLY);
            playerUnits.Add(allyUnit);

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

    private async Task ResolveCombat(MoveSelectionData data)
    {
        // We are declaring the 0 index of the list as in this implementation, the list will never have more entries inside it
        List<UnitData> alliesData = data.Allies.Select(u => u.GetBaseUnit()).ToList();
        List<UnitData> targetData = data.Targets.Select(u => u.GetBaseUnit()).ToList();

        AttackResolutionInfo attackResolutionInfo = data.SelectedMove.ExecuteMove(usersInfo: alliesData, userInfo: data.SourceUnit.GetBaseUnit(),targetsInfo: targetData, targetInfo: data.Targets[0].GetBaseUnit());

        // Call the combat component for the user passing in info on the target. 
        data.SourceUnit.GetCombatComponent().OnEndAttackingCombat += EndTurn;


        // IMPORTANT:: WE ARE RETURNING A NULL COMBAT RETURN DATA WHILE WE WORK ON CREATING UNIT SELECTION!!!!
        await data.SourceUnit.GetCombatComponent().StartCombat(attackResolutionInfo, new CombatReturnData());

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
    void ProcessCombatForUnit(BaseBattleUnit user, List<BaseBattleUnit> allies, List<BaseBattleUnit> targets)
    {
        // Fill out the combat data with the required info that this unit would require. I.e. Possible targets for ally units would only be the enemy units
        combatSceneData.targets = targets;
        combatSceneData.allies = allies;

        // Call the Combat function from the InputManager class and send data about the scene to it
        if (user != null)
        {
            MoveSelectionData data = user.GetMoveSelectorComponent().SelectMove(combatSceneData);
            
            _ = ResolveCombat(data);
        }
    }


    private void HandlePlayer1Turn()
    {
        //Debug.Log("This is the start of Player 1's turn");

        if (playerUnits.Count >= 1)
        {
            StartCoroutine(MoveArrowToTurnObject(playerBattleStations[0]));

            ProcessCombatForUnit(playerUnits[0], playerUnits, enemyUnits);
        }
    }

    private void HandlePlayer2Turn()
    {
        //Debug.Log("This is the start of Player 2's turn");

        if (playerUnits.Count >= 2)
        {
            StartCoroutine(MoveArrowToTurnObject(playerBattleStations[1]));

            ProcessCombatForUnit(playerUnits[1], playerUnits, enemyUnits);
        }
    }

    private void HandleEnemy1Turn()
    {
        //Debug.Log("This is the start of Enemy 1's turn");

        if (enemyUnits.Count >= 1)
        {
            StartCoroutine(MoveArrowToTurnObject(enemyBattleStations[0]));

            ProcessCombatForUnit(enemyUnits[0], enemyUnits, playerUnits);
        }
    }

    private void HandleEnemy2Turn()
    {
        //Debug.Log("This is the start of Enemy 2's turn");

        if (enemyUnits.Count >= 2)
        {
            StartCoroutine(MoveArrowToTurnObject(enemyBattleStations[1]));

            ProcessCombatForUnit(enemyUnits[1], enemyUnits, playerUnits);
        }
    }

    private void HandleEnemy3Turn()
    {
        //Debug.Log("This is the start of Enemy 3's turn");

        if (enemyUnits.Count >= 3)
        {
            StartCoroutine(MoveArrowToTurnObject(enemyBattleStations[2]));

            ProcessCombatForUnit(enemyUnits[2], enemyUnits, playerUnits);
        }
    }
    private void HandleEnemy4Turn()
    {
        //Debug.Log("This is the start of Enemy 4's turn");


        if (enemyUnits.Count >= 4)
        {
            StartCoroutine(MoveArrowToTurnObject(enemyBattleStations[3]));

            ProcessCombatForUnit(enemyUnits[2], enemyUnits, playerUnits);
        }
        
    }

}
