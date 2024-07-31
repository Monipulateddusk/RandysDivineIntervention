using System.Collections;
using System.Collections.Generic;
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
    [Header("Inspector variables")]
    [SerializeField] List<EnvironmentalEffect> effects = new();

    // Keep track of what the allies and the enemies are doing to display what effect is active
    EnvironmentalEffect allyEnvirEffect, enemyEnvirEffect;

    public void ConvertEnvirElemToEffect()
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
        BattleMoveAction action = curEffect.ConvertToMove(combinedEffect);


        // Once a move is determined and set to be executed, set the current effect of either the player or enemy
        curEffect = null;
    }
}
