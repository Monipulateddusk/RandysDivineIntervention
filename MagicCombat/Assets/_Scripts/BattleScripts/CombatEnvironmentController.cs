using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Same order as 'Element' enum in BaseUnit Class for easy integer conversion between the two
/// </summary>
public enum EnvironmentalElement
{
   IMBUE_FIRE, IMBUE_WATER, IMBUE_ICE, IMBUE_EARTH, IMBUE_LIGHT, IMBUE_DARKNESS, NULL,
}


public class CombatEnvironmentController : MonoBehaviour
{
    private static CombatEnvironmentController _instance;
    public static CombatEnvironmentController Instance
    {
        get
        {
            if( _instance == null)
            {
                Debug.LogError("CombatEnvironmentController is null");
            }
            return _instance;
        }
    }

    [Header("Inspector variables")]
    [SerializeField] List<EnvironmentalEffect> effects = new();

    Dictionary<Element, EnvironmentalEffect> effectDict; 

    // Keep track of what the allies and the enemies are doing to display what effect is active
    EnvironmentalEffect allyEnvirEffect, enemyEnvirEffect;


    private void Awake()
    {
        // Populate the dictionary
        effectDict = new Dictionary<Element, EnvironmentalEffect>();

        // This adds a means for other classes to input an Element enum and get the corresponding elemental effect. 
        // It is imperative that the order of the Element Enum and the EnvironmentalEffect list match to the correct element type.
        // E.g. Element index 0 is fire and so fire should also be element 0 in effects list
        for(int i = 0; i < effects.Count; i++){
            effectDict.Add((Element)i, effects[i]);
        }
    }

    /// <summary>
    /// Called by exterior classes to use their Element Enum when imbuing the environment for elemental attacks
    /// </summary>
    public void AddEnvironmentalElement()
    {

    }

    public void AddEnvironmentalEffect(EnvironmentalEffect effect, List<BaseBattleUnit> users, List<BaseBattleUnit> targets, bool isAlly)
    {
        // If there is no environmental effect active, make it so. If not, process the environmental move
        switch (isAlly)
        {
            case true:

                if(allyEnvirEffect != null)
                {
                    allyEnvirEffect = effect;
                }
                else
                {
                    ProcessElementalMove(ref allyEnvirEffect, effect, users, targets);
                }

                break;

            case false:
                if(enemyEnvirEffect != null)
                {
                    enemyEnvirEffect = effect;
                }
                else
                {
                    ProcessElementalMove(ref enemyEnvirEffect, effect, users, targets);
                }

            break;
        }

    }

    /// <summary>
    /// This function determines what the elemental move will be and based on that info, sends the info to another function to resolve it. 
    /// Once resolved, set the 
    /// </summary>
    /// <param name="curEffect"></param>
    /// <param name="combinedEffect"></param>
    /// <param name="users"></param>
    /// <param name="targets"></param>
    void ProcessElementalMove(ref EnvironmentalEffect curEffect, EnvironmentalEffect combinedEffect, List<BaseBattleUnit> users, List<BaseBattleUnit> targets)
    {
        // Find out what move the elements combine into and do that move to get the info needed to resolve it
        BattleMoveAction action = curEffect.ConvertToMove(combinedEffect);

        // Convert basebattleunit to baseunit for each list using LINQ for shorthand usage.
        List<BaseUnit> baseUnitsUsers = users.Select(user => user.GetBaseUnit()).ToList();
        List<BaseUnit> baseUnitsTargets = targets.Select(user => user.GetBaseUnit()).ToList();

        AttackResolutionInfo info = action.DoMove(usersInfo: baseUnitsUsers, targetsInfo: baseUnitsTargets);

        // Once a move is determined and set to be executed, set the current effect of either the player or enemy
        curEffect = null;

        // Execute the move
        ExecuteEnvirMove(info, users, targets);
    }

    /// <summary>
    /// This function will be delegated else where. Having two seperate systems to execute moves is not good and is the beginnings of spaghetti code.
    /// Another class as a static to handle this will be better in the future
    /// </summary>
    /// <param name="info"></param>
    /// <param name="users"></param>
    /// <param name="targets"></param>
    void ExecuteEnvirMove(AttackResolutionInfo info, List<BaseBattleUnit> users, List<BaseBattleUnit> targets)
    {
        Debug.LogWarning("EXECUTING MOVE: " + info.moveName);
        foreach(var action in info.Actions)
        {
            switch(action.Type)
            {
                case AttackAction.ActionType.DAMAGE:
                    foreach(var target in targets)
                    {
                        target.Damage(action.Value);
                    }

                    break;

                case AttackAction.ActionType.HEALING:
                    foreach (var user in users)
                    {
                        user.Heal(action.Value);
                    }


                    break;

                case AttackAction.ActionType.STATUS_EFFECT:
                    Debug.Log("Imbuing target with: " + action.StaEffect);
                    break;

                case AttackAction.ActionType.IMBUE_ENVIRONMENTS:

                    break;
            
            }
        }
    }

}
