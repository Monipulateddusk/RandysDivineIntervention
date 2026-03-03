using TurnBased;
using UnityEngine;

namespace TurnBased
{
    public interface ICommand
    {
        void Execute();
    }

    public class ExampleAttackCommand : ICommand
    {

        public void Execute()
        {
            Debug.Log("ATTACKING!!!");

        }

    }

    public class AttackCommand : ICommand
    {
        CombatEnvironmentController combatEnvironmentController;
        AttackResolutionInfo attackResolutionInfo;
        UnitIntention unitIntention;

        public AttackCommand(CombatEnvironmentController cEC, AttackResolutionInfo aRI, UnitIntention UI)
        {
            this.combatEnvironmentController = cEC;
            this.attackResolutionInfo = aRI;
            this.unitIntention = UI;
        }
        public void Execute()
        {
            CombatAttackHandler.ProcessAttackStep(combatEnvironmentController, attackResolutionInfo, unitIntention);
        }
    }
}