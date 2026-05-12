namespace TurnBased
{
    #region Attack Data Classes

    public class AttackStep
    {
        public System.Collections.Generic.List<AttackAction> Actions = new();

        public AttackStep()
        {
            Actions = new();
        }

        public AttackStep(System.Collections.Generic.List<AttackAction> actions)
        {
            this.Actions = actions; 
        }
    }
    public class TargetDeclarationGroup
    {
        public int TargetGroupID { get; }
        public MoveTarget GroupMoveTargetType { get; }

        public TargetDeclarationGroup(int groupID, MoveTarget targetType)
        {
            this.TargetGroupID = groupID;
            this.GroupMoveTargetType = targetType;
        }
    }

    public class AttackResolutionInfo
    {
        public System.Collections.Generic.List<TargetDeclarationGroup> TargetDeclarationGroups { get; }
        public System.Collections.Generic.List<AttackStep> Steps { get; private set; }

        public AttackResolutionInfo()
        {
            this.Steps = new();
            this.TargetDeclarationGroups = new();
        }
    }



    #endregion

    public interface IBattleMove
    {
        public abstract AttackResolutionInfo ExecuteMove(UnitData userInfo = null, System.Collections.Generic.List<UnitData> usersInfo = null, System.Collections.Generic.List<UnitData> targetsInfo = null);
        public MoveResolutionTiming GetResolutionTiming();
        public int GetMaxTargets();
        public bool DoesSourceUnitMove();
        public string GetMoveName();
    }

    /// <summary>
    /// An attack that does the user's full damage stat to the target
    /// </summary>
    public class HeavyAttack : IBattleMove
    {
        public AttackResolutionInfo ExecuteMove(UnitData userInfo = null, System.Collections.Generic.List<UnitData> usersInfo = null, System.Collections.Generic.List<UnitData> targetsInfo = null)
        {
            AttackResolutionInfo resolutionInfo = new()
            {
                TargetDeclarationGroups = { new TargetDeclarationGroup(groupID: 0, MoveTarget.SingleEnemy) },

                Steps =
                {
                    new AttackStep()
                    {
                        Actions =
                        {
                            new DamageAttackAction(userInfo.attack, 1, groupID: 0),
                        }
                    },
                }
            };
            return resolutionInfo;
        }

        public int GetMaxTargets() => 1;
        public bool DoesSourceUnitMove() => true;

        public string GetMoveName() => "HeavyAttack";

        public MoveResolutionTiming GetResolutionTiming() => MoveResolutionTiming.TurnOrderSequence;
    }

    /// <summary>
    /// An attack that does 1 damage 2 times and then 1/3 of damage stat of user
    /// </summary>
    public class LightAttack : IBattleMove
    {
        public AttackResolutionInfo ExecuteMove(UnitData userInfo = null, System.Collections.Generic.List<UnitData> usersInfo = null, System.Collections.Generic.List<UnitData> targetsInfo = null)
        {
            int damage = userInfo.attack / 3;
            AttackResolutionInfo resolutionInfo = new()
            {
                TargetDeclarationGroups = { new TargetDeclarationGroup(groupID: 0, MoveTarget.SingleEnemy) },

                Steps =
                {
                    new AttackStep()
                    {
                        Actions =
                        {
                            new DamageAttackAction(damageAmount: 1, 2, groupID: 0)
                        }
                    },
                    new AttackStep()
                    {
                        Actions =
                        {
                            new DamageAttackAction(damageAmount: damage, 1, groupID: 0)
                        }
                    },
                }

            };
            return resolutionInfo;
        }

        public int GetMaxTargets() => 1;
        public bool DoesSourceUnitMove() => true;

        public string GetMoveName() => "LightAttack";
        public MoveResolutionTiming GetResolutionTiming() => MoveResolutionTiming.TurnOrderSequence;
    }

    /// <summary>
    /// An attack that imbues the environment with the user's element. Used for the joint attacks proc-ing
    /// </summary>
    public class ImbueEnvrionment : IBattleMove
    {
        public AttackResolutionInfo ExecuteMove(UnitData userInfo = null, System.Collections.Generic.List<UnitData> usersInfo = null, System.Collections.Generic.List<UnitData> targetsInfo = null)
        {
            AttackResolutionInfo resolutionInfo = new()
            {
                TargetDeclarationGroups = { new TargetDeclarationGroup(groupID: 0, MoveTarget.Area) },

                Steps =
                {
                    new AttackStep()
                    {
                        Actions =
                        {
                            new ImbueEnvironmentAttackAction(userInfo.element, groupID: 0)
                        },
                    }
                }

            };
            return resolutionInfo;
        }

        public int GetMaxTargets() => 0;
        public bool DoesSourceUnitMove() => false;
        public string GetMoveName() => "ImbueEnvironment";
        public MoveResolutionTiming GetResolutionTiming() => MoveResolutionTiming.TurnOrderSequence;
    }
    
}