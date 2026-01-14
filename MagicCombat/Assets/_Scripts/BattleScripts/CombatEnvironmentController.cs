using System.Collections.Generic;
using System.Linq;
using TurnBased;
using UnityEngine;

public struct ImbuedEnvironmentElement
{
    public UnitTeam team;
    public Element imbuedEnvironmentElement;

    public ImbuedEnvironmentElement(UnitTeam team, Element element)
    {
        this.team = team;
        this.imbuedEnvironmentElement = element;
    }
}

public class CombatEnvironmentController
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

    private readonly List<ImbuedEnvironmentElement> environmentEffects;

    public CombatEnvironmentController()
    {
        environmentEffects = new();
    }

    public void AddEnvironmentalEffect(Element effect, UnitTeam team, List<BaseBattleUnit> users, List<BaseBattleUnit> targets)
    {
        Debug.Log("Imbuing effect: " + effect.ToString());
        environmentEffects.Add(new ImbuedEnvironmentElement(team, effect));

        /*  As we loop through the environment list, add to a Queue for each effect within the list that belongs to either the Enemy or Ally (Based on the isAlly perameter). */
        var elmEffects = new Queue<ImbuedEnvironmentElement>(environmentEffects.Where(iEE => iEE.team == team));

        if(elmEffects.Count >= 2)
        {
            /*  Once a move is determined and set to be executed, Clear the elemental list of the team specified by isAlly.  */
            for (int i = 0; i < elmEffects.Count; i++)
            {
                Debug.Log("Removing effect: " + elmEffects.ToList()[i].imbuedEnvironmentElement.ToString());
                environmentEffects.Remove(elmEffects.ToList()[i]);
            }

            Element elementA = elmEffects.Dequeue().imbuedEnvironmentElement;
            Element elementB = elmEffects.Dequeue().imbuedEnvironmentElement;


            ProcessElementalMove(elementA, elementB, team, users, targets);
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
    public void ProcessElementalMove(Element elementValueA, Element elementValueB, UnitTeam team, List<BaseBattleUnit> users, List<BaseBattleUnit> targets)
    {
        // Find out what move the elements combine into and do that move to get the info needed to resolve it
        IElementalMoveAction action = ProcessElementalCombination(elementValueA, elementValueB);

        if (action == null)
        {
            Debug.LogError("ELEMENTAL COMBINATION ERROR: NOT VALID!!!");
        }

        List<UnitData> baseUnitsUsers = new();
        List<UnitData> baseUnitsTargets = new();

        foreach (BaseBattleUnit unit in users)
        {
            baseUnitsUsers.Add(unit.GetBaseUnit());
        }
        foreach (BaseBattleUnit unit in targets)
        {
            baseUnitsTargets.Add(unit.GetBaseUnit());
        }


        AttackResolutionInfo info = action.DoElementalMove(usersInfo: baseUnitsUsers, targetsInfo: baseUnitsTargets);

        Debug.Log("PROCESSING MOVE: " + info.moveName);

        CombatReturnData data = new(action, team, users, targets);

        int actionsCount = info.Actions.Count;
        for (int i = 0; i < actionsCount; i++)
        {
            CombatAttackHandler.ProcessAttack(this, info, data);
        }
    }


    IElementalMoveAction ProcessElementalCombination(Element elementValueA, Element elementValueB)
    {
        if ((uint)elementValueA < (uint)ElementalMoveLookUpTable.GetLength(0) &&
            (uint)elementValueB < (uint)ElementalMoveLookUpTable.GetLength(1))
        {
            return ElementalMoveLookUpTable[(uint)elementValueA, (uint)elementValueB];
        }
        return null;
    }
}
