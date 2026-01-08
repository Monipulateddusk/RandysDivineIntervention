using TurnBased;
using System;
using System.Threading.Tasks;
using UnityEngine;

public class CombatComponent : BaseComponent
{

    Transform unitTransform;

    /*  Starting Position of the Unit   */
    Vector3 battleStationLocation;

    /*  Event: Invoked when Attacking is done   */
    public event Action OnEndAttackingCombat;

    /*  Current Attack Information */
    AttackResolutionInfo currentAttackInfo;
    CombatReturnData combatReturnData;


    public CombatComponent()
    {
        battleStationLocation = Vector3.zero;
        battleUnit = null;
        unitTransform = null;
    }

    public CombatComponent(BaseBattleUnit battleUnit, UnitData unitData, Transform unitTransf)
    {
        this.battleUnit = battleUnit;
        this.unitData = unitData;
        unitTransform = unitTransf;
        battleStationLocation = unitTransform.position;
    }

    ~CombatComponent()
    {
        currentAttackInfo = null;
        combatReturnData = null;
        unitTransform = null;
    }

    /// <summary>
    /// Called externally from animationControllerComponent when an attack is animated to process the attack.
    /// </summary>
    public void ProcessAttack()
    {
        CombatAttackHandler.ProcessAttack(currentAttackInfo, combatReturnData);
    }
    public async Task StartCombat(AttackResolutionInfo info, CombatReturnData data)
    {
        currentAttackInfo = info;
        combatReturnData = data;

        // Clear the event at the start of the Turn
        OnEndAttackingCombat = null;

        // Move the user to the target ( this is where we'd evaluate if the move necessitates movement )
        await MoveUserToTarget(data.targets[0].gameObject.transform.position);
        battleUnit.PlayCombatAttackAnimation();
    }

    /// <summary>
    /// ANIMATION EVENT: Called when an Attack's Animation is over. Move the unit back to the battle station.
    /// </summary>
    public async Task OnEndAttackAnimation()
    {
        await MoveUserToTarget(battleStationLocation);
        OnEndAttackingCombat?.Invoke();

    }

    async Task MoveUserToTarget(Vector3 target)
    {
        await MoveUnitToTarget(target);
    }

    private async Task MoveUnitToTarget(Vector3 targetPos)
    {
        Vector3 startPos = unitTransform.position;
        float elapsedTime = 0;

        while (elapsedTime < 1.0f) 
        {
            if(unitTransform == null) { break; }

            unitTransform.position = Vector3.Lerp(startPos, targetPos, (elapsedTime / 1));
            elapsedTime += Time.deltaTime;
            await Task.Yield();
        }

        // Snap the Unit's position to the Target
        if (unitTransform != null) { unitTransform.position = targetPos; }    
    }

    public AttackResolutionInfo GetCurrentAttackInformation() {  return currentAttackInfo; }
    public CombatReturnData GetCombatReturnData() { return combatReturnData; }
}
