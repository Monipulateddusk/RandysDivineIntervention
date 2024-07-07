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
    [SerializeField]Animator animator;
    public override void Init(BaseBattleUnit bBU)
    {
        base.Init(bBU);
        animator = GetComponent<Animator>();
    }

    public void SetAttackValues(BattleMoveAction move)
    {
        attackValues.Clear();
        foreach (AttackAction action in move.resolutionInfo.actions)
        {
            attackValues.Add(action.Value);
        }
    }

    public int AttackAnim()
    {
        return 0;
    }
}
