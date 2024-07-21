using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatComponent : BaseCombatComponent
{
    [SerializeReference] AttackResolutionInfo currentAttackInfo;
    [SerializeField] CombatReturnData combatReturnData;
    Vector3 battleStationLocation;
    public override void Init(BaseBattleUnit bBU)
    {
        base.Init(bBU);
        battleStationLocation = this.transform.position;
    }

    public void StartCombat(AttackResolutionInfo info, CombatReturnData data)
    {
        currentAttackInfo = info;
        combatReturnData = data;
        // Allocate the animation data and then play correlating animation to the move name
        bBU.GetAnimationControllerComponent().SetAttackValues(currentAttackInfo);
        bBU.GetAnimationControllerComponent().GetAnimator().Play(currentAttackInfo.moveName);
    }

    /// <summary>
    /// Called from animationControllerComponent when an attack is animated to process the attack.
    /// </summary>
    public void ProcessAttack(AttackAction attackAction)
    {
        switch (attackAction.Type)
        {
            case AttackAction.ActionType.DAMAGE:
                Debug.LogWarning("Dealing Damage to " + combatReturnData.target + " by: " + attackAction.Value);
                combatReturnData.target.Damage(attackAction.Value);

                break;
            case AttackAction.ActionType.HEALING:
                break;
        }

    }

    /// <summary>
    /// Called just before combat occours so that we move the users of attacks. We need info in the attack's data to determine if we are actually moving or not, but if we are,
    /// we move the user to the target's world space location leaving a gap between.
    /// </summary>
    /// <param name="target"></param>
    IEnumerator MoveUser(Transform target)
    {
        Vector3 startPos = transform.position;
        Vector3 targetPos = target.position;

        float elapsedTime = 0;

        while (elapsedTime < 3.0f) // Constant time right now of 3 but we may make it so that moving to target takes different amounts of time based on conditions
        { 
            transform.position = Vector3.Lerp(startPos, targetPos, (elapsedTime /3));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        // Set the pos of the user to the target to finish the movement
        transform.position = target.position;

        // Invoke an event saying we are finished moving
    }
}
