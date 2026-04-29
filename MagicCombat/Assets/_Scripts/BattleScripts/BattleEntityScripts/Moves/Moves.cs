using System.Collections.Generic;

namespace TurnBased
{
    #region Attack Data Classes


    public class AttackAction
    {
        public AttackActionType Type { get; private set; }
        public Element ElementEffect { get; private set; }
        public MoveTarget AttackTarget { get; private set; }
        public int Value { get; private set; }
        public string StaEffect { get; private set; }


        public AttackAction(AttackActionType type, int value = 0, string staEffect = "", Element elementEff = 0, MoveTarget attackTarget = MoveTarget.SingleEnemy)
        {
            Type = type;
            ElementEffect = elementEff;
            Value = value;
            StaEffect = staEffect;
            AttackTarget = attackTarget;
        }
    }

    



    public class AttackStep
    {
        public List<AttackAction> Actions = new();

        public AttackStep()
        {
            Actions = new();
        }

        public AttackStep(List<AttackAction> actions)
        {
            this.Actions = actions; 
        }
    }


    public class AttackResolutionInfo
    {
        public List<AttackStep> Steps { get; private set; }

        public AttackResolutionInfo()
        {
            Steps = new List<AttackStep>();
        }
    }
    #endregion

    public interface IBattleMove
    {
        public abstract AttackResolutionInfo ExecuteMove(UnitData userInfo = null, List<UnitData> usersInfo = null, List<UnitData> targetsInfo = null);
        public MoveTarget GetMoveTargetType();
        public int GetMaxTargets();
        public bool DoesSourceUnitMove();
        public string GetMoveName();
    }

    /// <summary>
    /// An attack that does the user's full damage stat to the target
    /// </summary>
    public class HeavyAttack : IBattleMove
    {
        public AttackResolutionInfo ExecuteMove(UnitData userInfo = null, List<UnitData> usersInfo = null, List<UnitData> targetsInfo = null)
        {
            AttackResolutionInfo resolutionInfo = new()
            {
                Steps =
                {
                    new AttackStep()
                    {
                        Actions =
                        {
                            new AttackAction(AttackActionType.DAMAGE, userInfo.attack, attackTarget: MoveTarget.SingleEnemy)
                        }                        
                    }, 
                }
            };
            return resolutionInfo;
        }

        public int GetMaxTargets() => 1;
        public MoveTarget GetMoveTargetType() => MoveTarget.SingleEnemy;
        public bool DoesSourceUnitMove() => true;

        public string GetMoveName() => "HeavyAttack";
    }

    /// <summary>
    /// An attack that does 1 damage 2 times and then 1/3 of damage stat of user
    /// </summary>
    public class LightAttack : IBattleMove
    {
        public AttackResolutionInfo ExecuteMove(UnitData userInfo = null, List<UnitData> usersInfo = null, List<UnitData> targetsInfo = null)
        {
            int damage = userInfo.attack / 3;
            AttackResolutionInfo resolutionInfo = new()
            { 
                Steps =
                {
                    new AttackStep()
                    {
                        Actions =
                        {
                            new AttackAction(AttackActionType.DAMAGE, 1, attackTarget: MoveTarget.SingleEnemy)
                        }
                    },
                    new AttackStep()
                    {
                        Actions =
                        {
                            new AttackAction(AttackActionType.DAMAGE, 1, attackTarget: MoveTarget.SingleEnemy)
                        }
                    },
                    new AttackStep()
                    {
                        Actions =
                        {

                            new AttackAction(AttackActionType.DAMAGE, damage, attackTarget: MoveTarget.SingleEnemy)
                        }
                    },
                }

            };
            return resolutionInfo;
        }

        public int GetMaxTargets() => 1;
        public MoveTarget GetMoveTargetType() => MoveTarget.SingleEnemy;
        public bool DoesSourceUnitMove() => true;

        public string GetMoveName() => "LightAttack";
    }

    /// <summary>
    /// An attack that imbues the environment with the user's element. Used for the joint attacks proc-ing
    /// </summary>
    public class ImbueEnvrionment : IBattleMove
    {
        public AttackResolutionInfo ExecuteMove(UnitData userInfo = null, List<UnitData> usersInfo = null, List<UnitData> targetsInfo = null)
        {
            AttackResolutionInfo resolutionInfo = new()
            {
                Steps =
                {
                    new AttackStep()
                    {
                        Actions =
                        {
                            new AttackAction(AttackActionType.IMBUE_ENVIRONMENTS, elementEff: userInfo.element, attackTarget: MoveTarget.Area),
                        },
                    }
                }

            };
            return resolutionInfo;
        }

        public int GetMaxTargets() => 0;
        public MoveTarget GetMoveTargetType() => MoveTarget.Area;
        public bool DoesSourceUnitMove() => false;
        public string GetMoveName() => "ImbueEnvironment";
    }
}
