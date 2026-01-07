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
    [SerializeField]Animator animator;
    public override void Init(BaseBattleUnit bBU)
    {
        base.Init(bBU);
        animator = GetComponent<Animator>();
        animator.runtimeAnimatorController = bBU.GetBaseUnit().unitAnimator;
    }

    private void OnDestroy()
    {
        animator = null;
    }

    /// <summary>
    /// Called by the animation event in the animations of the unit's attack. Sends in the information of how much damage each part of the attack will do to the target,
    /// allowing the health bar to be updated in realtime instead of at the start of the turn
    /// </summary>
    /// <returns></returns>
    public void AttackAnim()
    {
        // Call function in combat component to relay info to target
        bBU.GetCombatComponent().ProcessAttack();
    }

    /// <summary>
    /// Called by animation event to determine when an animation is complete. Invoke combatComponent script to enter the next phase of combat
    /// </summary>
    public void EndAttackAnim()
    {
        bBU.GetCombatComponent().EndAttack();
    }

    public Animator GetAnimator() { return animator; }  
}
