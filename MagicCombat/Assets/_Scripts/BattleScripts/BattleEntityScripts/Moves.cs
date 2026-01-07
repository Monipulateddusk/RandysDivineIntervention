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
        public abstract AttackResolutionInfo DoMove(List<BaseUnit> usersInfo = null, BaseUnit userInfo = null, List<BaseUnit> targetsInfo = null, BaseUnit targetInfo = null);
    }

    /// <summary>
    /// An attack that does the user's full damage stat to the target
    /// </summary>
    [System.Serializable]
    public class HeavyAttack : IBattleMoveAction
    {
        public AttackResolutionInfo DoMove(List<BaseUnit> usersInfo = null, BaseUnit userInfo = null, List<BaseUnit> targetsInfo = null, BaseUnit targetInfo = null)
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
        public AttackResolutionInfo DoMove(List<BaseUnit> usersInfo = null, BaseUnit userInfo = null, List<BaseUnit> targetsInfo = null, BaseUnit targetInfo = null)
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
        public AttackResolutionInfo DoMove(List<BaseUnit> usersInfo = null, BaseUnit userInfo = null, List<BaseUnit> targetsInfo = null, BaseUnit targetInfo = null)
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

/// <summary>
/// This move is when the fire element combines with an ice element. It afflicts the target with two status; frostburn and slippery
/// </summary>
public class EM_Frostburn : IBattleMoveAction
{
    public AttackResolutionInfo DoMove(List<BaseUnit> usersInfo = null, BaseUnit userInfo = null, List<BaseUnit> targetsInfo = null, BaseUnit targetInfo = null)
    {
        AttackResolutionInfo resolutionInfo = new()
        {
            moveName = "Frostburn"
        };

        // Afflicting status: Frostburn to enemies & Slippery
        resolutionInfo.Actions.Add(new AttackAction(AttackAction.ActionType.DAMAGE, elementEff: Element.WATER, staEffect: "Frostburn"));
        return resolutionInfo;
    }
}
/// <summary>
/// This move is when the earth element combines with another earth element. It deals high earth damage to all targets based on all earth user's magic power and level 
/// </summary>
public class EM_Fissure : IBattleMoveAction
{
    public AttackResolutionInfo DoMove(List<BaseUnit> usersInfo = null, BaseUnit userInfo = null, List<BaseUnit> targetsInfo = null, BaseUnit targetInfo = null)
    {
        AttackResolutionInfo resolutionInfo = new()
        {
            moveName = "Fissure"
        };

        int totalAttackValue = 0;
        foreach (var unit in usersInfo)
        {
            if (unit.element == Element.EARTH)
            {
                totalAttackValue = +unit.attack;
            }
        }

        // Give a multiplier to the attack to make it better than the sum of it's parts
        totalAttackValue = (int)(totalAttackValue * 1.5f);

        resolutionInfo.Actions.Add(new AttackAction(AttackAction.ActionType.DAMAGE, value: totalAttackValue, elementEff: Element.EARTH));
        return resolutionInfo;
    }
}

/// <summary>
/// This move is when the ice element combines with another ice element. It deals high ice damage to all targets based on all ice user's magic power and level 
/// </summary>
public class EM_IceAge : IBattleMoveAction
{
    public AttackResolutionInfo DoMove(List<BaseUnit> usersInfo = null, BaseUnit userInfo = null, List<BaseUnit> targetsInfo = null, BaseUnit targetInfo = null)
    {
        AttackResolutionInfo resolutionInfo = new()
        {
            moveName = "Ice Age"
        };

        int totalAttackValue = 0;
        foreach (var unit in usersInfo)
        {
            if (unit.element == Element.ICE)
            {
                totalAttackValue = +unit.attack;
            }
        }

        // Give a multiplier to the attack to make it better than the sum of it's parts
        totalAttackValue = (int)(totalAttackValue * 1.5f);

        resolutionInfo.Actions.Add(new AttackAction(AttackAction.ActionType.DAMAGE, value: totalAttackValue, elementEff: Element.ICE));
        return resolutionInfo;
    }
}


/// <summary>
/// This move is when the fire element combines with a water element. It deals medium water damage and afflicts targets with burning (deals 1 hp per turn)
/// </summary>
public class EM_Steam : IBattleMoveAction
{
    public AttackResolutionInfo DoMove(List<BaseUnit> usersInfo = null, BaseUnit userInfo = null, List<BaseUnit> targetsInfo = null, BaseUnit targetInfo = null)
    {
        AttackResolutionInfo resolutionInfo = new()
        {
            moveName = "Steam"
        };

        int totalAttackValue = 0;
        foreach (var unit in usersInfo)
        {
            if (unit.element == Element.WATER)
            {
                totalAttackValue = +unit.attack;
            }
        }

        // Give a multiplier to the attack to make it slightly better than the sum of it's parts
        totalAttackValue = (int)(totalAttackValue * 1.25f);

        // Afflicting status: Burned to enemies
        resolutionInfo.Actions.Add(new AttackAction(AttackAction.ActionType.DAMAGE, value: totalAttackValue, elementEff: Element.WATER, staEffect: "Burned"));
        return resolutionInfo;
    }
}


/// <summary>
/// This move is when the Earth element combines with an Ice element. It afflicts targets with the Frost Lock condition. Units cannot move (melee attackers skip their attack, ranged attackers can still attack)
/// </summary>
public class EM_FrostLock : IBattleMoveAction
{
    public AttackResolutionInfo DoMove(List<BaseUnit> usersInfo = null, BaseUnit userInfo = null, List<BaseUnit> targetsInfo = null, BaseUnit targetInfo = null)
    {
        AttackResolutionInfo resolutionInfo = new()
        {
            moveName = "Frost Lock"
        };

        // Afflict targets with frost lock

        resolutionInfo.Actions.Add(new AttackAction(AttackAction.ActionType.STATUS_EFFECT, staEffect: "Frost Lock"));
        return resolutionInfo;
    }
}


/// <summary>
/// This move is when the water element combines with another water element. It deals high water damage to target
/// </summary>
public class EM_Tsunami : IBattleMoveAction
{
    public AttackResolutionInfo DoMove(List<BaseUnit> usersInfo = null, BaseUnit userInfo = null, List<BaseUnit> targetsInfo = null, BaseUnit targetInfo = null)
    {
        AttackResolutionInfo resolutionInfo = new()
        {
            moveName = "Tsunami"
        };

        int totalAttackValue = 0;
        foreach (var unit in usersInfo)
        {
            if (unit.element == Element.WATER)
            {
                totalAttackValue = +unit.attack;
            }
        }

        // Give a multiplier to the attack to make it better than the sum of it's parts
        totalAttackValue = (int)(totalAttackValue * 1.5f);

        resolutionInfo.Actions.Add(new AttackAction(AttackAction.ActionType.DAMAGE, value: totalAttackValue, elementEff: Element.WATER));
        return resolutionInfo;
    }
}


/// <summary>
/// This move is when the water element combines with an earth element. Allies are healed for 2 points of HP
/// </summary>
public class EM_Wellspring : IBattleMoveAction
{
    public AttackResolutionInfo DoMove(List<BaseUnit> usersInfo = null, BaseUnit userInfo = null, List<BaseUnit> targetsInfo = null, BaseUnit targetInfo = null)
    {
        AttackResolutionInfo resolutionInfo = new()
        {
            moveName = "Wellspring"
        };

        // All allies are healed for 2 HP

        resolutionInfo.Actions.Add(new AttackAction(AttackAction.ActionType.HEALING, value: 2));
        return resolutionInfo;
    }
}

/// <summary>
/// This move is when the water element combines with an ice element. Allies are granted the status condition: Hail Cloak. (50% chance to not take damage) guarantees damage every other hit
/// </summary>
public class EM_HailCloak : IBattleMoveAction
{
    public AttackResolutionInfo DoMove(List<BaseUnit> usersInfo = null, BaseUnit userInfo = null, List<BaseUnit> targetsInfo = null, BaseUnit targetInfo = null)
    {
        AttackResolutionInfo resolutionInfo = new()
        {
            moveName = "Hail Cloak"
        };

        // All allies are granted Hailcloak

        resolutionInfo.Actions.Add(new AttackAction(AttackAction.ActionType.STATUS_EFFECT, staEffect: "Hail Cloak"));
        return resolutionInfo;
    }
}


/// <summary>
/// This move is when the fire element combines with an earth element. It deals medium earth damage and inflicts targets with burning status dealing 1 damage per turn 
/// </summary>
public class EM_Volcano : IBattleMoveAction
{
    public AttackResolutionInfo DoMove(List<BaseUnit> usersInfo = null, BaseUnit userInfo = null, List<BaseUnit> targetsInfo = null, BaseUnit targetInfo = null)
    {
        AttackResolutionInfo resolutionInfo = new()
        {
            moveName = "Volcano"
        };

        int totalAttackValue = 0;
        foreach (var unit in usersInfo)
        {
            if (unit.element == Element.EARTH)
            {
                totalAttackValue = +unit.attack;
            }
        }

        // Give a multiplier to the attack to make it slightly better than the sum of it's parts
        totalAttackValue = (int)(totalAttackValue * 1.25f);

        // Afflicting status: Burned to enemies
        resolutionInfo.Actions.Add(new AttackAction(AttackAction.ActionType.DAMAGE, value: totalAttackValue, elementEff: Element.EARTH, staEffect: "Burned"));
        return resolutionInfo;
    }
}

/// <summary>
/// An attack that imbues the environment with the user's element. Used for the joint attacks proc-ing
/// </summary>
public class ImbueEnvrionment : IBattleMoveAction
{
    public AttackResolutionInfo DoMove(List<BaseUnit> usersInfo = null, BaseUnit userInfo = null, List<BaseUnit> targetsInfo = null, BaseUnit targetInfo = null)
    {
        AttackResolutionInfo resolutionInfo = new()
        {
            moveName = "ImbueEnvironment"
        };
        resolutionInfo.Actions.Add(new AttackAction(AttackAction.ActionType.IMBUE_ENVIRONMENTS, elementEff: userInfo.element));
        return resolutionInfo;
    }
}

/// <summary>
/// This move is when the fire element combines with another fire element. It deals high fire damage to all targets based on all fire user's magic power and level 
/// </summary>
public class EM_Inferno : IBattleMoveAction
{
    public AttackResolutionInfo DoMove(List<BaseUnit> usersInfo = null, BaseUnit userInfo = null, List<BaseUnit> targetsInfo = null, BaseUnit targetInfo = null)
    {
        AttackResolutionInfo resolutionInfo = new() 
        {
            moveName = "Inferno"
        };

        int totalAttackValue = 0;
        foreach (var unit in usersInfo)
        {
            if (unit.element == Element.FIRE)
            {
                totalAttackValue = +unit.attack;
            }
        }

        // Give a multiplier to the attack to make it better than the sum of it's parts
        totalAttackValue = (int)(totalAttackValue * 1.5f);

        resolutionInfo.Actions.Add(new AttackAction(AttackAction.ActionType.DAMAGE, value: totalAttackValue, elementEff: Element.FIRE));
        return resolutionInfo;
    }
}
