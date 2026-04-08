using System.Collections.Generic;
using TurnBased;
using TurnBased.TargetSelection;


public interface IElementalMoveAction
{
    public abstract AttackResolutionInfo DoElementalMove(List<UnitData> usersInfo = null, UnitData userInfo = null, List<UnitData> targetsInfo = null, UnitData targetInfo = null);
    public abstract string GetMoveName();
}

#region Fire Moves

/// <summary>
/// This move is when the fire element combines with another fire element. It deals high fire damage to all targets based on all fire user's magic power and level 
/// </summary>
public class EM_Inferno : IElementalMoveAction
{
    public AttackResolutionInfo DoElementalMove(List<UnitData> usersInfo = null, UnitData userInfo = null, List<UnitData> targetsInfo = null, UnitData targetInfo = null)
    {
        int totalAttackValue = 0;
        foreach (var unit in usersInfo)
        {
            if (unit.element == Element.FIRE)
            {
                totalAttackValue += unit.attack;
            }
        }

        // Give a multiplier to the attack to make it better than the sum of it's parts
        totalAttackValue = (int)(totalAttackValue * 1.5f);

        AttackResolutionInfo resolutionInfo = new()
        {
            Steps =
            {
                new AttackStep()
                {
                    Actions =
                    {
                        
                        // Afflicting status: Burned to enemies
                        new AttackAction(AttackActionType.DAMAGE, value: totalAttackValue, elementEff: Element.FIRE, attackTarget: MoveTarget.SingleEnemy)
                    }
                }
            }
        };
        return resolutionInfo;
    }
    public string GetMoveName() => "Inferno";
}

/// <summary>
/// This move is when the fire element combines with a water element. It deals medium water damage and afflicts targets with burning (deals 1 hp per turn)
/// </summary>
public class EM_Steam : IElementalMoveAction
{
    public AttackResolutionInfo DoElementalMove(List<UnitData> usersInfo = null, UnitData userInfo = null, List<UnitData> targetsInfo = null, UnitData targetInfo = null)
    {
        int totalAttackValue = 0;
        foreach (var unit in usersInfo)
        {
            if (unit.element == Element.WATER)
            {
                totalAttackValue += unit.attack;
            }
        }

        // Give a multiplier to the attack to make it slightly better than the sum of it's parts
        totalAttackValue = (int)(totalAttackValue * 1.25f);

        AttackResolutionInfo resolutionInfo = new()
        {
            Steps =
            {
                new AttackStep()
                {
                    Actions =
                    {
                        
                        // Afflicting status: Burned to enemies
                        new AttackAction(AttackActionType.DAMAGE, value: totalAttackValue, elementEff: Element.WATER, staEffect: "Burned", attackTarget: MoveTarget.SingleEnemy)
                    }
                }
            }

        };
        return resolutionInfo;
    }
    public string GetMoveName() => "Steam";
}

/// <summary>
/// This move is when the fire element combines with an ice element. It afflicts the target with two status; frostburn and slippery
/// </summary>
public class EM_Frostburn : IElementalMoveAction
{
    public AttackResolutionInfo DoElementalMove(List<UnitData> usersInfo = null, UnitData userInfo = null, List<UnitData> targetsInfo = null, UnitData targetInfo = null)
    {
        AttackResolutionInfo resolutionInfo = new()
        {
            Steps =
            {
                new AttackStep()
                {
                    Actions =
                    {
                        
                        // Afflicting status: Frostburn to enemies & Slippery
                        new AttackAction(AttackActionType.DAMAGE, elementEff: Element.WATER, staEffect: "Frostburn", attackTarget: MoveTarget.SingleEnemy)
                    }
                }

            }
        };
        return resolutionInfo;
    }
    public string GetMoveName() => "Frostburn";
}

/// <summary>
/// This move is when the fire element combines with an earth element. It deals medium earth damage and inflicts targets with burning status dealing 1 damage per turn 
/// </summary>
public class EM_Volcano : IElementalMoveAction
{
    public AttackResolutionInfo DoElementalMove(List<UnitData> usersInfo = null, UnitData userInfo = null, List<UnitData> targetsInfo = null, UnitData targetInfo = null)
    {
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

        AttackResolutionInfo resolutionInfo = new()
        {
            Steps =
            {
                new AttackStep()
                {
                    Actions =
                    {
                        
                        // Afflicting status: Burned to enemies
                        new AttackAction(AttackActionType.DAMAGE, value: totalAttackValue, elementEff: Element.EARTH, staEffect: "Burned", attackTarget: MoveTarget.SingleEnemy)
                    }
                }

            }
        };

        return resolutionInfo;
    }
    public string GetMoveName() => "Volcano";
}

#endregion

#region Water Moves

/// <summary>
/// This move is when the water element combines with another water element. It deals high water damage to target
/// </summary>
public class EM_Tsunami : IElementalMoveAction
{
    public AttackResolutionInfo DoElementalMove(List<UnitData> usersInfo = null, UnitData userInfo = null, List<UnitData> targetsInfo = null, UnitData targetInfo = null)
    {
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

        AttackResolutionInfo resolutionInfo = new()
        {
            Steps =
            {
                new AttackStep()
                {
                    Actions =
                    {
                      new AttackAction(AttackActionType.DAMAGE, value: totalAttackValue, elementEff: Element.WATER, attackTarget: MoveTarget.SingleEnemy)
                    }
                }

            }
        };

        return resolutionInfo;
    }
    public string GetMoveName() => "Tsunami";
}

/// <summary>
/// This move is when the water element combines with an earth element. Allies are healed for 2 points of HP
/// </summary>
public class EM_Wellspring : IElementalMoveAction
{
    public AttackResolutionInfo DoElementalMove(List<UnitData> usersInfo = null, UnitData userInfo = null, List<UnitData> targetsInfo = null, UnitData targetInfo = null)
    {
        AttackResolutionInfo resolutionInfo = new()
        {
            Steps =
            {
                new AttackStep()
                {
                    Actions =
                    {
                        // All allies are healed for 2 HP
                        new AttackAction(AttackActionType.HEALING, value: 2, attackTarget: MoveTarget.AllAllies)
                    }
                }

            }

        };

        return resolutionInfo;
    }
    public string GetMoveName() => "Wellspring";
}

#endregion

#region Ice Moves

/// <summary>
/// This move is when the ice element combines with another ice element. It deals high ice damage to all targets based on all ice user's magic power and level 
/// </summary>
public class EM_IceAge : IElementalMoveAction
{
    public AttackResolutionInfo DoElementalMove(List<UnitData> usersInfo = null, UnitData userInfo = null, List<UnitData> targetsInfo = null, UnitData targetInfo = null)
    {

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

        AttackResolutionInfo resolutionInfo = new()
        {
            Steps =
            {
                new AttackStep()
                {
                    Actions =
                    {
                        new AttackAction(AttackActionType.DAMAGE, value: totalAttackValue, elementEff: Element.ICE, attackTarget: MoveTarget.SingleEnemy)
                    }
                }

            }
        };
        return resolutionInfo;
    }
    public string GetMoveName() => "Ice Age";
}

/// <summary>
/// This move is when the water element combines with an ice element. Allies are granted the status condition: Hail Cloak. (50% chance to not take damage) guarantees damage every other hit
/// </summary>
public class EM_HailCloak : IElementalMoveAction
{
    public AttackResolutionInfo DoElementalMove(List<UnitData> usersInfo = null, UnitData userInfo = null, List<UnitData> targetsInfo = null, UnitData targetInfo = null)
    {
        AttackResolutionInfo resolutionInfo = new()
        {
            Steps =
            {
                new AttackStep()
                {
                    Actions =
                    {
                        // All allies are granted Hailcloak
                        new AttackAction(AttackActionType.STATUS_EFFECT, staEffect: "Hail Cloak", attackTarget: MoveTarget.AllAllies)
                    }
                }

            }

        };
        return resolutionInfo;
    }
    public string GetMoveName() => "Hail Cloak";
}

#endregion

#region Earth Moves

/// <summary>
/// This move is when the earth element combines with another earth element. It deals high earth damage to all targets based on all earth user's magic power and level 
/// </summary>
public class EM_Fissure : IElementalMoveAction
{
    public AttackResolutionInfo DoElementalMove(List<UnitData> usersInfo = null, UnitData userInfo = null, List<UnitData> targetsInfo = null, UnitData targetInfo = null)
    {

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

        AttackResolutionInfo resolutionInfo = new()
        {
            Steps =
            {
                new AttackStep()
                {
                    Actions =
                    {
                        new AttackAction(AttackActionType.DAMAGE, value: totalAttackValue, elementEff: Element.EARTH, attackTarget: MoveTarget.SingleEnemy)
                    }
                }

            }
        };


        return resolutionInfo;
    }
    public string GetMoveName() => "Fissure";
}

/// <summary>
/// This move is when the Earth element combines with an Ice element. It afflicts targets with the Frost Lock condition. Units cannot move (melee attackers skip their attack, ranged attackers can still attack)
/// </summary>
public class EM_FrostLock : IElementalMoveAction
{
    public AttackResolutionInfo DoElementalMove(List<UnitData> usersInfo = null, UnitData userInfo = null, List<UnitData> targetsInfo = null, UnitData targetInfo = null)
    {
        AttackResolutionInfo resolutionInfo = new()
        {
            Steps =
            {
                new AttackStep()
                {
                    Actions =
                    {
                        // Afflict targets with frost lock
                        new AttackAction(AttackActionType.STATUS_EFFECT, staEffect: "Frost Lock", attackTarget: MoveTarget.AllEnemies)
                    }
                }

            }
        };


        return resolutionInfo;
    }
    public string GetMoveName() => "Frost Lock";
}

#endregion




