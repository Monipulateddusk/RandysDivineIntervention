using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CombatComponent : BaseCombatComponent
{
    // This event handles logic that are called upon the unit ending the attack section of combat.
    public event Action OnEndCombat;

    // This event is called on two occasions; firstly when the unit has moved from their starting location to their target, and when they finish moving back to their starting location
    public event Action OnFinishedMovement;

    [SerializeField] AttackResolutionInfo currentAttackInfo;
    [SerializeField] CombatReturnData combatReturnData;
    Vector3 battleStationLocation;
    public override void Init(BaseBattleUnit bBU)
    {
        base.Init(bBU);
        battleStationLocation = transform.position;
    }

    /// <summary>
    /// Called externally from animationControllerComponent when an attack is animated to process the attack.
    /// </summary>
    public void ProcessAttack(AttackAction attackAction)
    {
        switch (attackAction.Type)
        {
            case AttackAction.ActionType.DAMAGE:
                //Debug.LogWarning("Dealing Damage to " + combatReturnData.target + " by: " + attackAction.Value);
                combatReturnData.target.Damage(attackAction.Value);

                break;
            case AttackAction.ActionType.HEALING:
                combatReturnData.target.Heal(attackAction.Value);
                break;
            case AttackAction.ActionType.IMBUE_ENVIRONMENTS:
                CombatEnvironmentController.Instance.AddEnvironmentalEffect()
                break;
        }

    }
    public void StartCombat(AttackResolutionInfo info, CombatReturnData data)
    {
        currentAttackInfo = info;
        combatReturnData = data;

        // Clear the event and subscribe to it
        OnFinishedMovement = null;
        OnFinishedMovement += Combat;

        MoveUserToTarget(data.target.gameObject.transform.position);
    }

    /// <summary>
    /// Called externally from animationControllerComponent when an animation attack is over. Tells this component to move the unit back to their battle station.
    /// </summary>
    public void EndAttack()
    {
        // Clear the event trigger for movement in preparation for the CombatSceneManager event subscription
        OnFinishedMovement = null;

        OnEndCombatSubscriptionChange();

        MoveUserToTarget(battleStationLocation);
    }



    void Combat()
    {
        // Allocate the animation data and then play correlating animation to the move name
        bBU.GetAnimationControllerComponent().SetAttackValues(currentAttackInfo);
        bBU.GetAnimationControllerComponent().GetAnimator().Play(currentAttackInfo.moveName);

    }

    // Passes on the event subscription allocated to the OnEndCombat and attaches that subscription to the OnFinishedMovement event
    // so that the end turn function is called when the Unit moves back to their battlestation
    void OnEndCombatSubscriptionChange()
    {
        OnFinishedMovement = OnEndCombat;
        OnEndCombat = null;
    }

    void MoveUserToTarget(Vector3 target)
    {
        StartCoroutine(MoveUser(target));
    }

    /// <summary>
    /// Called just before combat occours so that we move the users of attacks. We need info in the attack's data to determine if we are actually moving or not, but if we are,
    /// we move the user to the target's world space location leaving a gap between.
    /// </summary>
    /// <param name="target"></param>
    IEnumerator MoveUser(Vector3 targetPos)
    {
        Vector3 startPos = transform.position;
 

        float elapsedTime = 0;

        while (elapsedTime < 1.0f) // Constant time right now of 3 but we may make it so that moving to target takes different amounts of time based on conditions
        { 
            transform.position = Vector3.Lerp(startPos, targetPos, (elapsedTime /1));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        // Set the pos of the user to the target to finish the movement
        transform.position = targetPos;

        // Invoke an event saying we are finished moving
        OnFinishedMovement?.Invoke();
    }
}
