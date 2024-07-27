using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Same order as 'Element' enum in BaseUnit Class for easy integer conversion between the two
/// </summary>
public enum EnvironmentalElement
{
    NULL, IMBUE_FIRE, IMBUE_WATER, IMBUE_ICE, IMBUE_EARTH, IMBUE_LIGHT, IMBUE_DARKNESS,
}


public class CombatEnvironmentController : MonoBehaviour
{
    [Header("Inspector variables")]
    [SerializeField] List<EnvironmentalEffect> effects = new();


    Dictionary<EnvironmentalElement, EnvironmentalEffect> effectsDict;
    // Keep track of what the allies and the enemies are doing to display what effect is active
    EnvironmentalEffect allyEnvirEffect, enemyEnvirEffect;

    public void AddEnvironmentalEffect(EnvironmentalEffect effect, bool isAlly)
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

                }

                break;

            case false:


            break;
        }

    }

    void ProcessElementalMove(EnvironmentalEffect curEffect, EnvironmentalEffect combinedEffect, List<BaseUnit> users, List<BaseUnit> targets)
    {
        // BattleMoveAction action = curEffect.ConvertToMove(combinedEffect);
    }
}
