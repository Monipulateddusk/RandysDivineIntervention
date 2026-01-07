using System;
using System.Collections;
using System.Collections.Generic;
using TurnBased;
using Unity.VisualScripting;
using UnityEngine;

public class CombatComponent : BaseCombatComponent
{
    // This event handles logic that are called upon the unit ending the attack section of combat.
    public event Action OnEndCombat;

    // This event is called on two occasions; firstly when the unit has moved from their starting location to their target, and when they finish moving back to their starting location
    public event Action OnFinishedMovement;

    // Data filled in at the start of combat on the unit's turn, this is where this information will be, no where else
    [SerializeField] AttackResolutionInfo currentAttackInfo;
    [SerializeField] CombatReturnData combatReturnData;
    Vector3 battleStationLocation;
    public override void Init(BaseBattleUnit bBU)
    {
        base.Init(bBU);
        battleStationLocation = transform.position;
    }

    private void OnDestroy()
    {
        currentAttackInfo = null;
        combatReturnData = null;
    }

    /// <summary>
    /// Called externally from animationControllerComponent when an attack is animated to process the attack.
    /// </summary>
    public void ProcessAttack()
    {
        CombatAttackHandler.ProcessAttack(currentAttackInfo, combatReturnData);
    }
    public void StartCombat(AttackResolutionInfo info, CombatReturnData data)
    {
        currentAttackInfo = info;
        combatReturnData = data;

        // Clear the event and subscribe to it
        OnFinishedMovement = null;
        OnEndCombat = null;
        OnFinishedMovement += AnimateCombat;


        // Move the user to the target ( this is where we'd evaluate if the move necessitates movement )
        MoveUserToTarget(data.targets[0].gameObject.transform.position);
    }
    void AnimateCombat()
    {
        // Play correlating animation to the move name
        bBU.GetAnimationControllerComponent().GetAnimator().Play(currentAttackInfo.moveName);
    }


    /// <summary>
    /// Called externally from animationControllerComponent when an animation attack is over. Tells this component to move the unit back to their battle station.
    /// </summary>
    public void EndAttack()
    {
        // Clear the event trigger for movement in preparation for the CombatSceneManager event subscription
        OnFinishedMovement = null;
        OnFinishedMovement = OnEndCombatMovement;

        MoveUserToTarget(battleStationLocation);

    }

    /// <summary>
    /// This function is called by the event OnFinishedMovement; specifically the second movement when the user moves back from the target to their battlestation
    /// Invokes the OnEndCombat event (which ends the turn)
    /// </summary>
    void OnEndCombatMovement()
    {
        OnEndCombat?.Invoke();
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

    /// <summary>
    /// Allows other components to access the resolution data. This is so other classes can work along side the combat compoent to read the moves data such as the target or the move actions
    /// </summary>
    /// <returns></returns>
    public CombatReturnData GetCombatReturnData() { return combatReturnData; }
}
