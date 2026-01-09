using System.Collections.Generic;
using System.Linq;
using TurnBased;
using UnityEngine;

public class CombatEnvironmentController : MonoBehaviour
{
    /*  Elemental Look-Up Table     */
    /*  Same size as the Element Enum. Units are able to imbue the Environment with their Element to do an attack if their Ally participates.   */
    readonly IElementalMoveAction[,] ElementalMoveLookUpTable = new IElementalMoveAction[7, 7]
    {   /*  NULL,   Fire                Water                   Ice                     Earth                       Light                   Darkness    */
        {   null,   null,               null,                   null,                   null,                       null,                   null,  },   /* NULL     */
        {   null,   new EM_Inferno(),   new EM_Steam(),         new EM_Frostburn(),     new EM_Volcano(),           null,                   null,  },   /* Fire     */
        {   null,   new EM_Steam(),     new EM_Tsunami(),       new EM_HailCloak(),     new EM_Wellspring(),        null,                   null,  },   /* Water    */
        {   null,   new EM_Frostburn(), new EM_HailCloak(),     new EM_IceAge(),        new EM_FrostLock(),         null,                   null,  },   /* Ice      */
        {   null,   new EM_Volcano(),   new EM_Wellspring(),    new EM_FrostLock(),     new EM_Fissure(),           null,                   null,  },   /* Earth    */
        {   null,   null,               null,                   null,                   null,                       null,                   null,  },   /* Light    */
        {   null,   null,               null,                   null,                   null,                       null,                   null,  }    /* Darkness */
    };

    [SerializeField] private static CombatEnvironmentController _instance;
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
    [SerializeField] List<Element> effects = new();


    [SerializeField] SerializableDictionary<string, int> keyValuePairs = new();

   

    // Keep track of what the allies and the enemies are doing to display what effect is active
    Element allyEnvirEffect, enemyEnvirEffect;


    private void Awake()
    {

        // Declare the instance
        _instance = this;

    }

    /// <summary>
    /// Called by exterior classes to use their Element Enum when imbuing the environment for elemental attacks
    /// </summary>
    public void AddEnvironmentalElement(Element element)
    {

    }

    public void AddEnvironmentalEffect(Element effect, List<BaseBattleUnit> users, List<BaseBattleUnit> targets, bool isAlly)
    {
        // If there is no environmental effect active, make it so. If not, process the environmental move
        switch (isAlly)
        {
            case true:

                if(allyEnvirEffect != Element.NULL)
                {
                    allyEnvirEffect = effect;
                }
                else
                {
                    ProcessElementalMove(allyEnvirEffect, effect, users, targets);
                }

                break;

            case false:
                if(enemyEnvirEffect != Element.NULL)
                {
                    enemyEnvirEffect = effect;
                }
                else
                {
                    ProcessElementalMove(enemyEnvirEffect, effect, users, targets);
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
    void ProcessElementalMove(Element elementValueA, Element elementValueB, List<BaseBattleUnit> users, List<BaseBattleUnit> targets)
    {
        // Find out what move the elements combine into and do that move to get the info needed to resolve it
        IElementalMoveAction action = ProcessElementalCombination(elementValueA, elementValueB);

        // Convert basebattleunit to baseunit for each list using LINQ for shorthand usage.
        List <UnitData> baseUnitsUsers = users.Select(user => user.GetBaseUnit()).ToList();
        List<UnitData> baseUnitsTargets = targets.Select(user => user.GetBaseUnit()).ToList();

        AttackResolutionInfo info = action.DoElementalMove(usersInfo: baseUnitsUsers, targetsInfo: baseUnitsTargets);

        // Once a move is determined and set to be executed, clear the elemental list of either the Ally or Enemy depending
   

        // Execute the move by calling the ProcessAttack function in CombatAttackHandler as many times as there are Actions in the attack
        foreach(AttackAction a in info.Actions)
        {
            // Convert into CombatReturnData
            CombatReturnData data = new(action, users, targets);
            CombatAttackHandler.ProcessAttack(info, data);
        }
    }


    IElementalMoveAction ProcessElementalCombination(Element elementValueA, Element elementValueB)
    {
        return ElementalMoveLookUpTable[(int)elementValueA, (int)elementValueB];
    }
}
