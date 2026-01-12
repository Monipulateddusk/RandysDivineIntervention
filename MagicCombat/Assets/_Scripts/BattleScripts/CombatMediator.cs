using System.Collections;
using System.Collections.Generic;
using TurnBased;
using UnityEngine;

/*  Mediator for handling Attack processing Timing. Concrete Mediator owned by CombatSceneManager and Notified by Unity Animation Events or by directly calling the Method. */
public interface ICombatMediator
{
    void Notify(BaseBattleUnit sender, string ev);
}

public class CombatMediator : ICombatMediator
{
    private CombatEnvironmentController environmentController;
    public CombatMediator(CombatEnvironmentController envController)
    {
        environmentController = envController;
    }

    public void Notify(BaseBattleUnit sender, string ev)
    {
        if(ev == "Attack")
        {
            AttackResolutionInfo attackInfo = sender.GetCombatComponent().GetCurrentAttackInformation();
            CombatReturnData returnData = sender.GetCombatComponent().GetCombatReturnData();

            CombatAttackHandler.ProcessAttack(environmentController, attackInfo, returnData);
        }
    }
}
