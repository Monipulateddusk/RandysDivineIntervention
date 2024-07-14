using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This component will handle the animations attached onto each component. Speciifically, it will handle light, heavy and other combat animations by utilising Unity's Animation Events
/// to invoke functionality when a specific section the animation plays. Information on how much damage each part of an attack will do will be provided and then that information will be
/// sent to relevent classes when needed to add correct delays.
/// </summary>

[RequireComponent(typeof(Animator))]
public class AnimationControllerComponent : BaseCombatComponent
{
    List<int> attackValues = new List<int>();
    int attackIndex = 0;

    [SerializeField]Animator animator;
    public override void Init(BaseBattleUnit bBU)
    {
        base.Init(bBU);
        animator = GetComponent<Animator>();
        animator.runtimeAnimatorController = bBU.GetBaseUnit().unitAnimator;
    }

    public void SetAttackValues(BattleMoveAction move)
    {
        attackValues.Clear();
        foreach (AttackAction action in move.resolutionInfo.actions)
        {
            attackValues.Add(action.Value);
        }
    }

    /// <summary>
    /// Called by the animation event in the animations of the unit's attack. Sends in the information of how much damage each part of the attack will do to the target,
    /// allowing the health bar to be updated in realtime instead of at the start of the turn
    /// </summary>
    /// <returns></returns>
    public int AttackAnim()
    {
        // Get the damage of that part of the attack
        int damage = attackValues[attackIndex];

        // Increment the index and set it to 0 if it exceeds the list's value
        attackIndex++;
        if(attackIndex >= attackValues.Count)
        {
            attackIndex = 0;
        }
        Debug.Log("Damage dealt is: " + damage);
        
        return damage;
    }

    public Animator GetAnimator() { return animator; }  
}
