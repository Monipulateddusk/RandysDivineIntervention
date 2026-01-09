using System.Collections;
using System.Collections.Generic;
using TurnBased;
using UnityEngine;

namespace TurnBased
{
    #region Attack Data Classes
    public enum StatusEffect
    {
        NULL,
    }
    [System.Serializable]
    public class AttackAction
    {
        public enum ActionType { DAMAGE, HEALING, STATUS_EFFECT, IMBUE_ENVIRONMENTS }

        public ActionType Type { get; private set; }
        public int Value { get; private set; }
        public string StaEffect { get; private set; }
        public Element ElementEffect { get; private set; }

        public AttackAction(ActionType type, int value = 0, string staEffect = "", Element elementEff = 0)
        {
            Type = type;
            Value = value;
            StaEffect = staEffect;
            ElementEffect = elementEff;
        }
    }

    [System.Serializable]
    public class AttackResolutionInfo
    {
        public string moveName;
        public List<AttackAction> Actions { get; private set; }

        public AttackResolutionInfo()
        {
            Actions = new List<AttackAction>();
        }
        public void RemoveAtIndex(int index) { Actions.RemoveAt(index); }
    }
    #endregion

    public interface IBattleMoveAction
    {
        public abstract AttackResolutionInfo DoMove(List<UnitData> usersInfo = null, UnitData userInfo = null, List<UnitData> targetsInfo = null, UnitData targetInfo = null);
    }

    /// <summary>
    /// An attack that does the user's full damage stat to the target
    /// </summary>
    [System.Serializable]
    public class HeavyAttack : IBattleMoveAction
    {
        public AttackResolutionInfo DoMove(List<UnitData> usersInfo = null, UnitData userInfo = null, List<UnitData> targetsInfo = null, UnitData targetInfo = null)
        {
            AttackResolutionInfo resolutionInfo = new()
            {
                moveName = "HeavyAttack"
            };
            resolutionInfo.Actions.Add(new AttackAction(AttackAction.ActionType.DAMAGE, userInfo.attack));
            return resolutionInfo;
        }
    }

    /// <summary>
    /// An attack that does 1 damage 2 times and then 1/3 of damage stat of user
    /// </summary>
    public class LightAttack : IBattleMoveAction
    {
        public AttackResolutionInfo DoMove(List<UnitData> usersInfo = null, UnitData userInfo = null, List<UnitData> targetsInfo = null, UnitData targetInfo = null)
        {
            AttackResolutionInfo resolutionInfo = new()
            {
                moveName = "LightAttack"
            };
            int damage = userInfo.attack / 3;
            resolutionInfo.Actions.Add(new AttackAction(AttackAction.ActionType.DAMAGE, 1));
            resolutionInfo.Actions.Add(new AttackAction(AttackAction.ActionType.DAMAGE, 1));
            resolutionInfo.Actions.Add(new AttackAction(AttackAction.ActionType.DAMAGE, damage));

            return resolutionInfo;
        }
    }

    /// <summary>
    /// An attack that imbues the environment with the user's element. Used for the joint attacks proc-ing
    /// </summary>
    public class ImbueEnvrionment : IBattleMoveAction
    {
        public AttackResolutionInfo DoMove(List<UnitData> usersInfo = null, UnitData userInfo = null, List<UnitData> targetsInfo = null, UnitData targetInfo = null)
        {
            AttackResolutionInfo resolutionInfo = new()
            {
                moveName = "ImbueEnvironment",


            };
            resolutionInfo.Actions.Add(new AttackAction(AttackAction.ActionType.IMBUE_ENVIRONMENTS, elementEff: userInfo.element));
            return resolutionInfo;
        }
    }
}
